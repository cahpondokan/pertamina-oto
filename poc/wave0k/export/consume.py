"""Read bounded JSON/text exports only. Never opens a PDF or runs an engine."""
import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import re
import stat

CASES = ('AB', 'CAB', 'AA', 'AC')
SUFFIXES = ('binding.json', 'result.json', 'version.stdout', 'version.stderr',
            'observation.stdout', 'observation.stderr')


def require(value, code):
    if not value:
        raise ValueError(code)


def sha(raw):
    return hashlib.sha256(raw).hexdigest()


def digest(value):
    return type(value) is str and re.fullmatch('[0-9a-f]{64}', value) is not None


def exact(value, keys):
    require(type(value) is dict and set(value) == set(keys), 'SCHEMA')


def safe(path):
    path = Path(path).absolute()
    for item in (path, *path.parents):
        info = item.lstat()
        require(not stat.S_ISLNK(info.st_mode) and not
                (getattr(info, 'st_file_attributes', 0) & 0x400), 'REPARSE_PATH')
    return path


def read(path, limit, empty=False):
    path = safe(path)
    info = path.stat()
    require(stat.S_ISREG(info.st_mode) and info.st_nlink == 1 and
            (0 if empty else 1) <= info.st_size <= limit, 'FILE_BOUND')
    with path.open('rb') as handle:
        raw = handle.read(limit + 1)
    require((0 if empty else 1) <= len(raw) <= limit, 'FILE_BOUND')
    return raw


def parse(raw):
    def pairs(items):
        out = {}
        for key, value in items:
            require(key not in out, 'DUPLICATE_KEY')
            out[key] = value
        return out
    return json.loads(raw.decode('utf-8'), object_pairs_hook=pairs,
                      parse_constant=lambda _: require(False, 'NONFINITE'))


def load_observer(expected_hash):
    # Fixed repository-owned source, never a filename from guest evidence.
    path = Path(__file__).resolve().parents[2] / 'wave0k/oracle/observe.py'
    require(sha(read(path, 1048576)) == expected_hash, 'OBSERVER_SOURCE_HASH')
    spec = importlib.util.spec_from_file_location('trusted_wave0i_observer', path)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)  # import is inert; main/capture never called
    return module


def validate(directory, expected, observer, cases=CASES):
    exact(expected, ('schema', 'observerScriptSha256', 'sourceSha256', 'qpdfFiles'))
    require(type(expected['schema']) is int and expected['schema'] == 1 and
            digest(expected['observerScriptSha256']), 'EXPECTED_SCHEMA')
    require(tuple(cases) == CASES or (len(cases) == 1 and cases[0] in CASES), 'CASE_SET')
    directory = safe(directory)
    require(directory.is_dir() and {p.name for p in directory.iterdir()} ==
            {c + '.' + s for c in cases for s in SUFFIXES}, 'INVENTORY')
    results = []
    hashes = {}
    for case in cases:
        raw = {}
        for suffix in SUFFIXES:
            name = case + '.' + suffix
            limit = 1048576 if suffix.endswith('stdout') else 65536
            raw[suffix] = read(directory / name, limit, suffix.endswith('stderr'))
            hashes[name] = sha(raw[suffix])
        binding, result = parse(raw['binding.json']), parse(raw['result.json'])
        observer.validate_binding(binding, case)
        require(binding['sourceSha256'] == expected['sourceSha256'] and
                binding['qpdfFiles'] == expected['qpdfFiles'], 'FROZEN_INPUT_DRIFT')
        exact(result, ('schema', 'verdict', 'case', 'observer', 'merger', 'pageCount',
                      'pageMarkers', 'bindingSha256', 'outputSha256', 'sourceSha256',
                      'qpdfFiles', 'observerScriptSha256', 'versionCapture',
                      'observationCapture', 'observerArguments', 'fidelity', 'provenance'))
        require(type(result['schema']) is int and result['schema'] == 1 and
                result['case'] == case and result['verdict'] == 'BOUNDED_STRUCTURAL_MATCH'
                and result['observer'] == 'qpdf/12.4.1' and result['merger'] == 'PDFsharp/6.2.4'
                and result['fidelity'] == 'NOT_ASSESSED'
                and result['provenance'] == 'ROOT_LEDGER_REVIEW_REQUIRED', 'RESULT_SCOPE')
        for field in ('sourceSha256', 'qpdfFiles', 'outputSha256'):
            require(result[field] == binding[field], 'RESULT_BINDING_DRIFT')
        require(result['bindingSha256'] == sha(raw['binding.json']) and
                result['observerScriptSha256'] == expected['observerScriptSha256'] and
                result['observerArguments'] == [*observer.JSON_ARGS, '<bound-output>'], 'RESULT_HASH_ARGS')
        for label in ('version', 'observation'):
            capture = result[label + 'Capture']
            exact(capture, ('exitCode', 'stdoutSha256', 'stderrSha256', 'elapsedSeconds'))
            elapsed = capture['elapsedSeconds']
            require(type(elapsed) in (int, float) and math.isfinite(elapsed) and
                    0 <= elapsed <= 30, 'CAPTURE_TIME')
            observer.checked_capture(capture['exitCode'], raw[label + '.stdout'], raw[label + '.stderr'])
            require(capture['stdoutSha256'] == sha(raw[label + '.stdout']) and
                    capture['stderrSha256'] == sha(raw[label + '.stderr']), 'CAPTURE_HASH')
        require(raw['version.stdout'].decode('ascii').splitlines()[0] == 'qpdf version 12.4.1', 'VERSION')
        markers = observer.inspect_json(raw['observation.stdout'], case)
        require(type(result['pageCount']) is int and result['pageCount'] == len(markers)
                and result['pageMarkers'] == markers, 'MARKER_RESULT')
        results.append({'case': case, 'pageCount': len(markers),
                        'outputSha256': binding['outputSha256']})
    return {'status': 'BOUNDED_EXPORT_CONSISTENT', 'cases': results, 'exportSha256': hashes,
            'provenance': 'ROOT_LEDGER_REVIEW_REQUIRED', 'fidelity': 'NOT_ASSESSED',
            'outputHashAuthority': 'GUEST_ASSERTED_OUTPUT_HASH',
            'runtimeAuthenticity': 'NOT_PROVEN_BY_EXPORT'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--directory', required=True)
    parser.add_argument('--expected', required=True)
    parser.add_argument('--expected-sha256', required=True)
    parser.add_argument('--case', choices=CASES)
    args = parser.parse_args()
    require(digest(args.expected_sha256), 'EXPECTED_HASH_FORMAT')
    raw = read(args.expected, 65536)
    require(sha(raw) == args.expected_sha256, 'EXPECTED_HASH')
    expected = parse(raw)
    exact(expected, ('schema', 'observerScriptSha256', 'sourceSha256', 'qpdfFiles'))
    observer = load_observer(expected['observerScriptSha256'])
    print(json.dumps(validate(args.directory, expected, observer,
                              (args.case,) if args.case else CASES), sort_keys=True))


if __name__ == '__main__':
    try:
        main()
    except (OSError, ValueError, TypeError, KeyError, IndexError, RecursionError):
        print('REJECTED:EXPORT_VALIDATION_FAILED')
        raise SystemExit(1)
