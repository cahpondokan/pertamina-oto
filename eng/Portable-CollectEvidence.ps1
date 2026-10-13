param([Parameter(Mandatory=$true)][string]$EvidenceRoot)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$before = Get-Content -LiteralPath "$EvidenceRoot/source-before.json" -Raw | ConvertFrom-Json
$after = @($before.Files | ForEach-Object {
 $path = Join-Path $project $_.Path
 @{Path=$_.Path;Length=(Get-Item -LiteralPath $path).Length;Sha256=(Get-FileHash -LiteralPath $path).Hash}
})
$changed = @($before.Files | Where-Object {
 $item = $after | Where-Object Path -eq $_.Path
 $item.Sha256 -cne $_.Sha256 -or $item.Length -ne $_.Length
})
if (@($changed | Where-Object { $_.Path -notlike 'eng/portable/*' -and $_.Path -ne 'eng/Build-PortablePackage.ps1' }).Count) { throw 'Core source changed during shared build.' }
[IO.File]::WriteAllText("$EvidenceRoot/source-inventory.json",(@{Head=$before.Head;Dirty=$true;Files=$after;CoreUnchangedDuringBuild=$true} | ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
$runtime = 'C:/Program Files/dotnet'
$files = @()
foreach ($relative in @('dotnet.exe','LICENSE.txt','ThirdPartyNotices.txt','host/fxr/10.0.11','shared/Microsoft.NETCore.App/10.0.11')) {
 $item = Get-Item -LiteralPath (Join-Path $runtime $relative)
 $items = if ($item.PSIsContainer) { Get-ChildItem -LiteralPath $item.FullName -Recurse -File -Force } else { @($item) }
 foreach ($file in $items) { $files += @{Path=$file.FullName.Substring($runtime.Length+1).Replace('\','/');Length=$file.Length;Sha256=(Get-FileHash -LiteralPath $file.FullName).Hash} }
}
$signatures = @('dotnet.exe','host/fxr/10.0.11/hostfxr.dll','shared/Microsoft.NETCore.App/10.0.11/coreclr.dll') | ForEach-Object {
 $file = Join-Path $runtime $_; $signature = Get-AuthenticodeSignature -LiteralPath $file
 @{Path=$_;Status=[string]$signature.Status;SignerSubject=$signature.SignerCertificate.Subject;ProductVersion=[Diagnostics.FileVersionInfo]::GetVersionInfo($file).ProductVersion}
}
$candidate = @{Status='NOT_ADMITTED';Scope='PORTABLE-PUBLIC-DEV-20261002';Version='10.0.11';Provenance='Existing Microsoft-signed local install; original archive/metadata acquisition binding absent';License='Existing root LICENSE.txt Microsoft .NET Library terms; distribution obligations unresolved';Files=$files;Signatures=@($signatures);Bytes=($files|Measure-Object Length -Sum).Sum;SourceCommit=(Get-Content -LiteralPath "$runtime/shared/Microsoft.NETCore.App/10.0.11/.version")[0];NoAcquisition=$true;NoCopyToPublicBundle=$true}
[IO.File]::WriteAllText("$EvidenceRoot/runtime-candidate.json",($candidate | ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))
Write-Output "Core source unchanged during build. Runtime candidate files=$($files.Count); bytes=$($candidate.Bytes); status=NOT_ADMITTED."
