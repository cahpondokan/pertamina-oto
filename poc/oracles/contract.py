"""Observation comparator only. No PDF parsing, engine execution or publication authority."""
import re

def validate(value, schema):
    known = {'type', 'properties', 'required', 'additionalProperties', 'items', 'minItems', 'enum', 'pattern', 'minimum'}
    if set(schema) - known:
        raise ValueError('UNSUPPORTED_SCHEMA_KEYWORD')
    kind = schema['type']
    checks = {'object': lambda x: type(x) is dict, 'array': lambda x: type(x) is list,
              'string': lambda x: type(x) is str, 'integer': lambda x: type(x) is int,
              'boolean': lambda x: type(x) is bool, 'null': lambda x: x is None}
    if kind not in checks or not checks[kind](value):
        raise ValueError('SCHEMA_TYPE')
    if 'enum' in schema and value not in schema['enum']:
        raise ValueError('SCHEMA_ENUM')
    if kind == 'object':
        properties = schema['properties']
        if set(schema['required']) - set(value):
            raise ValueError('SCHEMA_REQUIRED')
        if schema.get('additionalProperties') is not False or set(value) - set(properties):
            raise ValueError('SCHEMA_ADDITIONAL')
        for key, child in value.items():
            validate(child, properties[key])
    if kind == 'array':
        if len(value) < schema.get('minItems', 0):
            raise ValueError('SCHEMA_MIN_ITEMS')
        for child in value:
            validate(child, schema['items'])
    if kind == 'string' and 'pattern' in schema and re.fullmatch(schema['pattern'], value, flags=re.ASCII) is None:
        raise ValueError('SCHEMA_PATTERN')
    if kind == 'integer' and value < schema.get('minimum', value):
        raise ValueError('SCHEMA_MINIMUM')

def compare(expected, observed, expected_schema, observation_schema):
    try:
        validate(expected, expected_schema)
        validate(observed, observation_schema)
    except (ValueError, TypeError, KeyError):
        return {'status': 'REJECT', 'reason': 'INVALID_RECORD', 'publishAuthorized': False}
    reasons = []
    if expected['manifestSha256'] != observed['manifestSha256']:
        reasons.append('MANIFEST_MISMATCH')
    matches = [r for r in expected['recipes'] if r['id'] == observed['recipeId']]
    if len(matches) != 1:
        reasons.append('RECIPE_UNKNOWN_OR_DUPLICATE')
    else:
        recipe = matches[0]
        if recipe['pageCount'] != len(recipe['pages']):
            reasons.append('INVALID_EXPECTATION')
        if observed['pageCount'] != recipe['pageCount'] or len(observed['pages']) != recipe['pageCount']:
            reasons.append('PAGE_COUNT')
        if observed['pages'] != recipe['pages']:
            reasons.append('PAGE_SEQUENCE_OR_CONTENT')
    if observed['structure'] != 'VALID' or observed['reopen'] is not True:
        reasons.append('STRUCTURE_OR_REOPEN')
    return {'status': 'REJECT' if reasons else 'MATCH_ONLY',
            'reason': ','.join(reasons) if reasons else 'OBSERVATION_MATCH', 'publishAuthorized': False}
