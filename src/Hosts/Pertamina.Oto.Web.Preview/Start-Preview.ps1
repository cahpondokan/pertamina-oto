param([ValidateRange(1024,65535)][int]$Port = 5266, [switch]$Website)
$ErrorActionPreference = 'Stop'
Push-Location -LiteralPath $PSScriptRoot
try {
    dotnet restore './Pertamina.Oto.Web.Preview.csproj' --configfile './NuGet.Config'
    if ($LASTEXITCODE -ne 0) { throw 'Restore framework lokal gagal. Pastikan .NET SDK 10.0.400 tersedia.' }
    dotnet build './Pertamina.Oto.Web.Preview.csproj' --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build gagal.' }
    Write-Host "Pratinjau lokal: http://127.0.0.1:$Port/"
    Write-Host 'Buka alamat di browser setelah server aktif. Tekan Ctrl+C untuk berhenti.'
    $previewArguments = @('run', '--no-build', '--no-launch-profile', '--project', './Pertamina.Oto.Web.Preview.csproj', '--urls', "http://127.0.0.1:$Port")
    if ($Website) { $previewArguments += '--Preview:CatalogOnly=true' }
    dotnet @previewArguments
} finally { Pop-Location }
