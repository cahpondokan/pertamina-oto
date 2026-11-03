#Requires -Version 5.1
[CmdletBinding()]
param([switch]$SetupHosts)
$ErrorActionPreference='Stop'
$knownHosts=Join-Path $PSScriptRoot 'known_hosts'
if(!(Test-Path -LiteralPath $knownHosts)){throw 'known_hosts tidak tersedia di folder launcher.'}
$stream=[IO.File]::OpenRead($knownHosts);$hasher=[Security.Cryptography.SHA256]::Create()
try{$actualPin=[BitConverter]::ToString($hasher.ComputeHash($stream)).Replace('-','').ToLowerInvariant()}
finally{$stream.Dispose();$hasher.Dispose()}
if($actualPin -cne '232b43bcd50d53ba6d2dfa6054c5c4c43c25d894d050aec1710866f83b2ae5a6'){throw 'Pin kunci host SSH berubah.'}
$hostsPath=Join-Path $env:windir 'System32\drivers\etc\hosts'
$hostsText=[IO.File]::ReadAllText($hostsPath)
$matching=@($hostsText -split '\r?\n' | Where-Object { ($_ -split '#',2)[0] -match '(?i)(?:^|\s)monitor\.ayani\.id(?:\s|$)' })
if($matching.Count -gt 0 -and ($matching.Count -ne 1 -or $matching[0] -notmatch '^\s*127\.0\.0\.1\s+monitor\.ayani\.id\s*(?:#.*)?$')){throw 'Sudah ada pemetaan monitor.ayani.id yang berbeda; periksa hosts secara manual.'}
if($SetupHosts){
 $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
 $principal=New-Object Security.Principal.WindowsPrincipal($identity)
 if(!$principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'SetupHosts perlu PowerShell Run as administrator; hanya satu baris monitor yang ditambahkan.'}
 if($matching.Count -eq 0){
  $backup=Join-Path $PSScriptRoot ('hosts-backup-'+[DateTime]::Now.ToString('yyyyMMdd-HHmmss')+'.txt')
  Copy-Item -LiteralPath $hostsPath -Destination $backup -ErrorAction Stop
  [IO.File]::AppendAllText($hostsPath,"`r`n127.0.0.1 monitor.ayani.id # OTO private SSH monitor`r`n",(New-Object Text.UTF8Encoding($false)))
 }
 Write-Host 'Pemetaan privat monitor.ayani.id siap. Jalankan launcher biasa untuk menyambungkan SSH.'
 exit
}
if($matching.Count -eq 0){throw 'Jalankan Connect-Monitor.ps1 -SetupHosts sekali dari PowerShell Administrator terlebih dahulu.'}
$listeners=[Net.NetworkInformation.IPGlobalProperties]::GetIPGlobalProperties().GetActiveTcpListeners()
if(@($listeners | Where-Object {$_.Port -eq 443}).Count -gt 0){throw 'Port lokal 443 sudah digunakan. Jangan hentikan proses lain; periksa tunnel Monitor yang mungkin sudah aktif.'}
Write-Host 'Menyambungkan SSH privat. Setelah tersambung, buka https://monitor.ayani.id pada browser yang Anda pilih.'
Write-Host 'Biarkan terminal ini terbuka. Tekan Ctrl+C untuk memutus koneksi. Dashboard tetap membutuhkan login.'
Push-Location -LiteralPath $PSScriptRoot
try {
 & ssh.exe -N -L 127.0.0.1:443:127.0.0.1:5443 -o BatchMode=yes -o ExitOnForwardFailure=yes -o ServerAliveInterval=30 -o ServerAliveCountMax=3 -o StrictHostKeyChecking=yes -o UserKnownHostsFile=known_hosts -o GlobalKnownHostsFile=NUL -o HostKeyAlgorithms=ssh-ed25519 -o ConnectTimeout=15 aynadmin@187.53.134.25
 if($LASTEXITCODE -ne 0){throw ('SSH berhenti dengan kode '+$LASTEXITCODE)}
} finally {Pop-Location}
