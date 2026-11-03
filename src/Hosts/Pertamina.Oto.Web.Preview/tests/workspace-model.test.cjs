const test = require('node:test');
const assert = require('node:assert/strict');
const m = require('../wwwroot/js/site.js');

test('example contains three sources and five traceable pages', () => {
  const state = m.demo(); assert.equal(state.sources.length, 3); assert.equal(state.items.length, 5);
  assert.deepEqual(m.validate(state), state);
});
test('organizer reorder preserves original page identity and source', () => {
  const before = m.demo(), after = m.move(before, 'B2', -3, 'organize');
  assert.equal(after.items[0].id, 'B2'); assert.equal(after.items[0].originalPage, 2);
  assert.equal(before.items[0].id, 'A1'); assert.deepEqual(after.sources, before.sources);
});
test('merge reorder moves the entire source group', () => {
  const after = m.move(m.demo(), 'C1', -2, 'merge');
  assert.deepEqual(after.sources.map(s => s.id), ['C', 'A', 'B']);
  assert.deepEqual(after.items.map(i => i.id), ['C1', 'A1', 'A2', 'B1', 'B2']);
});
test('out-of-bounds and unknown moves have no effect', () => {
  const state = m.demo();
  assert.deepEqual(m.move(state, 'A1', -1, 'organize'), state);
  assert.deepEqual(m.move(state, 'missing', 1, 'merge'), state);
});
test('removing a source removes only its items, without renumbering remaining sources', () => {
  const after = m.removeSource(m.demo(), 'A');
  assert.deepEqual(after.sources.map(s => s.id), ['B', 'C']);
  assert.deepEqual(after.items.map(i => i.id), ['B1', 'B2', 'C1']);
  assert.equal(after.nextSource, 3);
});
test('malformed or oversized stored state is rejected', () => {
  assert.deepEqual(m.validate(null), m.empty());
  assert.deepEqual(m.validate({ sources: Array(51).fill({}), items: [] }), m.empty());
  const state = m.demo(); state.items[0].source = 'missing'; assert.deepEqual(m.validate(state), m.empty());
});
test('duplicate IDs and dangling sources are rejected', () => {
  const state = m.demo(); state.items.push(state.items[0]); assert.deepEqual(m.validate(state), m.empty());
  const state2 = m.demo(); state2.items = []; assert.deepEqual(m.validate(state2), m.empty());
});
test('real metadata never claims known page count or original page', () => {
  const state = { nextSource: 1, sources: [{ id: 'A', name: 'example.pdf', size: 10, pages: 9, example: false, color: 0 }], items: [{ id: 'A-file', source: 'A', originalPage: 5, rotation: 1 }] };
  const safe = m.validate(state); assert.equal(safe.sources[0].pages, null); assert.equal(safe.items[0].originalPage, null); assert.equal(safe.items[0].rotation, 0);
});
test('source counter cannot reuse an existing source ID after reload', () => {
  const state = m.demo(); state.nextSource = 0; assert.equal(m.validate(state).nextSource, 3);
  assert.equal(m.sourceId(26), 'AA'); assert.equal(m.sourceId(49), 'AX');
});
