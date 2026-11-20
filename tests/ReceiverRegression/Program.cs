using System;
using System.Reflection;
using System.Diagnostics;
class Program {
 static int checks;
 static void Check(bool v,string s) { if(!v)throw new Exception(s); checks++; }
 static long Ticks(double t)=>(long)(t*Stopwatch.Frequency);
 static byte[] Packet(byte id,bool status,ushort seq,uint ms) {
  var b=new byte[status?20:16]; b[0]=2;b[1]=id;b[2]=3;b[3]=0x80;
  BitConverter.GetBytes(seq).CopyTo(b,4); BitConverter.GetBytes(ms).CopyTo(b,6);
  if(!status)BitConverter.GetBytes((short)400).CopyTo(b,12);
  return b;
 }
 static object Wrap(byte[] bytes,double t,bool status=false) {
  var type=typeof(BiofitBLEMonitor).GetNestedType("Packet",BindingFlags.NonPublic);
  var p=Activator.CreateInstance(type);
  type.GetField("expectedId").SetValue(p,bytes[1]);type.GetField("bytes").SetValue(p,bytes);
  type.GetField("callbackTicks").SetValue(p,Ticks(t));type.GetField("isStatus").SetValue(p,status);return p;
 }
 static void Read(BiofitBLEMonitor m,byte[] b,double t,bool status=false) {
  typeof(BiofitBLEMonitor).GetMethod(status?"ReadStatus":"ReadEvent",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(m,new[]{Wrap(b,t,status)});
 }
 static void CycleIntegration() {
  var m=new BiofitBLEMonitor();int pairs=0;m.BilateralJumpCandidate+=g=>pairs++;
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;
  foreach(string side in new[]{"left","right"}){
   var tracker=typeof(BiofitBLEMonitor).GetField(side,flags).GetValue(m);
   tracker.GetType().GetField("connection").SetValue(tracker,"CONECTADO - NOTIFICACIONES ACTIVAS");
  }
  double now=Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
  for(int i=0;i<12;i++)for(byte id=1;id<=2;id++)Read(m,Packet(id,true,(ushort)i,(uint)(100000+100*i)),now-1.1+.1*i,true);
  m.LiftCycleMode=true;m.StartGameCapture();
  var transport=typeof(BiofitBLEMonitor).GetMethod("ReadTransport",flags);
  Action<byte,int> firmware=(id,version)=>{var b=new byte[20];b[0]=2;b[1]=id;b[2]=3;BitConverter.GetBytes((ushort)version).CopyTo(b,18);transport.Invoke(m,new[]{Wrap(b,now)});};
  Action<byte,ushort,uint,bool> onset=(id,seq,ms,marker)=>{var b=Packet(id,false,seq,ms);b[2]=4;b[3]=(byte)(marker?0xa0:0x80);Read(m,b,now);};
  firmware(1,229);firmware(2,228);
  onset(1,1,101100,true);onset(2,1,101110,true);
  Check(!m.CycleFirmwareReady&&pairs==0,"mixed firmware fails closed");
  firmware(2,229);Check(m.CycleFirmwareReady,"both real version pages enable experimental mode");
  Read(m,Packet(1,false,2,101100),now);Read(m,Packet(2,false,2,101110),now);
  Check(pairs==0,"legacy UP cannot operate experimental mode");
  onset(1,3,101100,false);onset(2,3,101110,false);
  Check(pairs==0,"new type without algorithm marker cannot operate");
  onset(1,4,101100,true);onset(2,4,101110,true);
  Check(pairs==1&&m.CyclePairCount==1,"real EVENT parser -> freshness/sequence -> cycle pair");
  onset(1,4,101100,true);onset(2,4,101110,true);
  Check(pairs==1&&m.RejectedEvents==2,"duplicate onset notifications rejected");
  onset(1,5,100000,true);onset(2,5,100010,true);
  Check(pairs==1&&m.RejectedEvents==4,"late onset never treated as a new lift");
  int epoch=m.ControlEpoch;m.LiftCycleMode=false;
  Check(m.ControlEpoch==epoch+1,"changing algorithm invalidates pending control state");
  onset(1,6,101100,true);onset(2,6,101110,true);
  Check(pairs==1,"new events ignored for legacy actions but their sequences consumed");
  Read(m,Packet(1,false,7,101100),now);Read(m,Packet(2,false,7,101110),now);
  Check(pairs==2,"legacy path still accepts original UP after new channel traffic");
  var diag=new byte[20];diag[0]=3;diag[1]=1;diag[2]=3;diag[3]=4;
  BitConverter.GetBytes((short)-500).CopyTo(diag,8);transport.Invoke(m,new[]{Wrap(diag,now)});
  m.RecordGameAction("BLE_CICLO_EXPERIMENTAL");m.RecordGameAction("SALTO_BLE_BUFFERED");
  m.StopGameCapture();m.CaptureCompletion.GetAwaiter().GetResult();
  string csv=System.IO.File.ReadAllText(m.SavedCapturePath);
  Check(csv.Contains("LIFT_ONSET")&&csv.Contains("LIFT_PAIR=Paired")&&csv.Contains("velocityMmS=-500"),"new onset/decision/signed state retained in verified archive");
  Check(csv.Contains("BLE_CICLO_EXPERIMENTAL")&&csv.Contains("SALTO_BLE_BUFFERED"),"mode and buffer APP events are not silently dropped");
  Check(BiofitCaptureArchive.Verify(csv),"new event type preserves archive contract");
 }
 static void Main() {
  var m=new BiofitBLEMonitor(); int solo=0,pairs=0;
  m.AnkleUpCandidate+=(a,b,c)=>solo++;m.BilateralJumpCandidate+=g=>pairs++;
  Check(m.StartGameCapture(),"capture starts");
  double now=Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
  for(int i=0;i<12;i++) { Read(m,Packet(1,true,(ushort)i,(uint)(100000+100*i)),now-1.1+i*.1,true);Read(m,Packet(2,true,(ushort)i,(uint)(500000+100*i)),now-1.1+i*.1,true); }
  Read(m,Packet(1,false,1,99600),now);Read(m,Packet(2,false,1,499600),now);
  Check(solo==0&&pairs==0&&m.CapturedEventCount==2,"stale batch logged, no gameplay");
  // All prior STATUS observations used a simulated callback clock "now".
  // Reflection JIT and CI host load can spend >250 ms before the first EVENT;
  // that would fail the *real* stale-queue gate for a reason unrelated to the
  // event algorithm. Re-anchor at the CURRENT real callback time, advancing
  // both simulated sensor clocks by exactly the same elapsed time.
  double freshNow=Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
  uint elapsedMs=(uint)Math.Max(0d,Math.Round((freshNow-now)*1000d));
  uint freshLeft=101100u+elapsedMs, freshRight=501111u+elapsedMs;
  Read(m,Packet(1,true,12,freshLeft),freshNow,true);
  Read(m,Packet(2,true,12,501100u+elapsedMs),freshNow,true);
  Read(m,Packet(1,false,2,freshLeft),freshNow);
  Read(m,Packet(2,false,2,freshRight),freshNow);
  Check(solo==2&&pairs==1,"fresh reverse-clock pair acts");
  Read(m,Packet(1,false,2,freshLeft),freshNow);
  Check(solo==2&&pairs==1&&m.RejectedEvents==3,"duplicate cannot act");
  // A callback held >250ms inside Unity cannot act even with a recent sensor time.
  Read(m,Packet(1,false,3,100100),now-1);
  Check(solo==2&&m.RejectedEvents==4,"Unity stalled packet rejected");
  Check(m.StopGameCapture(),"stop requested"); m.CaptureCompletion.GetAwaiter().GetResult();
  Check(m.CopyGameCapture(),"export finishes");
  Check(UnityEngine.GUIUtility.systemCopyBuffer.Contains("RECHAZO_ANTIGUO")&&UnityEngine.GUIUtility.systemCopyBuffer.Contains("RECHAZO_SECUENCIA"),"CSV keeps rejection reasons");
  var cold=new BiofitBLEMonitor();cold.AnkleUpCandidate+=(a,b,c)=>solo++;
  Read(cold,Packet(1,false,1,1),now);
  Check(solo==2&&cold.RejectedEvents==1,"no STATUS cannot jump");
  // Actual bounded callback queue + disconnect epoch invalidation in Update().
  var q=new BiofitBLEMonitor(); int qActions=0; q.AnkleUpCandidate+=(a,b,c)=>qActions++;
  var enqueue=typeof(BiofitBLEMonitor).GetMethod("Enqueue",BindingFlags.Instance|BindingFlags.NonPublic);
  var update=typeof(BiofitBLEMonitor).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
  for(int i=0;i<1100;i++) enqueue.Invoke(q,new[]{Wrap(Packet(1,false,(ushort)i,100),now)});
  Check((int)typeof(BiofitBLEMonitor).GetField("pendingCount",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(q)==1024,"queue bounded");
  Check((int)typeof(BiofitBLEMonitor).GetField("callbackDropped",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(q)==76,"queue overflow counted");
  var reset=Wrap(Packet(1,false,1,0),now);reset.GetType().GetField("reset").SetValue(reset,true);
  enqueue.Invoke(q,new[]{reset});
  for(int i=0;i<12;i++)update.Invoke(q,null);
  Check(qActions==0,"unsynchronized queue cannot jump");
  Check((int)typeof(BiofitBLEMonitor).GetField("pendingCount",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(q)==0,"queue drained including disconnect");
  Check(UnityEngine.GUIUtility.systemCopyBuffer.Contains("BILATERAL_DECISION=Paired") &&
        UnityEngine.GUIUtility.systemCopyBuffer.Contains("BILATERAL_DECISION=WaitingForOther"),"actual receiver stores bilateral decisions and timestamp trace");
  Check(System.IO.File.Exists(m.SavedCapturePath),"capture saved to disk");
  string archive=System.IO.File.ReadAllText(m.SavedCapturePath);
  Check(BiofitCaptureArchive.Verify(archive),"saved archive hash and counts valid");
  // Press ARCHIVO: this must create a real CSV file, without copying or chooser.
  Check(m.ExportGameCapture() && m.CaptureFileStatus.StartsWith("EDITOR:"),
        "ARCHIVO stores a file and reports actual completion in host mode");
  string exportPath=System.IO.Path.Combine(UnityEngine.Application.persistentDataPath,
      "BiofitEditorExports",System.IO.Path.GetFileName(m.SavedCapturePath));
  Check(System.IO.File.Exists(exportPath) &&
        System.IO.File.ReadAllText(exportPath)==archive &&
        BiofitCaptureArchive.Verify(System.IO.File.ReadAllText(exportPath)),
        "one-tap export includes complete hashed CSV with all events");
  Check(!BiofitCaptureArchive.Verify(archive.Substring(0,archive.Length-20)),"truncated archive rejected");
  Check(!BiofitCaptureArchive.Verify(archive.Replace("RECHAZO_ANTIGUO","ALTERADO")),"tampered archive rejected");
  Check(!BiofitCaptureArchive.Verify(BiofitCaptureArchive.Build("tipo;etapa;\n# filas_total=1; finalizada=1\n# filas_eventos=0\n","")),"wrong row count rejected even with valid hash");
  var readiness=new BiofitTrackerReadiness();
  var status=Packet(1,true,1,100);status[2]=0x2c;
  readiness.Observe(status,1,10);
  Check(readiness.Describe(10).Contains("CAL:OK ARM:?"),"v226 arm unknown");
  status[2]=0xec;BitConverter.GetBytes((uint)200).CopyTo(status,6);
  readiness.Observe(status,1,10.1);
  Check(readiness.Describe(10.2).Contains("CAL:OK ARM:SI"),"v227 calibrated and armed");
  readiness.Observe(status,1,11.2);
  Check(!readiness.Current(11.2),"frozen timestamp cannot renew readiness");
  readiness.Reset();Check(!readiness.Current(11.2),"disconnect clears readiness");
  var capture=new BiofitEventCapture();capture.BeginGame(0);
  capture.RecordGameAction("SOLO_DER_DESCARTADO_UMBRAL",1);
  capture.RecordDiagnostic(2,1,"line\r\nwith;delimiters");
  for(int i=0;i<6100;i++)capture.RecordDiagnostic(i,1,"stress");
  capture.FinishGame(6200);
  Check(capture.Csv.Contains("SOLO_DER_DESCARTADO_UMBRAL"),"solo rejection retained");
  Check(capture.DroppedRows>0 && BiofitCaptureArchive.Verify(BiofitCaptureArchive.Build(capture.Csv,"stress")),"bounded large archive verified with drop count");
  var telemetry=new BiofitBLEMonitor();telemetry.StartGameCapture();
  var readTransport=typeof(BiofitBLEMonitor).GetMethod("ReadTransport",BindingFlags.Instance|BindingFlags.NonPublic);
  var page=new byte[20];page[0]=2;page[1]=1;page[3]=0xec;
  BitConverter.GetBytes((short)-125).CopyTo(page,12);BitConverter.GetBytes((short)-80).CopyTo(page,14);
  readTransport.Invoke(telemetry,new[]{Wrap(page,now)});
  page[2]=3;BitConverter.GetBytes((ushort)227).CopyTo(page,18);
  readTransport.Invoke(telemetry,new[]{Wrap(page,now)});
  telemetry.StopGameCapture();telemetry.CaptureCompletion.GetAwaiter().GetResult();telemetry.CopyGameCapture();
  Check(UnityEngine.GUIUtility.systemCopyBuffer.Contains("upMg=-125 velocityMmS=-80"),"firmware v2 signed diagnostic fields decoded");
  Check(UnityEngine.GUIUtility.systemCopyBuffer.Contains("fwVersion=227"),"v227 identity decoded");
  // Recover the last valid private archive after an APK restart without a new capture.
  string prevFolder=UnityEngine.Application.persistentDataPath;
  string recoverDir=System.IO.Path.Combine(System.IO.Path.GetTempPath(),
      "biofit-archive-recover-"+Guid.NewGuid().ToString("N"));
  System.IO.Directory.CreateDirectory(recoverDir);
  try {
    string good=System.IO.Path.Combine(recoverDir,"BIOFIT_v09_good.csv");
    string damaged=System.IO.Path.Combine(recoverDir,"BIOFIT_v09_damaged.csv");
    System.IO.File.WriteAllText(good,System.IO.File.ReadAllText(m.SavedCapturePath));
    System.IO.File.WriteAllText(damaged,"truncated");
    System.IO.File.SetLastWriteTimeUtc(damaged,DateTime.UtcNow.AddSeconds(30));
    UnityEngine.Application.persistentDataPath=recoverDir;
    var restarted=new BiofitBLEMonitor();
    Check(restarted.ExportGameCapture() && restarted.SavedCapturePath==good,
          "ARCHIVO after restart recovers most recent valid private session");
    Check(restarted.CopyGameCapture() &&
          BiofitCaptureArchive.Verify(UnityEngine.GUIUtility.systemCopyBuffer),
          "COPIAR after restart recovers full verified session");
    Check(!BiofitCaptureArchive.FindNewestValid(System.IO.Path.Combine(recoverDir,"missing")).Contains(".csv"),
          "no phantom recovered archive");
    System.IO.File.Delete(good);
    Check(!new BiofitBLEMonitor().ExportGameCapture(),
          "damaged latest archive not exported after restart");
  }
  finally {
    UnityEngine.Application.persistentDataPath=prevFolder;
    System.IO.Directory.Delete(recoverDir,true);
  }
  // A failed private-file save must NOT be misreported as successful finalization.
  string originalPath=UnityEngine.Application.persistentDataPath;
  string obstructed=System.IO.Path.GetTempFileName();
  try {
    UnityEngine.Application.persistentDataPath=obstructed; // existing file, not directory
    var failed=new BiofitBLEMonitor();
    Check(failed.StartGameCapture(),"failure test capture starts");
    Check(failed.StopGameCapture(),"asynchronous failure close requested");
    failed.CaptureCompletion.GetAwaiter().GetResult();
    Check(failed.CaptureFileStatus.StartsWith("ERROR archivo:"),
          "failed private save reported, not hidden behind FINALIZAR");
    Check(failed.CopyGameCapture() &&
          BiofitCaptureArchive.Verify(UnityEngine.GUIUtility.systemCopyBuffer),
          "lossless hashed clipboard fallback after storage failure");
  } finally {
    UnityEngine.Application.persistentDataPath=originalPath;
    System.IO.File.Delete(obstructed);
  }
  CycleIntegration();
  Console.WriteLine("PASS: "+checks+" receiver integration checks using API shims (not a Unity build).");
 }
}
