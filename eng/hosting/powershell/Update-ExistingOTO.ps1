#Requires -Version 5.1
[CmdletBinding()]
param([ValidateSet('VerifyOnly','Check','Activate','Rollback')][string]$Action='Check')
$ErrorActionPreference='Stop'
Set-StrictMode -Version 2.0
$packagePin='__PACKAGE_SHA256__'
$sshOptions=@('-o','StrictHostKeyChecking=yes','-o','UserKnownHostsFile=known_hosts','-o','GlobalKnownHostsFile=NUL','-o','HostKeyAlgorithms=ssh-ed25519','-o','ConnectTimeout=15')
$target='aynadmin@187.53.134.25'
function Assert-Hash([string]$File,[string]$Hash) {
    if (!(Test-Path -LiteralPath $File -PathType Leaf) -or (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash -ne $Hash) { throw "Paket berubah: $File" }
}
function Invoke-CheckedSsh([string]$Command,[switch]$Interactive) {
    if ($Interactive) { & ssh.exe @sshOptions -tt $target $Command }
    else { & ssh.exe @sshOptions $target $Command }
    if ($LASTEXITCODE -ne 0) { throw "SSH/tahap VPS gagal ($LASTEXITCODE); publikasi tidak dianggap selesai." }
}
Push-Location -LiteralPath $PSScriptRoot
try {
    Assert-Hash 'package.json' $packagePin
    $package=Get-Content -LiteralPath package.json -Raw | ConvertFrom-Json
    if ($package.id -cnotmatch '^oto-update-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$' -or $package.metadataSha256 -cnotmatch '^[a-f0-9]{64}$' -or $package.archiveSha256 -cnotmatch '^[a-f0-9]{64}$') { throw 'Identitas paket tidak valid.' }
    foreach ($entry in $package.files) {
        if ($entry.name -cnotmatch '^[A-Za-z0-9_.-]+$') { throw 'Nama paket tidak valid.' }
        Assert-Hash $entry.name $entry.sha256
    }
    if ($Action -eq 'VerifyOnly') { Write-Host 'PASS integritas paket lokal. Belum diunggah/diaktifkan.'; return }
    $remote='/home/aynadmin/oto-updates/'+$package.id+'-'+$package.archiveSha256.Substring(0,12)
    Invoke-CheckedSsh "set -eu; umask 077; test ! -L /home/aynadmin/oto-updates; mkdir -p /home/aynadmin/oto-updates; test ! -L '$remote'; mkdir -p '$remote'; test `"`$(realpath '$remote')`" = '$remote'; chmod 700 '$remote'; test ! -L '$remote/payload.tar.gz'; test ! -L '$remote/unpack.py'"
    & scp.exe @sshOptions payload.tar.gz unpack.py "${target}:$remote/"
    if ($LASTEXITCODE -ne 0) { throw 'Unggahan gagal; belum ada aktivasi.' }
    $unpackHash=($package.files | Where-Object { $_.name -eq 'unpack.py' }).sha256
    Invoke-CheckedSsh "set -eu; echo '$unpackHash  $remote/unpack.py' | sha256sum --check; python3 -B '$remote/unpack.py' '$remote/payload.tar.gz' '$remote/contents' '$($package.archiveSha256)'; echo '$($package.updateScriptSha256)  $remote/contents/update_existing.py' | sha256sum --check; python3 -B '$remote/contents/update_existing.py' verify --metadata-sha256 '$($package.metadataSha256)'"
    if ($Action -eq 'Check') { Write-Host 'PASS staging privat dan inventory. Website/feed publik belum berubah.'; return }
    $operation=if ($Action -eq 'Rollback') { 'rollback' } else { 'activate' }
    Write-Host 'Masukkan password sudo langsung di terminal jika diminta. Jangan kirim password ke chat.'
    Invoke-CheckedSsh "sudo -- python3 -B '$remote/contents/update_existing.py' $operation --metadata-sha256 '$($package.metadataSha256)'" -Interactive
    Write-Host "PASS $operation; lihat bukti HTTP/hash pada keluaran VPS."
} catch { Write-Host ('GAGAL: '+$_.Exception.Message); exit 1 }
finally { Pop-Location }
