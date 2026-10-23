import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/native_scan.dart';
import 'package:oto_scan/pdf_reading.dart';
import 'package:oto_scan/pdf_reader.dart';
import 'reader_fixture.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  final calls = <MethodCall>[];
  late Future<Map?> Function(MethodCall) render;
  var pages = 20;
  setUp(() {
    calls.clear();
    pages = 20;
    render = (_) async => readerRaster();
    messenger.setMockMethodCallHandler(NativeScan.channel, (c) async {
      calls.add(c);
      if (c.method == 'readerOpen') return readerMeta(pages);
      if (c.method == 'readerRender') return render(c);
      return null;
    });
  });
  tearDown(() => messenger.setMockMethodCallHandler(NativeScan.channel, null));
  PdfReading create() => PdfReading(NativeScan(), {
    'id': 'fixture',
    'name': 'Uji.pdf',
    'pages': pages,
  });
  Iterable<MethodCall> getRenders() =>
      calls.where((c) => c.method == 'readerRender');

  testWidgets(
    'first viewport renders screen then one prefetch, never all 20 pages',
    (t) async {
      final r = create();
      addTearDown(r.dispose);
      r.viewport(0, 0, 1080, 1);
      await t.pump();
      await t.pump();
      expect(getRenders().map((c) => c.arguments['page']), [0, 1]);
      expect(getRenders().every((c) => c.arguments['detail'] == false), isTrue);
      final count = r.requests;
      r.viewport(0, 0, 1080, 1);
      await t.pump();
      expect(r.requests, count);
      expect(r.rasters.keys, [0, 1]);
    },
  );
  testWidgets(
    'fast scroll discards obsolete in-flight output and prioritizes new viewport',
    (t) async {
      final pending = Completer<Map?>();
      render =
          (c) =>
              c.arguments['page'] == 0
                  ? pending.future
                  : Future.value(readerRaster());
      final r = create();
      addTearDown(r.dispose);
      r.viewport(0, 0, 1080, 1);
      await t.pump();
      r.viewport(10, 10, 1080, 1);
      pending.complete(readerRaster());
      await t.pump();
      await t.pump();
      expect(getRenders().map((c) => c.arguments['page']), [0, 10, 11]);
      expect(r.discarded, 1);
      expect(r.rasters.keys, [10, 11]);
    },
  );
  testWidgets('closing while render is pending cancels remaining page work', (
    t,
  ) async {
    final pending = Completer<Map?>();
    render = (_) => pending.future;
    final r = create();
    await t.pump();
    r.dispose();
    pending.complete(readerRaster());
    await t.pump();
    expect(getRenders(), hasLength(1));
    expect(r.rasters, isEmpty);
    expect(
      calls.where((c) => c.method == 'readerClose').single.arguments['session'],
      r.session,
    );
  });
  testWidgets(
    'zoom tiles prioritize the page being read and cover both visible pages',
    (t) async {
      final r = create();
      addTearDown(r.dispose);
      r.viewport(0, 1, 1080, 1);
      await t.pump();
      final screen = r.rasters[1];
      final pending = Completer<Map?>();
      render =
          (c) =>
              c.arguments['detail'] == true
                  ? pending.future
                  : Future.value(readerRaster());
      r.viewport(
        0,
        1,
        1080,
        3,
        focus: 1,
        regions: {
          0: const Rect.fromLTRB(0, .98, .3, 1),
          1: const Rect.fromLTRB(0, 0, .3, .2),
        },
      );
      await t.pump();
      expect(r.rasters[1], same(screen));
      expect(getRenders().last.arguments['page'], 1);
      expect(getRenders().last.arguments['tile'], isNotNull);
      pending.complete(readerRaster());
      await t.pump();
      expect(r.tiles.values.map((v) => v.page).toSet(), {0, 1});
      expect(r.rasters.values.every((v) => !v.detail), true);
      expect(
        r.tiles.values.every((v) => v.width <= 513 && v.height <= 513),
        true,
      );
      expect(r.rasterBytes, lessThanOrEqualTo(48 * 1024 * 1024));
      r.viewport(8, 8, 1080, 1);
      await t.pump();
      expect(r.tiles, isEmpty);
      expect(r.rasters.keys, [8, 9]);
    },
  );
  testWidgets('stale zoom tile cannot replace new viewport detail', (t) async {
    final r = create();
    addTearDown(r.dispose);
    await t.pump();
    final pending = Completer<Map?>();
    render =
        (c) =>
            c.arguments['detail'] == true
                ? pending.future
                : Future.value(readerRaster());
    r.viewport(
      0,
      0,
      1080,
      6,
      regions: {0: const Rect.fromLTRB(0, 0, .16, .15)},
    );
    await t.pump();
    final obsolete = r.wantedTiles.first.key;
    r.viewport(0, 0, 1080, 6, regions: {0: const Rect.fromLTRB(.8, .8, 1, 1)});
    pending.complete(readerRaster());
    await t.pump();
    expect(r.tiles.containsKey(obsolete), false);
    expect(r.discarded, greaterThan(0));
    expect(r.tiles.values.every((tile) => tile.x > 4000), true);
  });
  testWidgets('tile failure retains screen and can retry', (t) async {
    final r = create();
    addTearDown(r.dispose);
    await t.pump();
    render = (c) async {
      if (c.arguments['detail'] == true) throw PlatformException(code: 'read');
      return readerRaster();
    };
    r.viewport(0, 0, 1080, 2, regions: {0: const Rect.fromLTRB(0, 0, .2, .1)});
    await t.pump();
    final count = r.requests;
    expect(r.tileErrors, isNotEmpty);
    expect(r.rasters[0], isNotNull);
    await t.pump();
    expect(r.requests, count);
    render = (_) async => readerRaster();
    r.retry(0);
    await t.pump();
    expect(r.tileErrors, isEmpty);
    expect(r.tiles, isNotEmpty);
  });
  testWidgets('retry refreshes corrupt cache and warm results are counted', (
    t,
  ) async {
    final r = create();
    addTearDown(r.dispose);
    await t.pump();
    render = (_) async => readerRaster(hit: true);
    r.retry(0);
    await t.pump();
    expect(getRenders().last.arguments['refresh'], true);
    expect(r.cacheHits, 1);
    expect(r.errors, isEmpty);
  });
  testWidgets('failed page is retryable and does not start endless requests', (
    t,
  ) async {
    render = (_) async => throw PlatformException(code: 'read');
    final r = create();
    addTearDown(r.dispose);
    await t.pump();
    expect(r.errors.keys, [0, 1]);
    expect(r.requests, 2);
    render = (_) async => readerRaster();
    r.retry(0);
    await t.pump();
    expect(r.rasters.containsKey(0), true);
    expect(r.requests, 3);
  });
  testWidgets(
    'vertical reader shows two page edges together and back works during pending render',
    (t) async {
      pages = 2;
      final pending = Completer<Map?>();
      render = (_) => pending.future;
      t.view.physicalSize = const Size(390, 844);
      t.view.devicePixelRatio = 1;
      addTearDown(t.view.resetPhysicalSize);
      addTearDown(t.view.resetDevicePixelRatio);
      var closed = false;
      await t.pumpWidget(
        MaterialApp(
          home: PdfReader(
            document: {'id': 'fixture', 'name': 'Dua halaman.pdf', 'pages': 2},
            native: NativeScan(),
            onClose: () => closed = true,
          ),
        ),
      );
      await t.pump();
      expect(find.byType(PageView), findsNothing);
      final first = t.getRect(find.byKey(const ValueKey('pdf-page-0')));
      final second = t.getRect(find.byKey(const ValueKey('pdf-page-1')));
      expect(second.top - first.bottom, closeTo(12, 1));
      expect(second.top, lessThan(800));
      await t.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(0, -400),
      );
      await t.pumpAndSettle();
      expect(
        t.getTopLeft(find.byKey(const ValueKey('pdf-page-1'))).dy,
        lessThan(second.top),
      );
      await t.tap(find.byTooltip('Kembali'));
      await t.pump();
      expect(closed, true);
      await t.pumpWidget(const SizedBox());
      pending.complete(readerRaster());
      await t.pump();
      expect(t.takeException(), isNull);
    },
  );
}
