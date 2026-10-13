"""Check package legal mapping against exact manifest; no legal approval claim."""
import argparse
import json
import hashlib
import zipfile
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('zip', type=Path)
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
with zipfile.ZipFile(args.zip) as archive:
    prefix = 'PERTAMINA-OTO/'
    manifest = json.loads(archive.read(prefix + 'package-manifest.json'))
    files = {f['Path']: f for f in manifest['Files']}
    mapping = manifest['LicenseMapping']
    failures = []
    for item in mapping:
        digest = hashlib.sha256(archive.read(prefix + item['Path'])).hexdigest().upper()
        if digest != item['Sha256'] or digest != files[item['Path']]['Sha256']:
            failures.append(item['Path'])
    report = {'zipSha256': hashlib.sha256(args.zip.read_bytes()).hexdigest().upper(),
              'mappedLegalFiles': len(mapping),
              'allLegalFiles': [p for p in files if p.startswith('lisensi/')],
              'components': sorted({m['Component'] for m in mapping}), 'failures': failures,
              'runtimeBundled': manifest['RuntimeBundled'],
              'sourceInventoryHashMatches': hashlib.sha256(archive.read(prefix + 'source-inventory.json')).hexdigest().upper() == manifest['SourceInventorySha256'],
              'verdict': 'MAPPING_INTEGRITY_ONLY; distribution obligations/disposition unresolved'}
args.output.write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report))
raise SystemExit(bool(failures) or not report['sourceInventoryHashMatches'])
