param([ValidateSet('native','release-ui')][string]$Mode='native',[switch]$Install)
$ErrorActionPreference='Stop'
$project=Split-Path $PSScriptRoot -Parent
$adb='C:/Users/cahpo/AppData/Local/Android/sdk/platform-tools/adb.exe'
$serial='emulator-5580'
$package='com.niam.ayani.oto'
$evidence=Join-Path $project 'evidence/play-release'
function Invoke-Emulator([string[]]$Command){
    $result=& $adb -s $serial @Command 2>&1
    if($LASTEXITCODE -ne 0){throw ($result -join "`n")}
    return $result
}
$qemu=(Invoke-Emulator -Command @('shell','getprop','ro.kernel.qemu')) -join ''
if($qemu.Trim() -ne '1'){throw 'Refusing non-emulator target'}
if(((Invoke-Emulator -Command @('shell','getprop','sys.boot_completed')) -join '').Trim() -ne '1'){throw 'Emulator is not booted'}
if($Install){
    $installed=(Invoke-Emulator -Command @('shell','pm','list','packages',$package)) -join ''
    if($installed.Trim()){throw 'Existing app found; do not overwrite emulator user data'}
    Invoke-Emulator -Command @('install','-g',(Join-Path $project 'release-output/1.0.0/OTO-Scan-1.0.0.apk'))
    Invoke-Emulator -Command @('install','-t',(Join-Path $project 'build/app/outputs/apk/androidTest/release/app-release-androidTest.apk'))
}
$result=Invoke-Emulator -Command @('shell','am','instrument','-w','-e','mode',$Mode,"${package}.test/id.ayani.oto.oto_scan.ScanInstrumentation")
$result | Set-Content (Join-Path $evidence "emulator-$Mode.log")
$result
if(($result -join "`n") -notmatch 'checks PASS'){throw 'Instrumentation did not confirm PASS; inspect QA output'}
