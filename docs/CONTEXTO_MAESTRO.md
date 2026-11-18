# BIOFIT GAME — CONTEXTO MAESTRO

> **Punto de entrada y memoria técnica versionada del proyecto.** Leer esta página **antes** de modificar firmware, Unity, Android, pruebas o documentación.
>
> **Última revisión manual:** 2026-11-17 · **Estado técnico de referencia:** revisión diagnóstica **v0.9.5 RAW**, [PR #10](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10), CI host verde, sin APK ni validación física; base Unity **v0.9.4 EXP A/B** ([PR #8](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/8), **borrador**, CI host verde, APK nueva pendiente); la **v0.9.3 sí fue probada físicamente en Android**, con exportación CSV válida. Firmware instalado **v2.2.7** en ambos tobillos.
>
> **Importante:** este archivo es una síntesis de continuidad, **no una certificación** de funcionamiento físico. Para comprobar el código real, consultar las ramas y PR actuales; las pruebas offline no equivalen a ensayos BLE/Android en un dispositivo.

## 0. Cómo usar este documento al iniciar un chat nuevo

1. Abrir **este archivo en la rama `docs/contexto-maestro`** (no confundir con `main`): [CONTEXTO_MAESTRO.md](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/docs/contexto-maestro/docs/CONTEXTO_MAESTRO.md).
2. Revisar los **PR abiertos y las ramas activas** de los dos repositorios. Los commits/estados aquí descritos pueden haber cambiado después de la última revisión.
3. Leer el informe de la versión actual y, cuando el trabajo lo requiera, los `.ino` y `.cs` reales. No reconstruir código a partir de conversaciones anteriores.
4. Distinguir: **implementado**, **compilado/probado offline**, **probado en Unity Play**, **validado en Android real**, y **validado con sensores reales**.
5. Antes de actualizar la PC, preguntar o revisar los **cambios locales no confirmados en GitHub Desktop**. No descartar escenas ni configuración local sin inspección.

## 1. Propósito y arquitectura prevista

**BIOFIT GAME** es una plataforma de *exergaming* de bajo costo donde un **smartphone Android actúa como consola**, recibe por **Bluetooth Low Energy (BLE)** señales de sensores inerciales corporales y controla un juego desarrollado en **Unity**. La misma aplicación está pensada para modo pantalla normal, salida a TV y, posteriormente, modo VR estereoscópico mediante visor para smartphone.

**Hardware final previsto (no implementado todo aún):**
- Hasta **4 trackers Waveshare ESP32-S3-LCD-1.47B**, cada uno con IMU **QMI8658**: dos tobillos y, más adelante, dos muñecas.
- **Joystick Bluetooth Gadnic**, posterior a la validación básica del control inercial.
- Smartphone Android como procesador y pantalla / fuente de proyección.
- Primera etapa **solo dos tobillos**; después caminar/correr en el lugar; finalmente juego completo, muñecas, joystick, VR.

**Arquitectura lógica:**

`QMI8658 → adquisición/filtrado/calibración en ESP32-S3 → eventos y telemetría BLE → receptor Unity C# → diagnósticos y decisiones de gameplay → juego Android`

Las detecciones `MOV_ON`, `MOV_OFF` y `UP_IMPULSE` **no son, por sí mismas, pasos o saltos biomecánicos confirmados**. Se deben validar los gestos y el conteo humano antes de declarar exactitud.

## 2. Repositorios y fuentes de verdad

**Unity:** https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY

**Firmware:** https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-Firmware

| Elemento | Referencia comprobada al 2026-11-17 | Estado |
| --- | --- | --- |
| Unity v0.9.5 RAW | `feat/raw-session-v095`, commit `7fdd72b61dda8bb53eafc528be166443eb39489b`, [PR #10](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10) | Borrador; 273 checks C# y CI verde; sin compilación Unity/APK ni prueba física |
| Unity v0.9.4 (propuesta experimental A/B) | `feat/diagnostic-threshold-ab-v094`, commit `6feb3c0fcfb205e87d7e6c9d606a1147285a87dc`, [PR #8](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/8) | **Borrador**, CI host verde, **todavía sin compilar ni validar APK**; EXP permite probar 0,15g solo opt-in |
| Unity v0.9.3 | `fix/csv-resource-close-audit-v093`, commit `4f58562f757fc608d0951127afe995670d300878`, [PR #7](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/7) | **Borrador**, CI host verde; exportación física confirmada para CSV v0.8 y nueva captura v0.9.3 (secciones 7A/7B) |
| Unity v0.9.2 (base de v0.9.3) | `fix/automatic-download-csv-v092`, [PR #6](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/6) | Borrador, CI host verde |
| Unity v0.9.1 | `fix/export-package-recover-v091`, [PR #5](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/5) | Borrador |
| Unity v0.9 | `fix/defaults-025-phase-export-v09`, [PR #4](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/4) | Borrador |
| Firmware v2.2.7 | `fix/imu-state-rest-v227`, commit `9cb5b7ac06315b0b657a33b8e608aca335eccb9e`, [PR #2](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-Firmware/pull/2) | Borrador; **físicamente cargado en ambos tobillos** según ensayos del usuario |
| Documento maestro | `docs/contexto-maestro`, archivo `docs/CONTEXTO_MAESTRO.md` | **Documento canónico** de continuidad |

**No asumir que `main` contiene estas revisiones.** No se fusionaron los PR citados. Cada nueva intervención de código debe derivar de la rama **más reciente efectivamente verificada**, salvo justificación explícita.

**Estado de la PC del usuario:** en sesiones anteriores había **8–9 archivos locales modificados de escenas y configuración Unity** no confirmados en GitHub. Su presencia actual debe comprobarse. Cuidado al hacer checkout, stash, commit, discard o merge. No afirmar que esos cambios locales están publicados.

## 3. Hardware, montaje y mapeo anatómico

**Trackers activos:**
- `TRACKER_ID=1`: izquierdo, BLE `BIOFIT-LEFT-01`.
- `TRACKER_ID=2`: derecho, BLE `BIOFIT-RIGHT-02`.
- Los sketch del firmware son equivalentes salvo identificador del tracker.
- Placas: Waveshare ESP32-S3-LCD-1.47B, LCD sin táctil, IMU QMI8658; firmware Arduino-ESP32 3.3.x.
- Monitor Serie: 115200 baudios.

**Montaje físico de referencia:** ambas placas en el **lado izquierdo anatómico de su respectiva pierna**, con la **pantalla orientada hacia la izquierda corporal** y **USB-C apuntando hacia atrás/hacia el talón**. **No montarlas en espejo.**

**Sistema anatómico ideal de referencia:** `+X` hacia adelante, `+Y` hacia la izquierda, `+Z` hacia arriba. No confundir directamente los ejes físicos del QMI8658 con esos ejes; la calibración determina la transformación efectiva. Hay material de orientación vectorial en los archivos del proyecto.

**Precaución:** conservar calibración, NVS, mapa anatómico y datos de flash, salvo necesidad experimental explícita. No activar *Erase All Flash* durante actualizaciones ordinarias.

## 4. Firmware activo: v2.2.7

Rama: `fix/imu-state-rest-v227` en el repositorio Firmware.

- Adquisición IMU, mapa anatómico, referencia gravitatoria, filtrado, estados de movimiento/reposo y detección de candidato `UP_IMPULSE`.
- BLE con EVENT V2 de **16 bytes**, STATUS de **20 bytes**, y telemetría TRANSPORT v2 opcional/rotativa. Mantener UUID e identificadores de protocolo.
- v2.2.7 endureció calibración de estabilidad, corrigió rearme tras reposo válido y añadió controles de estabilidad de pose para AUTO REST.
- Pantalla `MOVIMIENTO` indica actividad general, **no garantiza que se haya generado `UP_IMPULSE`**.
- `CAL`, `ARM`, `MOV`, `restCount`, `rearms`, `blockedSamples`, `blockMask`, sesgo, gravedad y amplitud son diagnósticos relevantes para fallos del derecho.
- El integrador `shortV` no equivale a una velocidad física absoluta. Los saltos al descender pueden deberse a aceleración al frenar la caída, pero **no hay causalidad demostrada**.

**Firmware v2.2.7 se mantiene congelado para las pruebas siguientes** mientras no se compruebe una causa reproducible que justifique cambiarlo.

Fuentes: [README firmware](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-Firmware/blob/fix/imu-state-rest-v227/README.md), [revisión firmware v2.2.7](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-Firmware/blob/fix/imu-state-rest-v227/docs/REVISION_V08.md).

## 5. Unity / BIOFIT DASH: estado del juego

**Entorno de PC conocido:** Unity **6.3 LTS, 6000.3.28f1**, GitHub Desktop para cambios de ramas. Android como plataforma de compilación. Escenas esperadas en Build Profiles:
1. `DiagnosticoBLE` habilitada, índice 0.
2. `BiofitDash` habilitada, índice 1.
3. `SampleScene` deshabilitada.

En Unity Play se puede validar interfaz, pista, posición del cubo, botones, escenas y lógica manual **sin esperar una compilación APK**. Para BLE, MediaStore, permisos y funcionamiento real del teléfono sí se necesita una APK.

**Configuraciones de referencia encontradas experimentalmente:**

| Parámetro | Valor |
| --- | ---: |
| Umbral unilateral izquierdo en Unity | **0,25 g** |
| Umbral unilateral derecho en Unity | **0,25 g** |
| Umbral bilateral UP | **0,12 g** |
| Emparejamiento bilateral | **180 ms** |
| Firmware de cada sensor | **v2.2.7** |

No bajar arbitrariamente umbrales ni prolongar anti-rebote/cooldown: puede mejorar un contador y empeorar latencia, falsos positivos o habilidad para saltar consecutivamente. Los controles independientes por tobillo se mantienen.

BIOFIT DASH actual es una **base de juego 2D de diagnóstico de salto**, no un Geometry Dash completo. Existen modos `BLE: SOLO IZQ`, `SOLO DER`, `BILATERAL`, `OFF`; botones `QUIETO`, `CAMINAR`, `CORRER`, `ELEVACION`, `DASH`, así como `INICIAR`, `FINALIZAR`, `ARCHIVO`, `COPIAR`. Las etiquetas de marcha/carrera **no implican que exista un clasificador físico validado**.

## 6. Evidencia experimental y distinciones de diagnóstico

**Registros v0.8** (reconstruidos de pruebas anteriores, no necesariamente los archivos CSV originales íntegros):

| Sesión y bloque | UP recibidos | Bajo umbral | Anti | Cubo ocupado | Saltos ejecutados |
| --- | ---: | ---: | ---: | ---: | ---: |
| Inicial, SOLO IZQ 0,35 g | 24 | 8 | 1 | 3 | 12 |
| Inicial, SOLO DER 0,35 g | 20 | 15 | 0 | 0 | 5 |
| Posterior, SOLO DER 0,25 g | 73 | 18 | 0 | 14 | 41 |
| Posterior, SOLO IZQ 0,25 g | 25 | 9 | 0 | 5 | 11 |

Son conteos **de impulsos electrónicos**, no porcentajes de acierto sobre elevaciones humanas. Observación del usuario: con 0,25 g mejoró mucho en ambos pies (derecho ~90%, izquierdo ~95% de aciertos percibidos en sesiones distintas; estimaciones no estandarizadas). Algunos impulsos dobles cuando se mantiene el pie alto y algunos saltos tardíos durante la bajada. No introducir un bloqueo agresivo sin ensayar el impacto.

**Última sesión física v0.9** (según captura y conteo humano, **antes** de compilar v0.9.3):

- Panel final: **340 eventos**, `Sensores 2/2`, reloj alineado, CAL:OK y ARM:SI para ambos, pares bilaterales 20, **0 rechazos BLE declarados**.
- SOLO IZQ: `UP=59`, `bajo=14`, `anti=2`, `salto=32`, `ocupado=11`.
- SOLO DER: `UP=15`, `bajo=4`, `anti=0`, `salto=9`, `ocupado=2`.
- Observación del usuario: las **primeras 2 elevaciones derechas tardías**, ~8 siguientes satisfactorias, y **últimas 10 sin saltos visibles**; izquierdo y bilateral funcionaron bien.
- **Sin explicación causal confirmada** para la caída de respuesta derecha. `ARM:SI` final no prueba que estuvo armado continuamente ni descarta falta de UP o cambio de dinámica.
- El archivo de Google Docs llamado `CSV v9` es **incompleto**: 20.151 bytes y corte a **37,845 s** en mitad de una línea; NO incluye tramo derecho y bilateral ni footer SHA256. No usarlo como registro físico completo.
- **Prioridad:** recuperar el CSV **original privado íntegro** desde la misma instalación Android antes de exigir una nueva sesión.

**Regla de interpretación:** distinguir ausencia de `UP_IMPULSE` en firmware; UP recibido pero rechazado por Unity (`BAJO/ANTI`); aceptado mientras cubo ocupado; y salto ejecutado tarde. Una cola BLE pequeña no mide el tiempo desde el inicio biomecánico del gesto.

## 7. Exportación CSV — defecto confirmado y revisión v0.9.3

**Problema real observado en APK v0.9:** botón `3. ARCHIVO` devolvió `java.lang.ClassNotFoundException` al invocar una `CsvExportActivity` personalizada. `COPIAR` usa portapapeles y el contenido pegado se truncó; **no constituye una exportación fiable de sesiones largas**.

**Cadena de cambios (estado histórico; actualización física en 7A/7B):**
- **v0.9.1 / PR #5:** intentó corregir estructura del plugin Java y recuperar archivos privados válidos entre reinicios.
- **v0.9.2 / PR #6:** cambió la ruta activa de exportación a `MediaStore.Downloads` desde Unity/C#, **sin selector ni COPIAR**, y conservó recuperación del último CSV privado válido.
- **v0.9.3 / PR #7:** Astra auditó v0.9.2 y corrigió cierre explícito de streams/descriptores Java (no basta liberar JNI), comparación completa del contenido y SHA256 del destino, `IS_PENDING` durante escritura y una sola instantánea del archivo fuente. En esa auditoría las comprobaciones Android reales estaban pendientes; ver validación posterior en 7A/7B.

**Comportamiento previsto en futura APK v0.9.3:** `FINALIZAR` conserva CSV privado con conteos y footer SHA256; presionar `ARCHIVO` guarda automáticamente en **Descargas** sin selector; mensaje de éxito solo tras verificar y publicar. En una instalación actualizada con la **misma firma e ID de paquete**, `ARCHIVO` puede recuperar la sesión previa **sin INICIAR**. Android 10+; no borrar datos ni desinstalar la versión anterior.

**Verificado por Astra en v0.9.3:** pruebas host C# 207 comprobaciones; test de contrato; firmware contrato y replay existentes (90 gestos / 120 s simulados); CI Unity verde. **No se ejecutó ni confirmó** compilación de APK Unity, Gradle/puente JNI en Android real ni recuperación física de datos. Una Activity Java heredada puede compilar en CI sin participar en el exportador actual. No declarar éxito físico hasta probar en celular.

Detalles y fuentes: [Auditoría v0.9.2, cambios v0.9.3](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/fix/csv-resource-close-audit-v093/docs/AUDITORIA_V092.md), [protocolo anterior v0.9.2](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/fix/automatic-download-csv-v092/docs/REVISION_V092.md).

## 7A. Validación física nueva: exportación real de v0.9.3

**Fecha:** 2026-11-17. El usuario instaló una APK construida desde la rama Unity v0.9.3 y pulsó `3. ARCHIVO` sin iniciar una captura nueva. Android generó un CSV descargable, adjuntado luego al chat con nombre `BIOFIT_v08_20261116_175748_c69d4882.csv`.

**Inspección de bytes del CSV exportado:** 384.042 bytes; **3.066 filas de registro** más cabecera y footer; **351 eventos `EV`**; **cero descartados**; `finalizada=1`; SHA-256 del cuerpo coincide exactamente con el footer `a8f80f81d810d82bf1fbc165b94998267af6fc3d857671db6f09cba1b6c4d270`. La exportación a Descargas en **este teléfono real** funcionó y conservó íntegramente el archivo v0.8. Esto **no** prueba todas las versiones Android ni que el contenido faltante de otra sesión pueda recuperarse.

**Advertencia crítica:** este archivo pertenece a **v0.8**, no a la última prueba **v0.9 de 340 eventos** con fallo intermitente derecho. En v0.8 figura umbral unilateral `0.35 g`; resumen `IZQ UP=124`, `DER UP=57`, `PARES UP=23`. **No usar ese CSV para atribuir causas a los últimos diez fallos derechos de la sesión v0.9.** La recuperación automática eligió un archivo v0.8 válido; con los datos disponibles no es posible afirmar si la captura v0.9 existe, está truncada, inaccesible o no se guardó. Primero buscar en Descargas otros archivos `BIOFIT_v09_*.csv`; si no aparecen, una prueba física breve posterior será la vía de nuevo diagnóstico.

**Estado actualizado:** exportación Android v0.9.3 **confirmada físicamente para un archivo íntegro**, recuperación de la sesión v0.9 **pendiente**; falla del derecho **sin resolver**. Conservar la APK y los datos privados. No repetir una sesión larga mientras se decide si el archivo original puede localizarse.

## 7B. Nueva prueba física v0.9.3 (215 EV) y explicación parcial de latencia

**Fecha:** 2026-11-17. Archivo recibido `BIOFIT_v09_20261117_135943_b05673f0.csv`; **240.111 bytes**; **1.970 registros**, **215 EV**, **135 APP**, cero descartados; footer SHA256 `80c7004300652188c11a6cfce26362ed347ef56fcf4ad996562c5bc0bb1026bf` validado. El archivo original no se ha subido al repositorio público.

**Resultados físicos relatados:** 20 elevaciones derechas todas con salto visual, primeras 10 con activación en pie alto/descenso y últimas 10 con respuesta rápida; 10 elevaciones izquierdas sin retraso perceptible y cinco bilaterales con retraso nuevo, aunque saltaron.

**Evidencia del CSV:**
- Tramo SOLO DER: **43 UP, nueve bajo 0,25g, 34 candidatos aceptados, 23 saltos Unity, 11 cubo ocupado**. Los 23 saltos no son 23 gestos físicos: algunos movimientos generan dos impulsos.
- En las primeras 10 secuencias derechas, **7/10 primeros UP tienen amplitud inferior a 0,25g**; segundos UP de ~0,7 s después sí superan el umbral. Ejemplo `180,550s:0,172g` rechazado; `181,319s:0,373g` aceptado. Este patrón coincide con la latencia percibida sin requerir demora BLE.
- Tramo SOLO IZQ: 23 UP, cuatro bajo umbral, 19 aceptados, 11 saltos y ocho ocupados.
- Bilateral: cinco pares aceptados y cinco saltos; en **tres episodios** el primer UP derecho fue `0,111g`, `0,114g` o `0,100g`, inferior al corte 0,12g; pares posteriores sí disparan el salto. Ejemplo primeros candidatos `288,771s` izquierdo 0,191g y `288,804s` derecho 0,111g, seguidos por par aceptado cerca de 289,831s.
- Entrega BLE sin rechazos, huecos, descartes ni tiempo comprimido. Cola Unity mediana derecho UP ~17,4ms, máximo 33,3ms. **No equivale a medir movimiento físico→píxel**; la latencia observada es compatible con lógica de selección de eventos, no con cola BLE de un segundo.

**Distinción científica:** se demuestra el rechazo de primeros eventos débiles y aceptación de posteriores; **la asignación exacta del impulso a fase de subida/descenso sigue siendo una hipótesis** porque no hay marcador externo del movimiento físico ni RAW de IMU etiquetado. No declarar que el firmware está defectuoso por este CSV; tampoco asumir que reducir el umbral a 0,15g será seguro sin comprobar falsos positivos.

**Nueva revisión propuesta:** [Unity PR #8](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/8) añade botón `EXP: NO/SI` al panel. Normal: umbral unilateral mínimo 0,25g. EXP: permite explorar 0,20 y 0,15g; desactivar EXP restaura mínimos 0,25g. Bilateral sigue 0,12g, con control manual para ensayo 0,10g. Los cambios se registran en CSV. **No modifica firmware, NVS ni valores predeterminados.** CI: 58+9+31, 34 receptor, 16 Android simulated, 66 UI, contrato estático y Java heredado; [Actions](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/actions/runs/44100922377) **verde**. *No hay APK v0.9.4 compilada todavía.*

[Informe y protocolo completo de una sola sesión A/B/A](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/feat/diagnostic-threshold-ab-v094/docs/ANALISIS_CSV_20261117_AB.md).

## 7C. Auditoría de fase: límite reproducido sin cambiar producción

**2026-11-17 — [PR #9, borrador](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/9)**, rama `audit/lift-phase-v094`, commit `52d61c0c2ca9aac6a3266f4f6181f04be8cfe005`, derivada de v0.9.4. [Informe comparativo y protocolo](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/audit/lift-phase-v094/docs/AUDITORIA_FASE_ELEVACION.md).

- **Demostrado en host:** se compiló el `processSample` real de v2.2.7 y tres descensos sintéticos emitieron UP durante frenado, todavía con velocidad física negativa. Ejemplo: frenado0,35g, UP0,307210g a520ms, velocidad real −0,274586m/s y `shortV` +0,046149m/s. Ese UP supera también el corte unilateral normal. No demuestra que ese fuera el movimiento del usuario en cada evento.
- `shortV` tiene fuga, saturación y resets; **no identifica velocidad/fase física**. Histéresis y rearme ya existen; aumentarlos puede perder sensibilidad. `downSeen` usa aceleración negativa, que también puede frenar una subida.
- El UP transmite amplitud **al disparar**, no el máximo del gesto. Un evento0,172g no prueba que el pico posterior de esa subida no cruzara0,25g. El análisis de fase humana necesita vídeo y la trayectoria fina requiere RAW.
- **Implementado:** pruebas y documentación, sin cambios en Assets, firmware, NVS/calibración, UUID o defaults0,25/0,25/0,12g y180ms. Local:236 comprobaciones C# PASS; contrato/replay firmware original y nuevo contraejemplo PASS; contrato Android estático PASS. No se compiló Unity/APK/ESP32 ni se hizo ensayo físico. El CSV privado no se releyó: las cifras experimentales provienen del informe7B.
- **Propuesta no implementada:** detector causal en modo sombra con estados de ascenso/pausa/descenso/frenado, integración con incertidumbre y gyro auxiliar. No prometer detección temprana si todo el ascenso permanece por debajo del corte obligatorio.
- **Captura pendiente:** firmware ofrece RAW20Hz vía STREAM,1, pero Unityv0.9.4 no lo suscribe ni lo guarda.20Hz no reproduce decisiones de dos muestras. Un clasificador fino necesita instrumentación RAW a mayor cadencia, sincronización y evaluación de carga antes de validación física.
- **Siguiente sesión mínima de fase:**18 ciclos filmados (6derechos,6izquierdos,6bilaterales; mitad lentos/mitad normales, pausa arriba~1s), umbrales normales, marcas visibles/CSV y exportación íntegra. Sirve para asignar fase a UP, no para certificar el nuevo clasificador. Para validar también el clasificador en una sola sesión, preparar antes RAW; no pedir dos sesiones redundantes ni otra APK por este PR solo documental/de pruebas. El A/B/A de la sección8 queda como experimento alternativo de umbral, no obligatorio para esta auditoría.

## 7D. v0.9.5: instrumentación para una única sesión integral

**2026-11-17.** [PR #10 en borrador](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10), rama `feat/raw-session-v095`, commit `7fdd72b61dda8bb53eafc528be166443eb39489b`, derivado de `audit/lift-phase-v094`. [Informe y protocolo completo](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/feat/raw-session-v095/docs/REVISION_V095_RAW.md). [CI verde](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/actions/runs/44109090787): regresiones C#, replay C++ de fase y Java heredado (no APK).

- **Implementado:** RAW opcional de ambos sensores a 20 Hz, característica existente y comandos STREAM,1/0 con ACK `OK,STREAM,n`. Seis ejes físicos, timestamp sensor, secuencia, reloj de recepción, pérdidas, intervalos y espera de worker. Colas separadas de la cola del juego y escritura en segundo plano.
- **Almacenamiento DASH:** principal EV/APP/CONFIG/MARCA/DIAG y compañero `_RAW.csv`, ambos con SHA256. Escritura progresiva y cierre asíncrono, parciales preservados ante fallos. El límite de 6.000 filas queda para la vista previa, no el archivo de la sesión DASH. ARCHIVO exporta/recupera ambos; conserva capturas anteriores. El modo guiado antiguo mantiene su portapapeles acotado.
- **Corregido:** v0.9.4 intentaba registrar MODO_EXP_UMBRAL_ON/OFF, pero su whitelist los descartaba. v0.9.5 sí los conserva y registra CONFIG por pie. Agrega botón SYNC numerado con dos relojes y métricas de transporte RAW ON/OFF.
- **Sin cambios:** detector de producción, defaults 0,25/0,25/0,12 g y 180 ms, firmware v2.2.7, NVS/calibración, UUID, escenas locales. No hay nuevo clasificador implementado.
- **Verificación:** 273 checks C# host PASS; carga sintética de 24.002 RAW válidos sin drops del writer/cola, 12.000 APP y 120 EV conservados; fallos GATT/ACK/IO, timeout, saturación, pausa/desconexión y recuperación. Contrato Android estático y replay C++ PASS. No Unity/IL2CPP/APK, radio Android ni validación física ejecutados.
- **Límites:** a un tracker ya desconectado no se le puede confirmar STREAM,0; apagado local y remoto best effort, comando de limpieza al conectar de nuevo. RAW 20 Hz no reproduce las decisiones internas ni garantiza capturar máximos. Se mide impacto físico BLE; los tests host no garantizan ausencia de degradación en el teléfono.
- **Decisión:** no aumentar frecuencia ni reflashear antes de esta sesión. 40 notificaciones RAW/s entre ambos trackers, 800 B/s de valores (920 B/s a nivel ATT), más overhead y tráfico existente. Adecuado para investigar lóbulos separados ~0,7 s con vídeo; insuficiente para certificar un clasificador fino de cadencia IMU.
- **Siguiente acción única:** preservar cambios locales, comprobar Play y compilar una APK de esta rama con firma/paquete existentes. Una captura filmada de **40 ciclos** integra A/B/A derecho, control izquierdo, bilateral y pausas arriba/descenso controlado; cuatro ciclos RAW OFF permiten comparación de transporte. FINALIZAR, esperar cierre, ARCHIVO y conservar dos CSV + vídeo. El procedimiento detallado sustituye los protocolos anteriores para evitar sesiones redundantes.

## 8. Procedimiento anterior (referencia histórica; sustituido por v0.9.5): A/B/A v0.9.4 (sin repetir múltiples APK)

**Actualización:** para la siguiente compilación y ensayo seguir la sección 7D y su protocolo v0.9.5; no ejecutar también estos 60 gestos. **Estado histórico:** la sesión v0.9.3 ya aporta diagnóstico parcial; la exportación a Descargas sí funcionó en Android real. El nuevo ensayo A/B/A de v0.9.4 busca comparar adelanto de activación **contra** falsos positivos. No alterar firmware ni recalibración NVS.

1. GitHub Desktop: cerrar Unity y preservar archivos locales de escenas/configuración. `Fetch origin`, checkout `feat/diagnostic-threshold-ab-v094`. Si se ofrece `Bring my changes` y no hay conflictos, trasladarlos. **Stash: NO; Squash: NO; Discard: NO; Commit: NO por defecto**, especialmente con escenas locales. No fusionar PR.
2. Unity Play: comprobar `EXP: NO`, umbrales 0,25/0,25g y bilateral 0,12g/180ms; activar `EXP: SI` y constatar que en el pie derecho los controles permiten 0,20 y 0,15g; desactivar y verificar que regresa a 0,25g. Sin errores rojos, escenas `DiagnosticoBLE` 0 y `BiofitDash` 1.
3. Una sola APK v0.9.4, misma identidad y firma, instalar como actualización sin borrar datos. Firmware **v2.2.7 sin flashear**.
4. Con ambos BLE y CAL OK, activar SIN COLISION, `1.INICIAR` y QUIETO20s.
5. `ELEVACION / SOLO DER`: **A** 10 elevaciones a 0,25g (5 lentas + 5 normales). Activar `EXP:SI`, bajar solo DER a **0,15g** (dos pulsaciones de `-`); **B** otras 10 elevaciones. Desactivar EXP (DER debe volver a 0,25g); **A'** otras 10 elevaciones. Anotar cada intento: inmediato/tardío/no detectado/doble.
6. `SOLO IZQ` a 0,25g, 10 elevaciones; luego `BILATERAL / DASH` a UP0,12g, 10 elevaciones; bajar UP bilateral una pulsación a 0,10g, otras 10; regresar UP a 0,12g.
7. QUIETO20s, FINALIZAR y ARCHIVO; adjuntar CSV íntegro original y anotaciones. Opcionalmente video simultáneo pie/pantalla para fase física. **60 gestos humanos en una sola captura**. No declarar un nuevo umbral óptimo solo por el conteo de saltos.
8. En paralelo puede avanzar el primer nivel de videojuego por **otra rama**, validándolo en Play sin esperar a corregir cada detalle del tobillo derecho. Mantener la decisión de detección desacoplada del gameplay.

## 9. Hoja de ruta futura (propuesta, no entregables terminados)

- Cerrar exportación/recuperación y diagnóstico de inestabilidad derecha.
- Validar **caminar y correr en el lugar** con dos tobillos: adquisición IMU, etiquetado de gestos humanos, cadencia, clasificación, sensibilidad/especificidad y latencia.
- Desarrollar primer nivel original 2D inspirado en *endless runner / Geometry Dash* (sin copiar recursos protegidos): avance, obstáculos, colisiones, reinicio, efectos y UI, con teclado/táctil y BLE.
- Incorporar varios niveles, audio original/licenciado, progresión y estadísticas.
- Más adelante integrar muñecas, joystick BLE y VR/TV.

**Metodología:** soluciones prácticas, económicas, progresivas; agrupar cambios compatibles, usar GitHub Actions y Unity Play primero, **mínima cantidad de compilaciones APK de ~20 min**, y una sola sesión física consolidada por hito cuando sea pertinente.

## 10. Reglas obligatorias para cualquier asistente o Astra que modifique el proyecto

1. **ANTES de comenzar:** leer este documento canónico y comparar su vigencia con el estado de ambos repositorios y sus PR.
2. **No tocar `main`, no fusionar PR sin autorización, no reflashear, no borrar NVS, UUID BLE ni archivos locales;** crear rama de trabajo nueva y PR en borrador para código.
3. **CADA VEZ QUE haya un hito material** (cambio de código, nuevo firmware, nueva APK, experimento, hallazgo o cierre de incidencia), **actualizar este mismo documento en la rama `docs/contexto-maestro`**, no una copia aislada en la rama del PR.
4. La actualización debe incluir, de forma concisa y comprobable: fecha, rama/commit/PR, qué cambió, nivel de validación (host/Unity/Android/físico), compatibilidad, nuevos datos experimentales, problemas abiertos, siguiente acción y enlaces a fuentes. **No declarar completado lo que está pendiente de prueba real.**
5. **El commit de documentación es independiente del commit de código:** utilizar GitHub API/herramienta de edición de archivos contra `docs/contexto-maestro`; dejar la URL y commit de contexto en la descripción del PR de código. No publicar un código grande sin actualizar su contexto cuando sea posible; si la herramienta carece de permisos, proporcionar parche Markdown completo y declarar que no se publicó.
6. Si una nueva rama/PR altera lo descrito aquí, **resolver inconsistencia explícitamente**; no duplicar cientos de líneas ni reemplazar historial comprobable por suposiciones. Conservar la separación entre evidencia, hipótesis y tareas.
7. NO ejecutar automatizaciones por sí solas: **este documento no se sincroniza mágicamente**. Debe actualizarlo conscientemente la persona o agente que efectúe el hito.

## 7D. Incidencia Android v0.9.5: sesión larga no exportada

**2026-11-17 — reporte físico del usuario:** tras una prueba integral de aproximadamente **10 minutos**, la aplicación mostraba simultáneamente `Cerrando CSV y RAW...` y en la línea inferior `RAW OFF | CSV guardado SHA256 OK + RAW: ...` con contadores RAW elevados (usuario cita 31.821 / 15.924). Tras aproximadamente **cinco minutos** no vio archivos descargados en Descargas. En una nueva prueba muy corta, la exportación funcionó. **No hay CSV de la sesión larga adjunto ni prueba de su desaparición del almacenamiento privado.** Tampoco debe calificarse la v0.9.5 como exportación fiable para sesiones largas hasta validación Android real.

**Diagnóstico de código verificado en PR #10 (sin modificar producción aún):**
- El mensaje `Cerrando CSV y RAW...` puede quedar **obsoleto** en el rótulo superior (`BiofitDashGame` guarda `experimentStatus` en el instante de FINALIZAR), mientras el rótulo inferior sí consulta `monitor.CaptureFileStatus` actualizado. **No significa necesariamente cierre pendiente**.
- `CSV guardado SHA256 OK` se asigna **después de terminar de escribir/verificar el archivo principal privado**; no confirma exportación a Descargas. `3. ARCHIVO` es proceso independiente, actualmente síncrono.
- `BiofitAndroidExport.SaveToDownloads` valida y copia el archivo entero en memoria, después realiza verificación de contenido Android mediante `java.util.Scanner` y JNI con una cadena completa, todo en llamada síncrona; **hipótesis plausible**, no demostrada, de bloqueo en archivos grandes.
- `BiofitBLEMonitor.ExportGameCapture` elige `SavedCapturePath` activo o, tras reinicio, `FindNewestValid`. Una captura corta posterior puede desplazar la sesión larga como candidata «más reciente», **sin borrar por sí sola sus archivos anteriores**.
- Prioridad: **no repetir los 40 gestos**. No desinstalar, borrar datos, reflashear ni generar sesiones adicionales. Recuperar primero sesiones privadas largas, incluidas parejas RAW, usando futuro historial de archivos y selección por ID; corregir mensajes UI; hacer exportación por bloques no bloqueante con progreso/errores y SHA streaming, más una alternativa Android SAF/FileProvider para rescate. Verificar en Android real con pruebas de tamaños escalonados y fallo simulado.
- Especificación técnica publicada en [comentario de incidencia PR #10](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10#issuecomment-6955479213). Revisión sugerida **v0.9.6 recuperación/exportación**, PR draft separado, sin modificar lógica de juego/sensores hasta validación. Ningún arreglo se ha implementado aún.

## 7E. v0.9.6 — recuperación de archivos y doble exportación (implementación no validada en dispositivo)

**2026-11-18.** Luego de repetidos fallos de exportación de la APK v0.9.5 (incluidos bloques breves), el usuario decidió detener las pruebas físicas. Conserva APK y archivos privados; **no repetir los 40 movimientos antes de intentar recuperar la evidencia**. El documento de anotaciones presentado contiene 8 bloques SYNC 001–008; *no hay CSV comprobados* de esos bloques:

- Derecha 0,25g RAW OFF: dos elevaciones sin salto. Derecha 0,25g RAW ON: elevaciones lentas mayormente sin respuesta y rápidas con respuesta principalmente en el descenso.
- Derecha EXP 0,15g: algunos saltos durante el ascenso pero también respuesta repetida durante ascenso/descenso (5 saltos reportados en 3 elevaciones rápidas). De regreso a DER0,25g predominan respuestas en descenso.
- Izquierda 0,25g: lentas sin salto; rápidas con respuesta en ascenso y descenso.
- Bilateral UP0,12g: lentas sin salto, rápidas en ascenso y descenso. Bilateral UP0,10g: las lentas generan salto en ascenso y las rápidas ascenso + descenso. Control derecho final 0,25g: saltos en descenso.

Estas son **observaciones humanas sin registro RAW** y no demuestran tiempo BLE, sensibilidad clínica, latencia o fase exacta de detección. No modificar detector/umbrales de producción sin datos etiquetados.

**Se creó PR #11** [v0.9.6 recuperación/UI](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/11) rama `fix/export-recovery-ui-v096`, base **PR #10 rama feat/raw-session-v095** (no fusionada). [Informe](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/fix/export-recovery-ui-v096/docs/REVISION_V096_RECUPERACION.md). Implementado:

- File writer privado incremental SHA256, validador streaming y renombrado a CSV después de verificar; `*.partial` preservado.
- Historial de archivos privados v095/v096 y antiguos, pares RAW, integridad y selección de sesión antigua aunque se haya grabado otra nueva.
- **Dos destinos independientes**: MediaStore Descargas en Java worker por bloques + verificación SHA; selector Android SAF `ACTION_CREATE_DOCUMENT` para cada archivo, también SHA por bloques. No se borra el origen privado ante fallo.
- Interfaz con RAW y EXP adyacentes, estado prominente GRABANDO/FINALIZANDO/GUARDADO/EXPORTANDO, botones SESIONES y RESCATE.
- Sin cambios de firmware v2.2.7, NVS, calibración, UUID, detector, valores 0,25/0,25/0,12g y 180ms. Nuevas sesiones con prefijo BIOFIT_v096_; las antiguas no se migran.

**Nivel de validación:** código y pruebas host/Android-Java SDK en CI en curso; **no** compilación Unity, APK o prueba física. Aun con CI verde, inspeccionar cambios locales en GitHub Desktop, abrir rama v096 Unity 6.3, verificar Console y botones, generar solo una APK con mismo paquete/firma e instalar como actualización sin borrar datos. **Primer objetivo Android: recuperar sesión larga del historial y exportar principal/RAW por ambos destinos**, luego una prueba corta de humo (no 40 elevaciones).

## 7F. Recuperación parcial v0.9.5 — auditoría de dos CSV originales

**2026-11-18.** El usuario instaló v0.9.6 y recuperó dos CSV privados que adjuntó para contrastar con su PDF de ocho bloques SYNC 001–008. Ambos archivos están **íntegros según footer SHA256, recuentos y 15 columnas**, pero **ninguno corresponde a la sesión de diez minutos de ocho bloques**.

| CSV | Duración | Filas | EV | SYNC | RAW_ACTIVE |
| --- | ---: | ---: | ---: | --- | --- |
| `BIOFIT_v095_20261117_162940_f00d8d66.csv` | 123,558 s | 891 | 0 | 001 | False en 123 diagnósticos |
| `BIOFIT_v095_20261118_112611_aa0b6103.csv` | 96,671 s | 730 | 18 | 001 y 002 | False en 95 diagnósticos |

El segundo archivo solo registra **18 EV del sensor derecho** (7 MOV_ON, 7 MOV_OFF, 4 UP_IMPULSE) y UP de **0,103, 0,139, 0,185, 0,470 g**. Los tres primeros están por debajo de 0,25 g. No se registran APP de salto, activación EXP ni los ocho SYNC. Ninguno contiene datos RAW y **no se recibieron archivos `_RAW.csv`**. Sus marcas son insuficientes para asignar fases físicas observadas del PDF a los eventos de estos CSV.

**Implicación:** la recuperación selectiva y exportación Android de v0.9.6 funcionó para *al menos dos CSV íntegros*; **NO prueba fiabilidad de sesiones RAW largas, exportación de parejas ni el canal SAF**. Mantener la distinción entre archivos privados verificados y exportación a Descargas. No garantizar guardado de diez minutos sin ensayo real de duración y RAW equivalente. Antes de repetir 40 gestos: comprobar si existe v095 de ~10 minutos con SHA/RAW o archivos .partial; si no, ensayo de carga RAW ON con dos sensores y verificación de **dos** archivos exportados con mismo ID, idealmente primero corto y después ~10 minutos inactivos. Preservar el PDF de observaciones como evidencia cualitativa separada.

## 7G. Validación Android real v0.9.6 — sesión larga RAW de ambos tobillos recuperada

**2026-11-18.** El usuario realizó una grabación de reposo aproximadamente de once minutos con la APK v0.9.6 y exportó desde Android dos archivos originales emparejados (mismo ID de sesión `f3a2f9a9`), posteriormente adjuntos y auditados byte a byte:

| Archivo | Tamaño | Filas de datos | SHA256 y footer |
| --- | ---: | ---: | --- |
| `BIOFIT_v096_20261118_125734_f3a2f9a9.csv` | 2.002.639 B | 18.151 (18.139 DIAG, 4 MARCA, 4 EV, 3 CONFIG, 1 APP) | Válidos; `finalizada=1`, `descartadas_por_limite=0` |
| `BIOFIT_v096_20261118_125734_f3a2f9a9_RAW.csv` | 2.580.971 B | 26.517 RAW (izq 13.269, der 13.248) | Válidos; `finalizada=1`, `descartadas_por_limite=0` |

La sesión principal abarca **684,463 s (11 min 24,5 s)** entre INICIO y último DIAG; el flujo RAW abarca **667,411 s (~11 min 7,4 s)**. Frecuencia media por reloj sensor: **izq 19,878 Hz; der 19,864 Hz**. En ambos sensores la secuencia avanza estrictamente de 1 al último índice, con **0 huecos de seq, 0 duplicados/desorden, 0 inválidos, 0 descartes en callbacks y 0 rechazos de disco**. RAW worker: mediana ~0,16 ms, p99 global ~1,16 ms, máximo ~10,13 ms. La recepción sí muestra jitter (máximo intervalo por pie ~163 ms) sin pérdida observada; **no demuestra latencia BLE absoluta de ~0 ms**. En la captura QUIETO hubo solo cuatro EV MOV_ON/OFF (~t Unity 457 s), **cero UP_IMPULSE**; sus causas físicas no están etiquetadas.

**Conclusión de alcance:** validación física exitosa de **una** sesión prolongada con RAW 20 Hz bilateral, cierre del privado y exportación/recuperación de la pareja completa con hashes correctos. **No** valida estadísticamente tasa de fallos, todas las versiones/dispositivos, el canal alternativo SAF si no fue usado, ni un procedimiento de 40 movimientos, ni reconstrucción del detector ~224 Hz. Mantener respaldo de ambos CSV y no llamarlos datos de elevaciones (son QUIETO). La próxima prueba de movimientos puede realizarse por bloques y verificando al finalizar cada pareja CSV, preservando anotaciones SIN vídeo.

## 7H. v0.9.6.1 — incidencia de RAW OFF y legibilidad (PR #12 borrador)

**18/11/2026.** El usuario mostró screenshot Android v0.9.6 (`EXPORTADO: CSV VERIFICADO`, `RAW: OFF`, `RAW OFF local; STREAM,0 no confirmado: A task was canceled`, `n=0`) y reportó que no logra activar RAW. La imagen corresponde a una sesión anterior ya exportada. **El firmware/streaming no se puede culpar solo por esa imagen**: `ToggleRawCapture` únicamente habilita STREAM,1 si `gameCaptureActive`, `BothTrackersConnected` y links GATT existen; fuera de sesión el tap se ignoraba sin mensaje. Una sesión anterior v0.9.6 **sí** registró 26.517 RAW de ambos tobillos en ~11min; por tanto hay funcionamiento real demostrado, no invariabilidad ante todos los estados de conexión. `A task was canceled` en la captura corresponde a orden **STREAM,0 de apagado** sin confirmación, no prueba fallo de la orden de encendido.

**Se creó PR #12** [v0.9.6.1 RAW/UX](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/12) rama `fix/raw-ux-v0961`, base `fix/export-recovery-ui-v096` (PR #11). Cambios:

- `ToggleRawCapture` indica «primero pulse INICIAR» fuera de sesión, «conectar ambos» si no hay enlaces y «esperar respuesta BLE» durante operación en curso. Botón `RAW: ESPERE` no muestra `RAW: ON` antes de que termine handshake.
- `rawMayBeStreaming` conserva necesidad de stop si se intentó STREAM,1; se limpia únicamente tras confirmación de ambos STREAM,0; evita reenviar STREAM,0 al finalizar o pausar cuando se sabe que ya está apagado. Cualquier error de GATT se conserva como tal, sin falso éxito.
- Panel lateral ensanchado: ancho `min(500,max(410,0.30*width))`; para captura 1536×691 pasa de ~412 a ~493 unidades virtuales y reduce ligeramente el juego. `EXP`/`RAW` contiguos más anchos. Mensajes RAW y exportación separados. Rótulos `INICIAR`, `FINALIZAR`, `ARCHIVO`, `RESCATE` sin prefijos 1./2./3./4. Pantalla marca `v0.9.6.1`.
- Pruebas host de GUI y RAW extendidas: clic sin iniciar, ancho lateral, no enviar STREAM,0 antes de STREAM,1, no duplicar stop tras FINALIZAR.

**No cambia** detector, umbrales 0,25/0,25/0,12 g, 180 ms, firmware v2.2.7, NVS, calibración, formato de CSV ni guardado que fue validado en v096. **No fue compilado/validado Android real aún**, mantener PR draft y no borrar datos locales. El usuario puede probar ahora sin recompilar v096: ambos sensores conectados -> INICIAR -> RAW OFF -> verificar RAW ON y contadores; si funciona, la confusión era el requisito de sesión. Para adoptar GUI v096.1 necesita construir APK e instalar como actualización (package/firma iguales), conservando los 9 archivos de Unity locales. Si RAW sigue OFF dentro de sesión activa, capturar la línea de estado exacta y no deducir automáticamente causa.

## 11. Bitácora breve de actualizaciones del contexto

| Fecha | Cambio | Confirmación |
| --- | --- | --- |
| 2026-11-17 | Creado el contexto maestro estable; incorporado estado Unity v0.9.3 / PR #7, firmware v2.2.7, resultados experimentales, limitaciones y protocolo de recuperación CSV. | Rama documental `docs/contexto-maestro`; referenciado PR #7 y documentos técnicos. |
| 2026-11-17 | Primera exportación Android real v0.9.3 comprobada: CSV v0.8 íntegro (384.042 bytes; 3.066 filas; 351 EV; SHA256 válido); sesión v0.9 de 340 eventos aún no recuperada. | CSV original aportado por usuario; inspección de hash/filas sin reconstrucción. |
| 2026-11-17 | Nueva captura v0.9.3 íntegra (215 EV, 1.970 registros): primeros UP derechos débiles rechazados, laterales fuertes ~0,7s después; tres bilaterales con primer UP derecho inferior a 0,12g. Creado modo EXP A/B/A v0.9.4 sin cambiar defaults, PR #8 y CI host verde. | CSV original del usuario; [análisis](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/feat/diagnostic-threshold-ab-v094/docs/ANALISIS_CSV_20261117_AB.md), [PR #8](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/8). |
| 2026-11-17 | Auditoría de fase: UP al frenar descenso reproducido en host; 236 checks C#; propuesta en sombra y límites de RAW documentados, sin cambios de producción. | [PR #9](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/9), commit `52d61c0`; validación física de fase pendiente. |

| 2026-11-17 | v0.9.5 implementa RAW opcional, archivos completos separados, marcas EXP/SYNC y protocolo único de 40 ciclos. | [PR #10](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10), commit `7fdd72b`; 273 checks host y CI verde; APK y validación física pendientes. |

| 2026-11-17 | Incidencia Android v0.9.5: sesión larga con CSV privado aparentemente verificado, sin exportación observada; sesión breve posterior sí exportada. Identificada UI de cierre obsoleta y riesgo en exportación síncrona y selección solo de la captura más reciente. | [Comentario técnico PR #10](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/10#issuecomment-6955479213), pendientes recuperación e instrumentación. |

| 2026-11-18 | v0.9.6 en PR #11 borrador: historial privado, SHA streaming, MediaStore por bloques, respaldo SAF, GUI RAW+EXP y estado grabación. Observaciones de ocho bloques sin CSV archivadas como cualitativas. | [PR #11](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/11), APK/Android reales pendientes. |

| 2026-11-18 | Android v0.9.6 comprobada en reposo largo con pareja principal/RAW íntegra: 11m24s de sesión, 26.517 RAW, 0 huecos/descartes, SHA256 OK; sin validar SAF alternativo ni fase humana. | CSV originales `BIOFIT_v096_20261118_125734_f3a2f9a9*.csv` adjuntos y auditados. |

---

**Regla de cierre:** no perder la evidencia física para mejorar el código. Conservar firmware y APK funcionales, CSV originales íntegros, decisiones de umbral validadas y escenas locales. La siguiente conversación debe partir del **estado real verificable**, no simplemente de una versión mencionada en un mensaje anterior.
