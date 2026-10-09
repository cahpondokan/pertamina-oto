#requires -Version 7.0
[CmdletBinding()]
param(
    [ValidateSet('Verify', 'Build')][string] $Mode = 'Verify',
    [string] $DotnetRoot = 'C:\Program Files\dotnet',
    [string] $EvidenceRoot = 'D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434',
    [string] $OutputDirectory
)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/CandidateTools.ps1"
$repo = [IO.Path]::GetFullPath("$PSScriptRoot/../../..")
$lockText = [IO.File]::ReadAllText("$PSScriptRoot/inputs.lock.json").Replace(([string][char]13 + [char]10), [string][char]10)
# Reviewed lock is not an externally supplied resolution plan.
if ((Get-BytesHash ([Text.Encoding]::UTF8.GetBytes($lockText))) -ne '3825D5ACC6A2692C6160B474EB3DDBA2FF4223E1D0FBDE6C32FFC83E7354AD27') { throw 'LOCK_CHANGED_REVIEW_REQUIRED' }
$lock = $lockText | ConvertFrom-Json
if ($lock.schemaVersion -ne 1 -or $lock.sdk -ne '10.0.400' -or $lock.runtime -ne '10.0.11') { throw 'LOCK_SCHEMA' }
$sourcePath = Get-SafePath $repo $lock.source.path
$sourceBytes = [Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($sourcePath).Replace(([string][char]13 + [char]10), [string][char]10))
Assert-Bytes $sourceBytes $lock.source
$noticePath = Get-SafePath $repo $lock.notice.path
$noticeBytes = [Text.Encoding]::UTF8.GetBytes([IO.File]::ReadAllText($noticePath).Replace(([string][char]13 + [char]10), [string][char]10))
Assert-Bytes $noticeBytes $lock.notice
foreach ($file in $lock.dotnetFiles) {
    $null = Read-BoundFile (Get-SafePath $DotnetRoot $file.path) $file
}
foreach ($package in $lock.packages) { Test-Package $package $EvidenceRoot }
$libraryNames = @($lock.packages | ForEach-Object { $_.members } | ForEach-Object { [IO.Path]::GetFileName($_.path) })
if (@($libraryNames | Sort-Object -Unique).Count -ne $libraryNames.Count) { throw 'DUPLICATE_LIBRARY' }
if ($Mode -eq 'Verify') {
    [ordered]@{status='PASS';mode='STATIC_INPUT_VERIFICATION';dotnetFiles=$lock.dotnetFiles.Count;
        packages=$lock.packages.Count;members=$libraryNames.Count;sourceBound=$true;
        compilerExecuted=$false;engineExecuted=$false} | ConvertTo-Json -Compress
    return
}
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { throw 'FRESH_OUTPUT_REQUIRED' }
$outRoot = [IO.Path]::GetFullPath($OutputDirectory)
Assert-NoReparse $outRoot
if (Test-Path -LiteralPath $outRoot) { throw 'OUTPUT_EXISTS' }
# A failed build leaves its private partial directory for diagnosis; it is never reused or deleted here.
[IO.Directory]::CreateDirectory($outRoot) | Out-Null
$guest = Get-SafePath $outRoot 'guest'
$runtimeRoot = Get-SafePath $guest 'dotnet'
$app = Get-SafePath $guest 'app'
$noticeRoot = Get-SafePath $guest 'notices'
$buildRoot = Get-SafePath $outRoot 'build'
foreach ($file in $lock.dotnetFiles) {
    $bytes = Read-BoundFile (Get-SafePath $DotnetRoot $file.path) $file
    $target = switch ($file.role) {
        'runtime' { Get-SafePath $runtimeRoot $file.path }
        'notice' { Get-SafePath $noticeRoot ('dotnet/' + [IO.Path]::GetFileName($file.path)) }
        'reference' { Get-SafePath $buildRoot ('ref/' + [IO.Path]::GetFileName($file.path)) }
        'compiler' { Get-SafePath $buildRoot ('compiler/' + [IO.Path]::GetFileName($file.path)) }
        default { throw 'UNKNOWN_FILE_ROLE' }
    }
    Write-NewBytes $target $bytes
}
foreach ($package in $lock.packages) { Test-Package $package $EvidenceRoot $app $noticeRoot }
Write-NewBytes (Get-SafePath $buildRoot 'MergeSmoke.cs') $sourceBytes
Write-NewBytes (Get-SafePath $noticeRoot 'PDFsharp-MIT.md') $noticeBytes
Write-NewText (Get-SafePath $app 'MergeSmoke.runtimeconfig.json') ((New-RuntimeConfig) | ConvertTo-Json -Depth 8)
Write-NewText (Get-SafePath $app 'MergeSmoke.deps.json') ((New-AppDeps $libraryNames) | ConvertTo-Json -Depth 10)
$references = @($lock.dotnetFiles | Where-Object role -eq 'reference' | ForEach-Object {
    Get-SafePath $buildRoot ('ref/' + [IO.Path]::GetFileName($_.path))
}) + @($libraryNames | ForEach-Object { Get-SafePath $app $_ })
$compilerArguments = @(New-CompilerArguments (Get-SafePath $buildRoot 'compiler/csc.dll') (Get-SafePath $buildRoot 'MergeSmoke.cs') (Get-SafePath $app 'MergeSmoke.dll') $references)
$response = New-CompilerResponse $compilerArguments
$responsePath = Get-SafePath $buildRoot 'compiler.rsp'
Write-NewText $responsePath $response
$start = [Diagnostics.ProcessStartInfo]::new()
$start.FileName = Get-SafePath $runtimeRoot 'dotnet.exe'
$start.UseShellExecute = $false
$start.CreateNoWindow = $true
$start.WorkingDirectory = $buildRoot
$start.RedirectStandardOutput = $true
$start.RedirectStandardError = $true
# Eliminate inherited profiler/startup-hook/resolver controls and shell interpretation.
$start.Environment.Clear()
$start.Environment['SystemRoot'] = $env:SystemRoot
$start.Environment['WINDIR'] = $env:SystemRoot
$start.Environment['TEMP'] = $buildRoot
$start.Environment['TMP'] = $buildRoot
$start.Environment['DOTNET_ROOT'] = $runtimeRoot
$start.Environment['DOTNET_MULTILEVEL_LOOKUP'] = '0'
$start.Environment['DOTNET_ROLL_FORWARD'] = 'Disable'
$start.Environment['DOTNET_CLI_TELEMETRY_OPTOUT'] = '1'
$start.Environment['DOTNET_NOLOGO'] = '1'
$start.Environment['DOTNET_CLI_UI_LANGUAGE'] = 'en-US'
foreach ($argument in @($compilerArguments[0..4]) + @('@' + $responsePath)) { $start.ArgumentList.Add($argument) }
Write-NewText (Get-SafePath $buildRoot 'command.json') (@{executable='guest/dotnet/dotnet.exe';
    arguments=@($compilerArguments | ForEach-Object { $_.Replace($outRoot, '<OUTPUT>') });
    invocation=@($start.ArgumentList | ForEach-Object { $_.Replace($outRoot, '<OUTPUT>') });
    responseSha256=(Get-BytesHash ([Text.Encoding]::UTF8.GetBytes($response)));
    timeoutSeconds=60;restore=$false;packageTargets=$false;analyzers=$false} | ConvertTo-Json -Depth 5)
