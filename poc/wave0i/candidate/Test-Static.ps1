#requires -Version 7.0
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/CandidateTools.ps1"
$results = [Collections.Generic.List[object]]::new()
function Check([string] $Name, [scriptblock] $Body) {
    & $Body
    $results.Add(@{name=$Name;status='PASS'})
}
function MustReject([scriptblock] $Body, [string] $Expected) {
    try { & $Body | Out-Null } catch {
        if ($_.Exception.Message -like ('*' + $Expected + '*')) { return }
        throw
    }
    throw "EXPECTED_REJECTION:$Expected"
}
Check 'PowerShell syntax for both executable sources and tests' {
    foreach ($name in @('CandidateTools.ps1', 'Build-Offline.ps1', 'Test-Static.ps1')) {
        $tokens=$null; $errors=$null
        $null=[Management.Automation.Language.Parser]::ParseFile("$PSScriptRoot/$name", [ref]$tokens, [ref]$errors)
        if ($errors.Count) { throw 'SYNTAX_ERROR' }
    }
}
Check 'path traversal, rooted, alternate stream and UNC rejected' {
    foreach ($path in @('../outside', 'a/../outside', '/root', 'C:/root', 'a:stream', '\\server\share')) {
        MustReject { Get-SafePath $PSScriptRoot $path } 'UNSAFE_RELATIVE_PATH'
    }
    MustReject { Assert-NoReparse '\\server\share' } 'LOCAL_DRIVE_REQUIRED'
}
Check 'hash and length mismatch rejected' {
    $bytes=[Text.Encoding]::UTF8.GetBytes('benign text fixture')
    Assert-Bytes $bytes ([pscustomobject]@{sha256=(Get-BytesHash $bytes);bytes=$bytes.Length})
    MustReject { Assert-Bytes $bytes ([pscustomobject]@{sha256=('0'*64);bytes=$bytes.Length}) } 'HASH_MISMATCH'
    MustReject { Assert-Bytes $bytes ([pscustomobject]@{sha256=(Get-BytesHash $bytes);bytes=1}) } 'SIZE_MISMATCH'
}
Check 'missing existing asset fails closed' {
    MustReject { Read-BoundFile (Get-SafePath $PSScriptRoot 'deliberately-missing.asset') ([pscustomobject]@{sha256=('0'*64)}) } 'MISSING_FILE'
}
Check 'ZIP selected member verified; missing and duplicate entries rejected' {
    # Text-only in-memory ZIP, never a PDF or executable fixture.
    $memory=[IO.MemoryStream]::new()
    $write=[IO.Compression.ZipArchive]::new($memory, [IO.Compression.ZipArchiveMode]::Create, $true)
    foreach ($name in @('safe.txt','duplicate.txt','duplicate.txt')) {
        $entry=$write.CreateEntry($name); $stream=$entry.Open()
        try { $stream.WriteByte(65) } finally { $stream.Dispose() }
    }
    $write.Dispose(); $memory.Position=0
    $read=[IO.Compression.ZipArchive]::new($memory, [IO.Compression.ZipArchiveMode]::Read)
    try {
        $record=[pscustomobject]@{sha256=(Get-BytesHash ([byte[]]@(65)));bytes=1}
        $null=Read-ZipMember $read 'safe.txt' $record
        MustReject { Read-ZipMember $read 'missing.txt' $record } 'MISSING_OR_DUPLICATE_MEMBER'
        MustReject { Read-ZipMember $read 'duplicate.txt' $record } 'MISSING_OR_DUPLICATE_MEMBER'
        MustReject { Read-ZipMember $read 'SAFE.txt' $record } 'MISSING_OR_DUPLICATE_MEMBER'
        MustReject { Read-ZipMember $read 'safe.txt' ([pscustomobject]@{sha256=('0'*64)}) } 'HASH_MISMATCH'
    } finally { $read.Dispose(); $memory.Dispose() }
}
Check 'compiler response preserves spaces and excludes restore/analyzer imports' {
    $argsList=@(New-CompilerArguments 'C:\safe space\csc.dll' 'C:\safe space\MergeSmoke.cs' 'C:\safe space\MergeSmoke.dll' @('C:\safe space\System.Runtime.dll'))
    $rsp=New-CompilerResponse $argsList
    if ($argsList[0] -ne 'exec' -or $argsList[4] -ne '/noconfig' -or $rsp -notmatch '"/reference:C:\\safe space\\System.Runtime.dll"' -or
        $rsp -match '/analyzer:|restore|\.targets|\.csproj|/noconfig') { throw 'UNSAFE_COMPILER_PLAN' }
    MustReject { New-CompilerResponse @('exec','--fx-version','10.0.11','csc.dll','/noconfig','/out:x', ('bad'+[char]10+'/analyzer:evil')) } 'RESPONSE_INJECTION_REJECTED'
    MustReject { New-CompilerArguments 'csc.dll' 'source.cs' 'output.dll' @() } 'EMPTY_REFERENCES'
}
Check 'runtime exact version and deps bind application libraries only' {
    $config=New-RuntimeConfig
    if ($config.runtimeOptions.framework.version -ne '10.0.11' -or $config.runtimeOptions.rollForward -ne 'Disable') { throw 'RUNTIME_ROLL_FORWARD' }
    $deps=New-AppDeps @('PdfSharp.dll','PdfSharp.Shared.dll')
    if ($deps.targets['.NETCoreApp,Version=v10.0']['MergeSmoke/1.0.0'].runtime.Count -ne 3) { throw 'DEPS_MISSING' }
    MustReject { New-AppDeps @('../evil.dll') } 'INVALID_RUNTIME_ASSET'
    MustReject { New-AppDeps @('PdfSharp.dll','PdfSharp.dll') } 'INVALID_RUNTIME_ASSET'
}
Check 'locked real source, installed bytes, packages and selected members' {
    $verification = (& "$PSScriptRoot/Build-Offline.ps1" -Mode Verify) | ConvertFrom-Json
    if ($verification.status -ne 'PASS' -or $verification.compilerExecuted -or $verification.engineExecuted -or
        $verification.packages -ne 4 -or $verification.members -ne 12) { throw 'VERIFICATION_CONTRACT' }
}
[ordered]@{schemaVersion=1;status='PASS';tests=$results.Count;results=$results;
    compilerExecuted=$false;engineExecuted=$false;pdfGenerated=$false} | ConvertTo-Json -Depth 5
