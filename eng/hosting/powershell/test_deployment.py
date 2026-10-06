import io
import pathlib
import ssl
import tarfile
import tempfile
import unittest
import urllib.error
from unittest.mock import MagicMock, patch
import deploy
from deploy import extract_regular, safe_member, validate_baseline, digest
from unpack import unpack

class DeploymentChecks(unittest.TestCase):
    def test_safe_relative_path(self):
        self.assertEqual(str(safe_member('./web/app.dll')), 'web/app.dll')

    def test_absolute_path_rejected(self):
        with self.assertRaises(RuntimeError): safe_member('/etc/passwd')

    def test_parent_path_rejected(self):
        with self.assertRaises(RuntimeError): safe_member('web/../../etc/passwd')

    def test_windows_path_rejected(self):
        with self.assertRaises(RuntimeError): safe_member('C:\\outside')

    def config(self):
        return dict(baselineNginxSha256='old', candidateNginxSha256='new', deploymentId='this', unitSha256='unit')

    def test_fresh_exact_target_allowed(self):
        validate_baseline(self.config(), 'old', None, None, False, 'pin')

    def test_nginx_drift_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'other-site', None, None, False, 'pin')

    def test_unknown_install_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'old', None, None, True, 'pin')

    def test_unknown_service_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'old', None, 'other', False, 'pin')

    def test_exact_partial_retry_allowed(self):
        validate_baseline(self.config(), 'old', dict(deploymentId='this', manifest='pin'), 'unit', True, 'pin')

    def test_exact_active_retry_allowed(self):
        validate_baseline(self.config(), 'new', dict(deploymentId='this', manifest='pin'), 'unit', True, 'pin')

    def test_different_deployment_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'new', dict(deploymentId='other', manifest='pin'), 'unit', True, 'pin')

    def test_different_payload_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'new', dict(deploymentId='this', manifest='other'), 'unit', True, 'pin')

    def test_service_changed_rejected(self):
        with self.assertRaises(RuntimeError): validate_baseline(self.config(), 'new', dict(deploymentId='this', manifest='pin'), 'other', True, 'pin')

    def failed_marker(self, **changes):
        marker = dict(deploymentId='this', manifest=deploy.PREVIOUS_FAILED_MANIFEST, status='failed-retained-for-retry')
        marker.update(changes)
        return marker

    def test_reviewed_failed_predecessor_can_retry(self):
        validate_baseline(self.config(), 'old', self.failed_marker(), 'unit', True, 'new-pin')

    def test_predecessor_active_or_unknown_status_rejected(self):
        for status in ('active', 'preparing', 'rolled-back', None):
            with self.subTest(status=status), self.assertRaises(RuntimeError):
                validate_baseline(self.config(), 'old', self.failed_marker(status=status), 'unit', True, 'new-pin')

    def test_predecessor_candidate_site_rejected(self):
        with self.assertRaises(RuntimeError):
            validate_baseline(self.config(), 'new', self.failed_marker(), 'unit', True, 'new-pin')

    def test_predecessor_wrong_release_manifest_or_unit_rejected(self):
        for marker, unit in ((self.failed_marker(deploymentId='other'), 'unit'),
                             (self.failed_marker(manifest='other'), 'unit'),
                             (self.failed_marker(), None), (self.failed_marker(), 'other')):
            with self.subTest(marker=marker, unit=unit), self.assertRaises(RuntimeError):
                validate_baseline(self.config(), 'old', marker, unit, True, 'new-pin')

    def response(self, status):
        response = MagicMock()
        response.__enter__.return_value.status = status
        return response

    def test_reload_old_404_then_ready_still_runs_full_verification(self):
        error = urllib.error.HTTPError('https://oto.ayani.id/health', 404, 'Not Found', {}, None)
        with patch.object(deploy, 'request', side_effect=[error, self.response(204)]) as requests, \
             patch.object(deploy.time, 'sleep') as sleep, patch.object(deploy, 'verify_http') as verify:
            deploy.verify_public(self.config())
            self.assertEqual(requests.call_count, 2)
            self.assertEqual(sleep.call_count, 1)
            verify.assert_called_once_with('https://oto.ayani.id', self.config())

    def test_persistent_404_expires_with_endpoint_and_no_acceptance(self):
        error = urllib.error.HTTPError('https://oto.ayani.id/health', 404, 'Not Found', {}, None)
        with patch.object(deploy, 'request', side_effect=error), \
             patch.object(deploy.time, 'monotonic', side_effect=[0, 0, 45, 45]), \
             patch.object(deploy, 'verify_http') as verify:
            with self.assertRaisesRegex(RuntimeError, r'https://oto.ayani.id/health.*HTTP 404'):
                deploy.verify_public(self.config())
            verify.assert_not_called()

    def test_transient_gateway_error_then_ready(self):
        for status in (502, 503, 504):
            error = urllib.error.HTTPError('https://oto.ayani.id/health', status, 'Gateway', {}, None)
            with self.subTest(status=status), patch.object(deploy, 'request', side_effect=[error, self.response(204)]), \
                 patch.object(deploy.time, 'sleep'):
                deploy.wait_public_health('https://oto.ayani.id')

    def test_forbidden_and_tls_errors_are_not_retried(self):
        for error in (urllib.error.HTTPError('https://oto.ayani.id/health', 403, 'Forbidden', {}, None),
                      urllib.error.URLError(ssl.SSLCertVerificationError('invalid certificate')),
                      ssl.SSLError('TLS failure')):
            with self.subTest(error=error), patch.object(deploy, 'request', side_effect=error) as requests, \
                 patch.object(deploy.time, 'sleep') as sleep:
                with self.assertRaises(type(error)):
                    deploy.wait_public_health('https://oto.ayani.id')
                requests.assert_called_once()
                sleep.assert_not_called()

    def test_200_placeholder_is_not_health(self):
        with patch.object(deploy, 'request', return_value=self.response(200)), patch.object(deploy, 'verify_http') as verify:
            with self.assertRaisesRegex(RuntimeError, 'Health HTTPS tidak sesuai'):
                deploy.verify_public(self.config())
            verify.assert_not_called()

    def test_ready_health_does_not_hide_bad_download(self):
        with patch.object(deploy, 'request', return_value=self.response(204)), \
             patch.object(deploy, 'verify_http', side_effect=RuntimeError('Hash EXE HTTP tidak cocok.')):
            with self.assertRaisesRegex(RuntimeError, 'Hash EXE HTTP tidak cocok'):
                deploy.verify_public(self.config())

    def archive(self, folder, names, kind=None):
        path = pathlib.Path(folder) / 'payload.tar.gz'
        with tarfile.open(path, 'w:gz') as archive:
            for name in names:
                entry = tarfile.TarInfo(name)
                if kind:
                    entry.type = kind
                    entry.linkname = '/etc/passwd'
                    archive.addfile(entry)
                else:
                    entry.size = 4
                    archive.addfile(entry, io.BytesIO(b'test'))
        return path

    def test_valid_archive_and_resume(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['web/a.txt'])
            destination = pathlib.Path(folder) / 'contents'
            unpack(archive, destination, digest(archive))
            unpack(archive, destination, digest(archive))
            self.assertEqual((destination / 'web/a.txt').read_bytes(), b'test')
            extract_regular(archive, pathlib.Path(folder) / 'runtime')

    def test_corrupt_upload_rejected_before_output(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['a'])
            destination = pathlib.Path(folder) / 'contents'
            with self.assertRaises(ValueError): unpack(archive, destination, '0' * 64)
            self.assertFalse(destination.exists())

    def test_archive_traversal_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['../outside'])
            with self.assertRaises(ValueError): unpack(archive, pathlib.Path(folder) / 'contents', digest(archive))
            with self.assertRaises(RuntimeError): extract_regular(archive, pathlib.Path(folder) / 'runtime')

    def test_archive_symlink_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['a'], tarfile.SYMTYPE)
            with self.assertRaises(ValueError): unpack(archive, pathlib.Path(folder) / 'contents', digest(archive))
            with self.assertRaises(RuntimeError): extract_regular(archive, pathlib.Path(folder) / 'runtime')

    def test_archive_hardlink_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['a'], tarfile.LNKTYPE)
            with self.assertRaises(ValueError): unpack(archive, pathlib.Path(folder) / 'contents', digest(archive))
            with self.assertRaises(RuntimeError): extract_regular(archive, pathlib.Path(folder) / 'runtime')

    def test_archive_duplicate_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            archive = self.archive(folder, ['a', './a'])
            with self.assertRaises(ValueError): unpack(archive, pathlib.Path(folder) / 'contents', digest(archive))
            with self.assertRaises(RuntimeError): extract_regular(archive, pathlib.Path(folder) / 'runtime')

    def rollback_fixture(self, folder):
        folder = pathlib.Path(folder)
        site = folder / 'site.conf'
        site.write_text('candidate')
        backup = folder / 'oto.ayani.id.before.conf'
        backup.write_text('original')
        config = dict(baselineNginxSha256=digest(backup), candidateNginxSha256=digest(site))
        return site, config

    def test_rollback_restores_only_known_site(self):
        with tempfile.TemporaryDirectory() as folder:
            site, config = self.rollback_fixture(folder)
            with patch.object(deploy, 'SITE', site), patch.object(deploy, 'backup_path', return_value=pathlib.Path(folder)), patch.object(deploy, 'run') as commands:
                deploy.restore_site(config)
                self.assertEqual(site.read_text(), 'original')
                self.assertEqual([item.args for item in commands.call_args_list], [('nginx', '-t'), ('systemctl', 'reload', 'nginx')])

    def test_rollback_does_not_overwrite_new_admin_edits(self):
        with tempfile.TemporaryDirectory() as folder:
            site, config = self.rollback_fixture(folder)
            site.write_text('administrator edit')
            with patch.object(deploy, 'SITE', site), patch.object(deploy, 'backup_path', return_value=pathlib.Path(folder)), patch.object(deploy, 'run') as commands:
                with self.assertRaises(RuntimeError): deploy.restore_site(config)
                self.assertEqual(site.read_text(), 'administrator edit')
                commands.assert_not_called()

    def test_failed_nginx_validation_restores_candidate_file(self):
        with tempfile.TemporaryDirectory() as folder:
            site, config = self.rollback_fixture(folder)
            with patch.object(deploy, 'SITE', site), patch.object(deploy, 'backup_path', return_value=pathlib.Path(folder)), patch.object(deploy, 'run', side_effect=RuntimeError('invalid unrelated config')):
                with self.assertRaises(RuntimeError): deploy.restore_site(config)
                self.assertEqual(site.read_text(), 'candidate')

if __name__ == '__main__':
    unittest.main(verbosity=2)
