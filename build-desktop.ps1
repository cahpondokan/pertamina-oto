param([switch]$RestoreOffline, [switch]$Test)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$vsWherePath = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
if (-not (Test-Path -LiteralPath $vsWherePath)) { throw 'Visual Studio Build Tools belum ditemukan. Tidak ada instalasi otomatis.' }
$buildToolPath = & $vsWherePath -latest -version '[18.0,)' -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/MSBuild.exe' | Select-Object -First 1
if (-not $buildToolPath) { throw 'MSBuild Visual Studio 18+ diperlukan untuk SDK proyek ini. Tidak ada instalasi otomatis.' }
Push-Location $projectRoot
try {
    if ($RestoreOffline) {
        & dotnet restore Pertamina.Oto.Development.slnx --configfile dev-offline.config --packages packages
        if ($LASTEXITCODE -ne 0) { throw 'Restore offline gagal. Tidak mencoba sumber internet.' }
    }
    & $buildToolPath Pertamina.Oto.Development.slnx /t:Build /p:Configuration=Debug /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build gagal.' }
    if ($Test) {
        & dotnet run --project tests/Functional/Pertamina.Oto.LocalPdf.SmokeTests/Pertamina.Oto.LocalPdf.SmokeTests.csproj --no-restore --no-build
        if ($LASTEXITCODE -ne 0) { throw 'Pengujian integrasi gagal.' }
    }
    Write-Output (Join-Path $projectRoot 'artifacts/desktop/Pertamina.Oto.Desktop.App.exe')
} finally { Pop-Location }
