param([Parameter(Mandatory=$true)][string]$BuildOutput, [Parameter(Mandatory=$true)][string]$Evidence, [string]$From = 'zoom')
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Path $Evidence -Force | Out-Null
$exe=Join-Path $BuildOutput 'Pertamina.Oto.Desktop.App.exe'
$fixture=Join-Path $BuildOutput 'editor-source.pdf'
$workerResults=@(& (Join-Path $BuildOutput 'worker/Pertamina.Oto.LocalPdf.Worker.exe') --self-test-editor)
$workerExit=$LASTEXITCODE
$workerResults | Set-Content (Join-Path $Evidence 'editor-worker.txt')
if($workerExit -ne 0) { throw 'Editor worker fixture generation failed.' }
$fixtureDirectory=($workerResults | Where-Object { $_ -match 'Evidence: ' } | Select-Object -Last 1) -replace '^.*Evidence: ',''
Copy-Item -LiteralPath (Join-Path $fixtureDirectory 'source.pdf') -Destination $fixture
$cases=@(
 @{Name='zoom';Args=@('--audit-zoom');Report='zoom-audit/audit.txt'},
 @{Name='reader';Args=@('--audit-reader-ui');Pattern='reader-ui-audit-*'},
 @{Name='editor';Args=@('--audit-live-editor',('"'+$fixture+'"'));Pattern='live-editor-audit-*'},
 @{Name='preview';Args=@('--audit-read-previews');Report='read-preview-audit/audit.txt'},
 @{Name='history';Args=@('--audit-history-ui',('"'+$fixture+'"'));Report='history-ui-audit/audit.txt'},
 @{Name='startup';Args=@('--audit-startup-window');Report='startup-window-audit/audit.txt'},
 @{Name='update-dialog';Args=@('--audit-update-completion');Report='restart-audit/audit.txt'},
 @{Name='about';Args=@('--audit-responsive-dialogs');Report='responsive-dialogs/audit.txt'}
)
$results=@(); $start=@($cases.Name).IndexOf($From); if($start -lt 0) {throw 'Unknown From case'}; $cases=$cases[$start..($cases.Count-1)]
foreach($case in $cases) {
 $started=Get-Date
 $process=Start-Process -FilePath $exe -ArgumentList $case.Args -WindowStyle Hidden -PassThru -Wait
 $record=[ordered]@{Name=$case.Name;Arguments=$case.Args;ExitCode=$process.ExitCode;Started=$started.ToString('o');Completed=(Get-Date).ToString('o')}
 if($case.Report) { $auditPath=Join-Path $BuildOutput $case.Report } else { $auditPath=Join-Path (Get-ChildItem -LiteralPath $BuildOutput -Directory -Filter $case.Pattern | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName 'audit.txt' }
 $lines=@(Get-Content -LiteralPath $auditPath -ErrorAction Stop)
 $record.Report=$auditPath; $record.Passes=@($lines | Where-Object {$_ -like 'PASS *'}).Count; $record.Failures=@($lines | Where-Object {$_ -like 'FAIL *'}).Count
 $results += [pscustomobject]$record
 $results | ConvertTo-Json -Depth 6 | Set-Content (Join-Path $Evidence 'regression-results.json')
 Write-Output "$($case.Name) EXIT=$($process.ExitCode)"
 if($process.ExitCode -ne 0 -or $record.Failures -ne 0 -or $record.Passes -eq 0) { throw "Native regression $($case.Name) failed; packaging blocked." }
}
