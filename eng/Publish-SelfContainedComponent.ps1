param([Parameter(Mandatory)][string]$ProjectFile, [Parameter(Mandatory)][string]$Output, [string]$MSBuild = 'D:/DevTools/VS2026Community/MSBuild/Current/Bin/MSBuild.exe', [switch]$Desktop)
$ErrorActionPreference = 'Stop'
$Output = [IO.Path]::GetFullPath($Output)
$ProjectFile = [IO.Path]::GetFullPath($ProjectFile)
$root = Split-Path $PSScriptRoot -Parent
$intake = Join-Path $root 'artifacts/self-contained-20261009/intake'
$admission = Get-Content -LiteralPath (Join-Path $intake 'verified-packages.json') -Raw | ConvertFrom-Json
foreach ($package in $admission.Packages) {
    if (!$package.SignatureVerified -or $package.Version -cne '10.0.12' -or (Get-FileHash -LiteralPath (Join-Path $intake "$($package.Id).$($package.Version).nupkg")).Hash -cne $package.Sha256) { throw 'Runtime intake not verified.' }
}
$argsList = @($ProjectFile, '/restore', '/t:Publish', '/p:Configuration=Debug', '/p:OtoSelfContained=true', "/p:RestorePackagesPath=$root/packages", "/p:PublishDir=$Output/", '/v:minimal', '/nologo')
if ($Desktop) { $argsList += "/p:DesktopUiOutputPath=$Output-build/" }
& $MSBuild @argsList
if ($LASTEXITCODE -ne 0) { throw "Self-contained publish failed: $ProjectFile" }
if ($Desktop) {
    # WinUI's unpackaged publish list omits generated XBF/PRI with the custom OutDir.
    # Preserve the exact native resource payload produced by this same build.
    foreach ($name in @('App.xbf','Pertamina.Oto.Desktop.App.pri')) {
        $resource = Get-Item -LiteralPath (Join-Path "$Output-build" $name)
        if ($resource.Length -eq 0) { throw "Empty WinUI resource: $name" }
        Copy-Item -LiteralPath $resource.FullName -Destination (Join-Path $Output $name) -Force
    }
}
# Preserve upstream redistribution notices verbatim from the signed admitted archive.
$zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $intake 'microsoft.netcore.app.runtime.win-x64.10.0.12.nupkg'))
try {
    foreach ($name in @('LICENSE.TXT','THIRD-PARTY-NOTICES.TXT')) {
        $entry = @($zip.Entries | Where-Object { $_.FullName -ieq $name })
        if ($entry.Count -ne 1) { throw "Missing runtime notice: $name" }
        [IO.Compression.ZipFileExtensions]::ExtractToFile($entry[0], (Join-Path $Output $name), $true)
    }
} finally { $zip.Dispose() }
