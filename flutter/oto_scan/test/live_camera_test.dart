import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/main.dart';
import 'package:oto_scan/native_scan.dart';
import 'package:oto_scan/crop_screen.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  const found = {
    'status': 'ready',
    'frame': 1000,
    'corners': [
      [.1, .1],
      [.9, .1],
      [.9, .9],
      [.1, .9],
    ],
  };
  late List<int> created, disposed;
  late Future<Object?> Function() detect;
  late List<MethodCall> calls;
  setUp(() {
    created = [];
    disposed = [];
    detect = () async => found;
    calls = [];
    messenger.setMockMethodCallHandler(SystemChannels.platform_views, (
      call,
    ) async {
      final args = call.arguments as Map?;
      if (call.method == 'create') {
        created.add(args!['id'] as int);
        return created.length;
      }
      if (call.method == 'dispose') {
        disposed.add(args!['id'] as int);
        return null;
      }
      if (call.method == 'resize')
        return {'width': args!['width'], 'height': args['height']};
      return null;
    });
    messenger.setMockMethodCallHandler(NativeScan.channel, (call) async {
      calls.add(call);
      if (call.method == 'recover') return '[]';
      if (call.method == 'permission') return true;
      if (call.method == 'detect') return detect();
      if (call.method == 'focus') return true;
      if (call.method == 'capture') throw PlatformException(code: 'fixture');
      return null;
    });
  });
  tearDown(() {
    messenger.setMockMethodCallHandler(SystemChannels.platform_views, null);
    messenger.setMockMethodCallHandler(NativeScan.channel, null);
  });
  Future<void> poll(WidgetTester tester) async {
    await tester.pump(const Duration(milliseconds: 300));
    await tester.pump();
  }

  testWidgets(
    'tap directs native focus and selected object is passed through to capture',
    (tester) async {
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      final finder = find.byKey(const ValueKey('camera-focus-target'));
      final box = tester.getRect(finder);
      calls.clear();
      await tester.tapAt(
        box.topLeft + Offset(box.width * .25, box.height * .4),
      );
      await tester.pump();
      final focus =
          calls.where((c) => c.method == 'focus').single.arguments as Map;
      expect(focus['x'], closeTo(.25, 1e-6));
      expect(focus['y'], closeTo(.4, 1e-6));
      expect(
        find.text('Fokus diarahkan · ketuk objek lain untuk ganti'),
        findsOneWidget,
      );
      await poll(tester);
      final detectArgs =
          calls.firstWhere((c) => c.method == 'detect').arguments as Map;
      expect(detectArgs['target'][0], closeTo(.25, 1e-6));
      final shutter = find.byWidgetPredicate(
        (w) => w is Semantics && w.properties.label == 'Ambil foto dokumen',
      );
      await tester.tap(shutter);
      await tester.pumpAndSettle();
      final capture =
          calls.where((c) => c.method == 'capture').single.arguments as Map;
      expect(capture['target'][0], closeTo(.5, 1e-6));
      expect(capture['previous'], hasLength(4));
      expect(capture['previousFrame'], 1000);
      await tester.pumpWidget(const SizedBox());
    },
    variant: TargetPlatformVariant.only(TargetPlatform.android),
  );

  testWidgets(
    'old detection response cannot overwrite a newly tapped object',
    (tester) async {
      final pending = Completer<Object?>();
      detect = () => pending.future;
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      await poll(tester);
      final box = tester.getRect(
        find.byKey(const ValueKey('camera-focus-target')),
      );
      await tester.tapAt(box.center);
      await tester.pump();
      pending.complete(found);
      await tester.pump();
      final painter =
          tester
              .widgetList<CustomPaint>(find.byType(CustomPaint))
              .map((w) => w.painter)
              .whereType<CornerPainter>()
              .single;
      expect(painter.corners, isNull);
      await tester.pumpWidget(const SizedBox());
    },
    variant: TargetPlatformVariant.only(TargetPlatform.android),
  );

  testWidgets(
    'native corners reach visible painter; failure restart creates a new camera',
    (tester) async {
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      await poll(tester);
      expect(
        find.text('Tepi ditemukan · akan diperiksa pada foto'),
        findsOneWidget,
      );
      final painter =
          tester
              .widgetList<CustomPaint>(find.byType(CustomPaint))
              .map((w) => w.painter)
              .whereType<CornerPainter>()
              .single;
      expect(painter.corners?.length, 4);
      for (var i = 0; i < 4; i++) {
        final point = (found['corners'] as List)[i] as List<double>;
        expect(painter.corners![i].dx, closeTo(point[0], 1e-8));
        expect(painter.corners![i].dy, closeTo(point[1], 1e-8));
      }
      final old = created.single;
      detect = () async => {'status': 'unavailable'};
      await poll(tester);
      expect(
        find.text('Deteksi tepi terhenti · ketuk untuk mulai ulang'),
        findsOneWidget,
      );
      detect = () async => found;
      await tester.tap(
        find.text('Deteksi tepi terhenti · ketuk untuk mulai ulang'),
      );
      await tester.pumpAndSettle();
      await poll(tester);
      expect(created.length, 2);
      expect(disposed, contains(old));
      expect(
        find.text('Tepi ditemukan · akan diperiksa pada foto'),
        findsOneWidget,
      );
      expect(tester.takeException(), isNull);
      await tester.pumpWidget(const SizedBox());
    },
    variant: TargetPlatformVariant.only(TargetPlatform.android),
  );
  testWidgets(
    'inactive then resume within one frame cannot reuse a closed camera',
    (tester) async {
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      final old = created.single;
      tester.binding.handleAppLifecycleStateChanged(AppLifecycleState.inactive);
      tester.binding.handleAppLifecycleStateChanged(AppLifecycleState.resumed);
      await tester.pumpAndSettle();
      await poll(tester);
      expect(created.length, 2);
      expect(disposed, contains(old));
      expect(
        find.text('Tepi ditemukan · akan diperiksa pada foto'),
        findsOneWidget,
      );
      expect(tester.takeException(), isNull);
      await tester.pumpWidget(const SizedBox());
    },
    variant: TargetPlatformVariant.only(TargetPlatform.android),
  );
  testWidgets(
    'hung detection times out visibly and later frames recover',
    (tester) async {
      final pending = Completer<Object?>();
      int calls = 0;
      detect = () {
        calls++;
        return pending.future;
      };
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      await poll(tester);
      await tester.pump(const Duration(seconds: 1));
      expect(calls, 1);
      await tester.pump(const Duration(seconds: 1));
      await tester.pump();
      expect(
        find.text('Deteksi tepi terhenti · ketuk untuk mulai ulang'),
        findsOneWidget,
      );
      detect = () async => found;
      pending.complete({'status': 'unavailable'});
      await poll(tester);
      await poll(tester);
      expect(
        find.text('Tepi ditemukan · akan diperiksa pada foto'),
        findsOneWidget,
      );
      expect(tester.takeException(), isNull);
      await tester.pumpWidget(const SizedBox());
    },
    variant: TargetPlatformVariant.only(TargetPlatform.android),
  );
}
