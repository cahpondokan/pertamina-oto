"""Revalidate complete artifacts and produce file/SBOM inventory, no vendor execution."""
import base64
import hashlib
import json
import pathlib
import xml.etree.ElementTree as ET
from RapidOcrMetadataIntake import ROOT
from RapidOcrPackageIntake import extract_zip

metadata = json.loads((ROOT / 'metadata-manifest.json').read_text())['packages']
metadata += json.loads((ROOT / 'followup-manifest.json').read_text())['packages']
result = {'engineExecutions': 0, 'packages': [], 'held': [], 'limitations': [
    'No runtime, model accuracy, egress or product admission proof.',
    'Dependency graph is metadata-derived for net10 CPU Windows; no restore resolver executed.',
    'Embedded native revisions and exact license reachability require further mapping.']}
short_root = ROOT / 'x'
short_root.mkdir(exist_ok=False)
for index, p in enumerate(metadata):
    if p['id'] == 'rapidocrnet':
        result['held'].append({'id': p['id'], 'version': p['version'], 'reason': 'Exact v5 converted model license/provenance not closed; package contains models.'})
        continue
    paths = sorted((ROOT / 'packages').glob(p['id'] + '.' + p['version'] + '*.nupkg'))
    complete = None
    for archive in paths:
        if archive.stat().st_size != p['packageSize']:
            continue
        sha512 = hashlib.sha512()
        with archive.open('rb') as inp:
            while data := inp.read(1024 * 1024):
                sha512.update(data)
        if base64.b64encode(sha512.digest()).decode() == p['packageHash']:
            complete = archive
            break
    if complete is None:
        result['held'].append({'id': p['id'], 'version': p['version'], 'reason': 'No complete package matching catalog size/SHA512; partials quarantined.'})
        continue
    files, expanded = extract_zip(complete, short_root / str(index))
    extracted = short_root / str(index)
    nuspecs = list(extracted.glob('*.nuspec'))
    if len(nuspecs) != 1:
        raise ValueError('Expected one nuspec')
    # NuGet XML is parsed as data, never evaluated as MSBuild.
    tree = ET.parse(nuspecs[0])
    ns = {'n': tree.getroot().tag.split('}')[0].lstrip('{')}
    node = tree.find('n:metadata', ns)
    found_id = node.findtext('n:id', namespaces=ns)
    version = node.findtext('n:version', namespaces=ns)
    if found_id.lower() != p['id'] or version != p['version']:
        raise ValueError('Nuspec identity mismatch')
    repo = node.find('n:repository', ns)
    license_node = node.find('n:license', ns)
    result['packages'].append({'id': p['id'], 'version': version, 'archive': str(complete.relative_to(ROOT)),
        'bytes': complete.stat().st_size, 'sha256': hashlib.sha256(complete.read_bytes()).hexdigest(),
        'sha512CatalogMatch': True, 'expandedBytes': expanded, 'files': files,
        'repository': repo.attrib if repo is not None else None,
        'nuspecLicense': {'type': license_node.get('type'), 'value': license_node.text} if license_node is not None else None,
        'noticeFiles': [f for f in files if any(s in f['path'].lower() for s in ('license', 'notice', 'copyright'))],
        'nativeFiles': [f for f in files if f['path'].lower().endswith(('.dll', '.so', '.dylib')) and 'runtimes/' in f['path']],
        'buildFiles': [f for f in files if f['path'].lower().endswith(('.targets', '.props', '.ps1', '.sh', '.bat', '.cmd'))],
        'signaturePresent': any(f['path'] == '.signature.p7s' for f in files),
        'extractionPath': 'x/' + str(index)})
    print(p['id'], 'HASH/CRC/PATH/IDENTITY PASS; files=' + str(len(files)), flush=True)
(ROOT / 'static-inventory.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print('complete=' + str(len(result['packages'])) + '; held=' + str(len(result['held'])))
