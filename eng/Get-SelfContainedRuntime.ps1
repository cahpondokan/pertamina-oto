param([string]$Evidence)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
if (!$Evidence) { $Evidence = Join-Path $project 'artifacts/self-contained-20261009/intake' }
New-Item -ItemType Directory -Path $Evidence -Force | Out-Null
$records = @()
foreach ($id in @('microsoft.netcore.app.runtime.win-x64','microsoft.netcore.app.host.win-x64')) {
    $version = '10.0.12'
    $url = "https://api.nuget.org/v3-flatcontainer/$id/$version/$id.$version.nupkg"
    $file = Join-Path $Evidence "$id.$version.nupkg"
    if (!(Test-Path -LiteralPath $file)) { Invoke-WebRequest -Uri $url -OutFile $file }
    # Verify the official NuGet repository package signature before restore may consume it.
    $verification = @(& dotnet nuget verify $file --all --verbosity normal 2>&1)
    $code = $LASTEXITCODE
    $verification | Set-Content -LiteralPath (Join-Path $Evidence "$id.signature.txt")
    if ($code -ne 0) { throw "Signature verification failed: $id" }
    $records += [ordered]@{ Id=$id; Version=$version; Source=$url; Length=(Get-Item -LiteralPath $file).Length; Sha256=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash; Sha512=(Get-FileHash -LiteralPath $file -Algorithm SHA512).Hash; SignatureVerified=$true }
}
@{ Scope='Local development candidate; not Production admission'; Runtime='10.0.12'; Packages=$records } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $Evidence 'verified-packages.json')
$records | Select-Object Id, Version, Length, SignatureVerified
