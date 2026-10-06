# Icon-only maintenance: retain launcher binaries, version pointers and shortcut target.
param([string]$InstallRoot = "$env:LOCALAPPDATA/Programs/PertaminaOTO",
      [string]$Desktop = [Environment]::GetFolderPath('Desktop'))
$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot '../src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico'
$shortcut = Join-Path $Desktop 'PERTAMINA OTO.lnk'
$launcher = Join-Path $InstallRoot 'PERTAMINA-OTO.exe'
if (!(Test-Path -LiteralPath $shortcut -PathType Leaf) -or !(Test-Path -LiteralPath $launcher -PathType Leaf)) { throw 'Existing installation/shortcut required.' }
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
if ([IO.Path]::GetFullPath($link.TargetPath) -ne [IO.Path]::GetFullPath($launcher)) { throw 'Shortcut target mismatch; no changes made.' }
$hash = (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
$icon = Join-Path $InstallRoot ("oto-icon-" + $hash.Substring(0,16) + '.ico')
if (Test-Path -LiteralPath $icon) {
 if ((Get-FileHash -LiteralPath $icon).Hash -ne $hash) { throw 'Existing icon hash mismatch.' }
} else { Copy-Item -LiteralPath $source -Destination $icon }
$backup = Join-Path $InstallRoot ('shortcut-before-icon-' + [Guid]::NewGuid().ToString('N') + '.lnk')
Copy-Item -LiteralPath $shortcut -Destination $backup
$link.IconLocation = "$icon,0"
$link.Save()
$check = $shell.CreateShortcut($shortcut)
if ($check.TargetPath -ne $launcher -or $check.IconLocation -ne "$icon,0") { throw 'Shortcut verification failed; backup retained.' }
Write-Output "Shortcut icon updated and verified: $icon"
Write-Output "Original shortcut backup: $backup"