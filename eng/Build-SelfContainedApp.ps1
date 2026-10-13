param([Parameter(Mandatory)][string]$Output, [string]$MSBuild = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe')
$ErrorActionPreference = 'Stop'
$Output = [IO.Path]::GetFullPath($Output)
$root = Split-Path $PSScriptRoot -Parent
if (Test-Path -LiteralPath $Output) { throw 'Use a fresh isolated output directory.' }
& "$PSScriptRoot/Publish-SelfContainedComponent.ps1" -ProjectFile "$root/src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj" -Output $Output -MSBuild $MSBuild -Desktop
foreach ($item in @(
    @{ Project='src/Workers/Pertamina.Oto.LocalPdf.Worker/Pertamina.Oto.LocalPdf.Worker.csproj'; Folder='worker'; Assembly='Pertamina.Oto.LocalPdf.Worker' },
    @{ Project='src/Workers/Pertamina.Oto.WordPdf.Worker/Pertamina.Oto.WordPdf.Worker.csproj'; Folder='word-worker'; Assembly='Pertamina.Oto.WordPdf.Worker' },
    @{ Project='eng/PdfFileLauncher/PdfFileLauncher.csproj'; Folder='pdf-opener'; Assembly='PertaminaOTO.PdfOpener' },
    @{ Project='eng/LocalLauncher/LocalLauncher.csproj'; Folder='restart-launcher'; Assembly='PERTAMINA-OTO' }
)) {
    $target = Join-Path $Output $item.Folder
    & "$PSScriptRoot/Publish-SelfContainedComponent.ps1" -ProjectFile (Join-Path $root $item.Project) -Output $target -MSBuild $MSBuild
    & "$PSScriptRoot/Test-SelfContainedPayload.ps1" -Directory $target -AssemblyName $item.Assembly
}
# Launcher contract excludes symbols, which are not required at runtime.
$launcher = Join-Path $Output 'restart-launcher'
Get-ChildItem -LiteralPath $launcher -File -Filter '*.pdb' | ForEach-Object { Remove-Item -LiteralPath $_.FullName }
$process = Start-Process -FilePath "$launcher/PERTAMINA-OTO.exe" -ArgumentList '--write-restart-capability' -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Self-contained launcher contract generation failed.' }
& "$PSScriptRoot/Test-SelfContainedPayload.ps1" -Directory $Output -AssemblyName 'Pertamina.Oto.Desktop.App'
Copy-Item -LiteralPath "$Output-build/product-info.json" -Destination "$Output/product-info.json"
