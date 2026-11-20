# BIOFIT GAME — GeomeTry ORIGINAL + BILATERAL LEGACY

## Identidad y alcance

- Base BIOFIT Unity v0.9.11: `5616fc4571193d4a2d076550a909089344144384`.
- GeomeTry original: https://github.com/Christ0u/GeomeTry , commit `d922caddb4d21ecb791a266143e279bdb969d42c`.
- Rama nueva independiente: `feat/geometry-original-legacy-bridge`. No depende de PR18 ni PR19.
- **Sin reescritura de los mapas, escenas, prefabs, física ni controladores originales.**
- No se altera firmware ESP32, NVS, UUID, umbrales, calibración, receptor ni los CSV históricos.
- Reutiliza el monitor BLE automático del proyecto BIOFIT, con el selector forzado exclusivamente a LEGACY.

## Arquitectura corregida para Unity 6.3: submódulo aislado y assets idénticos

El repositorio original COMPLETO se conserva sin cambios como submódulo Git en
`ThirdParty/GeomeTryOriginal` (commit `d922caddb4d21ecb791a266143e279bdb969d42c`). No se inserta esa raíz dentro
del directorio `Assets`: contiene `ProjectSettings` y `Packages` propios de
OTRO proyecto y Unity 6 los intentaba importar como assets globales, produciendo
errores de importación `XRSettings.asset` y `ShaderGraphSingleton`.

Solo los archivos de `ThirdParty/GeomeTryOriginal/Assets`, incluyendo todos los
prefabs, mapas, animaciones, scripts, sprites y sus `.meta` GUID originales,
se **copian byte por byte** a `Assets/GeomeTryOriginal/Assets` con el script de
preparación. No se reducen, regeneran ni adaptan mapas ni físicas. El script
verifica SHA-256 de cada uno de los archivos copiados. El árbol fuente queda
íntegro fuera de Assets.

### Preparación obligatoria ANTES de abrir Unity (clon NUEVO)

```powershell
cd "C:\Users\Capo\Documents"
git clone --recurse-submodules --branch feat/geometry-original-legacy-bridge https://github.com/LucianoRodriguezUrquiza/BIOFIT-GAME-UNITY.git BIOFIT-GeomeTry-ORIGINAL-CORREGIDO
cd ".\BIOFIT-GeomeTry-ORIGINAL-CORREGIDO"
powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\prepare_geometry_original.ps1"
```

El mensaje esperado es `OK: copia exacta...`, sin diferencias SHA-256.
Abrí SOLO `BIOFIT-GeomeTry-ORIGINAL-CORREGIDO` como proyecto en Unity Hub.

**No uses `git reset --hard`, `git clean`, `Discard Changes`, ni muevas
manualmente el submódulo.** El clon anterior con errores se conserva intacto
para diagnóstico; no vuelvas a abrirlo hasta decidir su destino. Un ZIP sin
submódulos no contiene la fuente completa.

`Assets/GeomeTryOriginal/Assets/` es un artefacto local de la extracción,
intencionalmente ignorado por Git: la fuente canónica, versionada y verificable
sigue siendo el submódulo externo.

## Exactitud de la jugabilidad

El original sigue ejecutando `Character`, `Cube`, `Ship`, `Wave`,
`Game`, `GameManager`, `CharacterPortal`, `JumpOrb`, `JumpPad`
y los prefabs/animaciones/escenas originales. `Character.Start` mantiene
`Time.timeScale = 1.5`; `Cube` mantiene `jumpForce = 13.5`, velocidad
`6.5` y `gravityScale = 4`. `ProjectSettings/Physics2DSettings.asset` y
`TimeManager.asset` eran idénticos entre BIOFIT y GeomeTry al comparar.
Se importan las etiquetas y capas originales para los prefabs.

El único puente de entrada está en
`Assets/BIOFIT/GeomeTry/BiofitGeomeTryLegacyBridge.cs`:

```
UP_IMPULSE -> BiofitBLEMonitor (filtros/tiempos originales)
           -> BiofitBilateralJumpDetector LEGACY (180 ms, 900 ms)
           -> BilateralJumpCandidate
           -> Cube.onClick() (pulso único)
           -> Cube.Jump() ORIGINAL -> Rigidbody2D ORIGINAL
```

