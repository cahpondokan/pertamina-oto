"""Read private anonymous diagnostics; never expose a public log/report endpoint.
Usage: python diagnostics_report.py --data-dir PRIVATE_DIR --output report.html
No network, account, dependency, log modification, or automatic retention purge.
"""
import argparse
from collections import Counter, defaultdict
from datetime import datetime, timezone
import html
import json
from pathlib import Path
import re

FEATURES = set('shell merge organize split images-pdf numbers watermark remove extract rotate crop'.split())
STAGES = set('input process save runtime cleanup preview cloud'.split())
EVENTS = set('started succeeded failed cancelled download_requested save_succeeded save_cancelled save_failed runtime_error'.split())
CODES = set('NONE UNKNOWN CANCELLED WORKER TIMEOUT VERIFY INVALID ENCRYPTED LIMIT SESSION_LIMIT TYPE SINGLE MERGE EMPTY SPLIT PARTITION RANGE OPTIONS TEXT LAYOUT IMAGE_FORMAT IMAGE_LIMIT IMAGE_BROWSER FILE_COUNT TOTAL_BYTES STORAGE_BROWSER STORAGE UNSUPPORTED UNSUPPORTED_FORM UNSUPPORTED_SIGNATURE UNSUPPORTED_ATTACHMENT UNSUPPORTED_ACTION UNSUPPORTED_ANNOTATIONS UNSUPPORTED_TAGGED UNSAFE_LINK LINK_TARGET ANNOTATION_OPERATION TAGGED_STRUCTURE TAGGED_OPERATION TAGGED_MIXED TAGGED_CONFLICT CLOUD_CONFIG CLOUD_LOAD CLOUD_AUTH CLOUD_DOWNLOAD CLOUD_RESPONSE CLOUD_CHANGED TYPE_ERROR RANGE_ERROR REFERENCE_ERROR SYNTAX_ERROR QUOTA NOT_ALLOWED NOT_READABLE SECURITY NETWORK SERVER_ERROR'.split())
BROWSERS = {'edge', 'firefox', 'chromium', 'safari', 'other'}
UUID = re.compile(r'[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}')
FIELDS = set('id processId event feature stage code elapsedMs occurredAt clientVersion browser'.split())

def valid(row):
    try:
        if set(row) != {'receivedAtUtc', 'source', 'serverVersion', 'data'}: return False
        e = row['data']
        return (set(e) == FIELDS and row['source'] in {'anonymous-client', 'server'}
                and re.fullmatch(r'[A-Za-z0-9_.-]{1,80}', row['serverVersion'])
                and datetime.fromisoformat(row['receivedAtUtc']).tzinfo is not None
                and UUID.fullmatch(e['id']) and UUID.fullmatch(e['processId'])
                and e['event'] in EVENTS and e['stage'] in STAGES and e['feature'] in FEATURES and e['code'] in CODES
                and e['browser'] in BROWSERS and e['clientVersion'] == 'web-audit-1'
                and type(e['elapsedMs']) is int and 0 <= e['elapsedMs'] <= 3600000
                and type(e['occurredAt']) is int and 0 <= e['occurredAt'] <= 4102444800000)
    except (KeyError, TypeError, ValueError, AttributeError): return False

def report(directory):
    path = Path(directory)/'events.jsonl'
    size = path.stat().st_size if path.exists() else 0
    if size > 16*1024*1024: raise ValueError('Log exceeds the supported capacity; operator review required.')
    rows, rejected, duplicates, seen = [], 0, 0, set()
    if path.exists():
        with path.open(encoding='utf-8') as stream:
            for line in stream:
                try: row = json.loads(line)
                except (json.JSONDecodeError, UnicodeError): rejected += 1; continue
                if not valid(row): rejected += 1; continue
                if row['data']['id'] in seen: duplicates += 1; continue
                seen.add(row['data']['id']); rows.append(row)
    counts = Counter(row['data']['event'] for row in rows)
    errors = Counter((row['data']['feature'], row['data']['stage'], row['data']['code'], row['serverVersion'], row['data']['browser'])
                     for row in rows if row['data']['event'] in {'failed', 'save_failed', 'runtime_error'})
    processes = defaultdict(set)
    for row in rows:
        e = row['data']; processes[(e['processId'], e['stage'])].add(e['event'])
    pending = sum('started' in events and not events.intersection({'succeeded', 'failed', 'cancelled'}) for events in processes.values())
    return {'generatedAtUtc': datetime.now(timezone.utc).isoformat(), 'events': len(rows), 'counts': dict(counts),
            'errors': [dict(zip(('feature','stage','code','release','browser'), key), count=value) for key,value in errors.most_common()],
            'startedWithoutTerminalReceived': pending, 'rejectedRows': rejected, 'duplicateRows': duplicates,
            'storageBytes': size, 'capacityBytes': 16*1024*1024, 'recordLimit': 32768,
            'lastReceivedAtUtc': max((r['receivedAtUtc'] for r in rows), default=None), 'recent': rows[-100:]}

