param([Parameter(Mandatory)][string]$Output,
      [string]$Compiler = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/Roslyn/csc.exe')
$ErrorActionPreference = 'Stop'
$Output = [IO.Path]::GetFullPath($Output)
New-Item -ItemType Directory -Path $Output -Force | Out-Null
$exe = Join-Path $Output 'Uninstall-OTO.exe'
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$project = Split-Path $PSScriptRoot -Parent
$options = @('/nologo','/nostdlib+','/langversion:7.3','/target:winexe','/platform:x64','/optimize+','/deterministic+','/warnaserror+',('/out:"' + $exe + '"'),('/win32manifest:"' + (Join-Path $PSScriptRoot 'WindowsSetup/app.manifest') + '"'),('/win32icon:"' + (Join-Path $project 'src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico') + '"'))
foreach ($name in @('mscorlib','System','System.Core','System.Windows.Forms','System.Drawing','System.Web.Extensions','Microsoft.CSharp')) { $options += '/reference:"' + (Join-Path $framework "$name.dll") + '"' }
$options += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'WindowsUninstall') -Filter '*.cs' -File | ForEach-Object { '"' + $_.FullName + '"' })
[IO.File]::WriteAllLines((Join-Path $Output 'compiler.rsp'), $options, [Text.UTF8Encoding]::new($false))
& $Compiler /noconfig ('@' + (Join-Path $Output 'compiler.rsp'))
if ($LASTEXITCODE -ne 0) { throw 'Uninstaller compilation failed.' }
[IO.File]::WriteAllText(($exe + '.config'), '<?xml version="1.0"?><configuration><startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/></startup></configuration>')
$check = Start-Process -FilePath $exe -ArgumentList @('--self-test',('"' + (Join-Path $Output ('tests-' + [Guid]::NewGuid().ToString('N'))) + '"')) -WindowStyle Hidden -PassThru -Wait
if ($check.ExitCode -ne 0) { throw 'Uninstaller regression failed. See tests/results.txt.' }
Write-Output "Uninstaller verified: $exe"
