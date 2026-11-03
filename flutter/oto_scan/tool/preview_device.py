import subprocess,time
from device_qa import dump,tap,adb,ROOT,PKG
p=subprocess.Popen(['adb','shell','am','instrument','-w','-e','mode','preview',PKG+'.test/id.ayani.oto.oto_scan.ScanInstrumentation'],stdout=subprocess.PIPE,stderr=subprocess.STDOUT,text=True)
time.sleep(1)
nodes=dump()
if any(n['desc']=='Tambah' and n['package']==PKG for n in nodes):tap('Tambah')
output=p.communicate(timeout=25)[0]
print(output)
if 'evidence saved' not in output:raise SystemExit(1)
for filename in ['preview.png','preview-screen.png','preview-geometry.json']:
    destination=ROOT/'evidence'/('device/r03-'+filename if filename.endswith('.png') else 'r03-'+filename)
    adb('pull','/sdcard/Android/data/'+PKG+'/files/qa/'+filename,str(destination))
