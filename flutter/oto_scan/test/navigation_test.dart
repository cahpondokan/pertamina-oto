import 'reader_fixture.dart';
import 'dart:async';
import 'dart:convert';
import 'dart:io';
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
  final path = File('assets/oto.png').absolute.path;
  late Directory scanFixtures;
  late String secondPagePath;
  setUpAll(() {
    scanFixtures = Directory.systemTemp.createTempSync('oto-save-flow-');
    secondPagePath =
        File(path).copySync('${scanFixtures.path}/page-two.png').path;
  });
  tearDownAll(() => scanFixtures.deleteSync(recursive: true));
  final page = {
    'source': path,
    'image': path,
    'width': 256,
    'height': 256,
    'corners': [
      [.1, .1],
      [.9, .1],
      [.9, .9],
      [.1, .9],
    ],
    'turns': 0,
    'filter': 'clear',
    'detected': true,
  };
  final calls = <MethodCall>[], system = <MethodCall>[];
  late List<Map> draft;
  late Map document;
  late String renderPath;
  late Future<Object?> Function() save;
  var failShare = false,
      failCheckpoint = false,
      failList = false,
      failThumbnail = false;
  setUp(() {
    calls.clear();
    system.clear();
    draft = [Map.of(page)];
    renderPath = path;
    failShare = failCheckpoint = failList = failThumbnail = false;
    document = {
      'id': 'fixture',
      'name': 'Uji.pdf',
      'pages': 3,
      'bytes': 1234,
      'created': 1,
    };
    save = () async {
      document = {...document, 'pages': draft.length};
      return document;
    };
    messenger.setMockMethodCallHandler(SystemChannels.platform, (c) async {
      system.add(c);
      return null;
    });
    messenger.setMockMethodCallHandler(NativeScan.channel, (c) async {
      calls.add(c);
      switch (c.method) {
        case 'recover':
          return jsonEncode(draft);
        case 'permission':
          return false;
        case 'list':
          if (failList) throw PlatformException(code: 'list');
          return [document];
        case 'readerOpen':
          return readerMeta(document['pages'] as int);
        case 'readerRender':
          return readerRaster();
        case 'thumbnail':
          if (failThumbnail) throw PlatformException(code: 'thumbnail');
          return renderPath;
        case 'checkpoint':
          if (failCheckpoint) throw PlatformException(code: 'checkpoint');
          draft = (jsonDecode(c.arguments['json']) as List).cast<Map>();
          return null;
        case 'edit':
          return {...page, ...(c.arguments as Map)};
        case 'save':
          return save();
        case 'rename':
          document = {...document, 'name': '${c.arguments['name']}.pdf'};
          return document;
        case 'shareMany':
          if (failShare)
            throw PlatformException(code: 'share', message: 'Tidak tersedia.');
          return null;
        default:
          return null;
      }
    });
  });
  tearDown(() {
    messenger.setMockMethodCallHandler(NativeScan.channel, null);
    messenger.setMockMethodCallHandler(SystemChannels.platform, null);
  });
  Future<void> launch(WidgetTester t) async {
    await t.pumpWidget(const OtoScanApp());
    await t.pumpAndSettle();
  }

  Future<void> resume(WidgetTester t) async {
    await launch(t);
    await t.tap(find.text('Lanjutkan scan'));
    await t.pumpAndSettle();
  }

  Future<void> crop(WidgetTester t) async {
    await resume(t);
    await t.tap(find.byKey(const ValueKey('edit-page')));
    await t.pumpAndSettle();
    expect(find.byType(CropScreen), findsOneWidget);
  }

  Future<void> reader(WidgetTester t) async {
    await launch(t);
    await t.tap(find.text('Dokumen Saya'));
    await t.pumpAndSettle();
    await t.tap(find.text('Uji.pdf'));
    await t.pumpAndSettle();
  }

  testWidgets(
    'startup offers draft, cancel new scan preserves it, pages back continues camera',
    (t) async {
      await launch(t);
      expect(calls.where((c) => c.method == 'permission'), isEmpty);
      await t.tap(find.text('Scan baru'));
      await t.pumpAndSettle();
      await t.tap(find.text('Batal'));
      await t.pumpAndSettle();
      expect(calls.where((c) => c.method == 'checkpoint'), isEmpty);
      await t.tap(find.text('Lanjutkan scan'));
      await t.pumpAndSettle();
      await t.binding.handlePopRoute();
      await t.pumpAndSettle();
      expect(find.text('OTO Scan'), findsOneWidget);
      expect(draft, hasLength(1));
      await t.binding.handlePopRoute();
      await t.pumpAndSettle();
      expect(
        system.where((c) => c.method == 'SystemNavigator.pop'),
        hasLength(1),
      );
      expect(draft, hasLength(1));
    },
  );
  testWidgets('explicit new scan clears draft only after confirmation', (
    t,
  ) async {
    await launch(t);
    await t.tap(find.text('Scan baru'));
    await t.pumpAndSettle();
    await t.tap(
      find.descendant(
        of: find.byType(AlertDialog),
        matching: find.text('Scan baru'),
      ),
    );
    await t.pumpAndSettle();
    expect(draft, isEmpty);
    expect(calls.where((c) => c.method == 'checkpoint'), hasLength(1));
    expect(find.text('OTO Scan'), findsOneWidget);
  });
  testWidgets('gallery opened from pages returns to same pages', (t) async {
    await resume(t);
    await t.tap(find.byTooltip('Pilihan scan'));
    await t.pumpAndSettle();
    await t.tap(find.text('Dokumen Saya'));
    await t.pumpAndSettle();
    await t.tap(find.byTooltip('Kembali'));
    await t.pumpAndSettle();
    expect(find.text('1 halaman'), findsOneWidget);
    expect(calls.where((c) => c.method == 'permission'), isEmpty);
  });
  testWidgets(
    'back during successful save lands on gallery and finishes draft without reopening reader',
    (t) async {
      final pending = Completer<Object?>();
      save = () => pending.future;
      await resume(t);
      await t.tap(find.text('Simpan PDF'));
      await t.pumpAndSettle();
      await t.tap(find.text('Simpan'));
      await t.pump();
      await t.binding.handlePopRoute();
      await t.pump();
      expect(
        find.text('Menyelesaikan proses sebelum kembali…'),
        findsOneWidget,
      );
      expect(find.text('1 halaman'), findsOneWidget);
      pending.complete({...document, 'pages': 1});
      await t.pumpAndSettle();
      expect(find.byType(PdfReader), findsNothing);
      expect(find.byType(GridView), findsOneWidget);
      expect(find.text('Uji.pdf'), findsOneWidget);
      expect(draft, isEmpty);
      expect(calls.where((c) => c.method == 'permission'), isEmpty);
      expect(calls.where((c) => c.method == 'save'), hasLength(1));
      await t.pump(const Duration(seconds: 3));
    },
  );
  testWidgets('failed save stays on pages even if back was queued', (t) async {
    final pending = Completer<Object?>();
    save = () => pending.future;
    await resume(t);
    await t.tap(find.text('Simpan PDF'));
    await t.pumpAndSettle();
    await t.tap(find.text('Simpan'));
    await t.pump();
    await t.binding.handlePopRoute();
    await t.pump();
    pending.completeError(
      PlatformException(code: 'save', message: 'Ruang tidak cukup.'),
    );
    await t.pumpAndSettle();
    expect(find.text('1 halaman'), findsOneWidget);
    expect(find.text('Ruang tidak cukup.'), findsOneWidget);
    expect(draft, hasLength(1));
    await t.pump(const Duration(seconds: 2));
  });
  for (final systemBack in [false, true]) {
    testWidgets(
      'saved preview Back ($systemBack) opens gallery, Scan baru starts empty and survives restart',
      (t) async {
        if (systemBack)
          draft = [
            Map.of(page),
            {...page, 'image': secondPagePath, 'source': secondPagePath},
          ];
        await resume(t);
        await t.tap(find.text('Simpan PDF'));
        await t.pumpAndSettle();
        await t.tap(find.text('Simpan'));
        await t.pumpAndSettle();
        expect(find.byType(PdfReader), findsOneWidget);
        expect(draft, isEmpty);
        final methods = calls.map((c) => c.method).toList();
        expect(
          methods.indexOf('checkpoint'),
          greaterThan(methods.indexOf('save')),
        );
        expect(calls.where((c) => c.method == 'save'), hasLength(1));
        await t.tap(find.text('Kirim'));
        await t.pumpAndSettle();
        expect(
          calls.lastWhere((c) => c.method == 'shareMany').arguments['ids'],
          ['fixture'],
        );
        if (systemBack) {
          await t.binding.handlePopRoute();
        } else {
          await t.tap(find.byTooltip('Kembali'));
        }
        await t.pumpAndSettle();
        expect(find.byType(GridView), findsOneWidget);
        expect(find.text('Uji.pdf'), findsOneWidget);
        expect(calls.where((c) => c.method == 'permission'), isEmpty);
        await t.tap(find.text('Scan baru'));
        await t.pumpAndSettle();
        expect(find.byType(AlertDialog), findsNothing);
        expect(find.text('OTO Scan'), findsOneWidget);
        expect(draft, isEmpty);
        expect(
          t
              .widget<TextButton>(find.widgetWithText(TextButton, 'Selesai'))
              .onPressed,
          isNull,
        );
        await t.pumpWidget(const SizedBox());
        await t.pump();
        await launch(t);
        expect(find.text('Lanjutkan scan sebelumnya?'), findsNothing);
        expect(find.text('OTO Scan'), findsOneWidget);
        expect(draft, isEmpty);
        expect(t.takeException(), isNull);
      },
    );
  }
  testWidgets(
    'save clears old gallery search and fallback still exposes saved PDF if list or thumbnail fails',
    (t) async {
      await resume(t);
      await t.tap(find.byTooltip('Pilihan scan'));
      await t.pumpAndSettle();
      await t.tap(find.text('Dokumen Saya'));
      await t.pumpAndSettle();
      await t.enterText(find.byType(TextField), 'tidak cocok');
      await t.pumpAndSettle();
      await t.tap(find.byTooltip('Kembali'));
      await t.pumpAndSettle();
      failList = failThumbnail = true;
      await t.tap(find.text('Simpan PDF'));
      await t.pumpAndSettle();
      await t.tap(find.text('Simpan'));
      await t.pumpAndSettle();
      expect(find.byType(PdfReader), findsOneWidget);
      await t.tap(find.byTooltip('Kembali'));
      await t.pumpAndSettle();
      expect(t.widget<TextField>(find.byType(TextField)).controller!.text, '');
      expect(find.text('Uji.pdf'), findsOneWidget);
      expect(draft, isEmpty);
      expect(find.text('1 halaman'), findsNothing);
      expect(t.takeException(), isNull);
    },
  );
  testWidgets(
    'failed draft completion retries existing PDF without a duplicate save',
    (t) async {
      failCheckpoint = true;
      await resume(t);
      await t.tap(find.text('Simpan PDF'));
      await t.pumpAndSettle();
      await t.tap(find.text('Simpan'));
      await t.pumpAndSettle();
      expect(find.byType(PdfReader), findsNothing);
      expect(draft, hasLength(1));
      expect(find.text('Selesaikan penyimpanan'), findsOneWidget);
      expect(calls.where((c) => c.method == 'save'), hasLength(1));
      failCheckpoint = false;
      await t.tap(find.text('Selesaikan penyimpanan'));
      await t.pumpAndSettle();
      expect(find.byType(PdfReader), findsOneWidget);
      expect(calls.where((c) => c.method == 'save'), hasLength(1));
      expect(draft, isEmpty);
      await t.pump(const Duration(seconds: 2));
      expect(t.takeException(), isNull);
    },
  );
  testWidgets('cancelled naming and unconfirmed save retain all draft pages', (
    t,
  ) async {
    await resume(t);
    await t.tap(find.text('Simpan PDF'));
    await t.pumpAndSettle();
    await t.tap(find.text('Batal'));
    await t.pumpAndSettle();
    expect(calls.where((c) => c.method == 'save'), isEmpty);
    expect(calls.where((c) => c.method == 'checkpoint'), isEmpty);
    save = () async => null;
    await t.tap(find.text('Simpan PDF'));
    await t.pumpAndSettle();
    await t.tap(find.text('Simpan'));
    await t.pumpAndSettle();
    expect(draft, hasLength(1));
    expect(calls.where((c) => c.method == 'checkpoint'), isEmpty);
    expect(find.text('1 halaman'), findsOneWidget);
    expect(find.byType(PdfReader), findsNothing);
    await t.pump(const Duration(seconds: 2));
  });
  testWidgets(
    'dirty crop back can cancel then discard without modifying saved page',
    (t) async {
      await crop(t);
      await t.tap(find.text('Putar 0°'));
      await t.pumpAndSettle();
      await t.binding.handlePopRoute();
      await t.pumpAndSettle();
      await t.tap(find.text('Batal kembali'));
      await t.pumpAndSettle();
      expect(find.text('Putar 90°'), findsOneWidget);
      await t.tap(find.byTooltip('Kembali'));
      await t.pumpAndSettle();
      await t.tap(find.text('Buang perubahan'));
      await t.pumpAndSettle();
      expect(find.text('1 halaman'), findsOneWidget);
      expect(draft.single['turns'], 0);
      expect(calls.where((c) => c.method == 'edit'), isEmpty);
    },
  );
  testWidgets('dirty crop back can apply and checkpoint edited page', (
    t,
  ) async {
    await crop(t);
    await t.tap(find.text('Putar 0°'));
    await t.pumpAndSettle();
    await t.tap(find.byTooltip('Kembali'));
    await t.pumpAndSettle();
    await t.tap(find.text('Terapkan'));
    await t.pumpAndSettle();
    expect(find.text('1 halaman'), findsOneWidget);
    expect(draft.single['turns'], 1);
    expect(calls.where((c) => c.method == 'checkpoint'), hasLength(1));
  });
  testWidgets(
    'pinch zoom retains horizontal pan and fit width returns to vertical reading',
    (t) async {
      await launch(t);
      await t.tap(find.text('Dokumen Saya'));
      await t.pumpAndSettle();
      await t.tap(find.text('Uji.pdf'));
      await t.pumpAndSettle();
      final center = t.getCenter(find.byKey(const ValueKey('pdf-pages')));
      final left = await t.startGesture(
        center - const Offset(40, 0),
        pointer: 1,
      );
      final right = await t.startGesture(
        center + const Offset(40, 0),
        pointer: 2,
      );
      await t.pump();
      await left.moveBy(const Offset(-20, 0));
      await right.moveBy(const Offset(20, 0));
      await t.pump();
      await left.moveBy(const Offset(-50, 0));
      await right.moveBy(const Offset(50, 0));
      await t.pump();
      await left.up();
      await right.up();
      await t.pumpAndSettle();
      expect(find.byTooltip('Pas lebar layar'), findsOneWidget);
      await t.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(-300, 0),
      );
      await t.pumpAndSettle();
      expect(find.text('1 / 3'), findsOneWidget);
      await t.tap(find.byTooltip('Pas lebar layar'));
      await t.pumpAndSettle();
      await t.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(0, -1150),
      );
      await t.pumpAndSettle();
      expect(find.text('2 / 3'), findsOneWidget);
      expect(t.takeException(), isNull);
    },
  );

  testWidgets(
    'PDF swipe changes page, rename refreshes gallery, share failure remains retryable',
    (t) async {
      await reader(t);
      await t.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(0, -1150),
      );
      await t.pumpAndSettle();
      expect(find.text('2 / 3'), findsOneWidget);
      failShare = true;
      await t.tap(find.text('Kirim'));
      await t.pumpAndSettle();
      expect(find.text('Tidak tersedia.'), findsOneWidget);
      expect(find.byType(PdfReader), findsOneWidget);
      failShare = false;
      await t.tap(find.text('Kirim'));
      await t.pumpAndSettle();
      expect(find.text('Tidak tersedia.'), findsNothing);
      await t.tap(find.byTooltip('Tindakan dokumen'));
      await t.pumpAndSettle();
      await t.tap(find.text('Ganti nama'));
      await t.pumpAndSettle();
      await t.enterText(find.byType(TextField), 'Nama baru');
      await t.tap(find.text('Simpan'));
      await t.pumpAndSettle();
      expect(find.text('Nama baru.pdf'), findsOneWidget);
      await t.binding.handlePopRoute();
      await t.pumpAndSettle();
      expect(find.byType(GridView), findsOneWidget);
      expect(find.text('Nama baru.pdf'), findsOneWidget);
      await t.pump(const Duration(seconds: 2));
      expect(t.takeException(), isNull);
    },
  );
}
