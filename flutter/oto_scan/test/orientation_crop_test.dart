import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/crop_screen.dart';
import 'package:oto_scan/live_detection.dart';
import 'package:oto_scan/native_scan.dart';
import 'package:oto_scan/scan_model.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  const square = [
    Offset(.1, .1),
    Offset(.9, .1),
    Offset(.9, .9),
    Offset(.1, .9),
  ];
  final path = File('assets/oto.png').absolute.path;
  final page = ScanPage(
    source: path,
    image: path,
    width: 256,
    height: 256,
    corners: square,
    detected: false,
    proposed: true,
    qualityWarning: 'Periksa ketajaman',
    orientationWarning: 'Periksa orientasi',
  );
  final calls = <MethodCall>[];
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  setUp(() {
    calls.clear();
    messenger.setMockMethodCallHandler(NativeScan.channel, (call) async {
      calls.add(call);
      if (call.method == 'edit')
        return {...page.toMap(), ...(call.arguments as Map), 'proposed': false};
      return null;
    });
  });
  tearDown(() => messenger.setMockMethodCallHandler(NativeScan.channel, null));

  test(
    'small manual cards remain editable and old drafts retain free mode',
    () {
      expect(
        validQuad(const [
          Offset(.45, .45),
          Offset(.55, .45),
          Offset(.55, .55),
          Offset(.45, .55),
        ]),
        isTrue,
      );
      final old =
          page.toMap()
            ..remove('aspect')
            ..remove('proposed')
            ..remove('orientationWarning');
      expect(ScanPage.fromMap(old).aspect, 'free');
      expect(ScanPage.fromMap(old).proposed, isFalse);
      final saved = ScanPage.fromMap({
        ...page.toMap(),
        'aspect': 'card',
        'turns': 3,
      });
      expect(ScanPage.fromMap(saved.toMap()).toMap(), saved.toMap());
      expect(saved.qualityWarning, 'Periksa ketajaman');
    },
  );
  test('only accepted new corners refresh the native capture timestamp', () {
    final state = LiveDetection()..start(0);
    Map frame(int time, List<Offset>? q) => {
      'status': 'ready',
      'frame': time,
      'corners': q?.map((p) => [p.dx, p.dy]).toList(),
    };
    state.receive(frame(1000, square), 100);
    expect(state.cornersFrame, 1000);
    state.receive(frame(1000, square), 200);
    expect(state.cornersFrame, 1000);
    const smaller = [
      Offset(.4, .4),
      Offset(.6, .4),
      Offset(.6, .6),
      Offset(.4, .6),
    ];
    state.receive(frame(1200, smaller), 300);
    expect(
      state.cornersFrame,
      1000,
      reason: 'A pending candidate is not the visible guide',
    );
    state.receive(frame(1400, smaller), 500);
    expect(state.cornersFrame, 1400);
    state.receive(frame(1400, smaller), 1200);
    expect(state.corners, isNull);
    expect(state.cornersFrame, isNull);
    state.receive(frame(2500, smaller), 1300);
    state.receive({'status': 'waiting'}, 2000);
    expect(state.cornersFrame, isNull);
  });
  Future<void> open(WidgetTester t) async {
    await t.pumpWidget(
      MaterialApp(
        home: Builder(
          builder:
              (context) => Scaffold(
                body: TextButton(
                  onPressed:
                      () => Navigator.push(
                        context,
                        MaterialPageRoute<void>(
                          builder:
                              (_) =>
                                  CropScreen(page: page, native: NativeScan()),
                        ),
                      ),
                  child: const Text('Open crop'),
                ),
              ),
        ),
      ),
    );
    await t.tap(find.text('Open crop'));
    await t.pumpAndSettle();
    // File/image completion needs a real event-loop turn between fake frames.
    await t.runAsync(
      () => Future<void>.delayed(const Duration(milliseconds: 30)),
    );
    await t.pumpAndSettle();
  }

  Future<void> ratio(WidgetTester t, String label) async {
    await t.tap(find.byKey(const ValueKey('crop-aspect')));
    await t.pumpAndSettle();
    await t.tap(find.text(label));
    await t.pumpAndSettle();
  }

  testWidgets(
    'unverified proposal is explicit and crop confirmation retains quality metadata',
    (t) async {
      await open(t);
      expect(find.textContaining('belum terverifikasi'), findsOneWidget);
      expect(find.text('Rasio: Bebas (perkiraan foto)'), findsOneWidget);
      expect(calls.where((c) => c.method == 'edit'), isEmpty);
      await ratio(t, 'Kartu landscape');
      await t.tap(find.text('Gunakan halaman'));
      await t.pumpAndSettle();
      final edit =
          calls.singleWhere((c) => c.method == 'edit').arguments as Map;
      expect(edit['aspect'], 'card');
      expect(edit['corners'], page.toMap()['corners']);
      expect(edit['qualityWarning'], page.qualityWarning);
    },
  );
  testWidgets('preset-only change is dirty and can be discarded', (t) async {
    await open(t);
    await ratio(t, 'A4 potret');
    await t.tap(find.byTooltip('Kembali'));
    await t.pumpAndSettle();
    expect(find.text('Simpan perubahan potongan?'), findsOneWidget);
    await t.tap(find.text('Buang perubahan'));
    await t.pumpAndSettle();
    expect(calls.where((c) => c.method == 'edit'), isEmpty);
    expect(find.text('Open crop'), findsOneWidget);
  });
  testWidgets(
    'rotated preview preserves corner dragging in source coordinates',
    (t) async {
      await open(t);
      await ratio(t, 'Kartu landscape');
      await t.tap(find.text('Putar 0°'));
      await t.pumpAndSettle();
      expect(find.text('Rasio: Kartu potret'), findsOneWidget);
      expect(
        t
            .widget<RotatedBox>(
              find.descendant(
                of: find.byType(CropScreen),
                matching: find.byType(RotatedBox),
              ),
            )
            .quarterTurns,
        1,
      );
      final gestureFinder = find.descendant(
        of: find.byType(CropScreen),
        matching: find.byWidgetPredicate(
          (w) => w is GestureDetector && w.onPanStart != null,
        ),
      );
      final box = t.renderObject<RenderBox>(gestureFinder);
      final start = box.localToGlobal(
        Offset(box.size.width * .1, box.size.height * .1),
      );
      final end = box.localToGlobal(
        Offset(box.size.width * .22, box.size.height * .2),
      );
      final drag = await t.startGesture(start);
      await drag.moveTo(Offset.lerp(start, end, .5)!);
      await t.pump();
      await drag.moveTo(end);
      await t.pump();
      await drag.up();
      await t.tap(find.text('Gunakan halaman'));
      await t.pumpAndSettle();
      final edit =
          calls.singleWhere((c) => c.method == 'edit').arguments as Map;
      expect(edit['turns'], 1);
      expect(edit['aspect'], 'card-portrait');
      expect(edit['corners'][0][0], closeTo(.22, .025));
      expect(edit['corners'][0][1], closeTo(.2, .025));
      expect(
        (edit['corners'] as List).skip(1).toList(),
        (page.toMap()['corners'] as List).skip(1).toList(),
      );
    },
  );
  testWidgets('crop controls fit a small screen with enlarged text', (t) async {
    t.view.physicalSize = const Size(360, 640);
    t.view.devicePixelRatio = 1;
    t.platformDispatcher.textScaleFactorTestValue = 1.3;
    addTearDown(t.view.resetPhysicalSize);
    addTearDown(t.view.resetDevicePixelRatio);
    addTearDown(t.platformDispatcher.clearTextScaleFactorTestValue);
    await open(t);
    await ratio(t, 'A4 landscape');
    await t.tap(find.text('Putar 0°'));
    await t.pumpAndSettle();
    expect(t.takeException(), isNull);
    expect(find.text('Gunakan halaman'), findsOneWidget);
    await t.pumpWidget(const SizedBox());
  });
}
