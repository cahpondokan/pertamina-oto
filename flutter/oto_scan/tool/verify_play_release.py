"""Offline release checks. Does not touch any Android device or private keystore."""
import hashlib, json, pathlib, struct, subprocess, zipfile
import xml.etree.ElementTree as ET

root = pathlib.Path(__file__).resolve().parents[1]
out = root / 'evidence/play-release'
release = root / 'release-output/1.0.0'
sdk = pathlib.Path('C:/Users/cahpo/AppData/Local/Android/sdk')
jbr = pathlib.Path('C:/Program Files/Android/Android Studio/jbr/bin')
apk, aab = release/'OTO-Scan-1.0.0.apk', release/'OTO-Scan-1.0.0.aab'
checks, elf = {}, {}
ns = '{http://schemas.android.com/apk/res/android}'
manifest = ET.parse(out/'bundle-manifest.xml').getroot()
app = manifest.find('application')
uses = manifest.find('uses-sdk')
checks['package'] = manifest.get('package') == 'com.niam.ayani.oto'
checks['version'] = manifest.get(ns+'versionName') == '1.0.0' and manifest.get(ns+'versionCode') == '17'
checks['api36_android10_min'] = uses.get(ns+'targetSdkVersion') == '36' and uses.get(ns+'minSdkVersion') == '29'
permissions = {x.get(ns+'name') for x in manifest.findall('uses-permission')}
checks['camera_only_platform_permission'] = {p for p in permissions if p.startswith('android.permission.')} == {'android.permission.CAMERA'}
checks['only_private_receiver_permission_added'] = permissions - {'android.permission.CAMERA'} == {'com.niam.ayani.oto.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION'} and any(p.get(ns+'name') == 'com.niam.ayani.oto.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION' and p.get(ns+'protectionLevel') == '0x00000002' for p in manifest.findall('permission'))
checks['not_debuggable'] = app.get(ns+'debuggable', 'false') == 'false'
checks['backup_disabled'] = app.get(ns+'allowBackup') == 'false'
checks['no_cleartext'] = app.get(ns+'usesCleartextTraffic') == 'false'
checks['share_provider_private'] = all(p.get(ns+'exported') == 'false' for p in app.findall('provider') if 'ScanShareProvider' in p.get(ns+'name',''))
checks['qa_disabled'] = 'boolean QA = false' in (root/'build/app/generated/source/buildConfig/release/id/ayani/oto/oto_scan/BuildConfig.java').read_text()
checks['bundle_16kb_packaging'] = 'PAGE_ALIGNMENT_16K' in (out/'bundle-config.json').read_text()
with zipfile.ZipFile(aab) as bundle, zipfile.ZipFile(apk) as package:
    libs = [n for n in bundle.namelist() if n.endswith('.so')]
    checks['three_abis'] = {n.split('/')[2] for n in libs} == {'arm64-v8a','armeabi-v7a','x86_64'}
    checks['same_native_binaries_apk_aab'] = all(bundle.read(n) == package.read(n.removeprefix('base/')) for n in libs)
    dex_names = [n for n in bundle.namelist() if n.startswith('base/dex/') and n.endswith('.dex')]
    checks['same_dex_apk_aab'] = all(bundle.read(n) == package.read(n.removeprefix('base/dex/')) for n in dex_names)
    checks['no_test_classes'] = all(all(test_class not in bundle.read(n) for test_class in (b'ReleaseEmulatorChecks',b'ScanInstrumentation')) for n in dex_names)
    checks['no_signing_secrets'] = not any(n.lower().endswith(('.jks','.p12','.keystore','.dpapi')) for n in bundle.namelist())
    checks['brand_icon'] = bundle.read('base/assets/flutter_assets/assets/oto.png') == (root/'assets/oto.png').read_bytes()
    checks['licenses'] = all(n in bundle.namelist() for n in ['base/assets/flutter_assets/NOTICES.Z','base/assets/flutter_assets/assets/FLUTTER_LICENSE.txt','base/assets/flutter_assets/assets/THIRD_PARTY_NOTICES.txt'])
    for name in libs:
        data = bundle.read(name)
        assert data[:4] == b'\x7fELF'
        bits, endian = data[4], '<' if data[5] == 1 else '>'
        phoff = struct.unpack_from(endian+('Q' if bits == 2 else 'I'),data,32 if bits == 2 else 28)[0]
        phsize, phnum = struct.unpack_from(endian+'HH',data,54 if bits == 2 else 42)
        loads=[]
        for i in range(phnum):
            h = struct.unpack_from(endian+('IIQQQQQQ' if bits == 2 else 'IIIIIIII'),data,phoff+i*phsize)
            if h[0] != 1: continue
            offset, virtual, align = (h[2],h[3],h[7]) if bits == 2 else (h[1],h[2],h[7])
            loads.append({'alignment':align,'offset':offset,'virtual':virtual,'pass':align>=16384 and offset%16384==virtual%16384})
        elf[name] = loads
    checks['elf_load_segments_16kb'] = all(rows and all(x['pass'] for x in rows) for rows in elf.values())
sign = subprocess.run([str(sdk/'build-tools/36.0.0/apksigner.bat'),'verify','--verbose','--print-certs',str(apk)],capture_output=True,text=True,encoding='utf-8')
(out/'apk-signature.txt').write_text(sign.stdout+sign.stderr,encoding='utf-8')
checks['apk_signature_valid'] = sign.returncode == 0
checks['not_historical_test_certificate'] = '14cc8947fc0bbe31d006ccf3b0b0d9053bd9f40b8d83f3c72cead1c249201547' not in sign.stdout
signed = subprocess.run([str(jbr/'jarsigner.exe'),'-verify',str(aab)],capture_output=True,text=True,encoding='utf-8')
(out/'aab-signature.txt').write_text(signed.stdout+signed.stderr,encoding='utf-8')
checks['aab_signature_valid'] = signed.returncode == 0 and 'jar verified' in signed.stdout
align = subprocess.run([str(sdk/'build-tools/36.0.0/zipalign.exe'),'-c','-P','16','-v','4',str(apk)],capture_output=True,text=True)
(out/'apk-alignment.txt').write_text(align.stdout+align.stderr,encoding='utf-8')
checks['apk_zipalign_16kb'] = align.returncode == 0
report = {'checks':checks,'passed':sum(checks.values()),'total':len(checks),'elf':elf,'artifacts':{p.name:{'bytes':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in (apk,aab)}}
(out/'release-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({'checks':checks,'passed':sum(checks.values()),'total':len(checks)},indent=2))
raise SystemExit(0 if all(checks.values()) else 1)
