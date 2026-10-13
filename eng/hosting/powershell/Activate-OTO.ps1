#Requires -Version 5.1
[CmdletBinding()]
param([ValidateSet('Activate','Check','SmokeTest','Rollback','VerifyOnly')][string]$Action = 'Activate')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0
$expectedInfo = '__INFO_SHA256__'
$target = 'aynadmin@187.53.134.25'
$sshOptions = @('-o','StrictHostKeyChecking=yes','-o','UserKnownHostsFile=known_hosts','-o','GlobalKnownHostsFile=NUL','-o','HostKeyAlgorithms=ssh-ed25519','-o','ConnectTimeout=15','-o','ServerAliveInterval=15','-o','ServerAliveCountMax=3')

function Assert-Hash([string]$Path, [string]$Expected) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf) -or (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash -ne $Expected) {
        throw "File paket hilang atau berubah: $Path. Gunakan kembali paket asli."
    }
}
function Invoke-OtoSsh([string]$Command, [switch]$Interactive) {
    if ($Interactive) { & ssh.exe @sshOptions -tt $target $Command }
    else { & ssh.exe @sshOptions $target $Command }
    if ($LASTEXITCODE -ne 0) { throw "Tahap SSH belum berhasil (kode $LASTEXITCODE). Pesan penyebab ada di atas; situs tidak dianggap aktif." }
}

Push-Location -LiteralPath $PSScriptRoot
try {
    Write-Host '[1/5] Memeriksa paket aktivasi OTO...' -ForegroundColor Cyan
    Assert-Hash 'package.json' $expectedInfo
    $package = Get-Content -LiteralPath 'package.json' -Raw | ConvertFrom-Json
    if ($package.deploymentId -cnotmatch '^oto-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}-v[0-9]+$' -or $package.archiveSha256 -cnotmatch '^[a-f0-9]{64}$' -or $package.manifestSha256 -cnotmatch '^[a-f0-9]{64}$') { throw 'Identitas paket tidak valid.' }
    foreach ($file in $package.files) {
        if ($file.name -cnotmatch '^[A-Za-z0-9_.-]+$') { throw 'Path paket tidak valid.' }
        Assert-Hash $file.name $file.sha256
    }
    if ($Action -eq 'VerifyOnly') { Write-Host 'PASS paket lokal lengkap dan semua SHA256 cocok.' -ForegroundColor Green; return }
    foreach ($tool in @('ssh.exe','scp.exe')) { if (!(Get-Command $tool -ErrorAction SilentlyContinue)) { throw 'OpenSSH Client Windows belum tersedia. Aktifkan melalui Windows Optional Features, lalu jalankan kembali.' } }
    $remote = '/home/aynadmin/oto-activation/' + $package.deploymentId + '-' + $package.archiveSha256.Substring(0,12)
    Write-Host "[2/5] Menghubungkan ke $target dan menyiapkan folder privat..." -ForegroundColor Cyan
    Write-Host 'Jika diminta password SSH/sudo, ketik langsung di terminal ini. Ketikan password tidak ditampilkan atau disimpan oleh skrip.'
    $prepare = "set -eu; umask 077; test ! -L /home/aynadmin; test ! -L /home/aynadmin/oto-activation; mkdir -p /home/aynadmin/oto-activation; test ! -L '$remote'; mkdir -p '$remote'; test `"`$(realpath '$remote')`" = '$remote'; chmod 700 '$remote'"
    Invoke-OtoSsh $prepare
    $cached = & ssh.exe @sshOptions $target "if test -f '$remote/payload.tar.gz' && test ! -L '$remote/payload.tar.gz'; then sha256sum '$remote/payload.tar.gz'; fi"
    if ($LASTEXITCODE -ne 0) { throw 'Pemeriksaan unggahan VPS gagal.' }
    if (($cached -join '').StartsWith($package.archiveSha256 + ' ')) {
        Write-Host '[3/5] Paket yang sama sudah lengkap di VPS; unggahan digunakan kembali.' -ForegroundColor Cyan
    } else {
        Write-Host '[3/5] Mengunggah portal, EXE dan runtime ASP.NET. Tunggu hingga selesai...' -ForegroundColor Cyan
        Invoke-OtoSsh "test ! -L '$remote/payload.upload'"
        & scp.exe @sshOptions 'payload.tar.gz' "${target}:$remote/payload.upload"
        if ($LASTEXITCODE -ne 0) { throw 'Unggahan terputus. Jalankan lagi; aktivasi belum dimulai.' }
        Invoke-OtoSsh "set -eu; echo '$($package.archiveSha256)  $remote/payload.upload' | sha256sum --check; mv -f -- '$remote/payload.upload' '$remote/payload.tar.gz'"
    }
    $controlNames = @('unpack.py','deploy.py','test_deployment.py','SHA256SUMS')
    foreach ($name in $controlNames) { Invoke-OtoSsh "test ! -L '$remote/$name'" }
    & scp.exe @sshOptions @controlNames "${target}:$remote/"
    if ($LASTEXITCODE -ne 0) { throw 'Unggahan pemeriksa paket gagal.' }
    $controlChecks = @($package.files | Where-Object { $_.name -in $controlNames } | ForEach-Object { "echo '$($_.sha256)  $remote/$($_.name)' | sha256sum --check" }) -join '; '
    Write-Host '[4/5] Memeriksa ulang checksum dan kondisi VPS...' -ForegroundColor Cyan
    Invoke-OtoSsh "set -eu; $controlChecks; python3 -B '$remote/unpack.py' '$remote/payload.tar.gz' '$remote/contents' '$($package.archiveSha256)'; cp -- '$remote/deploy.py' '$remote/test_deployment.py' '$remote/unpack.py' '$remote/SHA256SUMS' '$remote/contents/'; python3 -B '$remote/contents/deploy.py' check --manifest '$($package.manifestSha256)'"
    if ($Action -eq 'Check') { Write-Host 'PASS pemeriksaan VPS. Belum ada aktivasi/sudo/perubahan Nginx.' -ForegroundColor Green; return }
    if ($Action -eq 'SmokeTest') {
        Invoke-OtoSsh "python3 -B '$remote/contents/deploy.py' smoke --manifest '$($package.manifestSha256)'"
        Write-Host 'PASS uji portal Linux sementara; proses dihentikan dan situs publik belum diaktifkan.' -ForegroundColor Green
        return
    }
    $remoteAction = if ($Action -eq 'Rollback') { 'rollback' } else { 'activate' }
    Write-Host "[5/5] Menjalankan $remoteAction. Masukkan password sudo VPS bila diminta..." -ForegroundColor Cyan
    Invoke-OtoSsh "sudo -- python3 -B '$remote/contents/deploy.py' $remoteAction --manifest '$($package.manifestSha256)'" -Interactive
    if ($Action -eq 'Rollback') { Write-Host 'Rollback selesai. Konfigurasi situs sebelumnya dipulihkan.' -ForegroundColor Green }
    else { Write-Host 'SELESAI: https://oto.ayani.id — portal dan unduhan EXE telah diperiksa dari VPS.' -ForegroundColor Green }
} catch {
    Write-Host ('GAGAL: ' + $_.Exception.Message) -ForegroundColor Red
    Write-Host 'Simpan pesan kesalahannya. Jangan kirim password. Anda dapat menjalankan ulang paket yang sama.'
    exit 1
} finally { Pop-Location }
