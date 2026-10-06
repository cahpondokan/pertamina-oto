import importlib.util
import json
import os
import pathlib
import tempfile
import types
import unittest
from unittest.mock import patch, MagicMock

# Portable unit harness; actual flock remains Linux-only in deployment.
if os.name == 'nt':
    import sys
    sys.modules.setdefault('fcntl', types.SimpleNamespace())
    sys.modules.setdefault('grp', types.SimpleNamespace())
spec = importlib.util.spec_from_file_location('update_existing', pathlib.Path(__file__).with_name('update_existing.py'))
deploy = importlib.util.module_from_spec(spec)
spec.loader.exec_module(deploy)


class UpdateChecks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = pathlib.Path(self.temp.name)
        self.version = '20261002-160000-aaaaaaaa'
        self.zip = 'PERTAMINA-OTO-Windows-x64-' + self.version + '.zip'
        self.exe = 'PERTAMINA-OTO-Setup-x64-' + self.version + '.exe'
        for name in ['web/Pertamina.Oto.Downloads.dll', 'web/wwwroot/guide/guide.css', 'releases/' + self.zip, 'releases/' + self.exe]:
            path = self.root / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text('INERT FIXTURE')
        self.catalog = dict(schema=1, version=self.version, channel='development', fileName=self.zip,
                            size=13, sha256=deploy.digest(self.root / 'releases' / self.zip),
                            installerFileName=self.exe, installerSize=13,
                            installerSha256=deploy.digest(self.root / 'releases' / self.exe))
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.config = dict(id='oto-update-' + self.version, version=self.version, files=[])
        self.pin = self.save()

    def tearDown(self):
        self.temp.cleanup()

    def save(self):
        self.config['files'] = [dict(path=p.relative_to(self.root).as_posix(), size=p.stat().st_size,
                                     sha256=deploy.digest(p)) for folder in ('web', 'releases')
                                for p in (self.root / folder).rglob('*') if p.is_file()]
        (self.root / 'publication.json').write_text(json.dumps(self.config))
        return deploy.digest(self.root / 'publication.json')

    def reject(self):
        with self.assertRaises(RuntimeError):
            deploy.inventory(self.root, self.pin)

    def test_exact_fixture_inventory(self):
        config, catalog = deploy.inventory(self.root, self.pin)
        self.assertEqual(config['version'], self.version)
        self.assertEqual(catalog['fileName'], self.zip)

    def test_metadata_pin(self):
        (self.root / 'publication.json').write_text('{}')
        self.reject()

    def test_payload_corruption(self):
        (self.root / 'releases' / self.exe).write_text('CORRUPTION')
        self.reject()

    def test_unlisted_file(self):
        (self.root / 'web/unlisted.txt').write_text('extra')
        self.reject()

    def test_missing_file(self):
        (self.root / 'web/Pertamina.Oto.Downloads.dll').unlink()
        self.reject()

    def test_duplicate_inventory(self):
        self.config['files'].append(self.config['files'][0])
        (self.root / 'publication.json').write_text(json.dumps(self.config))
        self.pin = deploy.digest(self.root / 'publication.json')
        self.reject()

    def test_traversal_inventory(self):
        self.config['files'][0]['path'] = 'web/../../outside'
        (self.root / 'publication.json').write_text(json.dumps(self.config))
        self.pin = deploy.digest(self.root / 'publication.json')
        self.reject()

    def test_catalog_version_mismatch(self):
        self.catalog['version'] = '20261002-170000-bbbbbbbb'
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.pin = self.save()
        self.reject()

    def test_catalog_hash_mismatch(self):
        self.catalog['installerSha256'] = '0' * 64
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.pin = self.save()
        self.reject()

    def test_catalog_absolute_path(self):
        self.catalog['fileName'] = '/etc/passwd'
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.pin = self.save()
        self.reject()

    def test_invalid_release_id(self):
        self.config['id'] = '../danger'
        self.pin = self.save()
        self.reject()

    def test_missing_guide(self):
        (self.root / 'web/wwwroot/guide/guide.css').unlink()
        self.pin = self.save()
        self.reject()

    def test_activation_denied_before_mutation(self):
        with patch.object(deploy, 'baseline', side_effect=RuntimeError('baseline drift')):
            with self.assertRaisesRegex(RuntimeError, 'baseline drift'):
                deploy.activate(self.root, self.config, self.catalog)
        self.assertEqual((self.root / 'releases' / self.zip).read_text(), 'INERT FIXTURE')

    def test_atomic_write_preserves_old_on_replace_failure(self):
        pointer = self.root / 'pointer.json'
        pointer.write_bytes(b'old')
        with patch.object(deploy.os, 'chown', create=True), patch.object(deploy.os, 'replace', side_effect=OSError('fixture')):
            with self.assertRaises(OSError):
                deploy.atomic(pointer, b'new', 0)
        self.assertEqual(pointer.read_bytes(), b'old')
        self.assertFalse(list(self.root.glob('.oto-pending-*')))

    def activation_fixture(self, fail=False, collision=False):
        base = self.root / 'target-base'
        data = self.root / 'target-releases'
        backups = self.root / 'backups'
        base.mkdir()
        data.mkdir()
        old = b'{"version":"old"}'
        (data / 'current.json').write_bytes(old)
        config = dict(self.config, previousWeb='/previous/web')
        if collision:
            (data / self.zip).write_text('DIFFERENT EXISTING PAYLOAD')
        links = []
        response = MagicMock()
        response.__enter__.return_value.status = 200
        response.__enter__.return_value.read.return_value = b'<section id="update">Guide</section>'
        def restart_check():
            # First web readiness happens before latest changes; rollback also restores it.
            self.assertEqual((data / 'current.json').read_bytes(), old)
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), \
                patch.object(deploy, 'BACKUPS', backups), patch.object(deploy, 'baseline'), \
                patch.object(deploy.grp, 'getgrnam', return_value=types.SimpleNamespace(gr_gid=0), create=True), \
                patch.object(deploy.os, 'chown', create=True), patch.object(deploy, 'link', side_effect=links.append), \
                patch.object(deploy, 'restart', side_effect=restart_check), \
                patch.object(deploy.urllib.request, 'urlopen', return_value=response), \
                patch.object(deploy, 'verify_http', side_effect=RuntimeError('HTTP fixture failure') if fail else None):
            if fail or collision:
                with self.assertRaises(RuntimeError):
                    deploy.activate(self.root, config, self.catalog)
                self.assertEqual((data / 'current.json').read_bytes(), old)
                if fail:
                    self.assertEqual(links[-1], pathlib.Path('/previous/web'))
            else:
                deploy.activate(self.root, config, self.catalog)
                self.assertEqual(json.loads((data / 'current.json').read_text())['version'], self.version)
                self.assertTrue((backups / config['id'] / 'SUCCESS').is_file())
            self.assertEqual((backups / config['id'] / 'current.json').read_bytes(), old)
        self.assertTrue((data / self.zip).exists())

    def test_feed_switched_after_website_ready(self):
        self.activation_fixture()

    def test_http_failure_restores_feed_and_web(self):
        self.activation_fixture(fail=True)

    def test_immutable_collision_preserves_existing_feed(self):
        self.activation_fixture(collision=True)

    def test_public_verification_rejects_redirect(self):
        handler = deploy.NoRedirect()
        with self.assertRaisesRegex(RuntimeError, 'Redirect rejected'):
            handler.redirect_request(None, None, 302, '', {}, 'https://other.example/file')

    def test_public_feed_rejects_changed_filename_with_matching_hash(self):
        health = MagicMock()
        health.__enter__.return_value.status = 204
        feed = MagicMock()
        feed.__enter__.return_value.read.return_value = json.dumps(
            dict(self.catalog, fileName='different.zip')).encode()
        opener = MagicMock()
        opener.open.side_effect = [health, feed]
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            with self.assertRaisesRegex(RuntimeError, 'HTTP feed mismatch'):
                deploy.verify_http('https://oto.ayani.id', self.catalog, guide=False)
        self.assertEqual(opener.open.call_count, 2)

    def test_rollback_rejects_same_version_catalog_drift_before_mutation(self):
        base = self.root / 'rollback-base'
        data = self.root / 'rollback-data'
        backups = self.root / 'rollback-backups'
        base.mkdir()
        data.mkdir()
        backup = backups / self.config['id']
        backup.mkdir(parents=True)
        (backup / 'SUCCESS').write_text('fixture')
        (backup / 'publication.json').write_text(json.dumps(self.config))
        (data / 'current.json').write_text(json.dumps(dict(self.catalog, notes=['later edit'])))
        before = (data / 'current.json').read_bytes()
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), \
                patch.object(deploy, 'BACKUPS', backups), \
                patch.object(deploy.os, 'geteuid', return_value=0, create=True), \
                patch.object(deploy.os, 'readlink', return_value=str(base / self.config['id'] / 'web')), \
                patch.object(deploy, 'atomic') as atomic, patch.object(deploy, 'restart') as restart:
            with self.assertRaisesRegex(RuntimeError, 'Feed drift detected'):
                deploy.rollback(self.config)
            atomic.assert_not_called()
            restart.assert_not_called()
        self.assertEqual((data / 'current.json').read_bytes(), before)


if __name__ == '__main__':
    unittest.main(verbosity=2)
