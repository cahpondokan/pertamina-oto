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
    config = json.loads((root / 'publication.json').read_text(encoding='utf-8-sig'))
    require(re.fullmatch(r'[0-9]{8}-[0-9]{6}-[a-f0-9]{8}', config['version']), 'Invalid version.')
    original_id = 'oto-update-' + config['version']
    if 'retryOf' in config or 'retryMetadataSha256' in config:
        require(config.get('retryOf') == original_id and config['id'] == original_id + '-r2'
                and re.fullmatch(r'[a-f0-9]{64}', config.get('retryMetadataSha256', '')),
                'Invalid controlled retry identity.')
    else:
        require(config['id'] == original_id, 'Invalid publication ID.')
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
    catalog = json.loads((root / 'releases/current.json').read_text(encoding='utf-8-sig'))
    require(catalog['schema'] == 1 and catalog['version'] == config['version'] and catalog['channel'] == 'development', 'Catalog identity mismatch.')
    require(all(type(catalog.get(flag, False)) is bool for flag in ('runtimeBundled', 'manualInstallerOnly')), 'Invalid release policy.')
    for field, size, checksum in [('fileName', 'size', 'sha256'), ('installerFileName', 'installerSize', 'installerSha256')]:
        name = catalog[field]
        require(name == pathlib.Path(name).name and ':' not in name and '\\' not in name, 'Unsafe catalog name.')
        require(name in {e['path'][9:] for e in config['files'] if e['path'].startswith('releases/')}, 'Unlisted catalog file.')
        payload = root / 'releases' / name
        require(payload.stat().st_size == catalog[size] and digest(payload) == catalog[checksum].lower(), 'Catalog payload mismatch.')
    require((root / 'web/Pertamina.Oto.Downloads.dll').is_file() and (root / 'web/wwwroot/guide/guide.css').is_file(), 'Website/guide incomplete.')
    baseline_catalog(root, config, catalog)
    return config, catalog


def baseline_catalog(root, config, legacy):
    path = root / 'releases/distribution-v2.json'
    if not path.exists():
        require(not path.is_symlink(), 'Baseline catalog symlink rejected.')
        return None
    regular(path)
    require(0 < path.stat().st_size <= 98304, 'Baseline catalog size invalid.')
    value = json.loads(path.read_text(encoding='utf-8-sig'))
    require(value.get('schema') == 2 and value.get('product') == 'PERTAMINA OTO'
            and value.get('version') == config['version'] and value.get('channel') == 'development'
            and value.get('architecture') == 'x64' and value.get('runtimeBundled') is True
            and type(value.get('updaterProtocol')) is int and value['updaterProtocol'] == 2
            and type(value.get('minimumUpdaterBaseline')) is int and value['minimumUpdaterBaseline'] == 1,
            'Baseline identity/protocol mismatch.')
    require(re.fullmatch(r'[0-9]+\.[0-9]+\.[0-9]+', value.get('productVersion', ''))
            and re.fullmatch(r'[a-fA-F0-9]{64}', value.get('manifestSha256', ''))
            and type(value.get('expandedBytes')) is int and 0 < value['expandedBytes'] <= 2 * 1024**3,
            'Baseline product/manifest/budget invalid.')
    require(legacy.get('manualInstallerOnly') is True and legacy.get('runtimeBundled') is True
            and all(value.get(k) == legacy.get(k) for k in ('version', 'productVersion', 'publishedAt', 'channel', 'notes', 'releaseNotesJson', 'releaseNotesSha256')),
            'Legacy projection disagrees with baseline.')
    listed = {e['path'] for e in config['files']}
    for key, prefix in [('onlineInstaller', 'OnlineSetup'), ('offlineInstaller', 'Setup'), ('updatePayload', 'Windows')]:
        artifact = value.get(key, {})
        extension = '.zip' if key == 'updatePayload' else '.exe'
        expected = 'PERTAMINA-OTO-' + prefix + '-x64-' + config['version'] + extension
        require(artifact.get('fileName') == expected and type(artifact.get('size')) is int
                and 0 < artifact['size'] <= 2 * 1024**3 and re.fullmatch(r'[A-Fa-f0-9]{64}', artifact.get('sha256', '')),
                'Baseline artifact identity invalid.')
        require('releases/' + expected in listed, 'Baseline artifact not inventoried.')
        payload = root / 'releases' / expected
        regular(payload)
        require(payload.stat().st_size == artifact['size'] and digest(payload) == artifact['sha256'].lower(), 'Baseline artifact bytes mismatch.')
    require(value['onlineInstaller']['size'] < value['updatePayload']['size'], 'Online installer is not thin.')
    for key, name, size, checksum in [('offlineInstaller', 'installerFileName', 'installerSize', 'installerSha256'), ('updatePayload', 'fileName', 'size', 'sha256')]:
        artifact = value[key]
        require((artifact['fileName'], artifact['size'], artifact['sha256'].lower()) == (legacy[name], legacy[size], legacy[checksum].lower()), 'Baseline artifact projection mismatch.')
    notes = value.get('releaseNotesJson')
    if notes is not None:
        require(isinstance(notes, str) and len(notes.encode('utf-8')) <= 65536
                and hashlib.sha256(notes.encode('utf-8')).hexdigest() == value.get('releaseNotesSha256', '').lower(), 'Baseline notes hash invalid.')
    else:
        require(value.get('releaseNotesSha256') is None, 'Baseline notes pair invalid.')
    return value


