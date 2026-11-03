#Requires -Version 5.1
[CmdletBinding()]
param([ValidateSet('VerifyOnly','Check','Activate','Rollback')][string]$Action='VerifyOnly')
$ErrorActionPreference='Stop'
Set-StrictMode -Version 2.0
$packagePin='__PACKAGE_SHA256__'
$sshOptions=@('-o','BatchMode=yes','-o','StrictHostKeyChecking=yes','-o','UserKnownHostsFile=known_hosts','-o','GlobalKnownHostsFile=NUL','-o','HostKeyAlgorithms=ssh-ed25519','-o','ConnectTimeout=15')
$target='aynadmin@187.53.134.25'
function Assert-Hash([string]$File,[string]$Hash) {
 if (!(Test-Path -LiteralPath $File -PathType Leaf) -or (Get-FileHash -LiteralPath $File -Algorithm SHA256).Hash -ine $Hash) { throw "Package changed: $File" }
}
function Invoke-Ssh([string]$Command,[switch]$Interactive) {
 if($Interactive){ & ssh.exe @sshOptions -tt $target $Command }
 else { & ssh.exe @sshOptions $target $Command }
 if($LASTEXITCODE -ne 0){throw "SSH operation failed ($LASTEXITCODE). Publication is not confirmed."}
}
Push-Location -LiteralPath $PSScriptRoot
try {
 Assert-Hash 'package.json' $packagePin
 $package=Get-Content -LiteralPath 'package.json' -Raw | ConvertFrom-Json
 if($package.id -cnotmatch '^oto-web-[0-9]{8}-[a-f0-9]{12}$'){throw 'Invalid web release id.'}
 foreach($entry in $package.files){
  if($entry.name -cnotmatch '^[A-Za-z0-9_.-]+$'){throw 'Unsafe package filename.'}
  Assert-Hash $entry.name $entry.sha256
 }
 Assert-Hash 'contents/publication.json' $package.metadataSha256
 Assert-Hash 'contents/deploy_web.py' $package.helperSha256
 $publication=Get-Content -LiteralPath 'contents/publication.json' -Raw | ConvertFrom-Json
 foreach($entry in $publication.files){
  if($entry.path -match '(^/|\\|:|(^|/)\.\.?(/|$))'){throw 'Unsafe web path.'}
  $file=Join-Path 'contents/web' $entry.path
  Assert-Hash $file $entry.sha256
  if((Get-Item -LiteralPath $file).Length -ne $entry.size){throw 'Web file size changed.'}
 }
 if($Action -eq 'VerifyOnly'){Write-Host ('PASS local package: '+$package.id+'. Not uploaded.');return}
 $remote='/home/aynadmin/oto-web-updates/'+$package.id+'-'+$package.archiveSha256.Substring(0,12)
 Invoke-Ssh "set -eu; umask 077; test ! -L /home/aynadmin/oto-web-updates; mkdir -p /home/aynadmin/oto-web-updates; test ! -L '$remote'; mkdir -p '$remote'; test `"`$(realpath '$remote')`" = '$remote'; chmod 700 '$remote'; test ! -L '$remote/payload.tar.gz'; test ! -L '$remote/unpack.py'"
 & scp.exe @sshOptions payload.tar.gz unpack.py "${target}:$remote/"
 if($LASTEXITCODE -ne 0){throw 'Upload failed. No activation performed.'}
 $unpackHash=($package.files|Where-Object {$_.name -eq 'unpack.py'}).sha256
 Invoke-Ssh "set -eu; echo '$unpackHash  $remote/unpack.py' | sha256sum --check; python3 -B '$remote/unpack.py' '$remote/payload.tar.gz' '$remote/contents' '$($package.archiveSha256)'; echo '$($package.helperSha256)  $remote/contents/deploy_web.py' | sha256sum --check; python3 -B '$remote/contents/deploy_web.py' verify --metadata-sha256 '$($package.metadataSha256)'"
 if($Action -eq 'Check'){Write-Host 'PASS private upload and inventory. Public website unchanged.';return}
 $operation=if($Action -eq 'Rollback'){'rollback'}else{'activate'}
 Write-Host 'Enter the VPS sudo password in this terminal only. Do not send it to chat.'
 Invoke-Ssh "set -eu; echo '$($package.helperSha256)  $remote/contents/deploy_web.py' | sha256sum --check; sudo -- python3 -B '$remote/contents/deploy_web.py' $operation --metadata-sha256 '$($package.metadataSha256)'" -Interactive
 [ordered]@{id=$package.id;action=$operation;status='PASS';completedUtc=[DateTime]::UtcNow.ToString('o');independentPublicVerificationRequired=$true}|ConvertTo-Json|Set-Content -LiteralPath ('operator-'+$operation+'-result.json') -Encoding UTF8
 Write-Host ('PASS '+$operation+'. Independent HTTPS verification remains required.')
} catch { Write-Host ('FAILED: '+$_.Exception.Message);exit 1 }
finally { Pop-Location }
