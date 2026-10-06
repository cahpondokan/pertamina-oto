"""Read-only portable ZIP structural verifier; standard library only."""
import argparse
import hashlib
import json
import re
import zipfile
from pathlib import Path, PurePosixPath

def inspect(path):
    failures, files, seen = [], [], set()
    manifest_report = None
    with zipfile.ZipFile(path) as archive:
        for entry in archive.infolist():
            name = entry.filename.replace('\\', '/')
            parts = PurePosixPath(name).parts
            key = name.rstrip('/').casefold()
            if (name.startswith('/') or '..' in parts or ':' in name
                or any(p.rstrip(' .') != p for p in parts)
                or re.search(r'(^|/)(con|prn|aux|nul|com[1-9]|lpt[1-9])(\.|/|$)', name, re.I)):
                failures.append('unsafe entry: ' + name)
            if key in seen:
                failures.append('duplicate/case collision: ' + name)
            seen.add(key)
            if (entry.external_attr >> 16) & 0o170000 == 0o120000:
                failures.append('symlink entry: ' + name)
            if entry.flag_bits & 1:
                failures.append('encrypted entry: ' + name)
            if entry.is_dir():
                continue
            digest, size = hashlib.sha256(), 0
            with archive.open(entry) as stream:
                while chunk := stream.read(1024 * 1024):
                    digest.update(chunk)
                    size += len(chunk)
            if size != entry.file_size:
                failures.append('size mismatch: ' + name)
            files.append(dict(path=name, size=size, sha256=digest.hexdigest().upper()))
        manifests = [f for f in files if f['path'].endswith('/package-manifest.json')]
        if len(manifests) == 1:
            manifest_file = manifests[0]
            prefix = manifest_file['path'][:-len('package-manifest.json')]
            manifest = json.loads(archive.read(manifest_file['path']))
            actual = {f['path'][len(prefix):]: f for f in files if f['path'] != manifest_file['path']}
            expected = {}
            for entry in manifest['Files']:
                relative = entry['Path'].replace('\\', '/')
                if relative.casefold() in {p.casefold() for p in expected}:
                    failures.append('manifest duplicate: ' + relative)
                expected[relative] = entry
            if set(actual) != set(expected):
                failures.append('manifest path inventory mismatch')
            for relative in set(actual) & set(expected):
                if actual[relative]['size'] != expected[relative]['Length'] or actual[relative]['sha256'] != expected[relative]['Sha256']:
                    failures.append('manifest size/hash mismatch: ' + relative)
            for required in ['PERTAMINA-OTO.exe', 'app/Pertamina.Oto.Desktop.App.exe',
                             'app/portable-profile.json', 'MULAI-DI-SINI.txt', 'source-inventory.json']:
                if required not in expected:
                    failures.append('missing required manifest file: ' + required)
            manifest_report = dict(sha256=manifest_file['sha256'], buildId=manifest.get('BuildId'),
                                   runtimeBundled=manifest.get('RuntimeBundled'), releaseGate=manifest.get('ReleaseGate'),
                                   declaredFiles=len(expected))
        elif manifests:
            failures.append('multiple package manifests')
    return dict(zip=str(path), zipSha256=hashlib.sha256(path.read_bytes()).hexdigest().upper(),
                zipBytes=path.stat().st_size, fileCount=len(files), files=files,
                manifest=manifest_report,
                failures=failures, status='FAIL' if failures else 'PASS_STATIC_STRUCTURE_ONLY')

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('zip', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    report = inspect(args.zip)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(json.dumps({k: v for k, v in report.items() if k != 'files'}))
    raise SystemExit(bool(report['failures']))
