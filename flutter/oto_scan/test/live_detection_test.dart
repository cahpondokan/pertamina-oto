import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/live_detection.dart';

void main() {
  final quad = [
    [.1, .1],
    [.9, .1],
    [.9, .9],
    [.1, .9],
  ];
  test('repeated native frame cannot renew a frozen guide', () {
    final state = LiveDetection()..start(0);
    state.receive({'status': 'ready', 'corners': quad, 'frame': 100}, 100);
    state.receive({'status': 'ready', 'corners': quad, 'frame': 100}, 750);
    expect(state.corners, isNull);
    state.receive({'status': 'ready', 'corners': quad, 'frame': 800}, 800);
    expect(state.corners, hasLength(4));
  });
  test('camera startup is distinct from active detection', () {
    final state = LiveDetection()..start(0);
    state.receive({'status': 'waiting'}, 250);
    expect(state.state, 'starting');
    state.receive({'status': 'ready', 'corners': null}, 500);
    expect(state.state, 'searching');
    expect(state.corners, isNull);
  });
  test('valid native frame produces visible corner coordinates', () {
    final state = LiveDetection()..start(0);
    state.receive({'status': 'ready', 'corners': quad}, 250);
    expect(state.state, 'found');
    expect(state.corners?.length, 4);
    state.receive({'status': 'ready', 'corners': null}, 1000);
    expect(state.state, 'searching');
    expect(state.corners, isNull);
  });
  test('missing frames expire and offer restart, never pretend to search', () {
    final state = LiveDetection()..start(0);
    state.receive({'status': 'waiting'}, 3000);
    expect(state.failed, isTrue);
    expect(state.message, contains('mulai ulang'));
    state.start(4000);
    expect(state.failed, isFalse);
    expect(state.state, 'starting');
  });
  test(
    'native failure removes stale corners and can recover on a new frame',
    () {
      final state = LiveDetection()..start(0);
      state.receive({'status': 'ready', 'corners': quad}, 250);
      state.fail();
      expect(state.failed, isTrue);
      expect(state.corners, isNull);
      state.receive({'status': 'processing'}, 400);
      expect(state.failed, isTrue);
      state.receive({'status': 'ready', 'corners': quad}, 500);
      expect(state.state, 'found');
    },
  );
  test('malformed channel response never draws an invented rectangle', () {
    final state = LiveDetection()..start(0);
    state.receive({
      'status': 'ready',
      'corners': [
        [1, 2],
      ],
    }, 250);
    expect(state.failed, isTrue);
    expect(state.corners, isNull);
    state.receive(null, 500);
    expect(state.failed, isTrue);
  });
}
