"""Acquisition/static extraction only. Never imports or runs acquired source."""
import hashlib
import json
import pathlib
import re
import tarfile
import urllib.request
from datetime import datetime, timezone

ROOT = pathlib.Path(__file__).resolve().parents[1] / 'artifacts' / 'ocr-source-intake-20261001'
CANDIDATES = [('tesseract-ocr/tesseract', 'db0ec62f81b0737fbbe184d8fea40af5738f8eef'),
              ('DanBloomberg/leptonica', '13275a278eb55b5746e33f95fbf5a2c8f604b3ab')]
MAX_DOWNLOAD = 64 * 1024 * 1024
MAX_TOTAL = 512 * 1024 * 1024
MAX_FILE = 32 * 1024 * 1024
MAX_ENTRIES = 10000
ALLOWED_HOSTS = {'github.com', 'codeload.github.com', 'api.github.com'}


class RedirectGuard(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        validate_url(newurl)
        return super().redirect_request(req, fp, code, msg, headers, newurl)


def validate_url(url):
    from urllib.parse import urlsplit
    parsed = urlsplit(url)
    if parsed.scheme != 'https' or parsed.hostname not in ALLOWED_HOSTS or parsed.username:
        raise ValueError('Unexpected source URL')


def acquire(url, destination):
    validate_url(url)
    opener = urllib.request.build_opener(RedirectGuard())
    request = urllib.request.Request(url, headers={'User-Agent': 'OTO-static-source-intake'})
    total = 0
    with opener.open(request, timeout=45) as response, destination.open('xb') as output:
        validate_url(response.url)
        while chunk := response.read(1024 * 1024):
            total += len(chunk)
            if total > MAX_DOWNLOAD:
                raise ValueError('Download ceiling exceeded')
            output.write(chunk)
    return {'url': url, 'bytes': total, 'sha256': hashlib.sha256(destination.read_bytes()).hexdigest()}


def safe_name(name):
    path = pathlib.PurePosixPath(name)
    if '\\' in name or name.startswith('/') or ':' in name or not path.parts:
        raise ValueError('Unsafe archive path')
    for part in path.parts:
        if part in ('.', '..') or part.endswith((' ', '.')) or re.search(r'[\x00-\x1f<>|?*]', part):
            raise ValueError('Unsafe archive segment')
        if re.fullmatch(r'(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?', part, re.I):
            raise ValueError('Reserved archive segment')
    # PurePosixPath normalizes . and //, so reject these before that normalization too.
    if any(p in ('.', '..', '') for p in name.rstrip('/').split('/')):
        raise ValueError('Noncanonical archive path')
    return path


def extract(archive, destination, expected_prefix):
    destination.mkdir()
    records = []
    with tarfile.open(archive, mode='r:gz') as source:
        members = []
        names = set()
        total = 0
        for member in source:
            if len(members) >= MAX_ENTRIES:
                raise ValueError('Archive entry ceiling exceeded')
            name = safe_name(member.name)
            if name.parts[0] != expected_prefix:
                raise ValueError('Wrong immutable archive prefix')
            key = str(name).casefold()
            if key in names:
                raise ValueError('Duplicate or case-colliding archive path')
            names.add(key)
            if not (member.isdir() or member.isfile()) or member.sparse is not None:
                raise ValueError('Link/special/sparse archive member denied')
            if member.size < 0 or member.size > MAX_FILE:
                raise ValueError('Archive file ceiling exceeded')
            total += member.size
            if total > MAX_TOTAL:
                raise ValueError('Expanded size ceiling exceeded')
            members.append(member)
        # Every entry has been validated before any payload extraction.
        for member in members:
            target = destination.joinpath(*safe_name(member.name).parts)
            if not target.resolve().is_relative_to(destination.resolve()):
                raise ValueError('Archive path escape')
            if member.isdir():
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            stream = source.extractfile(member)
            if stream is None:
                raise ValueError('Missing archive payload')
            data = stream.read(MAX_FILE + 1)
            if len(data) != member.size:
                raise ValueError('Archive payload size mismatch')
            with target.open('xb') as output:
                output.write(data)
            records.append({'path': str(pathlib.PurePosixPath(member.name).relative_to(expected_prefix)),
                            'bytes': len(data), 'sha256': hashlib.sha256(data).hexdigest(),
                            'gitBlob': hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()})
    return records, len(members), total


def main():
    ROOT.mkdir(parents=True, exist_ok=False)
    manifest = {'task': 'OCR-SOURCE-INTAKE-20261001', 'executionAuthorized': False,
                'timestampUtc': datetime.now(timezone.utc).isoformat(), 'candidates': []}
    for repo, commit in CANDIDATES:
        project = repo.split('/')[1]
        archive = ROOT / (project + '.tar.gz')
        receipt = acquire('https://github.com/' + repo + '/archive/' + commit + '.tar.gz', archive)
        metadata = ROOT / (project + '-tree.json')
        acquire('https://api.github.com/repos/' + repo + '/git/trees/' + commit + '?recursive=1', metadata)
        tree = json.loads(metadata.read_text(encoding='utf-8'))
        if tree.get('truncated') or not isinstance(tree.get('tree'), list):
            raise ValueError('Incomplete official source tree')
        records, count, total = extract(archive, ROOT / (project + '-extracted'), project + '-' + commit)
        remote = {entry['path']: entry['sha'] for entry in tree['tree'] if entry['type'] == 'blob'}
        mismatches = [record['path'] for record in records if remote.get(record['path']) != record['gitBlob']]
        missing = sorted(set(remote) - {r['path'] for r in records})
        record = {'repository': repo, 'commit': commit, 'archive': receipt, 'entries': count,
                  'expandedBytes': total, 'files': records, 'treeBlobMismatches': mismatches,
                  'treeFilesOmittedFromArchive': missing, 'officialTreeSha': tree['sha']}
        manifest['candidates'].append(record)
        (ROOT / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n', encoding='utf-8')
        print(project + ': files=' + str(len(records)) + ', blob mismatches=' + str(len(mismatches))
              + ', omitted=' + str(len(missing)), flush=True)
        if mismatches:
            raise ValueError('Source provenance mismatch')
    print('ACQUISITION_AND_STATIC_EXTRACTION_COMPLETE; ENGINE_EXECUTIONS=0', flush=True)


if __name__ == '__main__':
    main()
