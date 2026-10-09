param([Parameter(Mandatory)][ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$LedgerSha256)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
# Guest only. Root separately binds all runtime, DLL and script payload hashes.
function Assert-PlainFile([string]$Path) {
    $item=Get-Item -LiteralPath $Path -Force
    if($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'FILE_SHAPE' }
}
function Assert-Source($Entry,[string]$Path) {
    Assert-PlainFile $Path
    if($Entry.sha256 -cnotmatch '^[a-f0-9]{64}$' -or $Entry.bytes -isnot [int] -or $Entry.bytes -le 0 -or $Entry.bytes -gt 65536) { throw 'SOURCE_SCHEMA' }
    if((Get-Item -LiteralPath $Path).Length -ne $Entry.bytes -or (Get-FileHash -LiteralPath $Path).Hash -ine $Entry.sha256) { throw 'SOURCE_CHANGED' }
}
function Assert-Outcome([int]$Code,[long]$Bytes,[bool]$Unchanged) {
    if($Code -ne 0 -or $Bytes -le 0 -or $Bytes -gt 262144 -or -not $Unchanged) { throw 'ENGINE_RESULT_REJECTED' }
}
try {
    # Root embeds this hash-bound source in EncodedCommand; no staged script needed.
    if([Environment]::GetEnvironmentVariable('SystemRoot') -ine 'C:\Windows' -or [Environment]::GetEnvironmentVariable('USERNAME') -ine 'WDAGUtilityAccount') { exit 20 }
    if(@([Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | Where-Object {$_.NetworkInterfaceType -ne 'Loopback'}).Count -ne 0) { exit 21 }
    foreach($dir in @('C:\PocInput','C:\PocInput\corpus','C:\PocInput\app','C:\PocInput\dotnet')) {
        if((Get-Item -LiteralPath $dir -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) { exit 22 }
    }
    $ledgerPath='C:\PocInput\corpus\sources.json'
    Assert-PlainFile $ledgerPath
    if((Get-Item -LiteralPath $ledgerPath).Length -gt 4096 -or (Get-FileHash -LiteralPath $ledgerPath).Hash -ine $LedgerSha256) { exit 22 }
    $ledger=Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
    if((@($ledger.PSObject.Properties.Name | Sort-Object) -join ',') -cne 'A,B,C') { exit 22 }
    foreach($name in @('A','B','C')) { Assert-Source $ledger.$name ('C:\PocInput\corpus\'+$name+'.pdf') }
    if(Test-Path -LiteralPath 'C:\PocPrivate') { exit 24 }
    $null=New-Item -ItemType Directory -Path 'C:\PocPrivate'
    $cases=[ordered]@{AB=@('A','B');CAB=@('C','A','B');AA=@('A','A');AC=@('A','C')}
    foreach($case in $cases.Keys) {
        foreach($name in @('A','B','C')) { Assert-Source $ledger.$name ('C:\PocInput\corpus\'+$name+'.pdf') }
        $caseDir='C:\PocPrivate\'+$case
        $null=New-Item -ItemType Directory -Path $caseDir
        $output=$caseDir+'\result.pdf'
        $info=New-Object Diagnostics.ProcessStartInfo
        $info.FileName='C:\PocInput\dotnet\dotnet.exe'
        $info.Arguments='C:\PocInput\app\MergeSmoke.dll '+$output
        foreach($name in $cases[$case]) { $info.Arguments+=' C:\PocInput\corpus\'+$name+'.pdf' }
        $info.WorkingDirectory=$caseDir; $info.UseShellExecute=$false; $info.CreateNoWindow=$true
        $info.EnvironmentVariables.Clear()
        $values=@{SystemRoot='C:\Windows';WINDIR='C:\Windows';TEMP=$caseDir;TMP=$caseDir;DOTNET_ROOT='C:\PocInput\dotnet';DOTNET_MULTILEVEL_LOOKUP='0';DOTNET_ROLL_FORWARD='Disable';DOTNET_CLI_TELEMETRY_OPTOUT='1'}
        foreach($key in $values.Keys) { $info.EnvironmentVariables[$key]=$values[$key] }
        $child=[Diagnostics.Process]::Start($info)
        try {
            if(-not $child.WaitForExit(15000)) {
                $child.Kill()
                if(-not $child.WaitForExit(5000)) { exit 26 }
                exit 25
            }
            $code=$child.ExitCode
        } finally { $child.Dispose() }
        Assert-PlainFile $output
        foreach($name in @('A','B','C')) { Assert-Source $ledger.$name ('C:\PocInput\corpus\'+$name+'.pdf') }
        Assert-Outcome $code (Get-Item -LiteralPath $output).Length $true
        # Guest-private receipt only; no raw PDF, error text or path export.
        $record=[ordered]@{caseId=$case;status='ENGINE_SMOKE_NOT_ORACLE_VALIDATED';outputSha256=(Get-FileHash -LiteralPath $output).Hash;bytes=(Get-Item -LiteralPath $output).Length}
        $json=$record | ConvertTo-Json -Compress
        $stream=[IO.File]::Open($caseDir+'\receipt.json',[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
        try { $bytes=[Text.Encoding]::UTF8.GetBytes($json); $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
    }
    if((Get-FileHash -LiteralPath $ledgerPath).Hash -ine $LedgerSha256) { exit 22 }
    exit 0
} catch { exit 27 }
