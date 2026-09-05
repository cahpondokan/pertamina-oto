import hashlib,json,re,stat,struct,sys,zipfile
from pathlib import Path
p=Path(sys.argv[1]); expected='5647882339b327cbf6da9ea3f882efb335f8d602e6fd45323adf9e78da53cf67'
b=p.read_bytes(); assert len(b)==4625326 and hashlib.sha256(b).hexdigest()==expected
patterns={'interop':'DllImport|LibraryImport|NativeLibrary|Marshal|kernel32|gdi32|user32|PInvoke','network':'HttpClient|WebClient|WebRequest|Socket|System.Net|Download|Upload','process':'ProcessStartInfo|ShellExecute|CreateProcess','dynamic':'AssemblyLoadContext|LoadFrom|LoadLibrary|GetProcAddress|Reflection.Emit','telemetry_activation':'Telemetry|ApplicationInsights|Activation|LicenseKey','unsafe':'Unsafe|Unmanaged|IntPtr|AllocHGlobal|MemoryCopy','active_pdf':'JavaScript|OpenAction|Launch|EmbeddedFile|SubmitForm|GoToR|URI'}
rows=[]
with zipfile.ZipFile(p) as z:
 names=[e.filename.casefold() for e in z.infolist()]; assert len(names)==len(set(names))
 for e in z.infolist():
  assert not e.filename.startswith('/') and '\\' not in e.filename and ':' not in e.filename
  assert all(x not in ('.','..') and not x.endswith((' ','.')) for x in e.filename.split('/') if x)
  assert not stat.S_ISLNK(e.external_attr>>16)
  d=z.read(e); r={'name':e.filename,'bytes':len(d),'sha256':hashlib.sha256(d).hexdigest(),'nativeBuildScriptPath':bool(re.search(r'(^runtimes/|/native/|\.(exe|so|dylib|ps1|cmd|bat|targets|props|js|vbs)$)',e.filename,re.I))}
  if e.filename.endswith('.dll'):
   assert d[:2]==b'MZ'; pe=struct.unpack_from('<I',d,60)[0]; assert d[pe:pe+4]==b'PE\0\0'
   op=pe+24; magic=struct.unpack_from('<H',d,op)[0]; dr=op+(96 if magic==267 else 112)
   r['cliHeaderPresent']=all(struct.unpack_from('<II',d,dr+112))
   strings=[s.decode('ascii') for s in re.findall(rb'[\x20-\x7e]{4,}',d)]+[s.decode('utf-16le') for s in re.findall(rb'(?:[\x20-\x7e]\x00){4,}',d)]
   r['indicators']={k:sorted(set(m.group(0) for s in strings for m in re.finditer(v,s,re.I))) for k,v in patterns.items()}
  rows.append(r)
assert hashlib.sha256(p.read_bytes()).hexdigest()==expected
print(json.dumps({'sha256Before':expected,'sha256After':expected,'bytes':len(b),'archiveChecks':'PASS','signatureValidation':'NOT_RUN','assemblyExecution':False,'method':'PE CLI directory and string vocabulary only; no IL/callgraph/PInvoke-table decoding','entries':rows},indent=2))
