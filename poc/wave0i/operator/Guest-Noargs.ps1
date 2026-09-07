$ErrorActionPreference='Stop'
try {
    $n=@([System.Net.NetworkInformation.NetworkInterface]::GetAllNetworkInterfaces() | Where-Object { $_.NetworkInterfaceType -ne 'Loopback' }).Count
    if($n -ne 0) { exit 21 }
    $info=[Diagnostics.ProcessStartInfo]::new()
    $info.FileName='C:\PocInput\dotnet\dotnet.exe'
    $info.Arguments='C:\PocInput\app\MergeSmoke.dll'
    $info.WorkingDirectory='C:\Windows\Temp'
    $info.UseShellExecute=$false; $info.CreateNoWindow=$true
    $info.EnvironmentVariables.Clear()
    $values=@{SystemRoot='C:\Windows';WINDIR='C:\Windows';TEMP='C:\Windows\Temp';TMP='C:\Windows\Temp';DOTNET_ROOT='C:\PocInput\dotnet';DOTNET_MULTILEVEL_LOOKUP='0';DOTNET_ROLL_FORWARD='Disable';DOTNET_CLI_TELEMETRY_OPTOUT='1'}
    foreach($key in $values.Keys){$info.EnvironmentVariables[$key]=$values[$key]}
    $child=[Diagnostics.Process]::Start($info)
    if(-not $child.WaitForExit(15000)) { $child.Kill(); $null=$child.WaitForExit(5000); exit 23 }
    $result=$child.ExitCode; $child.Dispose(); exit $result
} catch { exit 22 }
