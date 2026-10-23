import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/live_edges.dart';

void main() {
  const q = [Offset(.1, .1), Offset(.9, .1), Offset(.9, .9), Offset(.1, .9)];
  test('first detection responds immediately', () {
    final edges = LiveEdges();
    expect(edges.update(q, 0), q);
    expect(identical(edges.corners, q), isFalse);
  });
  test('brief dropout holds guide, then expires', () {
    final edges = LiveEdges()..update(q, 100);
    expect(edges.update(null, 350), q);
    expect(edges.update(null, 600), q);
    expect(edges.update(null, 700), isNull);
  });
  test('small jitter is damped without waiting for another frame', () {
    final edges = LiveEdges()..update(q, 0);
    final moved = [for (final p in q) p + const Offset(.01, 0)];
    final result = edges.update(moved, 250)!;
    expect(result.first.dx, greaterThan(.1));
    expect(result.first.dx, lessThan(.106));
  });
  test('large movement requires a second consistent observation', () {
    final edges = LiveEdges()..update(q, 0);
    const next = [
      Offset(.2, .2),
      Offset(.8, .2),
      Offset(.8, .8),
      Offset(.2, .8),
    ];
    expect(edges.update(next, 250), q);
    expect(edges.update(next, 500), next);
    edges.reset();
    expect(edges.update(null, 501), isNull);
  });
  test('invalid corners never renew an old guide', () {
    final edges = LiveEdges()..update(q, 0);
    final crossed = [q[0], q[2], q[1], q[3]];
    expect(edges.update(crossed, 250), q);
    expect(edges.update(crossed, 600), isNull);
  });
  test('one-frame distractor never replaces selected paper', () {
    final edges = LiveEdges()..update(q, 0);
    const other = [
      Offset(.4, .2),
      Offset(.8, .2),
      Offset(.8, .6),
      Offset(.4, .6),
    ];
    expect(edges.update(other, 250), q);
    expect(edges.update(q, 500), q);
  });
  test('cyclic corner ordering does not make a rotating page jump', () {
    final edges = LiveEdges()..update(q, 0);
    final reordered = [q[1], q[2], q[3], q[0]];
    final result = edges.update(reordered, 250)!;
    for (var i = 0; i < 4; i++)
      expect((result[i] - q[i]).distance, lessThan(1e-8));
  });
  test(
    'alternating unrelated candidates expire instead of latching forever',
    () {
      final edges = LiveEdges()..update(q, 0);
      const a = [
        Offset(.1, .1),
        Offset(.4, .1),
        Offset(.4, .6),
        Offset(.1, .6),
      ];
      const b = [
        Offset(.6, .3),
        Offset(.9, .3),
        Offset(.9, .9),
        Offset(.6, .9),
      ];
      edges.update(a, 250);
      edges.update(b, 500);
      expect(edges.update(a, 750), isNull);
      expect(edges.update(b, 1000), isNull);
      expect(edges.update(b, 1250), b);
    },
  );
  test('slow real movement remains responsive', () {
    final edges = LiveEdges()..update(q, 0);
    for (var i = 1; i <= 6; i++) {
      final next = [for (final p in q) p + Offset(.01 * i, 0)];
      final actual = edges.update(next, 250 * i)!;
      expect((actual.first - next.first).distance, lessThan(.025));
    }
  });
}