def baseline_pointer(config):
    path = DATA / 'distribution-v2.json'
    expected = config.get('previousBaselineSha256')
    require(not path.is_symlink(), 'Baseline pointer symlink rejected.')
    if expected is None:
        require(not path.exists(), 'Unexpected baseline catalog; stop for fresh preflight.')
    else:
        require(re.fullmatch(r'[a-f0-9]{64}', expected), 'Invalid baseline pin.')
        regular(path)
        require(digest(path) == expected, 'Baseline catalog drift; stop.')


def restore_baseline(content, gid):
    path = DATA / 'distribution-v2.json'
    if content is not None:
        atomic(path, content, gid)
    elif path.exists() or path.is_symlink():
        regular(path)
        path.unlink()  # Only this exact managed pointer, restoring its prior absence.


def baseline(config):
    require(os.geteuid() == 0, 'Use sudo in your own SSH terminal; no password argument.')
    require(BASE.stat().st_uid == 0 and DATA.stat().st_uid == 0, 'Deployment ownership changed.')
    require(not BASE.is_symlink() and not DATA.is_symlink(), 'Deployment root symlink rejected.')
    require(digest(UNIT) == config['unitSha256'] and digest(SITE) == config['nginxSha256'], 'Service/Nginx drift; stop.')
    current = BASE / 'current'
    require(current.is_symlink() and os.readlink(current) == config['previousWeb'], 'Website baseline changed.')
    regular(DATA / 'current.json')
    prior = json.loads((DATA / 'current.json').read_text(encoding='utf-8-sig'))
    require(all(prior.get(key) == value for key, value in config['previousCatalog'].items()), 'Feed baseline changed.')
    baseline_pointer(config)
    require(subprocess.run(['systemctl', 'is-active', '--quiet', SERVICE]).returncode == 0, 'OTO service is not active.')


def retry_preflight(config):
    """One reviewed retry only; preserve failed targets/backups and reject any drift."""
    if 'retryOf' not in config:
        return
    original_id = 'oto-update-' + config['version']
    require(config['retryOf'] == original_id and config['id'] == original_id + '-r2', 'Retry identity mismatch.')
    previous = BACKUPS / original_id
    regular(previous / 'FAILED-ROLLED-BACK')
    require(not (previous / 'SUCCESS').exists(), 'Successful publication cannot be retried.')
    regular(previous / 'publication.json')
    require(digest(previous / 'publication.json') == config['retryMetadataSha256'], 'Retry predecessor metadata changed.')
    original = json.loads((previous / 'publication.json').read_text(encoding='utf-8-sig'))
    equivalent = {k: v for k, v in config.items() if k not in ('retryOf', 'retryMetadataSha256')}
    equivalent['id'] = original_id
    require(equivalent == original, 'Retry cannot change the verified payload or baseline.')
    regular(previous / 'current.json')
    require(digest(DATA / 'current.json') == digest(previous / 'current.json'), 'Failed publication rollback catalog drift.')
    regular(previous / 'previous-web.txt')
    require((previous / 'previous-web.txt').read_text() == config['previousWeb'], 'Failed publication rollback website drift.')
    regular(previous / 'baseline-state.json')
    prior_present = json.loads((previous / 'baseline-state.json').read_text())['present']
    if prior_present:
        regular(previous / 'distribution-v2.json')
        regular(DATA / 'distribution-v2.json')
        require(digest(previous / 'distribution-v2.json') == digest(DATA / 'distribution-v2.json'), 'Retry baseline catalog drift.')
    else:
        require(not (DATA / 'distribution-v2.json').exists(), 'Retry expected previous baseline absence.')
    # The retained failed website is evidence, never overwritten by a retry.
    for entry in config['files']:
        if entry['path'].startswith('web/'):
            file = BASE / original_id / entry['path']
            regular(file)
            require(file.stat().st_size == entry['size'] and digest(file) == entry['sha256'].lower(), 'Failed candidate website drift.')
    print('PASS exact failed predecessor and rollback verified; retained unchanged.', flush=True)


