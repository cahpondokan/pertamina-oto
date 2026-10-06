# Verification only. Does not publish feeds, install launchers, or touch user instances.
param([string]$MSBuild = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$evidence = Join-Path $projectRoot ('artifacts/rv-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
$output = Join-Path $evidence 'app'
New-Item -ItemType Directory -Path $evidence | Out-Null
$report = [Collections.Generic.List[string]]::new()
function NativeAudit([string]$argument,[string]$resultFile,[int]$minimum) {
    $auditProcess = Start-Process -FilePath (Join-Path $output 'Pertamina.Oto.Desktop.App.exe') -ArgumentList $argument -WindowStyle Hidden -PassThru -Wait
    if ($auditProcess.ExitCode -ne 0) { throw "Native audit failed: $argument ($($auditProcess.ExitCode))" }
    $auditLines = Get-Content -LiteralPath (Join-Path $output $resultFile)
    $passes = @($auditLines | Where-Object { $_ -like 'PASS *' }).Count
    if ($passes -lt $minimum -or @($auditLines | Where-Object { $_ -like 'FAIL *' }).Count) { throw "Native audit discovery/results failed: $argument" }
    $report.Add("PASS $argument : $passes checks, exit 0")
}
try {
    & $MSBuild (Join-Path $projectRoot 'src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj') /t:Rebuild /p:Configuration=Debug "/p:DesktopUiOutputPath=$output/" /v:minimal /nologo *> (Join-Path $evidence 'build.log')
    if ($LASTEXITCODE -ne 0) { throw 'Desktop rebuild/analyzers failed.' }; $report.Add('PASS desktop Rebuild Debug with analyzers, exit 0')
    foreach ($argument in @('--self-test','--self-test-editor','--self-test-search')) {
        $workerLines = & (Join-Path $output 'worker/Pertamina.Oto.LocalPdf.Worker.exe') $argument 2>&1
        $workerLines | Out-File -LiteralPath (Join-Path $evidence ($argument.Substring(2) + '.log')) -Encoding utf8
        if ($LASTEXITCODE -ne 0) { throw "Worker regression failed: $argument" }
        $report.Add("PASS worker $argument, exit 0")
    }
    $distributionProject = Join-Path $projectRoot 'tests/DistributionChecks/DistributionChecks.csproj'
    & dotnet restore $distributionProject -p:RestoreSources= *> (Join-Path $evidence 'distribution-restore.log')
    if ($LASTEXITCODE -ne 0) { throw 'Distribution restore failed.' }
    & dotnet run --project $distributionProject --no-restore -- (Join-Path $evidence 'distribution') *> (Join-Path $evidence 'distribution.log')
    if ($LASTEXITCODE -ne 0) { throw 'Distribution regression failed.' }; $report.Add('PASS DistributionChecks, exit 0')
    $association = Start-Process -FilePath (Join-Path $output 'pdf-opener/PertaminaOTO.PdfOpener.exe') -ArgumentList @('--self-test',('"' + (Join-Path $evidence 'association') + '"')) -WindowStyle Hidden -PassThru -Wait
    if ($association.ExitCode -ne 0) { throw 'PDF opener regression failed.' }; $report.Add('PASS isolated PDF opener regression, exit 0')
    $quality = Start-Process -FilePath (Join-Path $output 'Pertamina.Oto.Desktop.App.exe') -ArgumentList '--audit-quality' -WindowStyle Hidden -PassThru -Wait
    if ($quality.ExitCode -ne 0) { throw 'Quality regression failed.' }
    $qualityPath = Get-Content -LiteralPath (Join-Path $output 'quality-audit-path.txt')
    $qualityResults = Get-Content -LiteralPath (Join-Path $qualityPath 'audit.txt')
    if (@($qualityResults | Where-Object { $_ -like 'PASS *' }).Count -lt 30 -or @($qualityResults | Where-Object { $_ -like 'FAIL *' }).Count) { throw 'Quality audit discovery failed.' }
    $report.Add('PASS native quality audit, exit 0')
    NativeAudit '--audit-update-completion' 'restart-audit/audit.txt' 163
    NativeAudit '--audit-read-previews' 'read-preview-audit/audit.txt' 60
    NativeAudit '--audit-about' 'about-audit/audit.txt' 50
    & (Join-Path $PSScriptRoot 'Test-NativeRestart.ps1') -BuildOutput $output -LauncherOutput (Join-Path $projectRoot 'artifacts/restart-launcher')
    $report.Add('PASS isolated native WinUI restart, old exits normally and new commits')
    $report.Add('LIMIT No feed publication or installed launcher maintenance; both remain separate delivery actions.')
} catch { $report.Add("FAIL $($_.Exception.Message)"); throw }
finally { $report | Out-File -LiteralPath (Join-Path $evidence 'verification.txt') -Encoding utf8; Write-Output "Verification evidence: $evidence" }
