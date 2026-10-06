"""Read official vulnerability/source metadata only."""
import json
from RapidOcrMetadataIntake import ROOT, save, records

index = json.loads(save('nuget-vulnerability-index.json', 'https://api.nuget.org/v3/vulnerabilities/index.json'))
pins = {p['id']: p['version'] for p in json.loads((ROOT / 'metadata-manifest.json').read_text())['packages']}
pins.update({p['id']: p['version'] for p in json.loads((ROOT / 'followup-manifest.json').read_text())['packages']})
matches = []
for entry in index:
    data = json.loads(save('nuget-vulnerability-' + entry['@name'] + '.json', entry['@id']))
    for package, version in pins.items():
        if package in data:
            matches.append({'id': package, 'candidateVersion': version, 'feed': entry['@name'], 'entries': data[package]})
save('skiasharp-tree.json', 'https://api.github.com/repos/mono/SkiaSharp/git/trees/cc78b5933d23e6383db5d246e70db915770d55d6?recursive=1')
with (ROOT / 'security-metadata-manifest.json').open('x', encoding='utf-8') as out:
    json.dump({'pins': pins, 'nugetFeedPackageEntries': matches, 'receipts': records,
               'engineExecutions': 0, 'note': 'No package entry is not proof of no embedded/native or unpublished vulnerability.'}, out, indent=2)
print(json.dumps(matches, indent=2))
