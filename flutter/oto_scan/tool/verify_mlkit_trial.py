"""Read-only signed APK checks; never install or inspect a connected device."""
from pathlib import Path
import hashlib,json,os,struct,subprocess,zipfile,xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[1];out=root/'evidence/mlkit-trial'
apk=root/'dist/OTO-Scan-Uji-MLKit-1.0.1.apk'
sdk=Path('C:/Users/cahpo/AppData/Local/Android/sdk')
env={**os.environ,'JAVA_HOME':'C:/Program Files/Android/Android Studio/jbr'}
def run(command):
    p=subprocess.run(list(map(str,command)),capture_output=True,text=True,encoding='utf-8',env=env)
    if p.returncode: raise RuntimeError(p.stderr or p.stdout)
    return p.stdout
xml=run([sdk/'cmdline-tools/latest/bin/apkanalyzer.bat','manifest','print',apk]);(out/'apk-manifest.xml').write_text(xml,encoding='utf-8')
tree=ET.fromstring(xml);ns='{http://schemas.android.com/apk/res/android}';app=tree.find('application');uses=tree.find('uses-sdk')
checks={
    'separate_package':tree.get('package')=='com.niam.ayani.oto.uji',
    'trial_label':app.get(ns+'label')=='OTO Scan Uji',
    'version':tree.get(ns+'versionName')=='1.0.1-mlkit.1' and tree.get(ns+'versionCode')=='18',
    'api_levels':uses.get(ns+'minSdkVersion')=='29' and uses.get(ns+'targetSdkVersion')=='36',
    'not_debuggable':app.get(ns+'debuggable','false')=='false',
    'backup_disabled':app.get(ns+'allowBackup')=='false',
    'cleartext_disabled':app.get(ns+'usesCleartextTraffic')=='false',
    'private_share_provider':any(x.get(ns+'authorities')=='com.niam.ayani.oto.uji.share' and x.get(ns+'exported')=='false' for x in app.findall('provider')),
}
permissions=[p.get(ns+'name') for p in tree.findall('uses-permission')]
checks['no_microphone_broad_storage_or_contacts']=not any(any(s in p for s in ('RECORD_AUDIO','EXTERNAL_STORAGE','READ_CONTACTS','READ_MEDIA_')) for p in permissions)
cfg=(root/'build/app/generated/source/buildConfig/release/id/ayani/oto/oto_scan/BuildConfig.java').read_text()
checks['qa_off_mlkit_on']='boolean QA = false' in cfg and 'boolean MLKIT_TRIAL = true' in cfg
with zipfile.ZipFile(apk) as z:
    names=z.namelist();dex=b''.join(z.read(n) for n in names if n.endswith('.dex'))
    checks['no_test_classes']=not any(n in dex for n in (b'ScanInstrumentation',b'GoogleImportChecks',b'GoogleUiChecks'))
    checks['no_signing_secrets']=not any(n.lower().endswith(('.p12','.jks','.keystore','.dpapi')) for n in names)
    checks['arm64_and_emulator']={n.split('/')[1] for n in names if n.startswith('lib/') and n.endswith('.so')}=={'arm64-v8a','x86_64'}
    checks['flutter_engine_and_aot_for_each_abi']=all(f'lib/{abi}/{lib}' in names and z.getinfo(f'lib/{abi}/{lib}').file_size>1_000_000 for abi in ('arm64-v8a','x86_64') for lib in ('libflutter.so','libapp.so'))
    checks['matching_aot_architecture']=checks['flutter_engine_and_aot_for_each_abi'] and all(struct.unpack_from('<H',z.read(f'lib/{abi}/libapp.so'),18)[0]==machine for abi,machine in (('arm64-v8a',183),('x86_64',62)))
    checks['original_icon']=z.read('assets/flutter_assets/assets/oto.png')==(root/'assets/oto.png').read_bytes()
    checks['google_notice']='assets/flutter_assets/assets/MLKIT_NOTICE.txt' in names
    checks['google_scanner_sdk_present']=b'com/google/mlkit/vision/documentscanner' in dex
signature=run([sdk/'build-tools/36.0.0/apksigner.bat','verify','--verbose','--print-certs',apk]);(out/'apk-signature.txt').write_text(signature)
checks['valid_signature_not_debug']='Verified using v2 scheme (APK Signature Scheme v2): true' in signature and 'Android Debug' not in signature
alignment=run([sdk/'build-tools/36.0.0/zipalign.exe','-c','-P','16','-v','4',apk]);(out/'apk-alignment.txt').write_text(alignment)
checks['16kb_zip_alignment']='Verification successful' in alignment
report={'checks':checks,'permissions':permissions,'passed':sum(checks.values()),'total':len(checks),'bytes':apk.stat().st_size,'sha256':hashlib.sha256(apk.read_bytes()).hexdigest()}
(out/'apk-verification.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
raise SystemExit(0 if all(checks.values()) else 1)
