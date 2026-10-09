# Inert fixture feed only. Never points at releases/windows-local or installed files.
param([Parameter(Mandatory=$true)][string]$Evidence)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$fixture = [IO.Path]::GetFullPath($Evidence)
if (!(($fixture + '/').Replace('\','/').StartsWith(($project + '/artifacts/').Replace('\','/'), [StringComparison]::OrdinalIgnoreCase))) { throw 'Evidence must be in workspace artifacts.' }
New-Item -ItemType Directory -Path "$fixture/source/worker" -Force | Out-Null
[IO.File]::WriteAllText("$fixture/source/Pertamina.Oto.Desktop.App.exe", 'INERT TEST FIXTURE; NOT AN APPLICATION')
[IO.File]::WriteAllText("$fixture/source/worker/Pertamina.Oto.LocalPdf.Worker.exe", 'INERT TEST FIXTURE')
$testBinary = Join-Path $project 'tests/ProductVersionChecks/bin/Debug/net10.0/ProductVersionChecks.dll'
Copy-Item -LiteralPath $testBinary -Destination "$fixture/source/Pertamina.Oto.Desktop.App.dll"
Copy-Item -LiteralPath (Join-Path (Split-Path $testBinary -Parent) 'product-info.json') -Destination "$fixture/source/product-info.json"
$results = [Collections.Generic.List[string]]::new()
function Check([bool]$value,[string]$label) { if (!$value) { throw $label }; $results.Add('PASS ' + $label) }
& (Join-Path $PSScriptRoot 'Publish-LocalUpdate.ps1') -BuildOutput "$fixture/source" -Feed "$fixture/feed"
$latest = Get-Content "$fixture/feed/latest.json" -Raw | ConvertFrom-Json
$manifestFile = "$fixture/feed/$($latest.Version)/manifest.json"
$manifest = Get-Content $manifestFile -Raw | ConvertFrom-Json
$info = Get-Content "$fixture/feed/$($latest.Version)/app/build-info.json" -Raw | ConvertFrom-Json
Check ($latest.Version -cmatch '^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$') 'immutable build ID remains existing wire format'
Check ($latest.Version -ceq $manifest.Version -and $latest.Version -ceq $info.Version) 'build identity agrees across package graph'
Check ($latest.ProductVersion -ceq ([xml](Get-Content (Join-Path $PSScriptRoot 'ProductRelease.props') -Raw)).Project.PropertyGroup.OtoProductVersion -and $manifest.ProductVersion -ceq $latest.ProductVersion -and $info.ProductVersion -ceq $latest.ProductVersion) 'product identity agrees across package graph'
Check ($latest.Channel -ceq 'Pengembangan' -and $manifest.Channel -ceq $latest.Channel -and $info.Channel -ceq $latest.Channel) 'channel agrees across package graph'
Check ((Get-FileHash $manifestFile).Hash -ceq $latest.ManifestSha256) 'manifest integrity includes product identity'
$allValid = $true
foreach ($entry in $manifest.Files) {
 $file = "$fixture/feed/$($latest.Version)/app/$($entry.Path)"
 if ((Get-Item $file).Length -ne $entry.Length -or (Get-FileHash $file).Hash -cne $entry.Sha256) { $allValid = $false }
}
Check $allValid 'all inert payload lengths and hashes agree'
$original = Get-Content "$fixture/source/product-info.json" -Raw
$differentProduct = if (($original | ConvertFrom-Json).ProductVersion -ceq '0.0.0') { '0.0.1' } else { '0.0.0' }
[IO.File]::WriteAllText("$fixture/source/product-info.json", (@{ProductVersion=$differentProduct;Channel='Pengembangan'} | ConvertTo-Json))
$rejected = $false
try { & (Join-Path $PSScriptRoot 'Publish-LocalUpdate.ps1') -BuildOutput "$fixture/source" -Feed "$fixture/rejected-feed" } catch { $rejected = $true }
Check ($rejected -and !(Test-Path "$fixture/rejected-feed/latest.json")) 'mismatched binary identity rejected before publishing'
[IO.File]::WriteAllText("$fixture/source/product-info.json", $original)
Check ((Get-Content "$fixture/feed/latest.json" -Raw | ConvertFrom-Json).Version -ceq $latest.Version) 'failure leaves prior fixture release unchanged'
$results | Set-Content "$fixture/results.txt"
Write-Output "$($results.Count) PASS; inert fixture package only."
