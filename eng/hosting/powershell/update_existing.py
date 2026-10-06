#!/usr/bin/env python3
"""Guarded update of the existing OTO development portal; no Nginx/runtime changes."""
import argparse
import fcntl
import grp
import hashlib
import json
import os
import pathlib
import re
import shutil
import stat
import subprocess
import tempfile
import time
import urllib.request

BASE = pathlib.Path('/opt/oto-downloads')
DATA = pathlib.Path('/var/lib/oto-downloads/releases')
UNIT = pathlib.Path('/etc/systemd/system/oto-downloads.service')
SITE = pathlib.Path('/etc/nginx/sites-available/oto.ayani.id')
BACKUPS = pathlib.Path('/var/backups/oto-downloads')
SERVICE = 'oto-downloads.service'


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        raise RuntimeError('Redirect rejected during publication verification.')


def require(ok, message):
    if not ok:
        raise RuntimeError(message)


def digest(path):
    result = hashlib.sha256()
    with open(path, 'rb') as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            result.update(chunk)
    return result.hexdigest().lower()


def regular(path):
    path = pathlib.Path(path).absolute()
    for part in (path, *path.parents):
        require(not part.is_symlink(), 'Symlink rejected: ' + str(part))
    require(path.is_file() and stat.S_ISREG(path.stat().st_mode), 'Regular file required.')


def atomic(path, content, gid):
    require(not path.is_symlink(), 'Pointer symlink rejected.')
    fd, name = tempfile.mkstemp(prefix='.oto-pending-', dir=path.parent)
    try:
        with os.fdopen(fd, 'wb') as stream:
            stream.write(content)
            stream.flush()
            os.fsync(stream.fileno())
        os.chown(name, 0, gid)
        os.chmod(name, 0o640)
        os.replace(name, path)
    finally:
        if os.path.exists(name):
            os.unlink(name)


def link(target):
    pending = BASE / ('.current-' + str(os.getpid()))
    require(not pending.exists() and not pending.is_symlink(), 'Pending pointer already exists.')
    os.symlink(str(target), pending)
    os.replace(pending, BASE / 'current')


def inventory(root, expected):
    regular(root / 'publication.json')
    require(digest(root / 'publication.json') == expected.lower(), 'Publication metadata changed.')
    config = json.loads((root / 'publication.json').read_text())
    require(re.fullmatch(r'oto-update-[0-9]{8}-[0-9]{6}-[a-f0-9]{8}', config['id']), 'Invalid publication ID.')
    require(re.fullmatch(r'[0-9]{8}-[0-9]{6}-[a-f0-9]{8}', config['version']), 'Invalid version.')
    listed = set()
    for entry in config['files']:
        name = entry['path']
        parts = pathlib.PurePosixPath(name)
        require(name == parts.as_posix() and not parts.is_absolute() and '..' not in parts.parts
                and '\\' not in name and ':' not in name, 'Unsafe inventory path.')
        require(name.startswith(('web/', 'releases/')) and name not in listed, 'Invalid/duplicate inventory entry.')
        listed.add(name)
        file = root / name
        regular(file)
        require(file.stat().st_size == entry['size'] and digest(file) == entry['sha256'].lower(), 'Payload mismatch: ' + name)
    actual = set()
    for folder in ('web', 'releases'):
        for file in (root / folder).rglob('*'):
            require(not file.is_symlink(), 'Payload symlink rejected.')
            if file.is_file():
                actual.add(file.relative_to(root).as_posix())
    require(actual == listed, 'Unlisted/missing payload.')
    catalog = json.loads((root / 'releases/current.json').read_text())
    require(catalog['schema'] == 1 and catalog['version'] == config['version'] and catalog['channel'] == 'development', 'Catalog identity mismatch.')
    for field, size, checksum in [('fileName', 'size', 'sha256'), ('installerFileName', 'installerSize', 'installerSha256')]:
        name = catalog[field]
        require(name == pathlib.Path(name).name and ':' not in name and '\\' not in name, 'Unsafe catalog name.')
        require(name in {e['path'][9:] for e in config['files'] if e['path'].startswith('releases/')}, 'Unlisted catalog file.')
        payload = root / 'releases' / name
        require(payload.stat().st_size == catalog[size] and digest(payload) == catalog[checksum].lower(), 'Catalog payload mismatch.')
    require((root / 'web/Pertamina.Oto.Downloads.dll').is_file() and (root / 'web/wwwroot/guide/guide.css').is_file(), 'Website/guide incomplete.')
    return config, catalog


