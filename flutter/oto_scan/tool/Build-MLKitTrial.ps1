param([switch]$AllowDependencyDownload, [switch]$Instrumentation)
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$evidence = Join-Path $project 'evidence/mlkit-trial'
$output = Join-Path $project 'dist'
$store = Join-Path $project '.release-private/oto-scan-upload.p12'
$secret = Join-Path $project '.release-private/password.dpapi'
if (!(Test-Path -LiteralPath $store) -or !(Test-Path -LiteralPath $secret)) { throw 'Existing release signer is required.' }
New-Item -ItemType Directory -Force $evidence,$output | Out-Null
$env:JAVA_HOME = 'C:/Program Files/Android/Android Studio/jbr'
$secure = (Get-Content -LiteralPath $secret -Raw).Trim() | ConvertTo-SecureString
$credential = New-Object System.Management.Automation.PSCredential('upload',$secure)
try {
    $env:OTO_UPLOAD_PASSWORD = $credential.GetNetworkCredential().Password
    $env:OTO_UPLOAD_STORE = $store
    $env:OTO_SCAN_QA = 'false'
    $env:OTO_MLKIT_TRIAL = 'true'
    $env:OTO_DEPENDENCY_INTAKE = if ($AllowDependencyDownload) { 'true' } else { 'false' }
    $env:CI = 'true'
    $env:FLUTTER_SUPPRESS_ANALYTICS = 'true'
    $env:DART_SUPPRESS_ANALYTICS = 'true'
    Push-Location $project
    try {
        if ($Instrumentation) {
            Push-Location android
            try {
                $defines = ((@('OTO_TEST_MODE=false','OTO_MLKIT_TRIAL=true') | ForEach-Object { [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_)) }) -join ',')
                & ./gradlew.bat --no-daemon --console plain "-Pdart-defines=$defines" '-Ptarget-platform=android-arm64,android-x64' :app:assembleReleaseAndroidTest 2>&1 | Tee-Object (Join-Path $evidence 'build-instrumentation.log')
                if ($LASTEXITCODE -ne 0) { throw 'Trial instrumentation build failed' }
            } finally { Pop-Location }
        } else {
            & 'D:/Flutter/flutter/bin/cache/dart-sdk/bin/dart.exe' 'D:/Flutter/flutter/bin/cache/flutter_tools.snapshot' --no-version-check build apk --release --no-pub --target-platform android-arm64,android-x64 --build-name=1.0.1-mlkit.1 --build-number=18 --dart-define=OTO_TEST_MODE=false --dart-define=OTO_MLKIT_TRIAL=true 2>&1 | Tee-Object (Join-Path $evidence 'build-apk.log')
            if ($LASTEXITCODE -ne 0) { throw 'Trial APK build failed' }
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            $zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $project 'build/app/outputs/flutter-apk/app-release.apk'))
            try {
                foreach ($abi in @('arm64-v8a','x86_64')) {
                    foreach ($library in @('libapp.so','libflutter.so')) {
                        $entry = $zip.GetEntry("lib/$abi/$library")
                        if ($null -eq $entry -or $entry.Length -lt 1000000) { throw "Incomplete APK: missing $abi/$library" }
                    }
                }
            } finally { $zip.Dispose() }
            $target = Join-Path $output 'OTO-Scan-Uji-MLKit-1.0.1.apk'
            Copy-Item -LiteralPath 'build/app/outputs/flutter-apk/app-release.apk' -Destination $target
            Get-FileHash -LiteralPath $target -Algorithm SHA256 | Select-Object Algorithm,Hash,Path | ConvertTo-Json | Set-Content (Join-Path $evidence 'apk-sha256.json')
        }
    } finally { Pop-Location }
} finally {
    Remove-Item Env:OTO_UPLOAD_PASSWORD,Env:OTO_UPLOAD_STORE,Env:OTO_MLKIT_TRIAL,Env:OTO_DEPENDENCY_INTAKE -ErrorAction SilentlyContinue
}
