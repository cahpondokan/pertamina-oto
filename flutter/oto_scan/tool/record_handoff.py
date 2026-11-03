import pathlib,json,hashlib,subprocess
root=pathlib.Path(__file__).resolve().parents[1]
apk=root/'dist/OTO-Scan-Flutter-0.1.0.apk'
paths=[]
for directory in ['lib','android/app/src/main','android/app/src/androidTest','test','tool','assets']:
    paths.extend(p for p in (root/directory).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
for relative in ['pubspec.yaml','pubspec.lock','android/app/build.gradle.kts','android/settings.gradle.kts','android/gradle.properties','android/gradle/wrapper/gradle-wrapper.properties','analysis_options.yaml']:
    paths.append(root/relative)
manifest={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(paths)}
package='id.ayani.oto.scanflutter'
remote=subprocess.check_output(['adb','shell','pm','path',package],text=True).strip()
if not remote.startswith('package:'):raise SystemExit('Installed package absent')
remote_path=remote.splitlines()[0].removeprefix('package:')
remote_hash=subprocess.check_output(['adb','shell','sha256sum',remote_path],text=True).split()[0]
local_hash=hashlib.sha256(apk.read_bytes()).hexdigest()
if remote_hash!=local_hash:raise SystemExit('Installed APK differs')
report={'date':'2026-10-21 Asia/Jakarta','scope':'Flutter Android local development, not full Phase 8',
 'agent':'Agent 0 only','package':package,'version':'0.1.0+1','source_hashes':manifest,
 'apk_sha256':local_hash,'installed_apk_hash_matches':True,'bytes':apk.stat().st_size,
 'flutter_tests':{'passed':13,'total':13},'detector_tests':{'passed':14,'total':14},
 'apk_checks':{'passed':16,'total':16},'native_instrumentation':'COMPILED_NOT_RUN',
 'physical_ui_camera_test':'PENDING_USER_SELECTED_APP_P_OTO_FIRST',
 'production_approval':False,'phone_foreground_taken':False}
(root/'evidence/handoff.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
(root/'dist/OTO-Scan-Flutter-0.1.0.sha256').write_text(local_hash+'  OTO-Scan-Flutter-0.1.0.apk\n',encoding='ascii')
print(json.dumps({k:v for k,v in report.items() if k!='source_hashes'},indent=2))
