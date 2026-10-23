"""Source preservation audit and installation handoff for r11."""
from pathlib import Path
import difflib,hashlib,json
root=Path(__file__).resolve().parents[1];e=root/'evidence'
base=json.loads((e/'r11-baseline/source-sha256.json').read_text(encoding='utf-8-sig'))
native='android/app/src/main/java/id/ayani/oto/oto_scan/'
allowed={'lib/document_gallery.dart','lib/main.dart','lib/pdf_reader.dart','pubspec.yaml',
    'test/gallery_test.dart','test/navigation_test.dart','test/pdf_open_test.dart','test/scan_test.dart',
    *(native+n for n in ['ExternalPdf.java','MainActivity.java','ScanStore.java'])}
added=['lib/pdf_reading.dart','lib/pdf_canvas.dart','test/reader_fixture.dart','test/pdf_reading_test.dart',
    'test/pdf_reader_visual_test.dart','test/native/ReaderCacheTest.java','tool/check_reader_pdf.py',
    'tool/Install-R11.ps1','tool/record_reader_handoff.py',
    *(native+n for n in ['ReaderPolicy.java','ReaderDiskCache.java','PdfReaderEngine.java'])]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
current={k:sha(root/k) for k in base};changed=[k for k,v in base.items() if current[k]!=v]
assert not set(changed)-allowed,set(changed)-allowed
before=(e/'r11-baseline'/native/'ScanStore.java').read_text()
after=(root/native/'ScanStore.java').read_text()
body=lambda s:s[s.index('  Map<String,Object> save('):s.index('  Map<String,Object> info(')]
assert body(before).replace('return info(id,true);','return info(id,false,true);')==body(after),'PDF creation changed beyond removing unused preview'
diff=[]
for name in changed+added:
    old=(e/'r11-baseline'/name).read_text().splitlines(True) if name in changed else []
    diff.extend(difflib.unified_diff(old,(root/name).read_text().splitlines(True),fromfile='r10/'+name,tofile='r11/'+name))
    current[name]=sha(root/name)
result={'changed':changed,'added':added,'unchanged':[k for k in base if k not in changed],
    'unexpected_changes':[],'pdf_creation_identical_except_unused_return_preview':True,'source_sha256':current}
(e/'r11-preservation.json').write_text(json.dumps(result,indent=2))
(e/'r11-change-review.diff').write_text(''.join(diff),encoding='utf-8')
print('PASS source preservation: camera, detector, enhancement, PDF encoding, icon, permissions, dependency lock unchanged.')
receipt=e/'r11-installation.json'
if receipt.exists():
    install=json.loads(receipt.read_text(encoding='utf-8-sig'));apk=json.loads((e/'r11/apk-verification.json').read_text())
    assert install['artifact_sha256']==install['installed_sha256']==apk['sha256']
    assert 'All tests passed!' in (e/'r11-flutter-tests.log').read_text(encoding='utf-8-sig')
    assert 'No issues found!' in (e/'r11-analyze.log').read_text(encoding='utf-8-sig')
    files=['r11-flutter-tests.log','r11-reader-tests.log','r11-analyze.log','r11-build.log','r11-ReaderCacheTest.log',
        'r11-DocumentBatchTest.log','r11-preservation.json','r11-change-review.diff','r11-reader-desktop.png',
        'r11-reader-page2-desktop.png','r11-reader/measurements.json','r11/apk-verification.json','r11-installation.json']
    handoff={'revision':'r11','version':'0.1.10+11','status':'INSTALLED_AND_LAUNCHED_USER_TEST_PENDING',
        'objective':'Continuous vertical PDF reader, screen-first preview, on-demand detail, reusable bounded disk cache',
        'installation':install,'verification':{'flutter_tests':67,'java_reader_checks':23,'java_batch_checks':16,
        'dart_analysis':'PASS','android_release_build':'PASS','apk_checks':apk['passed'],'phone_runtime_tests':'NOT_RUN_USER_REQUEST'},
        'source_sha256':current,'evidence_sha256':{name:sha(e/name) for name in files},
        'limits':['Desktop PDFium/Pillow timings do not measure Android PdfRenderer or phone speed.',
        'One native render already in progress finishes; subsequent obsolete work is skipped.',
        'The initial preview may still take time for external/complex PDFs or a cold cache.',
        'Raster detail stays bounded to 8 MP/3800 pixels; zoom does not create detail absent in source.',
        'Viewer retains nearby pages and one active detail page; 48 MiB is a soft raster-cache target, not a process-memory guarantee.'],
        'no_phone_ui_test':True,'no_dependency_added':True,'no_publication':True,'no_phase_promotion':True}
    (e/'install-handoff-r11.json').write_text(json.dumps(handoff,indent=2))
    print('PASS installation handoff recorded.')
