"""JSON/text-only tests. Never generate/open PDFs or start a child process."""
import base64
import copy
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch, MagicMock

import observe as observer


def fixture(case='AB'):
    markers = [m for source in observer.CASES[case] for m in observer.SOURCES[source]]
    objects = {
        'trailer': {'value': {'/Root': '1 0 R', '/Size': 40}},
        'obj:1 0 R': {'value': {'/Type': '/Catalog', '/Pages': '2 0 R'}},
        'obj:2 0 R': {'value': {'/Type': '/Pages', '/Count': len(markers), '/Kids': []}},
        'obj:3 0 R': {'value': {'/Type': '/Font', '/Subtype': '/Type1', '/BaseFont': '/Helvetica'}}}
    for index, marker in enumerate(markers):
        page, stream = 4 + index * 2, 5 + index * 2
        objects['obj:2 0 R']['value']['/Kids'].append(f'{page} 0 R')
        objects[f'obj:{page} 0 R'] = {'value': {'/Type': '/Page', '/Parent': '2 0 R',
            '/MediaBox': [0, 0, 612, 792], '/Contents': f'{stream} 0 R',
            '/Resources': {'/Font': {'/F1': '3 0 R'}}}}
        text = f'BT /F1 24 Tf 72 720 Td ({marker}) Tj ET\n'
        objects[f'obj:{stream} 0 R'] = {'stream': {'dict': {}, 'data': base64.b64encode(text.encode()).decode()}}
    return {'qpdf': [{'jsonversion': 2, 'pdfversion': '1.4'}, objects]}


def binding():
    return {'schema': 1, 'case': 'AB', 'merger': 'PDFsharp/6.2.4',
            'sourceSha256': {n: 'a' * 64 for n in observer.SOURCES},
            'outputSha256': 'b' * 64, 'qpdfFiles': {'qpdf.exe': 'c' * 64, 'qpdf30.dll': 'd' * 64}}


