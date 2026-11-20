# BIOFIT GeomeTry ORIGINAL — safe byte-for-byte extraction of upstream Assets.
# Run only in a new dedicated clone while Unity is CLOSED.
# Does not modify source submodule, other projects, maps or firmware.
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$module = Join-Path $root "ThirdParty\GeomeTryOriginal"
$source = Join-Path $module "Assets"
$targetParent = Join-Path $root "Assets\GeomeTryOriginal"
$target = Join-Path $targetParent "Assets"
$expectedCommit = "d922caddb4d21ecb791a266143e279bdb969d42c"

if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw "Falta el submodulo original. Ejecuta desde la carpeta nueva: git submodule update --init --recursive"
}
$gitSha = (& git -C $module rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $gitSha -ne $expectedCommit) {
    throw "Commit original inesperado: $gitSha. Esperado: $expectedCommit"
}
if (Test-Path -LiteralPath $target) {
    throw "La carpeta destino YA EXISTE: $target. No se sobrescribe ni se borran assets. Usa un clon nuevo para ejecutar la preparacion."
}

New-Item -ItemType Directory -Path $targetParent -Force | Out-Null
Copy-Item -LiteralPath $source -Destination $targetParent -Recurse -Force -ErrorAction Stop

$files = Get-ChildItem -LiteralPath $source -File -Recurse -Force
foreach ($f in $files) {
    $relative = $f.FullName.Substring($source.Length).TrimStart([char]'\', [char]'/')
    $copy = Join-Path $target $relative
    if (-not (Test-Path -LiteralPath $copy -PathType Leaf)) {
        throw "No se copio el archivo original: $relative"
    }
    $hash1 = (Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash
    $hash2 = (Get-FileHash -LiteralPath $copy -Algorithm SHA256).Hash
    if ($hash1 -ne $hash2) {
        throw "La copia es distinta del original: $relative"
    }
}
if (Test-Path -LiteralPath (Join-Path $targetParent "ProjectSettings")) {
    throw "No debe haber ProjectSettings dentro de Assets/GeomeTryOriginal"
}
# Apply only checked Unity import compatibility overlays after SHA-256 source verification.
& (Join-Path $PSScriptRoot "apply_geometry_compat.ps1")

Write-Host "OK: copia exacta de $($files.Count) archivos originales en Assets/GeomeTryOriginal/Assets"
Write-Host "OK: ProjectSettings y Packages de GeomeTry quedan FUERA de Unity, sin importarse."
Write-Host "OK: originales intactos en ThirdParty/GeomeTryOriginal."
Write-Host "Ahora abri esta carpeta como proyecto principal en Unity Hub: $root"
