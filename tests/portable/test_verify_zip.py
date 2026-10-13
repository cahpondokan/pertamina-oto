import importlib.util
import tempfile
import unittest
import zipfile
import json
import hashlib
from pathlib import Path

spec = importlib.util.spec_from_file_location('verify_zip', Path(__file__).with_name('verify_zip.py'))
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)

class ZipBoundaryTests(unittest.TestCase):
    def inspect(self, entries):
        with tempfile.TemporaryDirectory(prefix='oto-portable-qa-') as root:
            path = Path(root) / 'synthetic.zip'
            with zipfile.ZipFile(path, 'w') as archive:
                for name, data in entries:
                    archive.writestr(name, data)
            return module.inspect(path)

    def test_benign_unicode_space_and_hash(self):
        result = self.inspect([('OTO uji/文档.txt', b'synthetic')])
        self.assertEqual([], result['failures'])
        self.assertEqual(9, result['files'][0]['size'])
        self.assertEqual(64, len(result['files'][0]['sha256']))

    def test_windows_unsafe_paths(self):
        for name in ['../outside', '/absolute', 'C:/drive', 'app/a:stream',
                     'app/CON.txt', 'app/file. ', 'app/../escape', '..\\escape']:
            with self.subTest(name=name):
                self.assertTrue(self.inspect([(name, b'inert')])['failures'])

    def test_case_collision(self):
        self.assertTrue(self.inspect([('app/A.dll', b'a'), ('app/a.dll', b'b')])['failures'])

    def test_symlink(self):
        info = zipfile.ZipInfo('app/link')
        info.create_system = 3
        info.external_attr = 0o120777 << 16
        self.assertTrue(self.inspect([(info, b'outside')])['failures'])

    def manifest_entries(self):
        names = ['PERTAMINA-OTO.exe', 'app/Pertamina.Oto.Desktop.App.exe',
                 'app/portable-profile.json', 'MULAI-DI-SINI.txt', 'source-inventory.json']
        data = b'inert'
        manifest = {'Files': [{'Path': n, 'Length': len(data),
                     'Sha256': hashlib.sha256(data).hexdigest().upper()} for n in names]}
        return [('OTO/' + n, data) for n in names] + [('OTO/package-manifest.json', json.dumps(manifest))]

    def test_exact_manifest_inventory(self):
        self.assertFalse(self.inspect(self.manifest_entries())['failures'])

    def test_undeclared_payload(self):
        entries = self.manifest_entries() + [('OTO/app/extra.dll', b'inert')]
        self.assertIn('manifest path inventory mismatch', self.inspect(entries)['failures'])

    def test_payload_hash_mismatch(self):
        entries = self.manifest_entries()
        entries[0] = (entries[0][0], b'changed')
        self.assertTrue(any('size/hash mismatch' in f for f in self.inspect(entries)['failures']))

if __name__ == '__main__':
    unittest.main(verbosity=2)
