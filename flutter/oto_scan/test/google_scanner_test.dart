import 'dart:convert';
import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/main.dart';
import 'package:oto_scan/native_scan.dart';
import 'reader_fixture.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  final calls = <MethodCall>[];
  late List<Map> draft;
  late Future<Object?> Function() scan;
  late Directory fixture;
  late String image;
  var failCheckpoint = false;
  Map<String, Object> page(String id, {bool landscape = false}) => {
    'source': '$image-$id',
    'image': image,
    'width': landscape ? 1600 : 1240,
    'height': landscape ? 900 : 1754,
    'corners': [
      [0, 0],
      [1, 0],
      [1, 1],
      [0, 1],
    ],
    'detected': true,
    'filter': 'color',
    'turns': 0,
  };
  setUpAll(() {
    fixture = Directory.systemTemp.createTempSync('oto-google-fixtures-');
    final file = File('${fixture.path}/page.png')
      ..writeAsBytesSync(readerFixtureBytes);
    image = file.path;
  });
  tearDownAll(() => fixture.deleteSync(recursive: true));
  setUp(() {
    calls.clear();
    draft = [];
    failCheckpoint = false;
    scan = () async => null;
    messenger.setMockMethodCallHandler(
      SystemChannels.platform,
      (_) async => null,
    );
    messenger.setMockMethodCallHandler(NativeScan.channel, (call) async {
      calls.add(call);
      switch (call.method) {
        case 'recover':
          return jsonEncode(draft);
        case 'googleScan':
          return scan();
        case 'checkpoint':
          if (failCheckpoint) throw PlatformException(code: 'disk');
          draft = (jsonDecode(call.arguments['json']) as List).cast<Map>();
          return null;
        case 'list':
          return <Map>[];
        default:
          return null;
      }
    });
  });
  tearDown(() {
    messenger.setMockMethodCallHandler(NativeScan.channel, null);
    messenger.setMockMethodCallHandler(SystemChannels.platform, null);
  });
  Future<void> open(WidgetTester tester) async {
    await tester.pumpWidget(const OtoScanApp(useGoogleScanner: true));
    await tester.pumpAndSettle();
    if (find.text('Lanjutkan scan').evaluate().isNotEmpty) {
      await tester.tap(find.text('Lanjutkan scan'));
      await tester.pumpAndSettle();
    }
  }

  testWidgets(
    'Google cancel and resume never relaunch or request local camera',
    (tester) async {
      await open(tester);
      expect(find.text('Mulai scan'), findsOneWidget);
      tester.binding.handleAppLifecycleStateChanged(AppLifecycleState.inactive);
      tester.binding.handleAppLifecycleStateChanged(AppLifecycleState.resumed);
      await tester.pumpAndSettle();
      expect(calls.where((c) => c.method == 'googleScan'), hasLength(1));
      expect(
        calls.where(
          (c) => ['permission', 'capture', 'detect'].contains(c.method),
        ),
        isEmpty,
      );
      await tester.tap(find.byTooltip('Dokumen Saya'));
      await tester.pumpAndSettle();
      expect(find.text('Dokumen Saya'), findsWidgets);
    },
  );
  testWidgets(
    'multiple Google pages enter OTO once with their orientations and without recrop',
    (tester) async {
      scan = () async => [page('portrait'), page('landscape', landscape: true)];
      await open(tester);
      expect(find.text('2 halaman'), findsOneWidget);
      expect(draft, hasLength(2));
      expect(draft[0]['height'], greaterThan(draft[0]['width']));
      expect(draft[1]['width'], greaterThan(draft[1]['height']));
      expect(
        calls.where((c) => ['edit', 'capture'].contains(c.method)),
        isEmpty,
      );
      expect(
        calls.singleWhere((c) => c.method == 'googleScan').arguments['limit'],
        20,
      );
    },
  );
  testWidgets('retake requests one page and replaces selected page only', (
    tester,
  ) async {
    draft = [page('first'), page('second')];
    scan = () async => [page('replacement', landscape: true)];
    await open(tester);
    await tester.tap(find.text('Ambil ulang'));
    await tester.pumpAndSettle();
    expect(
      calls.singleWhere((c) => c.method == 'googleScan').arguments['limit'],
      1,
    );
    expect(draft, hasLength(2));
    expect(draft[0]['source'], endsWith('replacement'));
    expect(draft[1]['source'], endsWith('second'));
  });
  testWidgets(
    'cancel adding returns to unchanged draft and limits remaining pages',
    (tester) async {
      draft = [page('first'), page('second')];
      await open(tester);
      await tester.tap(find.text('Tambah'));
      await tester.pumpAndSettle();
      expect(find.text('2 halaman'), findsOneWidget);
      expect(draft, hasLength(2));
      expect(
        calls.singleWhere((c) => c.method == 'googleScan').arguments['limit'],
        18,
      );
      expect(calls.where((c) => c.method == 'checkpoint'), isEmpty);
    },
  );
  testWidgets('20 page limit does not open scanner or discard draft', (
    tester,
  ) async {
    draft = [for (var i = 0; i < 20; i++) page('$i')];
    await open(tester);
    await tester.tap(find.text('Tambah'));
    await tester.pumpAndSettle();
    expect(find.text('Maksimal 20 halaman per dokumen.'), findsOneWidget);
    expect(calls.where((c) => c.method == 'googleScan'), isEmpty);
    expect(draft, hasLength(20));
  });
  testWidgets('checkpoint failure retains previous draft', (tester) async {
    draft = [page('first')];
    scan = () async => [page('new')];
    await open(tester);
    failCheckpoint = true;
    await tester.tap(find.text('Tambah'));
    await tester.pumpAndSettle();
    expect(draft, hasLength(1));
    expect(draft.single['source'], endsWith('first'));
    expect(find.text('Proses belum berhasil. Coba lagi.'), findsOneWidget);
  });
  testWidgets('SDK preparation failure has actionable retry', (tester) async {
    scan =
        () async =>
            throw PlatformException(
              code: 'google_scanner',
              message: 'Pemindai Google belum siap. Coba lagi.',
            );
    await open(tester);
    expect(find.text('Pemindai Google belum siap. Coba lagi.'), findsOneWidget);
    expect(find.text('Mulai scan'), findsOneWidget);
    scan = () async => [page('ok')];
    await tester.tap(find.text('Mulai scan'));
    await tester.pumpAndSettle();
    expect(find.text('1 halaman'), findsOneWidget);
  });
  test(
    'invalid SDK page count is rejected before committing any draft',
    () async {
      scan = () async => [page('a'), page('b')];
      await expectLater(NativeScan().googleScan(1), throwsFormatException);
    },
  );
}
