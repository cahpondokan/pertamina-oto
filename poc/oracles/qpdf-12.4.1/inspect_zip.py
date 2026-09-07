"""Read exact approved ZIP in memory. No archive member is extracted or loaded."""
import hashlib,json,re,stat,struct,zipfile
from pathlib import Path,PurePosixPath
raw=Path(r'D:\OTO\pertamina-oto\artifacts\evidence\P02-20260905-1434\agent-3-qpdf-intake')
asset=raw/'qpdf-12.4.1-msvc64.zip'
assert hashlib.sha256(asset.read_bytes()).hexdigest()=='3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6'
entries=[];pes=[];seen=set();flags=[]
with zipfile.ZipFile(asset) as z:
 for info in z.infolist():
  n=info.filename;p=PurePosixPath(n)
  if p.is_absolute() or '..' in p.parts or '\\' in n or ':' in n or n.lower() in seen or stat.S_ISLNK(info.external_attr>>16): raise ValueError('STOP_UNSAFE_ZIP_PATH')
  seen.add(n.lower())
  if info.flag_bits&1: raise ValueError('STOP_ENCRYPTED_ENTRY')
  b=b'' if info.is_dir() else z.read(info)
  entries.append({'path':n,'bytes':len(b),'compressedBytes':info.compress_size,'sha256':hashlib.sha256(b).hexdigest(),'directory':info.is_dir()})
  if n.lower().endswith(('.exe','.dll','.js','.cmd','.bat','.ps1','.vbs','.lib')): flags.append(n)
  if not n.lower().endswith(('.exe','.dll')): continue
  def u16(o): return struct.unpack_from('<H',b,o)[0]
  def u32(o): return struct.unpack_from('<I',b,o)[0]
  assert b[:2]==b'MZ';pe=u32(60);assert b[pe:pe+4]==b'PE\0\0'
  opt=pe+24;size=u16(pe+20);magic=u16(opt);assert magic in (0x10b,0x20b)
  dd=opt+(112 if magic==0x20b else 96)
  sections=[]
  for i in range(u16(pe+6)):
   s=opt+size+40*i;sections.append((u32(s+12),u32(s+8),u32(s+20),u32(s+16)))
  def offset(rva):
   for va,vs,start,rs in sections:
    if va<=rva<va+min(vs,rs):return start+rva-va
   raise ValueError('UNMAPPED_RVA')
  def name(rva):
   start=offset(rva);return b[start:b.index(0,start)].decode('ascii')
  imports=[];delay=[]
  if u32(dd+8):
   pos=offset(u32(dd+8))
   while any(b[pos:pos+20]):imports.append(name(u32(pos+12)));pos+=20
  if u32(dd+13*8):
   pos=offset(u32(dd+13*8))
   while any(b[pos:pos+32]):
    delay.append(name(u32(pos+4)) if u32(pos)&1 else 'UNRESOLVED_VA');pos+=32
  certOffset=u32(dd+32);certSize=u32(dd+36)
  strings=[s.decode('ascii') for s in re.findall(rb'[ -~]{6,}',b)]
  needles=['OpenSSL','zlib','libjpeg','GnuTLS','WinHttp','WinInet','LoadLibrary','CreateProcess','ShellExecute']
  pes.append({'path':n,'machine':hex(u16(pe+4)),'imports':sorted(imports),'delayImports':sorted(delay),'certificateTableBytes':certSize,'certificateTableSha256':hashlib.sha256(b[certOffset:certOffset+certSize]).hexdigest() if certSize else None,'signatureStatus':'NOT_VERIFIED_STATIC_TABLE_ONLY','staticStringIndicators':{k:sum(k.lower() in s.lower() for s in strings) for k in needles}})
result={'asset':asset.name,'zipSha256':hashlib.sha256(asset.read_bytes()).hexdigest(),'pathDuplicateSymlinkEncryptionChecks':'PASS','crcCheck':'PASS_BY_READING_ALL_ENTRIES','entryCount':len(entries),'entries':entries,'pe':pes,'activationSurfacePaths':flags,'noExtraction':True,'noExecution':True}
(raw/'archive-static-inventory.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8',newline='\n')
print('ENTRIES='+str(len(entries))+';PE='+str(len(pes))+';ACTIVATION_PATHS='+str(len(flags)))
for p in pes:print(json.dumps(p))
