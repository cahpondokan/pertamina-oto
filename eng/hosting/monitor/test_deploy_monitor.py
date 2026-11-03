import importlib.util
import hashlib
from pathlib import Path
import tempfile
import unittest

spec=importlib.util.spec_from_file_location('deploy_monitor',Path(__file__).with_name('deploy_monitor.py'))
deployment=importlib.util.module_from_spec(spec);spec.loader.exec_module(deployment)

class PackageBoundaryTests(unittest.TestCase):
    def setUp(self):
        self.tmp=tempfile.TemporaryDirectory();self.addCleanup(self.tmp.cleanup);self.root=Path(self.tmp.name)
        self.file=self.root/'sample.txt';self.file.write_text('synthetic')
        self.entry={'path':'sample.txt','size':9,'sha256':hashlib.sha256(b'synthetic').hexdigest()}
    def test_inventory_accepts_exact_payload(self):deployment.inventory(self.root,[self.entry])
    def test_inventory_rejects_added_or_changed_file(self):
        (self.root/'extra.txt').write_text('extra')
        with self.assertRaises(RuntimeError):deployment.inventory(self.root,[self.entry])
        (self.root/'extra.txt').unlink();self.file.write_text('different')
        with self.assertRaises(RuntimeError):deployment.inventory(self.root,[self.entry])
    def test_inventory_rejects_path_traversal_and_duplicate(self):
        for name in ['../sample.txt','/sample.txt','a//b','a/./b','C:sample.txt','a\\b']:
            with self.subTest(name=name),self.assertRaises(RuntimeError):deployment.inventory(self.root,[dict(self.entry,path=name)])
        with self.assertRaises(RuntimeError):deployment.inventory(self.root,[self.entry,self.entry])
    @unittest.skipIf(__import__('os').name=='nt','Linux symlink boundary verified on VPS')
    def test_inventory_rejects_symlink(self):
        (self.root/'link').symlink_to(self.file)
        with self.assertRaises(RuntimeError):deployment.inventory(self.root,[self.entry])
    def test_public_nginx_has_no_dashboard_proxy(self):
        text=Path(__file__).with_name('monitor.ayani.id.conf').read_text()
        public,private=text.split('# This listener is reachable only',1)
        self.assertNotIn('proxy_pass',public)
        self.assertEqual(public.count('return 404;'),2)
        self.assertIn('listen 127.0.0.1:5443 ssl;',private)
        self.assertIn('proxy_pass http://127.0.0.1:5091;',private)
        self.assertNotIn('listen 5443',private)
    def test_service_reads_only_diagnostics_with_separate_identity(self):
        text=Path(__file__).with_name('oto-monitor.service').read_text()
        self.assertIn('User=oto-monitor\n',text)
        self.assertIn('ReadOnlyPaths=/var/lib/oto-diagnostics\n',text)
        self.assertIn('UMask=0077\n',text)
        self.assertNotIn('ReadWritePaths=/var/lib/oto-diagnostics',text)

if __name__=='__main__':unittest.main(verbosity=2)
