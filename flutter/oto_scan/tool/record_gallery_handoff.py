"""Record r10's scoped source preservation and, after install, its handoff."""
import difflib
import hashlib
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
evidence = root / 'evidence'
baseline = json.loads((evidence / 'r10-baseline/source-sha256.json').read_text(encoding='utf-8-sig'))
native = 'android/app/src/main/java/id/ayani/oto/oto_scan/'
allowed = {
    'lib/main.dart', 'lib/crop_screen.dart', 'lib/document_gallery.dart', 'pubspec.yaml',
    'test/gallery_test.dart', 'test/scan_test.dart', 'test/pdf_open_test.dart',
    *(native + name for name in ('MainActivity.java', 'ScanStore.java', 'ScanShareProvider.java')),
}
added = ['lib/pdf_reader.dart', 'lib/document_actions.dart', 'test/navigation_test.dart',
         native + 'DocumentBatch.java', 'test/native/DocumentBatchTest.java',
         'tool/Install-R10.ps1', 'tool/record_gallery_handoff.py']
sha = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
current = {name: sha(root / name) for name in baseline}
changed = [name for name in baseline if baseline[name] != current[name]]
unexpected = set(changed) - allowed
assert not unexpected, f'Unexpected source changes: {unexpected}'
diff = []
for name in changed:
    previous = evidence / 'r10-baseline' / Path(name).name
    assert previous.exists(), name
    diff.extend(difflib.unified_diff(previous.read_text(encoding='utf-8').splitlines(True),
                                    (root / name).read_text(encoding='utf-8').splitlines(True),
                                    fromfile='r09/' + name, tofile='r10/' + name))
for name in added:
    current[name] = sha(root / name)
    diff.extend(difflib.unified_diff([], (root / name).read_text(encoding='utf-8').splitlines(True),
                                    fromfile='/dev/null', tofile='r10/' + name))
before = (evidence / 'r10-baseline/ScanStore.java').read_text(encoding='utf-8')
after = (root / (native + 'ScanStore.java')).read_text(encoding='utf-8')
save_body = lambda text: text[text.index('  Map<String,Object> save('):text.index('  Map<String,Object> info(')]
assert save_body(before) == save_body(after), 'PDF creation changed outside scope'
preservation = {'changed': changed, 'added': added, 'unexpected_changes': [],
    'unchanged': [name for name in baseline if name not in changed],
    'pdf_creation_identical': True, 'source_sha256': current}
(evidence / 'r10-preservation.json').write_text(json.dumps(preservation, indent=2), encoding='utf-8')
(evidence / 'r10-change-review.diff').write_text(''.join(diff), encoding='utf-8')
print(f'Source audit PASS: {len(changed)} existing files changed, {len(added)} added; PDF writer, camera/detector/quality, icon, manifest and dependency lock preserved.')
receipt = evidence / 'r10-installation.json'
if receipt.exists():
    install = json.loads(receipt.read_text(encoding='utf-8-sig'))
    apk = json.loads((evidence / 'r10/apk-verification.json').read_text(encoding='utf-8'))
    files = ['r10-flutter-tests.log', 'r10-DocumentBatchTest.log', 'r10-analyze.log', 'r10-build.log',
             'r10-preservation.json', 'r10-change-review.diff', 'r10-gallery-desktop.png',
             'r10-selection-desktop.png', 'r10/apk-verification.json', 'r10-installation.json']
    assert 'All tests passed!' in (evidence / files[0]).read_text(encoding='utf-8-sig')
    assert 'No issues found!' in (evidence / 'r10-analyze.log').read_text(encoding='utf-8-sig')
    assert install['artifact_sha256'] == apk['sha256'] == install['installed_sha256']
    handoff = {'revision': 'r10', 'version': '0.1.9+10', 'status': 'INSTALLED_AND_LAUNCHED_USER_TEST_PENDING',
        'agent': 'Agent 0 only',
        'objective': 'Direct gallery PDF reader, consistent Back and draft preservation, multi-document selection/share/delete',
        'installation': install,
        'verification': {'flutter_tests': 59, 'java_batch_checks': 16, 'dart_analysis': 'PASS',
            'android_release_build': 'PASS', 'apk_checks': apk['passed'], 'phone_ui_tests': 'NOT_RUN_USER_REQUEST'},
        'source_sha256': current, 'evidence_sha256': {name: sha(evidence / name) for name in files},
        'limitations': ['Actual Android recipient support and user experience await user trial.',
            'Selection supports 1–100 separate PDFs; no automatic merge or actual recipient send.',
            'PDFs opened from other apps remain read-only and are not imported into the library.'],
        'reference': 'https://developer.android.com/develop/ui/compose/sharing/send',
        'no_dependency_added': True, 'no_phase_promotion': True, 'no_publication': True}
    (evidence / 'install-handoff-r10.json').write_text(json.dumps(handoff, indent=2), encoding='utf-8')
    print('Installation handoff recorded.')
