import 'dart:io';
import 'dart:convert';
import 'dart:math';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/live_edges.dart';

// The r06 guide used a fixed 0.7 interpolation for small moves and accepted
// every larger jump immediately. This is an exact baseline for valid sequences.
List<Offset> legacy(List<Offset>? previous, List<Offset> next) =>
    previous != null &&
            List.generate(
              4,
              (i) => (previous[i] - next[i]).distance,
            ).every((d) => d < .035)
        ? List.generate(4, (i) => Offset.lerp(previous[i], next[i], .7)!)
        : List.of(next);

void main() {
  test(
    'matched temporal sequences improve jitter and reject isolated distractors',
    () {
      const q = [
        Offset(.1, .1),
        Offset(.8, .1),
        Offset(.8, .85),
        Offset(.1, .85),
      ];
      final random = Random(732), current = LiveEdges();
      List<Offset>? old;
      double before = 0, after = 0;
      for (var frame = 0; frame < 160; frame++) {
        final noisy = [
          for (final p in q)
            p +
                Offset(
                  (random.nextDouble() - .5) * .024,
                  (random.nextDouble() - .5) * .024,
                ),
        ];
        old = legacy(old, noisy);
        final now = current.update(noisy, frame * 250)!;
        if (frame > 9)
          for (var i = 0; i < 4; i++) {
            before += (old[i] - q[i]).distanceSquared;
            after += (now[i] - q[i]).distanceSquared;
          }
      }
      before = sqrt(before / 600);
      after = sqrt(after / 600);
      expect(after, lessThan(before * .8));
      const other = [
        Offset(.4, .3),
        Offset(.9, .3),
        Offset(.9, .7),
        Offset(.4, .7),
      ];
      current.reset();
      old = null;
      int oldSwitches = 0, newSwitches = 0;
      for (var i = 0; i < 100; i++) {
        final observation = i % 10 == 5 ? other : q;
        old = legacy(old, observation);
        final now = current.update(observation, i * 250)!;
        if ((old.first - q.first).distance > .07) oldSwitches++;
        if ((now.first - q.first).distance > .07) newSwitches++;
      }
      expect(oldSwitches, 10);
      expect(newSwitches, 0);
      current.reset();
      double lag = 0;
      for (var i = 0; i < 12; i++) {
        final next = [for (final p in q) p + Offset(.008 * i, 0)];
        final actual = current.update(next, i * 250)!;
        lag = max(lag, (next.first - actual.first).distance);
      }
      expect(lag, lessThan(.025));
      File('evidence/r07-tracking-benchmark.json').writeAsStringSync(
        const JsonEncoder.withIndent('  ').convert({
          'scope':
              'Synthetic coordinate sequences, identical inputs; not phone or ML Kit measurements',
          'seed': 732,
          'stationary_frames': 160,
          'r06_rms': before,
          'r07_rms': after,
          'jitter_reduction_percent': 100 * (1 - after / before),
          'isolated_distractors': 10,
          'r06_wrong_object_frames': oldSwitches,
          'r07_wrong_object_frames': newSwitches,
          'slow_motion_max_normalized_lag': lag,
        }),
      );
    },
  );
}
