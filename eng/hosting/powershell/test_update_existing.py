import importlib.util
import io
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

    def add_baseline(self):
        self.catalog.update(productVersion='0.3.0', runtimeBundled=True, manualInstallerOnly=True,
                            publishedAt='2026-10-11T08:00:00Z', notes=['Development fixture'])
        self.online = 'PERTAMINA-OTO-OnlineSetup-x64-' + self.version + '.exe'
        (self.root / 'releases' / self.online).write_bytes(b'THIN')
        self.new_baseline = dict(schema=2, product='PERTAMINA OTO', productVersion='0.3.0',
            version=self.version, channel='development', architecture='x64', runtimeBundled=True,
            updaterProtocol=2, minimumUpdaterBaseline=1, manifestSha256='a'*64, expandedBytes=100,
            publishedAt=self.catalog['publishedAt'], notes=self.catalog['notes'])
        for key, name in [('onlineInstaller', self.online), ('offlineInstaller', self.exe), ('updatePayload', self.zip)]:
            path = self.root / 'releases' / name
            self.new_baseline[key] = dict(fileName=name, size=path.stat().st_size, sha256=deploy.digest(path))
        self.save_baseline()

    def save_baseline(self):
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        (self.root / 'releases/distribution-v2.json').write_text(json.dumps(self.new_baseline))
        self.pin = self.save()

    def test_baseline_inventory_three_artifacts(self):
        self.add_baseline()
        config, catalog = deploy.inventory(self.root, self.pin)
        self.assertEqual(deploy.baseline_catalog(self.root, config, catalog), self.new_baseline)

    def test_unicode_catalogs_use_explicit_utf8_not_windows_locale(self):
        self.add_baseline()
        for value in (self.catalog, self.new_baseline):
            value['notes'] = ['Pengembangan — pilihan café ✓']
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog, ensure_ascii=False), encoding='utf-8')
        (self.root / 'releases/distribution-v2.json').write_text(json.dumps(self.new_baseline, ensure_ascii=False), encoding='utf-8')
        self.pin = self.save()
        deploy.inventory(self.root, self.pin)

    def test_baseline_rejects_invalid_protocol_identity_and_budget(self):
        self.add_baseline()
        original = dict(self.new_baseline)
        for field, value in [('updaterProtocol', True), ('minimumUpdaterBaseline', 2),
                             ('runtimeBundled', False), ('architecture', 'arm64'),
                             ('manifestSha256', 'bad'), ('expandedBytes', 0), ('productVersion', 'bad')]:
            with self.subTest(field=field):
                self.new_baseline = dict(original, **{field:value})
                self.save_baseline()
                self.reject()

    def test_baseline_rejects_projection_mismatch(self):
        self.add_baseline()
        self.catalog['manualInstallerOnly'] = False
        self.save_baseline()
        self.reject()

    def test_baseline_rejects_online_hash_filename_and_size(self):
        self.add_baseline()
        original = dict(self.new_baseline['onlineInstaller'])
        for field, value in [('fileName', '../escape.exe'), ('size', 5), ('sha256', '0'*64)]:
            with self.subTest(field=field):
                self.new_baseline['onlineInstaller'] = dict(original, **{field:value})
                self.save_baseline()
                self.reject()

    def test_baseline_notes_hash_invalid(self):
        self.add_baseline()
        for obj in (self.catalog, self.new_baseline):
            obj.update(releaseNotesJson='{}', releaseNotesSha256='0'*64)
        self.save_baseline()
        self.reject()

    def test_baseline_pointer_absence_and_drift(self):
        with patch.object(deploy, 'DATA', self.root):
            deploy.baseline_pointer({})
            pointer = self.root / 'distribution-v2.json'
            pointer.write_text('fixture')
            with self.assertRaisesRegex(RuntimeError, 'Unexpected baseline'):
                deploy.baseline_pointer({})
            config = dict(previousBaselineSha256=deploy.digest(pointer))
            deploy.baseline_pointer(config)
            pointer.write_text('changed')
            with self.assertRaisesRegex(RuntimeError, 'catalog drift'):
                deploy.baseline_pointer(config)

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

    def test_release_policy_requires_boolean(self):
        self.catalog['manualInstallerOnly'] = 'true'
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.pin = self.save()
        self.reject()

    def test_manual_installer_inventory(self):
        self.catalog.update(runtimeBundled=True, manualInstallerOnly=True)
        (self.root / 'releases/current.json').write_text(json.dumps(self.catalog))
        self.pin = self.save()
        _, catalog = deploy.inventory(self.root, self.pin)
        self.assertTrue(catalog['manualInstallerOnly'])

    def test_invalid_release_id(self):
        self.config['id'] = '../danger'
        self.pin = self.save()
        self.reject()

    def test_retry_inventory_requires_exact_predecessor_and_pin(self):
        original = self.config['id']
        self.config['id'] = original + '-r2'
        self.pin = self.save()
        self.reject()
        self.config.update(retryOf=original, retryMetadataSha256='a'*64)
        self.pin = self.save()
        deploy.inventory(self.root, self.pin)
        for key, value in [('retryOf', '../escape'), ('retryMetadataSha256', 'bad'), ('id', original+'-r3')]:
            good = self.config[key]
            self.config[key] = value
            self.pin = self.save()
            self.reject()
            self.config[key] = good

    def test_retry_preflight_requires_exact_rolled_back_bytes(self):
        import shutil
        base = self.root / 'server'
        data = self.root / 'data'
        backups = self.root / 'backups'
        prior = backups / self.config['id']
        prior.mkdir(parents=True)
        data.mkdir()
        self.config['previousWeb'] = '/previous/web'
        self.pin = self.save()
        (prior / 'publication.json').write_bytes((self.root / 'publication.json').read_bytes())
        (prior / 'current.json').write_bytes(b'original-catalog')
        (data / 'current.json').write_bytes(b'original-catalog')
        (prior / 'previous-web.txt').write_text('/previous/web')
        (prior / 'baseline-state.json').write_text('{"present":false}')
        marker = prior / 'FAILED-ROLLED-BACK'
        marker.write_text('failed')
        shutil.copytree(self.root / 'web', base / self.config['id'] / 'web')
        retry = dict(self.config, id=self.config['id']+'-r2', retryOf=self.config['id'], retryMetadataSha256=self.pin)
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), patch.object(deploy, 'BACKUPS', backups):
            deploy.retry_preflight(retry)
            (data / 'current.json').write_bytes(b'drift')
            with self.assertRaisesRegex(RuntimeError, 'catalog drift'):
                deploy.retry_preflight(retry)
            (data / 'current.json').write_bytes(b'original-catalog')
            with self.assertRaisesRegex(RuntimeError, 'cannot change'):
                deploy.retry_preflight(dict(retry, previousWeb='/other'))
            (prior / 'SUCCESS').write_text('success')
            with self.assertRaisesRegex(RuntimeError, 'Successful'):
                deploy.retry_preflight(retry)
            (prior / 'SUCCESS').unlink()
            (base / self.config['id'] / 'web/Pertamina.Oto.Downloads.dll').write_text('drift')
            with self.assertRaisesRegex(RuntimeError, 'website drift'):
                deploy.retry_preflight(retry)
            marker.unlink()
            with self.assertRaises(RuntimeError):
                deploy.retry_preflight(retry)

    def test_warm_guide_requires_cold_and_steady_success(self):
        opener = MagicMock()
        def respond(*args, **kwargs):
            response = MagicMock()
            response.__enter__.return_value.status = 200
            response.__enter__.return_value.read.return_value = b'<section id="update">OK</section>'
            return response
        opener.open.side_effect = respond
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            deploy.warm_guide('http://127.0.0.1:5088')
        self.assertEqual([c.kwargs['timeout'] for c in opener.open.call_args_list], [60, 15])

    def test_warm_guide_timeout_is_not_ignored_or_retried(self):
        opener = MagicMock()
        opener.open.side_effect = TimeoutError('cold start timeout')
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            with self.assertRaises(TimeoutError):
                deploy.warm_guide('http://127.0.0.1:5088')
        self.assertEqual(opener.open.call_count, 1)

    def test_warm_guide_rejects_missing_marker_and_oversize(self):
        for content in (b'not a guide', b'id="update"'+b'x'*524288):
            response = MagicMock()
            response.__enter__.return_value.status = 200
            response.__enter__.return_value.read.return_value = content
            opener = MagicMock()
            opener.open.return_value = response
            with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
                with self.assertRaises(RuntimeError):
                    deploy.warm_guide('http://127.0.0.1:5088')

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

    def activation_fixture(self, fail=False, collision=False, previous_baseline=None, readiness_fail=False):
        base = self.root / 'target-base'
        data = self.root / 'target-releases'
        backups = self.root / 'backups'
        base.mkdir()
        data.mkdir()
        old = b'{"version":"old"}'
        (data / 'current.json').write_bytes(old)
        if previous_baseline is not None:
            (data / 'distribution-v2.json').write_bytes(previous_baseline)
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
            pointer = data / 'distribution-v2.json'
            self.assertEqual(pointer.read_bytes() if pointer.exists() else None, previous_baseline)
        def http_check(*args, **kwargs):
            if hasattr(self, 'new_baseline'):
                self.assertEqual(json.loads((data / 'distribution-v2.json').read_text()), self.new_baseline)
                self.assertEqual(kwargs['baseline_release'], self.new_baseline)
            if fail:
                raise RuntimeError('HTTP fixture failure')
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), \
                patch.object(deploy, 'BACKUPS', backups), patch.object(deploy, 'baseline'), \
                patch.object(deploy.grp, 'getgrnam', return_value=types.SimpleNamespace(gr_gid=0), create=True), \
                patch.object(deploy.os, 'chown', create=True), patch.object(deploy, 'link', side_effect=links.append), \
                patch.object(deploy, 'restart', side_effect=restart_check), \
                patch.object(deploy, 'warm_guide', side_effect=TimeoutError('readiness timeout') if readiness_fail else None), \
                patch.object(deploy, 'verify_http', side_effect=http_check):
            if fail or collision or readiness_fail:
                with self.assertRaises((RuntimeError, TimeoutError)):
                    deploy.activate(self.root, config, self.catalog)
                self.assertEqual((data / 'current.json').read_bytes(), old)
                pointer = data / 'distribution-v2.json'
                self.assertEqual(pointer.read_bytes() if pointer.exists() else None, previous_baseline)
                if fail or readiness_fail:
                    self.assertEqual(links[-1], pathlib.Path('/previous/web'))
            else:
                deploy.activate(self.root, config, self.catalog)
                self.assertEqual(json.loads((data / 'current.json').read_text())['version'], self.version)
                self.assertTrue((backups / config['id'] / 'SUCCESS').is_file())
            self.assertEqual((backups / config['id'] / 'current.json').read_bytes(), old)
        self.assertTrue((data / self.zip).exists())

    def test_baseline_activation_publishes_both_after_web_ready(self):
        self.add_baseline()
        self.activation_fixture()

    def test_baseline_activation_failure_restores_absence(self):
        self.add_baseline()
        self.activation_fixture(fail=True)

    def test_baseline_activation_failure_restores_previous_bytes(self):
        self.add_baseline()
        self.activation_fixture(fail=True, previous_baseline=b'{"previous":"baseline"}')

    def test_guide_timeout_rolls_back_without_publishing_catalog(self):
        self.add_baseline()
        self.activation_fixture(readiness_fail=True)

    def baseline_http(self, fault=None):
        class Response(io.BytesIO):
            def __init__(self, data=b'', status=200, headers=None):
                super().__init__(data)
                self.status = status
                self.headers = headers or {}
        downloads = []
        def respond(request, **kwargs):
            url = request if isinstance(request, str) else request.full_url
            if url.endswith('/health') or url.endswith('/windows-x64/latest'):
                return Response(status=204)
            if url.endswith('/installer'):
                return Response(json.dumps(self.catalog).encode())
            if url.endswith('/baseline-1/latest'):
                value = dict(self.new_baseline)
                if fault == 'metadata': value['productVersion'] = '9.0.0'
                return Response(json.dumps(value).encode(), headers={'Cache-Control':'public' if fault == 'cache' else 'no-store'})
            name = url.rsplit('/', 1)[-1]
            payload = (self.root / 'releases' / name).read_bytes()
            if not isinstance(request, str) and request.get_method() == 'HEAD':
                return Response(headers={'Content-Length': str(len(payload))})
            if not isinstance(request, str) and request.has_header('Range'):
                # Tiny online fixture uses a real 10-byte payload in this harness.
                return Response(payload[:10], status=206)
            downloads.append(name)
            if fault == 'online-bytes' and name == self.online: payload = b'CORRUPTION'
            return Response(payload)
        opener = MagicMock()
        opener.open.side_effect = respond
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            deploy.verify_http('https://oto.ayani.id', self.catalog, guide=False, baseline_release=self.new_baseline)
        return downloads

    def prepare_http_baseline(self):
        self.add_baseline()
        payload = self.root / 'releases' / self.online
        payload.write_bytes(b'THIN-SETUP!')
        self.new_baseline['onlineInstaller'].update(size=payload.stat().st_size, sha256=deploy.digest(payload))
        self.save_baseline()

    def test_baseline_http_all_three_hashes(self):
        self.prepare_http_baseline()
        self.assertEqual(self.baseline_http(), [self.zip, self.exe, self.online])

    def test_baseline_http_rejects_changed_metadata_cached_feed_and_corrupt_online(self):
        self.prepare_http_baseline()
        for fault in ('metadata', 'cache', 'online-bytes'):
            with self.subTest(fault=fault), self.assertRaises(RuntimeError):
                self.baseline_http(fault)

    def test_explicit_rollback_restores_baseline_and_rejects_baseline_drift(self):
        self.add_baseline()
        self.config.update(unitSha256='a'*64, nginxSha256='b'*64)
        self.pin = self.save()
        self.activation_fixture()
        base = self.root / 'target-base'
        data = self.root / 'target-releases'
        backups = self.root / 'backups'
        config = json.loads((backups / self.config['id'] / 'publication.json').read_text())
        pointer = data / 'distribution-v2.json'
        good = pointer.read_bytes()
        pointer.write_bytes(b'changed')
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), \
             patch.object(deploy, 'BACKUPS', backups), \
             patch.object(deploy.os, 'geteuid', return_value=0, create=True), \
             patch.object(deploy.os, 'readlink', return_value=str(base / config['id'] / 'web')), \
             patch.object(deploy, 'atomic') as atomic:
            with self.assertRaisesRegex(RuntimeError, 'Baseline feed drift'):
                deploy.rollback(config)
            atomic.assert_not_called()
        pointer.write_bytes(good)
        actual_digest = deploy.digest
        def pinned_digest(path):
            if path == deploy.UNIT: return 'a'*64
            if path == deploy.SITE: return 'b'*64
            return actual_digest(path)
        with patch.object(deploy, 'BASE', base), patch.object(deploy, 'DATA', data), \
             patch.object(deploy, 'BACKUPS', backups), \
             patch.object(deploy.os, 'geteuid', return_value=0, create=True), \
             patch.object(deploy.os, 'readlink', return_value=str(base / config['id'] / 'web')), \
             patch.object(deploy.os, 'chown', create=True), \
             patch.object(deploy.grp, 'getgrnam', return_value=types.SimpleNamespace(gr_gid=0), create=True), \
             patch.object(deploy, 'digest', side_effect=pinned_digest), \
             patch.object(deploy, 'link') as link, patch.object(deploy, 'restart') as restart:
            deploy.rollback(config)
            self.assertFalse(pointer.exists())
            self.assertEqual((data / 'current.json').read_bytes(), b'{"version":"old"}')
            link.assert_called_once_with(pathlib.Path('/previous/web'))
            restart.assert_called_once()

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
        feed.__enter__.return_value.status = 200
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

    def test_manual_installer_rejects_active_update_feed(self):
        health = MagicMock()
        health.__enter__.return_value.status = 204
        feed = MagicMock()
        feed.__enter__.return_value.status = 200
        opener = MagicMock()
        opener.open.side_effect = [health, feed]
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            with self.assertRaisesRegex(RuntimeError, 'Manual installer leaked'):
                deploy.verify_http('https://oto.ayani.id', dict(self.catalog, manualInstallerOnly=True), guide=False)

    def test_manual_installer_checks_policy_on_separate_endpoint(self):
        health = MagicMock()
        health.__enter__.return_value.status = 204
        empty = MagicMock()
        empty.__enter__.return_value.status = 204
        empty.__enter__.return_value.read.return_value = b''
        installer = MagicMock()
        installer.__enter__.return_value.status = 200
        installer.__enter__.return_value.read.return_value = json.dumps(self.catalog).encode()
        opener = MagicMock()
        opener.open.side_effect = [health, empty, installer]
        with patch.object(deploy.urllib.request, 'build_opener', return_value=opener):
            with self.assertRaisesRegex(RuntimeError, 'HTTP release policy mismatch'):
                deploy.verify_http('https://oto.ayani.id', dict(self.catalog, runtimeBundled=True, manualInstallerOnly=True), guide=False)
        self.assertTrue(opener.open.call_args_list[-1].args[0].endswith('/installer'))


if __name__ == '__main__':
    unittest.main(verbosity=2)
