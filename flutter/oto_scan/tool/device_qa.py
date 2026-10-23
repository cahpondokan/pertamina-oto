import subprocess, sys, pathlib, re, json, time, xml.etree.ElementTree as ET
ROOT=pathlib.Path(__file__).resolve().parents[1]
PKG='com.niam.ayani.oto'
ACT=PKG+'/id.ayani.oto.oto_scan.MainActivity'
def adb(*args, binary=False):
    p=subprocess.run(['adb',*args],capture_output=True,check=True)
    return p.stdout if binary else p.stdout.decode('utf-8','replace')
def dump():
    adb('shell','uiautomator','dump','/sdcard/oto-flutter-ui.xml')
    raw=adb('shell','cat','/sdcard/oto-flutter-ui.xml')
    root=ET.fromstring(raw[raw.index('<?xml'):])
    nodes=[{'text':n.get('text',''),'desc':n.get('content-desc',''),'bounds':n.get('bounds'),'package':n.get('package'), 'class':n.get('class'), 'enabled':n.get('enabled'), 'focused':n.get('focused')} for n in root.iter('node')]
    return nodes
def tap(label):
    nodes=dump()
    matches=[n for n in nodes if (n['text']==label or n['desc']==label) and n['enabled']=='true' and n['package'] in (PKG,'com.google.android.permissioncontroller','com.google.android.documentsui')]
    if not matches:
        raise RuntimeError('Control absent: '+label+'; visible='+str([(n['text'],n['desc']) for n in nodes if n['text'] or n['desc']]))
    a=list(map(int,re.findall(r'\d+',matches[-1]['bounds'])))
    adb('shell','input','tap',str((a[0]+a[2])//2),str((a[1]+a[3])//2))
def save(name):
    data=dump();(ROOT/'evidence'/f'{name}.json').write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps([n for n in data if n['text'] or n['desc']],ensure_ascii=False,indent=2))
def share_proof(name):
    nodes=dump()
    allowed={'Kirim PDF','Berbagi','Bagikan','Bluetooth','Drive','Gmail','WhatsApp','Quick Share','Share','Nearby Share'}
    report={'packages':sorted({n['package'] for n in nodes}),
            'share_controls':sorted({n[key] for n in nodes for key in ('text','desc') if n[key] in allowed}),
            'receiver_selected':False}
    (ROOT/'evidence'/f'{name}.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(report,indent=2))
if __name__=='__main__':
    mode=sys.argv[1]
    if mode=='dump':save(sys.argv[2])
    elif mode=='tap':tap(sys.argv[2])
    elif mode=='start':print(adb('shell','am','start','-n',ACT))
    elif mode=='share-proof':share_proof(sys.argv[2])
    elif mode=='pull':print(adb('pull','/sdcard/Android/data/'+PKG+'/files/qa',str(ROOT/'evidence'/'device')))
