"""Native library smoke, only on the QA extracted package with intact marker."""
import argparse
import hashlib
import json
import os
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('root', type=Path)
parser.add_argument('--output', type=Path, required=True)
parser.add_argument('--reader', action='store_true')
args = parser.parse_args()
root = args.root.resolve()
host = root / 'app/Pertamina.Oto.Desktop.App.exe'
marker = root / 'app/portable-profile.json'
if json.loads(marker.read_text()) != {'schema': 1, 'dataFolder': 'Data'}:
    raise SystemExit('Intact portable marker required')
env = os.environ.copy()
for key in list(env):
    if key.startswith('DOTNET_') or key.startswith('COREHOST_'):
        del env[key]
env['PATH'] = str(Path(env['WINDIR']) / 'System32')
result = {'hostSha256': hashlib.sha256(host.read_bytes()).hexdigest().upper(),
          'hostAssemblySha256': hashlib.sha256(host.with_suffix('.dll').read_bytes()).hexdigest().upper(),
          'command': '--audit-reader-ui' if args.reader else '--self-test-library', 'runtime': 'installed host runtime'}
try:
    process = subprocess.run([str(host), result['command']], cwd=host.parent,
                             env=env, timeout=120 if args.reader else 45, capture_output=True)
    result['exitCode'] = process.returncode
    report = next(host.parent.glob('reader-ui-audit-*/audit.txt'), None) if args.reader else host.parent / 'library-selftest.txt'
    result['report'] = report.read_text() if report is not None and report.exists() else None
    result['reportPath'] = str(report) if report else None
    result['passed'] = process.returncode == 0 and result['report'] is not None and 'FAIL' not in result['report']
except subprocess.TimeoutExpired:
    result.update(passed=False, timeout=True)
args.output.write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(result))
raise SystemExit(not result['passed'])
