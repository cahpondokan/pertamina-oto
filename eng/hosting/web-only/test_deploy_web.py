import copy
import hashlib
import json
from pathlib import Path
import tempfile
import unittest
import types
import sys
from unittest.mock import patch
import deploy_web as d


class DeploymentTests(unittest.TestCase):
    def test_diagnostics_policy_requires_explicit_release_flag_and_exact_same_origin(self):
        marker = b'<meta name="oto-diagnostics" content="/api/diagnostics/events"/>'
        for enabled in (False, True):
            expected = "'self'" if enabled else "'none'"
            headers = {'Content-Security-Policy': "worker-src 'self'; connect-src " + expected + ';'}
            d.verify_browser_policy(headers, marker if enabled else b'', enabled)
            with self.assertRaises(RuntimeError): d.verify_browser_policy(headers, marker if enabled else b'', not enabled)
        for policy in ("connect-src *; worker-src 'self'", "connect-src 'self' https://other.example; worker-src 'self'", "connect-src 'self'; connect-src *; worker-src 'self'"):
            with self.assertRaises(RuntimeError): d.verify_browser_policy({'Content-Security-Policy':policy},marker,True)

    def test_rendered_catalog_checks_all_tools_and_ignores_inert_templates(self):
        main = '<main>' + ''.join('<a class="tool-card-content" href="/alat/' + tool + '">Tool</a>' for tool in d.TOOLS) + '<article class="tool-card planned"></article>' * 8 + '</main>'
        d.verify_catalog_html((main + '<template>' + main + '</template>').encode())
        for invalid in (main.replace('/alat/crop', '/alat/unavailable'), main.replace('tool-card-content', 'missing', 1), main.replace('tool-card planned', 'active', 1)):
            with self.subTest(invalid=invalid), self.assertRaises(RuntimeError):
                d.verify_catalog_html(invalid.encode())

    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)
        self.web = self.root / 'source/web'
        self.web.mkdir(parents=True)
        for name, data in {
            'Pertamina.Oto.Downloads.dll': b'synthetic dll',
            'wwwroot/web-release.json': b'{"id":"oto-web-20261021-abcdef123456"}',
            'wwwroot/js/pdf-worker.js': b'synthetic worker'
        }.items():
            file = self.web / name
            file.parent.mkdir(parents=True, exist_ok=True)
            file.write_bytes(data)
        self.files = [{'path': p.relative_to(self.web).as_posix(), 'sha256': d.digest(p), 'size': p.stat().st_size} for p in self.web.rglob('*') if p.is_file()]
        self.base = self.root / 'opt'
        self.base.mkdir()
        self.old = self.base / 'old/web'
        self.old.mkdir(parents=True)
        self.data = self.root / 'data'
        self.data.mkdir()
        for name in ('current.json', 'distribution-v2.json'):
            (self.data / name).write_bytes(b'preserved desktop metadata')
        self.unit = self.root / 'unit'; self.unit.write_bytes(b'unit')
        self.nginx = self.root / 'nginx'; self.nginx.write_bytes(b'nginx')
        self.config = {'id': 'oto-web-20261021-abcdef123456', 'previousWeb': str(self.old), 'files': self.files,
                       'unitSha256': d.digest(self.unit), 'nginxSha256': d.digest(self.nginx),
                       'catalogPins': {p.name: d.digest(p) for p in self.data.iterdir()}}
        self.publication = self.web.parent / 'publication.json'
        self.publication.write_text(json.dumps(self.config))
        self.pin = d.digest(self.publication)
        self.overrides = patch.multiple(d, BASE=self.base, DATA=self.data, UNIT=self.unit, NGINX=self.nginx)
        self.overrides.start()

    def tearDown(self):
        self.overrides.stop()
        self.temp.cleanup()

    def test_valid_inventory(self):
        d.inventory(self.web, self.files)

    def test_changed_bytes_rejected(self):
        (self.web / 'Pertamina.Oto.Downloads.dll').write_bytes(b'changed')
        with self.assertRaises(RuntimeError): d.inventory(self.web, self.files)

    def test_extra_file_rejected(self):
        (self.web / 'secret.txt').write_bytes(b'synthetic')
        with self.assertRaises(RuntimeError): d.inventory(self.web, self.files)

    def test_path_escape_rejected(self):
        for name in ('../outside', '/absolute', 'a\\b', 'C:bad', 'a//b'):
            invalid = copy.deepcopy(self.files); invalid[0]['path'] = name
            with self.subTest(name=name), self.assertRaises(RuntimeError): d.inventory(self.web, invalid)

    def test_duplicate_rejected(self):
        with self.assertRaises(RuntimeError): d.inventory(self.web, self.files + [self.files[0]])

    def test_baseline_drift_rejected(self):
        (self.data / 'current.json').write_bytes(b'new deployment')
        with self.assertRaises(RuntimeError): d.unchanged(self.config)

    def test_unit_drift_rejected(self):
        self.unit.write_bytes(b'changed')
        with self.assertRaises(RuntimeError): d.unchanged(self.config)

    def test_wrong_publication_pin_rejected(self):
        with self.assertRaises(RuntimeError): d.load(self.web.parent, '0' * 64)

    def virtual_activation(self, fail):
        current = [str(self.old)]
        def fake_pointer(expected): d.require(current[0] == expected, 'Pointer drift')
        def fake_switch(target): current[0] = str(target)
        group = types.SimpleNamespace(getgrnam=lambda name: types.SimpleNamespace(gr_gid=1000))
        before = {p.name: p.read_bytes() for p in self.data.iterdir()}
        with patch.dict(sys.modules, {'grp': group}), patch.object(d.os, 'chown', create=True), patch.object(d.subprocess, 'run'), patch.object(d, 'restart'), patch.object(d, 'ready'), patch.object(d, 'pointer', fake_pointer), patch.object(d, 'switch', fake_switch), patch.object(d, 'verify_http', side_effect=RuntimeError('synthetic HTTP failure') if fail else None):
            if fail:
                with self.assertRaises(RuntimeError): d.activate(self.web.parent, self.config, self.pin)
            else:
                d.activate(self.web.parent, self.config, self.pin)
                self.assertEqual(current[0], str(self.base / self.config['id'] / 'web'))
                d.rollback(self.config, self.pin)
        self.assertEqual(current[0], str(self.old))
        self.assertEqual(before, {p.name: p.read_bytes() for p in self.data.iterdir()})

    def test_activation_protocol_preserves_desktop_catalog(self):
        self.virtual_activation(False)

    def test_failure_protocol_restores_previous_pointer(self):
        self.virtual_activation(True)

    @unittest.skipIf(__import__('os').name == 'nt', 'Linux symbolic-link integration')
    def test_payload_symlink_rejected(self):
        target = self.web / 'linked'; target.symlink_to(self.unit)
        with self.assertRaises(RuntimeError): d.inventory(self.web, self.files)

    @unittest.skipIf(__import__('os').name == 'nt', 'Linux activation integration')
    def test_success_and_guarded_rollback_preserve_catalog(self):
        (self.base / 'current').symlink_to(self.old)
        before = {p.name: p.read_bytes() for p in self.data.iterdir()}
        with patch.object(d.os, 'chown'), patch('grp.getgrnam') as group, patch.object(d.subprocess, 'run'), patch.object(d, 'restart'), patch.object(d, 'verify_http'), patch.object(d, 'ready'):
            group.return_value.gr_gid = 1000
            d.activate(self.web.parent, self.config, self.pin)
            d.pointer(str(self.base / self.config['id'] / 'web'))
            d.rollback(self.config, self.pin)
            d.pointer(str(self.old))
        self.assertEqual(before, {p.name: p.read_bytes() for p in self.data.iterdir()})

    @unittest.skipIf(__import__('os').name == 'nt', 'Linux activation integration')
    def test_http_failure_rolls_back_and_preserves_catalog(self):
        (self.base / 'current').symlink_to(self.old)
        before = {p.name: p.read_bytes() for p in self.data.iterdir()}
        with patch.object(d.os, 'chown'), patch('grp.getgrnam') as group, patch.object(d.subprocess, 'run'), patch.object(d, 'restart'), patch.object(d, 'verify_http', side_effect=RuntimeError('synthetic failure')), patch.object(d, 'ready'):
            group.return_value.gr_gid = 1000
            with self.assertRaises(RuntimeError): d.activate(self.web.parent, self.config, self.pin)
        d.pointer(str(self.old))
        self.assertEqual(before, {p.name: p.read_bytes() for p in self.data.iterdir()})
        self.assertEqual(json.loads((self.base / self.config['id'] / 'state.json').read_text())['status'], 'failed-rolled-back')

    @unittest.skipIf(__import__('os').name == 'nt', 'Linux activation integration')
    def test_pointer_drift_does_not_deploy(self):
        (self.base / 'current').symlink_to(self.base / 'other')
        with self.assertRaises(RuntimeError): d.activate(self.web.parent, self.config, self.pin)
        self.assertFalse((self.base / self.config['id']).exists())


if __name__ == '__main__':
    unittest.main(verbosity=2)
