"""Real HTTP tests for the private monitor. Uses only synthetic metadata and stdlib."""
import base64
import hashlib
import html
import http.client
import json
import os
from pathlib import Path
import re
import secrets
import socket
import subprocess
import tempfile
import time
import unittest
import uuid
from datetime import datetime, timezone, timedelta
from urllib.parse import urlencode

REPO = Path(__file__).resolve().parents[3]
DLL = Path(os.environ.get('OTO_MONITOR_DLL', REPO/'src/Hosts/Pertamina.Oto.Monitor/bin/Release/net10.0/Pertamina.Oto.Monitor.dll'))
ORIGIN = 'https://monitor.ayani.id:9443'

def event(name='succeeded', stage='process', code='NONE', feature='merge', process=None, age=0):
    now=datetime.now(timezone.utc)-timedelta(days=age)
    return dict(receivedAtUtc=now.isoformat(),source='anonymous-client',serverVersion='web-test-1',data=dict(
        id=str(uuid.uuid4()),processId=process or str(uuid.uuid4()),event=name,feature=feature,stage=stage,code=code,
        elapsedMs=42,occurredAt=int(now.timestamp()*1000),clientVersion='web-audit-1',browser='chromium'))

class Host:
    def __init__(self):
        self.tmp=tempfile.TemporaryDirectory(prefix='oto-monitor-test-');self.root=Path(self.tmp.name)
        self.data=self.root/'data';self.data.mkdir();self.key=self.root/'keys';self.key.mkdir()
        self.password=secrets.token_urlsafe(24);salt=secrets.token_bytes(32)
        self.credentials=self.root/'admin.json'
        self.credentials.write_text(json.dumps(dict(username='pengelola',salt=base64.b64encode(salt).decode(),hash=base64.b64encode(hashlib.pbkdf2_hmac('sha256',self.password.encode(),salt,600000)).decode(),iterations=600000)))
        self.credentials.chmod(0o600)
        with socket.socket() as sock:sock.bind(('127.0.0.1',0));self.port=sock.getsockname()[1]
        self.cookies={};self.log=(self.root/'host.log').open('wb');self.start()
    def start(self):
        env=dict(os.environ,ASPNETCORE_ENVIRONMENT='Production',Monitor__Origin=ORIGIN,Monitor__Port=str(self.port),Monitor__CredentialFile=str(self.credentials),Monitor__DataDirectory=str(self.data),Monitor__KeyDirectory=str(self.key))
        self.process=subprocess.Popen([os.environ.get('OTO_DOTNET','dotnet'),str(DLL)],cwd=DLL.parent,env=env,stdout=self.log,stderr=self.log,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
        for _ in range(80):
            if self.process.poll() is not None:raise RuntimeError('Monitor failed to start')
            try:
                if self.req('/login')[0]==200:return
            except OSError:pass
            time.sleep(.1)
        raise RuntimeError('Monitor startup timeout')
    def req(self,path='/',method='GET',body=None,headers=None,cookies=True):
        h={'Host':'monitor.ayani.id:9443','X-Forwarded-Proto':'https'}
        if cookies and self.cookies:h['Cookie']='; '.join(k+'='+v for k,v in self.cookies.items())
        if method=='POST':h.update({'Origin':ORIGIN,'Content-Type':'application/x-www-form-urlencoded'})
        h.update(headers or {})
        conn=http.client.HTTPConnection('127.0.0.1',self.port,timeout=8)
        conn.request(method,path,body,h);r=conn.getresponse();data=r.read();hs=r.getheaders();status=r.status;conn.close()
        for name,value in hs:
            if name.lower()=='set-cookie':
                k,v=value.split(';',1)[0].split('=',1);self.cookies[k]=v
        return status,dict((k.lower(),v) for k,v in hs),data
    def token(self,path='/login'):
        status,headers,body=self.req(path);assert status==200
        return html.unescape(re.search(rb'name="csrf" value="([^"]+)"',body).group(1).decode())
    def login(self,password=None):
        return self.req('/login','POST',urlencode(dict(username='pengelola',password=password or self.password,csrf=self.token())))
    def write(self,rows):
        (self.data/'events.jsonl').write_text(''.join(json.dumps(row)+'\n' for row in rows),encoding='utf-8')
    def stop(self):
        self.process.terminate();self.process.wait(timeout=15)
    def close(self):
        self.stop();self.log.close();self.tmp.cleanup()

class MonitorTests(unittest.TestCase):
    def setUp(self):self.host=Host();self.addCleanup(self.host.close)
    def test_01_authentication_boundary(self):
        h=self.host
        self.assertEqual(h.req('/')[0],302)
        self.assertEqual(h.req('/api/overview')[0],401)
        self.assertEqual(h.req('/health')[0],302)
        self.assertEqual(h.req('/Pages/dashboard.html')[0],404)
        self.assertEqual(h.req('/admin.json')[0],404)
        self.assertEqual(h.req('/events.jsonl')[0],404)
        self.assertEqual(h.req('/login',headers={'Host':'oto.ayani.id'})[0],404)
        self.assertEqual(h.req('/login',headers={'X-Forwarded-Proto':'http'})[0],404)
    def test_02_login_cookie_headers_and_logout_revocation(self):
        h=self.host;status,headers,_=h.login();self.assertEqual(status,302)
        cookie=headers['set-cookie'].lower()
        for value in ['__host-oto-monitor=','secure','httponly','samesite=strict','path=/']:self.assertIn(value,cookie)
        self.assertNotIn('domain=',cookie)
        self.assertEqual(h.req('/api/overview')[0],200)
        token=h.token('/');old=h.cookies.copy()
        status,headers,_=h.req('/logout','POST',urlencode({'csrf':token}));self.assertEqual(status,302)
        h.cookies=old;self.assertEqual(h.req('/api/overview')[0],401)
        self.assertIn("frame-ancestors 'none'",headers['content-security-policy'])
        self.assertIn('no-store',headers['cache-control'])
    def test_03_csrf_cross_origin_and_forged_cookie(self):
        h=self.host
        self.assertEqual(h.req('/login','POST',urlencode({'username':'pengelola','password':h.password}))[0],400)
        token=h.token()
        self.assertEqual(h.req('/login','POST',urlencode({'username':'pengelola','password':h.password,'csrf':token}),headers={'Origin':'https://oto.ayani.id'})[0],403)
        self.assertEqual(h.req('/login',headers={'Sec-Fetch-Site':'same-site'})[0],403)
        self.assertEqual(h.req('/login',headers={'Sec-Fetch-Site':'cross-site'})[0],403)
        h.cookies['__Host-oto-monitor']='forged';self.assertEqual(h.req('/api/overview')[0],401)
        h.login();self.assertEqual(h.req('/logout','POST','csrf=bad')[0],400)
        self.assertEqual(h.req('/api/overview')[0],200)
    def test_04_metadata_privacy_and_historical_filters(self):
        h=self.host;process=str(uuid.uuid4())
        rows=[event('started',process=process),event(process=process),event('failed',code='INVALID'),event(age=4)]
        bad=event();bad['data']['filename']='PRIVATE_PDF_FILENAME.pdf';rows+=[bad,rows[0]]
        bad2=event();bad2['data']['code']='PRIVATE_DOCUMENT_CONTENT';rows+=[bad2]
        h.write(rows);h.login();status,_,body=h.req('/api/overview?days=0');self.assertEqual(status,200)
        data=json.loads(body);self.assertEqual(data['events'],4);self.assertEqual(data['errorEvents'],1);self.assertEqual(data['successfulProcesses'],2)
        self.assertEqual(data['startedWithoutTerminal'],0);self.assertEqual(data['rejectedRows'],2);self.assertEqual(data['duplicateRows'],1)
        self.assertNotIn(b'PRIVATE_',body)
        self.assertEqual(json.loads(h.req('/api/overview?days=1')[2])['events'],3)
        self.assertEqual(json.loads(h.req('/api/overview?process='+process)[2])['events'],2)
        self.assertEqual(json.loads(h.req('/api/overview?outcome=errors')[2])['events'],1)
        self.assertEqual(json.loads(h.req('/api/overview?feature=rotate')[2])['events'],0)
        self.assertEqual(h.req('/api/overview?path=/etc/passwd')[0],400)
        self.assertEqual(h.req('/api/overview?feature=private-text')[0],400)
    def test_05_refresh_pagination_empty_and_capacity(self):
        h=self.host;h.login();self.assertEqual(json.loads(h.req('/api/overview')[2])['state'],'waiting')
        rows=[event() for _ in range(51)];h.write(rows)
        data=json.loads(h.req('/api/overview')[2]);self.assertEqual(len(data['recent']),50);self.assertEqual(data['pages'],2)
        self.assertEqual(len(json.loads(h.req('/api/overview?page=2')[2])['recent']),1)
        h.write(rows+[event('failed',code='TIMEOUT')]);self.assertEqual(json.loads(h.req('/api/overview')[2])['errorEvents'],1)
        with (h.data/'events.jsonl').open('wb') as f:f.truncate(16*1024*1024+1)
        self.assertEqual(h.req('/api/overview')[0],503)
    def test_06_lockout_and_restart_revokes_sessions(self):
        h=self.host;h.login();old=h.cookies.copy();h.stop();h.start();h.cookies=old
        self.assertEqual(h.req('/api/overview')[0],401)
        for _ in range(5):self.assertEqual(h.login('incorrect')[0],401)
        self.assertEqual(h.login()[0],429)
        self.assertEqual(h.req('/api/overview')[0],401)
    def test_07_no_mutating_or_public_read_surfaces(self):
        h=self.host;h.login()
        for route in ['/api/events','/api/export','/api/users','/register','/delete','/wwwroot/monitor.js']:
            self.assertEqual(h.req(route)[0],404)
        self.assertEqual(h.req('/api/overview','POST','')[0],405)
        self.assertEqual(h.req('/assets/monitor.js')[0],200)
        status,headers,body=h.req('/');self.assertEqual(status,200)
        self.assertNotIn(b'oto.ayani.id',body);self.assertNotIn('access-control-allow-origin',headers)
        self.assertNotIn(b'{{csrf}}',body)
    def test_08_partial_tail_is_never_rendered(self):
        h=self.host;h.write([event()])
        with (h.data/'events.jsonl').open('a') as f:f.write('{"filename":"PRIVATE_TAIL.pdf"')
        h.login();data=json.loads(h.req('/api/overview')[2]);self.assertEqual(data['events'],1);self.assertEqual(data['rejectedRows'],1)
        self.assertNotIn('PRIVATE_',json.dumps(data))

if __name__=='__main__':unittest.main(verbosity=2)
