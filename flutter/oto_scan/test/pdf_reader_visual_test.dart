import 'dart:io';
import 'dart:ui' as ui;
import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/native_scan.dart';
import 'package:oto_scan/pdf_reader.dart';
import 'reader_fixture.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  late Uint8List imageBytes;
  setUpAll(() async {
    final fonts =
        '${File(Platform.resolvedExecutable).parent.parent.parent.path}/material_fonts';
    for (final entry
        in {
          'Roboto': 'roboto-regular.ttf',
          'MaterialIcons': 'materialicons-regular.otf',
        }.entries) {
      final file = File('$fonts/${entry.value}');
      final loader = FontLoader(entry.key)
        ..addFont(file.readAsBytes().then(ByteData.sublistView));
      await loader.load();
    }
    imageBytes =
        await File('evidence/r13-quality/text-render.png').readAsBytes();
  });
  testWidgets(
    'two-page vertical reader visual and zoomed cross-page scroll use synthetic document only',
    (t) async {
      t.view.physicalSize = const Size(390, 844);
      t.view.devicePixelRatio = 1;
      addTearDown(t.view.resetPhysicalSize);
      addTearDown(t.view.resetDevicePixelRatio);
      final messenger =
          TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
      final requests = <MethodCall>[];
      messenger.setMockMethodCallHandler(NativeScan.channel, (c) async {
        requests.add(c);
        if (c.method == 'readerOpen') return readerMeta(2);
        if (c.method == 'readerRender') {
          final tile = c.arguments['tile'];
          if (tile != null) {
            final bytes =
                File(
                  'evidence/r13-quality/tiles/${c.arguments['width']}-${tile['x']}-${tile['y']}.png',
                ).readAsBytesSync();
            return {
              ...readerRaster(bytes: bytes),
              'rasterWidth': tile['width'],
              'rasterHeight': tile['height'],
            };
          }
          return readerRaster(bytes: imageBytes);
        }
        return null;
      });
      addTearDown(
        () => messenger.setMockMethodCallHandler(NativeScan.channel, null),
      );
      final boundary = GlobalKey();
      await t.pumpWidget(
        RepaintBoundary(
          key: boundary,
          child: MaterialApp(
            debugShowCheckedModeBanner: false,
            theme: ThemeData(
              colorScheme: ColorScheme.fromSeed(
                seedColor: const Color(0xFF09243C),
              ),
              fontFamily: 'Roboto',
            ),
            home: PdfReader(
              document: {
                'id': 'synthetic',
                'name': 'Pemeriksaan rutin.pdf',
                'pages': 2,
              },
              native: NativeScan(),
            ),
          ),
        ),
      );
      await t.pump();
      await t.runAsync(
        () => precacheImage(
          MemoryImage(imageBytes),
          t.element(find.byType(PdfReader)),
        ),
      );
      await t.pumpAndSettle();
      Future<void> capture(String name) async {
        await t.runAsync(() async {
          final object =
              boundary.currentContext!.findRenderObject()
                  as RenderRepaintBoundary;
          final image = await object.toImage(pixelRatio: 2);
          final data = await image.toByteData(format: ui.ImageByteFormat.png);
          await File('evidence/$name').writeAsBytes(data!.buffer.asUint8List());
          image.dispose();
        });
      }

      await capture('r13-reader-desktop.png');
      final surface = find.byKey(const ValueKey('pdf-pages'));
      final center = t.getCenter(surface);
      final a = await t.startGesture(center - const Offset(45, 0), pointer: 1);
      final b = await t.startGesture(center + const Offset(45, 0), pointer: 2);
      await t.pump();
      await a.moveBy(const Offset(-20, 0));
      await b.moveBy(const Offset(20, 0));
      await t.pump();
      await a.moveBy(const Offset(-45, 0));
      await b.moveBy(const Offset(45, 0));
      await t.pump();
      await a.up();
      await b.up();
      await t.pumpAndSettle();
      await t.pump(const Duration(milliseconds: 250));
      await t.pumpAndSettle();
      expect(find.byTooltip('Pas lebar layar'), findsOneWidget);
      expect(
        requests
            .where((c) => c.method == 'readerRender')
            .any((c) => c.arguments['detail'] == true),
        true,
      );
      final zoomProviders =
          t.widgetList<Image>(find.byType(Image)).map((w) => w.image).toList();
      await t.runAsync(() async {
        for (final provider in zoomProviders) {
          await precacheImage(provider, t.element(find.byType(PdfReader)));
        }
      });
      await t.pumpAndSettle();
      await capture('r13-reader-zoom-desktop.png');
      for (var i = 0; i < 3; i++) {
        await t.drag(surface, const Offset(0, -500));
        await t.pumpAndSettle();
        await t.pump(const Duration(milliseconds: 250));
      }
      expect(find.text('2 / 2'), findsOneWidget);
      await t.tap(find.byTooltip('Pas lebar layar'));
      await t.pumpAndSettle();
      expect(find.byTooltip('Pas lebar layar'), findsNothing);
      // Precaching must use the exact MethodChannel result providers, not the
      // original bytes: channel serialization creates distinct Uint8List keys.
      final providers =
          t.widgetList<Image>(find.byType(Image)).map((w) => w.image).toList();
      await t.runAsync(() async {
        for (final provider in providers) {
          await precacheImage(provider, t.element(find.byType(PdfReader)));
        }
      });
      await t.pumpAndSettle();
      expect(find.text('2 / 2'), findsOneWidget);
      await capture('r13-reader-page2-desktop.png');
      expect(t.takeException(), isNull);
    },
  );
}
