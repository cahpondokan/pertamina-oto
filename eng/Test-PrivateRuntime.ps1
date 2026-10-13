param([Parameter(Mandatory)][string]$Application, [Parameter(Mandatory)][string]$Evidence)
$ErrorActionPreference = 'Stop'
$Application = [IO.Path]::GetFullPath($Application)
$Evidence = [IO.Path]::GetFullPath($Evidence)
New-Item -ItemType Directory -Path $Evidence -Force | Out-Null
function Invoke-Probe([string]$Exe, [string[]]$Arguments, [string]$Name, [bool]$ExpectSuccess) {
    $trace = Join-Path $Evidence "$Name.host.txt"
    $start = [Diagnostics.ProcessStartInfo]::new($Exe)
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.WorkingDirectory = Split-Path $Exe -Parent
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($arg in $Arguments) { $start.ArgumentList.Add($arg) }
    $start.Environment['DOTNET_ROOT'] = Join-Path $Evidence 'nonexistent-runtime'
    $start.Environment['DOTNET_ROOT_X64'] = Join-Path $Evidence 'nonexistent-runtime'
    $start.Environment['DOTNET_DISABLE_GUI_ERRORS'] = '1'
    $start.Environment['DOTNET_HOST_TRACE'] = '1'
    $start.Environment['DOTNET_HOST_TRACEFILE'] = $trace
    $process = [Diagnostics.Process]::Start($start)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        if (!$process.WaitForExit(120000)) { throw "Runtime probe timed out: $Name (PID $($process.Id))" }
        [IO.File]::WriteAllText((Join-Path $Evidence "$Name.stdout.txt"), $stdout.GetAwaiter().GetResult())
        [IO.File]::WriteAllText((Join-Path $Evidence "$Name.stderr.txt"), $stderr.GetAwaiter().GetResult())
        if ($ExpectSuccess -ne ($process.ExitCode -eq 0)) { throw "Unexpected runtime probe exit: $Name / $($process.ExitCode)" }
        $hostLog = [IO.File]::ReadAllText($trace)
        if ($ExpectSuccess) {
            $localCore = Join-Path (Split-Path $Exe -Parent) 'coreclr.dll'
            if (!$hostLog.Contains($localCore, [StringComparison]::OrdinalIgnoreCase)) { throw "No private coreclr proof: $Name" }
        }
        [pscustomobject]@{Probe=$Name;ExitCode=$process.ExitCode;PrivateRuntimeVerified=$ExpectSuccess;HostTrace=$trace}
    } finally { $process.Dispose() }
}
$results = @()
$results += Invoke-Probe "$Application/restart-launcher/PERTAMINA-OTO.exe" @('--self-test', "$Evidence/launcher-fixture") 'launcher' $true
$results += Invoke-Probe "$Application/worker/Pertamina.Oto.LocalPdf.Worker.exe" @('--self-test') 'worker' $true
$results += Invoke-Probe "$Application/word-worker/Pertamina.Oto.WordPdf.Worker.exe" @('--self-test') 'word-worker' $true
$results += Invoke-Probe "$Application/pdf-opener/PertaminaOTO.PdfOpener.exe" @('--self-test', "$Evidence/opener-fixture") 'pdf-opener' $true
$results += Invoke-Probe "$Application/Pertamina.Oto.Desktop.App.exe" @('--audit-quality') 'desktop' $true
$negative = Join-Path $Evidence ('missing-runtime-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $negative | Out-Null
foreach ($name in @('PERTAMINA-OTO.exe','PERTAMINA-OTO.dll','PERTAMINA-OTO.deps.json','PERTAMINA-OTO.runtimeconfig.json')) {
    Copy-Item -LiteralPath "$Application/restart-launcher/$name" -Destination $negative
}
$results += Invoke-Probe "$negative/PERTAMINA-OTO.exe" @('--self-test', "$Evidence/negative-fixture") 'missing-private-runtime' $false
@{Results=$results;CleanWindowsVmTested=$false;Limit='System runtime remains installed. App-local host trace and negative fallback checks are not clean-machine UAT.'} | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath "$Evidence/summary.json"
$results | Select-Object Probe,ExitCode,PrivateRuntimeVerified
