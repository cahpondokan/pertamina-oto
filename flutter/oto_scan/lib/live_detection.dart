import 'dart:ui';
import 'live_edges.dart';
import 'scan_model.dart';

/// Distinguishes a working detector with no document from a missing camera feed.
class LiveDetection {
  final edges = LiveEdges();
  String state = 'starting';
  int lastReady = 0;
  num? _lastFrame;
  num? cornersFrame;
  List<Offset>? get corners => edges.corners;
  bool get failed => state == 'unavailable';
  String get message => switch (state) {
    'found' => 'Tepi ditemukan · akan diperiksa pada foto',
    'searching' => 'Mencari empat tepi dokumen…',
    'unavailable' => 'Deteksi tepi terhenti · ketuk untuk mulai ulang',
    _ => 'Menyiapkan deteksi tepi…',
  };
  void start(int now) {
    edges.reset();
    state = 'starting';
    lastReady = now;
    _lastFrame = null;
    cornersFrame = null;
  }

  void fail() {
    edges.reset();
    state = 'unavailable';
    cornersFrame = null;
  }

  void receive(Map? result, int now) {
    final status = result?['status'];
    if (status == 'ready') {
      final frame = result?['frame'];
      if (frame is num && frame == _lastFrame) {
        edges.update(null, now);
        if (corners == null) cornersFrame = null;
        state = corners == null ? 'searching' : 'found';
        return;
      }
      if (frame is num) _lastFrame = frame;
      final raw = result?['corners'];
      List<Offset>? next;
      if (raw != null) {
        try {
          next = [
            for (final p in raw as List)
              Offset((p[0] as num).toDouble(), (p[1] as num).toDouble()),
          ];
          if (!validQuad(next)) throw const FormatException();
        } catch (_) {
          fail();
          return;
        }
      }
      lastReady = now;
      edges.update(next, now);
      if (next != null && edges.lastSeen == now)
        cornersFrame = frame is num ? frame : null;
      if (corners == null) cornersFrame = null;
      state = corners == null ? 'searching' : 'found';
    } else if ((status == 'waiting' || status == 'processing') &&
        now - lastReady < 3000) {
      if (failed) return;
      edges.update(null, now);
      if (corners == null) cornersFrame = null;
      state = corners == null ? 'starting' : 'found';
    } else {
      fail();
    }
  }
}
