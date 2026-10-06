"""Static metadata follow-up; uses only our intake helper, not vendor code."""
import json
from RapidOcrMetadataIntake import ROOT, save, records

PIN = '549847df506a32421086d2a08d19646ee958a6e0'
errors = []
for name, url in [
    ('rapidocr-tree.json', f'https://api.github.com/repos/RapidAI/RapidOCR/git/trees/{PIN}?recursive=1'),
    ('RapidOCR-MODEL_LICENSES.md', f'https://raw.githubusercontent.com/RapidAI/RapidOCR/{PIN}/python/MODEL_LICENSES.md'),
    ('RapidOCR-LICENSE.txt', f'https://raw.githubusercontent.com/RapidAI/RapidOCR/{PIN}/LICENSE'),
    ('RapidOCR-default_models.yaml', f'https://raw.githubusercontent.com/RapidAI/RapidOCR/{PIN}/python/rapidocr/default_models.yaml')]:
    try:
        save(name, url)
    except Exception as e:
        errors.append({'file': name, 'error': str(e)[:300]})

extra = [('system.memory', '4.5.5'), ('system.numerics.tensors', '9.0.0')]
packages = []
for package, version in extra:
    reg = json.loads(save(package + '.registration.json', f'https://api.nuget.org/v3/registration5-semver1/{package}/{version}.json'))
    cat = json.loads(save(package + '.catalog.json', reg['catalogEntry']))
    packages.append({'id': package, 'version': version, 'licenseExpression': cat.get('licenseExpression'),
        'licenseFile': cat.get('licenseFile'), 'packageContent': reg['packageContent'],
        'packageHash': cat.get('packageHash'), 'packageHashAlgorithm': cat.get('packageHashAlgorithm'),
        'packageSize': cat.get('packageSize'), 'dependencyGroups': cat.get('dependencyGroups')})

all_pins = [(p['id'], p['version']) for p in json.loads((ROOT / 'metadata-manifest.json').read_text())['packages']] + extra
for package, version in all_pins:
    try:
        save(package + '.advisories.json', f'https://api.github.com/advisories?ecosystem=nuget&affects={package}%40{version}&per_page=100')
    except Exception as e:
        errors.append({'package': package, 'error': str(e)[:300]})

with (ROOT / 'followup-manifest.json').open('x', encoding='utf-8') as out:
    json.dump({'packages': packages, 'receipts': records, 'errors': errors, 'engineExecutions': 0}, out, indent=2)
for p in packages:
    print(p['id'], p['version'], p['licenseExpression'] or p['licenseFile'])
    for g in p['dependencyGroups']:
        print(g.get('targetFramework'), [(d['id'], d['range']) for d in g.get('dependencies', [])])
print('errors=' + json.dumps(errors))
