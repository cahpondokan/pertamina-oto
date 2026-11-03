#!/usr/bin/env python3
"""Pinned web-only switch. Never writes desktop catalog, runtime, nginx or unit."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import stat
import subprocess
import time
import urllib.request
import urllib.error

BASE = Path('/opt/oto-downloads')
DATA = Path('/var/lib/oto-downloads/releases')
UNIT = Path('/etc/systemd/system/oto-downloads.service')
NGINX = Path('/etc/nginx/sites-available/oto.ayani.id')
SERVICE = 'oto-downloads.service'


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def digest(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest() if hasattr(hashlib, 'file_digest') else stream_hash(stream)


def stream_hash(stream):
    value = hashlib.sha256()
    for block in iter(lambda: stream.read(1024 * 1024), b''):
        value.update(block)
    return value.hexdigest()


def regular(path):
    path = Path(path).absolute()
    require(all(not item.is_symlink() for item in (path, *path.parents)), 'Unexpected symlink.')
    require(path.is_file() and stat.S_ISREG(path.stat().st_mode), 'Regular file required.')


def inventory(root, files):
    require(root.is_dir() and not root.is_symlink(), 'Web directory required.')
    expected = set()
    require(1 <= len(files) <= 2000, 'Invalid inventory size.')
    for entry in files:
        name = entry['path']
        require(isinstance(name, str) and not name.startswith('/') and '\\' not in name and ':' not in name
                and all(part not in ('', '.', '..') for part in name.split('/')) and name not in expected, 'Unsafe inventory path.')
        require(re.fullmatch('[a-f0-9]{64}', entry['sha256']) and 0 <= entry['size'] <= 256 * 1024 ** 2, 'Invalid file metadata.')
        expected.add(name)
        file = root / name
        regular(file)
        require(file.stat().st_size == entry['size'] and digest(file) == entry['sha256'], 'Web payload hash/size mismatch: ' + name)
    actual = set()
    for file in root.rglob('*'):
        require(not file.is_symlink(), 'Payload symlink rejected.')
        if file.is_file():
            actual.add(file.relative_to(root).as_posix())
        else:
            require(file.is_dir(), 'Special payload file rejected.')
    require(actual == expected, 'Unlisted or missing web payload.')
    require({'Pertamina.Oto.Downloads.dll', 'wwwroot/web-release.json', 'wwwroot/js/pdf-worker.js'} <= expected, 'Required web payload absent.')


def load(root, pin):
    regular(root / 'publication.json')
    require(digest(root / 'publication.json') == pin, 'Publication pin mismatch.')
    config = json.loads((root / 'publication.json').read_text())
    require(re.fullmatch(r'oto-web-\d{8}-[a-f0-9]{12}', config['id']), 'Invalid web release id.')
    require(re.fullmatch(r'/opt/oto-downloads/oto-(update|web)-[a-z0-9-]+/web', config['previousWeb']), 'Invalid predecessor path.')
    require(set(config['catalogPins']) == {'current.json', 'distribution-v2.json'}, 'Catalog pins required.')
    for value in [config['unitSha256'], config['nginxSha256'], *config['catalogPins'].values()]:
        require(re.fullmatch('[a-f0-9]{64}', value), 'Invalid baseline hash.')
    inventory(root / 'web', config['files'])
    require(json.loads((root / 'web/wwwroot/web-release.json').read_text())['id'] == config['id'], 'Release marker mismatch.')
    return config


def unchanged(config):
    regular(UNIT)
    regular(NGINX)
    require(digest(UNIT) == config['unitSha256'] and digest(NGINX) == config['nginxSha256'], 'Hosting configuration drift; stopped.')
    for name, expected in config['catalogPins'].items():
        regular(DATA / name)
        require(digest(DATA / name) == expected, 'Desktop catalog drift; stopped.')


def pointer(expected):
    require(BASE.is_dir() and not BASE.is_symlink(), 'Invalid deployment root.')
    current = BASE / 'current'
    require(current.is_symlink() and os.readlink(current) == expected, 'Current web pointer drift; stopped.')


def switch(target):
    pending = BASE / ('.web-switch-' + str(os.getpid()))
    require(not pending.exists() and not pending.is_symlink(), 'Pending switch exists.')
    os.symlink(str(target), pending)
    try:
        os.replace(pending, BASE / 'current')
    finally:
        if pending.is_symlink():
            pending.unlink()


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


def request(origin, path, method='GET', headers=None, limit=3 * 1024 ** 2):
    opener = urllib.request.build_opener(NoRedirect())
    req = urllib.request.Request(origin + path, method=method, headers=headers or {})
    with opener.open(req, timeout=20) as response:
        body = response.read(limit + 1)
        require(len(body) <= limit, 'HTTP response exceeded verification limit.')
        return response.status, response.headers, body


def ready(origin, config=None):
    for attempt in range(20):
        try:
            require(request(origin, '/health')[0] == 204, 'Health status mismatch.')
            if config:
                require(json.loads(request(origin, '/web-release.json')[2])['id'] == config['id'], 'Wrong active web release.')
            return
        except (OSError, RuntimeError, ValueError):
            if attempt == 19:
                raise
            time.sleep(1)


TOOLS = ('merge', 'organize', 'split', 'images-pdf', 'numbers', 'remove', 'extract', 'rotate', 'watermark', 'crop')


def verify_catalog_html(body):
    # Ignore inert navigation templates when counting the rendered catalog.
    match = re.search(rb'<main\b.*?</main>', body, re.S)
    require(match is not None, 'Rendered catalog missing.')
    main = match.group()
    require(main.count(b'class="tool-card-content"') == len(TOOLS), 'Active catalog count changed.')
    require(main.count(b'class="tool-card planned"') == 8, 'Unavailable catalog count changed.')
    for tool in TOOLS:
        require(('href="/alat/' + tool + '"').encode() in main, 'Missing active tool: ' + tool)


def verify_browser_policy(headers, body, diagnostics=False):
    directives = [part.strip() for part in headers.get('Content-Security-Policy', '').split(';') if part.strip()]
    connections = [part for part in directives if part.startswith('connect-src ')]
    require(connections == ["connect-src 'self'" if diagnostics else "connect-src 'none'"] and "worker-src 'self'" in directives, 'Browser CSP mismatch.')
    marker = b'name="oto-diagnostics" content="/api/diagnostics/events"' in body
    require(marker == diagnostics, 'Anonymous diagnostics activation mismatch.')


def verify_http(origin, config, full_assets=False):
    ready(origin, config)
    if config.get('privacyPolicy'):
        policy = config['privacyPolicy']
        status, headers, body = request(origin, policy['path'])
        require(status == 200 and 'text/html' in headers.get('Content-Type', ''), 'Privacy page unavailable.')
        for phrase in policy['requiredText']:
            require(phrase.encode('utf-8') in body, 'Privacy page content missing: ' + phrase)
        for asset in policy['assets']:
            status, _, body = request(origin, asset['path'])
            require(status == 200 and hashlib.sha256(body).hexdigest() == asset['sha256'], 'Privacy asset mismatch.')
    status, headers, body = request(origin, '/')
    require(status == 200, 'Web catalog is missing.')
    verify_catalog_html(body)
    diagnostics = config.get('anonymousDiagnostics') is True
    verify_browser_policy(headers, body, diagnostics)
    if diagnostics:
        require(request(origin, '/privasi-web')[0] == 200, 'Web diagnostics privacy notice missing.')
    for tool in TOOLS:
        status, _, body = request(origin, '/alat/' + tool)
        main = re.search(rb'<main\b.*?</main>', body, re.S)
        require(status == 200 and main is not None, 'Tool route failed: ' + tool)
        require(('data-browser-tool="' + tool + '"').encode() in main.group() and b'workspace-feature-icon' in main.group(), 'Wrong workspace: ' + tool)
        if tool != 'merge':
            require(b'data-workspace-layout="shared"' in main.group(), 'Shared template missing: ' + tool)
    for route in ('/favorit', '/panduan', '/unduh'):
        require(request(origin, route)[0] == 200, 'Required route failed: ' + route)
    rejected = [('/alat/merge', 'POST', 405), ('/alat/ocr', 'GET', 404)]
    if diagnostics:
        rejected += [('/api/diagnostics/events', 'GET', 405), ('/api/diagnostics/events', 'POST', 403), ('/events.jsonl', 'GET', 404)]
    for route, method, expected in rejected:
        try:
            request(origin, route, method=method)
        except urllib.error.HTTPError as error:
            require(error.code == expected, 'Unexpected rejected route status.')
        else:
            raise RuntimeError('Restricted route accepted.')
    require(request(origin, '/api/releases/windows-x64/latest')[0] == 204, 'Legacy updater boundary changed.')
    baseline = json.loads(request(origin, '/api/releases/windows-x64/baseline-1/latest')[2])
    require(baseline == config['publicBaseline'], 'Public baseline metadata changed.')
    require(json.loads(request(origin, '/api/releases/windows-x64/installer')[2]) == config['publicInstaller'], 'Public installer metadata changed.')
    for key in ('onlineInstaller', 'offlineInstaller', 'updatePayload'):
        artifact = baseline[key]
        path = '/downloads/' + artifact['fileName']
        status, hdr, _ = request(origin, path, method='HEAD')
        require(status == 200 and int(hdr.get('Content-Length', '-1')) == artifact['size'], 'Download HEAD mismatch.')
        status, hdr, body = request(origin, path, headers={'Range': 'bytes=0-31'})
        require(status == 206 and len(body) == 32, 'Download range failed.')
    selected = config['files'] if full_assets else [f for f in config['files'] if f['path'] in ('wwwroot/js/pdf-worker.js', 'wwwroot/js/pdf-preview.js', 'wwwroot/vendor/pdfjs-6.4.299/build/pdf.worker.min.mjs')]
    for entry in selected:
        if entry['path'].startswith('wwwroot/'):
            path = '/' + entry['path'][8:]
            # Static files without a known content type are intentionally not served.
            if Path(path).suffix not in ('.js', '.mjs', '.css', '.png', '.svg', '.json', '.txt'):
                continue
            status, _, body = request(origin, path, limit=max(entry['size'], 1))
            require(status == 200 and hashlib.sha256(body).hexdigest() == entry['sha256'], 'Served asset differs: ' + path)
    print('PASS HTTP web/catalog/guide/security and unchanged desktop metadata/download ranges: ' + origin)


def restart():
    subprocess.run(['systemctl', 'restart', SERVICE], check=True, timeout=45)


def write_state(path, state):
    path.write_text(json.dumps(state, indent=2) + '\n')
    path.chmod(0o640)


def activate(root, config, pin):
    import grp
    unchanged(config)
    pointer(config['previousWeb'])
    subprocess.run(['systemctl', 'is-active', '--quiet', SERVICE], check=True)
    destination = BASE / config['id']
    require(not destination.exists() and not destination.is_symlink(), 'Immutable release already exists; inspect before retry.')
    destination.mkdir(mode=0o750)
    gid = grp.getgrnam('oto-web').gr_gid
    os.chown(destination, 0, gid)
    shutil.copytree(root / 'web', destination / 'web')
    inventory(destination / 'web', config['files'])
    for file in (destination / 'web', *(destination / 'web').rglob('*')):
        os.chown(file, 0, gid)
        file.chmod(0o750 if file.is_dir() else 0o640)
    shutil.copyfile(root / 'publication.json', destination / 'publication.json')
    (destination / 'publication.json').chmod(0o640)
    state = {'id': config['id'], 'publicationSha256': pin, 'previousWeb': config['previousWeb'], 'status': 'prepared'}
    write_state(destination / 'state.json', state)
    # Recheck after the copy and immediately before switching.
    unchanged(config)
    pointer(config['previousWeb'])
    try:
        switch(destination / 'web')
        restart()
        verify_http('http://127.0.0.1:5088', config)
        verify_http('https://oto.ayani.id', config)
        unchanged(config)
        state['status'] = 'active'
        write_state(destination / 'state.json', state)
        print('PASS published ' + config['id'] + '; desktop feed and hosting config unchanged.')
    except BaseException:
        # Refuse to overwrite another deployment's pointer.
        pointer(str(destination / 'web'))
        switch(config['previousWeb'])
        restart()
        ready('http://127.0.0.1:5088')
        unchanged(config)
        state['status'] = 'failed-rolled-back'
        write_state(destination / 'state.json', state)
        print('ROLLBACK completed; previous website restored; failed candidate retained.')
        raise


def rollback(config, pin):
    destination = BASE / config['id']
    regular(destination / 'publication.json')
    require(digest(destination / 'publication.json') == pin, 'Installed publication pin changed.')
    regular(destination / 'state.json')
    state = json.loads((destination / 'state.json').read_text())
    require(state['status'] == 'active' and state['publicationSha256'] == pin, 'Release is not the expected active publication.')
    unchanged(config)
    pointer(str(destination / 'web'))
    switch(config['previousWeb'])
    restart()
    ready('http://127.0.0.1:5088')
    unchanged(config)
    state['status'] = 'rolled-back'
    write_state(destination / 'state.json', state)
    print('PASS guarded rollback; desktop catalog/artifacts retained.')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['verify', 'http', 'activate', 'rollback'])
    parser.add_argument('--metadata-sha256', required=True)
    parser.add_argument('--origin', default='https://oto.ayani.id')
    args = parser.parse_args()
    root = Path(__file__).absolute().parent
    config = load(root, args.metadata_sha256)
    if args.action == 'verify':
        print('PASS pinned web-only inventory; no service changes.')
    elif args.action == 'http':
        verify_http(args.origin, config, full_assets=True)
    else:
        require(os.geteuid() == 0, 'Operator sudo required.')
        import fcntl
        lockpath = Path('/run/lock/oto-web-release.lock')
        fd = os.open(lockpath, os.O_CREAT | os.O_RDWR | os.O_NOFOLLOW, 0o600)
        with os.fdopen(fd, 'w') as lock:
            fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
            if args.action == 'activate':
                activate(root, config, args.metadata_sha256)
            else:
                rollback(config, args.metadata_sha256)


if __name__ == '__main__':
    main()
