#!/usr/bin/env python3
"""Interactive operator-only credential creation/reset; no password arguments or logs."""
import base64
import getpass
import grp
import hashlib
import json
import os
from pathlib import Path
import re
import secrets
import subprocess
import sys

TARGET=Path('/etc/oto-monitor/admin.json')

def set_admin(reset=False):
    if os.geteuid()!=0 or not sys.stdin.isatty():raise RuntimeError('Jalankan melalui terminal sudo interaktif.')
    if TARGET.is_symlink() or TARGET.parent.is_symlink():raise RuntimeError('Symlink ditolak.')
    if TARGET.exists() and not reset:raise RuntimeError('Akun sudah ada; gunakan --reset secara eksplisit.')
    print('Nama pengguna login memakai huruf/angka, tanda - atau _. Tekan Enter untuk memakai pengelola.')
    while True:
        username=input('Nama pengguna login [pengelola]: ').strip() or 'pengelola'
        if re.fullmatch(r'[A-Za-z0-9_-]{3,40}',username):break
        print('Gunakan 3-40 huruf/angka/-/_, tanpa spasi atau apostrof; nama lengkap tidak diperlukan. Silakan ulangi.')
    while True:
        password=getpass.getpass('Kata sandi dashboard (14-128 karakter): ')
        if not 14<=len(password)<=128 or password.strip()!=password:
            print('Panjang kata sandi 14-128 karakter, tanpa spasi di awal/akhir. Silakan ulangi.');continue
        if password==getpass.getpass('Ulangi kata sandi: '):break
        print('Kata sandi tidak sama. Silakan ulangi.')
    salt=secrets.token_bytes(32)
    credential=dict(username=username,salt=base64.b64encode(salt).decode(),hash=base64.b64encode(hashlib.pbkdf2_hmac('sha256',password.encode(),salt,600000)).decode(),iterations=600000)
    gid=grp.getgrnam('oto-monitor').gr_gid
    TARGET.parent.mkdir(mode=0o750,exist_ok=True);os.chown(TARGET.parent,0,gid);TARGET.parent.chmod(0o750)
    temporary=TARGET.parent/('.admin-'+secrets.token_hex(8))
    fd=os.open(temporary,os.O_WRONLY|os.O_CREAT|os.O_EXCL|os.O_NOFOLLOW,0o600)
    with os.fdopen(fd,'w') as f:json.dump(credential,f);f.flush();os.fsync(f.fileno())
    os.chown(temporary,0,gid);temporary.chmod(0o640);os.replace(temporary,TARGET)
    print('Kredensial tersimpan sebagai hash; kata sandi tidak dicatat.')
    if reset:subprocess.run(['systemctl','restart','oto-monitor.service'],check=True)

if __name__=='__main__':
    if sys.argv[1:] not in ([],['--reset']):raise SystemExit('Usage: sudo python3 set_admin.py [--reset]')
    try:set_admin('--reset' in sys.argv)
    except (RuntimeError,OSError,subprocess.SubprocessError) as e:raise SystemExit(str(e)) from None
