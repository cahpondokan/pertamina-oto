#!/usr/bin/env python3
"""Narrow recovery for an installation stopped at the username prompt.
Verifies the immutable original package, installed files, identities, runtime and
unchanged public predecessor before continuing. Never deletes/recreates data.
"""
import argparse
import fcntl
import grp
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import pwd
import shutil
import socket
import subprocess

HERE=Path(__file__).resolve().parent
ORIGINAL=Path('/home/aynadmin/oto-web-updates/oto-monitor-20261027-1baeff233927-f5a379c6a6b4/contents')
ORIGINAL_PIN='f848dcb100cde00cc35b575e4f5f97795aaa4e63c251c17b9a27201277400ae5'
ORIGINAL_HELPER='68726d73aecdc23fdd75e3dba579ffef527dcb6fefbe8aee3bdaa016e3cc9889'
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def require(value,message):
    if not value:raise RuntimeError(message)
def load(path,name):
    spec=importlib.util.spec_from_file_location(name,path);m=importlib.util.module_from_spec(spec);spec.loader.exec_module(m);return m
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--manifest-sha256',required=True);args=parser.parse_args()
    require(sha(HERE/'recovery.json')==args.manifest_sha256,'Recovery manifest pin mismatch.')
    manifest=json.loads((HERE/'recovery.json').read_text())
    for name in ('resume_after_username.py','set_admin_retry.py'):
        require(not (HERE/name).is_symlink() and sha(HERE/name)==manifest['files'][name],'Recovery file changed.')
    require(os.geteuid()==0,'Interactive sudo required.')
    require(sha(ORIGINAL/'deploy_monitor.py')==ORIGINAL_HELPER,'Original helper changed.')
    d=load(ORIGINAL/'deploy_monitor.py','original_deployment');config,helper,web=d.load(ORIGINAL_PIN)
    fd=os.open('/run/lock/oto-web-release.lock',os.O_CREAT|os.O_RDWR|os.O_NOFOLLOW,0o600)
    with os.fdopen(fd,'w') as lock:
        fcntl.flock(lock,fcntl.LOCK_EX|fcntl.LOCK_NB)
        saved=json.loads(d.STATE.read_text())
        require(saved=={'id':config['id'],'status':'failed-safe','webRolledBack':False},'Not the expected stopped-username checkpoint.')
        helper.unchanged(web);helper.pointer(web['previousWeb'])
        require(set(socket.gethostbyname_ex('monitor.ayani.id')[2])=={'187.53.134.25'},'DNS drift.')
        for path in (d.SITE,d.LINK,d.UNIT,d.DROP,d.HOOK,Path('/etc/oto-monitor'),Path('/var/lib/oto-monitor-acme')):
            d.no_links(path);require(not path.exists(),'Later installation step exists; inspect before recovery: '+str(path))
        account=pwd.getpwnam('oto-monitor');group=grp.getgrnam('oto-monitor');readers=grp.getgrnam('oto-monitor-readers')
        require(account.pw_shell=='/usr/sbin/nologin' and account.pw_dir=='/nonexistent' and account.pw_gid==group.gr_gid,'Monitor account drift.')
        require(set(readers.gr_mem)=={'oto-monitor'},'Reader group drift.')
        require(d.DATA.is_dir() and not d.DATA.is_symlink() and not list(d.DATA.iterdir()),'Diagnostics store not the expected empty directory.')
        info=d.DATA.stat();require(info.st_uid==pwd.getpwnam('oto-web').pw_uid and info.st_gid==readers.gr_gid and info.st_mode&0o7777==0o2750,'Diagnostics permissions drift.')
        destination=d.BASE/'releases'/config['id'];require((d.BASE/'current').is_symlink() and (d.BASE/'current').readlink()==destination,'Monitor pointer drift.')
        d.inventory(destination,config['monitorFiles'])
        approved=Path('/opt/oto-downloads/runtime/10.0.12');copied=d.BASE/'runtime/10.0.12'
        original_files={p.relative_to(approved).as_posix() for p in approved.rglob('*') if p.is_file()}
        copied_files={p.relative_to(copied).as_posix() for p in copied.rglob('*') if p.is_file()}
        require(original_files==copied_files,'Runtime inventory drift.')
        for name in original_files:
            d.no_links(copied/name);require(sha(approved/name)==sha(copied/name),'Runtime bytes drift.')
        print('PASS exact stopped checkpoint, payload/runtime, reader permissions and public predecessor.')
        web_active=False
        try:
            load(HERE/'set_admin_retry.py','retry_admin').set_admin()
            shutil.copyfile(HERE/'set_admin_retry.py',d.BASE/'set_admin.py');(d.BASE/'set_admin.py').chmod(0o700)
            acme=Path('/var/lib/oto-monitor-acme');acme.mkdir(mode=0o755)
            template=(ORIGINAL/'monitor.ayani.id.conf').read_text();initial=template.split('server {\n    listen 443 ssl;',1)[0]
            d.write_new(d.SITE,initial);d.LINK.symlink_to(d.SITE);d.run('nginx','-t');d.run('systemctl','reload','nginx')
            d.run('certbot','certonly','--non-interactive','--webroot','-w',str(acme),'--cert-name','monitor.ayani.id','-d','monitor.ayani.id','--keep-until-expiring',timeout=180)
            d.SITE.write_text(template);d.run('nginx','-t');d.run('systemctl','reload','nginx')
            d.write_new(d.HOOK,'#!/bin/sh\nset -eu\nif [ "${RENEWED_LINEAGE:-}" = "/etc/letsencrypt/live/monitor.ayani.id" ]; then /usr/sbin/nginx -t && /usr/bin/systemctl reload nginx; fi\n',0o755)
            d.write_new(d.UNIT,(ORIGINAL/'oto-monitor.service').read_text());d.run('systemctl','daemon-reload');d.run('systemctl','enable','--now','oto-monitor.service');d.verify_private()
            drop='[Service]\nEnvironment=WebDiagnostics__Enabled=true\nEnvironment=WebDiagnostics__ReaderGroup=true\nEnvironment=WebDiagnostics__Directory=/var/lib/oto-diagnostics\nEnvironment=WebDiagnostics__Release='+web['id']+'\nSupplementaryGroups=oto-monitor-readers\nReadWritePaths=/var/lib/oto-diagnostics\n'
            d.write_new(d.DROP,drop);d.run('systemctl','daemon-reload');helper.activate(ORIGINAL,web,config['webMetadataSha256']);web_active=True
            d.verify_private();helper.unchanged(web)
            d.state(config,'active',webId=web['id'],webMetadataSha256=config['webMetadataSha256'],previousWeb=web['previousWeb'],dropSha256=d.digest(d.DROP),siteSha256=d.digest(d.SITE),unitSha256=d.digest(d.UNIT))
            print('PASS OTO Monitor active privately. Public OTO collector enabled; PDF contents are not accepted.')
        except BaseException:
            if web_active:helper.rollback(web,config['webMetadataSha256'])
            if d.DROP.exists():d.DROP.unlink();d.run('systemctl','daemon-reload');d.run('systemctl','restart','oto-downloads.service')
            if d.UNIT.exists():subprocess.run(['systemctl','disable','--now','oto-monitor.service'],check=False,timeout=30)
            if d.LINK.is_symlink() and d.LINK.readlink()==d.SITE:d.LINK.unlink();d.run('nginx','-t');d.run('systemctl','reload','nginx')
            d.state(config,'recovery-stopped',webRolledBack=web_active)
            print('Recovery stopped safely; inspect state before further action.');raise
if __name__=='__main__':
    try:main()
    except (RuntimeError,OSError,subprocess.SubprocessError) as e:raise SystemExit(str(e)) from None
