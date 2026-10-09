# Reproduksi lane Agent 4; hanya dependency existing dan output evidence yang disanitasi.
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:TESTINGPLATFORM_TELEMETRY_OPTOUT = '1'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
Push-Location $repo
try {
    $project = 'tests/Unit/OfflineFoundation/Pertamina.Oto.OfflineFoundation.Tests.csproj'
    $records = [System.Collections.Generic.List[string]]::new()
    $records.Add('Run P01-20260905-1319 / P1-A4-OFFLINE-PORTS')
    $records.Add('Baseline d497ec68ea8fa3e21ca311a580a44111045cfb20')
    $records.Add('Source commit: ' + (& git rev-parse HEAD))
    foreach ($arguments in @(
        @('restore', $project, '--locked-mode', '--ignore-failed-sources'),
        @('build', $project, '-c', 'Release', '--no-restore'),
        @('run', '--project', $project, '-c', 'Release', '--no-build')
    )) {
        $records.Add('COMMAND: dotnet ' + ($arguments -join ' '))
        $output = & dotnet @arguments 2>&1
        $result = $LASTEXITCODE
        foreach ($line in $output) { $records.Add($line.ToString().Replace($repo, '<WORKTREE>')) }
        $records.Add('EXIT: ' + $result)
        if ($result -ne 0) { throw 'Verification failed.' }
    }
    & git diff --check
    if ($LASTEXITCODE -ne 0) { throw 'Whitespace check failed.' }
    $records.Add('git diff --check EXIT: 0')
    $records | Set-Content (Join-Path $PSScriptRoot 'VERIFICATION.txt')
    Write-Output 'Locked restore/build/test/diff checks passed; sanitized VERIFICATION.txt written.'
} finally { Pop-Location }
