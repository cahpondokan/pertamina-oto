param([Parameter(Mandatory)][string]$Application, [Parameter(Mandatory)][string]$EditorFixture, [Parameter(Mandatory)][string]$Evidence)
$ErrorActionPreference = 'Stop'
$Application = (Resolve-Path -LiteralPath $Application).Path
$EditorFixture = (Resolve-Path -LiteralPath $EditorFixture).Path
$Evidence = [IO.Path]::GetFullPath($Evidence)
New-Item -ItemType Directory -Path $Evidence -Force | Out-Null
$results = @()
foreach ($case in @(
    @{Name='reader';Args=@('--audit-reader-ui');Pattern='reader-ui-audit-*';Minimum=400},
    @{Name='zoom';Args=@('--audit-zoom-smoothness');Pattern='zoom-smoothness';Minimum=50},
    @{Name='editor';Args=@('--audit-live-editor',$EditorFixture);Pattern='live-editor-audit-*';Minimum=140}
)) {
    $started = Get-Date
    $start = [Diagnostics.ProcessStartInfo]::new((Join-Path $Application 'Pertamina.Oto.Desktop.App.exe'))
    $start.UseShellExecute = $false
    $start.WorkingDirectory = $Application
    foreach ($arg in $case.Args) { $start.ArgumentList.Add($arg) }
    $process = [Diagnostics.Process]::Start($start)
    try {
        if (!$process.WaitForExit(180000)) { throw "Native audit timeout: $($case.Name); PID $($process.Id)" }
        if ($process.ExitCode -ne 0) { throw "Native audit failed: $($case.Name) / $($process.ExitCode)" }
    } finally { $process.Dispose() }
    $folder = Get-ChildItem -LiteralPath $Application -Directory -Filter $case.Pattern | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    $report = Get-Item -LiteralPath (Join-Path $folder.FullName 'audit.txt')
    $lines = Get-Content -LiteralPath $report.FullName
    $passes = @($lines | Where-Object { $_ -like 'PASS *' }).Count
    if ($report.LastWriteTime -lt $started -or $passes -lt $case.Minimum -or @($lines | Where-Object { $_ -match '^(FAIL|ERROR)\b' }).Count) { throw "Missing, stale or failing native evidence: $($case.Name)" }
    $results += [pscustomobject]@{Name=$case.Name;Pass=$passes;ExitCode=0;Report=$report.FullName;AssemblySha256=(Get-FileHash "$Application/Pertamina.Oto.Desktop.App.dll").Hash}
    $results | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath "$Evidence/native-final.json"
    "PASS native $($case.Name): $passes checks"
}
