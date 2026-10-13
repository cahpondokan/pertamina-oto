param([Parameter(Mandatory)][string]$Distribution, [Parameter(Mandatory)][string]$Evidence)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$Distribution=(Resolve-Path $Distribution).Path
$Evidence=[IO.Path]::GetFullPath($Evidence)
if(Test-Path $Evidence){throw 'Fresh evidence directory required.'}
New-Item -ItemType Directory -Path $Evidence|Out-Null
& dotnet run --project "$project/tests/DistributionChecks" --no-restore -- --verify-v2 $Distribution
if($LASTEXITCODE -ne 0){throw 'Actual artifact/schema2 gate failed.'}
$catalog=Get-Content "$Distribution/distribution-v2.json" -Raw|ConvertFrom-Json
$exe=Join-Path $Distribution $catalog.onlineInstaller.fileName
foreach($gate in @(@{mode='--self-test';folder='transport'},@{mode='--audit-ui';folder='native-ui'})){
 $process=Start-Process -FilePath $exe -ArgumentList @($gate.mode,('"'+$Evidence+'/'+$gate.folder+'"')) -WindowStyle Hidden -Wait -PassThru
 if($process.ExitCode -ne 0){throw ('Native bootstrap gate failed '+$gate.mode)}
 $lines=@(Get-Content (Join-Path $Evidence ($gate.folder+'/results.txt')))
 if(@($lines|Where-Object {$_ -like 'FAIL *'}).Count -gt 0 -or @($lines|Where-Object {$_ -like 'PASS *'}).Count -eq 0){throw 'No passing bootstrap evidence.'}
}
if((Get-FileHash $exe).Hash -ne $catalog.onlineInstaller.sha256){throw 'Bootstrap bytes changed.'}
@{status='PASS';onlineBytes=$catalog.onlineInstaller.size;downloadBytes=$catalog.offlineInstaller.size;installed=$false;published=$false;physicalDpi='NOT_TESTED'}|ConvertTo-Json|Set-Content "$Evidence/summary.json"
