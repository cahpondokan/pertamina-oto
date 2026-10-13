param([string]$Output, [string]$ReusePayloadArchive)
$ErrorActionPreference = 'Stop'
$project = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$source = Join-Path $PSScriptRoot 'powershell'
$id = 'oto-20261001-102920-701ec3e1-v1'
if (!$Output) { $Output = Join-Path $project ('artifacts/vps-activation-build-' + [Guid]::NewGuid().ToString('N')) }
$Output = [IO.Path]::GetFullPath($Output)
if (Test-Path -LiteralPath $Output) { throw 'Output sudah ada; gunakan folder baru.' }
$contents = Join-Path $Output 'staged-payload'
$package = Join-Path $Output 'OTO-Aktivasi-VPS'
New-Item -ItemType Directory -Path "$contents/web","$contents/releases","$contents/config",$package -Force | Out-Null
$utf8 = [Text.UTF8Encoding]::new($false)
function Write-Utf8([string]$Path,[string]$Text) { [IO.File]::WriteAllText($Path, $Text.Replace("`r`n","`n"), $utf8) }
function Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
$distribution = Join-Path $project 'artifacts/windows-exe-distribution'
$release = Get-Content -LiteralPath "$distribution/current.json" -Raw | ConvertFrom-Json
if ($release.version -ne '20261001-102920-701ec3e1' -or $release.installerSha256 -ne 'E6704300B8E3D40FE8E60F228CB942CF2C06E72622D4594859BCCB7A46C139B1') { throw 'Kandidat EXE berbeda dari yang diminta.' }
foreach ($name in @($release.fileName,$release.installerFileName)) {
    if ($name -ne [IO.Path]::GetFileName($name)) { throw 'Nama distribusi tidak aman.' }
    Copy-Item -LiteralPath (Join-Path $distribution $name) -Destination "$contents/releases"
}
Copy-Item -LiteralPath "$distribution/current.json" -Destination "$contents/releases"
if ((Hash "$contents/releases/$($release.installerFileName)") -ne $release.installerSha256 -or (Hash "$contents/releases/$($release.fileName)") -ne $release.sha256) {throw 'Hash distribusi tidak cocok.'}
Copy-Item (Join-Path $project 'artifacts/download-portal/linux-exe-publish/*') "$contents/web" -Recurse
Copy-Item -LiteralPath (Join-Path $project 'artifacts/aspnet-linux-runtime-intake-10.0.12/aspnetcore-runtime-10.0.12-linux-x64.tar.gz') -Destination "$contents/runtime.tar.gz"
$runtimeHash = 'c489fc3a9dcc85f24f2d52e9a81b46fdf51affc8a360d008dbd5e3b7ce524969adc4a54f555ad13e29b822f59ed1cae9785608cbac2282f2c9c4fda45c21c7d1'
if ((Get-FileHash -LiteralPath "$contents/runtime.tar.gz" -Algorithm SHA512).Hash -ne $runtimeHash) {throw 'Pin runtime resmi Microsoft tidak cocok.'}
foreach ($name in @('deploy.py','test_deployment.py','unpack.py')) { Write-Utf8 (Join-Path $contents $name) ([IO.File]::ReadAllText((Join-Path $source $name))) }
$unit = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'oto-downloads.service')).Replace('ExecStart=/usr/bin/dotnet','ExecStart=/opt/oto-downloads/runtime/10.0.12/dotnet').Replace('Environment=ASPNETCORE_ENVIRONMENT=Production',"Environment=ASPNETCORE_ENVIRONMENT=Production`nEnvironment=DOTNET_ROOT=/opt/oto-downloads/runtime/10.0.12`nEnvironment=DOTNET_CLI_TELEMETRY_OPTOUT=1`nEnvironment=DOTNET_NOLOGO=1")
Write-Utf8 "$contents/config/oto-downloads.service" $unit
Write-Utf8 "$contents/config/oto.ayani.id.conf" ([IO.File]::ReadAllText((Join-Path $PSScriptRoot 'oto.ayani.id.vps-reviewed.conf')))
$config = [ordered]@{deploymentId=$id;release=$release.version;runtimeVersion='10.0.12';runtimeSha512=$runtimeHash;baselineNginxSha256='7ca6b06b4911ae5b094c025ce13f7be87365ab6e95b7d19d42ddd965da706fba';candidateNginxSha256=(Hash "$contents/config/oto.ayani.id.conf");unitSha256=(Hash "$contents/config/oto-downloads.service");exeFileName=$release.installerFileName;exeSha256=$release.installerSha256.ToLowerInvariant();exeSize=$release.installerSize}
Write-Utf8 "$contents/deployment.json" ($config | ConvertTo-Json)
$checks = @(Get-ChildItem -LiteralPath $contents -Recurse -File | Sort-Object FullName | ForEach-Object { (Hash $_.FullName) + '  ' + [IO.Path]::GetRelativePath($contents,$_.FullName).Replace('\','/') })
Write-Utf8 "$contents/SHA256SUMS" (($checks -join "`n") + "`n")
if ($ReusePayloadArchive) {
    if ((Hash $ReusePayloadArchive) -ne '7fef7923cac268a142a6bbe7b9b97570e3c8ad7d0bc5d0f941e24ae853d3e488') {throw 'Reuse hanya diizinkan untuk arsip kandidat yang sudah diverifikasi.'}
    Copy-Item -LiteralPath $ReusePayloadArchive -Destination "$package/payload.tar.gz"
    $reused = Join-Path $Output 'reuse-verification'
    New-Item -ItemType Directory -Path $reused | Out-Null
    & tar.exe -xzf "$package/payload.tar.gz" -C $reused
    if ($LASTEXITCODE -ne 0) {throw 'Verifikasi reuse arsip gagal.'}
    foreach ($name in @('deploy.py','test_deployment.py','unpack.py','SHA256SUMS')) { Copy-Item -LiteralPath (Join-Path $contents $name) -Destination $reused }
    foreach ($entry in $checks) { $parts=$entry -split '  ',2; if((Hash (Join-Path $reused $parts[1])) -ne $parts[0]) {throw 'Arsip lama berbeda dari kandidat setelah overlay kontrol.'} }
    if (@(Get-ChildItem -LiteralPath $reused -Recurse -File).Count -ne ($checks.Count + 1)) {throw 'Ada file ekstra pada arsip reuse.'}
} else {
    & tar.exe -czf "$package/payload.tar.gz" -C $contents .
    if ($LASTEXITCODE -ne 0) {throw 'Pembuatan arsip VPS gagal.'}
}
foreach ($name in @('deploy.py','test_deployment.py','unpack.py','SHA256SUMS')) { Copy-Item -LiteralPath (Join-Path $contents $name) -Destination $package }
Write-Utf8 "$package/known_hosts" "187.53.134.25 ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIImh4jhFSN39c3T0AvnXnXImxd7J37GhLWZ8rHL5Zl0d`n"
$info = [ordered]@{deploymentId=$id;archiveSha256=(Hash "$package/payload.tar.gz");manifestSha256=(Hash "$contents/SHA256SUMS");files=@(foreach($name in @('known_hosts','unpack.py','deploy.py','test_deployment.py','SHA256SUMS','payload.tar.gz')) {@{name=$name;sha256=(Hash (Join-Path $package $name))}})}
Write-Utf8 "$package/package.json" ($info | ConvertTo-Json -Depth 4)
$script = [IO.File]::ReadAllText((Join-Path $source 'Activate-OTO.ps1')).Replace('__INFO_SHA256__',(Hash "$package/package.json"))
[IO.File]::WriteAllText("$package/Aktifkan-OTO.ps1",$script,[Text.UTF8Encoding]::new($true))
$readme = @'
PERTAMINA OTO - AKTIVASI VPS
Target: aynadmin@187.53.134.25 / https://oto.ayani.id
Kandidat: 20261001-102920-701ec3e1 (EXE pengembangan yang belum ditandatangani).

Jalankan dari PowerShell biasa (tidak perlu Run as Administrator di Windows):
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\Aktifkan-OTO.ps1"
Gunakan path lengkap apabila PowerShell sedang berada di folder lain.
ExecutionPolicy Bypass hanya berlaku untuk proses ini; kebijakan Windows tidak diubah permanen.

Skrip memverifikasi paket dan identitas SSH, mengunggahnya, memeriksa VPS,
lalu meminta sudo melalui sesi SSH interaktif. Ketik password langsung di
terminal bila diminta. Password tidak tampil dan tidak direkam skrip.
Jangan kirim password di chat. Tetap buka terminal hingga hasil akhir tampil.

Paket lengkap memuat portal, EXE/ZIP OTO dan runtime ASP.NET Core10.0.12 resmi
Microsoft (pin SHA512 diperiksa). Tidak perlu memasang SDK/dotnet di Windows.
Runtime server dipasang khusus OTO di /opt/oto-downloads/runtime/10.0.12.
Tidak mengubah repository apt, dotnet global, aplikasi/container lain atau firewall.
Library Ubuntu yang diperlukan sudah diperiksa. Jika kondisi berubah, skrip berhenti.
Nginx situs OTO dibackup, konfigurasi Certbot dipertahankan, lalu reload setelah
layanan OTO sehat. Health, metadata dan hash unduhan EXE HTTPS diperiksa sebelum sukses.

Apabila unggahan terputus atau sudo salah, jalankan perintah yang sama lagi.
Checksum paket yang sudah lengkap digunakan kembali. Kegagalan aktivasi mencoba
memulihkan konfigurasi situs sebelumnya; file deployment disimpan untuk diagnosis.
Jangan menghapus atau mengedit file paket. Jika host key/baseline situs berbeda,
minta pemeriksaan ulang; jangan menonaktifkan pemeriksaan SSH.

Pilihan tambahan:
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\Aktifkan-OTO.ps1" -Action VerifyOnly
    Periksa paket lokal saja; tidak ada SSH.
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\Aktifkan-OTO.ps1" -Action Check
    Unggah/periksa VPS tanpa sudo dan tanpa aktivasi situs.
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\Aktifkan-OTO.ps1" -Action Rollback
    Dengan sudo, pulihkan konfigurasi situs sebelumnya dan hentikan hanya layanan OTO.

Backup: /var/backups/oto-downloads/oto-20261001-102920-701ec3e1-v1/
Status layanan (setelah SSH): systemctl status oto-downloads.service
Log layanan (administrator): sudo journalctl -u oto-downloads.service -n 60 --no-pager

Aktivasi portal pengembangan ini bukan kelulusan phase/Production atau signing.
Runtime privat tidak mendapat update apt otomatis; pembaruan perlu paket yang diverifikasi.
Dokumen pengguna tidak diunggah. Aplikasi Windows tidak dipasang otomatis oleh paket VPS.
'@
Write-Utf8 "$package/BACA-DULU.txt" $readme
Write-Output "Package folder: $package"
Write-Output "Payload SHA256: $($info.archiveSha256)"
Write-Output "Manifest SHA256: $($info.manifestSha256)"
