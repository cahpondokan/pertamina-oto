"""Version 1.0 schemas. Explicit JSON Schema-compatible subset; no remote references."""
def obj(properties):
    return {'type': 'object', 'properties': properties, 'required': list(properties), 'additionalProperties': False}
def text(pattern=None, values=None):
    result = {'type': 'string'}
    if pattern is not None: result['pattern'] = pattern
    if values is not None: result['enum'] = values
    return result
def array(items, minimum=0):
    return {'type': 'array', 'items': items, 'minItems': minimum}
def integer(minimum=1):
    return {'type': 'integer', 'minimum': minimum}
def fixed(value):
    return text(values=[value])
VERSION = fixed('1.0')
HASH = text('[0-9a-f]{64}')
ID = text('[A-Z][A-Z0-9_]*')
MARKER = text('SYNTHETIC-[0-9a-f]{24}')
PAGE = obj({'occurrence': integer(), 'source': text(values=['A', 'B', 'C']), 'sourcePage': integer(), 'marker': MARKER})
EXPECTED = obj({'schemaVersion': VERSION, 'manifestSha256': HASH,
    'recipes': array(obj({'id': ID, 'pageCount': integer(), 'pages': array(PAGE, 1)}), 1)})
OBSERVATION = obj({'schemaVersion': VERSION, 'manifestSha256': HASH, 'recipeId': ID,
    'pageCount': integer(0), 'pages': array(PAGE), 'structure': text(values=['VALID', 'INVALID', 'INCONCLUSIVE']),
    'reopen': {'type': 'boolean'}, 'mode': fixed('SYNTHETIC_SELF_TEST'),
    'verifierId': fixed('RECORD_COMPARATOR_ONLY')})
MANIFEST = obj({'schemaVersion': VERSION, 'feature': fixed('FEAT-PDF-005'),
    'seed': fixed('P02-20260905-1434/CORP-v1'), 'classification': fixed('SYNTHETIC_NON_SENSITIVE'),
    'authority': fixed('P2-W0-CORPUS-001'), 'cost': fixed('ZERO_ADDITIONAL_COST'),
    'processingAuthorization': fixed('NOT_AUTHORIZED'),
    'provenance': obj({'generator': fixed('poc/corpus/generate.py'), 'recipeVersion': VERSION,
                       'externalAssets': {'type': 'array', 'items': text(), 'enum': [[]]},
                       'dependencies': fixed('PYTHON_STDLIB_ONLY')}),
    'retention': obj({'status': fixed('PREPARATION_ONLY_PENDING_RUN_POLICY'), 'expiry': {'type': 'null'},
                      'disposal': fixed('NO_AUTOMATIC_DELETE'), 'owner': fixed('Workspace User')}),
    'items': array(obj({'id': text(values=['A', 'B', 'C']), 'pageCount': integer(), 'byteCount': integer(),
                        'sha256': HASH, 'markers': array(MARKER, 1)}), 1),
    'recipes': array(obj({'id': ID, 'inputs': array(text(values=['A', 'B', 'C']), 2),
                          'tests': array(text('(UT-MERGE|TP-MRG)-[0-9]{3}'), 1)}), 1)})
