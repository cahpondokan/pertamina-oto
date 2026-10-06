"""Bounded independent tests on an exact extracted candidate, without a build."""
import argparse
import hashlib
import json
import os
import subprocess
import zipfile
from pathlib import Path
from verify_zip import inspect

parser = argparse.ArgumentParser()
parser.add_argument('zip', type=Path)
parser.add_argument('--sha256', required=True)
parser.add_argument('--output', type=Path, required=True)
args = parser.parse_args()
args.zip = args.zip.resolve()
args.output = args.output.resolve()
report = inspect(args.zip)
if report['zipSha256'] != args.sha256.upper() or report['failures'] or not report['manifest']:
    raise SystemExit('Candidate binding/structure/manifest failed')
if args.output.exists():
    raise SystemExit('Use a fresh immutable evidence directory')
args.output.mkdir(parents=True)
(args.output / 'inventory.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
extract = args.output / 'Ekstraksi uji 文档'
with zipfile.ZipFile(args.zip) as archive:
    # Explicit extended-path extraction avoids changing the host long-path policy.
    archive.extractall('\\\\?\\' + str(extract) if os.name == 'nt' else extract)
root = extract / 'PERTAMINA-OTO'
checks = []
env = os.environ.copy()
# Framework-dependent candidate: disclose installed-runtime use; no developer CLI on PATH.
for key in list(env):
    if key.startswith('DOTNET_') or key.startswith('COREHOST_'):
        del env[key]
env['PATH'] = str(Path(env['WINDIR']) / 'System32')
env['TEMP'] = env['TMP'] = str(args.output / 'owned-temp')
Path(env['TEMP']).mkdir()

def check(ok, name):
    checks.append({'name': name, 'passed': bool(ok)})
    if not ok:
        raise RuntimeError(name)

def launch_verify():
    return subprocess.run([str(root / 'PERTAMINA-OTO.exe'), '--verify'], cwd=root,
                          env=env, timeout=30, capture_output=True).returncode

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()

try:
    check(launch_verify() == 0, 'exact extracted launcher --verify succeeds in Unicode/spaces path')
    marker = root / 'app/portable-profile.json'
    original = marker.read_bytes()
    marker.write_bytes(original + b' ')
    check(launch_verify() == 1, 'changed marker rejected')
    marker.write_bytes(original)
    renamed = marker.with_suffix('.qa-held')
    marker.rename(renamed)
    try:
        check(launch_verify() == 1, 'missing marker rejected')
    finally:
        renamed.rename(marker)
    check(launch_verify() == 0, 'restored exact marker accepted')
    worker = root / 'app/worker/Pertamina.Oto.LocalPdf.Worker.exe'
    fixture = args.output / 'synthetic source.pdf'
    objects = [b'<< /Type /Catalog /Pages 2 0 R >>',
               b'<< /Type /Pages /Kids [3 0 R] /Count 1 >>',
               b'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 400 600] /Resources <<>> >>']
    pdf, offsets = b'%PDF-1.4\n', [0]
    for i, obj in enumerate(objects, 1):
        offsets.append(len(pdf))
        pdf += str(i).encode() + b' 0 obj\n' + obj + b'\nendobj\n'
    xref = len(pdf)
    pdf += b'xref\n0 4\n0000000000 65535 f \n'
    pdf += b''.join(f'{o:010d} 00000 n \n'.encode() for o in offsets[1:])
    pdf += f'trailer\n<< /Size 4 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n'.encode()
    fixture.write_bytes(pdf)
    before = digest(fixture)
    def request(tool, inputs, output):
        command = {'Tool': tool, 'Inputs': [str(p) for p in inputs], 'Output': str(output)}
        result = subprocess.run([str(worker)], input=json.dumps(command) + '\n', text=True,
                                capture_output=True, cwd=worker.parent, env=env, timeout=30)
        return result.returncode, json.loads(result.stdout)
    merged = args.output / 'merged.pdf'
    code, reply = request('merge', [fixture, fixture], merged)
    check(code == 0 and reply['Success'] and reply['Pages'] == 2 and merged.stat().st_size == reply['Bytes'],
          'extracted worker merges two synthetic pages with bounded PATH')
    invalid = args.output / 'invalid.pdf'
    invalid.write_bytes(b'Synthetic invalid input')
    failed = args.output / 'must-not-exist.pdf'
    code, reply = request('compress', [invalid], failed)
    check(code != 0 and not reply['Success'] and not failed.exists(), 'invalid input publishes no output')
    check(digest(fixture) == before, 'source SHA256 preserved')
    check(not (root / 'Data').exists(), 'verify and direct worker do not create application profile')
finally:
    result = {'zipSha256': report['zipSha256'], 'manifest': report['manifest'], 'checks': checks,
              'workerSha256': digest(root / 'app/worker/Pertamina.Oto.LocalPdf.Worker.exe'),
              'runtime': 'installed runtime; candidate framework-dependent',
              'limitations': ['No normal GUI startup or Save As/cancel UAT', 'No clean-PC/SAC proof',
                              'No network-denied runtime proof', 'Direct worker does not test desktop publication/cancellation']}
    (args.output / 'results.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
print(json.dumps(result))
