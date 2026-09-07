$ErrorActionPreference = 'Stop'
try {
    $interfaces = @([System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | Where-Object { $_.NetworkInterfaceType -ne 'Loopback' })
    $up = @($interfaces | Where-Object { $_.OperationalStatus -eq 'Up' }).Count
    Write-Output 'W0H_BENIGN_OBSERVATION_V1'
    Write-Output ('OS_VERSION=' + [Environment]::OSVersion.Version.ToString())
    Write-Output ('OS_64BIT=' + [Environment]::Is64BitOperatingSystem)
    Write-Output ('NON_LOOPBACK_INTERFACES=' + $interfaces.Count)
    Write-Output ('UP_NON_LOOPBACK_INTERFACES=' + $up)
    Write-Output ('OBSERVATION=' + $(if ($interfaces.Count -eq 0) { 'NO_NON_LOOPBACK_INTERFACE_OBSERVED' } else { 'REVIEW_REQUIRED' }))
    Write-Output 'EGRESS_ATTEMPTS_NOT_TESTED; NO_PDF_PROCESSED; NO_HOST_MAPPING_REQUESTED'
} catch { Write-Output 'OBSERVATION=UNKNOWN; QUERY_FAILED' }
Write-Output 'OPERATOR_RECORD_SCREEN_THEN_CLOSE_GUEST'
