import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/native_scan.dart';
import 'package:oto_scan/pdf_canvas.dart';
import 'package:oto_scan/pdf_reading.dart';
import 'reader_fixture.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  late List<Size> sizes;
  late PdfReading reading;
  late Future<Map?> Function(MethodCall) render;
  setUp(() {
    sizes = [const Size(842, 595)];
    render = (call) async {
      final size = sizes[call.arguments['page'] as int];
      return {...readerRaster(), 'width': size.width, 'height': size.height};
    };
    messenger.setMockMethodCallHandler(NativeScan.channel, (call) async {
      if (call.method == 'readerOpen')
        return {
          'pages': sizes.length,
          'width': sizes.first.width,
          'height': sizes.first.height,
        };
      if (call.method == 'readerRender') return render(call);
      return null;
    });
  });
  tearDown(() => messenger.setMockMethodCallHandler(NativeScan.channel, null));
  Future<void> show(WidgetTester t) async {
    t.view.physicalSize = const Size(390, 800);
    t.view.devicePixelRatio = 1;
    addTearDown(t.view.resetPhysicalSize);
    addTearDown(t.view.resetDevicePixelRatio);
    reading = PdfReading(NativeScan(), {
      'id': 'synthetic',
      'pages': sizes.length,
    });
    await t.pumpWidget(
      MaterialApp(home: Scaffold(body: PdfCanvas(reading: reading))),
    );
    await t.pumpAndSettle();
    addTearDown(reading.dispose);
  }

  Finder page(int index) => find.byKey(ValueKey('pdf-page-$index'));
  GestureDetector gestures(WidgetTester t) =>
      t.widget(find.byKey(const ValueKey('pdf-pages')));
  Offset point(WidgetTester t, int index, Offset fraction) {
    final rect = t.getRect(page(index));
    return rect.topLeft +
        Offset(rect.width * fraction.dx, rect.height * fraction.dy);
  }

  Future<void> zoom(WidgetTester t, Offset focal, double scale) async {
    gestures(t).onScaleStart!(
      ScaleStartDetails(
        localFocalPoint: focal,
        focalPoint: focal,
        pointerCount: 2,
      ),
    );
    gestures(t).onScaleUpdate!(
      ScaleUpdateDetails(
        localFocalPoint: focal,
        focalPoint: focal,
        scale: scale,
        pointerCount: 2,
      ),
    );
    await t.pump();
    await t.pump();
  }

  for (final size in [const Size(595, 842), const Size(842, 595)]) {
    testWidgets('zoom preserves exact document point on single $size page', (
      t,
    ) async {
      sizes = [size];
      await show(t);
      const fraction = Offset(.65, .45);
      final focal = point(t, 0, fraction);
      await zoom(t, focal, 2.5);
      expect(
        (point(t, 0, fraction) - focal).distance,
        lessThan(1),
        reason: 'The same text must stay under the pinch focus',
      );
      expect(t.getSize(page(0)).aspectRatio, closeTo(size.aspectRatio, .001));
      gestures(t).onScaleEnd!(ScaleEndDetails());
      await t.pumpAndSettle();
      expect((point(t, 0, fraction) - focal).distance, lessThan(1));
      await t.pumpWidget(const SizedBox());
    });
  }

  testWidgets('lifting a pinch finger does not fling the document away', (
    t,
  ) async {
    sizes = List.filled(3, const Size(595, 842));
    await show(t);
    const fraction = Offset(.55, .45);
    final focal = point(t, 0, fraction);
    await zoom(t, focal, 2.5);
    final before = point(t, 0, fraction);
    gestures(t).onScaleEnd!(
      ScaleEndDetails(
        velocity: const Velocity(pixelsPerSecond: Offset(0, -1400)),
        pointerCount: 1,
      ),
    );
    await t.pump();
    await t.pump(const Duration(milliseconds: 200));
    expect((point(t, 0, fraction) - before).distance, lessThan(1));
    await t.pumpWidget(const SizedBox());
  });

  testWidgets('late landscape metadata keeps the pinched page point stable', (
    t,
  ) async {
    sizes = [const Size(595, 842), const Size(842, 595), const Size(595, 842)];
    final pending = Completer<Map?>();
    final normalRender = render;
    render = (c) => c.arguments['page'] == 1 ? pending.future : normalRender(c);
    await show(t);
    final list = t.widget<ListView>(
      find.byKey(const ValueKey('pdf-vertical-list')),
    );
    list.controller!.jumpTo(400);
    await t.pumpAndSettle();
    const fraction = Offset(.55, .35);
    final focal = point(t, 1, fraction);
    await zoom(t, focal, 2);
    pending.complete({...readerRaster(), 'width': 842, 'height': 595});
    await t.pump();
    await t.pump();
    await t.pump();
    expect((point(t, 1, fraction) - focal).distance, lessThan(1));
    expect(t.getSize(page(1)).aspectRatio, closeTo(842 / 595, .001));
    gestures(t).onScaleEnd!(ScaleEndDetails());
    await t.pumpWidget(const SizedBox());
  });

  testWidgets('rapid pinch updates use the latest focal position only', (
    t,
  ) async {
    sizes = [const Size(842, 595), const Size(595, 842)];
    await show(t);
    const fraction = Offset(.6, .5);
    final initial = point(t, 0, fraction);
    gestures(t).onScaleStart!(
      ScaleStartDetails(localFocalPoint: initial, pointerCount: 2),
    );
    final finalFocus = initial + const Offset(-20, 15);
    for (final scale in [1.4, 2.2, 3.2]) {
      gestures(t).onScaleUpdate!(
        ScaleUpdateDetails(
          localFocalPoint: finalFocus,
          scale: scale,
          pointerCount: 2,
        ),
      );
    }
    await t.pump();
    await t.pump();
    expect((point(t, 0, fraction) - finalFocus).distance, lessThan(1));
    gestures(t).onScaleEnd!(ScaleEndDetails());
    await t.pumpAndSettle();
    expect((point(t, 0, fraction) - finalFocus).distance, lessThan(1));
    await t.pumpWidget(const SizedBox());
  });

  for (final landscape in [false, true]) {
    testWidgets(
      'real diagonal pinch and release stay anchored, landscape=$landscape',
      (t) async {
        sizes = [landscape ? const Size(842, 595) : const Size(595, 842)];
        await show(t);
        final focus = point(t, 0, const Offset(.55, .5));
        final a = await t.startGesture(focus - const Offset(35, 0), pointer: 1);
        final b = await t.startGesture(focus + const Offset(35, 0), pointer: 2);
        await a.moveTo(focus - const Offset(60, 20));
        await b.moveTo(focus + const Offset(60, 20));
        await t.pump();
        await t.pump();
        final rect = t.getRect(page(0));
        final fraction = Offset(
          (focus.dx - rect.left) / rect.width,
          (focus.dy - rect.top) / rect.height,
        );
        for (var step = 1; step <= 4; step++) {
          final span = Offset(60 + step * 12, 20 + step * 5);
          await a.moveTo(focus - span);
          await b.moveTo(focus + span);
          await t.pump(const Duration(milliseconds: 16));
          await t.pump();
          expect((point(t, 0, fraction) - focus).distance, lessThan(1));
        }
        await a.up();
        await b.up();
        await t.pump();
        await t.pump(const Duration(milliseconds: 250));
        await t.pumpAndSettle();
        expect((point(t, 0, fraction) - focus).distance, lessThan(1));
        expect(
          t.getSize(page(0)).aspectRatio,
          closeTo(sizes.first.aspectRatio, .001),
        );
        await t.pumpWidget(const SizedBox());
      },
    );
  }

  testWidgets(
    'mixed page zoom resets normally and one-finger scroll still flings',
    (t) async {
      sizes = [
        const Size(595, 842),
        const Size(842, 595),
        const Size(595, 842),
        const Size(595, 842),
      ];
      await show(t);
      final list = t.widget<ListView>(
        find.byKey(const ValueKey('pdf-vertical-list')),
      );
      list.controller!.jumpTo(480);
      await t.pumpAndSettle();
      final focal = point(t, 1, const Offset(.6, .6));
      await zoom(t, focal, 3);
      gestures(t).onScaleEnd!(ScaleEndDetails());
      await t.pumpAndSettle();
      final before = t.getRect(page(1));
      final fractionAtTop = -before.top / before.height;
      await t.tap(find.byTooltip('Pas lebar layar'));
      await t.pumpAndSettle();
      final after = t.getRect(page(1));
      expect(after.width, closeTo(366, .1));
      expect(after.size.aspectRatio, closeTo(842 / 595, .001));
      expect(-after.top / after.height, closeTo(fractionAtTop, .001));
      gestures(t).onScaleStart!(
        ScaleStartDetails(localFocalPoint: focal, pointerCount: 1),
      );
      gestures(t).onScaleUpdate!(
        ScaleUpdateDetails(
          localFocalPoint: focal - const Offset(0, 40),
          focalPointDelta: const Offset(0, -40),
          pointerCount: 1,
        ),
      );
      final prior = list.controller!.offset;
      gestures(t).onScaleEnd!(
        ScaleEndDetails(
          velocity: const Velocity(pixelsPerSecond: Offset(0, -900)),
          pointerCount: 0,
        ),
      );
      await t.pump();
      await t.pump(const Duration(milliseconds: 150));
      expect(list.controller!.offset, greaterThan(prior));
      await t.pumpWidget(const SizedBox());
    },
  );
}
