Set-StrictMode -Version Latest
function Assert-PrivateDirectory([string]$Path) {
    if (-not [IO.Path]::IsPathFullyQualified($Path) -or $Path.StartsWith('\\')) { throw 'LOCAL_ABSOLUTE_REQUIRED' }
    $full=[IO.Path]::GetFullPath($Path).TrimEnd('\')
    if ($full.Length -lt 10 -or -not [IO.Directory]::Exists($full)) { throw 'DEDICATED_DIRECTORY_REQUIRED' }
    for($p=$full; $p; $p=[IO.Path]::GetDirectoryName($p)) {
        if (([IO.File]::GetAttributes($p) -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'REPARSE_REJECTED' }
    }
    foreach($item in Get-ChildItem -LiteralPath $full -Force -Recurse -ErrorAction Stop) {
        if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) { throw 'REPARSE_REJECTED' }
    }
    return $full
}
function New-SmokeConfiguration([string]$InputRoot,[string]$ExportRoot) {
    $inputPath=Assert-PrivateDirectory $InputRoot
    $exportPath=Assert-PrivateDirectory $ExportRoot
    if ([IO.Path]::GetFileName($inputPath) -cne 'input' -or [IO.Path]::GetFileName($exportPath) -cne 'export') { throw 'ROOT_NAMES' }
    if ([IO.Path]::GetDirectoryName($inputPath) -cne [IO.Path]::GetDirectoryName($exportPath)) { throw 'RUN_SIBLINGS_REQUIRED' }
    if (@(Get-ChildItem -LiteralPath $exportPath -Force).Count -ne 0) { throw 'EXPORT_NOT_EMPTY' }
    $i=[Security.SecurityElement]::Escape($inputPath)
    $e=[Security.SecurityElement]::Escape($exportPath)
    return "<Configuration><Networking>Disable</Networking><ClipboardRedirection>Disable</ClipboardRedirection><PrinterRedirection>Disable</PrinterRedirection><AudioInput>Disable</AudioInput><VideoInput>Disable</VideoInput><vGPU>Disable</vGPU><ProtectedClient>Enable</ProtectedClient><MemoryInMB>4096</MemoryInMB><MappedFolders><MappedFolder><HostFolder>$i</HostFolder><SandboxFolder>C:\PocInput</SandboxFolder><ReadOnly>true</ReadOnly></MappedFolder><MappedFolder><HostFolder>$e</HostFolder><SandboxFolder>C:\PocExport</SandboxFolder><ReadOnly>false</ReadOnly></MappedFolder></MappedFolders></Configuration>"
}
function ConvertTo-SafeRecord([string]$CaseId,[string]$Status,[int]$ExitCode,[long]$ElapsedMs,[string]$OutputSha256) {
    if ($CaseId -cnotin @('AB','CAB','AA','AC') -or $Status -cnotin @('EXITED','TIMEOUT','FAILED') -or $ElapsedMs -lt 0 -or $OutputSha256 -cnotmatch '^[A-F0-9]{64}$') { throw 'INVALID_RECORD' }
    [ordered]@{schema=1;caseId=$CaseId;status=$Status;exitCode=$ExitCode;elapsedMs=$ElapsedMs;outputSha256=$OutputSha256;correctness='NOT_EVALUATED'} | ConvertTo-Json -Compress
}
function Set-CleanChildEnvironment([Diagnostics.ProcessStartInfo]$Info,[string]$CaseRoot) {
    if ($CaseRoot -cnotmatch '^C:\\PocPrivate\\(AB|CAB|AA|AC)$') { throw 'CASE_ENV_SCOPE' }
    $Info.EnvironmentVariables.Clear()
    $values=@{SystemRoot='C:\Windows';WINDIR='C:\Windows';TEMP=$CaseRoot;TMP=$CaseRoot;
        DOTNET_ROOT='C:\PocInput\dotnet';DOTNET_MULTILEVEL_LOOKUP='0';DOTNET_ROLL_FORWARD='Disable';DOTNET_CLI_TELEMETRY_OPTOUT='1'}
    foreach($key in $values.Keys) { $Info.EnvironmentVariables[$key]=$values[$key] }
}
function ConvertTo-StagingManifest($Bundle,$Supplement) {
    if($Bundle.schemaVersion -ne 1 -or $Bundle.status -cne 'BUILT_NOT_EXECUTED' -or $Bundle.engineExecuted -isnot [bool] -or $Bundle.engineExecuted -or $Bundle.entryPoint -cne 'dotnet/dotnet.exe' -or $Bundle.runner -cne 'app/MergeSmoke.dll' -or $Bundle.inputLockSha256 -cnotmatch '^[A-F0-9]{64}$' -or @($Bundle.files).Count -eq 0 -or $Supplement.schema -ne 1) { throw 'BUNDLE_SCHEMA' }
    $seen=@{}; $files=@()
    foreach($entry in @($Bundle.files)+@($Supplement.files)) {
        $name=[string]$entry.path
        if($name -cnotmatch '^[A-Za-z0-9_-]+(?:/[A-Za-z0-9_.-]+)*$' -or $name.Contains('..') -or $seen.ContainsKey($name) -or $entry.sha256 -cnotmatch '^[A-F0-9]{64}$') { throw 'MANIFEST_ENTRY' }
        $seen[$name]=$true; $files += [ordered]@{path=$name;sha256=[string]$entry.sha256}
    }
    if(-not $seen.ContainsKey('dotnet/dotnet.exe') -or -not $seen.ContainsKey('app/MergeSmoke.dll')) { throw 'ENTRYPOINT_MISSING' }
    [ordered]@{schema=1;files=@($files | Sort-Object { $_.path })} | ConvertTo-Json -Depth 5
}
Export-ModuleMember -Function Assert-PrivateDirectory,New-SmokeConfiguration,ConvertTo-SafeRecord,Set-CleanChildEnvironment,ConvertTo-StagingManifest
