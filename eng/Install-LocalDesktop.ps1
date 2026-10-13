param([Parameter(Mandatory=$true)][string]$LauncherOutput,
      [Parameter(Mandatory=$true)][string]$InstallRoot,
      [Parameter(Mandatory=$true)][string]$Desktop,
      [string]$Feed = 'D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local')
$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $InstallRoot) { throw 'Instalasi sudah ada. Gunakan tombol Update; installer awal tidak menimpa instalasi.' }
if (!(Test-Path -LiteralPath $Desktop -PathType Container)) { throw 'Folder Desktop tidak ditemukan.' }
$shortcut = Join-Path $Desktop 'PERTAMINA OTO.lnk'
if (Test-Path -LiteralPath $shortcut) { throw 'Shortcut sudah ada; tidak ditimpa.' }
New-Item -ItemType Directory -Path $InstallRoot | Out-Null
Get-ChildItem -LiteralPath $LauncherOutput -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $InstallRoot }
[IO.File]::WriteAllText((Join-Path $InstallRoot 'update-source.json'), (@{Folder=$Feed} | ConvertTo-Json), [Text.UTF8Encoding]::new($false))
$launcher = Join-Path $InstallRoot 'PERTAMINA-OTO.exe'
$process = Start-Process -FilePath $launcher -ArgumentList @('--install',('"' + $InstallRoot + '"')) -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0) { throw 'Pemasangan gagal; periksa launcher-error.txt. Shortcut belum dibuat.' }
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
$link.TargetPath = $launcher
$link.WorkingDirectory = $InstallRoot
$link.Description = 'PERTAMINA OTO - pengembangan lokal, dapat diperbarui dari Pengaturan'
$link.IconLocation = "$launcher,0"
$link.Save()
Write-Output "Terpasang: $launcher"
Write-Output "Shortcut: $shortcut"
