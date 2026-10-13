"""Exact isolated package intake; ZIP parsing only, no restore/load/build."""
import base64
import hashlib
import json
import pathlib
import stat
import subprocess
import urllib.request
import zipfile
from RapidOcrMetadataIntake import ROOT, HOSTS, Guard, validate, fetch, COMMIT
from OcrSourceIntake import safe_name

MAX_DOWNLOAD = 256 * 1024 * 1024
MAX_FILE = 256 * 1024 * 1024
MAX_EXPANDED = 1024 * 1024 * 1024
MAX_ENTRIES = 10000
# Model-containing RapidOcrNet package is held: exact v5 model license/provenance unresolved.
PINS = {'clipper2': '2.0.0', 'microsoft.ml.onnxruntime': '1.30.0',
        'microsoft.ml.onnxruntime.managed': '1.30.0', 'skiasharp': '3.119.1',
        'skiasharp.nativeassets.linux.nodependencies': '3.119.1',
        'skiasharp.nativeassets.win32': '3.119.1', 'skiasharp.nativeassets.macos': '3.119.1',
        'system.memory': '4.5.5', 'system.numerics.tensors': '9.0.0'}


def acquire_package(meta, target):
    url = meta['packageContent']
    validate(url)
    expected = meta['packageSize']
    if not expected or expected > MAX_DOWNLOAD or meta['packageHashAlgorithm'] != 'SHA512':
        raise ValueError('Unreviewed package size/hash')
    with target.open('xb'):
        pass
    result = subprocess.run(['C:/Windows/System32/curl.exe', '--fail', '--silent', '--show-error',
        '--proto', '=https', '--max-redirs', '0', '--connect-timeout', '15', '--max-time', '90',
        '--max-filesize', str(expected), '--output', str(target), url],
        capture_output=True, timeout=100, check=False)
    if result.returncode:
        raise ValueError('Bounded download failed: ' + result.stderr.decode(errors='replace')[:200])
    total = target.stat().st_size
    digest = hashlib.sha512()
    with target.open('rb') as inp:
        while chunk := inp.read(1024 * 1024):
            digest.update(chunk)
    if total != expected or base64.b64encode(digest.digest()).decode() != meta['packageHash']:
        raise ValueError('Official catalog size/hash mismatch')
    return {'bytes': total, 'sha256': hashlib.sha256(target.read_bytes()).hexdigest(),
            'sha512CatalogMatch': True, 'url': url}


def extract_zip(archive, destination):
    # Prefix only this task-owned absolute destination for Windows MAX_PATH.
    if __import__('os').name == 'nt' and not str(destination).startswith('\\\\?\\'):
        destination = pathlib.Path('\\\\?\\' + str(destination.absolute()))
    if destination.exists():
        raise ValueError('Extraction target already exists')
    with zipfile.ZipFile(archive) as source:
        members = source.infolist()
        if len(members) > MAX_ENTRIES:
            raise ValueError('Entry ceiling')
        names, file_names, total = set(), set(), 0
        for m in members:
            safe_name(m.orig_filename)
            if m.orig_filename != m.filename:
                raise ValueError('Normalized archive name denied')
            p = safe_name(m.filename)
            key = str(p).casefold()
            if key in names:
                raise ValueError('Collision')
            names.add(key)
            mode = m.external_attr >> 16
            if m.flag_bits & 1 or (stat.S_IFMT(mode) not in (0, stat.S_IFREG, stat.S_IFDIR)):
                raise ValueError('Encrypted/link/special member')
            if m.file_size > MAX_FILE or m.file_size < 0:
                raise ValueError('File ceiling')
            if m.file_size > 1024 * 1024 and m.file_size > max(1, m.compress_size) * 1000:
                raise ValueError('Compression ceiling')
            total += m.file_size
            if total > MAX_EXPANDED:
                raise ValueError('Expanded ceiling')
            if not m.is_dir():
                file_names.add(key)
        for m in members:
            p = safe_name(m.filename)
            for parent in p.parents:
                if str(parent).casefold() in file_names:
                    raise ValueError('File/dir prefix collision')
        # Full validation before any payload write. ZIP CRC checked by bounded reads.
        destination.mkdir()
        records = []
        for m in members:
            target = destination.joinpath(*safe_name(m.filename).parts)
            if not target.resolve().is_relative_to(destination.resolve()):
                raise ValueError('Path escape')
            if m.is_dir():
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            digest, count = hashlib.sha256(), 0
            with source.open(m) as inp, target.open('xb') as out:
                while data := inp.read(1024 * 1024):
                    count += len(data)
                    if count > m.file_size or count > MAX_FILE:
                        raise ValueError('Payload ceiling')
                    digest.update(data)
                    out.write(data)
            if count != m.file_size:
                raise ValueError('Payload size mismatch')
            records.append({'path': m.filename, 'bytes': count, 'sha256': digest.hexdigest()})
        return records, total