El puente usa el campo protegido `keyPressed` por reflexión sobre el
`Cube` **solo durante un único onClick()**. No mantiene pulsación, ni cola,
ni anticipación de aterrizaje, ni cambia los saltos físicos. Las órdenes
en aire pueden no hacer nada. No se promete que cada gesto produzca salto:
el detector legacy mantiene su cooldown y su ambigüedad biomecánica.
`Ship` y `Wave` siguen con el input clásico de toque mantenido de
GeomeTry; en Android se activa explícitamente la simulación táctil del botón
izquierdo del ratón. No se añade control BLE de vuelo por ahora.

Para que coexistan el input clásico de GeomeTry y el Input System de BIOFIT,
en esta rama únicamente se cambia `activeInputHandler: 1` a `2 (Both)`.
No se reemplazan ni se reconfiguran trackers.

## Interfaz y sesiones

En `Main Menu` y `Level Selection` hay una caja BIOFIT con:
- conexiones 2/2 automáticas, estado de reloj y recuento de órdenes;
- registro INICIAR/FINALIZAR, RAW opcional, SYNC, exportación Downloads/SAF.

No se superpone ningún HUD de BIOFIT sobre `Level`: juego, cámaras,
pistas, colisiones, música, nave, wave, orbes y portales son de GeomeTry.

La conexión BLE permanece en un objeto persistente mientras se cambia entre
escenas originales. Esta arquitectura todavía necesita prueba Android real.
La señal BLE no se interpreta como un salto humano confirmado.

## Construcción local de APK — sin sobrescribir tu instalación

1. **Copia/respaldá toda tu carpeta local** antes de cambiar de rama;
   especialmente tus escenas/configuraciones locales y datos de sesiones.
   Nunca uses discard/reset/clean para resolver conflictos.
2. Creá un clon NUEVO como se detalla arriba, con el submódulo recursivo.
   Antes de Unity, ejecutá `tools/prepare_geometry_original.ps1` para copiar
   solo los Assets del original (incluidos scripts, prefabs y GUID).
3. Unity Hub: abrí **BIOFIT-GeomeTry-ORIGINAL-CORREGIDO** con Editor
   **6000.3.28f1**. Unity importará solamente Assets, no ProjectSettings
   ni Packages anidados. Revisá Console; URP 17.3.0 debe instalarse.
4. Menú **BIOFIT > GeomeTry ORIGINAL > 1. Verificar fuente y mapas**.
   Verifica cuatro JSON con hashes de blob Git, escenas y prefabs.
5. Menú **2. Abrir Main Menu original**. Para ver exactamente los materiales
   originales en Editor, usá **4. Activar URP original para probar en Play**
   (solo en la copia); al terminar podrás usar **5. Restaurar render BIOFIT**.
   Play: probá pantalla táctil, selección, nivel, salto, muerte, nave, wave y
   orbes. Confirmá que no hay errores de referencias, Input/URP ni escenas.
6. File > Build Profiles > Android > Switch Platform. Comprobá el permiso Bluetooth, la firma y el versionCode. El comando de
   compilación aplica temporalmente otro identificador de Android y nombre;
   no altera permanentemente esos valores en el proyecto.
7. Menú **3. Compilar APK Android original + BLE LEGACY**. Si Unity lo completa,
   el archivo aparece en `Builds/BIOFIT_GeomeTry_ORIGINAL_LEGACY_*.apk`.
   El comando selecciona las diez escenas originales **sin sobrescribir
   las listas locales de Build Profiles** y restaura los ajustes temporales.
8. **Esta candidata se compila como una nueva app independiente**
   (`com.biofitgame.geometryoriginal`, nombre `BIOFIT GeomeTry`), no como
   actualización de BIOFIT DASH. El comando modifica temporalmente el
   identificador y el nombre solo durante el build, y después los restaura.
   Así podés tener las dos APK instaladas. Cada aplicación mantiene su propio
   almacenamiento privado y sus propias sesiones; no desinstales BIOFIT DASH.

## Verificaciones y limitaciones

- El código, la extracción segura y el submódulo están propuestos en GitHub para evaluar. La estructura original anterior dentro de Assets produjo errores reales al importar en Unity 6.3, por eso se aisló el submódulo fuera de Assets.
- No se ha ejecutado aquí Unity Editor/Play ni compilación Android:
  **no hay una APK verificada ni debe afirmarse lo contrario**.
- El submódulo guarda la geometría original, no una aproximación.
- Algunos recursos musicales/gráficos del repositorio tercero pueden tener
  licencias diferentes de la MIT del código. Antes de redistribuir una APK
  públicamente, comprobá sus permisos de uso; el submódulo no otorga derechos.
