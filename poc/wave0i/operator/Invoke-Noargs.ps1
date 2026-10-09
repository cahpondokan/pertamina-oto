#requires -Version 7.0
param([switch]$Execute)
$ErrorActionPreference='Stop'
$repo=[IO.Path]::GetFullPath("$PSScriptRoot/../../..")
$stage='C:\Users\cahpo\AppData\Local\Temp\OTO-W0I-Noargs-001'
$bundle=Join-Path $repo 'artifacts/evidence/P02-W0I/build-001/bundle.manifest.json'
$expected='C5F2CE96F35B5790A4D4D862ACA457AD65411A4143AE90EB011502928B59543D'
if((Get-FileHash -LiteralPath $bundle).Hash -ne $expected){throw 'BUNDLE_MANIFEST_DRIFT'}
$manifest=Get-Content -LiteralPath $bundle -Raw | ConvertFrom-Json
Import-Module (Join-Path $repo 'poc/wave0i/environment/Environment.psm1') -Force
$null=Assert-PrivateDirectory $stage
function Assert-Payload {
    $files=@(Get-ChildItem -LiteralPath $stage -Recurse -File -Force)
    if($files.Count -ne $manifest.files.Count){throw 'INVENTORY_DRIFT'}
    foreach($entry in $manifest.files){
        if($entry.path -notmatch '^[A-Za-z0-9_.-]+(/[A-Za-z0-9_.-]+)*$' -or $entry.path.Contains('..')){throw 'UNSAFE_MANIFEST'}
        $file=Join-Path $stage $entry.path
        if((Get-FileHash -LiteralPath $file).Hash -ne $entry.sha256 -or (Get-Item -LiteralPath $file).Length -ne $entry.bytes){throw 'PAYLOAD_DRIFT'}
    }
}
Assert-Payload
$escaped=[Security.SecurityElement]::Escape($stage)
$config="<Configuration><Networking>Disable</Networking><ClipboardRedirection>Disable</ClipboardRedirection><PrinterRedirection>Disable</PrinterRedirection><AudioInput>Disable</AudioInput><VideoInput>Disable</VideoInput><vGPU>Disable</vGPU><ProtectedClient>Enable</ProtectedClient><MemoryInMB>4096</MemoryInMB><MappedFolders><MappedFolder><HostFolder>$escaped</HostFolder><SandboxFolder>C:\PocInput</SandboxFolder><ReadOnly>true</ReadOnly></MappedFolder></MappedFolders></Configuration>"
$source=Get-Content (Join-Path $PSScriptRoot 'Guest-Noargs.ps1') -Raw
$command='powershell.exe -NoLogo -NoProfile -EncodedCommand '+[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($source))
$configHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($config)))
$commandHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($command)))
if(-not $Execute){[ordered]@{status='VERIFIED_NOT_EXECUTED';files=$manifest.files.Count;manifestSha256=$expected;configSha256=$configHash;commandSha256=$commandHash;readOnlyMappings=1;writableMappings=0}|ConvertTo-Json; return}
$wsb='C:\Users\cahpo\AppData\Local\Microsoft\WindowsApps\wsb.exe'
function Invoke-Cli([string[]]$Arguments,[int]$TimeoutSeconds=30){
    $info=[Diagnostics.ProcessStartInfo]::new(); $info.FileName=$wsb
    $info.UseShellExecute=$false; $info.CreateNoWindow=$true
    $info.RedirectStandardOutput=$true; $info.RedirectStandardError=$true
    foreach($arg in $Arguments){$info.ArgumentList.Add($arg)}
    $p=[Diagnostics.Process]::Start($info)
    try {
        # Drain stderr asynchronously, but never wait for EOF inherited by a broker.
        $err=$p.StandardError.ReadToEndAsync()
        if(-not $p.WaitForExit($TimeoutSeconds*1000)){$p.Kill($true);$null=$p.WaitForExit(5000);throw 'CLI_TIMEOUT'}
        if($p.ExitCode -ne 0){throw 'CLI_FAILED'}
        if($err.IsCompleted -and $err.GetAwaiter().GetResult().Length -gt 0){throw 'CLI_STDERR'}
        # connect/stop are exit-code operations. JSON operations finish when one
        # complete JSON object has arrived, not when a persistent broker closes IO.
        if($Arguments[0] -in @('connect','stop')){return ''}
        $stdout='';$complete=$false;$readClock=[Diagnostics.Stopwatch]::StartNew()
        for($lineCount=0;$lineCount -lt 64;$lineCount++){
            if($readClock.Elapsed.TotalSeconds -ge 5){throw 'CLI_JSON_READ_TIMEOUT'}
            $line=$p.StandardOutput.ReadLineAsync()
            if(-not $line.Wait(1000)){throw 'CLI_JSON_READ_TIMEOUT'}
            $value=$line.GetAwaiter().GetResult()
            if($null -eq $value){break}
            $stdout+=$value+"`n"
            if($stdout.Length -gt 65536){throw 'CLI_OUTPUT_LIMIT'}
            try{$null=$stdout|ConvertFrom-Json -ErrorAction Stop;$complete=$true}catch{}
            if($complete){break}
        }
        if(-not $complete){throw 'CLI_JSON_INCOMPLETE'}
        return $stdout
    } finally {$p.Dispose()}
}
function Read-Sessions([string]$Json){
    $data=$Json|ConvertFrom-Json
    if($null -eq $data -or @($data.PSObject.Properties).Count -ne 1 -or $data.PSObject.Properties.Name -cnotcontains 'WindowsSandboxEnvironments' -or $data.WindowsSandboxEnvironments -isnot [array]){throw 'INVALID_SESSION_LIST'}
    $seen=@{}
    foreach($item in $data.WindowsSandboxEnvironments){
        $id=[guid]::Empty
        if($null -eq $item -or -not [guid]::TryParse([string]$item.Id,[ref]$id) -or $seen.ContainsKey($id.ToString())){throw 'INVALID_SESSION_ID'}
        $seen[$id.ToString()]=$true
    }
    return $data
}
$before=Read-Sessions (Invoke-Cli @('list','--raw'))
if(@($before.WindowsSandboxEnvironments).Count -ne 0){throw 'PREEXISTING_SESSION'}
$os=Get-CimInstance Win32_OperatingSystem
if($os.FreePhysicalMemory/1024 -lt 5120){throw 'RESOURCE_HOLD'}
$session=$null;$receipt=$null;$closed=$false;$startAttempted=$false;$start=[datetimeoffset]::Now
try {
    $startAttempted=$true
    $created=(Invoke-Cli @('start','--config',$config,'--raw') 90)|ConvertFrom-Json
    $parsed=[guid]::Empty
    if(-not [guid]::TryParse([string]$created.Id,[ref]$parsed)){throw 'SESSION_ID_MISSING'}
    $session=$parsed.ToString()
    $null=Invoke-Cli @('connect','--id',$session) 30
    Start-Sleep -Seconds 15
    $receipt=(Invoke-Cli @('exec','--id',$session,'--command',$command,'--run-as','ExistingLogin','--raw') 45)|ConvertFrom-Json
    if($null -eq $receipt -or @($receipt.PSObject.Properties).Count -ne 1 -or $receipt.PSObject.Properties.Name -cnotcontains 'ExitCode' -or ($receipt.ExitCode -isnot [int] -and $receipt.ExitCode -isnot [long])){throw 'INVALID_EXEC_RECEIPT'}
} finally {
    if($session){
        $null=Invoke-Cli @('stop','--id',$session) 30
        $after=Read-Sessions (Invoke-Cli @('list','--raw'))
        $closed=@($after.WindowsSandboxEnvironments|Where-Object Id -eq $session).Count -eq 0
        if(-not $closed){throw 'SESSION_CLOSE_UNCONFIRMED'}
    } elseif($startAttempted){
        $recovery=Read-Sessions (Invoke-Cli @('list','--raw'))
        if(@($recovery.WindowsSandboxEnvironments).Count -ne 0){throw 'RECOVERY_REQUIRED_START_UNCERTAIN_DO_NOT_RETRY'}
        Write-Output 'START_FAILED_SESSION_ABSENCE_CONFIRMED'
    }
}
Assert-Payload
$elapsed=([datetimeoffset]::Now-$start).TotalSeconds
[ordered]@{status=$(if($receipt.ExitCode -eq 2 -and $closed -and $elapsed -lt 300){'NOARGS_EXIT_2'}else{'PROBE_FAILED'});startedAt=$start.ToString('o');elapsedSeconds=$elapsed;guestExitCode=$receipt.ExitCode;session=$session;sessionClosed=$closed;sourceUnchanged=$true;manifestSha256=$expected;configSha256=$configHash;commandSha256=$commandHash;pdfProcessed=$false}|ConvertTo-Json
if($receipt.ExitCode -ne 2 -or -not $closed -or $elapsed -ge 300){exit 3}
