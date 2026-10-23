param([ValidatePattern('^[a-z0-9-]+$')][string]$Kind,
 [switch]$Instrumentation)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$worktree=[IO.Path]::GetFullPath((Join-Path $project '../..'))
$previous=Join-Path $worktree 'artifacts/android-scan/A1/device-evaluation'
$authority=Get-Content -LiteralPath "$previous/authorization.json" -Raw | ConvertFrom-Json
if($authority.status -ne 'APPROVED'){throw 'Existing test-device authority absent'}
$source=Join-Path $project 'build/app/outputs/flutter-apk/app-release.apk'
if($Instrumentation){$source=Join-Path $project 'build/app/outputs/apk/androidTest/release/app-release-androidTest.apk'}
$target=Join-Path $project "evidence/$Kind"
New-Item -ItemType Directory -Force -Path $target | Out-Null
if(Test-Path -LiteralPath "$target/oto-scan-flutter.apk"){throw 'Candidate already signed; choose a new evidence location'}
$bt='C:/Users/cahpo/AppData/Local/Android/sdk/build-tools/36.0.0'
$java='C:/Program Files/Android/Android Studio/jbr/bin/java.exe'
& "$bt/zipalign.exe" -P 16 4 $source "$target/aligned.apk"
if($LASTEXITCODE -ne 0){throw 'Alignment failed'}
$secure=(Get-Content -LiteralPath "$previous/private/password.dpapi" -Raw).Trim() | ConvertTo-SecureString
$env:OTO_FLUTTER_SIGN_PASS=[Net.NetworkCredential]::new('', $secure).Password
try{
  & $java -jar "$bt/lib/apksigner.jar" sign --ks "$previous/private/a1-test.p12" --ks-key-alias oto-a1-test --ks-pass env:OTO_FLUTTER_SIGN_PASS --key-pass env:OTO_FLUTTER_SIGN_PASS --v4-signing-enabled false --out "$target/oto-scan-flutter.apk" "$target/aligned.apk"
  if($LASTEXITCODE -ne 0){throw 'Signing failed'}
  & $java -jar "$bt/lib/apksigner.jar" verify --verbose --print-certs "$target/oto-scan-flutter.apk" *> "$target/signature.log"
  if($LASTEXITCODE -ne 0){throw 'Signature verification failed'}
  & "$bt/zipalign.exe" -c -P 16 4 "$target/oto-scan-flutter.apk"
  if($LASTEXITCODE -ne 0){throw 'Alignment check failed'}
  $prior=Get-Content -LiteralPath "$previous/apk-signature-verification.log" -Raw
  $current=Get-Content -LiteralPath "$target/signature.log" -Raw
  $pattern='certificate SHA-256 digest: ([0-9a-f]+)'
  $cert=[regex]::Match($current,$pattern).Groups[1].Value
  if(!$cert -or $cert -ne [regex]::Match($prior,$pattern).Groups[1].Value){throw 'Unexpected signer'}
  [ordered]@{kind=$Kind;package='com.niam.ayani.oto';authority='AND-A1-DEVICE-001 + AND-FLUTTER-20261021 + user package rename';unsigned_sha256=(Get-FileHash -LiteralPath $source).Hash;signed_sha256=(Get-FileHash -LiteralPath "$target/oto-scan-flutter.apk").Hash;certificate_sha256=$cert;signature_verified=$true;aligned_16kb=$true} | ConvertTo-Json | Set-Content -LiteralPath "$target/receipt.json"
  Get-Content -LiteralPath "$target/receipt.json"
}finally{Remove-Item Env:OTO_FLUTTER_SIGN_PASS -ErrorAction SilentlyContinue;$secure.Dispose()}
