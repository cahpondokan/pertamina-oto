# Standard delivery command after a completed, verified application change.
param([string]$MSBuild = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe',
    [switch]$SelfContained = $true,
    [string]$PreparedBuild,
    [string]$Feed = 'D:/AFT Ahmad Yani Semarang/Teknik/Dokumentasi Pertamina OTO/releases/windows-local')
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$notesTests = Join-Path $project 'tests/ReleaseNotesChecks/ReleaseNotesChecks.csproj'
& dotnet restore $notesTests -p:RestoreSources=
if ($LASTEXITCODE -ne 0) { throw 'Restore catatan rilis gagal. Tidak ada update diterbitkan.' }
& dotnet run --project $notesTests --no-restore -- (Join-Path $project ('artifacts/release-notes-checks-' + [Guid]::NewGuid().ToString('N')))
if ($LASTEXITCODE -ne 0) { throw 'Regression catatan rilis gagal. Tidak ada update diterbitkan.' }
$productTests = Join-Path $project 'tests/ProductVersionChecks/ProductVersionChecks.csproj'
& dotnet restore $productTests -p:RestoreSources=
if ($LASTEXITCODE -ne 0) { throw 'Restore pemeriksaan versi produk gagal. Tidak ada update diterbitkan.' }
& dotnet run --project $productTests --no-restore -- (Join-Path $project ('artifacts/product-version-checks-' + [Guid]::NewGuid().ToString('N')))
if ($LASTEXITCODE -ne 0) { throw 'Regression versi produk gagal. Tidak ada update diterbitkan.' }
& (Join-Path $PSScriptRoot 'Test-ProductVersionPackage.ps1') -Evidence (Join-Path $project ('artifacts/product-package-checks-' + [Guid]::NewGuid().ToString('N')))
$distributionTests = Join-Path $project 'tests/DistributionChecks/DistributionChecks.csproj'
& dotnet restore $distributionTests -p:RestoreSources=
if ($LASTEXITCODE -ne 0) { throw 'Restore pemeriksaan distribusi gagal. Tidak ada update diterbitkan.' }
& dotnet run --project $distributionTests --no-restore -- (Join-Path $project ('artifacts/distribution-checks-' + [Guid]::NewGuid().ToString('N')))
if ($LASTEXITCODE -ne 0) { throw 'Regression distribusi/update gagal. Tidak ada update diterbitkan.' }
$output = if ($PreparedBuild) { (Resolve-Path -LiteralPath $PreparedBuild).Path } else { Join-Path $project ('artifacts/update-build-' + [Guid]::NewGuid().ToString('N')) }
if ($PreparedBuild) {
    if (!$SelfContained) { throw 'Prepared delivery requires self-contained verification.' }
    foreach ($component in @(@{Folder='';Assembly='Pertamina.Oto.Desktop.App'},@{Folder='worker';Assembly='Pertamina.Oto.LocalPdf.Worker'},@{Folder='word-worker';Assembly='Pertamina.Oto.WordPdf.Worker'},@{Folder='pdf-opener';Assembly='PertaminaOTO.PdfOpener'},@{Folder='restart-launcher';Assembly='PERTAMINA-OTO'})) {
        & "$PSScriptRoot/Test-SelfContainedPayload.ps1" -Directory (Join-Path $output $component.Folder) -AssemblyName $component.Assembly
    }
} elseif ($SelfContained) {
    & "$PSScriptRoot/Build-SelfContainedApp.ps1" -Output $output -MSBuild $MSBuild
} else {
    & $MSBuild (Join-Path $project 'src/Hosts/Pertamina.Oto.Desktop.App/Pertamina.Oto.Desktop.App.csproj') /t:Rebuild /p:Configuration=Debug "/p:DesktopUiOutputPath=$output/" /v:minimal /nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build gagal. Tidak ada update diterbitkan.' }
}
$notes = Get-Content -LiteralPath "$output/Assets/release-notes.json" -Raw | ConvertFrom-Json
$product = Get-Content -LiteralPath "$output/product-info.json" -Raw | ConvertFrom-Json
if ($notes.Schema -ne 1 -or $notes.ProductVersion -cne $product.ProductVersion) { throw 'Versi catatan rilis tidak cocok. Tidak ada update diterbitkan.' }
& "$output/worker/Pertamina.Oto.LocalPdf.Worker.exe" --self-test
if ($LASTEXITCODE -ne 0) { throw 'Worker regression gagal. Tidak ada update diterbitkan.' }
& "$output/worker/Pertamina.Oto.LocalPdf.Worker.exe" --self-test-editor
if ($LASTEXITCODE -ne 0) { throw 'Editor regression gagal. Tidak ada update diterbitkan.' }
& "$output/worker/Pertamina.Oto.LocalPdf.Worker.exe" --self-test-search
if ($LASTEXITCODE -ne 0) { throw 'Search regression gagal. Tidak ada update diterbitkan.' }
$wordResults = @(& "$output/word-worker/Pertamina.Oto.WordPdf.Worker.exe" --self-test)
$wordExit = $LASTEXITCODE
$wordResults | Write-Output
if ($wordExit -ne 0 -or @($wordResults | Where-Object { $_ -like 'PASS *' }).Count -lt 27) { throw 'Word regression gagal atau tidak ditemukan. Tidak ada update diterbitkan.' }
$wordFixture = @($wordResults | Where-Object { $_ -like 'EVIDENCE *' })[-1].Substring(9)
$wordProcess = Start-Process -FilePath "$output/Pertamina.Oto.Desktop.App.exe" -ArgumentList @('--audit-word',('"' + $wordFixture + '"')) -WindowStyle Hidden -PassThru -Wait
if ($wordProcess.ExitCode -ne 0) { throw 'Word UI audit gagal. Tidak ada update diterbitkan.' }
$wordEvidence = Get-Content -LiteralPath "$output/word-audit-path.txt"
$wordAudit = Get-Content -LiteralPath (Join-Path $wordEvidence 'audit.txt')
if (@($wordAudit | Where-Object { $_ -like 'PASS *' }).Count -lt 39 -or @($wordAudit | Where-Object { $_ -like 'FAIL *' }).Count -gt 0) { throw 'Word UI evidence gagal atau tidak ditemukan. Tidak ada update diterbitkan.' }
$qualityProcess = Start-Process -FilePath "$output/Pertamina.Oto.Desktop.App.exe" -ArgumentList '--audit-quality' -WindowStyle Hidden -PassThru -Wait
if ($qualityProcess.ExitCode -ne 0) { throw 'Quality audit gagal. Tidak ada update diterbitkan.' }
$qualityEvidence = Get-Content -LiteralPath "$output/quality-audit-path.txt"
$qualityResults = Get-Content -LiteralPath (Join-Path $qualityEvidence 'audit.txt')
if (@($qualityResults | Where-Object { $_ -like 'PASS *' }).Count -lt 30 -or @($qualityResults | Where-Object { $_ -like 'FAIL *' }).Count -gt 0) { throw 'Quality regression gagal atau tidak ditemukan. Tidak ada update diterbitkan.' }
$associationEvidence = Join-Path $project 'artifacts/pdf-association-verification'
$associationTest = Start-Process -FilePath "$output/pdf-opener/PertaminaOTO.PdfOpener.exe" -ArgumentList @('--self-test',('"' + $associationEvidence + '"')) -WindowStyle Hidden -PassThru -Wait
if ($associationTest.ExitCode -ne 0) { throw 'PDF association regression gagal. Tidak ada update diterbitkan.' }
& (Join-Path $PSScriptRoot 'Publish-LocalUpdate.ps1') -BuildOutput $output -Feed $Feed

