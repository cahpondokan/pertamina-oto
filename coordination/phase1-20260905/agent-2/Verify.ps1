param()
$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:TESTINGPLATFORM_TELEMETRY_OPTOUT = '1'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
Push-Location $root
try {
    $records = @()
    $projects = @(
        'tests/Unit/Pdf/Pertamina.Oto.Modules.Pdf.UnitTests/Pertamina.Oto.Modules.Pdf.UnitTests.csproj',
        'tests/Contract/Pdf/Pertamina.Oto.Modules.Pdf.ContractTests/Pertamina.Oto.Modules.Pdf.ContractTests.csproj'
    )
    foreach ($project in $projects) {
        $suite = if ($project.Contains('/Unit/')) { 'unit' } else { 'contract' }
        foreach ($stage in @('restore', 'build', 'test')) {
            $arguments = switch ($stage) {
                'restore' { @('restore', $project, '--locked-mode') }
                'build' { @('build', $project, '-c', 'Release', '--no-restore') }
                'test' { @('run', '--project', $project, '-c', 'Release', '--no-build', '--no-restore', '--', '--report-trx') }
            }
            $output = & dotnet @arguments 2>&1
            $code = $LASTEXITCODE
            $safe = ($output | Out-String).Replace($root, '<WORKTREE>')
            Set-Content (Join-Path $PSScriptRoot "$suite-$stage.log") $safe.TrimEnd()
            $records += [ordered]@{ command = "dotnet $($arguments -join ' ')"; exitCode = $code; evidence = "$suite-$stage.log" }
            if ($code -ne 0) { throw "$suite $stage failed: $code" }
        }
        $trxPath = Join-Path (Split-Path $project) "bin/Release/net10.0/TestResults/Pertamina.Oto.Modules.Pdf.$(if ($suite -eq 'unit') {'UnitTests'} else {'ContractTests'})_net10.0_x64.trx"
        [xml]$trx = Get-Content $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        if ([int]$counters.total -eq 0 -or [int]$counters.failed -ne 0 -or [int]$counters.passed -ne [int]$counters.total) {
            throw 'Non-zero discovery / pass guard failed'
        }
        $records += [ordered]@{ suite = $suite; total = [int]$counters.total; passed = [int]$counters.passed; failed = [int]$counters.failed }
    }
    $baseline = Get-Content 'tests/Unit/Pertamina.Oto.Foundation.UnitTests/packages.lock.json' -Raw | ConvertFrom-Json -AsHashtable
    foreach ($project in $projects) {
        $candidate = Get-Content (Join-Path (Split-Path $project) 'packages.lock.json') -Raw | ConvertFrom-Json -AsHashtable
        foreach ($entry in $candidate.dependencies['net10.0'].GetEnumerator()) {
            if ($entry.Value.type -eq 'Project') { continue }
            $approved = $baseline.dependencies['net10.0'][$entry.Key]
            if (!$approved -or $entry.Value.resolved -ne $approved.resolved -or $entry.Value.contentHash -ne $approved.contentHash) {
                throw "Unapproved package delta: $($entry.Key)"
            }
        }
    }
    $sourceFiles = Get-ChildItem src/Modules/Pdf -Recurse -Filter '*.cs' | Where-Object FullName -NotMatch '[\\/](obj|bin)[\\/]'
    $forbidden = $sourceFiles | Select-String 'System.Net|HttpClient|SignalR|SqlConnection|File.Write|File.Delete|File.Move|Console.Write'
    if ($forbidden) { throw 'Forbidden IO/network source dependency' }
    $records += [ordered]@{ check = 'package version/contentHash parity with approved baseline'; result = 'PASS' }
    $records += [ordered]@{ check = 'no concrete network/filesystem/database/log IO in PDF source'; result = 'PASS'; limitation = 'static source check, not OS network denial' }
    $records | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'verification.json')
    Write-Output 'AGENT_2_VERIFICATION=PASS'
} finally { Pop-Location }