def main():
    metadata = json.loads((ROOT / 'metadata-manifest.json').read_text())['packages']
    metadata += json.loads((ROOT / 'followup-manifest.json').read_text())['packages']
    by_id = {p['id']: p for p in metadata}
    out_root = ROOT / 'packages'
    out_root.mkdir(exist_ok=True)
    manifest = {'engineExecutions': 0, 'restoreBuildInstall': False,
                'held': ['rapidocrnet/4.2.0 and four v5 models/dictionary: license/provenance scope not closed'],
                'packages': [], 'sourceFiles': [], 'errors': []}
    if (ROOT / 'package-manifest.json').exists():
        manifest = json.loads((ROOT / 'package-manifest.json').read_text())
    def checkpoint():
        (ROOT / 'package-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
    for package, version in PINS.items():
        try:
            existing = next((p for p in manifest['packages'] if p['id'] == package), None)
            if existing:
                if not existing.get('originalNameGuardRevalidated'):
                    files, expanded = extract_zip(out_root / f'{package}.{version}.nupkg',
                        out_root / f'{package}.{version}-revalidated')
                    if files != existing['files']:
                        raise ValueError('Revalidation inventory mismatch')
                    existing['originalNameGuardRevalidated'] = True
                checkpoint()
                continue
            meta = by_id[package]
            if meta['version'] != version:
                raise ValueError('Pin mismatch')
            archive = out_root / f'{package}.{version}.nupkg'
            if archive.exists():
                archive = out_root / f'{package}.{version}.retry1.nupkg'
            receipt = acquire_package(meta, archive)
            files, expanded = extract_zip(archive, out_root / f'{package}.{version}-extracted')
            manifest['packages'].append({'id': package, 'version': version, 'archive': receipt,
                                         'expandedBytes': expanded, 'files': files,
                                         'originalNameGuardRevalidated': True})
            print(package, version, 'catalog SHA512 MATCH; files=' + str(len(files)), flush=True)
        except Exception as e:
            manifest['errors'].append({'id': package, 'error': str(e)[:300]})
            print(package, 'HOLD', str(e)[:200], flush=True)
        checkpoint()
    # Source-only subset, no models, tests, binaries or signing keys acquired.
    tree = json.loads((ROOT / 'rapidocrnet-tree.json').read_text())
    if tree.get('truncated'):
        raise ValueError('Truncated source tree')
    source_root = ROOT / 'rapidocrnet-source-subset'
    source_root.mkdir(exist_ok=False)
    for entry in tree['tree']:
        name = entry['path']
        allowed = name in ('LICENSE.txt', 'NOTICE.txt', 'README.md') or (
            name.startswith('RapidOcrNet/') and pathlib.PurePosixPath(name).suffix in ('.cs', '.csproj', '.targets'))
        if entry['type'] != 'blob' or not allowed:
            continue
        safe_name(name)
        data = fetch(f'https://raw.githubusercontent.com/BobLd/RapidOcrNet/{COMMIT}/{name}')
        blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        if blob != entry['sha']:
            raise ValueError('Source tree blob mismatch')
        target = source_root.joinpath(*pathlib.PurePosixPath(name).parts)
        target.parent.mkdir(parents=True, exist_ok=True)
        with target.open('xb') as out:
            out.write(data)
        manifest['sourceFiles'].append({'path': name, 'sha256': hashlib.sha256(data).hexdigest(),
                                        'gitBlob': blob, 'bytes': len(data)})
    checkpoint()
    print('source subset files=' + str(len(manifest['sourceFiles'])) + '; engine executions=0', flush=True)


if __name__ == '__main__':
    main()
