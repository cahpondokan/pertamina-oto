import base64,hashlib,json,re,stat,struct,sys,zipfile
from pathlib import Path
import xml.etree.ElementTree as ET
raw=Path(sys.argv[1]); source=Path(sys.argv[2])
pins={'microsoft.extensions.logging.abstractions.8.0.3':(802188,'E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74'),'system.security.cryptography.pkcs.8.0.1':(1078389,'28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5'),'microsoft.extensions.dependencyinjection.abstractions.8.0.2':(278862,'51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606')}
patterns={'interop':r'DllImport|LibraryImport|Marshal|crypt32\.dll|bcrypt\.dll|ncrypt\.dll|kernel32\.dll','network':r'HttpClient|WebRequest|Socket|System\.Net','process':r'ProcessStartInfo|ShellExecute|CreateProcess','dynamic':r'AssemblyLoadContext|LoadFrom|LoadLibrary|GetProcAddress|Reflection\.Emit','unsafe':r'Unsafe|Unmanaged|AllocHGlobal|IntPtr','generator':r'IIncrementalGenerator|ISourceGenerator|GeneratorDriver','telemetry':r'Telemetry|ApplicationInsights'}
def peinfo(d):
 pe=struct.unpack_from('<I',d,60)[0]; assert d[:2]==b'MZ' and d[pe:pe+4]==b'PE\0\0'
 count=struct.unpack_from('<H',d,pe+6)[0]; size=struct.unpack_from('<H',d,pe+20)[0]; op=pe+24
 magic=struct.unpack_from('<H',d,op)[0]; assert magic in (267,523)
 dr=op+(96 if magic==267 else 112); sections=[]
 for i in range(count):
  s=op+size+40*i; vs,va,rs,rp=struct.unpack_from('<IIII',d,s+8);sections.append((va,max(vs,rs),rp))
 def offset(rva):
  for va,n,rp in sections:
   if va<=rva<va+n:return rp+rva-va
  raise ValueError('RVA_OUTSIDE_SECTIONS')
 imp,ilen=struct.unpack_from('<II',d,dr+8); imports=[]
 if imp:
  at=offset(imp)
  for i in range(ilen//20):
   row=struct.unpack_from('<IIIII',d,at+20*i)
   if not any(row):break
   n=offset(row[3]);imports.append(d[n:d.index(b'\0',n)].decode('ascii'))
 cli=struct.unpack_from('<II',d,dr+112)
 strings=[s.decode('ascii') for s in re.findall(rb'[\x20-\x7e]{4,}',d)]+[s.decode('utf-16le') for s in re.findall(rb'(?:[\x20-\x7e]\x00){4,}',d)]
 return {'cliHeader':bool(all(cli)),'peImportDlls':imports,'indicators':{k:sorted(set(m.group(0) for s in strings for m in re.finditer(v,s,re.I))) for k,v in patterns.items()}}
out=[]
for name,(size,h) in pins.items():
 p=raw/(name+'.nupkg');b=p.read_bytes();assert len(b)==size and hashlib.sha256(b).hexdigest().upper()==h
 receipt=json.loads((raw/(name+'.acquisition.json')).read_text(encoding='utf-8-sig'))
 sha512=base64.b64encode(hashlib.sha512(b).digest()).decode();assert sha512==receipt['catalogSha512']
 inventory=json.loads((source/(name+'.inventory.json')).read_text(encoding='utf-8-sig'));old={x['name']:x for x in inventory}
 entries=[];targets=[];deps=[]
 with zipfile.ZipFile(p) as z:
  names=z.namelist(); assert len(names)==len(set(n.casefold() for n in names)) and set(names)==set(old)
  for e in z.infolist():
   n=e.filename;assert not n.startswith('/') and '\\' not in n and ':' not in n
   assert all(x not in ('.','..') and not x.endswith((' ','.')) for x in n.split('/') if x)
   assert not stat.S_ISLNK(e.external_attr>>16)
   d=z.read(e);digest=hashlib.sha256(d).hexdigest().upper();assert old[n]['sha256']==digest and old[n]['bytes']==len(d)
   row={'name':n,'sha256':digest,'bytes':len(d)}
   if n.endswith('.dll'):row['pe']=peinfo(d)
   if n.endswith(('.targets','.props')):
    assert b'<!DOCTYPE' not in d.upper();t=ET.fromstring(d)
    targets.append({'name':n,'tags':sorted(set(x.tag for x in t.iter())),'rootAttributes':t.attrib,'targets':[x.attrib for x in t.iter('Target')],'imports':[x.attrib for x in t.iter('Import')],'usingTasks':[x.attrib for x in t.iter('UsingTask')],'execs':[x.attrib for x in t.iter('Exec')]})
   if n.endswith('.nuspec'):
    t=ET.fromstring(d);groups=[g for g in t.iter() if g.tag.endswith('group') and g.attrib.get('targetFramework')=='net8.0'];assert len(groups)==1;deps=[x.attrib for x in groups[0]]
   entries.append(row)
 assert hashlib.sha256(p.read_bytes()).hexdigest().upper()==h
 out.append({'package':name,'sha256Before':h,'sha256After':h,'bytes':size,'catalogSha512Match':True,'archiveInventoryMatch':True,'selectedDependencies':deps,'targets':targets,'entries':entries})
print(json.dumps({'method':'Byte-only ZIP/PE import-directory/string/XML inspection; no assembly loading or IL/ImplMap/callgraph decoding','packages':out},indent=2))
