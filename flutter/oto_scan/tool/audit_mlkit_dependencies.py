"""Record public Maven metadata and OSV results for the isolated scanner trial."""
from pathlib import Path
import hashlib, json, os, re, urllib.request, xml.etree.ElementTree as ET
from concurrent.futures import ThreadPoolExecutor

root = Path(__file__).resolve().parents[1]
out = root / 'evidence/mlkit-trial/dependencies'
log = (out/'gradle-runtime.log').read_text(encoding='utf-8-sig')
coordinates = set()
for line in log.splitlines():
    m = re.search(r'(?:\+---|\\---) ([\w.-]+):([\w.-]+):([\w.+-]+)(?: -> ([\w.+-]+))?', line)
    if m and not m[1].startswith('io.flutter'):
        coordinates.add((m[1], m[2], m[4] or m[3]))
cache = Path(os.environ.get('GRADLE_USER_HOME', str(Path.home()/'.gradle')))/'caches/modules-2/files-2.1'
ns = {'p':'http://maven.apache.org/POM/4.0.0'}
metadata = out/'poms'
metadata.mkdir(exist_ok=True)
def acquire_pom(coordinate):
    group, artifact, version = coordinate
    target = metadata/(group+'_'+artifact+'_'+version+'.pom')
    if not target.exists():
        repository = ('https://dl.google.com/dl/android/maven2/' if group.startswith(('androidx.', 'com.google.android.', 'com.google.mlkit', 'com.google.firebase')) else 'https://repo.maven.apache.org/maven2/')
        url = repository+group.replace('.', '/')+'/'+artifact+'/'+version+'/'+artifact+'-'+version+'.pom'
        with urllib.request.urlopen(url, timeout=30) as response:
            target.write_bytes(response.read())
    return coordinate, target
with ThreadPoolExecutor(max_workers=4) as pool:
    official_poms = dict(pool.map(acquire_pom, sorted(coordinates)))
packages = []
def pom_licenses(pom, depth=0):
    tree = ET.parse(pom)
    # Older Maven POMs sometimes omit the XML namespace.
    for node in tree.iter():
        node.tag = node.tag.rsplit('}', 1)[-1]
    licenses = [{'name':x.findtext('name', ''), 'url':x.findtext('url', '')} for x in tree.findall('licenses/license')]
    parent = tree.find('parent')
    if not licenses and parent is not None and depth < 4:
        coordinate = tuple(parent.findtext(x) for x in ('groupId','artifactId','version'))
        if all(coordinate):
            _, inherited = acquire_pom(coordinate)
            licenses = pom_licenses(inherited, depth+1)
    return licenses
for group, artifact, version in sorted(coordinates):
    folder = cache/group/artifact/version
    poms = [official_poms[(group, artifact, version)]]
    licenses = []
    for pom in poms:
        licenses.extend(pom_licenses(pom))
    binaries = [{'name':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in folder.glob('*/*') if p.suffix in ('.aar','.jar')]
    packages.append({'name':group+':'+artifact,'version':version,'licenses':licenses,'binaries':binaries})
body = json.dumps({'queries':[{'package':{'ecosystem':'Maven','name':p['name']},'version':p['version']} for p in packages]}).encode()
request = urllib.request.Request('https://api.osv.dev/v1/querybatch', data=body, headers={'Content-Type':'application/json'})
with urllib.request.urlopen(request, timeout=45) as response:
    result = json.load(response)
if len(result.get('results',[])) != len(packages):
    raise RuntimeError('Incomplete vulnerability lookup')
findings=[]
for package, response in zip(packages,result['results']):
    package['osv']=response
    for vulnerability in response.get('vulns',[]):
        findings.append({'package':package['name'],'version':package['version'],'id':vulnerability['id']})
report={'packages':packages,'lookup':'OSV public Maven coordinates, not a guarantee of vulnerability absence','findings':findings,'missingLicenseMetadata':[p['name'] for p in packages if not p['licenses']]}
(out/'dependency-audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({'packages':len(packages),'findings':findings,'missingLicenseMetadata':report['missingLicenseMetadata']},indent=2))
