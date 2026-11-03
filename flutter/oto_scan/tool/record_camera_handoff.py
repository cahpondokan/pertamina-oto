import pathlib,json,hashlib,subprocess
root=pathlib.Path(__file__).resolve().parents[1]
apk=root/'dist/OTO-Scan-Flutter-0.1.2.apk'
pkg='id.ayani.oto.scanflutter'
path=subprocess.check_output(['adb','shell','pm','path',pkg],text=True).splitlines()[0].removeprefix('package:')
remote=subprocess.check_output(['adb','shell','sha256sum',path],text=True).split()[0]
digest=hashlib.sha256(apk.read_bytes()).hexdigest()
assert remote==digest
paths=[]
for directory in ['lib','android/app/src/main','android/app/src/androidTest','test','tool','assets']:
    paths.extend(p for p in (root/directory).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
paths += [root/p for p in ['pubspec.yaml','pubspec.lock','android/app/build.gradle.kts','android/app/proguard-rules.pro']]
report={'version':'0.1.2+3','package':pkg,'apk_sha256':digest,'installed_apk_matches':True,
 'bytes':apk.stat().st_size,'agent':'Agent 0 only','real_camera_crop':'PASS_ONE_CURRENT_DOCUMENT',
 'manual_corner_edits':0,'save_and_reopen':'PASS','share_chooser':'PASS_CANCELLED_WITHOUT_RECIPIENT',
 'native_checks':24,'detector_checks':20,'apk_checks':16,
 'flutter_checks':'13 passed in original build; Dart UI source unchanged in this revision',
 'remaining':'Broader lighting/document/device matrix, fresh-install offline, export-to-selected-folder and performance acceptance not completed.',
 'production_approval':False,'source_sha256':{p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(paths)}}
(root/'evidence/camera-handoff-r03.json').write_text(json.dumps(report,indent=2))
print(json.dumps({k:v for k,v in report.items() if k!='source_sha256'},indent=2))
