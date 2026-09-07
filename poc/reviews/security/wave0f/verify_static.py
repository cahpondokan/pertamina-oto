import hashlib,json,re,stat,struct,sys,zipfile
from pathlib import Path
root=Path(sys.argv[1]);src=Path(sys.argv[2]);p=root/'qpdf-12.4.1-msvc64.zip'
h='3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6'
b=p.read_bytes();assert len(b)==28165367 and hashlib.sha256(b).hexdigest()==h
manifest=json.loads((src/'intake-manifest.json').read_text(encoding='utf-8-sig'))
for r in manifest['rawEvidence']:
 d=(root/r['name']).read_bytes();assert len(d)==r['bytes'] and hashlib.sha256(d).hexdigest()==r['sha256']
prior=json.loads((root/'archive-static-inventory.json').read_text());old={x['path']:x for x in prior['entries']};oldpe={x['path']:x for x in prior['pe']}
entries=[];pes=[];activation=[]
with zipfile.ZipFile(p) as z:
 names=z.namelist();assert len(names)==len(set(n.casefold() for n in names)) and set(names)==set(old)
 for e in z.infolist():
  n=e.filename;assert not n.startswith('/') and '\\' not in n and ':' not in n and not e.flag_bits&1
  assert all(x not in ('.','..') and not x.endswith((' ','.')) for x in n.split('/') if x)
  assert not stat.S_ISLNK(e.external_attr>>16)
  d=z.read(e);dh=hashlib.sha256(d).hexdigest();assert dh==old[n]['sha256'] and len(d)==old[n]['bytes']
  entries.append({'path':n,'bytes':len(d),'sha256':dh})
  if n.lower().endswith(('.exe','.dll','.js','.cmd','.bat','.ps1','.vbs','.lib')):activation.append(n)
  if not n.lower().endswith(('.exe','.dll')):continue
  pe=struct.unpack_from('<I',d,60)[0];assert d[:2]==b'MZ' and d[pe:pe+4]==b'PE\0\0'
  u32=lambda at:struct.unpack_from('<I',d,at)[0]
  u16=lambda at:struct.unpack_from('<H',d,at)[0]
  op=pe+24;dr=op+(112 if u16(op)==523 else 96);assert u16(op) in (267,523)
  sections=[]
  for i in range(u16(pe+6)):
   at=op+u16(pe+20)+40*i;sections.append((u32(at+12),u32(at+16),u32(at+20)))
  def off(rva):
   for va,size,start in sections:
    if va<=rva<va+size:return start+rva-va
   raise ValueError('UNMAPPED_RVA')
  def name(rva):
   at=off(rva);return d[at:d.index(b'\0',at)].decode('ascii')
  imports=[];delay=[]
  if u32(dr+8):
   at=off(u32(dr+8))
   for i in range(u32(dr+12)//20):
    row=struct.unpack_from('<IIIII',d,at+20*i)
    if not any(row):break
    imports.append(name(row[3]))
  if u32(dr+104):
   at=off(u32(dr+104))
   for i in range(u32(dr+108)//32):
    row=struct.unpack_from('<IIIIIIII',d,at+32*i)
    if not any(row):break
    delay.append(name(row[1]) if row[0]&1 else 'UNRESOLVED_VA')
  co,cs=struct.unpack_from('<II',d,dr+32);assert not cs or co+cs<=len(d)
  ch=hashlib.sha256(d[co:co+cs]).hexdigest() if cs else None
  o=oldpe[n];assert sorted(imports)==o['imports'] and sorted(delay)==o['delayImports'] and cs==o['certificateTableBytes'] and ch==o['certificateTableSha256']
  strings=[s.decode('ascii') for s in re.findall(rb'[\x20-\x7e]{6,}',d)]+[s.decode('utf-16le') for s in re.findall(rb'(?:[\x20-\x7e]\x00){6,}',d)]
  keys=['OpenSSL','zlib','libjpeg','GnuTLS','WinHttp','WinInet','LoadLibrary','CreateProcess','ShellExecute','GetProcAddress','password','JavaScript']
  pes.append({'path':n,'machine':hex(u16(pe+4)),'imports':sorted(imports),'delayImports':sorted(delay),'certificateTableBytes':cs,'certificateTableSha256':ch,'indicators':{k:sum(k.lower() in s.lower() for s in strings) for k in keys}})
assert hashlib.sha256(p.read_bytes()).hexdigest()==h
print(json.dumps({'sha256Before':h,'sha256After':h,'bytes':len(b),'entryCount':len(entries),'entryMatch':True,'peMatch':True,'rawEvidenceBinding':True,'entries':entries,'pe':pes,'activationPaths':activation,'method':'Independent byte-only ZIP/PE/XML-free inspection; no extraction/load/execution; certificate tables not cryptographically verified'},indent=2))
