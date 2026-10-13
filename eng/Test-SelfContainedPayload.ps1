param([Parameter(Mandatory)][string]$Directory, [Parameter(Mandatory)][string]$AssemblyName)
$ErrorActionPreference = 'Stop'
$config = Get-Content -LiteralPath (Join-Path $Directory "$AssemblyName.runtimeconfig.json") -Raw | ConvertFrom-Json
$runtime = $config.runtimeOptions
if ($runtime.framework -or $runtime.frameworks -or !@($runtime.includedFrameworks | Where-Object { $_.name -eq 'Microsoft.NETCore.App' -and $_.version -eq '10.0.12' }).Count) { throw "Not a pinned self-contained payload: $AssemblyName" }
foreach ($name in @("$AssemblyName.exe", "$AssemblyName.dll", "$AssemblyName.deps.json", 'coreclr.dll','hostfxr.dll','hostpolicy.dll','System.Private.CoreLib.dll','LICENSE.TXT','THIRD-PARTY-NOTICES.TXT')) {
    $file = Get-Item -LiteralPath (Join-Path $Directory $name)
    if ($file.Length -le 0 -or ($file.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Invalid payload file: $name" }
}
if ($AssemblyName -ceq 'Pertamina.Oto.Desktop.App') {
    foreach ($name in @('App.xbf','Pertamina.Oto.Desktop.App.pri')) {
        if ((Get-Item -LiteralPath (Join-Path $Directory $name)).Length -le 0) { throw "Missing WinUI resource: $name" }
    }
}
Write-Output "PASS self-contained $AssemblyName · runtime 10.0.12 · private files and notices present"
