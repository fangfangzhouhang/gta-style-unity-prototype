param(
    [ValidateSet('Run','Editor','Check','Route','TimerRoute')][string]$Mode = 'Run',
    [string]$Godot = $env:GODOT_BIN
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
if (-not $Godot) {
    $bundled = Join-Path $PSScriptRoot 'runtime/Godot_v4.7.2-stable_win64_console.exe'
    if (Test-Path -LiteralPath $bundled) { $Godot = $bundled }
    else {
        $command = Get-Command godot, godot4 -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($command) { $Godot = $command.Source }
    }
}
if (-not $Godot -or -not (Test-Path -LiteralPath $Godot -PathType Leaf)) {
    throw 'Install Godot 4.7.2 standard, then supply -Godot <absolute executable path> or set GODOT_BIN.'
}
$version = (& $Godot --version | Out-String).Trim()
if ($version -notmatch '^4\.7\.2\.') { throw "Expected tested Godot 4.7.2, got: $version" }
$testing = $Mode -in @('Check','Route','TimerRoute')
$profile = Join-Path $projectRoot $(if ($testing) { 'test_userdata' } else { 'local_data' })
New-Item -ItemType Directory -Path $profile -Force | Out-Null
New-Item -ItemType File -Path (Join-Path $profile '.gdignore') -Force | Out-Null
$logs = Join-Path $projectRoot 'evidence/logs'
New-Item -ItemType Directory -Path $logs -Force | Out-Null
$oldAppData = $env:APPDATA
$oldXdgData = $env:XDG_DATA_HOME
try {
    $env:APPDATA = $profile
    $env:XDG_DATA_HOME = $profile
    $engineArgs = @('--path', $projectRoot, '--log-file', (Join-Path $logs ('launch-' + $Mode.ToLowerInvariant() + '.log')))
    if ($Mode -eq 'Editor') { $engineArgs += '--editor' }
    if ($testing) {
        # Fresh clones must import fonts/audio and discover script classes before tests.
        & $Godot --headless --editor --path $projectRoot --import
        if ($LASTEXITCODE -ne 0) { throw 'Godot import failed.' }
        $script = if ($Mode -eq 'Check') { 'res://tests/acceptance.gd' } else { 'res://tests/route_trial.gd' }
        $engineArgs += @('--headless', '--script', $script, '--', '--test')
        if ($Mode -eq 'TimerRoute') { $engineArgs += '--timer-route' }
    }
    & $Godot @engineArgs
    $result = $LASTEXITCODE
} finally {
    $env:APPDATA = $oldAppData
    $env:XDG_DATA_HOME = $oldXdgData
}
exit $result
