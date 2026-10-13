"""Synthetic ZIP boundary checks only; does not execute OCR/model/package code."""
import json
import pathlib
import stat
import uuid
import zipfile
import RapidOcrPackageIntake as intake

root = intake.ROOT / ('guard-tests-' + uuid.uuid4().hex)
root.mkdir()
results = []


def check(name, members, reject=True):
    archive = root / (name + '.zip')
    destination = root / (name + '-extracted')
    with zipfile.ZipFile(archive, 'w') as out:
        for path, data, mode in members:
            info = zipfile.ZipInfo(path)
            info.create_system = 3
            info.external_attr = mode << 16
            out.writestr(info, data)
    if name == 'backslash':
        # ZipInfo on Windows normalizes this separator; patch both stored names
        # to create the malicious input the extractor actually must reject.
        archive.write_bytes(archive.read_bytes().replace(b'a/b', b'a\\b'))
    error = None
    try:
        intake.extract_zip(archive, destination)
    except ValueError as e:
        error = str(e)
    passed = bool(error) == reject
    if reject:
        passed = passed and not destination.exists()
    results.append({'name': name, 'pass': passed, 'error': error, 'zeroPayloadBeforeValidation': not destination.exists() if reject else None})
    if not passed:
        raise AssertionError(name)


regular = stat.S_IFREG | 0o644
for name, path in [('traversal', '../outside'), ('absolute', '/outside'), ('drive', 'C:/outside'),
                   ('backslash', 'a\\b'), ('ads', 'a:b'), ('reserved', 'CON.txt'),
                   ('dotsegment', 'a/./b'), ('trailingdot', 'a.'), ('emptysegment', 'a//b')]:
    check(name, [('valid.txt', b'ok', regular), (path, b'bad', regular)])
check('collision', [('A.txt', b'a', regular), ('a.txt', b'b', regular)])
check('prefix', [('a', b'a', regular), ('a/b', b'b', regular)])
check('symlink', [('valid.txt', b'ok', regular), ('link', b'../target', stat.S_IFLNK | 0o777)])
check('special', [('fifo', b'', stat.S_IFIFO | 0o644)])
old = intake.MAX_FILE
intake.MAX_FILE = 2
check('size', [('valid.txt', b'ok', regular), ('large', b'123', regular)])
intake.MAX_FILE = old
old = intake.MAX_ENTRIES
intake.MAX_ENTRIES = 1
check('entries', [('a', b'a', regular), ('b', b'b', regular)])
intake.MAX_ENTRIES = old
check('valid', [('folder/file.txt', b'ok', regular)], reject=False)
(root / 'results.json').write_text(json.dumps(results, indent=2), encoding='utf-8')
print(str(len(results)) + ' guard checks PASS; ' + str(root / 'results.json'))
