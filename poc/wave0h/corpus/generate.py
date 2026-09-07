"""Agent 0 only: create fixed benign PDFs in a NEW directory; stdlib only."""
import hashlib
import json
from pathlib import Path
import sys


def pdf(markers):
    objects = [b'<< /Type /Catalog /Pages 2 0 R >>', b'',
               b'<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>']
    kids = []
    for marker in markers:
        page_id = len(objects) + 1
        kids.append(f'{page_id} 0 R')
        content = f'BT /F1 24 Tf 72 720 Td ({marker}) Tj ET\n'.encode('ascii')
        objects.append((f'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] '
                        f'/Resources << /Font << /F1 3 0 R >> >> '
                        f'/Contents {page_id + 1} 0 R >>').encode('ascii'))
        objects.append(f'<< /Length {len(content)} >>\nstream\n'.encode() + content + b'endstream')
    objects[1] = f'<< /Type /Pages /Count {len(kids)} /Kids [{" ".join(kids)}] >>'.encode()
    result = bytearray(b'%PDF-1.4\n%\xe2\xe3\xcf\xd3\n')
    offsets = [0]
    for number, obj in enumerate(objects, 1):
        offsets.append(len(result))
        result.extend(f'{number} 0 obj\n'.encode() + obj + b'\nendobj\n')
    xref = len(result)
    result.extend(f'xref\n0 {len(offsets)}\n0000000000 65535 f \n'.encode())
    for offset in offsets[1:]:
        result.extend(f'{offset:010d} 00000 n \n'.encode())
    result.extend(f'trailer\n<< /Size {len(offsets)} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n'.encode())
    return bytes(result)


def main():
    if len(sys.argv) != 2:
        raise SystemExit('Usage: python generate.py NEW_DIRECTORY')
    destination = Path(sys.argv[1]).absolute()
    if any(p.is_symlink() or p.is_junction() for p in [destination, *destination.parents]):
        raise SystemExit('Refusing link/junction path')
    # Fixed values: no external document or unbounded spec is consumed.
    sources = {'A': ['W0H-A-P1', 'W0H-A-P2'],
               'B': ['W0H-B-P1', 'W0H-B-P2', 'W0H-B-P3'], 'C': ['W0H-C-P1']}
    destination.mkdir(exist_ok=False)  # Parent must exist; never overwrite a run.
    manifest = {}
    for name, markers in sources.items():
        data = pdf(markers)
        with (destination / f'{name}.pdf').open('xb') as handle:
            handle.write(data)
        manifest[name] = {'sha256': hashlib.sha256(data).hexdigest(),
                          'bytes': len(data), 'markers': markers}
    with (destination / 'sources.json').open('x', encoding='utf-8') as handle:
        json.dump(manifest, handle, indent=2)
    print('Created A/B/C and sources.json; independent validation still required.')


if __name__ == '__main__':
    main()
