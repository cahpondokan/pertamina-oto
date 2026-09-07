param(
    [Parameter(Mandatory)][ValidateSet('AB','CAB','AA','AC')][string]$CaseId,
    [Parameter(Mandatory)][string]$Executable,
    [Parameter(Mandatory)][string]$ExecutableSha256,
    [Parameter(Mandatory)][string]$Arguments,
    [ValidateRange(1,60)][int]$TimeoutSeconds=30
)
$ErrorActionPreference='Stop'
Import-Module (Join-Path $PSScriptRoot 'Environment.psm1') -Force
try {
    if (-not $PSScriptRoot.StartsWith('C:\PocInput\',[StringComparison]::OrdinalIgnoreCase)) { throw 'GUEST_STAGING_REQUIRED' }
    $exe=[IO.Path]::GetFullPath($Executable)
    if (-not $exe.StartsWith('C:\PocInput\',[StringComparison]::OrdinalIgnoreCase) -or $exe -notmatch '\.exe$') { throw 'EXECUTABLE_SCOPE' }
    if ((Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash -cne $ExecutableSha256) { throw 'EXECUTABLE_DIGEST' }
    # Arguments are trusted root-reviewed invocation text, never user/document content.
    if ($Arguments.Contains([char]0) -or $Arguments.Contains("`r") -or $Arguments.Contains("`n")) { throw 'ARGUMENT_SHAPE' }
    $private='C:\PocPrivate'
    if (-not (Test-Path -LiteralPath $private)) { New-Item -ItemType Directory -Path $private | Out-Null }
    $caseRoot=Join-Path $private $CaseId
    if (Test-Path -LiteralPath $caseRoot) { throw 'CASE_REUSE' }
    New-Item -ItemType Directory -Path $caseRoot | Out-Null
    $info=[Diagnostics.ProcessStartInfo]::new()
    $info.FileName=$exe; $info.Arguments=$Arguments; $info.WorkingDirectory=$caseRoot
    $info.UseShellExecute=$false; $info.CreateNoWindow=$true
    # No pipes: avoids unbounded capture/deadlock. Child writes only guest-private files by reviewed arguments.
    $child=[Diagnostics.Process]::new(); $child.StartInfo=$info
    $clock=[Diagnostics.Stopwatch]::StartNew()
    if (-not $child.Start()) { throw 'START_FAILED' }
    $status='EXITED'; $code=-1
    if (-not $child.WaitForExit($TimeoutSeconds*1000)) {
        $status='TIMEOUT'
        $kill=[Diagnostics.ProcessStartInfo]::new()
        $kill.FileName=Join-Path ([Environment]::GetFolderPath('System')) 'taskkill.exe'
        $kill.Arguments='/PID '+$child.Id+' /T /F'; $kill.UseShellExecute=$false; $kill.CreateNoWindow=$true
        $killer=[Diagnostics.Process]::Start($kill)
        if (-not $killer.WaitForExit(5000)) { throw 'TREE_STOP_UNCONFIRMED' }
        if ($killer.ExitCode -ne 0 -or -not $child.WaitForExit(5000)) { throw 'TREE_STOP_UNCONFIRMED' }
    } else { $code=$child.ExitCode }
    $clock.Stop()
    $output=Join-Path $caseRoot 'result.pdf'
    $digest=('0'*64)
    if ($status -eq 'EXITED' -and $code -eq 0 -and (Test-Path -LiteralPath $output -PathType Leaf)) {
        if ((Get-Item -LiteralPath $output).Length -gt 1048576) { throw 'OUTPUT_TOO_LARGE' }
        $digest=(Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
    } else { if ($status -ne 'TIMEOUT') { $status='FAILED' } }
    $record=ConvertTo-SafeRecord $CaseId $status $code $clock.ElapsedMilliseconds $digest
    $export=Join-Path 'C:\PocExport' ($CaseId+'.json')
    $stream=[IO.File]::Open($export,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    try { $bytes=[Text.Encoding]::UTF8.GetBytes($record); $stream.Write($bytes,0,$bytes.Length) } finally { $stream.Dispose() }
    Write-Output ('CASE='+$CaseId+'; STATUS='+$status+'; CLOSE_GUEST_AFTER_RUN')
    if ($status -ne 'EXITED') { exit 3 }
} catch { Write-Output 'SMOKE_FAILED_CLOSE_GUEST; NO_RAW_ERROR_EXPORTED'; exit 4 }
