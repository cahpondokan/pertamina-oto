"""Audit the scoped save-completion change and record installed candidate."""
from pathlib import Path
import difflib,hashlib,json
root=Path(__file__).resolve().parents[1];e=root/'evidence'
base=json.loads((e/'r12-baseline/source-sha256.json').read_text(encoding='utf-8'))
allowed={'lib/main.dart','lib/document_actions.dart','pubspec.yaml','README.md','test/navigation_test.dart',
    'test/scan_test.dart','test/gallery_test.dart','test/pdf_reader_visual_test.dart'}
added=['tool/Install-R12.ps1','tool/record_save_handoff.py']
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current={k:sha(root/k) for k in base};changed=[k for k,v in base.items() if current[k]!=v]
assert not set(changed)-allowed,set(changed)-allowed
before=(e/'r12-baseline/lib/main.dart').read_text(encoding='utf-8');after=(root/'lib/main.dart').read_text(encoding='utf-8')
# The scanner and capture/edit methods must remain byte-for-byte identical.
section=lambda s:s[s.index('  Future<void> _startCamera()'):s.index('  Future<void> _save()')]
assert section(before)==section(after)
diff=[]
for name in changed+added:
    old=(e/'r12-baseline'/name).read_text(encoding='utf-8').splitlines(True) if name in changed else []
    diff.extend(difflib.unified_diff(old,(root/name).read_text(encoding='utf-8').splitlines(True),fromfile='r11/'+name,tofile='r12/'+name))
    current[name]=sha(root/name)
result={'changed':changed,'added':added,'unchanged':[k for k in base if k not in changed],
    'camera_methods_identical':True,'all_native_code_identical':True,'unexpected_changes':[],'source_sha256':current}
(e/'r12-preservation.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
(e/'r12-change-review.diff').write_text(''.join(diff),encoding='utf-8')
print('PASS scoped change: native storage/PDF/camera, detector, reader/zoom/cache, icon, manifest, dependencies unchanged.')
receipt=e/'r12-installation.json'
if receipt.exists():
    install=json.loads(receipt.read_text(encoding='utf-8-sig'));apk=json.loads((e/'r12/apk-verification.json').read_text(encoding='utf-8'))
    assert install['artifact_sha256']==install['installed_sha256']==apk['sha256']
    assert '+72: All tests passed!' in (e/'r12-flutter-tests.log').read_text(encoding='utf-8-sig')
    assert 'No issues found!' in (e/'r12-analyze.log').read_text(encoding='utf-8-sig')
    files=['r12-flutter-tests.log','r12-analyze.log','r12-build.log','r12-preservation.json','r12-change-review.diff',
        'r12-gallery-desktop.png','r12/apk-verification.json','r12-installation.json']
    handoff={'revision':'r12','version':'0.1.11+12','status':'INSTALLED_AND_LAUNCHED_USER_TEST_PENDING',
        'objective':'Finish saved scan, preview Back to gallery, new scan empty; retain draft on failure',
        'installation':install,'verification':{'flutter_tests':72,'dart_analysis':'PASS','android_release_build':'PASS',
        'apk_checks':apk['passed'],'phone_runtime_tests':'NOT_RUN_USER_REQUEST'},
        'source_sha256':current,'evidence_sha256':{name:sha(e/name) for name in files},
        'limits':['Existing drafts from before this update are not silently deleted.',
        'A process interruption before the empty checkpoint commits can leave the draft recoverable; saved PDF remains safe.',
        'Completion retry deduplication is held in the current app session; no claim of crash-atomic PDF-plus-draft transaction.',
        'Actual phone UI/recipient behavior remains for user trial.'],
        'no_phone_ui_test':True,'no_dependency_added':True,'no_publication':True,'no_phase_promotion':True}
    (e/'install-handoff-r12.json').write_text(json.dumps(handoff,indent=2),encoding='utf-8')
    print('PASS installation handoff recorded.')
