$ErrorActionPreference = 'Stop'
$runtimeRoot = Join-Path $PSScriptRoot 'runtime'
$sourceRoot = 'D:\Unity\Editors\Godot'
New-Item -ItemType Directory -Path $runtimeRoot -Force | Out-Null
foreach ($name in @('Godot_v4.7.2-stable_win64.exe', 'Godot_v4.7.2-stable_win64_console.exe')) {
    $destination = Join-Path $runtimeRoot $name
    if (-not (Test-Path -LiteralPath $destination)) {
        Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination $destination
    }
}
# Isolate all editor preferences/cache from the original read-only installation.
New-Item -ItemType File -Path (Join-Path $runtimeRoot '_sc_') -Force | Out-Null
Write-Output $runtimeRoot
