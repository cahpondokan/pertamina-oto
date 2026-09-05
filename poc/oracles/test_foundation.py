"""Executable tests: all observations are synthetic; no candidate/parser is run."""
import ast
import copy
import json
from pathlib import Path
import re
import sys
import unittest
sys.dont_write_bytecode = True
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'corpus'))
from generate import bundle, canonical, digest, pdf
from contract import compare, validate
from schemas import EXPECTED, MANIFEST, OBSERVATION

class FoundationTests(unittest.TestCase):
    def setUp(self):
        self.manifest, self.expected, self.data = bundle()
    def observed(self, index=0):
        recipe = self.expected['recipes'][index]
        return {'schemaVersion': '1.0', 'manifestSha256': self.expected['manifestSha256'],
                'recipeId': recipe['id'], 'pageCount': recipe['pageCount'], 'pages': copy.deepcopy(recipe['pages']),
                'structure': 'VALID', 'reopen': True, 'mode': 'SYNTHETIC_SELF_TEST', 'verifierId': 'RECORD_COMPARATOR_ONLY'}
    def check(self, value):
        return compare(self.expected, value, EXPECTED, OBSERVATION)
    def test_frozen_digest_and_schema(self):
        validate(self.manifest, MANIFEST)
        validate(self.expected, EXPECTED)
        self.assertEqual(canonical(self.manifest), (ROOT/'corpus'/'manifest.v1.json').read_bytes())
        self.assertEqual(canonical(self.expected), (ROOT/'corpus'/'expected.v1.json').read_bytes())
        self.assertEqual(digest(canonical(self.manifest)), self.expected['manifestSha256'])
    def test_determinism(self):
        other = bundle()
        self.assertEqual(canonical(self.manifest), canonical(other[0]))
        self.assertEqual(canonical(self.expected), canonical(other[1]))
        self.assertEqual(self.data, other[2])
    def test_generator_xref_and_stream_lengths(self):
        # Checks generator serialization only; not an independent PDF conformance claim.
        for data in self.data.values():
            offset = int(re.search(rb'startxref\n([0-9]+)\n%%EOF\n$', data)[1])
            self.assertEqual(data[offset:offset+4], b'xref')
            entries = re.findall(rb'([0-9]{10}) 00000 n ', data[offset:])
            for number, entry in enumerate(entries, 1):
                self.assertTrue(data[int(entry):].startswith(f'{number} 0 obj\n'.encode()))
            for length, content in re.findall(rb'/Length ([0-9]+) >>\nstream\n(.*?)endstream', data, re.S):
                self.assertEqual(int(length), len(content))
    def test_no_active_content_or_external_assets(self):
        for data in self.data.values():
            for token in (b'/JavaScript', b'/JS', b'/OpenAction', b'/AA', b'/Launch', b'/URI', b'/EmbeddedFile', b'/Encrypt'):
                self.assertNotIn(token, data)
    def test_unapproved_generator_recipe(self):
        for item, count in [('D', 1), ('A', 100), ('A', 0), ('../A', 2)]:
            with self.assertRaises(ValueError): pdf(item, count)
    def test_explicit_order_and_duplicate_occurrences(self):
        cab = self.expected['recipes'][1]
        self.assertEqual([p['source'] for p in cab['pages']], ['C', 'A', 'A', 'B', 'B', 'B'])
        aa = self.expected['recipes'][2]
        self.assertEqual([p['occurrence'] for p in aa['pages']], [1, 1, 2, 2])
        self.assertEqual(aa['pages'][0]['marker'], aa['pages'][2]['marker'])
    def test_matches_never_authorize_publication(self):
        for i in range(4):
            result = self.check(self.observed(i))
            self.assertEqual(result['status'], 'MATCH_ONLY')
            self.assertFalse(result['publishAuthorized'])
    def test_missing_extra_and_wrong_count(self):
        for mutation in ('missing', 'extra', 'count'):
            value = self.observed()
            if mutation == 'missing': value['pages'].pop()
            if mutation == 'extra': value['pages'].append(value['pages'][0])
            if mutation == 'count': value['pageCount'] += 1
            self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_same_count_wrong_order_and_content(self):
        for mutation in ('order', 'marker', 'sourcePage', 'occurrence'):
            value = self.observed()
            if mutation == 'order': value['pages'].reverse()
            if mutation == 'marker': value['pages'][0]['marker'] = 'SYNTHETIC-' + '0'*24
            if mutation == 'sourcePage': value['pages'][0]['sourcePage'] = 2
            if mutation == 'occurrence': value['pages'][0]['occurrence'] = 2
            self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_deduplication_rejected(self):
        value = self.observed(2)
        value['pages'] = value['pages'][:2]
        value['pageCount'] = 2
        self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_wrong_binding_or_recipe(self):
        for key, replacement in [('manifestSha256', '0'*64), ('recipeId', 'UNKNOWN')]:
            value = self.observed()
            value[key] = replacement
            self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_invalid_inconclusive_and_reopen(self):
        for key, replacement in [('structure', 'INVALID'), ('structure', 'INCONCLUSIVE'), ('reopen', False)]:
            value = self.observed()
            value[key] = replacement
            self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_schema_missing_unknown_type_and_version(self):
        for mutation in ('missing', 'unknown', 'type', 'version', 'mode'):
            value = self.observed()
            if mutation == 'missing': del value['reopen']
            if mutation == 'unknown': value['extra'] = 'disallowed'
            if mutation == 'type': value['pageCount'] = True
            if mutation == 'version': value['schemaVersion'] = '2.0'
            if mutation == 'mode': value['mode'] = 'REAL_VERIFIER'
            self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_bad_expected_rejected(self):
        value = self.observed()
        self.expected['recipes'].append(copy.deepcopy(self.expected['recipes'][0]))
        self.assertEqual(self.check(value)['status'], 'REJECT')
    def test_schema_extension_fail_closed(self):
        with self.assertRaises(ValueError): validate('x', {'type': 'string', '$ref': 'unapproved'})
    def test_privacy_scan_with_negative_control(self):
        patterns = [r'[A-Za-z]:[\\/]', '-----' + 'BEGIN' + r'.*PRIVATE KEY', r'AK' + r'IA[0-9A-Z]{16}',
                    r'gh' + r'p_[A-Za-z0-9]{36}', r'OTO_SYNTHETIC_' + r'LEAK_CANARY',
                    r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}']
        def scan(data): return any(re.search(p, data) for p in patterns)
        for record in [self.manifest, self.expected, self.check(self.observed())]:
            self.assertFalse(scan(canonical(record).decode()))
        self.assertTrue(scan('OTO_SYNTHETIC_' + 'LEAK_CANARY'))
        for data in self.data.values(): self.assertFalse(scan(data.decode('ascii')))
        for folder in ('corpus', 'oracles'):
            for path in (ROOT/folder).glob('*.py'):
                self.assertFalse(scan(path.read_text(encoding='utf-8-sig')))
    def test_standard_library_import_allowlist(self):
        allowed = {'ast', 'copy', 'hashlib', 'json', 'pathlib', 're', 'sys', 'unittest', 'generate', 'contract', 'schemas'}
        for folder in ('corpus', 'oracles'):
            for path in (ROOT/folder).glob('*.py'):
                tree = ast.parse(path.read_text(encoding='utf-8-sig'))
                for node in ast.walk(tree):
                    if isinstance(node, ast.Import):
                        for alias in node.names: self.assertIn(alias.name, allowed)
                    if isinstance(node, ast.ImportFrom): self.assertIn(node.module, allowed)

if __name__ == '__main__':
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(FoundationTests)
    result = unittest.TextTestRunner(verbosity=2).run(suite)
    print(f'SELF_TEST_COUNT={result.testsRun};FAILURES={len(result.failures)};ERRORS={len(result.errors)}')
    sys.exit(0 if result.testsRun > 0 and result.wasSuccessful() else 1)
