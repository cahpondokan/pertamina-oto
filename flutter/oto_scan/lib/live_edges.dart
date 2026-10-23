import 'dart:math' as math;
import 'dart:ui';
import 'scan_model.dart';

/// Adaptive low-pass filtering (Casiez et al., CHI 2012) plus candidate hysteresis.
/// Large changes require two agreeing observations; absent evidence expires.
class LiveEdges {
  List<Offset>? corners, _pending;
  int lastSeen = 0, _lastTime = 0, _pendingTime = 0;
  final _velocity = List<double>.filled(8, 0);
  void reset() {
    corners = null;
    _pending = null;
    lastSeen = _lastTime = _pendingTime = 0;
    _velocity.fillRange(0, 8, 0);
  }

  static double _area(List<Offset> q) {
    double sum = 0;
    for (var i = 0; i < 4; i++) {
      final b = q[(i + 1) % 4];
      sum += q[i].dx * b.dy - b.dx * q[i].dy;
    }
    return sum.abs() / 2;
  }

  static double _distance(List<Offset> a, List<Offset> b) =>
      List.generate(4, (i) => (a[i] - b[i]).distance).reduce((a, b) => a + b) /
      4;

  static List<Offset> _align(List<Offset> q, List<Offset> reference) {
    var best = q;
    var distance = double.infinity;
    for (var shift = 0; shift < 4; shift++) {
      final candidate = List.generate(4, (i) => q[(i + shift) % 4]);
      final d = _distance(candidate, reference);
      if (d < distance) {
        distance = d;
        best = candidate;
      }
    }
    return best;
  }

  void _accept(List<Offset> next, int now) {
    corners = List.of(next);
    _pending = null;
    lastSeen = _lastTime = now;
    _velocity.fillRange(0, 8, 0);
  }

  static double _alpha(double cutoff, double dt) =>
      1 / (1 + 1 / (2 * math.pi * cutoff * dt));

  List<Offset>? update(List<Offset>? next, int now) {
    if (next == null || !validQuad(next)) {
      _pending = null;
      if (now - lastSeen >= 600) corners = null;
      return corners;
    }
    final prior = corners;
    if (prior == null || now - lastSeen >= 600) {
      if (_pending != null &&
          now - _pendingTime < 120 &&
          _distance(_align(next, _pending!), _pending!) < .035) {
        corners = null;
        return null;
      }
      if (_pending != null &&
          (_distance(_align(next, _pending!), _pending!) >= .035 ||
              now - _pendingTime > 550)) {
        _pending = List.of(next);
        _pendingTime = now;
        corners = null;
        return null;
      }
      _accept(next, now);
      return corners;
    }
    next = _align(next, prior);
    final ratio = _area(next) / _area(prior);
    if (_distance(next, prior) > .07 || ratio < .62 || ratio > 1.6) {
      if (_pending != null &&
          now - _pendingTime < 120 &&
          _distance(next, _pending!) < .035)
        return corners;
      if (_pending != null &&
          now - _pendingTime >= 120 &&
          now - _pendingTime <= 550 &&
          _distance(next, _pending!) < .035) {
        _accept(next, now);
      } else {
        _pending = List.of(next);
        _pendingTime = now;
      }
      return corners;
    }
    _pending = null;
    final dt = ((now - _lastTime) / 1000).clamp(.01, .5);
    final da = _alpha(1, dt);
    final values = <double>[];
    for (var i = 0; i < 8; i++) {
      final old = i.isEven ? prior[i ~/ 2].dx : prior[i ~/ 2].dy;
      final raw = i.isEven ? next[i ~/ 2].dx : next[i ~/ 2].dy;
      _velocity[i] += da * ((raw - old) / dt - _velocity[i]);
      final a = _alpha(.55 + 3 * _velocity[i].abs(), dt);
      values.add(old + a * (raw - old));
    }
    final filtered = List.generate(
      4,
      (i) => Offset(values[2 * i], values[2 * i + 1]),
    );
    corners = validQuad(filtered) ? filtered : List.of(next);
    lastSeen = _lastTime = now;
    return corners;
  }
}