- Cualquier error de importación, renderer URP o entrada en el teléfono deberá
  resolverse sin modificar mapas ni parámetros físicos originales.
- Este cambio no demuestra equivalencia bit a bit de físicas entre Unity
  6000.0.27f1 original y 6000.3.28f1 BIOFIT: se conservan código, prefabs
  y parámetros; falta la prueba en Editor/Android.


## Compatibilidad adicional detectada en Unity 6000.3.28f1

La segunda importacion de la copia corregida revelo tres archivos que el upstream
denomina "*.png", aunque internamente tienen formato RIFF/WEBP: 
\`Wave001.png\`, \`Ship.png\` y \`smallTile.png\`. Se decodificaron
en PNG genuino y se comprobo igualdad exacta del contenido RGBA pixel por pixel.
Se conservan sus nombres originales y archivos .meta/GUID: no se cambian
aspecto, posicion, niveles, colliders ni fisica.

Unity 6 tambien advierte que \`Camera\` coincide con una clase incorporada.
La reparacion renombra la clase MonoBehaviour de camara a
\`GeometryCamera\` SOLO en la copia importada, moviendo su \`.meta\` sin
alterar GUID, y cambia cuatro referencias tipadas al nombre nuevo. El
seguimiento de la camara conserva el algoritmo original.

La carpeta \`ThirdParty/GeomeTryOriginal\` no se toca.
La preparacion de un clon nuevo aplica el parche automaticamente despues de
copiar y comprobar los 469 archivos originales. Para una carpeta ya preparada,
**cerrar Unity** y, dentro de \`BIOFIT-GeomeTry-ORIGINAL-CORREGIDO\`, ejecutar:

\`\`\`powershell
git pull --ff-only origin feat/geometry-original-legacy-bridge
powershell -NoProfile -ExecutionPolicy Bypass -File ".\tools\apply_geometry_compat.ps1"
\`\`\`

El script es idempotente, compara archivos antes de reemplazar y rechaza
diferencias imprevistas en lugar de borrar cambios locales. No borres la
carpeta \`Library\`, no hagas reset, clean ni descartes modificaciones.
Volver a abrir Unity y verificar Console: las 3 imagenes deben importarse.
Si quedan errores de compilacion o prefabs, reportar la primera linea roja.


## Compatibilidad Windows: verificación de mapas CRLF (2026-11-20)

En Unity 6000.3.28f1 para Windows, el menú `1. Verificar fuente y mapas` devolvía
`Mapa original modificado: ... StereoMadness.json` aun cuando el mapa estaba
copiado desde el submódulo original. El verificador de editor comparaba el SHA-1
Git de los **bytes del checkout** sin contemplar que Git para Windows puede
convertir los saltos de línea LF del repositorio a CRLF en el disco.

Se corrigió **solo el verificador**: acepta el SHA-1 exacto del Git blob
original o el mismo SHA-1 tras convertir únicamente `CRLF -> LF` en memoria.
Si los bytes difieren por cualquier otra razón, el comando seguirá deteniéndose
con un error explícito y valores de hash. No modifica ni regraba los JSON, ni
relaja la comprobación de su contenido. Los tests host validan el caso LF/CRLF.

Para aplicar la corrección a `BIOFIT-GeomeTry-ORIGINAL-CORREGIDO`:
**cerrar Unity** y ejecutar `git pull --ff-only origin feat/geometry-original-legacy-bridge`
desde esa carpeta, sin borrar `Library` ni archivos locales; reabrir Unity y
repetir `BIOFIT > GeomeTry ORIGINAL > 1. Verificar fuente y mapas`.


## Navegación del menú original en Play Mode — diagnóstico y solución (20/11/2026)

**Causa comprobada del botón Play que anima pero no navega:** el botón original
está enlazado en `Main Menu.unity` a
`UIManager.OnClickSwitchSceneButton("Level Selection")`. Ese método invoca
`GameManager.LoadScene`, que comprueba `SceneExists` **solo en las escenas
habilitadas en Unity Build Settings**. La rama BIOFIT v0.9.11 contenía
únicamente `Assets/Scenes/SampleScene.unity` en esa lista, por lo que
`GameManager` descartaba el cambio de escena sin informar del fallo.

**Corrección publicada:** `ProjectSettings/EditorBuildSettings.asset` ahora
registra las diez escenas originales de GeomeTry en el orden del proyecto
original y con sus GUID originales. `Main Menu` ocupa el índice 0;
`SampleScene` sigue existiendo, pero queda deshabilitada en la lista.
El código de navegación de GeomeTry, los JSON de niveles y toda la física
se conservan literalmente sin cambios.

**Auditorías preventivas nuevas:**
- `BIOFIT > GeomeTry ORIGINAL > 1. Verificar fuente y mapas` comprueba la
  Scene List activa, las cinco fuentes JSON originales (los cuatro prioritarios
  y `WaveVibe`), su parseo con el parser original, música, prefabs, sprites
  y los hashes de cuatro mapas fijados.
- Al entrar a Play en el menú, el puente BIOFIT advierte explícitamente
  si falta alguna escena indispensable de navegación.
- El CI ejecuta `tests/check_geometry_navigation.py` para garantizar que
  las transiciones declaradas en las escenas originales coincidan con las
  escenas registradas y que todos los tipos de objeto de los mapas tengan
  los prefabs originales necesarios.
- El panel de estado BLE inicia compacto en el borde superior derecho para
  no tapar título o botones originales. `ESTADO / REGISTRO` lo expande.

**Aplicar en la instalación ya creada:**
1. Salir de Play y **cerrar Unity completamente**.
2. En PowerShell:
   `cd "C:\Users\Capo\Documents\BIOFIT-GeomeTry-ORIGINAL-CORREGIDO"`
3. Ejecutar:
   `git pull --ff-only origin feat/geometry-original-legacy-bridge`
4. Reabrir el mismo proyecto Unity, ejecutar `1. Verificar fuente y mapas`
   y probar `Play` > botón grande central > `Level Selection`.
5. Si algún paso falla, copiar el **primer error real** de Console. No usar
   `reset`, `clean`, `restore` o descarte de cambios.

No hace falta volver a clonar, reimportar todos los assets ni ejecutar otra
vez los scripts de conversión. El firmware y la selección del canal BLE
**BILATERAL LEGACY** siguen intactos. Los checks host no garantizan todavía
el funcionamiento del juego bajo Unity/Android real.


## Entrada BLE en orbes originales y ruta de selección (20/11/2026)

La auditoría de `Polargeist` y `JumpyNightmare` identificó que el código
original `JumpOrb.OnTriggerStay2D` solo aceptaba `Input.GetKey(Space)` o
`Input.GetMouseButton(0)`. Llamar exclusivamente `Cube.onClick()` no
activaba los orbes mediante las Waveshare.

El puente mantiene **BILATERAL LEGACY**, sin tocar umbrales/ventanas/cooldowns,
y añade un pulso opcional de **un solo consumo dentro del siguiente paso
físico** para los orbes. Un parche limitado a la **copia importada** de
`JumpOrb.cs` amplía únicamente su condición de entrada: si el collider es un
`Cube`, puede consumir ese pulso BLE. El valor original de los saltos de
orbes (12,5 / 20 / 27) y el resto de `OnTriggerStay2D` permanecen intactos.
Si llega sin orb bajo contacto, el pulso caduca. No se implementa una cola
ni se mantienen pulsaciones artificiales.

Asimismo se corrige la capitalización de la ruta del controlador
`LevelSelectionManager` en la copia importada de
`"Maps/Base"` a `"maps/BASE"`, que coincide con el directorio existente;
los archivos JSON siguen idénticos. El submódulo original completo permanece
prístino e inalterado. La nave y la wave conservan su entrada táctil original;
**no se afirma que puedan pilotarse con BLE**.

**Importante para la copia ya existente:** tras cerrar Unity y hacer
`git pull --ff-only origin feat/geometry-original-legacy-bridge`, ejecutar
también `powershell -NoProfile -ExecutionPolicy Bypass -File
".\\tools\\apply_geometry_compat.ps1"` desde
`BIOFIT-GeomeTry-ORIGINAL-CORREGIDO`. El script vuelve a comprobar las
correcciones ya aplicadas (PNG, Camera), agrega solo las dos compatibilidades
nuevas y rechaza sobrescribir diferencias locales inesperadas.

El verificador de Unity ahora informa si falta el parche del orbe.
El CI verifica la existencia del puente y la conservación del archivo
`JumpOrb.cs` original. La interacción real con orbes sigue pendiente de
Play Mode en Unity y de una APK física Android.
