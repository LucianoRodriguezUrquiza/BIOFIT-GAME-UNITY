# BIOFIT GAME — CONTEXTO MAESTRO

> **Punto de entrada y memoria técnica versionada del proyecto.** Leer esta página **antes** de modificar firmware, Unity, Android, pruebas o documentación.
>
> **Última revisión manual:** 2026-11-17 · **Estado técnico de referencia:** Unity **v0.9.3** (PR #7, **borrador**, CI host verde; **APK NO validada**), firmware instalado **v2.2.7** en ambos tobillos.
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
| Unity v0.9.3 | `fix/csv-resource-close-audit-v093`, commit `4f58562f757fc608d0951127afe995670d300878`, [PR #7](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/pull/7) | **Borrador**, CI host verde, Android físico pendiente |
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

**Cadena de cambios (todavía no probada Android real):**
- **v0.9.1 / PR #5:** intentó corregir estructura del plugin Java y recuperar archivos privados válidos entre reinicios.
- **v0.9.2 / PR #6:** cambió la ruta activa de exportación a `MediaStore.Downloads` desde Unity/C#, **sin selector ni COPIAR**, y conservó recuperación del último CSV privado válido.
- **v0.9.3 / PR #7:** Astra auditó v0.9.2 y corrigió cierre explícito de streams/descriptores Java (no basta liberar JNI), comparación completa del contenido y SHA256 del destino, `IS_PENDING` durante escritura y una sola instantánea del archivo fuente. Las comprobaciones Android reales siguen pendientes.

**Comportamiento previsto en futura APK v0.9.3:** `FINALIZAR` conserva CSV privado con conteos y footer SHA256; presionar `ARCHIVO` guarda automáticamente en **Descargas** sin selector; mensaje de éxito solo tras verificar y publicar. En una instalación actualizada con la **misma firma e ID de paquete**, `ARCHIVO` puede recuperar la sesión previa **sin INICIAR**. Android 10+; no borrar datos ni desinstalar la versión anterior.

**Verificado por Astra en v0.9.3:** pruebas host C# 207 comprobaciones; test de contrato; firmware contrato y replay existentes (90 gestos / 120 s simulados); CI Unity verde. **No se ejecutó ni confirmó** compilación de APK Unity, Gradle/puente JNI en Android real ni recuperación física de datos. Una Activity Java heredada puede compilar en CI sin participar en el exportador actual. No declarar éxito físico hasta probar en celular.

Detalles y fuentes: [Auditoría v0.9.2, cambios v0.9.3](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/fix/csv-resource-close-audit-v093/docs/AUDITORIA_V092.md), [protocolo anterior v0.9.2](https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY/blob/fix/automatic-download-csv-v092/docs/REVISION_V092.md).

## 8. Próximo procedimiento único, priorizado

**NO compilar ahora varias APK, NO repetir los 60 movimientos, NO tocar firmware todavía.**

1. **Antes de cambiar rama en GitHub Desktop:** cerrar Unity; revisar cambios locales pendientes en `BIOFIT-GAME-UNITY`. Conservar archivos locales de escenas y configuración. Al seleccionar `fix/csv-resource-close-audit-v093`, si GitHub Desktop ofrece `Bring my changes` sin conflictos, traer cambios. Si hay conflictos, detenerse y examinarlos. **Stash: NO por defecto; Squash: NO; Discard: NO; Commit: NO salvo decisión explícita.**
2. **Unity Play:** verificar `BiofitDash`, consola sin errores, controles 0,25/0,25/0,12 g, PAR=180 ms, panel y salto manual. Comprobar escenas Android.
3. **Una única APK** desde la revisión v0.9.3, preservando package ID y firma. Si Android exige **desinstalar primero**, DETENERSE, para no perder CSV privado de la sesión anterior.
4. **Primera acción en la nueva APK:** abrir BIOFIT DASH y pulsar `3. ARCHIVO` **sin `1. INICIAR`**; revisar mensaje real y archivo en **Descargas**. Conservar CSV original sin PDF/Google Docs. Si sale error, copiar el mensaje completo; no borrar datos.
5. **Analizar el CSV previo recuperado**, especialmente los ~10 fallos derechos finales: alinear `EV`, `CAL/ARM`, restCount, bias/gravedad, `MOV_ON/OFF`, UP, valores de umbral y decisiones de Unity. Identificar lo demostrable.
6. Si no se recupera o hace falta experimento adicional, **única microprueba posterior**: quietud 15 s; 10 derechas, 10 izquierdas y 5 bilaterales, espaciadas ~2 s; quietud 10 s; FINALIZAR → ARCHIVO. Anotar intentos físicos, inmediatos/tardíos/no detectados/dobles, especialmente los primeros y los últimos.

**Criterio para pasar de etapa:** guardar CSV en Descargas confirmado **en el celular** y control derecho caracterizado con suficiente evidencia. El desarrollo de un nivel 2D jugable puede empezar en una rama independiente, pero todavía no declarar confiabilidad clínica o tasa física superior al 95%.

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

## 11. Bitácora breve de actualizaciones del contexto

| Fecha | Cambio | Confirmación |
| --- | --- | --- |
| 2026-11-17 | Creado el contexto maestro estable; incorporado estado Unity v0.9.3 / PR #7, firmware v2.2.7, resultados experimentales, limitaciones y protocolo de recuperación CSV. | Rama documental `docs/contexto-maestro`; referenciado PR #7 y documentos técnicos. |

---

**Regla de cierre:** no perder la evidencia física para mejorar el código. Conservar firmware y APK funcionales, CSV originales íntegros, decisiones de umbral validadas y escenas locales. La siguiente conversación debe partir del **estado real verificable**, no simplemente de una versión mencionada en un mensaje anterior.
