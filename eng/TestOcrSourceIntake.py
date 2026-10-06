"""Synthetic archive guard checks only; never runs OCR or acquired code."""
import importlib.util
import io
import json
import pathlib
import tarfile
import uuid

spec = importlib.util.spec_from_file_location('intake_guard', pathlib.Path(__file__).with_name('OcrSourceIntake.py'))
guard = importlib.util.module_from_spec(spec)
spec.loader.exec_module(guard)
root = guard.ROOT.parent / ('ocr-intake-guard-tests-' + uuid.uuid4().hex)
root.mkdir()
checks = []
for path in ('/escape', '../escape', 'safe/../escape', 'C:/escape', 'safe\\escape',
             'safe/CON.txt', 'safe/name.', 'safe/name ', 'safe//escape', 'safe/./escape'):
    try:
        guard.safe_name(path)
    except ValueError:
        checks.append('deny ' + path)
    else:
        raise AssertionError('Unsafe path accepted')

for kind in ('valid', 'symlink', 'hardlink', 'traversal', 'collision', 'oversize'):
    archive = root / (kind + '.tar.gz')
    with tarfile.open(archive, 'w:gz') as out:
        good = tarfile.TarInfo('fixture/good.txt')
        good.size = 4
        out.addfile(good, io.BytesIO(b'test'))
        if kind != 'valid':
            entry = tarfile.TarInfo('fixture/bad.txt')
            if kind in ('symlink', 'hardlink'):
                entry.type = tarfile.SYMTYPE if kind == 'symlink' else tarfile.LNKTYPE
                entry.linkname = '../outside'
            elif kind == 'traversal':
                entry.name = 'fixture/../../outside'
            elif kind == 'collision':
                entry.name = 'fixture/GOOD.txt'
            elif kind == 'oversize':
                entry.size = 5
            out.addfile(entry, io.BytesIO(b'12345') if kind == 'oversize' else None)
    destination = root / kind
    original_limit = guard.MAX_FILE
    if kind == 'oversize':
        guard.MAX_FILE = 4
    try:
        records, _, _ = guard.extract(archive, destination, 'fixture')
        assert kind == 'valid' and len(records) == 1
        checks.append('valid archive hash-bound')
    except ValueError:
        assert kind != 'valid'
        assert not list(destination.rglob('*')), 'Payload written before validation completed'
        checks.append('deny ' + kind + ' before payload write')
    finally:
        guard.MAX_FILE = original_limit

result = {'passed': len(checks), 'checks': checks, 'engineExecutions': 0}
(root / 'results.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print('STATIC_GUARD_TESTS_PASS=' + str(len(checks)) + '; ENGINE_EXECUTIONS=0')
print(str(root / 'results.json'))
