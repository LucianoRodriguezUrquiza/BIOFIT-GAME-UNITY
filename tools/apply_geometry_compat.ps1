# BIOFIT GeomeTry ORIGINAL - isolated compatibility patch.
# Run only while UNITY IS CLOSED, inside the dedicated corrected clone.
# Original source lives in ThirdParty/GeomeTryOriginal and is NEVER modified.
# Physics, map JSON, game parameters, sprites' pixels and prefab GUIDs remain intact.
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$original = Join-Path $root "ThirdParty\GeomeTryOriginal\Assets"
$imported = Join-Path $root "Assets\GeomeTryOriginal\Assets"
$overlays = Join-Path $PSScriptRoot "geometry_compat"

if (!(Test-Path -LiteralPath $original -PathType Container)) {
    throw "Falta submodulo original. No se toca ninguna carpeta."
}
if (!(Test-Path -LiteralPath $imported -PathType Container)) {
    throw "Falta copia de Assets. Ejecuta primero tools/prepare_geometry_original.ps1."
}
$sourceCommit = (& git -C (Join-Path $root "ThirdParty\GeomeTryOriginal") rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -ne "d922caddb4d21ecb791a266143e279bdb969d42c") {
    throw "El submodulo original no esta en el commit verificado. No se realizan cambios."
}
function Same-Bytes([string]$a, [string]$b) {
    return ((Get-FileHash -LiteralPath $a -Algorithm SHA256).Hash -eq
            (Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash)
}

# The three upstream *.png blobs are actually RIFF/WEBP, which Unity's PNG
# importer cannot decode. These overlays are pixel-identical PNG transcodes.
# Keep the original path and .meta GUID; replace only contents of copied files.
$images = @(
    @{ Original="Resources\Sprites\Character\Wave\Wave001.png"; Overlay="Wave001.png" },
    @{ Original="Resources\Sprites\Character\Ship\Ship.png"; Overlay="Ship.png" },
    @{ Original="Resources\Sprites\Obstacles\Tile\smallTile.png"; Overlay="smallTile.png" }
)
foreach ($img in $images) {
    $source = Join-Path $original $img.Original
    $dest = Join-Path $imported $img.Original
    $fixed = Join-Path $overlays $img.Overlay
    if (!(Test-Path -LiteralPath $source -PathType Leaf) -or
        !(Test-Path -LiteralPath $dest -PathType Leaf) -or
        !(Test-Path -LiteralPath $fixed -PathType Leaf)) {
        throw "Falta imagen original, importada o corregida: $($img.Original)"
    }
    $pngSignature = [byte[]](137,80,78,71,13,10,26,10)
    $fixedBytes = [IO.File]::ReadAllBytes($fixed)
    if ($fixedBytes.Length -lt 8) { throw "Overlay vacio: $fixed" }
    for ($i=0; $i -lt 8; $i++) {
        if ($fixedBytes[$i] -ne $pngSignature[$i]) { throw "Overlay NO es PNG: $fixed" }
    }
    if (Same-Bytes $dest $fixed) {
        Write-Host "OK: PNG ya reparado: $($img.Overlay)"
    } elseif (Same-Bytes $dest $source) {
        Copy-Item -LiteralPath $fixed -Destination $dest -Force
        if (!(Same-Bytes $dest $fixed)) { throw "No se pudo validar PNG: $dest" }
        Write-Host "OK: PNG real, mismos pixeles: $($img.Overlay)"
    } else {
        throw "Imagen con modificaciones locales inesperadas; no se sobrescribe: $dest"
    }
    # The *.png.meta retains the EXACT upstream asset GUID.
    if (!(Test-Path -LiteralPath "$dest.meta" -PathType Leaf)) {
        throw "Falta la metadata/GUID original: $dest.meta"
    }
}

# Unity 6 rejects a MonoBehaviour class named Camera (built-in component).
# Rename ONLY the copied upstream Camera.cs and its .meta together to preserve
# the original GUID in all serialized scenes and prefabs.
$scriptDir = Join-Path $imported "Scripts"
$oldCamera = Join-Path $scriptDir "Camera.cs"
$newCamera = Join-Path $scriptDir "GeometryCamera.cs"
if (Test-Path -LiteralPath $newCamera) {
    if (Test-Path -LiteralPath $oldCamera) {
        throw "Camera.cs y GeometryCamera.cs estan presentes simultaneamente. No se toca."
    }
    $content = [IO.File]::ReadAllText($newCamera)
    if (!$content.Contains("public class GeometryCamera : MonoBehaviour")) {
        throw "GeometryCamera.cs tiene contenido inesperado"
    }
    if (!(Test-Path -LiteralPath "$newCamera.meta")) {
        throw "Falta GUID de GeometryCamera.cs"
    }
} else {
    if (!(Test-Path -LiteralPath $oldCamera) -or
        !(Test-Path -LiteralPath "$oldCamera.meta")) {
        throw "Falta Camera.cs o su .meta original"
    }
    if (!(Same-Bytes $oldCamera (Join-Path $original "Scripts\Camera.cs"))) {
        throw "Camera.cs tiene modificaciones locales. No se sustituye."
    }
    Move-Item -LiteralPath $oldCamera -Destination $newCamera
    Move-Item -LiteralPath "$oldCamera.meta" -Destination "$newCamera.meta"
    $content = [IO.File]::ReadAllText($newCamera)
    if (!$content.Contains("public class Camera : MonoBehaviour")) {
        throw "No se encuentra la clase de camara original"
    }
    $content = $content.Replace("public class Camera : MonoBehaviour",
                                "public class GeometryCamera : MonoBehaviour")
    [IO.File]::WriteAllText($newCamera, $content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "OK: GeometryCamera; GUID original conservado."
}

# Four upstream scripts reference the *custom* Camera class. Only identifiers
# in these known source files change; motion, gravity, levels and timing do not.
$references = @(
    "Scripts\Game.cs",
    "Scripts\EndZone.cs",
    "Scripts\GameMechanics\CharacterType\Character.cs",
    "Scripts\GameMechanics\Portal.cs"
)
foreach ($relative in $references) {
    $dest = Join-Path $imported $relative
    $source = Join-Path $original $relative
    if (!(Test-Path -LiteralPath $dest -PathType Leaf)) { throw "Falta: $dest" }
    $content = [IO.File]::ReadAllText($dest)
    if ($content.Contains("GeometryCamera")) {
        if ($content.Contains("FindFirstObjectByType<Camera>") -or
            $content.Contains("GetComponent<Camera>")) {
            throw "Referencia de camara a medio migrar: $dest"
        }
        Write-Host "OK: referencias ya corregidas: $relative"
        continue
    }
    if (!(Same-Bytes $dest $source)) {
        throw "Script con cambios locales imprevistos; no se sobrescribe: $dest"
    }
    $changed = [System.Text.RegularExpressions.Regex]::Replace(
        $content, '(?<![A-Za-z0-9_])Camera(?![A-Za-z0-9_])', 'GeometryCamera')
    if ($changed -eq $content) { throw "Falta referencia esperada Camera en $dest" }
    [IO.File]::WriteAllText($dest, $changed, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "OK: referencias de camara: $relative"
}
# Keep ALL original jump-orb forces, triggering physics and prefabs. Only
# expand the original press condition to accept ONE legacy BLE pulse.
# Do not edit the pinned source submodule or the *.cs.meta GUID.
$orbSource = Join-Path $original "Scripts\JumpOrb.cs"
$orbDest = Join-Path $imported "Scripts\JumpOrb.cs"
if (!(Test-Path -LiteralPath $orbSource -PathType Leaf) -or
    !(Test-Path -LiteralPath $orbDest -PathType Leaf)) {
    throw "Falta JumpOrb.cs original/importado, no se alteran los mapas."
}
$oldInput = 'Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)'
$newInput = $oldInput + ' ||' + [Environment]::NewLine +
    '            (other.GetComponent<Cube>() != null &&' + [Environment]::NewLine +
    '             Biofit.GeomeTryOriginal.BiofitGeomeTryLegacyBridge.TryConsumeOrbPulse())'
$orbCode = [IO.File]::ReadAllText($orbDest)
if ($orbCode.Contains('TryConsumeOrbPulse()')) {
    Write-Host "OK: entrada BLE de JumpOrb ya integrada."
} else {
    if (!(Same-Bytes $orbSource $orbDest)) {
        throw "JumpOrb.cs contiene cambios locales inesperados; no se sobrescribe."
    }
    if (!$orbCode.Contains($oldInput)) {
        throw "No se encontro la entrada original de JumpOrb; revisar version."
    }
    $patchedOrb = $orbCode.Replace($oldInput, $newInput)
    [IO.File]::WriteAllText($orbDest, $patchedOrb,
        (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "OK: JumpOrb ahora admite un pulso BLE sin cambiar la fuerza fisica."
}
if (!(Test-Path -LiteralPath "$orbDest.meta" -PathType Leaf)) {
    throw "Falta el .meta/GUID original de JumpOrb."
}

# The upstream level-selection Resources path uses different capitalization
# from its actual "Resources/maps/BASE" directory. Normalize only the string
# in the *imported* controller, so Editor and Android resolve identically.
$selectSource = Join-Path $original "Scripts\LevelSelectionManager.cs"
$selectDest = Join-Path $imported "Scripts\LevelSelectionManager.cs"
$before = 'Resources.LoadAll<TextAsset>("Maps/Base")'
$after  = 'Resources.LoadAll<TextAsset>("maps/BASE")'
if (!(Test-Path -LiteralPath $selectSource -PathType Leaf) -or
    !(Test-Path -LiteralPath $selectDest -PathType Leaf)) {
    throw "Falta script original de seleccion de niveles."
}
$selectCode = [IO.File]::ReadAllText($selectDest)
if ($selectCode.Contains($after)) {
    Write-Host "OK: ruta de mapas de seleccion ya normalizada."
} else {
    if (!(Same-Bytes $selectSource $selectDest) -or
        !$selectCode.Contains($before)) {
        throw "LevelSelectionManager.cs tiene modificaciones no previstas."
    }
    [IO.File]::WriteAllText($selectDest, $selectCode.Replace($before,$after),
        (New-Object System.Text.UTF8Encoding($false)))
    Write-Host "OK: seleccion de mapas usa Resources/maps/BASE (sin alterar mapas)."
}

Write-Host "OK: reparacion visual y de camara completada. Fuente original sin modificar."
Write-Host "Ya podes abrir Unity 6000.3.28f1 y comprobar la consola."
