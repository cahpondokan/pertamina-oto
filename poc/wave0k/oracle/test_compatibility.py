"""JSON-only compatibility tests; never reads PDF or starts qpdf."""
import base64
import json
import unittest
from unittest.mock import patch
import observe
from test_observe import fixture


def compatible(case='AB', metadata=b'<x:xmpmeta xmlns:x="adobe:ns:meta/"></x:xmpmeta>'):
    data = fixture(case)
    objects = data['qpdf'][1]
    objects['obj:1 0 R']['value']['/Metadata'] = '30 0 R'
    objects['obj:30 0 R'] = {'stream': {'dict': {'/Type': '/Metadata', '/Subtype': '/XML'},
                                      'data': base64.b64encode(metadata).decode()}}
    for ref in objects['obj:2 0 R']['value']['/Kids']:
        objects['obj:' + ref]['value']['/Group'] = {'/CS': '/DeviceRGB', '/S': '/Transparency'}
    return data


class CompatibilityTests(unittest.TestCase):
    def setUp(self):
        guard = patch.object(observe.subprocess, 'Popen', side_effect=AssertionError('CHILD_FORBIDDEN'))
        guard.start()
        self.addCleanup(guard.stop)

    def check(self, data, case='AB'):
        return observe.inspect_json(json.dumps(data).encode(), case)

    def test_four_metadata_group_cases(self):
        for case, count in [('AB', 5), ('CAB', 6), ('AA', 4), ('AC', 3)]:
            self.assertEqual(len(self.check(compatible(case), case)), count)

    def test_bad_group(self):
        for group in ({'/CS': '/DeviceCMYK', '/S': '/Transparency'},
                      {'/CS': '/DeviceRGB', '/S': '/Other'},
                      {'/CS': '/DeviceRGB', '/S': '/Transparency', '/I': True},
                      '30 0 R', None, []):
            data = compatible()
            data['qpdf'][1]['obj:4 0 R']['value']['/Group'] = group
            with self.assertRaises(observe.Rejected): self.check(data)

    def test_unknown_catalog(self):
        data = compatible()
        data['qpdf'][1]['obj:1 0 R']['value']['/Lang'] = 'en'
        with self.assertRaises(observe.Rejected): self.check(data)

    def test_metadata_dictionary(self):
        for dictionary in ({'/Type': '/Metadata'},
                           {'/Type': '/Metadata', '/Subtype': '/Other'},
                           {'/Type': '/Metadata', '/Subtype': '/XML', '/Filter': '/FlateDecode'}):
            data = compatible()
            data['qpdf'][1]['obj:30 0 R']['stream']['dict'] = dictionary
            with self.assertRaises(observe.Rejected): self.check(data)

    def test_metadata_encoding(self):
        for value in ('!!!!', base64.b64encode(b'\xff').decode(), 12, ''):
            data = compatible()
            data['qpdf'][1]['obj:30 0 R']['stream']['data'] = value
            with self.assertRaises(observe.Rejected): self.check(data)

    def test_metadata_size_empty_nul_declarations(self):
        for value in (b' ' * 8193, b'', b'   ', b'<x>\x00</x>',
                      b'<!DOCTYPE x><x/>', b'<!ENTITY x "value"><x/>',
                      b'<!doctype x><x/>'):
            with self.assertRaises(observe.Rejected): self.check(compatible(metadata=value))

    def test_bad_metadata_reference(self):
        for value in ('99 0 R', {}, '30 1 R'):
            data = compatible()
            data['qpdf'][1]['obj:1 0 R']['value']['/Metadata'] = value
            with self.assertRaises(observe.Rejected): self.check(data)

    def test_changed_marker_not_hidden_by_metadata(self):
        data = compatible(metadata=b'<x>W0H-A-P1</x>')
        data['qpdf'][1]['obj:5 0 R']['stream']['data'] = base64.b64encode(
            b'BT /F1 24 Tf 72 720 Td (W0H-B-P1) Tj ET').decode()
        with self.assertRaises(observe.Rejected): self.check(data)

    def test_metadata_is_not_xml_or_fidelity_validation(self):
        # Explicit limitation: inert metadata UTF8 is not semantically parsed.
        self.assertEqual(len(self.check(compatible(metadata=b'not XML'))), 5)


if __name__ == '__main__':
    unittest.main()
