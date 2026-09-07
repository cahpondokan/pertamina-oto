param([switch]$Observe, [switch]$SelfTest)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-Readiness([System.Collections.IDictionary]$Checks) {
    $required = @('FeatureEnabled','LauncherPresent','StartEntryPresent','VirtualizationObservable','NoPendingRestart','TemplateControls')
    $safe = [ordered]@{}
    foreach ($key in $required) {
        $value = 'UNKNOWN'
        if ($Checks.Contains($key) -and $Checks[$key] -is [string] -and $Checks[$key] -cin @('PASS','FAIL','UNKNOWN')) {
            $value = $Checks[$key]
        }
        $safe[$key] = $value
    }
    $status = 'BLOCKED'
    if (@($safe.Values | Where-Object { $_ -cne 'PASS' }).Count -eq 0) { $status = 'OBSERVATIONS_COMPLETE_REVIEW_REQUIRED' }
    [pscustomobject]@{
        SchemaVersion=1; Status=$status; Checks=$safe
        LaunchAuthorized=$false; RestartAuthorized=$false; ProcessingAuthorized=$false
        NetworkDisabledProven=$false; CostBoundary='ZERO_ADDITIONAL_COST'
        StopMarker='STOP_BEFORE_LAUNCH'
    }
}

function Read-HostChecks {
    $checks = [ordered]@{}
    foreach ($key in @('FeatureEnabled','LauncherPresent','StartEntryPresent','VirtualizationObservable','NoPendingRestart','TemplateControls')) { $checks[$key]='UNKNOWN' }
    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT) { return $checks }
    try {
        $f = Get-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -ErrorAction Stop
        $checks.FeatureEnabled = if ([string]$f.State -ceq 'Enabled') { 'PASS' } else { 'FAIL' }
    } catch { } # Permission/module failure is UNKNOWN; never elevate or print exception.
    try {
        $launcher = Join-Path ([Environment]::GetFolderPath('System')) 'WindowsSandbox.exe'
        $checks.LauncherPresent = if (Test-Path -LiteralPath $launcher -PathType Leaf -ErrorAction Stop) { 'PASS' } else { 'FAIL' }
    } catch { }
    try {
        $entries = @(Get-StartApps -ErrorAction Stop | Where-Object { $_.AppID -match '(?i)WindowsSandbox' })
        $checks.StartEntryPresent = if ($entries.Count -gt 0) { 'PASS' } else { 'UNKNOWN' }
    } catch { }
    try {
        $system = Get-CimInstance -ClassName Win32_ComputerSystem -ErrorAction Stop
        $processors = @(Get-CimInstance -ClassName Win32_Processor -ErrorAction Stop)
        if ($system.HypervisorPresent -eq $true) { $checks.VirtualizationObservable = 'PASS' }
        elseif ($processors.Count -gt 0 -and @($processors | Where-Object {
            $_.VirtualizationFirmwareEnabled -ne $true -or $_.SecondLevelAddressTranslationExtensions -ne $true -or $_.VMMonitorModeExtensions -ne $true
        }).Count -eq 0) { $checks.VirtualizationObservable = 'PASS' }
    } catch { }
    try {
        # Read registry keys without exporting pending rename values (which contain paths).
        $base = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, [Microsoft.Win32.RegistryView]::Registry64)
        try {
            $pending = $false
            foreach ($path in @('SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending','SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired')) {
                $key = $base.OpenSubKey($path, $false)
                if ($null -ne $key) { $pending = $true; $key.Dispose() }
            }
            $session = $base.OpenSubKey('SYSTEM\CurrentControlSet\Control\Session Manager', $false)
            if ($null -eq $session) { throw 'REGISTRY_UNAVAILABLE' }
            try {
                foreach ($name in @('PendingFileRenameOperations','PendingFileRenameOperations2')) {
                    if (@($session.GetValue($name, $null) | Where-Object { -not [string]::IsNullOrEmpty([string]$_) }).Count -gt 0) { $pending=$true }
                }
            } finally { $session.Dispose() }
            $checks.NoPendingRestart = if ($pending) { 'FAIL' } else { 'PASS' }
        } finally { $base.Dispose() }
    } catch { }
    try {
        & (Join-Path (Split-Path $PSScriptRoot -Parent) 'Validate-Template.ps1') | Out-Null
        $checks.TemplateControls = 'PASS'
    } catch { $checks.TemplateControls='FAIL' }
    return $checks
}

if ($Observe -and $SelfTest) { Write-Output 'INVALID_MODE'; exit 2 }
if ($SelfTest) {
    $good = [ordered]@{FeatureEnabled='PASS';LauncherPresent='PASS';StartEntryPresent='PASS';VirtualizationObservable='PASS';NoPendingRestart='PASS';TemplateControls='PASS'}
    $count = 0
    $r = Get-Readiness $good
    if ($r.Status -cne 'OBSERVATIONS_COMPLETE_REVIEW_REQUIRED' -or $r.LaunchAuthorized -or $r.RestartAuthorized -or $r.ProcessingAuthorized -or $r.NetworkDisabledProven) { throw 'POSITIVE_FAILED' }
    $count++
    foreach ($key in @($good.Keys)) {
        foreach ($value in @('FAIL','UNKNOWN','unsafe-path-canary')) {
            $fixture = [ordered]@{}
            foreach ($k in $good.Keys) { $fixture[$k]=$good[$k] }
            $fixture[$key]=$value
            $r = Get-Readiness $fixture
            $json = $r | ConvertTo-Json -Depth 5 -Compress
            if ($r.Status -cne 'BLOCKED' -or $json.Contains('unsafe-path-canary')) { throw 'NEGATIVE_FAILED' }
            $count++
        }
    }
    $r = Get-Readiness ([ordered]@{Extra='secret-canary'})
    if ($r.Status -cne 'BLOCKED' -or ($r | ConvertTo-Json -Depth 5).Contains('secret-canary')) { throw 'MISSING_FAILED' }
    $count++
    Write-Output "DISCOVERED=$count PASSED=$count FAILED=0; HOST_PROBES=0"
    exit 0
}
if (-not $Observe) { Write-Output 'NO_OBSERVATION: select -SelfTest or later authorized -Observe; STOP_BEFORE_LAUNCH'; exit 2 }
$result = Get-Readiness (Read-HostChecks)
$result | ConvertTo-Json -Depth 5
if ($result.Status -ceq 'BLOCKED') { exit 3 }
exit 0
