"""Operate only the explicit synthetic-test emulator, never a connected phone."""
from pathlib import Path
import subprocess, sys, re, xml.etree.ElementTree as ET

root=Path(__file__).resolve().parents[1]
out=root/'evidence/mlkit-trial'
adb=Path('C:/Users/cahpo/AppData/Local/Android/sdk/platform-tools/adb.exe')
serial='emulator-5582'
package='com.niam.ayani.oto.uji'
def run(*args, timeout=40):
    p=subprocess.run([str(adb),'-s',serial,*map(str,args)],capture_output=True,timeout=timeout)
    if p.returncode: raise RuntimeError(p.stderr.decode(errors='replace'))
    return p.stdout
if run('shell','getprop','ro.kernel.qemu').strip()!=b'1': raise RuntimeError('Refusing non-emulator target')
action=sys.argv[1]
if action=='state':
    run('shell','uiautomator','dump','/sdcard/oto-mlkit-window.xml')
    xml=run('shell','cat','/sdcard/oto-mlkit-window.xml')
    (out/'window.xml').write_bytes(xml)
    for n in ET.fromstring(xml).iter('node'):
        text=n.get('text') or n.get('content-desc')
        if text: print(text, '|',n.get('resource-id'), '|',n.get('bounds'))
elif action=='tap':
    # Require a fresh observed hierarchy and exact text, description or id.
    run('shell','uiautomator','dump','/sdcard/oto-mlkit-window.xml')
    xml=run('shell','cat','/sdcard/oto-mlkit-window.xml')
    matches=[n for n in ET.fromstring(xml).iter('node') if sys.argv[2] in (n.get('text'),n.get('content-desc'),n.get('resource-id'))]
    if len(matches)!=1: raise RuntimeError(f'Expected one match, found {len(matches)}')
    x1,y1,x2,y2=map(int,re.findall(r'\d+',matches[0].get('bounds')))
    run('shell','input','tap',(x1+x2)//2,(y1+y2)//2)
elif action=='install':
    if run('shell','pm','list','packages',package).strip(): raise RuntimeError('Trial already installed; preserve emulator data')
    print(run('install','-g',root/'dist/OTO-Scan-Uji-MLKit-1.0.1.apk',timeout=90).decode())
    print(run('install','-t',root/'build/app/outputs/apk/androidTest/release/app-release-androidTest.apk',timeout=90).decode())
elif action=='replace':
    # Only this side-by-side synthetic trial package on the guarded emulator.
    if run('shell','pm','list','packages',package).decode().strip() not in (f'package:{package}',f'package:{package}\npackage:{package}.test',f'package:{package}.test\npackage:{package}'):
        raise RuntimeError('Unexpected installed trial identity')
    print(run('install','-r','-g',root/'dist/OTO-Scan-Uji-MLKit-1.0.1.apk',timeout=90).decode())
    print(run('install','-r','-t',root/'build/app/outputs/apk/androidTest/release/app-release-androidTest.apk',timeout=90).decode())
elif action=='test':
    mode=sys.argv[2]
    if mode not in ('native','mlkit-import','mlkit-ui','inspect'): raise RuntimeError('Unsupported mode')
    log=run('shell','am','instrument','-w','-e','mode',mode,package+'.test/id.ayani.oto.oto_scan.ScanInstrumentation',timeout=240).decode(errors='replace')
    (out/('emulator-'+mode+'.log')).write_text(log,encoding='utf-8');print(log)
    run('pull',f'/sdcard/Android/data/{package}/files/qa',out/'device-qa',timeout=60)
    if mode!='inspect' and 'checks PASS' not in log: raise RuntimeError('Test did not pass; inspect evidence')
elif action=='key': run('shell','input','keyevent',sys.argv[2])
elif action=='launch': print(run('shell','am','start','-n',package+'/id.ayani.oto.oto_scan.MainActivity').decode())
elif action=='screenshot':
    target=out/(sys.argv[2]+'.png');target.write_bytes(run('exec-out','screencap','-p'));print(target)
elif action=='fixtures':
    run('shell','mkdir','-p','/sdcard/Pictures/OTO_MLKit_Test')
    for name in ('portrait','landscape','card'):
        target=f'/sdcard/Pictures/OTO_MLKit_Test/{name}.jpg'
        run('push',root/f'evidence/mlkit-trial/fixtures/{name}.jpg',target)
        run('shell','am','broadcast','-a','android.intent.action.MEDIA_SCANNER_SCAN_FILE','-d','file://'+target)
    print('Three synthetic fixtures copied to emulator')
else: raise RuntimeError('Unsupported emulator action')
