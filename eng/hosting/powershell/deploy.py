#!/usr/bin/env python3
"""Exact OTO development portal deployment; Python 3.10 standard library only."""
import argparse
import hashlib
import json
import os
import pathlib
import platform
import pwd
import grp
import shutil
import signal
import socket
import ssl
import stat
import subprocess
import sys
import tarfile
import tempfile
import time
import urllib.error
import urllib.request

ROOT = pathlib.Path(__file__).resolve().parent
BASE = pathlib.Path('/opt/oto-downloads')
DATA = pathlib.Path('/var/lib/oto-downloads')
SITE = pathlib.Path('/etc/nginx/sites-available/oto.ayani.id')
UNIT = pathlib.Path('/etc/systemd/system/oto-downloads.service')
SERVICE = 'oto-downloads.service'
# Exact package that reached loopback verification and then rolled back on HTTPS404.
PREVIOUS_FAILED_MANIFEST = 'fdce8cbf1a78c44c3f7f6e9e7b6e623313bbb2a2ce64b849ee74b2d04aa70e31'

def say(message):
    print(message, flush=True)

def require(condition, message):
    if not condition:
        raise RuntimeError(message)

def digest(path, algorithm='sha256'):
    result = hashlib.new(algorithm)
    with open(path, 'rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            result.update(block)
    return result.hexdigest()

def run(*args, check=True, capture=False):
    return subprocess.run(args, check=check, text=True, stdout=subprocess.PIPE if capture else None,
                          stderr=subprocess.STDOUT if capture else None)

def no_links(path):
    path = pathlib.Path(path).absolute()
    for item in (path, *path.parents):
        require(not item.is_symlink(), 'Tautan tidak diizinkan: ' + str(item))

def safe_member(name):
    require('\x00' not in name and '\\' not in name and ':' not in name, 'Nama arsip tidak aman.')
    path = pathlib.PurePosixPath(name)
    require(not path.is_absolute() and '..' not in path.parts, 'Path arsip keluar dari folder.')
    return path

def extract_regular(archive, destination):
    destination = pathlib.Path(destination)
    no_links(destination)
    with tarfile.open(archive, 'r:gz') as source:
        members = source.getmembers()
        require(0 < len(members) <= 15000, 'Jumlah file arsip tidak sesuai.')
        total = 0
        names = set()
        for member in members:
            path = safe_member(member.name)
            require(member.isfile() or member.isdir(), 'Arsip berisi link atau file khusus.')
            require(str(path) not in names, 'Nama arsip ganda.')
            names.add(str(path))
            total += member.size
            require(total <= 2 * 1024 ** 3 and member.size >= 0, 'Arsip terlalu besar.')
        destination.mkdir(parents=True, exist_ok=True)
        for member in members:
            target = destination.joinpath(*safe_member(member.name).parts)
            no_links(target)
            if member.isdir():
                target.mkdir(parents=True, exist_ok=True)
                continue
            target.parent.mkdir(parents=True, exist_ok=True)
            with source.extractfile(member) as input_file, open(target, 'wb') as output_file:
                shutil.copyfileobj(input_file, output_file, 1024 * 1024)
            require(target.stat().st_size == member.size, 'Ekstraksi tidak lengkap.')
            target.chmod(0o755 if member.mode & 0o111 else 0o644)

def verify_payload(expected):
    no_links(ROOT)
    require(digest(ROOT / 'SHA256SUMS') == expected, 'Daftar checksum paket berubah.')
    listed = set()
    for line in (ROOT / 'SHA256SUMS').read_text().splitlines():
        checksum, name = line.split('  ', 1)
        path = safe_member(name)
        require(str(path) == name and len(checksum) == 64, 'Format checksum tidak sesuai.')
        require(name not in listed, 'Checksum file ganda.')
        listed.add(name)
        file = ROOT / name
        no_links(file)
        require(file.is_file() and digest(file) == checksum, 'File paket berubah: ' + name)
    actual = set()
    for file in ROOT.rglob('*'):
        require(not file.is_symlink(), 'Paket berisi symlink.')
        if file.is_file() and file.relative_to(ROOT).as_posix() != 'SHA256SUMS':
            actual.add(file.relative_to(ROOT).as_posix())
    require(actual == listed, 'Ada file paket yang tidak terdaftar atau hilang.')
    config = json.loads((ROOT / 'deployment.json').read_text())
    require(config['release'] == '20261001-102920-701ec3e1' and config['deploymentId'] == 'oto-20261001-102920-701ec3e1-v1', 'Identitas rilis tidak sesuai.')
    require(digest(ROOT / 'runtime.tar.gz', 'sha512') == config['runtimeSha512'], 'Runtime Microsoft berubah.')
    return config

def validate_baseline(config, site_hash, marker, unit_hash, targets_exist, manifest):
    original = site_hash == config['baselineNginxSha256']
    candidate = site_hash == config['candidateNginxSha256']
    require(original or candidate, 'Konfigurasi Nginx OTO berubah; hentikan dan tinjau ulang.')
    if marker is None:
        require(original and not targets_exist and unit_hash is None, 'Ada pemasangan OTO yang tidak dimiliki paket ini.')
    else:
        known_failed_retry = (marker.get('manifest') == PREVIOUS_FAILED_MANIFEST
                              and marker.get('status') == 'failed-retained-for-retry'
                              and original and unit_hash == config['unitSha256'])
        require(marker.get('deploymentId') == config['deploymentId']
                and (marker.get('manifest') == manifest or known_failed_retry),
                'Pemasangan yang ada berasal dari paket lain.')
        require(unit_hash in (None, config['unitSha256']), 'Unit systemd berbeda dari paket.')

def preflight(config, manifest, admin=False):
    require(sys.platform == 'linux' and platform.machine() == 'x86_64', 'VPS harus Linux x86_64.')
    os_release = dict(line.split('=', 1) for line in pathlib.Path('/etc/os-release').read_text().splitlines() if '=' in line)
    require(os_release['ID'].strip('"') == 'ubuntu' and os_release['VERSION_ID'].strip('"') == '22.04', 'Paket ini khusus VPS Ubuntu22.04 yang sudah diperiksa.')
    require('187.53.134.25' in socket.gethostbyname_ex('oto.ayani.id')[2], 'DNS oto.ayani.id belum sesuai VPS.')
    for command in ('curl', 'systemctl', 'python3', 'runuser', 'nginx'):
        require(shutil.which(command), 'Perintah VPS belum tersedia: ' + command)
    for package in ('libc6', 'libgcc-s1', 'libstdc++6', 'libicu70', 'libssl3', 'zlib1g', 'libgssapi-krb5-2', 'ca-certificates'):
        status = run('dpkg-query', '-W', '-f=${Status}', package, capture=True, check=False)
        require(status.returncode == 0 and status.stdout.strip() == 'install ok installed', 'Dependency OS belum tersedia: ' + package)
    for path in (BASE, DATA, SITE, UNIT):
        no_links(path)
    marker_path = BASE / 'deployment.json'
    no_links(marker_path)
    marker = json.loads(marker_path.read_text()) if marker_path.exists() else None
    if marker is not None:
        require(marker_path.stat().st_uid == 0 and BASE.stat().st_uid == 0, 'Penanda deployment bukan milik root.')
    validate_baseline(config, digest(SITE), marker, digest(UNIT) if UNIT.exists() else None,
                      BASE.exists() or DATA.exists(), manifest)
    require(shutil.disk_usage('/opt').free >= 1024 ** 3, 'Ruang VPS kurang dari1GB.')
    active = run('systemctl', 'is-active', '--quiet', SERVICE, check=False).returncode == 0
    if active:
        require(marker is not None and UNIT.exists(), 'Layanan OTO aktif tidak dikenal.')
    else:
        with socket.socket() as probe:
            try:
                probe.bind(('127.0.0.1', 5088))
            except OSError as error:
                raise RuntimeError('Port5088 dipakai layanan lain.') from error
    if admin:
        require(os.geteuid() == 0, 'Aktivasi memerlukan sudo di terminal Anda.')
        run('nginx', '-t')
    say('PASS pemeriksaan VPS, DNS, dependency, baseline Nginx dan target deployment.')
    return marker

def request(url, method='GET', headers=None, timeout=30):
    request_headers = {'Cache-Control': 'no-cache'}
    request_headers.update(headers or {})
    return urllib.request.urlopen(urllib.request.Request(url, method=method, headers=request_headers), timeout=timeout)

def wait_public_health(origin, timeout=45):
    # nginx -s reload/systemctl reload signals the master; it does not wait for
    # the new workers. The old static site may still answer the first request.
    url = origin + '/health'
    deadline = time.monotonic() + timeout
    last_status = 'belum ada respons'
    notified = False
    while (remaining := deadline - time.monotonic()) > 0:
        try:
            with request(url, timeout=min(5, remaining)) as response:
                if response.status == 204:
                    return
                require(response.status in (404, 502, 503, 504),
                        'Health HTTPS tidak sesuai: ' + url + ' HTTP ' + str(response.status))
                last_status = 'HTTP ' + str(response.status)
        except urllib.error.HTTPError as error:
            if error.code not in (404, 502, 503, 504):
                raise
            last_status = 'HTTP ' + str(error.code)
            error.close()
        except urllib.error.URLError as error:
            # Never retry away a certificate/TLS validation failure.
            if isinstance(error.reason, ssl.SSLError):
                raise
            last_status = str(error.reason)
        except ssl.SSLError:
            raise
        except (TimeoutError, ConnectionError) as error:
            last_status = type(error).__name__
        if not notified:
            say('Menunggu proxy HTTPS siap setelah reload Nginx (maksimal ' + str(timeout) + ' detik)…')
            notified = True
        remaining = deadline - time.monotonic()
        if remaining > 0:
            time.sleep(min(1, remaining))
    raise RuntimeError('Proxy HTTPS belum siap: ' + url + ' — ' + last_status
                       + ' setelah ' + str(timeout) + ' detik.')

def verify_public(config):
    wait_public_health('https://oto.ayani.id')
    # Readiness is not acceptance: metadata, range and full file hash must pass.
    verify_http('https://oto.ayani.id', config)

def verify_http(origin, config):
    with request(origin + '/health') as response:
        require(response.status == 204, 'Health bukan204.')
    with request(origin + '/api/releases/windows-x64/latest') as response:
        body = response.read(16385)
        require(len(body) <= 16384, 'Metadata terlalu besar.')
        release = json.loads(body)
    require(release['version'] == config['release'] and release['installerSha256'].lower() == config['exeSha256'], 'Metadata rilis aktif berbeda.')
    require(release['installerFileName'] == config['exeFileName'], 'Nama installer berbeda.')
    with request(origin + '/') as response:
        require(('/downloads/' + config['exeFileName']).encode() in response.read(1024 * 1024), 'Tombol EXE tidak ditemukan.')
    url = origin + '/downloads/' + config['exeFileName']
    with request(url, 'HEAD') as response:
        require(int(response.headers['Content-Length']) == config['exeSize'], 'Ukuran EXE tidak sesuai.')
    with request(url, headers={'Range': 'bytes=0-9'}) as response:
        require(response.status == 206 and len(response.read()) == 10, 'Range EXE tidak sesuai.')
    result = hashlib.sha256()
    size = 0
    with request(url) as response:
        require('attachment' in response.headers.get('Content-Disposition', ''), 'EXE bukan attachment.')
        for chunk in iter(lambda: response.read(1024 * 1024), b''):
            size += len(chunk)
            require(size <= config['exeSize'], 'Unduhan EXE melebihi batas.')
            result.update(chunk)
    require(size == config['exeSize'] and result.hexdigest() == config['exeSha256'], 'Hash EXE HTTP tidak cocok.')
    say('PASS health, metadata, tombol EXE, HEAD, range dan SHA256 unduhan: ' + origin)

def wait_health(origin, process=None):
    for _ in range(30):
        if process is not None:
            require(process.poll() is None, 'Portal berhenti sebelum health siap. Lihat smoke.log.')
        try:
            with request(origin + '/health') as response:
                if response.status == 204:
                    return
        except (OSError, urllib.error.URLError):
            pass
        time.sleep(1)
    raise RuntimeError('Portal tidak siap dalam30detik.')

def smoke(config):
    # Temporary process belongs to the invoking SSH account; never touches systemd/Nginx.
    folder = ROOT.parent / 'smoke-runtime'
    extract_regular(ROOT / 'runtime.tar.gz', folder)
    environment = dict(os.environ, DOTNET_ROOT=str(folder), DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_NOLOGO='1', PYTHONDONTWRITEBYTECODE='1')
    run(str(folder / 'dotnet'), '--list-runtimes')
    with socket.socket() as probe:
        probe.bind(('127.0.0.1', 0))
        port = probe.getsockname()[1]
    origin = 'http://127.0.0.1:' + str(port)
    with open(ROOT.parent / 'smoke.log', 'w') as log:
        process = subprocess.Popen([str(folder / 'dotnet'), str(ROOT / 'web/Pertamina.Oto.Downloads.dll'), '--urls', origin,
                                    '--contentRoot', str(ROOT / 'web'), '--Distribution:Root=' + str(ROOT / 'releases')],
                                   cwd=ROOT / 'web', env=environment, stdout=log, stderr=subprocess.STDOUT)
        try:
            wait_health(origin, process)
            verify_http(origin, config)
        finally:
            process.terminate()
            try:
                process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                process.kill()
                process.wait()
    (ROOT.parent / 'smoke-result.txt').write_text('PASS Linux runtime and real portal/EXE HTTP checks. Temporary process stopped. No sudo/Nginx/systemd/public activation.\n')

def atomic_text(path, text, mode=0o644):
    no_links(path)
    handle, temp = tempfile.mkstemp(prefix='.oto-', dir=path.parent)
    with os.fdopen(handle, 'w') as stream:
        stream.write(text)
    os.chmod(temp, mode)
    os.replace(temp, path)

def private_permissions(path, group, executable=False):
    no_links(path)
    for file in (path, *path.rglob('*')):
        require(not file.is_symlink(), 'Symlink pada deployment ditolak.')
        os.chown(file, 0, group)
        mode = 0o750 if file.is_dir() or (executable and file.stat().st_mode & 0o111) else 0o640
        file.chmod(mode)

def service_account():
    try:
        account = pwd.getpwnam('oto-web')
        require(0 < account.pw_uid < 1000 and account.pw_shell in ('/usr/sbin/nologin', '/sbin/nologin', '/bin/false'), 'Akun oto-web tidak sesuai akun layanan.')
        require(grp.getgrgid(account.pw_gid).gr_name == 'oto-web', 'Grup akun oto-web berbeda.')
    except KeyError:
        try:
            grp.getgrnam('oto-web')
            raise RuntimeError('Grup oto-web sudah ada tanpa akun; tinjau dahulu.')
        except KeyError:
            run('useradd', '--system', '--user-group', '--no-create-home', '--shell', '/usr/sbin/nologin', 'oto-web')
        account = pwd.getpwnam('oto-web')
    return account.pw_gid

def backup_path(config):
    return pathlib.Path('/var/backups/oto-downloads') / config['deploymentId']

def set_marker(config, manifest, status):
    atomic_text(BASE / 'deployment.json', json.dumps({'deploymentId': config['deploymentId'], 'manifest': manifest, 'status': status}) + '\n')

def restore_site(config):
    backup = backup_path(config) / 'oto.ayani.id.before.conf'
    require(backup.is_file() and digest(backup) == config['baselineNginxSha256'], 'Backup Nginx tidak sesuai.')
    current = digest(SITE)
    require(current in (config['baselineNginxSha256'], config['candidateNginxSha256']), 'Nginx berubah; rollback otomatis dihentikan.')
    if current != config['baselineNginxSha256']:
        previous = SITE.read_text()
        atomic_text(SITE, backup.read_text())
        try:
            run('nginx', '-t')
            run('systemctl', 'reload', 'nginx')
        except Exception:
            atomic_text(SITE, previous)
            raise

def deactivate(config, manifest):
    require(os.geteuid() == 0, 'Rollback memerlukan sudo.')
    preflight(config, manifest, admin=True)
    require(UNIT.exists() and digest(UNIT) == config['unitSha256'], 'Unit layanan tidak sesuai; tidak dihentikan.')
    restore_site(config)
    run('systemctl', 'disable', '--now', SERVICE)
    set_marker(config, manifest, 'rolled-back')
    say('ROLLBACK SELESAI: konfigurasi situs sebelumnya dipulihkan; file OTO tetap disimpan.')

def activate(config, manifest):
    preflight(config, manifest, admin=True)
    if digest(SITE) == config['candidateNginxSha256'] and run('systemctl', 'is-active', '--quiet', SERVICE, check=False).returncode == 0:
        verify_http('http://127.0.0.1:5088', config)
        verify_public(config)
        run('systemctl', 'enable', SERVICE)
        set_marker(config, manifest, 'active')
        say('AKTIVASI SUDAH SESUAI; tidak perlu memasang ulang.')
        return
    backup = backup_path(config)
    no_links(backup)
    backup.mkdir(parents=True, exist_ok=True, mode=0o700)
    before = backup / 'oto.ayani.id.before.conf'
    no_links(before)
    if not before.exists():
        require(digest(SITE) == config['baselineNginxSha256'], 'Backup awal tidak dapat dibuat dari konfigurasi ini.')
        shutil.copy2(SITE, before)
        before.chmod(0o600)
    require(digest(before) == config['baselineNginxSha256'], 'Backup awal berubah.')
    group = service_account()
    BASE.mkdir(exist_ok=True, mode=0o755)
    DATA.mkdir(exist_ok=True, mode=0o755)
    BASE.chmod(0o755)
    DATA.chmod(0o755)
    set_marker(config, manifest, 'preparing')
    started = False
    try:
        if UNIT.exists():
            run('systemctl', 'stop', SERVICE)
        app = BASE / config['deploymentId'] / 'web'
        runtime = BASE / 'runtime' / config['runtimeVersion']
        no_links(runtime.parent)
        runtime.parent.mkdir(exist_ok=True, mode=0o755)
        runtime.parent.chmod(0o755)
        releases = DATA / 'releases'
        for path in (app, runtime, releases):
            no_links(path)
            if path.exists():
                for child in path.rglob('*'):
                    require(not child.is_symlink(), 'Tautan ditemukan di tujuan deployment.')
        say('Menyiapkan runtime ASP.NET khusus OTO dan paket aplikasi…')
        extract_regular(ROOT / 'runtime.tar.gz', runtime)
        shutil.copytree(ROOT / 'web', app, dirs_exist_ok=True)
        shutil.copytree(ROOT / 'releases', releases, dirs_exist_ok=True)
        private_permissions(app, group)
        os.chown(app.parent, 0, group)
        app.parent.chmod(0o750)
        private_permissions(releases, group)
        private_permissions(runtime, group, executable=True)
        runtime_info = run('runuser', '-u', 'oto-web', '--', str(runtime / 'dotnet'), '--list-runtimes', capture=True).stdout
        require('Microsoft.AspNetCore.App ' + config['runtimeVersion'] in runtime_info and 'Microsoft.NETCore.App ' + config['runtimeVersion'] in runtime_info, 'Runtime ASP.NET tidak dapat dijalankan akun layanan.')
        link = BASE / 'current'
        if link.is_symlink():
            require(link.resolve() == app.resolve(), 'Tautan current menunjuk versi lain.')
        else:
            require(not link.exists(), 'Target current bukan tautan milik OTO.')
            link.symlink_to(app, target_is_directory=True)
        atomic_text(UNIT, (ROOT / 'config/oto-downloads.service').read_text())
        run('systemctl', 'daemon-reload')
        started = True
        run('systemctl', 'start', SERVICE)
        wait_health('http://127.0.0.1:5088')
        verify_http('http://127.0.0.1:5088', config)
        say('Portal lokal sehat. Mengaktifkan proxy HTTPS oto.ayani.id…')
        atomic_text(SITE, (ROOT / 'config/oto.ayani.id.conf').read_text())
        run('nginx', '-t')
        run('systemctl', 'reload', 'nginx')
        verify_public(config)
        run('systemctl', 'enable', SERVICE)
        set_marker(config, manifest, 'active')
        say('AKTIVASI BERHASIL: https://oto.ayani.id — unduhan EXE sudah diverifikasi.')
    except BaseException:
        say('Aktivasi belum selesai. Memulihkan situs sebelumnya…')
        try:
            restore_site(config)
            if started:
                run('systemctl', 'disable', '--now', SERVICE, check=False)
            set_marker(config, manifest, 'failed-retained-for-retry')
        except Exception as rollback_error:
            say('Pemulihan memerlukan pemeriksaan administrator: ' + str(rollback_error))
        raise

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=('check', 'smoke', 'activate', 'rollback'))
    parser.add_argument('--manifest', required=True)
    args = parser.parse_args()
    require(len(args.manifest) == 64 and all(c in '0123456789abcdef' for c in args.manifest), 'Pin checksum tidak valid.')
    config = verify_payload(args.manifest)
    if args.action == 'check':
        preflight(config, args.manifest)
    elif args.action == 'smoke':
        preflight(config, args.manifest)
        smoke(config)
    else:
        require(os.geteuid() == 0, 'Gunakan sudo dari terminal SSH.')
        def interrupted(signum, frame):
            raise RuntimeError('Aktivasi dihentikan oleh terminal/sinyal; periksa hasil pemulihan.')
        for signum in (signal.SIGINT, signal.SIGTERM, signal.SIGHUP):
            signal.signal(signum, interrupted)
        import fcntl
        with open('/run/lock/oto-downloads-activation.lock', 'w') as lock:
            fcntl.flock(lock, fcntl.LOCK_EX | fcntl.LOCK_NB)
            if args.action == 'activate':
                activate(config, args.manifest)
            else:
                deactivate(config, args.manifest)

if __name__ == '__main__':
    try:
        main()
    except Exception as error:
        detail = ('HTTP ' + str(error.code) + ' pada ' + error.geturl()
                  if isinstance(error, urllib.error.HTTPError) else str(error))
        say('GAGAL: ' + detail)
        sys.exit(1)
