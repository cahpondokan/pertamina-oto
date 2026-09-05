"""Static PE import inventory using standard library; never loads a binary."""
import json, struct, sys
from pathlib import Path
root=Path(sys.argv[1]).resolve()
records=[]
for p in sorted((root/'Library'/'bin').iterdir()):
 if p.suffix.lower() not in ('.exe','.dll'): continue
 b=p.read_bytes()
 def u16(o): return struct.unpack_from('<H',b,o)[0]
 def u32(o): return struct.unpack_from('<I',b,o)[0]
 assert b[:2]==b'MZ'
 pe=u32(60); assert b[pe:pe+4]==b'PE\0\0'
 n=u16(pe+6); opt=pe+24; size=u16(pe+20); magic=u16(opt)
 assert magic in (0x10b,0x20b)
 dd=opt+(112 if magic==0x20b else 96)
 sections=[]
 for i in range(n):
  s=opt+size+40*i
  sections.append((u32(s+12),u32(s+8),u32(s+20),u32(s+16)))
 def offset(rva):
  for va,vs,raw,rs in sections:
   if va<=rva<va+min(vs,rs): return raw+rva-va
  raise ValueError('UNMAPPED_RVA')
 def name(rva):
  start=offset(rva); end=b.index(0,start)
  return b[start:end].decode('ascii')
 imports=[]; rva=u32(dd+8)
 if rva:
  pos=offset(rva)
  while any(b[pos:pos+20]):
   imports.append(name(u32(pos+12)));pos+=20
 delay=[]; rva=u32(dd+13*8)
 if rva:
  pos=offset(rva)
  while any(b[pos:pos+32]):
   if u32(pos)&1: delay.append(name(u32(pos+4)))
   else: delay.append('UNRESOLVED_VA_DESCRIPTOR')
   pos+=32
 records.append({'path':p.relative_to(root).as_posix(),'machine':hex(u16(pe+4)),'imports':sorted(imports),'delayImports':sorted(delay)})
out=Path(__file__).parent/'pe-imports.json'
out.write_text(json.dumps({'scope':'STATIC_IMPORTS_NOT_RUNTIME_LOAD_PROOF','files':records},indent=2)+'\n',encoding='utf-8')
print('PE_FILES='+str(len(records)))
for r in records:
 if r['path'].endswith(('pdfinfo.exe','pdftoppm.exe','poppler.dll')): print(json.dumps(r))
