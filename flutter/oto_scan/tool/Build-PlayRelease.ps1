param([switch]$CreateUploadKey, [ValidateSet('appbundle','apk')][string]$Kind = 'appbundle')
$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot -Parent
$private = Join-Path $project '.release-private'
$output = Join-Path $project 'release-output/1.0.0'
$evidence = Join-Path $project 'evidence/play-release'
New-Item -ItemType Directory -Force $output,$evidence | Out-Null
$env:JAVA_HOME = 'C:/Program Files/Android/Android Studio/jbr'
$keytool = Join-Path $env:JAVA_HOME 'bin/keytool.exe'
$store = Join-Path $private 'oto-scan-upload.p12'
$secret = Join-Path $private 'password.dpapi'
if (-not (Test-Path -LiteralPath $store)) {
    if (-not $CreateUploadKey) { throw 'Upload key missing. Use -CreateUploadKey once for the new app.' }
    if (Test-Path -LiteralPath $private) { throw 'Private directory already exists. Inspect before creating a key.' }
    New-Item -ItemType Directory $private | Out-Null
    $sid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    & icacls.exe $private /inheritance:r /grant:r "*${sid}:(OI)(CI)F" '*S-1-5-18:(OI)(CI)F' | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Cannot protect upload-key directory' }
    $random = New-Object byte[] 48
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($random)
    $env:OTO_UPLOAD_PASSWORD = [Convert]::ToBase64String($random)
    ConvertTo-SecureString $env:OTO_UPLOAD_PASSWORD -AsPlainText -Force | ConvertFrom-SecureString | Set-Content -LiteralPath $secret
    & $keytool -genkeypair -keystore $store -storetype PKCS12 -alias oto-scan-upload -keyalg RSA -keysize 4096 -sigalg SHA256withRSA -validity 10000 -dname 'CN=OTO Scan Upload' -storepass:env OTO_UPLOAD_PASSWORD -keypass:env OTO_UPLOAD_PASSWORD 2>&1 | Out-File (Join-Path $evidence 'upload-key-creation.log')
    if ($LASTEXITCODE -ne 0) { throw 'Upload-key creation failed; preserve private directory for inspection' }
} else {
    $secure = (Get-Content -LiteralPath $secret -Raw).Trim() | ConvertTo-SecureString
    $cred = New-Object System.Management.Automation.PSCredential('upload',$secure)
    $env:OTO_UPLOAD_PASSWORD = $cred.GetNetworkCredential().Password
}
try {
    $env:OTO_UPLOAD_STORE = $store
    $env:OTO_SCAN_QA = 'false'
    $env:CI = 'true'
    $env:FLUTTER_SUPPRESS_ANALYTICS = 'true'
    $env:DART_SUPPRESS_ANALYTICS = 'true'
    $env:TEMP = Join-Path $evidence 'temp'
    $env:TMP = $env:TEMP
    New-Item -ItemType Directory -Force $env:TEMP | Out-Null
    & $keytool -exportcert -rfc -keystore $store -storepass:env OTO_UPLOAD_PASSWORD -alias oto-scan-upload -file (Join-Path $output 'upload-certificate.pem') 2>&1 | Out-File (Join-Path $evidence 'upload-certificate-export.log')
    if ($LASTEXITCODE -ne 0) { throw 'Certificate export failed' }
    Push-Location $project
    try {
        & 'D:/Flutter/flutter/bin/cache/dart-sdk/bin/dart.exe' 'D:/Flutter/flutter/bin/cache/flutter_tools.snapshot' --no-version-check build $Kind --release --no-pub --target-platform android-arm,android-arm64,android-x64 --dart-define=OTO_TEST_MODE=false 2>&1 | Tee-Object (Join-Path $evidence "build-$Kind.log")
        if ($LASTEXITCODE -ne 0) { throw 'Release build failed' }
        $source = if ($Kind -eq 'appbundle') { 'build/app/outputs/bundle/release/app-release.aab' } else { 'build/app/outputs/flutter-apk/app-release.apk' }
        $extension = if ($Kind -eq 'appbundle') { 'aab' } else { 'apk' }
        $target = Join-Path $output "OTO-Scan-1.0.0.$extension"
        Copy-Item -LiteralPath $source -Destination $target
        Get-FileHash -LiteralPath $target -Algorithm SHA256 | Select-Object Algorithm,Hash,Path | ConvertTo-Json | Set-Content (Join-Path $output "$extension-sha256.json")
    } finally { Pop-Location }
} finally {
    Remove-Item Env:OTO_UPLOAD_PASSWORD,Env:OTO_UPLOAD_STORE -ErrorAction SilentlyContinue
}
