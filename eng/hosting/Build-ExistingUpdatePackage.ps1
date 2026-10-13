param([Parameter(Mandatory=$true)][string]$Web,[Parameter(Mandatory=$true)][string]$Release,
      [Parameter(Mandatory=$true)][string]$PreviousCatalog,[Parameter(Mandatory=$true)][string]$KnownHosts,
      [Parameter(Mandatory=$true)][string]$Output,
      [string]$PreviousWeb='/opt/oto-downloads/oto-20261001-102920-701ec3e1-v1/web',
      [string]$PreviousBaselineSha256='',
      [string]$UnitSha256='59e8478576c9ca537e7ddc68320575d1b44e911b28a01165883cbbe39e483551',
      [string]$NginxSha256='1e5024400b382868453d12e3c73b723ed436d606dc8b15a5312db1b664dfd767')
$ErrorActionPreference='Stop'
$Web=(Resolve-Path -LiteralPath $Web).Path
$Release=(Resolve-Path -LiteralPath $Release).Path
$Output=[IO.Path]::GetFullPath($Output)
if (Test-Path -LiteralPath $Output) { throw 'Paket immutable sudah ada; output baru diperlukan.' }
$catalog=Get-Content -LiteralPath (Join-Path $Release current.json) -Raw | ConvertFrom-Json
if ($catalog.version -cnotmatch '^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$' -or !$catalog.installerFileName) { throw 'Katalog installer wajib.' }
if ($PreviousWeb -cnotmatch '^/opt/oto-downloads/[A-Za-z0-9-]+/web$' -or $UnitSha256 -cnotmatch '^[a-f0-9]{64}$' -or $NginxSha256 -cnotmatch '^[a-f0-9]{64}$') { throw 'Baseline tidak valid.' }
if ($PreviousBaselineSha256 -and $PreviousBaselineSha256 -cnotmatch '^[a-f0-9]{64}$') { throw 'Pin baseline baru tidak valid.' }
$releaseNames=@('current.json',$catalog.fileName,$catalog.installerFileName)
$baselinePath=Join-Path $Release distribution-v2.json
if (Test-Path -LiteralPath $baselinePath) {
    $baseline=Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json
    if ($baseline.schema -ne 2 -or $baseline.version -cne $catalog.version -or $baseline.productVersion -cne $catalog.productVersion -or $baseline.updaterProtocol -ne 2 -or $baseline.minimumUpdaterBaseline -ne 1 -or !$catalog.manualInstallerOnly -or !$catalog.runtimeBundled) { throw 'Katalog baseline tidak sesuai proyeksi kompatibilitas.' }
    foreach ($artifact in @($baseline.onlineInstaller,$baseline.offlineInstaller,$baseline.updatePayload)) {
        $name=$artifact.fileName
        if (!$name -or $name -ne [IO.Path]::GetFileName($name) -or $name -match '[:\\/]') { throw 'Nama artifact baseline tidak aman.' }
        $payload=Get-Item -LiteralPath (Join-Path $Release $name)
        if ($payload.Length -ne $artifact.size -or (Get-FileHash -LiteralPath $payload.FullName).Hash -ine $artifact.sha256) { throw 'Hash/ukuran artifact baseline tidak cocok.' }
    }
    $releaseNames+=@('distribution-v2.json',$baseline.onlineInstaller.fileName)
}
elseif ($PreviousBaselineSha256) { throw 'Jangan menurunkan portal baseline baru ke paket legacy.' }
foreach ($source in @($Web,$Release)) {
    foreach ($item in @(Get-Item -LiteralPath $source)+@(Get-ChildItem -LiteralPath $source -Recurse -Force)) {
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Source berisi link.' }
    }
}
$previous=Get-Content -LiteralPath $PreviousCatalog -Raw | ConvertFrom-Json
if ($previous.version -ge $catalog.version) { throw 'Kandidat harus lebih baru dari baseline feed.' }
New-Item -ItemType Directory -Path $Output | Out-Null
$contents=Join-Path $Output contents
New-Item -ItemType Directory -Path $contents | Out-Null
Copy-Item -LiteralPath $Web -Destination (Join-Path $contents web) -Recurse
New-Item -ItemType Directory -Path (Join-Path $contents releases) | Out-Null
foreach ($name in $releaseNames) {
    if ($name -ne [IO.Path]::GetFileName($name) -or $name -match '[:\\]') { throw 'Nama payload tidak aman.' }
    Copy-Item -LiteralPath (Join-Path $Release $name) -Destination (Join-Path "$contents/releases" $name)
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot powershell/update_existing.py) -Destination $contents
$files=@(Get-ChildItem "$contents/web","$contents/releases" -Recurse -File | ForEach-Object {
    @{path=$_.FullName.Substring($contents.Length+1).Replace('\','/');size=$_.Length;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash.ToLowerInvariant()}
})
$publication=@{id='oto-update-'+$catalog.version;version=$catalog.version;previousWeb=$PreviousWeb;unitSha256=$UnitSha256;nginxSha256=$NginxSha256;previousCatalog=$previous;previousBaselineSha256=$(if ($PreviousBaselineSha256) {$PreviousBaselineSha256} else {$null});files=$files}
$utf8=[Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText("$contents/publication.json",($publication | ConvertTo-Json -Depth 8),$utf8)
& tar.exe -czf "$Output/payload.tar.gz" -C $contents .
if ($LASTEXITCODE -ne 0) { throw 'Pembuatan archive gagal.' }
Copy-Item -LiteralPath $KnownHosts -Destination "$Output/known_hosts"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot powershell/unpack.py) -Destination $Output
$package=@{id=$publication.id;version=$catalog.version;metadataSha256=(Get-FileHash "$contents/publication.json").Hash.ToLowerInvariant();archiveSha256=(Get-FileHash "$Output/payload.tar.gz").Hash.ToLowerInvariant();updateScriptSha256=(Get-FileHash "$contents/update_existing.py").Hash.ToLowerInvariant();files=@()}
foreach ($name in @('payload.tar.gz','known_hosts','unpack.py')) { $package.files+=@{name=$name;sha256=(Get-FileHash "$Output/$name").Hash.ToLowerInvariant()} }
[IO.File]::WriteAllText("$Output/package.json",($package | ConvertTo-Json -Depth 5),$utf8)
$script=(Get-Content -LiteralPath (Join-Path $PSScriptRoot powershell/Update-ExistingOTO.ps1) -Raw).Replace('__PACKAGE_SHA256__',(Get-FileHash "$Output/package.json").Hash.ToLowerInvariant())
[IO.File]::WriteAllText("$Output/Update-OTO.ps1",$script,$utf8)
Write-Output "Prepared only: $Output; publication $($package.id); metadata SHA256 $($package.metadataSha256)"