def warm_guide(origin):
    """Bound cold startup once, then require normal request latency; never ignore failure."""
    opener = urllib.request.build_opener(NoRedirect())
    for label, timeout in [('cold-start', 60), ('steady-state', 15)]:
        start = time.monotonic()
        with opener.open(origin + '/panduan', timeout=timeout) as response:
            body = response.read(512 * 1024 + 1)
            require(response.status == 200 and len(body) <= 512 * 1024
                    and b'id="update"' in body, 'Guide readiness verification failed.')
        print('PASS guide ' + label + ' %.3fs' % (time.monotonic() - start), flush=True)


def verify_http(origin, catalog, guide=True, baseline_release=None):
    opener = urllib.request.build_opener(NoRedirect())
    with opener.open(origin + '/health', timeout=15) as response:
        require(response.status == 204, 'Health not ready.')
    if guide:
        with opener.open(origin + '/panduan', timeout=15) as response:
            body = response.read(512 * 1024).decode('utf-8')
            require(response.status == 200 and 'id="update"' in body, 'Guide missing.')
    manual = catalog.get('manualInstallerOnly') is True
    if manual:
        with opener.open(origin + '/api/releases/windows-x64/latest', timeout=15) as response:
            require(response.status == 204 and not response.read(1), 'Manual installer leaked into in-app updater.')
    metadata_path = '/api/releases/windows-x64/installer' if manual else '/api/releases/windows-x64/latest'
    with opener.open(origin + metadata_path, timeout=15) as response:
        limit = 98304 if baseline_release is not None else 16384
        body = response.read(limit + 1)
        require(response.status == 200 and len(body) <= limit, 'HTTP metadata status/size invalid.')
        actual = json.loads(body)
        require(all(actual.get(key) == catalog.get(key) for key in
                    ('schema', 'version', 'channel', 'fileName', 'size', 'sha256',
                     'installerFileName', 'installerSize', 'installerSha256')), 'HTTP feed mismatch.')
        if manual:
            require(actual.get('manualInstallerOnly') is True and actual.get('runtimeBundled') == catalog.get('runtimeBundled'), 'HTTP release policy mismatch.')
    artifacts = [dict(fileName=catalog[name], size=catalog[size], sha256=catalog[sha])
                 for name, size, sha in [('fileName', 'size', 'sha256'), ('installerFileName', 'installerSize', 'installerSha256')]]
    if baseline_release is not None:
        with opener.open(origin + '/api/releases/windows-x64/baseline-1/latest', timeout=15) as response:
            body = response.read(98305)
            require(response.status == 200 and len(body) <= 98304, 'Baseline HTTP status/size invalid.')
            actual = json.loads(body)
            require(all(actual.get(k) == v for k, v in baseline_release.items()), 'Baseline HTTP metadata mismatch.')
            require('no-store' in response.headers.get('Cache-Control', ''), 'Baseline metadata must not be cached.')
        artifacts.append(baseline_release['onlineInstaller'])
    for artifact in artifacts:
        url = origin + '/downloads/' + artifact['fileName']
        with opener.open(urllib.request.Request(url, method='HEAD'), timeout=15) as response:
            require(int(response.headers['Content-Length']) == artifact['size'], 'HTTP size mismatch.')
        with opener.open(urllib.request.Request(url, headers={'Range': 'bytes=0-9'}), timeout=15) as response:
            require(response.status == 206 and len(response.read(11)) == 10, 'Range response invalid.')
        result = hashlib.sha256()
        total = 0
        with opener.open(url, timeout=30) as response:
            require(response.status == 200, 'Download response invalid.')
            for chunk in iter(lambda: response.read(1024 * 1024), b''):
                total += len(chunk)
                require(total <= artifact['size'], 'Oversize HTTP payload.')
                result.update(chunk)
        require(total == artifact['size'] and result.hexdigest() == artifact['sha256'].lower(), 'HTTP payload hash mismatch.')


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
    retry_preflight(config)
    new_baseline = baseline_catalog(root, config, catalog)
    gid = grp.getgrnam('oto-web').gr_gid
    backup = BACKUPS / config['id']
    require(not backup.exists(), 'Publication ID already used; do not overwrite/retry blindly.')
    target = BASE / config['id'] / 'web'
    require(not target.parent.exists(), 'Immutable website target already exists.')
    require(shutil.disk_usage(BASE).free > sum(e['size'] for e in config['files']) * 2, 'Insufficient free disk.')
    backup.mkdir(parents=True, mode=0o700)
    old_catalog = (DATA / 'current.json').read_bytes()
    old_baseline = (DATA / 'distribution-v2.json').read_bytes() if (DATA / 'distribution-v2.json').exists() else None
    (backup / 'current.json').write_bytes(old_catalog)
    (backup / 'baseline-state.json').write_text(json.dumps({'present': old_baseline is not None}))
    if old_baseline is not None:
        (backup / 'distribution-v2.json').write_bytes(old_baseline)
    (backup / 'previous-web.txt').write_text(config['previousWeb'])
    (backup / 'publication.json').write_bytes((root / 'publication.json').read_bytes())
    switched = False
    try:
        shutil.copytree(root / 'web', target)
        for path in [target.parent, target, *target.rglob('*')]:
            os.chown(path, 0, gid)
            path.chmod(0o750 if path.is_dir() else 0o640)
        for source in (root / 'releases').iterdir():
            if source.name in ('current.json', 'distribution-v2.json'):
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
        warm_guide('http://127.0.0.1:5088')
        print('PASS website readiness; now publishing hash-bound release metadata.', flush=True)
        atomic(DATA / 'current.json', (root / 'releases/current.json').read_bytes(), gid)
        if new_baseline is not None:
            atomic(DATA / 'distribution-v2.json', (root / 'releases/distribution-v2.json').read_bytes(), gid)
        verify_http('http://127.0.0.1:5088', catalog, baseline_release=new_baseline)
        verify_http('https://oto.ayani.id', catalog, baseline_release=new_baseline)
        (backup / 'SUCCESS').write_text(config['version'] + '\n')
        print('PASS public website, guide, guarded feeds and all installer/ZIP size/range/full SHA256; prior release retained.', flush=True)
    except BaseException:
        if switched:
            atomic(DATA / 'current.json', old_catalog, gid)
            restore_baseline(old_baseline, gid)
            link(pathlib.Path(config['previousWeb']))
            restart()
        (backup / 'FAILED-ROLLED-BACK').write_text('Review retained immutable files and backup before another publication.\n')
        raise