def render(data):
    esc = lambda value: html.escape(str(value))
    errors = ''.join('<tr>'+''.join('<td>'+esc(row[k])+'</td>' for k in ('feature','stage','code','release','browser','count'))+'</tr>' for row in data['errors']) or '<tr><td colspan="6">Belum ada kesalahan yang diterima.</td></tr>'
    recent = ''.join('<tr>'+''.join('<td>'+esc(v)+'</td>' for v in (row['receivedAtUtc'],row['source'],row['data']['processId'],row['data']['feature'],row['data']['stage'],row['data']['event'],row['data']['code']))+'</tr>' for row in reversed(data['recent']))
    return f'''<!doctype html><html lang="id"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Monitor OTO Web</title>
<style>body{{font:16px system-ui;margin:2rem auto;padding:0 1rem;max-width:1200px;background:#f5f7fa;color:#182d3c}}h1,h2{{color:#175b66}}.cards{{display:flex;gap:1rem;flex-wrap:wrap}}.card{{background:white;padding:1rem;border-radius:12px;flex:1;min-width:180px}}strong{{font-size:28px;display:block}}table{{border-collapse:collapse;width:100%;background:white}}td,th{{text-align:left;border-bottom:1px solid #dde4ea;padding:10px;font-size:13px}}.table{{overflow:auto}}.notice{{background:#fff5d9;padding:1rem;border-radius:12px}}small{{color:#526471}}</style>
<h1>Monitor OTO Web</h1><p>Catatan aktivitas dan kesalahan anonim. Laporan lokal pengelola; tidak tersedia untuk pengunjung.</p>
<div class="cards"><div class="card">Event diterima<strong>{data['events']}</strong></div><div class="card">Proses berhasil<strong>{data['counts'].get('succeeded',0)}</strong><small>Termasuk pemeriksaan input</small></div><div class="card">Kesalahan dilaporkan<strong>{sum(e['count'] for e in data['errors'])}</strong></div><div class="card">Belum menerima hasil akhir<strong>{data['startedWithoutTerminalReceived']}</strong></div></div>
<p class="notice">Laporan browser belum terverifikasi dan tidak membuktikan identitas orang. Catatan dapat hilang ketika tab ditutup, jaringan gagal, atau kapasitas penuh. Proses tanpa hasil akhir belum tentu gagal. Unduhan diminta tidak berarti file sudah tersimpan.</p>
<p>Terakhir diterima (UTC): {esc(data['lastReceivedAtUtc'] or 'belum ada')} · Pemakaian: {data['storageBytes']:,} / {data['capacityBytes']:,} byte; maksimum {data['recordLimit']:,} event. Baris rusak/ditolak: {data['rejectedRows']}. Duplikat: {data['duplicateRows']}.</p>
<h2>Kelompok kesalahan</h2><div class="table"><table><thead><tr><th>Fitur</th><th>Tahap</th><th>Kode</th><th>Rilis</th><th>Browser</th><th>Jumlah</th></tr></thead><tbody>{errors}</tbody></table></div>
<h2>100 kejadian terakhir</h2><div class="table"><table><thead><tr><th>Diterima (UTC)</th><th>Sumber</th><th>ID proses</th><th>Fitur</th><th>Tahap</th><th>Kejadian</th><th>Kode</th></tr></thead><tbody>{recent}</tbody></table></div>
<p><small>Dibuat {esc(data['generatedAtUtc'])}. Muat ulang data dengan menjalankan kembali alat laporan. Tidak ada isi PDF, nama/path file, akun, atau stack mentah dalam laporan.</small></p></html>'''

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--data-dir',required=True); parser.add_argument('--output',required=True)
    args=parser.parse_args(); data=report(args.data_dir); target=Path(args.output)
    target.write_text(json.dumps(data,indent=2) if target.suffix=='.json' else render(data),encoding='utf-8')
    print(json.dumps({k:v for k,v in data.items() if k not in {'recent','errors'}},indent=2))

if __name__=='__main__': main()
