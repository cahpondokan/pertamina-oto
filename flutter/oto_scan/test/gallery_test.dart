import 'reader_fixture.dart';
import 'dart:io';
import 'dart:ui' as ui;
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/document_gallery.dart';
import 'package:oto_scan/main.dart';
import 'package:oto_scan/native_scan.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  late Directory fixture;
  late List<Map> documents;
  final calls = <MethodCall>[];

  setUpAll(() async {
    final fonts =
        '${File(Platform.resolvedExecutable).parent.parent.parent.path}/material_fonts';
    for (final entry
        in {
          'Roboto': 'roboto-regular.ttf',
          'MaterialIcons': 'materialicons-regular.otf',
        }.entries) {
      final file = File('$fonts/${entry.value}');
      if (await file.exists()) {
        final loader = FontLoader(entry.key)..addFont(
          file.readAsBytes().then((bytes) => ByteData.sublistView(bytes)),
        );
        await loader.load();
      }
    }
    fixture = await Directory.systemTemp.createTemp('oto-gallery-fixture-');
    for (var index = 0; index < 4; index++) {
      final recorder = ui.PictureRecorder();
      final canvas = Canvas(recorder);
      canvas.drawColor(Colors.white, BlendMode.src);
      void text(
        String value,
        double x,
        double y,
        double size, {
        bool bold = false,
      }) {
        final painter = TextPainter(
          text: TextSpan(
            text: value,
            style: TextStyle(
              fontFamily: 'Roboto',
              fontSize: size,
              color: const Color(0xFF16334D),
              fontWeight: bold ? FontWeight.bold : FontWeight.normal,
            ),
          ),
          textDirection: TextDirection.ltr,
        )..layout(maxWidth: 410);
        painter.paint(canvas, Offset(x, y));
        painter.dispose();
      }

      text(
        [
          'CATATAN PEMERIKSAAN',
          'NOTA PEMBELIAN',
          'LAPORAN KEGIATAN',
          'BERITA ACARA',
        ][index],
        36,
        45,
        22,
        bold: true,
      );
      text('CONTOH DOKUMEN • DATA SINTETIS', 36, 82, 12);
      text('Tanggal: 22 Oktober 2026', 36, 125, 15);
      text('Nomor: OTO / 00${index + 1}', 36, 150, 15);
      final line =
          Paint()
            ..color = const Color(0xFF9DA8B2)
            ..strokeWidth = 1;
      for (var row = 0; row < 8; row++) {
        canvas.drawLine(
          Offset(36, 220 + row * 36.0),
          Offset(464, 220 + row * 36.0),
          line,
        );
        if (row < 7)
          text(
            '${row + 1}     Item pemeriksaan ${row + 1}          Baik',
            44,
            229 + row * 36.0,
            13,
          );
      }
      for (final x in [36.0, 75.0, 355.0, 464.0]) {
        canvas.drawLine(Offset(x, 220), Offset(x, 472), line);
      }
      text('Catatan:', 36, 505, 15, bold: true);
      canvas.drawLine(const Offset(36, 545), const Offset(464, 545), line);
      canvas.drawLine(const Offset(36, 570), const Offset(464, 570), line);
      text('Petugas', 370, 610, 13);
      final picture = recorder.endRecording();
      final image = await picture.toImage(500, 700);
      final data = await image.toByteData(format: ui.ImageByteFormat.png);
      await File(
        '${fixture.path}/$index.png',
      ).writeAsBytes(data!.buffer.asUint8List());
      image.dispose();
      picture.dispose();
    }
  });
  tearDownAll(() async => fixture.delete(recursive: true));

  setUp(() {
    calls.clear();
    documents = [
      for (var index = 0; index < 4; index++)
        {
          'id': 'document-$index',
          'name':
              [
                'Pemeriksaan rutin.pdf',
                'Nota alat kantor.pdf',
                'Laporan Oktober.pdf',
                'Berita acara.pdf',
              ][index],
          'pages': index + 1,
          'bytes': (index + 1) * 220000,
          'created': DateTime(2026, 10, 22 - index).millisecondsSinceEpoch,
          'preview': '${fixture.path}/$index.png',
        },
    ];
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(NativeScan.channel, (call) async {
          calls.add(call);
          switch (call.method) {
            case 'permission':
              return false;
            case 'recover':
              return '[]';
            case 'list':
              return documents;
            case 'readerOpen':
              return readerMeta(
                documents.firstWhere(
                  (d) => d['id'] == call.arguments['id'],
                )['pages'],
              );
            case 'readerRender':
              return readerRaster();
            case 'thumbnail':
              final id = call.arguments['id'];
              return documents.firstWhere((d) => d['id'] == id)['preview'];
            case 'document':
              return documents.firstWhere(
                (d) => d['id'] == call.arguments['id'],
              );
            default:
              return null;
          }
        });
  });

  Future<void> openGallery(WidgetTester tester) async {
    tester.view.physicalSize = const Size(390, 844);
    tester.view.devicePixelRatio = 1;
    addTearDown(tester.view.resetPhysicalSize);
    addTearDown(tester.view.resetDevicePixelRatio);
    await tester.pumpWidget(const OtoScanApp());
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Dokumen Saya'));
    await tester.pumpAndSettle();
  }

  test(
    'name search handles case, whitespace, multiple words and old documents',
    () {
      final many = <Map>[
        for (var i = 0; i < 150; i++) {'name': 'Dokumen $i.pdf'},
        {'name': 'Nota Pembelian Oktober.pdf'},
      ];
      expect(
        searchDocuments(many, '  OKTOBER   nota ').single['name'],
        'Nota Pembelian Oktober.pdf',
      );
      expect(searchDocuments(many, '   ').length, 151);
      expect(searchDocuments(many, 'tidak ada'), isEmpty);
    },
  );

  testWidgets(
    'gallery uses real thumbnails and opens PDF directly and preserves search on return',
    (tester) async {
      await openGallery(tester);
      expect(find.byType(GridView), findsOneWidget);
      expect(find.byType(ListTile), findsNothing);
      expect(find.text('4 dokumen'), findsOneWidget);
      await tester.enterText(
        find.byKey(const ValueKey('gallery-search')),
        '  NOTA  ',
      );
      await tester.pumpAndSettle();
      expect(find.text('1 dokumen ditemukan'), findsOneWidget);
      expect(find.text('Laporan Oktober.pdf'), findsNothing);
      expect(find.text('Nota alat kantor.pdf'), findsOneWidget);
      expect(
        calls
            .where(
              (c) =>
                  c.method == 'thumbnail' && c.arguments['id'] == 'document-1',
            )
            .length,
        1,
      );
      await tester.tap(find.text('Nota alat kantor.pdf'));
      await tester.pumpAndSettle();
      expect(find.text('Buka'), findsNothing);
      expect(find.text('1 / 2'), findsOneWidget);
      expect(
        calls.lastWhere((c) => c.method == 'readerRender').arguments['detail'],
        isFalse,
      );
      await tester.tap(find.text('Kirim'));
      await tester.pumpAndSettle();
      expect(calls.lastWhere((c) => c.method == 'shareMany').arguments['ids'], [
        'document-1',
      ]);
      await tester.tap(find.byTooltip('Kembali'));
      await tester.pumpAndSettle();
      expect(find.byType(GridView), findsOneWidget);
      expect(find.text('1 dokumen ditemukan'), findsOneWidget);
      expect(
        tester.widget<TextField>(find.byType(TextField)).controller!.text,
        '  NOTA  ',
      );
      expect(calls.where((c) => c.method == 'capture'), isEmpty);
      expect(tester.takeException(), isNull);
    },
  );

  testWidgets(
    'long press selects multiple PDFs; Back clears selection without leaving gallery',
    (tester) async {
      await openGallery(tester);
      await tester.longPress(find.text('Pemeriksaan rutin.pdf'));
      await tester.pumpAndSettle();
      expect(find.text('1 dipilih'), findsOneWidget);
      await tester.tap(find.text('Nota alat kantor.pdf'));
      await tester.pumpAndSettle();
      expect(find.text('2 dipilih'), findsOneWidget);
      expect(find.byType(PdfReader), findsNothing);
      await tester.tap(find.text('Kirim (2)'));
      await tester.pumpAndSettle();
      expect(
        calls.where((c) => c.method == 'shareMany').single.arguments['ids'],
        ['document-0', 'document-1'],
      );
      await tester.binding.handlePopRoute();
      await tester.pumpAndSettle();
      expect(find.byType(GridView), findsOneWidget);
      expect(find.text('Pilih'), findsOneWidget);
      await tester.tap(find.byTooltip('Kembali'));
      await tester.pumpAndSettle();
      expect(find.text('OTO Scan'), findsOneWidget);
      expect(tester.takeException(), isNull);
    },
  );

  testWidgets(
    'bulk delete cancel leaves selection and confirmed delete targets exact IDs',
    (tester) async {
      await openGallery(tester);
      await tester.longPress(find.text('Pemeriksaan rutin.pdf'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Nota alat kantor.pdf'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Hapus'));
      await tester.pumpAndSettle();
      expect(find.text('Hapus 2 dokumen?'), findsOneWidget);
      await tester.tap(find.text('Batal'));
      await tester.pumpAndSettle();
      expect(calls.where((c) => c.method == 'deleteDocuments'), isEmpty);
      expect(find.text('2 dipilih'), findsOneWidget);
      await tester.tap(find.text('Hapus'));
      await tester.pumpAndSettle();
      await tester.tap(
        find.descendant(
          of: find.byType(AlertDialog),
          matching: find.text('Hapus'),
        ),
      );
      await tester.pumpAndSettle();
      expect(
        calls
            .where((c) => c.method == 'deleteDocuments')
            .single
            .arguments['ids'],
        ['document-0', 'document-1'],
      );
      expect(find.text('2 dokumen'), findsOneWidget);
      expect(find.text('Pemeriksaan rutin.pdf'), findsNothing);
      expect(find.text('Laporan Oktober.pdf'), findsOneWidget);
      expect(tester.takeException(), isNull);
    },
  );

  testWidgets(
    'selection survives filtering; tapping selected card unchecks without opening PDF',
    (tester) async {
      await openGallery(tester);
      await tester.tap(find.text('Pilih'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Pemeriksaan rutin.pdf'));
      await tester.pumpAndSettle();
      await tester.enterText(find.byType(TextField), 'nota');
      await tester.pumpAndSettle();
      await tester.tap(find.text('Nota alat kantor.pdf'));
      await tester.pumpAndSettle();
      expect(find.text('2 dipilih'), findsOneWidget);
      await tester.tap(find.text('Nota alat kantor.pdf'));
      await tester.pumpAndSettle();
      await tester.tap(find.text('Kirim (1)'));
      await tester.pumpAndSettle();
      expect(
        calls.where((c) => c.method == 'shareMany').single.arguments['ids'],
        ['document-0'],
      );
      expect(find.byType(PdfReader), findsNothing);
      expect(tester.takeException(), isNull);
    },
  );

  testWidgets('PDF back restores exact gallery scroll position', (
    tester,
  ) async {
    documents = [
      for (var i = 0; i < 40; i++)
        {...documents.first, 'id': 'document-$i', 'name': 'Dokumen $i.pdf'},
    ];
    await openGallery(tester);
    await tester.drag(find.byType(GridView), const Offset(0, -700));
    await tester.pumpAndSettle();
    final scroll = find.descendant(
      of: find.byType(GridView),
      matching: find.byType(Scrollable),
    );
    final offset = tester.state<ScrollableState>(scroll).position.pixels;
    expect(offset, greaterThan(0));
    await tester.tap(
      find.textContaining(RegExp(r'^Dokumen \d+\.pdf$')).hitTestable().first,
    );
    await tester.pumpAndSettle();
    expect(find.byType(PdfReader), findsOneWidget);
    await tester.tap(find.byTooltip('Kembali'));
    await tester.pumpAndSettle();
    expect(
      tester.state<ScrollableState>(scroll).position.pixels,
      closeTo(offset, .1),
    );
  });

  testWidgets('selection toolbar fits narrow screen with enlarged text', (
    tester,
  ) async {
    await openGallery(tester);
    await tester.longPress(find.text('Pemeriksaan rutin.pdf'));
    await tester.pumpAndSettle();
    tester.view.physicalSize = const Size(320, 640);
    tester.platformDispatcher.textScaleFactorTestValue = 2;
    addTearDown(tester.platformDispatcher.clearTextScaleFactorTestValue);
    await tester.pumpAndSettle();
    expect(find.text('Kirim (1)'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('no match can be cleared to restore the gallery', (tester) async {
    await openGallery(tester);
    await tester.enterText(find.byType(TextField), 'Tidak ada');
    await tester.pumpAndSettle();
    expect(find.text('Dokumen tidak ditemukan'), findsOneWidget);
    await tester.tap(find.text('Tampilkan semua'));
    await tester.pumpAndSettle();
    expect(find.text('4 dokumen'), findsOneWidget);
    await tester.enterText(find.byType(TextField), 'Laporan');
    await tester.pumpAndSettle();
    await tester.tap(find.byTooltip('Hapus pencarian'));
    await tester.pumpAndSettle();
    expect(find.text('4 dokumen'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('only nearby thumbnails load and failed preview can be retried', (
    tester,
  ) async {
    documents = [
      for (var i = 0; i < 150; i++)
        {...documents.first, 'id': 'document-$i', 'name': 'Dokumen $i.pdf'},
    ];
    var fail = true;
    TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger
        .setMockMethodCallHandler(NativeScan.channel, (call) async {
          calls.add(call);
          if (call.method == 'recover') return '[]';
          if (call.method == 'permission') return false;
          if (call.method == 'list') return documents;
          if (call.method == 'thumbnail') {
            if (call.arguments['id'] == 'document-0' && fail) {
              throw PlatformException(
                code: 'preview',
                message: 'Uji kegagalan',
              );
            }
            return documents.first['preview'];
          }
          return null;
        });
    await openGallery(tester);
    expect(find.text('150 dokumen'), findsOneWidget);
    expect(calls.where((c) => c.method == 'thumbnail').length, lessThan(12));
    expect(find.byTooltip('Muat ulang pratinjau'), findsOneWidget);
    fail = false;
    await tester.tap(find.byTooltip('Muat ulang pratinjau'));
    await tester.pumpAndSettle();
    expect(
      calls.lastWhere((c) => c.method == 'thumbnail').arguments['refresh'],
      isTrue,
    );
    expect(find.byTooltip('Muat ulang pratinjau'), findsNothing);
    await tester.enterText(find.byType(TextField), 'Dokumen 149');
    await tester.pumpAndSettle();
    expect(find.text('Dokumen 149.pdf'), findsOneWidget);
    expect(
      calls.lastWhere((c) => c.method == 'thumbnail').arguments['id'],
      'document-149',
    );
    expect(tester.takeException(), isNull);
  });

  testWidgets('gallery handles narrow screen, enlarged text and keyboard', (
    tester,
  ) async {
    await openGallery(tester);
    tester.view.physicalSize = const Size(320, 640);
    tester.platformDispatcher.textScaleFactorTestValue = 2;
    addTearDown(tester.platformDispatcher.clearTextScaleFactorTestValue);
    tester.view.viewInsets = const FakeViewPadding(bottom: 220);
    addTearDown(tester.view.resetViewInsets);
    await tester.enterText(find.byType(TextField), 'nota');
    await tester.pumpAndSettle();
    expect(find.text('Nota alat kantor.pdf'), findsOneWidget);
    expect(tester.takeException(), isNull);
  });

  testWidgets('desktop gallery visual evidence uses only synthetic documents', (
    tester,
  ) async {
    await tester.pumpWidget(const MaterialApp(home: SizedBox()));
    final context = tester.element(find.byType(SizedBox).first);
    // Start file decoding outside fake async, before Image.file requests it.
    await tester.runAsync(() async {
      for (final document in documents) {
        final original = File(document['preview']);
        final visual = await original.copy(
          '${fixture.path}/visual-${document['id']}.png',
        );
        document['preview'] = visual.path;
        await precacheImage(
          ResizeImage.resizeIfNeeded(
            480,
            null,
            FileImage(File(document['preview'])),
          ),
          context,
        );
      }
    });
    await openGallery(tester);
    await tester.pumpAndSettle();
    expect(find.byType(Image), findsNWidgets(4));
    final boundary =
        proofKey.currentContext!.findRenderObject()! as RenderRepaintBoundary;
    await tester.runAsync(() async {
      final image = await boundary.toImage(pixelRatio: 2);
      final data = await image.toByteData(format: ui.ImageByteFormat.png);
      await File(
        'evidence/r13-gallery-desktop.png',
      ).writeAsBytes(data!.buffer.asUint8List());
      image.dispose();
    });
    await tester.longPress(find.text('Pemeriksaan rutin.pdf'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Nota alat kantor.pdf'));
    await tester.pumpAndSettle();
    await tester.runAsync(() async {
      final image = await boundary.toImage(pixelRatio: 2);
      final data = await image.toByteData(format: ui.ImageByteFormat.png);
      await File(
        'evidence/r13-selection-desktop.png',
      ).writeAsBytes(data!.buffer.asUint8List());
      image.dispose();
    });
    expect(tester.takeException(), isNull);
  });
}