def rollback(config):
    require(os.geteuid() == 0, 'Rollback requires sudo in your own terminal.')
    backup = BACKUPS / config['id']
    require((backup / 'SUCCESS').is_file(), 'Successful exact publication required.')
    require(os.readlink(BASE / 'current') == str(BASE / config['id'] / 'web'), 'Later website detected; rollback stopped.')
    current = json.loads((DATA / 'current.json').read_text(encoding='utf-8-sig'))
    expected = json.loads((backup / 'publication.json').read_text(encoding='utf-8-sig'))
    require(expected == config, 'Rollback publication metadata changed.')
    require(current['version'] == config['version'], 'Later feed detected; rollback stopped.')
    pointer = next(entry for entry in config['files'] if entry['path'] == 'releases/current.json')
    regular(DATA / 'current.json')
    require(digest(DATA / 'current.json') == pointer['sha256'].lower(), 'Feed drift detected; rollback stopped.')
    baseline_entry = next((e for e in config['files'] if e['path'] == 'releases/distribution-v2.json'), None)
    if baseline_entry is not None:
        regular(DATA / 'distribution-v2.json')
        require(digest(DATA / 'distribution-v2.json') == baseline_entry['sha256'].lower(), 'Baseline feed drift; rollback stopped.')
    else:
        require(not (DATA / 'distribution-v2.json').exists(), 'Newer baseline feed detected; rollback stopped.')
    require(digest(UNIT) == config['unitSha256'] and digest(SITE) == config['nginxSha256'], 'Service/Nginx drift; rollback stopped.')
    atomic(DATA / 'current.json', (backup / 'current.json').read_bytes(), grp.getgrnam('oto-web').gr_gid)
    state = backup / 'baseline-state.json'
    old_baseline = (backup / 'distribution-v2.json').read_bytes() if state.exists() and json.loads(state.read_text())['present'] else None
    restore_baseline(old_baseline, grp.getgrnam('oto-web').gr_gid)
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
