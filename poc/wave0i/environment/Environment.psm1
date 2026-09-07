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
Export-ModuleMember -Function Assert-PrivateDirectory,New-SmokeConfiguration,ConvertTo-SafeRecord
