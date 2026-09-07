"""Compare independently collected JSON observations; never parse PDFs."""
import hashlib
import json
from pathlib import Path
import sys


def read_json(path):
    path = Path(path)
    if path.stat().st_size > 65536:
        raise ValueError('Observation/spec exceeds 64 KiB')
    return json.loads(path.read_text(encoding='utf-8'))


def digest(path):
    # Benign smoke bound, not a production file size limit.
    path = Path(path)
    if path.stat().st_size > 1048576:
        raise ValueError('Smoke artifact exceeds 1 MiB')
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    if len(sys.argv) != 6:
        raise SystemExit('Usage: python oracle.py CASE SOURCE_DIR OUTPUT.pdf OBSERVATION.json RAW_EVIDENCE')
    case, source_dir, output, observation, evidence = sys.argv[1:]
    spec = read_json(Path(__file__).with_name('spec.json'))
    expected = spec['cases'][case]['expected']
    manifest = read_json(Path(source_dir) / 'sources.json')
    obs = read_json(observation)
    for name, markers in spec['sources'].items():
        if manifest[name]['markers'] != markers or digest(Path(source_dir) / f'{name}.pdf') != manifest[name]['sha256']:
            raise ValueError('Source invariant failed: ' + name)
    checks = [obs['case'] == case, obs['outputSha256'] == digest(output),
              obs['rawEvidenceSha256'] == digest(evidence),
              obs['pageCount'] == len(expected), obs['pageMarkers'] == expected,
              obs['reopened'] is True, bool(obs['observerTool']),
              bool(obs['observerVersion']), obs['independentOfMergeEngine'] is True]
    if not all(checks):
        raise ValueError('Structural observation mismatch')
    print('STRUCTURAL_OBSERVATION_MATCH: provenance requires reviewer audit; no fidelity or publication verdict.')


if __name__ == '__main__':
    try:
        main()
    except (ValueError, KeyError, OSError, TypeError) as error:
        print('FAIL:', error, file=sys.stderr)
        raise SystemExit(1)
