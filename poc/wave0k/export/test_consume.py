"""Only synthetic JSON/text. Child processes forbidden."""
import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch
import consume

sys.path.insert(0, str(Path(__file__).resolve().parents[2] / 'wave0k/oracle'))
import observe
from test_observe import fixture, binding


class ExportTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.directory = Path(self.temp.name)
        guard = patch.object(observe.subprocess, 'Popen', side_effect=AssertionError('NO_CHILD'))
        guard.start()
        self.addCleanup(guard.stop)
        self.expected = {'schema': 1, 'observerScriptSha256': 'e' * 64,
                         'sourceSha256': binding()['sourceSha256'], 'qpdfFiles': binding()['qpdfFiles']}
        for case in consume.CASES:
            data = binding()
            data['case'] = case
            self.write(case + '.binding.json', data)
            version = b'qpdf version 12.4.1\n'
            raw = json.dumps(fixture(case)).encode()
            for label, stdout in [('version', version), ('observation', raw)]:
                (self.directory / (case + '.' + label + '.stdout')).write_bytes(stdout)
                (self.directory / (case + '.' + label + '.stderr')).write_bytes(b'')
            markers = observe.inspect_json(raw, case)
            result = {'schema': 1, 'verdict': 'BOUNDED_STRUCTURAL_MATCH', 'case': case,
                      'observer': 'qpdf/12.4.1', 'merger': 'PDFsharp/6.2.4',
                      'pageCount': len(markers), 'pageMarkers': markers,
                      'bindingSha256': consume.sha((self.directory / (case + '.binding.json')).read_bytes()),
                      'outputSha256': data['outputSha256'], 'sourceSha256': data['sourceSha256'],
                      'qpdfFiles': data['qpdfFiles'], 'observerScriptSha256': 'e' * 64,
                      'observerArguments': [*observe.JSON_ARGS, '<bound-output>'],
                      'fidelity': 'NOT_ASSESSED', 'provenance': 'ROOT_LEDGER_REVIEW_REQUIRED'}
            for label, stdout in [('version', version), ('observation', raw)]:
                result[label + 'Capture'] = {'exitCode': 0, 'stdoutSha256': consume.sha(stdout),
                                            'stderrSha256': consume.sha(b''), 'elapsedSeconds': 0.2}
            self.write(case + '.result.json', result)

    def write(self, name, data):
        (self.directory / name).write_text(json.dumps(data), encoding='utf-8')

    def run_check(self):
        return consume.validate(self.directory, self.expected, observe)

    def mutate(self, field, value):
        path = self.directory / 'AB.result.json'
        data = json.loads(path.read_text())
        data[field] = value
        self.write(path.name, data)

    def test_all(self):
        result = self.run_check()
        self.assertEqual(len(result['cases']), 4)
        self.assertEqual(result['runtimeAuthenticity'], 'NOT_PROVEN_BY_EXPORT')

    def test_single_case(self):
        for path in self.directory.iterdir():
            if not path.name.startswith('AB.'):
                path.unlink()
        self.assertEqual(len(consume.validate(self.directory, self.expected, observe, ('AB',))['cases']), 1)

    def test_extra_file(self):
        self.write('extra.json', {})
        with self.assertRaises(ValueError): self.run_check()

    def test_missing_file(self):
        (self.directory / 'AC.result.json').unlink()
        with self.assertRaises(ValueError): self.run_check()

    def test_schema(self):
        self.mutate('extra', 1)
        with self.assertRaises(ValueError): self.run_check()

    def test_boolean(self):
        self.mutate('schema', True)
        with self.assertRaises(ValueError): self.run_check()

    def test_capture_tamper(self):
        (self.directory / 'AB.observation.stdout').write_bytes(b'{}')
        with self.assertRaises(ValueError): self.run_check()

    def test_rehashed_wrong_order(self):
        data = fixture('AB')
        data['qpdf'][1]['obj:2 0 R']['value']['/Kids'].reverse()
        raw = json.dumps(data).encode()
        (self.directory / 'AB.observation.stdout').write_bytes(raw)
        result = json.loads((self.directory / 'AB.result.json').read_text())
        result['observationCapture']['stdoutSha256'] = consume.sha(raw)
        self.write('AB.result.json', result)
        with self.assertRaises(ValueError): self.run_check()

    def test_binding_tamper(self):
        self.write('AB.binding.json', {})
        with self.assertRaises(ValueError): self.run_check()

    def test_frozen_source(self):
        self.expected['sourceSha256'] = {n: 'f' * 64 for n in 'ABC'}
        with self.assertRaises(ValueError): self.run_check()

    def test_stderr(self):
        (self.directory / 'AB.version.stderr').write_bytes(b'warning')
        with self.assertRaises(ValueError): self.run_check()

    def test_oversize(self):
        (self.directory / 'AB.result.json').write_bytes(b' ' * 65537)
        with self.assertRaises(ValueError): self.run_check()

    def test_duplicate_nonfinite(self):
        for raw in (b'{"a":1,"a":2}', b'{"a":NaN}'):
            with self.assertRaises(ValueError): consume.parse(raw)

    def test_script_hash(self):
        with self.assertRaises(ValueError): consume.load_observer('0' * 64)

    def test_directory_member(self):
        path = self.directory / 'AB.result.json'
        path.unlink()
        path.mkdir()
        with self.assertRaises(ValueError): self.run_check()

    def test_hardlink(self):
        path = self.directory / 'AB.result.json'
        alias = self.directory.parent / (self.directory.name + '.alias')
        os.link(path, alias)
        self.addCleanup(alias.unlink)
        with self.assertRaises(ValueError): self.run_check()

    def test_reparse_attribute(self):
        original = Path.lstat
        def fake(path):
            info = original(path)
            if path.name == 'AB.result.json':
                from types import SimpleNamespace
                return SimpleNamespace(st_mode=info.st_mode, st_file_attributes=0x400)
            return info
        with patch.object(Path, 'lstat', fake):
            with self.assertRaises(ValueError): self.run_check()


if __name__ == '__main__':
    unittest.main()
