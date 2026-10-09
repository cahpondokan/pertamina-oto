param(
    [string]$PayloadZip,
    [string]$Output,
    [string[]]$Notes = @('Periksa, unduh, verifikasi dan pasang update melalui aplikasi.', 'Installer Windows dengan pemeriksaan .NET 10 dan jalur transisi aplikasi lama.', 'Panduan pemakaian dan pencarian PDF per kemunculan tersedia.'),
    [string]$Compiler = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/Roslyn/csc.exe'
)
$ErrorActionPreference = 'Stop'
if ($Notes.Count -lt 1 -or $Notes.Count -gt 8 -or @($Notes | Where-Object { !$_.Trim() -or $_.Length -gt 240 -or $_ -match '[\x00-\x1f\x7f]' }).Count) { throw 'Catatan rilis tidak valid.' }
$project = Split-Path $PSScriptRoot -Parent
if (!$Output) { $Output = Join-Path $project 'artifacts/windows-exe-distribution' }
$Output = [IO.Path]::GetFullPath($Output)
$build = Join-Path $project ('artifacts/windows-setup-build-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $build -Force | Out-Null
if (!$PayloadZip) {
    & (Join-Path $PSScriptRoot 'Build-DownloadPackage.ps1') -Output (Join-Path $build 'payload') -Notes $Notes
    $catalog = Get-Content -LiteralPath "$build/payload/current.json" -Raw | ConvertFrom-Json
    $PayloadZip = Join-Path "$build/payload" $catalog.fileName
} else {
    $PayloadZip = (Resolve-Path -LiteralPath $PayloadZip).Path
    $catalog = Get-Content -LiteralPath (Join-Path (Split-Path $PayloadZip -Parent) 'current.json') -Raw | ConvertFrom-Json
}
if ($catalog.version -cnotmatch '^[0-9]{8}-[0-9]{6}-[a-f0-9]{8}$' -or (Get-FileHash -LiteralPath $PayloadZip).Hash -ne $catalog.sha256) { throw 'Payload metadata/hash mismatch.' }
$archive = [IO.Compression.ZipFile]::OpenRead($PayloadZip)
try { $expanded = ($archive.Entries | Measure-Object -Property Length -Sum).Sum } finally { $archive.Dispose() }
if ($expanded -le 0 -or $expanded -gt 2GB) { throw 'Invalid expanded payload size.' }
$setupProductVersion = 'tidak tersedia (paket lama)'
if ($catalog.productVersion) {
    if ($catalog.productVersion -cnotmatch '^\d+\.\d+\.\d+$') { throw 'Invalid product version.' }
    $setupProductVersion = $catalog.productVersion
}
$generated = @"
namespace Oto.Setup {
internal static class PayloadInfo {
    internal const string ProductVersion = "$setupProductVersion";
    internal const string Version = "$($catalog.version)";
    internal const string Sha256 = "$($catalog.sha256)";
    internal const long ExpandedBytes = ${expanded}L;
}
}
"@
[IO.File]::WriteAllText("$build/PayloadInfo.cs", $generated, [Text.UTF8Encoding]::new($false))
$fileName = "PERTAMINA-OTO-Setup-x64-$($catalog.version).exe"
$exe = Join-Path $build $fileName
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$icon = Join-Path $project 'src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico'
$options = @('/nologo','/nostdlib+','/langversion:7.3','/target:winexe','/platform:anycpu','/optimize+','/deterministic+','/warnaserror+',('/out:"' + $exe + '"'),('/win32manifest:"' + (Join-Path $PSScriptRoot 'WindowsSetup/app.manifest') + '"'),('/win32icon:"' + $icon + '"'),('/resource:"' + $PayloadZip + '",OTO.Payload.zip'),('/resource:"' + $icon + '",OTO.Brand.ico'))
foreach ($name in @('mscorlib','System','System.Core','System.Net.Http','System.Windows.Forms','System.Drawing','System.Security','System.IO.Compression','System.IO.Compression.FileSystem')) { $options += '/reference:"' + (Join-Path $framework "$name.dll") + '"' }
$options += '"' + "$build/PayloadInfo.cs" + '"'
$options += @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'WindowsSetup') -Filter '*.cs' -File | ForEach-Object { '"' + $_.FullName + '"' })
[IO.File]::WriteAllLines("$build/compiler.rsp", $options, [Text.UTF8Encoding]::new($false))
& $Compiler /noconfig ('@' + "$build/compiler.rsp")
if ($LASTEXITCODE -ne 0) { throw 'Bootstrapper compilation failed.' }
$tests = Start-Process -FilePath $exe -ArgumentList @('--self-test',('"' + "$build/tests" + '"')) -WindowStyle Hidden -PassThru -Wait
if ($tests.ExitCode -ne 0) { Get-Content "$build/tests/test-results.txt" -Tail 14 -ErrorAction SilentlyContinue; throw 'Bootstrapper regression failed.' }
$verify = Start-Process -FilePath $exe -ArgumentList @('--verify-bundle',('"' + "$build/extracted" + '"')) -WindowStyle Hidden -PassThru -Wait
if ($verify.ExitCode -ne 0) { Get-Content "$build/extracted/bundle-error.txt" -ErrorAction SilentlyContinue; throw 'Embedded package verification failed.' }
& "$build/extracted/Setup.exe" --verify "$build/extracted"
if ($LASTEXITCODE -ne 0) { throw 'Embedded core installer validation failed.' }
$audit = Start-Process -FilePath $exe -ArgumentList @('--audit-ui',('"' + "$build/ui-audit" + '"')) -WindowStyle Hidden -PassThru -Wait
if ($audit.ExitCode -ne 0) { throw 'Native wizard audit failed.' }
New-Item -ItemType Directory -Path $Output -Force | Out-Null
if ((Test-Path -LiteralPath (Join-Path $Output $fileName)) -or (Test-Path -LiteralPath (Join-Path $Output $catalog.fileName))) { throw 'Immutable release output already exists; use another output or new release.' }
Copy-Item -LiteralPath $exe -Destination (Join-Path $Output $fileName)
Copy-Item -LiteralPath $PayloadZip -Destination (Join-Path $Output $catalog.fileName)
# Keep the real ZIP metadata for old notification clients; the web CTA uses the new installer fields.
$catalog.notes = $Notes
$catalog | Add-Member -NotePropertyName installerFileName -NotePropertyValue $fileName -Force
$catalog | Add-Member -NotePropertyName installerSize -NotePropertyValue (Get-Item -LiteralPath $exe).Length -Force
$catalog | Add-Member -NotePropertyName installerSha256 -NotePropertyValue (Get-FileHash -LiteralPath $exe).Hash -Force
$pending = Join-Path $Output ('current-' + [Guid]::NewGuid().ToString('N') + '.json')
[IO.File]::WriteAllText($pending, ($catalog | ConvertTo-Json -Depth 4), [Text.UTF8Encoding]::new($false))
$current = Join-Path $Output 'current.json'
if (Test-Path -LiteralPath $current) { [IO.File]::Replace($pending, $current, (Join-Path $Output 'previous.json')) } else { [IO.File]::Move($pending, $current) }
Write-Output "Installer: $(Join-Path $Output $fileName)"
Write-Output "Evidence: $build"
Write-Output "SHA256: $($catalog.installerSha256)"
