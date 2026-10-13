"""Metadata-only intake. Never restores packages or executes vendor payloads."""
import gzip
import hashlib
import json
import pathlib
import urllib.request
from datetime import datetime, timezone
from urllib.parse import urlsplit

ROOT = pathlib.Path(__file__).resolve().parents[1] / 'artifacts' / 'rapidocr-intake-20261001'
HOSTS = {'api.github.com', 'raw.githubusercontent.com', 'api.nuget.org'}
PINS = [('rapidocrnet', '4.2.0'), ('clipper2', '2.0.0'),
        ('microsoft.ml.onnxruntime', '1.30.0'), ('microsoft.ml.onnxruntime.managed', '1.30.0'),
        ('skiasharp', '3.119.1'), ('skiasharp.nativeassets.linux.nodependencies', '3.119.1'),
        ('skiasharp.nativeassets.win32', '3.119.1'), ('skiasharp.nativeassets.macos', '3.119.1')]
COMMIT = '7b5c432e4262470b6d81bf1710c3bd52fc438566'


def validate(url):
    p = urlsplit(url)
    if p.scheme != 'https' or p.hostname not in HOSTS or p.username or p.password:
        raise ValueError('Unexpected metadata source')


class Guard(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        validate(newurl)
        return super().redirect_request(req, fp, code, msg, headers, newurl)


def fetch(url):
    validate(url)
    req = urllib.request.Request(url, headers={'User-Agent': 'OTO-static-OCR-intake', 'Accept-Encoding': 'identity'})
    with urllib.request.build_opener(Guard()).open(req, timeout=40) as response:
        validate(response.url)
        data = response.read(8 * 1024 * 1024 + 1)
    if len(data) > 8 * 1024 * 1024:
        raise ValueError('Metadata ceiling')
    if data.startswith(b'\x1f\x8b'):
        data = gzip.decompress(data)
    if len(data) > 16 * 1024 * 1024:
        raise ValueError('Expanded metadata ceiling')
    return data


def save(name, url):
    data = fetch(url)
    with (ROOT / name).open('xb') as out:
        out.write(data)
    records.append({'file': name, 'url': url, 'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest()})
    return data


records = []


def main():
    ROOT.mkdir(parents=True, exist_ok=False)
    summary = {'utc': datetime.now(timezone.utc).isoformat(), 'mode': 'metadata-only',
               'engineExecutions': 0, 'packages': [], 'receipts': records, 'errors': []}
    for package, version in PINS:
        try:
            reg = json.loads(save(package + '.registration.json',
                f'https://api.nuget.org/v3/registration5-semver1/{package}/{version}.json'))
            catalog = json.loads(save(package + '.catalog.json', reg['catalogEntry']))
            summary['packages'].append({'id': package, 'version': version,
                'licenseExpression': catalog.get('licenseExpression'), 'licenseFile': catalog.get('licenseFile'),
                'licenseUrl': catalog.get('licenseUrl'), 'packageContent': reg['packageContent'],
                'packageHash': catalog.get('packageHash'), 'packageHashAlgorithm': catalog.get('packageHashAlgorithm'),
                'packageSize': catalog.get('packageSize'), 'dependencyGroups': catalog.get('dependencyGroups'),
                'vulnerabilities': catalog.get('vulnerabilities'), 'deprecation': catalog.get('deprecation')})
        except Exception as e:
            summary['errors'].append({'id': package, 'error': str(e)[:300]})
    for name, url in [
        ('rapidocrnet-tag.json', 'https://api.github.com/repos/BobLd/RapidOcrNet/git/ref/tags/4.2.0'),
        ('rapidocrnet-commit.json', f'https://api.github.com/repos/BobLd/RapidOcrNet/commits/{COMMIT}'),
        ('rapidocrnet-tree.json', f'https://api.github.com/repos/BobLd/RapidOcrNet/git/trees/{COMMIT}?recursive=1'),
        ('RapidOcrNet-LICENSE.txt', f'https://raw.githubusercontent.com/BobLd/RapidOcrNet/{COMMIT}/LICENSE.txt'),
        ('RapidOcrNet-NOTICE.txt', f'https://raw.githubusercontent.com/BobLd/RapidOcrNet/{COMMIT}/NOTICE.txt'),
        ('RapidOcrNet.csproj.txt', f'https://raw.githubusercontent.com/BobLd/RapidOcrNet/{COMMIT}/RapidOcrNet/RapidOcrNet.csproj'),
        ('RapidOcrNet.targets.txt', f'https://raw.githubusercontent.com/BobLd/RapidOcrNet/{COMMIT}/RapidOcrNet/RapidOcrNet.targets'),
        ('rapidocr-main-metadata.json', 'https://api.github.com/repos/RapidAI/RapidOCR/commits/main')]:
        try:
            save(name, url)
        except Exception as e:
            summary['errors'].append({'file': name, 'error': str(e)[:300]})
    with (ROOT / 'metadata-manifest.json').open('x', encoding='utf-8') as out:
        json.dump(summary, out, indent=2)
    for p in summary['packages']:
        print(p['id'], p['version'], p['licenseExpression'] or p['licenseFile'], p['packageSize'],
              'vulnerabilities=' + str(p['vulnerabilities']))
        print('dependencies=' + json.dumps(p['dependencyGroups']))
    print('errors=' + json.dumps(summary['errors']))


if __name__ == '__main__':
    main()
