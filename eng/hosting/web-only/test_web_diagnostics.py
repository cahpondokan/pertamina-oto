"""Real HTTP/storage checks, using only synthetic metadata and canaries."""
import copy
import importlib.util
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import time
import unittest
import urllib.error
import urllib.request
import uuid

REPO=Path(__file__).resolve().parents[3]
EVIDENCE=Path(os.environ['OTO_DIAGNOSTICS_EVIDENCE']).resolve()
EVIDENCE.mkdir(parents=True,exist_ok=True)
spec=importlib.util.spec_from_file_location('report',Path(__file__).with_name('diagnostics_report.py'))
report=importlib.util.module_from_spec(spec);spec.loader.exec_module(report)

def event(**changes):
    value=dict(id=str(uuid.uuid4()),processId=str(uuid.uuid4()),event='started',feature='merge',stage='process',code='NONE',elapsedMs=0,occurredAt=int(time.time()*1000),clientVersion='web-audit-1',browser='chromium')
    value.update(changes);return value

class Host:
    def __init__(self,name='Pertamina.Oto.Web.Preview',enabled=True,directory=None):
        self.name=name;self.project=REPO/'src/Hosts'/name
        self.data=directory or Path(tempfile.mkdtemp(prefix='store-',dir=EVIDENCE))
        with socket.socket() as sock: sock.bind(('127.0.0.1',0));self.port=sock.getsockname()[1]
        self.origin=f'http://127.0.0.1:{self.port}'
        self.log=(EVIDENCE/f'host-{self.port}.log').open('w',encoding='utf-8')
        self.command=['dotnet',str(self.project/f'bin/Debug/net10.0/{name}.dll'),'--urls',self.origin,'--Preview:CatalogOnly=true','--WebDiagnostics:Enabled='+str(enabled).lower(),'--WebDiagnostics:Directory='+str(self.data)]
        try:self.start()
        except Exception:self.close();raise
    def start(self):
        # Downloads links shared content into its output; run from that real host root.
        workdir=self.project/'bin/Debug/net10.0' if self.name=='Pertamina.Oto.Downloads' else self.project
        self.process=subprocess.Popen(self.command,cwd=workdir,stdout=self.log,stderr=subprocess.STDOUT,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
        for _ in range(100):
            if self.process.poll() is not None: raise RuntimeError('Test server exited; inspect bounded host log.')
            try:
                if self.request('/')[0]==200:return
            except OSError:time.sleep(.1)
        raise RuntimeError('Test server did not become ready.')
    def stop(self):
        if self.process.poll() is None:
            self.process.terminate()
            try:self.process.wait(timeout=5)
            except subprocess.TimeoutExpired:self.process.kill();self.process.wait(timeout=5)
    def close(self):self.stop();self.log.close()
    def request(self,path='/api/diagnostics/events',payload=None,method=None,headers=None):
        body=json.dumps(payload).encode() if isinstance(payload,dict) else payload
        h={'Origin':self.origin,'Content-Type':'application/json','X-Oto-Diagnostics':'1','Sec-Fetch-Site':'same-origin'}
        h.update(headers or {})
        for key in [k for k,v in h.items() if v is None]:del h[key]
        req=urllib.request.Request(self.origin+path,data=body,method=method or ('POST' if body is not None else 'GET'),headers=h)
        try:
            with urllib.request.urlopen(req,timeout=5) as r:return r.status,dict(r.headers),r.read()
        except urllib.error.HTTPError as e:
            with e:return e.code,dict(e.headers),e.read()
    def post(self,*events,**kw):return self.request(payload={'version':1,'events':list(events)},**kw)
    def rows(self):
        p=self.data/'events.jsonl'
        return [json.loads(s) for s in p.read_text().splitlines()] if p.exists() else []

class DiagnosticsTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.hosts=[]
        try:
            for name in ['Pertamina.Oto.Web.Preview','Pertamina.Oto.Downloads']:cls.hosts.append(Host(name))
        except Exception:
            for host in cls.hosts:host.close()
            raise
    @classmethod
    def tearDownClass(cls):
        for host in cls.hosts:host.close()
    def test_01_private_store_dedupe_conflict_and_restart(self):
        for h in self.hosts:
            with self.subTest(host=h.name):
                e=event();self.assertEqual(h.post(e)[0],204);self.assertEqual(h.post(e,e)[0],204);self.assertEqual(len(h.rows()),1)
                bad=copy.deepcopy(e);bad['feature']='rotate';self.assertEqual(h.post(bad)[0],409);self.assertEqual(len(h.rows()),1)
                h.stop();h.start();self.assertEqual(h.post(e)[0],204);self.assertEqual(len(h.rows()),1)
                row=h.rows()[0];self.assertEqual(row['source'],'anonymous-client');self.assertEqual(set(row),{'receivedAtUtc','serverVersion','source','data'})
                self.assertNotIn('Set-Cookie',h.post(e)[1])
    def test_02_unknown_or_sensitive_fields_rejected_without_storage(self):
        for h in self.hosts:
            n=len(h.rows())
            for field in ['filename','path','document','bytes','text','stack','message','ip','url','token','userAgent']:
                with self.subTest(host=h.name,field=field):self.assertEqual(h.post(event(**{field:'SENSITIVE_CANARY'}))[0],400)
            for field in ['feature','stage','event','code','browser','clientVersion']:
                with self.subTest(host=h.name,field=field):self.assertEqual(h.post(event(**{field:'SENSITIVE_CANARY'}))[0],400)
            self.assertEqual(len(h.rows()),n);self.assertNotIn('SENSITIVE_CANARY',(h.data/'events.jsonl').read_text())
    def test_03_bad_envelopes_and_numbers(self):
        for h in self.hosts:
            for payload in [b'%PDF-SYNTHETIC_CANARY',b'{',b'{"version":1,"version":1,"events":[]}',{'version':1,'events':[]},{'version':1,'events':[event()]*17},{'version':2,'events':[event()]},{'version':1,'events':[event()],'extra':'SECRET'}]:
                self.assertEqual(h.request(payload=payload)[0],400)
            for change in [dict(elapsedMs=-1),dict(elapsedMs=1.5),dict(occurredAt=0),dict(id='not-a-uuid'),dict(event='failed',code='NONE'),dict(event='save_succeeded',stage='process')]:
                self.assertEqual(h.post(event(**change))[0],400)
    def test_04_method_origin_content_type_and_size_guards(self):
        for h in self.hosts:
            for headers in [{'Origin':'https://foreign.example'},{'Origin':None},{'X-Oto-Diagnostics':None},{'Sec-Fetch-Site':'cross-site'}]:self.assertEqual(h.post(event(),headers=headers)[0],403)
            self.assertEqual(h.post(event(),headers={'Content-Type':'multipart/form-data'})[0],415)
            self.assertEqual(h.post(event(),headers={'Content-Encoding':'gzip'})[0],415)
            self.assertEqual(h.request(payload=b'x'*12289)[0],413)
            self.assertEqual(h.post(event(),path='/api/diagnostics/events?document=CANARY')[0],403)
            self.assertEqual(h.request('/alat/merge',payload=b'{}')[0],405)
            self.assertEqual(h.request(method='DELETE')[0],405)
    def test_05_no_public_read_routes_and_clear_privacy(self):
        for h in self.hosts:
            for path in ['/api/diagnostics/events','/events.jsonl','/diagnostics/events.jsonl','/api/diagnostics/report']:
                self.assertIn(h.request(path)[0],(404,405))
            status,headers,body=h.request('/');self.assertEqual(status,200);self.assertIn("connect-src 'self'",headers['Content-Security-Policy']);self.assertIn(b'web-diagnostics.js',body);self.assertIn(b'/privasi-web',body)
            self.assertEqual(h.request('/privasi-web')[0],200)
            for asset in ['/js/web-diagnostics.js','/js/pdf-merge-ui.js','/js/browser-pdf.js','/css/site.css']:
                status,_,body=h.request(asset);self.assertEqual(status,200);self.assertGreater(len(body),100)
    def test_06_disabled_preserves_no_connection_policy(self):
        h=Host(enabled=False)
        try:
            status,headers,body=h.request('/');self.assertEqual(status,200);self.assertIn("connect-src 'none'",headers['Content-Security-Policy']);self.assertNotIn(b'name="oto-diagnostics"',body);self.assertEqual(h.post(event())[0],404);self.assertFalse((h.data/'events.jsonl').exists())
        finally:h.close()
    def test_07_capacity_and_corrupt_tail_fail_without_affecting_pdf_page(self):
        for kind in ['capacity','tail','unwritable']:
            directory=Path(tempfile.mkdtemp(prefix=kind+'-',dir=EVIDENCE));p=directory/'events.jsonl'
            if kind=='capacity':
                with p.open('wb') as f:f.truncate(16*1024*1024+1)
            elif kind=='tail':p.write_text('{"unfinished":')
            else:p.mkdir()
            h=Host(directory=directory)
            try:
                self.assertEqual(h.post(event())[0],503);self.assertEqual(h.request('/alat/merge')[0],200)
                if kind=='tail':self.assertEqual(p.read_text(),'{"unfinished":')
            finally:h.close()
    def test_08_operator_report_validates_and_escapes_untrusted_data(self):
        h=self.hosts[0];e=event(event='failed',code='VERIFY');self.assertEqual(h.post(e)[0],204)
        r=report.report(h.data);self.assertTrue(r['errors']);self.assertEqual(r['errors'][0]['code'],'VERIFY');page=report.render(r);self.assertIn('Monitor OTO Web',page);self.assertNotIn('<script',page)
        (EVIDENCE/'monitor-example.html').write_text(page,encoding='utf-8');(EVIDENCE/'monitor-example.json').write_text(json.dumps(r,indent=2))
        malformed={'receivedAtUtc':'x','data':{'stack':'<script>CANARY</script>'}};self.assertFalse(report.valid(malformed))
    def test_09_rate_limit_is_bounded_and_recovers_pdf_page(self):
        h=Host()
        try:
            statuses=[h.post(event())[0] for _ in range(62)];self.assertEqual(statuses.count(204),60);self.assertEqual(statuses[-1],429);self.assertEqual(h.request('/alat/merge')[0],200)
        finally:h.close()

if __name__=='__main__':unittest.main(verbosity=2)