def baseline(config):
    require(os.geteuid() == 0, 'Use sudo in your own SSH terminal; no password argument.')
    require(BASE.stat().st_uid == 0 and DATA.stat().st_uid == 0, 'Deployment ownership changed.')
    require(not BASE.is_symlink() and not DATA.is_symlink(), 'Deployment root symlink rejected.')
    require(digest(UNIT) == config['unitSha256'] and digest(SITE) == config['nginxSha256'], 'Service/Nginx drift; stop.')
    current = BASE / 'current'
    require(current.is_symlink() and os.readlink(current) == config['previousWeb'], 'Website baseline changed.')
    regular(DATA / 'current.json')
    prior = json.loads((DATA / 'current.json').read_text())
    require(all(prior.get(key) == value for key, value in config['previousCatalog'].items()), 'Feed baseline changed.')
    require(subprocess.run(['systemctl', 'is-active', '--quiet', SERVICE]).returncode == 0, 'OTO service is not active.')


def verify_http(origin, catalog, guide=True):
    opener = urllib.request.build_opener(NoRedirect())
    with opener.open(origin + '/health', timeout=15) as response:
        require(response.status == 204, 'Health not ready.')
    if guide:
        with opener.open(origin + '/panduan', timeout=15) as response:
            body = response.read(512 * 1024).decode('utf-8')
            require(response.status == 200 and 'id="update"' in body, 'Guide missing.')
    with opener.open(origin + '/api/releases/windows-x64/latest', timeout=15) as response:
        actual = json.loads(response.read(16385))
        require(all(actual.get(key) == catalog.get(key) for key in
                    ('schema', 'version', 'channel', 'fileName', 'size', 'sha256',
                     'installerFileName', 'installerSize', 'installerSha256')), 'HTTP feed mismatch.')
    for name, size, sha in [('fileName', 'size', 'sha256'), ('installerFileName', 'installerSize', 'installerSha256')]:
        url = origin + '/downloads/' + catalog[name]
        with opener.open(urllib.request.Request(url, method='HEAD'), timeout=15) as response:
            require(int(response.headers['Content-Length']) == catalog[size], 'HTTP size mismatch.')
        with opener.open(urllib.request.Request(url, headers={'Range': 'bytes=0-9'}), timeout=15) as response:
            require(response.status == 206 and len(response.read(11)) == 10, 'Range response invalid.')
        result = hashlib.sha256()
        total = 0
        with opener.open(url, timeout=30) as response:
            require(response.status == 200, 'Download response invalid.')
            for chunk in iter(lambda: response.read(1024 * 1024), b''):
                total += len(chunk)
                require(total <= catalog[size], 'Oversize HTTP payload.')
                result.update(chunk)
        require(total == catalog[size] and result.hexdigest() == catalog[sha].lower(), 'HTTP payload hash mismatch.')


def restart():
    subprocess.run(['systemctl', 'restart', SERVICE], check=True)
    for _ in range(45):
        try:
            with urllib.request.urlopen('http://127.0.0.1:5088/health', timeout=2) as response:
                if response.status == 204:
                    return
        except OSError:
            pass
        time.sleep(1)
    raise RuntimeError('OTO readiness timeout.')


