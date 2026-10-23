import json,hashlib,pathlib,subprocess,zipfile,sys,os
import xml.etree.ElementTree as ET
root=pathlib.Path(__file__).resolve().parents[1]
folder=root/'evidence'/(sys.argv[1] if len(sys.argv)>1 else 'candidate')
apk=folder/'oto-scan-flutter.apk'
aapt=pathlib.Path(r'C:\Users\cahpo\AppData\Local\Android\sdk\build-tools\36.0.0\aapt2.exe')
badging=subprocess.check_output([str(aapt),'dump','badging',str(apk)],text=True,encoding='utf-8')
manifest=subprocess.check_output([str(aapt),'dump','xmltree','--file','AndroidManifest.xml',str(apk)],text=True,encoding='utf-8')
checks={}
checks['correct_package']="name='com.niam.ayani.oto'" in badging
analyzer=pathlib.Path(r'C:\Users\cahpo\AppData\Local\Android\sdk\cmdline-tools\latest\bin\apkanalyzer.bat')
decoded=subprocess.check_output([str(analyzer),'manifest','print',str(apk)],text=True,encoding='utf-8',env={**os.environ,'JAVA_HOME':r'C:\Program Files\Android\Android Studio\jbr'})
tree=ET.fromstring(decoded)
android='{http://schemas.android.com/apk/res/android}'
activity=next(a for a in tree.findall('application/activity') if a.get(android+'name')=='id.ayani.oto.oto_scan.MainActivity')
filters=[f for f in activity.findall('intent-filter') if any(a.get(android+'name')=='android.intent.action.VIEW' for a in f.findall('action'))]
checks['pdf_view_handler']=len(filters)==1 and {c.get(android+'name') for c in filters[0].findall('category')}=={'android.intent.category.DEFAULT','android.intent.category.BROWSABLE'} and {d.get(android+'mimeType') for d in filters[0].findall('data') if d.get(android+'mimeType')}=={'application/pdf'}
checks['local_pdf_schemes_only']=len(filters)==1 and {d.get(android+'scheme') for d in filters[0].findall('data') if d.get(android+'scheme')}=={'content','file'}
checks['flutter_default_deeplinks_disabled']=any(m.get(android+'name')=='flutter_deeplinking_enabled' and m.get(android+'value')=='false' for m in activity.findall('meta-data'))
checks['new_share_authority']=any(p.get(android+'authorities')=='com.niam.ayani.oto.share' and p.get(android+'exported')=='false' and p.get(android+'grantUriPermissions')=='true' for p in tree.findall('application/provider'))
checks['new_app_label']=tree.find('application').get(android+'label')=='OTO Scan'
checks['android_10_minimum']="minSdkVersion:'29'" in badging
checks['target_api_35']="targetSdkVersion:'35'" in badging
checks['camera_permission']="android.permission.CAMERA" in badging
checks['no_internet_permission']='android.permission.INTERNET' not in badging
checks['no_storage_or_microphone_permission']=all(x not in badging for x in ['READ_EXTERNAL_STORAGE','WRITE_EXTERNAL_STORAGE','MANAGE_EXTERNAL_STORAGE','RECORD_AUDIO'])
checks['backup_disabled']='android:allowBackup' in manifest and any('android:allowBackup' in l and '0x0' in l for l in manifest.splitlines())
checks['provider_not_exported']='ScanShareProvider' in manifest and 'android:grantUriPermissions' in manifest
with zipfile.ZipFile(apk) as z:
    names=z.namelist()
    checks['flutter_engine_and_aot']=all(x in names for x in ['lib/arm64-v8a/libapp.so','lib/arm64-v8a/libflutter.so'])
    checks['arm64_only']=all('/arm64-v8a/' in n for n in names if n.startswith('lib/'))
    checks['original_oto_icon']=z.read('assets/flutter_assets/assets/oto.png')==(root/'assets/oto.png').read_bytes()
    checks['flutter_license_bundled']='assets/flutter_assets/assets/FLUTTER_LICENSE.txt' in names
    checks['no_signing_secrets']=not any(n.lower().endswith(('.p12','.jks','.keystore','.dpapi')) for n in names)
cfg=root/'build/app/generated/source/buildConfig/release/id/ayani/oto/oto_scan/BuildConfig.java'
checks['qa_disabled']='boolean QA = false' in cfg.read_text()
receipt=json.loads((folder/'receipt.json').read_text(encoding='utf-8-sig'))
checks['verified_test_signature']=receipt['signature_verified'] and receipt['aligned_16kb']
checks['signed_hash_matches']=hashlib.sha256(apk.read_bytes()).hexdigest().upper()==receipt['signed_sha256']
result={'checks':checks,'passed':sum(checks.values()),'total':len(checks),'bytes':apk.stat().st_size,'sha256':hashlib.sha256(apk.read_bytes()).hexdigest()}
(folder/'apk-verification.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
(folder/'manifest.txt').write_text(manifest,encoding='utf-8')
(folder/'manifest.xml').write_text(decoded,encoding='utf-8')
print(json.dumps(result,indent=2))
if not all(checks.values()):raise SystemExit(1)