$process = [Diagnostics.Process]::new()
$process.StartInfo = $start
try {
    if (-not $process.Start()) { throw 'COMPILER_START_FAILED' }
    $stdout = $process.StandardOutput.ReadToEndAsync()
    $stderr = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit(60000)) { $process.Kill($true); throw 'COMPILER_TIMEOUT' }
    $diagnostics = ($stdout.GetAwaiter().GetResult() + $stderr.GetAwaiter().GetResult()).Replace($outRoot, '<OUTPUT>')
    Write-NewText (Get-SafePath $buildRoot 'compiler.log') $diagnostics
    if ($process.ExitCode -ne 0) { throw 'COMPILER_FAILED_SEE_PRIVATE_LOG' }
} finally { $process.Dispose() }
if (-not [IO.File]::Exists((Get-SafePath $app 'MergeSmoke.dll'))) { throw 'MISSING_COMPILER_OUTPUT' }
$guestFiles = @(Get-ChildItem -LiteralPath $guest -Recurse -File | Sort-Object FullName | ForEach-Object {
    @{path=[IO.Path]::GetRelativePath($guest, $_.FullName).Replace('\','/');bytes=$_.Length;
      sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
})
Write-NewText (Get-SafePath $outRoot 'bundle.manifest.json') (@{schemaVersion=1;status='BUILT_NOT_EXECUTED';
    inputLockSha256=(Get-BytesHash ([Text.Encoding]::UTF8.GetBytes($lockText)));files=$guestFiles;
    entryPoint='dotnet/dotnet.exe';runner='app/MergeSmoke.dll';engineExecuted=$false} | ConvertTo-Json -Depth 7)
[ordered]@{status='BUILT_NOT_EXECUTED';guestFiles=$guestFiles.Count;manifest='bundle.manifest.json';
    engineExecuted=$false} | ConvertTo-Json -Compress
