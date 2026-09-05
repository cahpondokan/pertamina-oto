"""Deterministic benign PDF fixtures. Standard library only; no external input."""
import hashlib
import json

SEED = 'P02-20260905-1434/CORP-v1'

def canonical(value):
    return (json.dumps(value, sort_keys=True, separators=(',', ':'), ensure_ascii=True) + '\n').encode('ascii')

def digest(data):
    return hashlib.sha256(data).hexdigest()

def marker(item, page):
    return 'SYNTHETIC-' + digest(f'{SEED}/{item}/{page}'.encode('ascii'))[:24]

def pdf(item, count):
    if (item, count) not in (('A', 2), ('B', 3), ('C', 1)):
        raise ValueError('UNAPPROVED_RECIPE')
    kids = ' '.join(f'{4+2*i} 0 R' for i in range(count))
    objects = [b'<< /Type /Catalog /Pages 2 0 R >>',
               f'<< /Type /Pages /Count {count} /Kids [{kids}] >>'.encode('ascii'),
               b'<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>']
    for i in range(count):
        objects.append((f'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] '
                        f'/Resources << /Font << /F1 3 0 R >> >> /Contents {5+2*i} 0 R >>').encode('ascii'))
        stream = f'BT /F1 12 Tf 72 720 Td ({marker(item, i+1)}) Tj ET\n'.encode('ascii')
        objects.append(f'<< /Length {len(stream)} >>\nstream\n'.encode('ascii') + stream + b'endstream')
    result = bytearray(b'%PDF-1.4\n')
    offsets = [0]
    for index, obj in enumerate(objects, 1):
        offsets.append(len(result))
        result.extend(f'{index} 0 obj\n'.encode('ascii') + obj + b'\nendobj\n')
    xref = len(result)
    result.extend(f'xref\n0 {len(offsets)}\n0000000000 65535 f \n'.encode('ascii'))
    for offset in offsets[1:]:
        result.extend(f'{offset:010d} 00000 n \n'.encode('ascii'))
    result.extend((f'trailer\n<< /Size {len(offsets)} /Root 1 0 R >>\n'
                   f'startxref\n{xref}\n%%EOF\n').encode('ascii'))
    return bytes(result)

def bundle():
    payloads = {k: pdf(k, n) for k, n in (('A', 2), ('B', 3), ('C', 1))}
    items = [{'id': k, 'pageCount': n, 'byteCount': len(payloads[k]), 'sha256': digest(payloads[k]),
              'markers': [marker(k, p) for p in range(1, n+1)]} for k, n in (('A', 2), ('B', 3), ('C', 1))]
    recipes = [
        {'id': 'CONTROL_AB', 'inputs': ['A', 'B'], 'tests': ['UT-MERGE-003', 'TP-MRG-001']},
        {'id': 'ORDER_CAB', 'inputs': ['C', 'A', 'B'], 'tests': ['UT-MERGE-006', 'TP-MRG-006']},
        {'id': 'DUPLICATE_AA', 'inputs': ['A', 'A'], 'tests': ['UT-MERGE-009', 'TP-MRG-008']},
        {'id': 'REMOVE_B', 'inputs': ['A', 'C'], 'tests': ['UT-MERGE-007', 'TP-MRG-007']}]
    manifest = {'schemaVersion': '1.0', 'feature': 'FEAT-PDF-005', 'seed': SEED,
        'classification': 'SYNTHETIC_NON_SENSITIVE', 'authority': 'P2-W0-CORPUS-001',
        'cost': 'ZERO_ADDITIONAL_COST', 'processingAuthorization': 'NOT_AUTHORIZED',
        'provenance': {'generator': 'poc/corpus/generate.py', 'recipeVersion': '1.0',
                       'externalAssets': [], 'dependencies': 'PYTHON_STDLIB_ONLY'},
        'retention': {'status': 'PREPARATION_ONLY_PENDING_RUN_POLICY', 'expiry': None,
                      'disposal': 'NO_AUTOMATIC_DELETE', 'owner': 'Workspace User'},
        'items': items, 'recipes': recipes}
    by_id = {i['id']: i for i in items}
    expected = {'schemaVersion': '1.0', 'manifestSha256': digest(canonical(manifest)), 'recipes': []}
    for recipe in recipes:
        pages = [{'occurrence': occurrence, 'source': k, 'sourcePage': p, 'marker': by_id[k]['markers'][p-1]}
                 for occurrence, k in enumerate(recipe['inputs'], 1) for p in range(1, by_id[k]['pageCount']+1)]
        expected['recipes'].append({'id': recipe['id'], 'pageCount': len(pages), 'pages': pages})
    return manifest, expected, payloads

if __name__ == '__main__':
    m, e, data = bundle()
    print(json.dumps({'manifestSha256': digest(canonical(m)), 'expectedSha256': digest(canonical(e)),
                      'items': [{'id': k, 'sha256': digest(v), 'bytes': len(v)} for k, v in data.items()]}, sort_keys=True))
