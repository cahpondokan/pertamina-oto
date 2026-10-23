import 'reader_fixture.dart';
import 'package:flutter/services.dart';
import 'package:flutter/material.dart';
import 'dart:convert';
import 'dart:io';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/main.dart';
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
  test(
    'convex perspective quad accepted',
    () => expect(
      validQuad(const [
        Offset(.2, .1),
        Offset(.8, .2),
        Offset(.9, .9),
        Offset(.1, .8),
      ]),
      isTrue,
    ),
  );
  test(
    'crossed corners rejected',
    () => expect(
      validQuad([square[0], square[2], square[1], square[3]]),
      isFalse,
    ),
  );
  test(
    'out of image rejected',
    () =>
        expect(validQuad([const Offset(-.1, .1), ...square.skip(1)]), isFalse),
  );
  test(
    'nonfinite coordinates rejected',
    () => expect(
      validQuad([const Offset(double.nan, .1), ...square.skip(1)]),
      isFalse,
    ),
  );
  test(
    'tiny crop rejected',
    () => expect(
      validQuad(const [
        Offset(.1, .1),
        Offset(.11, .1),
        Offset(.11, .11),
        Offset(.1, .11),
      ]),
      isFalse,
    ),
  );
  test(
    'duplicate corner rejected',
    () => expect(
      validQuad([square[0], square[0], square[2], square[3]]),
      isFalse,
    ),
  );
  test('reorder keeps originals and content', () {
    final original = ['a', 'b', 'c'];
    expect(moved(original, 0, 2), ['b', 'c', 'a']);
    expect(original, ['a', 'b', 'c']);
  });
  test('draft serialization preserves crop and edits', () {
    const p = ScanPage(
      source: 'source',
      image: 'crop',
      width: 100,
      height: 200,
      corners: square,
      detected: true,
      turns: 3,
      filter: 'gray',
    );
    final read = ScanPage.fromMap(p.toMap());
    expect(read.toMap(), p.toMap());
  });
  setUp(() {
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(NativeScan.channel, (call) async {
          switch (call.method) {
            case 'recover':
              return '[]';
            case 'permission':
              return false;
            case 'list':
              return <Object>[];
            default:
              return null;
          }
        });
  });
  testWidgets('camera denial keeps actionable retry and OTO identity', (
    tester,
  ) async {
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    expect(find.text('OTO Scan'), findsOneWidget);
    expect(find.text('Coba kamera'), findsOneWidget);
    expect(find.text('Izinkan kamera untuk memindai dokumen.'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
  testWidgets('empty library and return to camera remain usable', (
    tester,
  ) async {
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Dokumen Saya'));
    await tester.pumpAndSettle();
    expect(find.text('Hasil scan Anda akan muncul di sini.'), findsOneWidget);
    await tester.tap(find.text('Scan baru'));
    await tester.pumpAndSettle();
    expect(find.text('OTO Scan'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
  testWidgets('small screen at enlarged text does not overflow', (
    tester,
  ) async {
    tester.view.physicalSize = const Size(360, 640);
    tester.view.devicePixelRatio = 1;
    tester.platformDispatcher.textScaleFactorTestValue = 1.3;
    addTearDown(tester.platformDispatcher.clearTextScaleFactorTestValue);
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    expect(tester.takeException(), isNull);
  });
  testWidgets('recovery failure does not pretend the draft is empty', (
    tester,
  ) async {
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(NativeScan.channel, (call) async {
          if (call.method == 'recover')
            throw PlatformException(
              code: 'draft',
              message: 'Draft perlu pemulihan.',
            );
          if (call.method == 'permission') return false;
          return null;
        });
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    expect(find.text('OTO Scan'), findsOneWidget);
    expect(find.text('Draft perlu pemulihan.'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });
  testWidgets('recovered page saves PDF and opens explicit share action', (
    tester,
  ) async {
    final path = File('assets/oto.png').absolute.path;
    final page = ScanPage(
      source: path,
      image: path,
      width: 256,
      height: 256,
      corners: square,
      detected: true,
    );
    final calls = <String>[];
    final renders = <Map>[];
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(NativeScan.channel, (call) async {
          calls.add(call.method);
          if (call.method == 'readerOpen') return readerMeta(1);
          if (call.method == 'readerRender') {
            renders.add(call.arguments as Map);
            return readerRaster();
          }
          if (call.method == 'recover') return jsonEncode([page.toMap()]);
          if (call.method == 'save')
            return {
              'id': 'fixture',
              'name': 'Uji.pdf',
              'pages': 1,
              'bytes': 1000,
              'preview': path,
            };
          return null;
        });
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    expect(find.text('Lanjutkan scan sebelumnya?'), findsOneWidget);
    await tester.tap(find.text('Lanjutkan scan'));
    await tester.pumpAndSettle();
    expect(find.text('1 halaman'), findsOneWidget);
    expect(
      tester.widget<InteractiveViewer>(find.byType(InteractiveViewer)).maxScale,
      6,
    );
    expect(
      find.text('Sudah dipotong otomatis · ketuk untuk rapikan'),
      findsOneWidget,
    );
    await tester.tap(find.text('Simpan PDF'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Simpan'));
    await tester.pumpAndSettle();
    expect(find.text('Tersimpan di HP'), findsOneWidget);
    expect(calls.where((c) => c == 'save').length, 1);
    expect(calls.contains('shareMany'), isFalse);
    expect(find.text('Buka'), findsNothing);
    expect(renders.single['detail'], isFalse);
    expect(renders.single['page'], 0);
    expect(find.text('1 / 1'), findsOneWidget);
    await tester.tap(find.text('Kirim'));
    await tester.pumpAndSettle();
    expect(calls.where((c) => c == 'shareMany').length, 1);
    await tester.tap(find.byTooltip('Kembali'));
    await tester.pumpAndSettle();
    expect(find.byType(GridView), findsOneWidget);
    expect(find.text('Uji.pdf'), findsOneWidget);
    await tester.pump(const Duration(seconds: 2));
    expect(tester.takeException(), isNull);
  });
}
