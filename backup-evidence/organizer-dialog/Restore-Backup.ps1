param(
    [Parameter(Mandatory)][string]$BaseZip,
    [Parameter(Mandatory)][string]$RecoveryZip,
    [Parameter(Mandatory)][string]$ExpectedRecoverySha256,
    [Parameter(Mandatory)][string]$Output
)
$ErrorActionPreference='Stop'
$baseHash='DF59A98415EA9BA77B5ABAFA5A3EF5501345BE8F9C6E3EF0D7FB9992D0A62BC8'
$manifestHash='2C2C71B84813A980810A9B5435DA71BF7C859D8FE4C285E147B870F4D33A6C4C'
if((Get-FileHash -LiteralPath $BaseZip).Hash -ne $baseHash){throw 'Base archive SHA256 mismatch'}
if((Get-FileHash -LiteralPath $RecoveryZip).Hash -ne $ExpectedRecoverySha256){throw 'Recovery archive SHA256 mismatch'}
$restoreRoot=[IO.Path]::GetFullPath($Output)
if(Test-Path -LiteralPath $restoreRoot){throw 'Output must be a fresh directory; existing files are never overwritten'}
New-Item -ItemType Directory -Path $restoreRoot | Out-Null
function Extract-PackageEntries([string]$ZipPath){
    $archive=[IO.Compression.ZipFile]::OpenRead([IO.Path]::GetFullPath($ZipPath))
    try {
        foreach($entry in $archive.Entries){
            $name=$entry.FullName.Replace([char]92,[char]47)
            if($name -ne 'manifest.json' -and -not $name.StartsWith('app/')){continue}
            if($name.Contains(':') -or $name.StartsWith('/') -or ($name -split '/') -contains '..'){throw 'Unsafe archive entry'}
            $target=[IO.Path]::GetFullPath((Join-Path $restoreRoot $name))
            if(-not $target.StartsWith($restoreRoot+'\',[StringComparison]::OrdinalIgnoreCase)){throw 'Archive path escaped output'}
            if($name.EndsWith('/')){New-Item -ItemType Directory -Path $target -Force | Out-Null;continue}
            New-Item -ItemType Directory -Path (Split-Path $target -Parent) -Force | Out-Null
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$target,$true)
        }
    }finally{$archive.Dispose()}
}
Extract-PackageEntries $BaseZip
Extract-PackageEntries $RecoveryZip
$manifestPath=Join-Path $restoreRoot 'manifest.json'
if((Get-FileHash -LiteralPath $manifestPath).Hash -ne $manifestHash){throw 'Final manifest mismatch'}
$manifest=Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if($manifest.Version -ne '20261013-142328-c5c96e5f' -or $manifest.ProductVersion -ne '0.3.0' -or $manifest.Channel -ne 'Pengembangan'){throw 'Final identity mismatch'}
foreach($file in $manifest.Files){
    $target=[IO.Path]::GetFullPath((Join-Path (Join-Path $restoreRoot 'app') $file.Path))
    if(-not $target.StartsWith($restoreRoot+'\app\',[StringComparison]::OrdinalIgnoreCase)){throw 'Unsafe manifest path'}
    if(-not(Test-Path -LiteralPath $target) -or (Get-Item -LiteralPath $target).Length -ne $file.Length -or (Get-FileHash -LiteralPath $target).Hash -ne $file.Sha256){throw ('Recovered payload mismatch: '+$file.Path)}
}
if(@(Get-ChildItem -LiteralPath (Join-Path $restoreRoot 'app') -File -Recurse).Count -ne $manifest.Files.Count){throw 'Unexpected extra payload files'}
Write-Output "RECOVERY_PASS=$($manifest.Version);FILES=$($manifest.Files.Count)"
Write-Output 'Recovery writes a fresh folder only; it does not install, activate or modify the current application.'