class ObserverTests(unittest.TestCase):
    def setUp(self):
        # Every test fails immediately if any code attempts a real child process.
        guard = patch.object(observer.subprocess, 'Popen', side_effect=AssertionError('CHILD_FORBIDDEN'))
        guard.start()
        self.addCleanup(guard.stop)

    def check(self, data, case='AB'):
        return observer.inspect_json(json.dumps(data).encode(), case)

    def reject(self, mutation):
        data = fixture()
        mutation(data['qpdf'][1])
        with self.assertRaises(observer.Rejected):
            self.check(data)

    def test_four_cases(self):
        for case, count in {'AB': 5, 'CAB': 6, 'AA': 4, 'AC': 3}.items():
            with self.subTest(case=case):
                self.assertEqual(len(self.check(fixture(case), case)), count)

    def test_exact_wave0h_spec(self):
        spec = json.loads((Path(__file__).parents[2] / 'wave0h/corpus/spec.json').read_text())
        self.assertEqual(spec['sources'], observer.SOURCES)
        for case, sources in observer.CASES.items():
            self.assertEqual(spec['cases'][case]['inputs'], sources)
            self.assertEqual(self.check(fixture(case), case), spec['cases'][case]['expected'])

    def test_reordered_pages(self):
        self.reject(lambda o: o['obj:2 0 R']['value']['/Kids'].reverse())

    def test_duplicate_reference(self):
        self.reject(lambda o: o['obj:2 0 R']['value']['/Kids'].__setitem__(1, '4 0 R'))

    def test_wrong_count_and_bool_count(self):
        for value in (4, True, '5'):
            self.reject(lambda o: o['obj:2 0 R']['value'].__setitem__('/Count', value))

    def test_dangling_reference(self):
        self.reject(lambda o: o.pop('obj:4 0 R'))

    def test_nested_or_cyclic_tree(self):
        self.reject(lambda o: o['obj:2 0 R']['value']['/Kids'].__setitem__(0, '2 0 R'))

    def test_empty_or_compressed_stream(self):
        self.reject(lambda o: o['obj:5 0 R']['stream'].__setitem__('data', ''))
        self.reject(lambda o: o['obj:5 0 R']['stream']['dict'].__setitem__('/Filter', '/FlateDecode'))

    def test_stream_encoding(self):
        for text in ('!!!!', base64.b64encode(b'\xff').decode()):
            self.reject(lambda o: o['obj:5 0 R']['stream'].__setitem__('data', text))

    def test_marker_comment_hidden_or_extra_operators(self):
        for text in ('% W0H-A-P1', 'BT /F1 24 Tf 72 720 Td (W0H-A-P1) Tj 3 Tr ET',
                     'BT /F1 24 Tf 72 720 Td (W0H-A-P2) Tj ET',
                     'q BT /F1 24 Tf 72 720 Td (W0H-A-P1) Tj ET Q'):
            self.reject(lambda o: o['obj:5 0 R']['stream'].__setitem__('data', base64.b64encode(text.encode()).decode()))

    def test_font_substitution(self):
        self.reject(lambda o: o['obj:3 0 R']['value'].__setitem__('/BaseFont', '/Symbol'))

    def test_custom_font_encoding(self):
        self.reject(lambda o: o['obj:3 0 R']['value'].__setitem__('/Encoding', {'/Differences': []}))

    def test_annotation_and_crop(self):
        for key, value in (('/Annots', []), ('/CropBox', [0, 0, 1, 1]), ('/Rotate', 90)):
            self.reject(lambda o: o['obj:4 0 R']['value'].__setitem__(key, value))

    def test_encryption_or_action(self):
        self.reject(lambda o: o['trailer']['value'].__setitem__('/Encrypt', '9 0 R'))
        self.reject(lambda o: o['obj:1 0 R']['value'].__setitem__('/OpenAction', '9 0 R'))

    def test_multiple_streams(self):
        self.reject(lambda o: o['obj:4 0 R']['value'].__setitem__('/Contents', ['5 0 R', '7 0 R']))

    def test_single_array_stream_allowed(self):
        data = fixture()
        data['qpdf'][1]['obj:4 0 R']['value']['/Contents'] = ['5 0 R']
        self.assertEqual(len(self.check(data)), 5)

    def test_empty_duplicate_nonfinite_or_oversize_json(self):
        for raw in (b'', b' ', b'{"qpdf": [], "qpdf": []}', b'{"x": NaN}', b'x' * (observer.LIMIT + 1)):
            with self.subTest(raw_length=len(raw)), self.assertRaises(observer.Rejected):
                observer.parse(raw)

    def test_json_schema(self):
        for data in ({}, [], {'qpdf': True}, {'qpdf': [None, {}]}, {'qpdf': [{'jsonversion': True}, {}]}):
            with self.subTest(data=data), self.assertRaises(observer.Rejected):
                self.check(data)

    def test_unknown_or_nonstring_case(self):
        for case in ('BB', [], True):
            with self.assertRaises(observer.Rejected):
                observer.inspect_json(b'{}', case)

    def test_binding_accepts_only_exact_schema(self):
        observer.validate_binding(binding(), 'AB')
        bad = binding()
        bad['independent'] = True
        with self.assertRaises(observer.Rejected):
            observer.validate_binding(bad, 'AB')

    def test_binding_rejects_self_observation_and_wrong_types(self):
        for key, value in (('merger', 'qpdf/12.4.1'), ('schema', True), ('outputSha256', 'x'), ('case', [])):
            bad = binding()
            bad[key] = value
            with self.assertRaises(observer.Rejected):
                observer.validate_binding(bad, 'AB')

    def test_binding_rejects_missing_source_and_empty_bundle(self):
        for key, value in (('sourceSha256', {}), ('qpdfFiles', {})):
            bad = binding()
            bad[key] = value
            with self.assertRaises(observer.Rejected):
                observer.validate_binding(bad, 'AB')

    def test_binding_rejects_traversal_alias_and_bad_hash(self):
        for name, digest in (('../escape.dll', 'e' * 64), ('QPDF.EXE', 'e' * 64), ('foo.dll', 'not-a-hash')):
            bad = binding()
            bad['qpdfFiles'][name] = digest
            with self.assertRaises(observer.Rejected):
                observer.validate_binding(bad, 'AB')

    def test_capture_rejects_warning_empty_stderr_and_types(self):
        for code, out, err in ((3, b'{}', b''), (0, b'', b''), (0, b' ', b''),
                               (0, b'{}', b'warning'), (False, b'{}', b''), (0, b'x' * (observer.LIMIT + 1), b'')):
            with self.assertRaises(observer.Rejected):
                observer.checked_capture(code, out, err)

    def test_bounded_text_io(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'fixture.txt'
            path.write_bytes(b'text-only')
            self.assertEqual(observer.bounded(path), b'text-only')
            with self.assertRaises(observer.Rejected):
                observer.bounded(path, 2)
            path.write_bytes(b'')
            with self.assertRaises(observer.Rejected):
                observer.bounded(path)

    def test_file_digest_and_inventory_rejection_with_mock_text(self):
        # In-memory text only. No path below is opened; bounded() is replaced.
        payloads = {'qpdf.exe': b'tool text', 'qpdf30.dll': b'library text',
                    'A.pdf': b'A text', 'B.pdf': b'B text', 'C.pdf': b'C text', 'output.pdf': b'output text'}
        bound = binding()
        bound['qpdfFiles'] = {n: observer.sha(payloads[n]) for n in ('qpdf.exe', 'qpdf30.dll')}
        bound['sourceSha256'] = {n: observer.sha(payloads[n + '.pdf']) for n in observer.SOURCES}
        bound['outputSha256'] = observer.sha(payloads['output.pdf'])
        with patch.object(observer, 'bounded', side_effect=lambda p, *a: payloads[Path(p).name]), \
             patch.object(Path, 'is_dir', return_value=True), \
             patch.object(Path, 'samefile', return_value=False), \
             patch.object(Path, 'iterdir', return_value=[Path('qpdf.exe'), Path('qpdf30.dll')]):
            observer.verify_files(bound, 'bundle', 'sources', 'output.pdf')
            for target, member in (('qpdfFiles', 'qpdf.exe'), ('sourceSha256', 'A'), ('outputSha256', None)):
                bad = copy.deepcopy(bound)
                if member:
                    bad[target][member] = '0' * 64
                else:
                    bad[target] = '0' * 64
                with self.assertRaises(observer.Rejected):
                    observer.verify_files(bad, 'bundle', 'sources', 'output.pdf')
            with patch.object(Path, 'samefile', return_value=True), self.assertRaises(observer.Rejected):
                observer.verify_files(bound, 'bundle', 'sources', 'output.pdf')
            with patch.object(Path, 'iterdir', return_value=[Path('qpdf.exe')]), self.assertRaises(observer.Rejected):
                observer.verify_files(bound, 'bundle', 'sources', 'output.pdf')

    def test_capture_timeout_kills_mock_child(self):
        process = MagicMock()
        process.poll.return_value = None
        with tempfile.TemporaryDirectory() as temp, \
             patch.object(observer.subprocess, 'Popen', return_value=process), \
             patch.object(observer.time, 'monotonic', side_effect=[0, 16]), \
             patch.dict(observer.os.environ, {'SystemRoot': 'C:/Windows'}):
            with self.assertRaisesRegex(observer.Rejected, 'QPDF_TIMEOUT'):
                observer.capture(Path('fake.exe'), ['--version'], Path(temp), 'fixture')
            process.kill.assert_called_once()
            process.wait.assert_called_once_with(timeout=5)

    def test_capture_size_guard_kills_mock_child(self):
        process = MagicMock()
        process.poll.return_value = None
        def fake_start(*args, **kwargs):
            kwargs['stdout'].write(b'x' * (observer.LIMIT + 1))
            kwargs['stdout'].flush()
            return process
        with tempfile.TemporaryDirectory() as temp, \
             patch.object(observer.subprocess, 'Popen', side_effect=fake_start), \
             patch.object(observer.time, 'monotonic', return_value=0), \
             patch.dict(observer.os.environ, {'SystemRoot': 'C:/Windows'}):
            with self.assertRaisesRegex(observer.Rejected, 'CAPTURE_LIMIT'):
                observer.capture(Path('fake.exe'), [], Path(temp), 'fixture')
            process.kill.assert_called_once()


if __name__ == '__main__':
    unittest.main(verbosity=2)
