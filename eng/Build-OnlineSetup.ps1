param(
    [Parameter(Mandatory)][string]$OfflineDirectory,
    [Parameter(Mandatory)][string]$Output,
    [string]$Compiler = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/Roslyn/csc.exe'
)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$Output = [IO.Path]::GetFullPath($Output)
$OfflineDirectory = (Resolve-Path -LiteralPath $OfflineDirectory).Path
$active = [IO.Path]::GetFullPath((Join-Path $project '../../releases/windows-local'))
if ($Output.Equals($active,[StringComparison]::OrdinalIgnoreCase) -or $Output.StartsWith($active + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Online candidate must not target active feed.' }
if (Test-Path -LiteralPath (Join-Path $Output 'distribution-v2.json')) { throw 'Immutable output exists.' }
$catalog = Get-Content -LiteralPath (Join-Path $OfflineDirectory 'current.json') -Raw | ConvertFrom-Json
if (!$catalog.runtimeBundled -or $catalog.version -cnotmatch '^\d{8}-\d{6}-[a-f0-9]{8}$' -or $catalog.productVersion -cnotmatch '^\d+\.\d+\.\d+$') { throw 'Self-contained release identity required.' }
foreach ($artifact in @(@{name=$catalog.fileName;size=$catalog.size;hash=$catalog.sha256;expected="PERTAMINA-OTO-Windows-x64-$($catalog.version).zip"},@{name=$catalog.installerFileName;size=$catalog.installerSize;hash=$catalog.installerSha256;expected="PERTAMINA-OTO-Setup-x64-$($catalog.version).exe"})) {
    if ($artifact.name -cne $artifact.expected) { throw 'Artifact name mismatch.' }
    $path=Join-Path $OfflineDirectory $artifact.name
    if ((Get-Item -LiteralPath $path).Length -ne $artifact.size -or (Get-FileHash -LiteralPath $path).Hash -ne $artifact.hash) { throw 'Artifact bytes/hash mismatch.' }
}
$zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $OfflineDirectory $catalog.fileName))
try {
    $expanded = [long]($zip.Entries | Measure-Object Length -Sum).Sum
    $entry = $zip.GetEntry('feed/latest.json'); if (!$entry) { throw 'Missing feed.' }
    $reader = [IO.StreamReader]::new($entry.Open()); try { $release=$reader.ReadToEnd() | ConvertFrom-Json } finally { $reader.Dispose() }
    if ($release.Version -cne $catalog.version) { throw 'Feed identity mismatch.' }
    $entry = $zip.GetEntry("feed/$($release.Version)/manifest.json"); if (!$entry) { throw 'Missing manifest.' }
    $stream=$entry.Open(); $sha=[Security.Cryptography.SHA256]::Create(); try { $manifestHash=[Convert]::ToHexString($sha.ComputeHash($stream)) } finally { $stream.Dispose(); $sha.Dispose() }
    if ($manifestHash -ne $release.ManifestSha256) { throw 'Manifest binding mismatch.' }
} finally { $zip.Dispose() }
if ($expanded -le 0 -or $expanded -gt 2GB) { throw 'Expanded budget invalid.' }
$build=Join-Path $project ('artifacts/distribution-refresh-app-20261011/online-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $build -Force | Out-Null
$generated=@"
namespace Oto.OnlineSetup { internal static class OnlineInfo {
internal const string ProductVersion = "$($catalog.productVersion)";
internal const string Url = "https://oto.ayani.id/downloads/$($catalog.installerFileName)";
internal const string Sha256 = "$($catalog.installerSha256)";
internal const long Size = $($catalog.installerSize)L;
internal const long ExpandedBytes = ${expanded}L;
} }
"@
[IO.File]::WriteAllText("$build/OnlineInfo.cs",$generated,[Text.UTF8Encoding]::new($false))
$name="PERTAMINA-OTO-OnlineSetup-x64-$($catalog.version).exe"; $exe=Join-Path $build $name
if (($exe + '.config').Length -ge 260) { throw 'CLR4 bootstrap activation needs a shorter build evidence path (including .config under 260 characters).' }
$framework=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$options=@('/nologo','/nostdlib+','/langversion:7.3','/target:winexe','/platform:anycpu','/optimize+','/deterministic+','/warnaserror+',('/out:"'+$exe+'"'),('/win32manifest:"'+$PSScriptRoot+'/WindowsSetup/app.manifest"'),('/win32icon:"'+$project+'/src/Hosts/Pertamina.Oto.Desktop.App/Assets/oto.ico"'))
foreach($reference in @('mscorlib','System','System.Core','System.Net.Http','System.Windows.Forms','System.Drawing','System.Security')) { $options+='/reference:"'+$framework+'/'+$reference+'.dll"' }
$options+='"'+$build+'/OnlineInfo.cs"'; $options+=@(Get-ChildItem "$PSScriptRoot/OnlineSetup" -File -Filter '*.cs' | ForEach-Object {'"'+$_.FullName+'"'})
[IO.File]::WriteAllLines("$build/compiler.rsp",$options,[Text.UTF8Encoding]::new($false))
& $Compiler /noconfig ('@'+$build+'/compiler.rsp'); if($LASTEXITCODE -ne 0){throw 'Online bootstrap compilation failed.'}
foreach($gate in @(@{mode='--self-test';folder='tests'},@{mode='--audit-ui';folder='ui'})) {
    $process=Start-Process -FilePath $exe -ArgumentList @($gate.mode,('"'+$build+'/'+$gate.folder+'"')) -WindowStyle Hidden -PassThru -Wait
    if($process.ExitCode -ne 0){throw "Online gate failed: $($gate.mode); $build"}
}
$downloadGate=Start-Process -FilePath $exe -ArgumentList @('--verify-local-download',('"'+(Join-Path $OfflineDirectory $catalog.installerFileName)+'"'),('"'+$build+'/actual-download"')) -WindowStyle Hidden -PassThru -Wait
if($downloadGate.ExitCode -ne 0){throw 'Actual pinned offline installer download gate failed.'}
New-Item -ItemType Directory -Path $Output -Force | Out-Null
if(Test-Path -LiteralPath (Join-Path $Output $name)){throw 'Immutable online installer exists.'}
$v2=@{schema=2;product='PERTAMINA OTO';productVersion=$catalog.productVersion;version=$catalog.version;publishedAt=$catalog.publishedAt;channel='development';architecture='x64';runtimeBundled=$true;updaterProtocol=2;minimumUpdaterBaseline=1;manifestSha256=$manifestHash;expandedBytes=$expanded;notes=$catalog.notes;onlineInstaller=@{fileName=$name;size=(Get-Item $exe).Length;sha256=(Get-FileHash $exe).Hash};offlineInstaller=@{fileName=$catalog.installerFileName;size=$catalog.installerSize;sha256=$catalog.installerSha256};updatePayload=@{fileName=$catalog.fileName;size=$catalog.size;sha256=$catalog.sha256}}
if($catalog.releaseNotesJson){$v2.releaseNotesJson=$catalog.releaseNotesJson;$v2.releaseNotesSha256=$catalog.releaseNotesSha256}
$validatedCatalog = Join-Path $build 'distribution-v2.json'
[IO.File]::WriteAllText($validatedCatalog,($v2|ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
& dotnet run --project (Join-Path $project 'tests/DistributionChecks') --no-restore -- --validate-v2 $validatedCatalog
if ($LASTEXITCODE -ne 0) { throw 'Generated schema2 metadata gate failed before output commit.' }
Copy-Item -LiteralPath $exe -Destination (Join-Path $Output $name)
Copy-Item -LiteralPath $validatedCatalog -Destination (Join-Path $Output 'distribution-v2.json')
@{status='LOCAL_CANDIDATE';build=$catalog.version;onlineBytes=$v2.onlineInstaller.size;downloadBytes=$catalog.installerSize;manifestSha256=$manifestHash;evidence=$build;installed=$false;published=$false}|ConvertTo-Json|Set-Content (Join-Path $Output 'online-verification.json')
Write-Output "Online installer: $(Join-Path $Output $name)"; Write-Output "Evidence: $build"
