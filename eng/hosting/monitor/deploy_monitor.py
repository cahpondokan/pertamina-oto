#!/usr/bin/env python3
"""Guarded installation of OTO's private monitor and anonymous collector.
Run from a hash-verified package using an interactive sudo terminal.
No new software download, password argument, public dashboard or PDF storage.
"""
import argparse
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import re
import shutil
import socket
import subprocess
import time
import urllib.error
import urllib.request

BASE=Path('/opt/oto-monitor')
DATA=Path('/var/lib/oto-diagnostics')
SITE=Path('/etc/nginx/sites-available/monitor.ayani.id')
LINK=Path('/etc/nginx/sites-enabled/monitor.ayani.id')
UNIT=Path('/etc/systemd/system/oto-monitor.service')
DROP=Path('/etc/systemd/system/oto-downloads.service.d/monitor-diagnostics.conf')
HOOK=Path('/etc/letsencrypt/renewal-hooks/deploy/oto-monitor-reload')
ROOT=Path(__file__).resolve().parent
STATE=BASE/'install-state.json'

def require(value,message):
    if not value:raise RuntimeError(message)
def digest(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def run(*args,**kwargs):return subprocess.run(args,check=True,timeout=kwargs.pop('timeout',60),**kwargs)
def module(path,name):
    spec=importlib.util.spec_from_file_location(name,path);value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
def no_links(path):
    require(all(not item.is_symlink() for item in (path,*path.parents)),'Unexpected link: '+str(path))
def inventory(root,files):
    require(root.is_dir() and not root.is_symlink(),'Payload directory invalid.')
    expected=set()
    for item in files:
        name=item['path']
        require(isinstance(name,str) and not name.startswith('/') and '\\' not in name and ':' not in name and all(p not in ('','.','..') for p in name.split('/')) and name not in expected,'Unsafe payload name.')
        expected.add(name);p=root/name;no_links(p)
        require(p.is_file() and p.stat().st_size==item['size'] and digest(p)==item['sha256'],'Payload mismatch: '+name)
    actual=set()
    for p in root.rglob('*'):
        require(not p.is_symlink() and (p.is_file() or p.is_dir()),'Special payload rejected.')
        if p.is_file():actual.add(p.relative_to(root).as_posix())
    require(actual==expected,'Unlisted/missing payload.')
def load(pin):
    require(re.fullmatch('[a-f0-9]{64}',pin) and digest(ROOT/'monitor-package.json')==pin,'Package pin mismatch.')
    config=json.loads((ROOT/'monitor-package.json').read_text())
    require(re.fullmatch(r'oto-monitor-20261027-[a-f0-9]{12}',config['id']),'Release id invalid.')
    inventory(ROOT/'monitor',config['monitorFiles'])
    for name,pin in config['supportPins'].items():
        require('/' not in name and '\\' not in name and name not in ('.','..'),'Support path invalid.')
        no_links(ROOT/name);require(digest(ROOT/name)==pin,'Support pin mismatch: '+name)
    helper=module(ROOT/'deploy_web.py','oto_web_deploy')
    web=helper.load(ROOT,config['webMetadataSha256'])
    return config,helper,web
def state(config,status,**extra):
    STATE.write_text(json.dumps(dict(id=config['id'],status=status,**extra),indent=2)+'\n');STATE.chmod(0o600)
def write_new(path,text,mode=0o644):
    no_links(path);require(not path.exists(),'Existing configuration; no overwrite: '+str(path))
    path.parent.mkdir(parents=True,exist_ok=True)
    with path.open('x') as stream:stream.write(text)
    path.chmod(mode)
def http_status(url,headers=None):
    request=urllib.request.Request(url,headers=headers or {})
    try:
        with urllib.request.urlopen(request,timeout=15) as r:return r.status,r.read()
    except urllib.error.HTTPError as e:return e.code,e.read()
def wait_private():
    for _ in range(30):
        try:
            status,body=http_status('http://127.0.0.1:5091/login',{'Host':'monitor.ayani.id','X-Forwarded-Proto':'https'})
            if status==200 and b'Monitor OTO' in body:return
        except OSError:pass
        time.sleep(.5)
    raise RuntimeError('Monitor private login not healthy.')
def verify_private():
    wait_private()
    require(http_status('http://127.0.0.1:5091/api/overview',{'Host':'monitor.ayani.id','X-Forwarded-Proto':'https'})[0]==401,'Private API bypassed authentication.')
    require(http_status('http://127.0.0.1:5091/login',{'Host':'oto.ayani.id','X-Forwarded-Proto':'https'})[0]==404,'Public hostname accepted.')
    # TLS is validated using the real certificate and SNI, with loopback resolution.
    result=run('curl','--silent','--show-error','--fail','--resolve','monitor.ayani.id:5443:127.0.0.1','https://monitor.ayani.id:5443/login',capture_output=True)
    require(b'Monitor OTO' in result.stdout,'Private TLS login missing.')
    for route in ['/','/login','/api/overview','/events.jsonl']:
        require(http_status('https://monitor.ayani.id'+route)[0]==404,'Public monitor route was reachable: '+route)
    listeners=subprocess.check_output(['ss','-ltnH'],text=True)
    for port in (5091,5443):
        bindings=[line.split()[3] for line in listeners.splitlines() if line.split()[3].endswith(':'+str(port))]
        require(bindings==['127.0.0.1:'+str(port)],'Non-loopback or absent monitor listener.')
    print('PASS private TLS/login, unauthenticated API denied, public routes denied, loopback-only ports.')
def activate(config,helper,web):
    import grp,pwd
    helper.unchanged(web);helper.pointer(web['previousWeb'])
    require(set(socket.gethostbyname_ex('monitor.ayani.id')[2])=={'187.53.134.25'},'DNS monitor.ayani.id must point only to approved VPS.')
    for path in (BASE,SITE,LINK,UNIT,DROP,HOOK,Path('/etc/oto-monitor'),DATA):
        no_links(path);require(not path.exists(),'Existing monitor installation or configuration: '+str(path))
    for name in ('oto-monitor','oto-monitor-readers'):
        try:grp.getgrnam(name)
        except KeyError:pass
        else:raise RuntimeError('Existing group needs operator review: '+name)
    try:pwd.getpwnam('oto-monitor')
    except KeyError:pass
    else:raise RuntimeError('Existing monitor account needs operator review.')
    require(shutil.which('certbot') and shutil.which('nginx') and shutil.which('curl'),'Existing certbot/nginx/curl required; no software installation is performed.')
    runtime=Path('/opt/oto-downloads/runtime/10.0.12');require((runtime/'dotnet').is_file(),'Approved existing .NET runtime absent.')
    for p in runtime.rglob('*'):
        require(not p.is_symlink(),'Runtime link requires review before copying.')
    run('nginx','-t');BASE.mkdir(mode=0o755);state(config,'preparing')
    web_active=False
    try:
        run('groupadd','--system','oto-monitor-readers');run('groupadd','--system','oto-monitor')
        run('useradd','--system','--gid','oto-monitor','--groups','oto-monitor-readers','--no-create-home','--home-dir','/nonexistent','--shell','/usr/sbin/nologin','oto-monitor')
        monitor_gid=grp.getgrnam('oto-monitor').gr_gid;reader_gid=grp.getgrnam('oto-monitor-readers').gr_gid
        owner=pwd.getpwnam('oto-web').pw_uid
        DATA.mkdir(mode=0o750);os.chown(DATA,owner,reader_gid);DATA.chmod(0o2750)
        destination=BASE/'releases'/config['id'];destination.parent.mkdir(mode=0o755)
        shutil.copytree(ROOT/'monitor',destination);inventory(destination,config['monitorFiles'])
        for p in (destination,*destination.rglob('*')):os.chown(p,0,monitor_gid);p.chmod(0o750 if p.is_dir() else 0o640)
        (BASE/'current').symlink_to(destination)
        (BASE/'runtime').mkdir(mode=0o755);shutil.copytree(runtime,BASE/'runtime/10.0.12')
        # Reuse the admitted runtime byte-for-byte, with only root write access.
        for p in runtime.rglob('*'):
            copy=BASE/'runtime/10.0.12'/p.relative_to(runtime)
            if p.is_file():require(digest(p)==digest(copy),'Runtime copy mismatch.')
        for p in (BASE/'runtime/10.0.12',*(BASE/'runtime/10.0.12').rglob('*')):
            executable=bool(p.stat().st_mode & 0o111)
            os.chown(p,0,monitor_gid);p.chmod(0o750 if p.is_dir() or executable else 0o640)
        # Exercise authentication and privacy on the actual Linux runtime before activation.
        test_env=dict(os.environ,OTO_DOTNET=str(BASE/'runtime/10.0.12/dotnet'),OTO_MONITOR_DLL=str(destination/'Pertamina.Oto.Monitor.dll'))
        run('python3','-B',str(ROOT/'test_monitor.py'),env=test_env,timeout=120)
        shutil.copyfile(ROOT/'set_admin.py',BASE/'set_admin.py');(BASE/'set_admin.py').chmod(0o700)
        module(ROOT/'set_admin.py','oto_monitor_admin').set_admin()
        acme=Path('/var/lib/oto-monitor-acme');no_links(acme);require(not acme.exists(),'ACME directory already exists.');acme.mkdir(mode=0o755)
        template=(ROOT/'monitor.ayani.id.conf').read_text();initial=template.split('server {\n    listen 443 ssl;',1)[0]
        write_new(SITE,initial);LINK.symlink_to(SITE);run('nginx','-t');run('systemctl','reload','nginx')
        # Reuse the existing ACME account; never silently accept new Terms of Service.
        run('certbot','certonly','--non-interactive','--webroot','-w',str(acme),'--cert-name','monitor.ayani.id','-d','monitor.ayani.id','--keep-until-expiring',timeout=180)
        SITE.write_text(template);run('nginx','-t');run('systemctl','reload','nginx')
        write_new(HOOK,'#!/bin/sh\nset -eu\nif [ "${RENEWED_LINEAGE:-}" = "/etc/letsencrypt/live/monitor.ayani.id" ]; then /usr/sbin/nginx -t && /usr/bin/systemctl reload nginx; fi\n',0o755)
        write_new(UNIT,(ROOT/'oto-monitor.service').read_text())
        run('systemctl','daemon-reload');run('systemctl','enable','--now','oto-monitor.service');verify_private()
        # No operator secrets are in the public application's environment or directory.
        drop='[Service]\nEnvironment=WebDiagnostics__Enabled=true\nEnvironment=WebDiagnostics__ReaderGroup=true\nEnvironment=WebDiagnostics__Directory=/var/lib/oto-diagnostics\nEnvironment=WebDiagnostics__Release='+web['id']+'\nSupplementaryGroups=oto-monitor-readers\nReadWritePaths=/var/lib/oto-diagnostics\n'
        write_new(DROP,drop);run('systemctl','daemon-reload')
        helper.activate(ROOT,web,config['webMetadataSha256']);web_active=True
        verify_private();helper.unchanged(web)
        state(config,'active',webId=web['id'],webMetadataSha256=config['webMetadataSha256'],previousWeb=web['previousWeb'],dropSha256=digest(DROP),siteSha256=digest(SITE),unitSha256=digest(UNIT))
        print('PASS monitor active privately; OTO anonymous metadata enabled. No PDF payload is accepted.')
    except BaseException:
        if web_active:helper.rollback(web,config['webMetadataSha256'])
        if DROP.exists():DROP.unlink();run('systemctl','daemon-reload');run('systemctl','restart','oto-downloads.service')
        if UNIT.exists():subprocess.run(['systemctl','disable','--now','oto-monitor.service'],check=False,timeout=30)
        if LINK.is_symlink() and LINK.readlink()==SITE:LINK.unlink();run('nginx','-t');run('systemctl','reload','nginx')
        state(config,'failed-safe',webRolledBack=web_active)
        print('Installation stopped safely. Private data/certificate are retained; inspect failed-safe state before retry.')
        raise
def rollback(config,helper,web):
    require(STATE.is_file(),'Installed state absent.');saved=json.loads(STATE.read_text())
    require(saved['id']==config['id'] and saved['status']=='active','Expected active installation required.')
    for path,key in ((DROP,'dropSha256'),(SITE,'siteSha256'),(UNIT,'unitSha256')):require(digest(path)==saved[key],'Installed configuration drift.')
    helper.rollback(web,config['webMetadataSha256'])
    DROP.unlink();run('systemctl','daemon-reload');run('systemctl','restart','oto-downloads.service')
    run('systemctl','disable','--now','oto-monitor.service')
    # Keep the deny-only public vhost and certificate; remove only the private listener.
    SITE.write_text(SITE.read_text().split('# This listener is reachable only',1)[0]);run('nginx','-t');run('systemctl','reload','nginx')
    state(config,'rolled-back',preserved='logs, credentials, runtime, certificate, inactive releases')
    print('PASS rollback; prior OTO restored and monitor stopped. Private records preserved.')
def main():
    parser=argparse.ArgumentParser();parser.add_argument('action',choices=['verify','activate','rollback','status']);parser.add_argument('--package-sha256',required=True);args=parser.parse_args()
    config,helper,web=load(args.package_sha256)
    if args.action=='verify':print('PASS pinned monitor + web payload; no changes.');return
    require(os.geteuid()==0,'Interactive operator sudo required.')
    if args.action=='status':verify_private();print(STATE.read_text());return
    import fcntl
    fd=os.open('/run/lock/oto-web-release.lock',os.O_CREAT|os.O_RDWR|os.O_NOFOLLOW,0o600)
    with os.fdopen(fd,'w') as lock:
        fcntl.flock(lock,fcntl.LOCK_EX|fcntl.LOCK_NB)
        if args.action=='activate':activate(config,helper,web)
        else:rollback(config,helper,web)

if __name__=='__main__':
    try:main()
    except (RuntimeError,OSError,subprocess.SubprocessError) as e:raise SystemExit(str(e)) from None
