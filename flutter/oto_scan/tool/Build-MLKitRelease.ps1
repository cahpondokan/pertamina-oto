param([ValidateSet('appbundle','apk')][string]$Kind = 'appbundle')
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$output = Join-Path $project 'release-output/1.0.1'
$evidence = Join-Path $project 'evidence/play-release-1.0.1'
$store = Join-Path $project '.release-private/oto-scan-upload.p12'
$secret = Join-Path $project '.release-private/password.dpapi'
if (!(Test-Path -LiteralPath $store) -or !(Test-Path -LiteralPath $secret)) { throw 'Existing upload signer is required.' }
New-Item -ItemType Directory -Force $output,$evidence | Out-Null
$secure = (Get-Content -LiteralPath $secret -Raw).Trim() | ConvertTo-SecureString
$credential = New-Object System.Management.Automation.PSCredential('upload',$secure)
$env:JAVA_HOME = 'C:/Program Files/Android/Android Studio/jbr'
try {
    $env:OTO_UPLOAD_PASSWORD = $credential.GetNetworkCredential().Password
    $env:OTO_UPLOAD_STORE = $store
    $env:OTO_SCAN_QA = 'false'
    $env:OTO_MLKIT_TRIAL = 'false'
    $env:OTO_GOOGLE_SCANNER = 'true'
    $env:OTO_DEPENDENCY_INTAKE = 'false'
    $env:CI = 'true'
    $env:FLUTTER_SUPPRESS_ANALYTICS = 'true'
    $env:DART_SUPPRESS_ANALYTICS = 'true'
    Set-Item 'Env:ORG_GRADLE_PROJECT_kotlin.compiler.execution.strategy' 'in-process'
    Set-Item 'Env:ORG_GRADLE_PROJECT_kotlin.daemon.jvmargs' '-Xmx384m -XX:+UseSerialGC -XX:ReservedCodeCacheSize=64m'
    Push-Location $project
    try {
        & 'D:/Flutter/flutter/bin/cache/dart-sdk/bin/dart.exe' 'D:/Flutter/flutter/bin/cache/flutter_tools.snapshot' --no-version-check build $Kind --release --no-pub --target-platform android-arm,android-arm64,android-x64 --build-name=1.0.1 --build-number=19 --dart-define=OTO_TEST_MODE=false --dart-define=OTO_MLKIT_TRIAL=false --dart-define=OTO_GOOGLE_SCANNER=true 2>&1 | Tee-Object (Join-Path $evidence "build-$Kind.log")
        if ($LASTEXITCODE -ne 0) { throw 'ML Kit Play build failed.' }
        $isBundle = $Kind -eq 'appbundle'
        $source = if ($isBundle) { 'build/app/outputs/bundle/release/app-release.aab' } else { 'build/app/outputs/flutter-apk/app-release.apk' }
        $extension = if ($isBundle) { 'aab' } else { 'apk' }
        $prefix = if ($isBundle) { 'base/lib' } else { 'lib' }
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        $zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $project $source))
        try {
            foreach ($abi in @('armeabi-v7a','arm64-v8a','x86_64')) {
                foreach ($library in @('libapp.so','libflutter.so')) {
                    $entry = $zip.GetEntry("$prefix/$abi/$library")
                    if ($null -eq $entry -or $entry.Length -lt 1000000) { throw "Incomplete package: missing $abi/$library" }
                }
            }
        } finally { $zip.Dispose() }
        $target = Join-Path $output "OTO-Scan-1.0.1.$extension"
        Copy-Item -LiteralPath $source -Destination $target
        Get-FileHash -LiteralPath $target -Algorithm SHA256 | Select-Object Algorithm,Hash,Path | ConvertTo-Json | Set-Content (Join-Path $output "$extension-sha256.json")
    } finally { Pop-Location }
} finally {
    Remove-Item Env:OTO_UPLOAD_PASSWORD,Env:OTO_UPLOAD_STORE,Env:OTO_MLKIT_TRIAL,Env:OTO_GOOGLE_SCANNER,Env:OTO_DEPENDENCY_INTAKE -ErrorAction SilentlyContinue
    Remove-Item 'Env:ORG_GRADLE_PROJECT_kotlin.compiler.execution.strategy','Env:ORG_GRADLE_PROJECT_kotlin.daemon.jvmargs' -ErrorAction SilentlyContinue
}
