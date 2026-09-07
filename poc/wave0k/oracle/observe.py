"""Tiny Wave0H marker observer: qpdf JSON for PDFsharp outputs only.

Not a general PDF parser, renderer, trust authority or publication validator.
Import is inert. Only main() launches the already admitted qpdf executable.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import subprocess
import time

SOURCES = {'A': ['W0H-A-P1', 'W0H-A-P2'],
           'B': ['W0H-B-P1', 'W0H-B-P2', 'W0H-B-P3'], 'C': ['W0H-C-P1']}
CASES = {'AB': ['A', 'B'], 'CAB': ['C', 'A', 'B'],
         'AA': ['A', 'A'], 'AC': ['A', 'C']}
LIMIT = 1048576
JSON_ARGS = ['--json-output=2', '--json-stream-data=inline', '--decode-level=generalized']


class Rejected(ValueError):
    """Only fixed, sanitized error codes belong in this exception."""


def require(condition, code):
    if not condition:
        raise Rejected(code)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def valid_hash(value):
    return type(value) is str and re.fullmatch('[0-9a-f]{64}', value) is not None


def exact(value, keys):
    require(type(value) is dict and set(value) == set(keys), 'SCHEMA_KEYS')


def unique(pairs):
    result = {}
    for key, value in pairs:
        require(key not in result, 'DUPLICATE_JSON_KEY')
        result[key] = value
    return result


def parse(raw):
    require(type(raw) is bytes and 0 < len(raw) <= LIMIT and raw.strip(), 'EMPTY_OR_OVERSIZE_JSON')
    try:
        return json.loads(raw.decode('utf-8'), object_pairs_hook=unique,
                          parse_constant=lambda _: (_ for _ in ()).throw(Rejected('NONFINITE_JSON')))
    except (UnicodeError, json.JSONDecodeError, RecursionError):
        raise Rejected('INVALID_JSON') from None


def inspect_json(raw, case):
    """Observe a flat page tree and exact simple text program from qpdf JSON v2."""
    require(type(case) is str and case in CASES, 'UNKNOWN_CASE')
    document = parse(raw)
    exact(document, ['qpdf'])
    data = document['qpdf']
    require(type(data) is list and len(data) == 2, 'QPDF_SCHEMA')
    header, objects = data
    require(type(header) is dict and type(header.get('jsonversion')) is int
            and header['jsonversion'] == 2, 'QPDF_VERSION')
    require(type(objects) is dict and 1 <= len(objects) <= 80, 'OBJECT_LIMIT')

    def indirect(reference):
        require(type(reference) is str and re.fullmatch(r'[1-9][0-9]* 0 R', reference), 'INDIRECT_REFERENCE')
        require('obj:' + reference in objects, 'MISSING_OBJECT')
        return objects['obj:' + reference]

    def value(reference):
        record = indirect(reference)
        exact(record, ['value'])
        require(type(record['value']) is dict, 'DICTIONARY_REQUIRED')
        return record['value']

    def dictionary(item):
        return value(item) if type(item) is str else item

    trailer = objects.get('trailer')
    exact(trailer, ['value'])
    trailer = trailer['value']
    require(type(trailer) is dict and '/Root' in trailer
            and set(trailer) <= {'/Root', '/Size', '/ID', '/Info'}, 'TRAILER_PROFILE')
    catalog = value(trailer['/Root'])
    require(type(catalog) is dict and {'/Type', '/Pages'} <= set(catalog)
            and set(catalog) <= {'/Type', '/Pages', '/Metadata'}, 'CATALOG_PROFILE')
    require(catalog['/Type'] == '/Catalog', 'CATALOG_TYPE')
    if '/Metadata' in catalog:
        metadata = indirect(catalog['/Metadata'])
        exact(metadata, ['stream'])
        metadata = metadata['stream']
        exact(metadata, ['dict', 'data'])
        exact(metadata['dict'], ['/Subtype', '/Type'])
        require(metadata['dict']['/Subtype'] == '/XML' and
                metadata['dict']['/Type'] == '/Metadata', 'METADATA_TYPE')
        require(type(metadata['data']) is str and 0 < len(metadata['data']) <= 10924,
                'METADATA_BOUND')
        try:
            metadata_bytes = base64.b64decode(metadata['data'], validate=True)
            metadata_text = metadata_bytes.decode('utf-8')
        except (ValueError, UnicodeError):
            raise Rejected('METADATA_ENCODING') from None
        require(0 < len(metadata_bytes) <= 8192 and metadata_text.strip()
                and '\x00' not in metadata_text, 'METADATA_BOUND')
        require('<!DOCTYPE' not in metadata_text.upper() and
                '<!ENTITY' not in metadata_text.upper(), 'METADATA_DECLARATION')
        # Metadata is not parsed, resolved, rendered or used as page evidence.
    tree = value(catalog['/Pages'])
    exact(tree, ['/Type', '/Kids', '/Count'])
    require(tree['/Type'] == '/Pages' and type(tree['/Count']) is int
            and type(tree['/Kids']) is list and 1 <= len(tree['/Kids']) <= 6
            and tree['/Count'] == len(tree['/Kids']), 'FLAT_PAGE_COUNT')
    markers, seen = [], set()
    for reference in tree['/Kids']:
        page = value(reference)
        require(reference not in seen, 'DUPLICATE_PAGE_REFERENCE')
        seen.add(reference)
        require({'/Type', '/Parent', '/MediaBox', '/Contents', '/Resources'} <= set(page)
                and set(page) <= {'/Type', '/Parent', '/MediaBox', '/Contents', '/Resources', '/Rotate', '/Group'}, 'PAGE_PROFILE')
        if '/Group' in page:
            exact(page['/Group'], ['/CS', '/S'])
            require(page['/Group']['/CS'] == '/DeviceRGB' and
                    page['/Group']['/S'] == '/Transparency', 'GROUP_PROFILE')
        require(page['/Type'] == '/Page' and page['/Parent'] == catalog['/Pages']
                and page['/MediaBox'] == [0, 0, 612, 792]
                and all(type(n) in (int, float) for n in page['/MediaBox'])
                and type(page.get('/Rotate', 0)) is int and page.get('/Rotate', 0) == 0, 'PAGE_GEOMETRY')
        resources = dictionary(page['/Resources'])
        exact(resources, ['/Font'])
        fonts = dictionary(resources['/Font'])
        exact(fonts, ['/F1'])
        font = dictionary(fonts['/F1'])
        require(type(font) is dict and {'/Type', '/Subtype', '/BaseFont'} <= set(font)
                and set(font) <= {'/Type', '/Subtype', '/BaseFont', '/Encoding'}
                and font['/Type'] == '/Font' and font['/Subtype'] == '/Type1'
                and font['/BaseFont'] == '/Helvetica'
                and font.get('/Encoding', '/WinAnsiEncoding') == '/WinAnsiEncoding', 'FONT_PROFILE')
        contents = page['/Contents']
        if type(contents) is list:
            require(len(contents) == 1, 'MULTIPLE_CONTENT_STREAMS')
            contents = contents[0]
        stream = indirect(contents)
        exact(stream, ['stream'])
        stream = stream['stream']
        exact(stream, ['dict', 'data'])
        exact(stream['dict'], [])  # Reject residual filters, external data, parameters.
        require(type(stream['data']) is str and 0 < len(stream['data']) <= 1024, 'STREAM_BOUND')
        try:
            content = base64.b64decode(stream['data'], validate=True).decode('ascii')
        except (ValueError, UnicodeError):
            raise Rejected('STREAM_ENCODING') from None
        match = re.fullmatch(r'\s*BT\s+/F1\s+24\s+Tf\s+72\s+720\s+Td\s+\((W0H-[ABC]-P[123])\)\s+Tj\s+ET\s*', content)
        require(match is not None, 'CONTENT_PROFILE')
        markers.append(match.group(1))
    expected = [marker for source in CASES[case] for marker in SOURCES[source]]
    require(markers == expected, 'ORDER_OR_MARKER_MISMATCH')
    return markers


def safe_path(path):
    path = Path(path).absolute()
    require(not any(p.is_symlink() or p.is_junction() for p in [path, *path.parents]), 'LINK_PATH')
    return path


def bounded(path, maximum=LIMIT):
    path = safe_path(path)
    require(path.is_file() and 0 < path.stat().st_size <= maximum, 'FILE_BOUND')
    with path.open('rb') as handle:
        data = handle.read(maximum + 1)
    require(0 < len(data) <= maximum, 'FILE_BOUND')
    return data


def validate_binding(binding, case):
    exact(binding, ['schema', 'case', 'merger', 'sourceSha256', 'outputSha256', 'qpdfFiles'])
    require(type(binding['schema']) is int and binding['schema'] == 1
            and type(case) is str and binding['case'] == case and case in CASES, 'BINDING_SCOPE')
    require(binding['merger'] == 'PDFsharp/6.2.4', 'INDEPENDENCE_SCOPE')
    exact(binding['sourceSha256'], SOURCES)
    require(all(valid_hash(h) for h in binding['sourceSha256'].values())
            and valid_hash(binding['outputSha256']), 'INVALID_HASH')
    files = binding['qpdfFiles']
    require(type(files) is dict and 2 <= len(files) <= 20
            and 'qpdf.exe' in files and 'qpdf30.dll' in files, 'BUNDLE_SCHEMA')
    for name, digest in files.items():
        require(type(name) is str and re.fullmatch(r'[A-Za-z0-9_.-]+', name)
                and name not in ('.', '..') and PurePosixPath(name).name == name
                and name.lower().endswith(('.exe', '.dll')) and valid_hash(digest), 'BUNDLE_MEMBER')
    require(len({n.lower() for n in files}) == len(files), 'BUNDLE_CASE_ALIAS')


def verify_files(binding, bundle, source_dir, output):
    bundle, source_dir, output = map(safe_path, (bundle, source_dir, output))
    require(bundle.is_dir() and source_dir.is_dir(), 'INPUT_DIRECTORY')
    require({p.name for p in bundle.iterdir()} == set(binding['qpdfFiles']), 'BUNDLE_INVENTORY')
    for name, expected in binding['qpdfFiles'].items():
        require(sha(bounded(bundle / name, 67108864)) == expected, 'TOOL_HASH')
    for name, expected in binding['sourceSha256'].items():
        source = source_dir / (name + '.pdf')
        require(not source.samefile(output), 'OUTPUT_SOURCE_ALIAS')
        require(sha(bounded(source, 65536)) == expected, 'SOURCE_HASH')
    require(sha(bounded(output, 262144)) == binding['outputSha256'], 'OUTPUT_HASH')


def checked_capture(code, stdout, stderr):
    require(type(code) is int and code == 0, 'QPDF_NONZERO')
    require(type(stdout) is bytes and 0 < len(stdout) <= LIMIT and stdout.strip(), 'EMPTY_OR_OVERSIZE_EVIDENCE')
    require(type(stderr) is bytes and len(stderr) == 0, 'QPDF_STDERR')
    return stdout


def capture(executable, arguments, directory, label):
    """Root's outer guest watchdog remains mandatory; kills this qpdf child only."""
    out, err = directory / (label + '.stdout'), directory / (label + '.stderr')
    started = time.monotonic()
    with out.open('xb') as stdout, err.open('xb') as stderr:
        system_root = os.environ.get('SystemRoot', '')
        require(bool(system_root), 'SYSTEM_ROOT_MISSING')
        child_environment = {'SystemRoot': system_root, 'WINDIR': system_root,
                             'PATH': str(Path(system_root) / 'System32'),
                             'TEMP': str(directory), 'TMP': str(directory)}
        process = subprocess.Popen([str(executable), *arguments], cwd=directory,
                                   stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr,
                                   shell=False, env=child_environment,
                                   creationflags=getattr(subprocess, 'CREATE_NO_WINDOW', 0))
        try:
            while process.poll() is None:
                require(time.monotonic() - started < 15, 'QPDF_TIMEOUT')
                require(out.stat().st_size <= LIMIT and err.stat().st_size <= 65536, 'CAPTURE_LIMIT')
                time.sleep(0.05)
        finally:
            if process.poll() is None:
                process.kill()
            process.wait(timeout=5)
    raw = bounded(out)
    require(err.stat().st_size <= 65536, 'CAPTURE_LIMIT')
    error = err.read_bytes()
    checked_capture(process.returncode, raw, error)
    return raw, {'exitCode': process.returncode, 'stdoutSha256': sha(raw),
                 'stderrSha256': sha(error), 'elapsedSeconds': round(time.monotonic() - started, 3)}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('binding', 'binding-sha256', 'bundle', 'sources', 'output', 'evidence'):
        parser.add_argument('--' + name, required=True)
    args = parser.parse_args()
    require(os.name == 'nt', 'WINDOWS_ONLY')
    require(valid_hash(args.binding_sha256), 'INVALID_BINDING_HASH')
    raw = bounded(args.binding, 65536)
    require(sha(raw) == args.binding_sha256, 'BINDING_HASH')
    binding = parse(raw)
    require(type(binding) is dict, 'BINDING_SCHEMA')
    case = binding.get('case')
    validate_binding(binding, case)
    verify_files(binding, args.bundle, args.sources, args.output)
    directory = safe_path(args.evidence)
    # A fresh private guest directory; no success reuse after any failure.
    directory.mkdir(exist_ok=False)
    executable = safe_path(args.bundle) / 'qpdf.exe'
    output = safe_path(args.output)
    version, version_record = capture(executable, ['--version'], directory, 'version')
    require(version.decode('ascii').splitlines()[0] == 'qpdf version 12.4.1', 'TOOL_VERSION')
    observed, observation_record = capture(executable, [*JSON_ARGS, str(output)], directory, 'observation')
    markers = inspect_json(observed, case)
    verify_files(binding, args.bundle, args.sources, args.output)
    require(sha(bounded(args.binding, 65536)) == args.binding_sha256, 'BINDING_CHANGED')
    result = {'schema': 1, 'verdict': 'BOUNDED_STRUCTURAL_MATCH', 'case': case,
              'observer': 'qpdf/12.4.1', 'merger': binding['merger'],
              'pageCount': len(markers), 'pageMarkers': markers,
              'bindingSha256': args.binding_sha256, 'outputSha256': binding['outputSha256'],
              'sourceSha256': binding['sourceSha256'], 'qpdfFiles': binding['qpdfFiles'],
              'observerScriptSha256': sha(bounded(__file__)),
              'versionCapture': version_record, 'observationCapture': observation_record,
              'observerArguments': [*JSON_ARGS, '<bound-output>'],
              'fidelity': 'NOT_ASSESSED', 'provenance': 'ROOT_LEDGER_REVIEW_REQUIRED'}
    with (directory / 'result.json').open('x', encoding='utf-8') as handle:
        json.dump(result, handle, indent=2)
    print('BOUNDED_STRUCTURAL_MATCH; ROOT_LEDGER_REVIEW_REQUIRED; FIDELITY_NOT_ASSESSED')


if __name__ == '__main__':
    try:
        main()
    except Rejected as error:
        print('REJECTED:' + str(error))
        raise SystemExit(1)
    except (OSError, ValueError, TypeError, KeyError, IndexError, subprocess.SubprocessError):
        print('REJECTED:OBSERVATION_FAILED')
        raise SystemExit(1)
