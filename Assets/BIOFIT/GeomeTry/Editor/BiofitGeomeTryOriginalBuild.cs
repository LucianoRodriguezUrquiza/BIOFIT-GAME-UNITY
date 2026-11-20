using System;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Biofit.GeomeTryOriginal.Editor
{
    /// <summary>
    /// Builds the ORIGINAL GeomeTry scenes. Does not generate replacement levels,
    /// change the source submodule, or overwrite EditorBuildSettings.
    /// </summary>
    public static class BiofitGeomeTryOriginalBuild
    {
        private const string Prefix = "Assets/GeomeTryOriginal/Assets/";
        private static readonly string[] SceneNames = {
            "Main Menu", "Level", "Level Editor", "Customisation",
            "Level Selection", "Success", "Settings", "Stats", "Profile", "Credits"
        };
        private static readonly string[] SourceMaps = {
            "StereoMadness", "BackOnTrack", "Polargeist", "JumpyNightmare"
        };
        // Git blob SHA-1 at Christ0u/GeomeTry@d922cadd... (raw bytes, no adaptation).
        private static readonly string[] MapBlobs = {
            "0af72f78ccc80ad982fcb7aabc0c39387bb2350c",
            "47e10ff7f5b48a8045e3ddcfdd20e1dce2c0deda",
            "fe4af269c83b2984c643d6f3282799152ea84dc4",
            "7c0b86d610daf6b79654b3edeacb5e412dcc2c80"
        };

        private static string Scene(string name) => Prefix + "Scenes/" + name + ".unity";

        private static string GitBlobSha(byte[] contents)
        {
            byte[] header = Encoding.ASCII.GetBytes("blob " + contents.Length + "\0");
            byte[] combined = new byte[header.Length + contents.Length];
            Buffer.BlockCopy(header, 0, combined, 0, header.Length);
            Buffer.BlockCopy(contents, 0, combined, header.Length, contents.Length);
            using (var sha = SHA1.Create())
                return BitConverter.ToString(sha.ComputeHash(combined)).Replace("-", "").ToLowerInvariant();
        }

        // Git autocrlf on Windows may convert only CRLF line endings in JSON.
        // All other bytes are preserved and must still match the pinned Git blob.
        private static byte[] NormalizeCrLf(byte[] contents)
        {
            using (var output = new MemoryStream(contents.Length))
            {
                for (int i = 0; i < contents.Length; i++)
                {
                    if (contents[i] == 13 && i + 1 < contents.Length && contents[i + 1] == 10)
                    {
                        output.WriteByte(10);
                        i++;
                    }
                    else
                    {
                        output.WriteByte(contents[i]);
                    }
                }
                return output.ToArray();
            }
        }


        // The upstream GameManager.SceneExists() silently returns false when
        // the scene isn't in Build Settings, even if the menu button animates.
        private static void VerifyNavigationAndResources()
        {
            string[] expected = Array.ConvertAll(SceneNames, Scene);
            string[] registered = EditorBuildSettings.scenes
                .Where(s => s.enabled).Select(s => s.path).ToArray();
            if (!registered.SequenceEqual(expected))
                throw new InvalidOperationException(
                    "GeomeTry requiere sus 10 escenas originales en Build Profiles > " +
                    "Scene List, con Main Menu primera. No modifiques los JSON.");
            if (UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings < SceneNames.Length)
                throw new InvalidOperationException(
                    "El perfil de Unity activo no reconoce todas las escenas GeomeTry. " +
                    "Revisa File > Build Profiles > Scene List.");

            // Validate Resources.Load exactly as used by upstream gameplay,
            // including the fifth original level WaveVibe (not just 4 JSON hashes).
            foreach (string levelName in new[] {
                "StereoMadness", "BackOnTrack", "Polargeist", "JumpyNightmare", "WaveVibe"
            })
            {
                TextAsset source = Resources.Load<TextAsset>("maps/BASE/" + levelName);
                if (source == null)
                    throw new InvalidOperationException(
                        "Resources no encuentra el mapa original " + levelName);
                Level level;
                try { level = new Level(source); }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Error al leer el nivel original " + levelName, ex);
                }
                if (level.Map == null || level.Map.Count == 0 || level.Music == null)
                    throw new InvalidOperationException(
                        "Mapa sin objetos o musica original: " + levelName);
                foreach (MapItem item in level.Map)
                {
                    // Upstream uses 'ground' as a marker; it is not a prefab.
                    if (item.Type == "ground") continue;
                    if (!Prefab.Prefabs.TryGetValue(item.Type, out GameObject obj)
                        || obj == null)
                        throw new InvalidOperationException(
                            "Prefab original faltante: " + item.Type +
                            " en el mapa " + levelName);
                }
            }

            // A legacy BLE pulse must operate the original in-air orbs too,
            // or Polargeist/JumpyNightmare cannot be played by bodily input.
            string orbPath = Prefix + "Scripts/JumpOrb.cs";
            if (!File.Exists(orbPath) ||
                !File.ReadAllText(orbPath).Contains("TryConsumeOrbPulse()"))
                throw new InvalidOperationException(
                    "Falta compatibilidad de entrada BLE en JumpOrb. " +
                    "Cierra Unity, actualiza Git y ejecuta " +
                    "tools/apply_geometry_compat.ps1. " +
                    "No modifiques los mapas ni la fisica.");

            foreach (string resource in new[] {
                "Prefabs/Player/CubePrefab", "Prefabs/Player/ShipPrefab",
                "Prefabs/Player/WavePrefab", "Prefabs/Decors/GroundPrefab",
                "Prefabs/Decors/CeilPrefab", "Prefabs/UI/LevelButtonPrefab"
            })
                if (Resources.Load<GameObject>(resource) == null)
                    throw new InvalidOperationException(
                        "Prefab de juego no disponible via Resources: " + resource);

            foreach (string resource in new[] {
                "Sprites/Character/Ship/Ship",
                "Sprites/Character/Wave/Wave001",
                "Sprites/Obstacles/Tile/smallTile"
            })
                if (Resources.LoadAll<Sprite>(resource).Length == 0)
                    throw new InvalidOperationException(
                        "PNG de GeomeTry no importado correctamente: " + resource);

            UnityEngine.Debug.Log(
                "BIOFIT GeomeTry PREVUELO: 10 escenas, 5 mapas originales, " +
                "prefabs, musica y sprites verificados.");
        }

        [MenuItem("BIOFIT/GeomeTry ORIGINAL/1. Verificar fuente y mapas")]
        public static void Verify()
        {
            if (!File.Exists(Prefix + "Scripts/GameMechanics/CharacterType/Cube.cs"))
                throw new InvalidOperationException(
                    "Faltan los Assets originales. Cerra Unity y ejecuta tools/prepare_geometry_original.ps1 desde un clon NUEVO.");
            foreach (string name in SceneNames)
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene(name)) == null)
                    throw new InvalidOperationException("Falta escena ORIGINAL: " + Scene(name));
            for (int i = 0; i < SourceMaps.Length; i++)
            {
                string path = Prefix + "Resources/maps/BASE/" + SourceMaps[i] + ".json";
                if (!File.Exists(path)) throw new InvalidOperationException("Falta mapa: " + path);
                byte[] raw = File.ReadAllBytes(path);
                // Git on Windows may check out JSON with CRLF instead of the LF
                // stored in the upstream blob. Accept *only* that EOL conversion.
                // Never rewrite the JSON or weaken the upstream hash contract.
                string actual = GitBlobSha(raw);
                if (actual != MapBlobs[i])
                {
                    byte[] lf = NormalizeCrLf(raw);
                    if (lf.Length == raw.Length || GitBlobSha(lf) != MapBlobs[i])
                    {
                        throw new InvalidOperationException(
                            "Mapa distinto del original incluso tras normalizar CRLF: " + path +
                            " (SHA1 Git=" + actual + ", esperado=" + MapBlobs[i] + "). " +
                            "No cambies los JSON. Revisa el checkout de Git.");
                    }
                    UnityEngine.Debug.Log("BIOFIT: mapa verificado con saltos de linea de Windows (CRLF): " + path);
                }
            }
            foreach (string name in new[] {"CubePrefab", "ShipPrefab", "WavePrefab"})
                if (AssetDatabase.LoadAssetAtPath<GameObject>(Prefix + "Resources/Prefabs/Player/" + name + ".prefab") == null)
                    throw new InvalidOperationException("Falta prefab original: " + name);
            VerifyNavigationAndResources();
            UnityEngine.Debug.Log("BIOFIT GeomeTry original: escenas, personajes y los cuatro mapas RAW verificados sin modificar.");
        }

        [MenuItem("BIOFIT/GeomeTry ORIGINAL/2. Abrir Main Menu original")]
        public static void Open()
        {
            Verify();
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Scene("Main Menu"));
        }

        [MenuItem("BIOFIT/GeomeTry ORIGINAL/4. Activar URP original para probar en Play")]
        public static void EnableOriginalPreview()
        {
            Verify();
            if (!EditorUtility.DisplayDialog("Cambiar render solo en esta copia",
                "La vista previa del GeomeTry original necesita URP 2D. Este cambio afecta temporalmente a la configuracion grafica de esta copia del proyecto, no al firmware, los mapas ni la fisica. ¿Continuar?",
                "Activar URP", "Cancelar")) return;
            var urp = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(Prefix + "Settings/UniversalRP.asset");
            if (urp == null) throw new InvalidOperationException("No se encontró el asset URP original.");
            QualitySettings.renderPipeline = null;
            GraphicsSettings.defaultRenderPipeline = urp;
            UnityEngine.Debug.Log("Vista previa GeomeTry: URP original activado. Usa opcion 5 para restaurar.");
        }

        [MenuItem("BIOFIT/GeomeTry ORIGINAL/5. Restaurar render BIOFIT de esta copia")]
        public static void RestoreBiofitPreview()
        {
            QualitySettings.renderPipeline = null;
            GraphicsSettings.defaultRenderPipeline = null; // base v0.9.11 uses Built-in
            UnityEngine.Debug.Log("Render BIOFIT restaurado a Built-in en esta copia.");
        }

        [MenuItem("BIOFIT/GeomeTry ORIGINAL/3. Compilar APK Android original + BLE LEGACY")]
        public static void BuildAndroid()
        {
            Verify();
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Detene Play antes del build.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                throw new InvalidOperationException("Selecciona Android en File > Build Profiles > Switch Platform.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            RenderPipelineAsset urp = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(
                Prefix + "Settings/UniversalRP.asset");
            if (urp == null) throw new InvalidOperationException("Falta URP original. Comproba el paquete Universal RP y el submodulo.");
            RenderPipelineAsset previousPipeline = GraphicsSettings.defaultRenderPipeline;
            bool previousBundle = EditorUserBuildSettings.buildAppBundle;
            string previousIdentifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            string previousProductName = PlayerSettings.productName;
            string folder = Path.Combine(Directory.GetCurrentDirectory(), "Builds");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder,
                "BIOFIT_GeomeTry_ORIGINAL_LEGACY_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".apk");
            try
            {
                // Only for this build, then restore. Original URP asset is untouched.
                // Build as a DIFFERENT Android application, protecting the existing
                // BIOFIT DASH installation and its private recordings.
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.biofitgame.geometryoriginal");
                PlayerSettings.productName = "BIOFIT GeomeTry";
                GraphicsSettings.defaultRenderPipeline = urp;
                EditorUserBuildSettings.buildAppBundle = false;
                string[] scenes = Array.ConvertAll(SceneNames, Scene);
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = scenes, locationPathName = path,
                    target = BuildTarget.Android, options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Unity no generó APK: " + report.summary.result);
                UnityEngine.Debug.Log("APK generada sin prueba en dispositivo: " + path);
                EditorUtility.RevealInFinder(path);
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBundle;
                GraphicsSettings.defaultRenderPipeline = previousPipeline;
                PlayerSettings.productName = previousProductName;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, previousIdentifier);
            }
        }
    }
}
