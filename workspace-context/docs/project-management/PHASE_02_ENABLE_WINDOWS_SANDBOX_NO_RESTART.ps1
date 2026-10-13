param(
    [Parameter(Mandatory = $true)]
    [string]$ResultPath
)

$ErrorActionPreference = 'Stop'
$featureName = 'Containers-DisposableClientVM'
$result = [ordered]@{
    SchemaVersion = 1
    TimestampUtc = [DateTimeOffset]::UtcNow.ToString('o')
    FeatureName = $featureName
    CostBoundary = 'ZERO_ADDITIONAL_COST'
    RestartCommandIssued = $false
    Action = 'INSPECT_ONLY'
    StateBefore = $null
    StateAfter = $null
    RestartNeeded = $null
    Status = 'STARTED'
    ErrorType = $null
    ErrorMessage = $null
}

try {
    $before = Get-WindowsOptionalFeature -Online -FeatureName $featureName
    $result.StateBefore = [string]$before.State

    if ($before.State -eq 'Enabled') {
        $result.Action = 'ALREADY_ENABLED'
        $result.RestartNeeded = $false
    } elseif ($before.State -in @('Disabled', 'DisabledWithPayloadRemoved')) {
        $result.Action = 'ENABLE_NO_RESTART'
        $enableResult = Enable-WindowsOptionalFeature -Online -FeatureName $featureName -All -NoRestart
        $result.RestartNeeded = [bool]$enableResult.RestartNeeded
    } else {
        throw "Unsupported feature state: $($before.State)"
    }

    $after = Get-WindowsOptionalFeature -Online -FeatureName $featureName
    $result.StateAfter = [string]$after.State
    $result.Status = if ($after.State -eq 'Enabled') { 'ENABLED' } else { 'PENDING_RESTART_OR_INCOMPLETE' }
} catch {
    $result.Status = 'FAILED'
    $result.ErrorType = $_.Exception.GetType().FullName
    $result.ErrorMessage = $_.Exception.Message
} finally {
    $result.TimestampUtc = [DateTimeOffset]::UtcNow.ToString('o')
    $parent = Split-Path -Parent $ResultPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    $result | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $ResultPath -Encoding UTF8
}

if ($result.Status -eq 'FAILED') { exit 1 }
if ($result.Status -ne 'ENABLED') { exit 2 }
exit 0