def activate(root, config, catalog):
    baseline(config)
    gid = grp.getgrnam('oto-web').gr_gid
    backup = BACKUPS / config['id']
    require(not backup.exists(), 'Publication ID already used; do not overwrite/retry blindly.')
    target = BASE / config['id'] / 'web'
    require(not target.parent.exists(), 'Immutable website target already exists.')
    require(shutil.disk_usage(BASE).free > sum(e['size'] for e in config['files']) * 2, 'Insufficient free disk.')
    backup.mkdir(parents=True, mode=0o700)
    old_catalog = (DATA / 'current.json').read_bytes()
    (backup / 'current.json').write_bytes(old_catalog)
    (backup / 'previous-web.txt').write_text(config['previousWeb'])
    (backup / 'publication.json').write_bytes((root / 'publication.json').read_bytes())
    switched = False
    try:
        shutil.copytree(root / 'web', target)
        for path in [target.parent, target, *target.rglob('*')]:
            os.chown(path, 0, gid)
            path.chmod(0o750 if path.is_dir() else 0o640)
        for source in (root / 'releases').iterdir():
            if source.name == 'current.json':
                continue
            regular(source)
            destination = DATA / source.name
            if destination.exists():
                regular(destination)
                require(destination.stat().st_size == source.stat().st_size and digest(destination) == digest(source), 'Existing immutable payload differs.')
            else:
                with open(source, 'rb') as input_file, open(destination, 'xb') as output_file:
                    shutil.copyfileobj(input_file, output_file, 1024 * 1024)
                os.chown(destination, 0, gid)
                destination.chmod(0o640)
            require(digest(destination) == digest(source), 'Server copy hash mismatch.')
        link(target)
        switched = True
        restart()
        # Website is healthy before desktop metadata becomes visible.
        with urllib.request.urlopen('http://127.0.0.1:5088/panduan', timeout=15) as response:
            require(response.status == 200 and 'id="update"' in response.read(512 * 1024).decode('utf-8'), 'Guide verification failed.')
        atomic(DATA / 'current.json', (root / 'releases/current.json').read_bytes(), gid)
        verify_http('http://127.0.0.1:5088', catalog)
        verify_http('https://oto.ayani.id', catalog)
        (backup / 'SUCCESS').write_text(config['version'] + '\n')
        print('PASS public website, guide, feed, EXE and ZIP size/range/full SHA256; prior release retained.', flush=True)
    except BaseException:
        if switched:
            atomic(DATA / 'current.json', old_catalog, gid)
            link(pathlib.Path(config['previousWeb']))
            restart()
        (backup / 'FAILED-ROLLED-BACK').write_text('Review retained immutable files and backup before another publication.\n')
        raise


def rollback(config):
    require(os.geteuid() == 0, 'Rollback requires sudo in your own terminal.')
    backup = BACKUPS / config['id']
    require((backup / 'SUCCESS').is_file(), 'Successful exact publication required.')
    require(os.readlink(BASE / 'current') == str(BASE / config['id'] / 'web'), 'Later website detected; rollback stopped.')
    current = json.loads((DATA / 'current.json').read_text())
    expected = json.loads((backup / 'publication.json').read_text())
    require(expected == config, 'Rollback publication metadata changed.')
    require(current['version'] == config['version'], 'Later feed detected; rollback stopped.')
    pointer = next(entry for entry in config['files'] if entry['path'] == 'releases/current.json')
    regular(DATA / 'current.json')
    require(digest(DATA / 'current.json') == pointer['sha256'].lower(), 'Feed drift detected; rollback stopped.')
    require(digest(UNIT) == config['unitSha256'] and digest(SITE) == config['nginxSha256'], 'Service/Nginx drift; rollback stopped.')
    atomic(DATA / 'current.json', (backup / 'current.json').read_bytes(), grp.getgrnam('oto-web').gr_gid)
    link(pathlib.Path((backup / 'previous-web.txt').read_text()))
    restart()
    print('PASS prior website/feed restored; all immutable payloads retained.', flush=True)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['verify', 'activate', 'rollback'])
    parser.add_argument('--metadata-sha256', required=True)
    args = parser.parse_args()
    root = pathlib.Path(__file__).resolve().parent
    config, catalog = inventory(root, args.metadata_sha256)
    if args.action == 'verify':
        print('PASS exact local staging inventory/catalog/guide; no server mutation.')
        return
    require(os.geteuid() == 0, 'Use sudo in your own SSH terminal; no password argument.')
    regular(UNIT)
    regular(SITE)
    with open('/opt/oto-downloads/.publication.lock', 'a') as lock:
        fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
        if args.action == 'activate':
            activate(root, config, catalog)
        else:
            rollback(config)


if __name__ == '__main__':
    main()
