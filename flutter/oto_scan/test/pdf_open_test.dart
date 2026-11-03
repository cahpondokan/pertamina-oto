import 'reader_fixture.dart';
import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/main.dart';
import 'package:oto_scan/native_scan.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();
  final messenger =
      TestDefaultBinaryMessengerBinding.instance.defaultBinaryMessenger;
  final calls = <MethodCall>[];
  var pending = true;
  final systemCalls = <MethodCall>[];
  late Future<Map?> Function() open;
  Map document(String id) => {
    'id': id,
    'name': 'Dokumen $id.pdf',
    'pages': 3,
    'external': true,
  };
  setUp(() {
    calls.clear();
    systemCalls.clear();
    messenger.setMockMethodCallHandler(SystemChannels.platform, (call) async {
      systemCalls.add(call);
      return null;
    });
    pending = true;
    open = () async => document('A');
    messenger.setMockMethodCallHandler(NativeScan.channel, (call) async {
      calls.add(call);
      if (call.method == 'hasPendingPdf') return pending;
      if (call.method == 'openPendingPdf') return open();
      if (call.method == 'readerOpen') return readerMeta();
      if (call.method == 'readerRender') return readerRaster();
      if (call.method == 'recover') return '[]';
      if (call.method == 'permission') return false;
      if (call.method == 'list') return <Map>[];
      return null;
    });
  });
  tearDown(() => messenger.setMockMethodCallHandler(NativeScan.channel, null));
  Future<void> signal(WidgetTester tester) async {
    // Engine-to-Dart delivery, not a Dart-to-native mocked method call.
    unawaited(
      messenger.handlePlatformMessage(
        NativeScan.channel.name,
        const StandardMethodCodec().encodeMethodCall(
          const MethodCall('pdfAvailable'),
        ),
        (_) {},
      ),
    );
    await tester.pumpAndSettle();
  }

  testWidgets(
    'cold PDF launch bypasses camera and draft and renders multiple pages',
    (tester) async {
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      expect(find.text('Dokumen A.pdf'), findsOneWidget);
      expect(find.text('1 / 3'), findsOneWidget);
      expect(
        calls.any(
          (c) =>
              ['permission', 'recover', 'capture', 'save'].contains(c.method),
        ),
        isFalse,
      );
      expect(find.byType(PageView), findsNothing);
      expect(find.byKey(const ValueKey('pdf-vertical-list')), findsOneWidget);
      await tester.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(0, -1150),
      );
      await tester.pumpAndSettle();
      expect(find.text('2 / 3'), findsOneWidget);
      expect(
        calls
            .where((c) => c.method == 'readerRender')
            .any((c) => c.arguments['page'] == 1),
        isTrue,
      );
      expect(tester.takeException(), isNull);
    },
  );
  testWidgets(
    'warm PDF event replaces the existing external reader and resets page',
    (tester) async {
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      await tester.drag(
        find.byKey(const ValueKey('pdf-pages')),
        const Offset(0, -1150),
      );
      await tester.pumpAndSettle();
      open = () async => document('B');
      await signal(tester);
      expect(find.text('Dokumen B.pdf'), findsOneWidget);
      expect(find.text('Dokumen A.pdf'), findsNothing);
      expect(find.text('1 / 3'), findsOneWidget);
      expect(calls.where((c) => c.method == 'permission'), isEmpty);
      expect(tester.takeException(), isNull);
    },
  );
  testWidgets(
    'warm external PDF returns to caller without reopening camera or clearing draft',
    (tester) async {
      pending = false;
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      expect(find.text('OTO Scan'), findsOneWidget);
      await signal(tester);
      expect(find.text('Dokumen A.pdf'), findsOneWidget);
      await tester.tap(find.byTooltip('Tutup PDF'));
      await tester.pumpAndSettle();
      expect(
        systemCalls.where((c) => c.method == 'SystemNavigator.pop'),
        hasLength(1),
      );
      expect(calls.where((c) => c.method == 'permission'), hasLength(1));
      expect(
        calls.lastWhere((c) => c.method == 'closeExternal').arguments['id'],
        'A',
      );
      expect(calls.where((c) => c.method == 'checkpoint'), isEmpty);
      expect(tester.takeException(), isNull);
    },
  );
  testWidgets(
    'denied or encrypted PDF shows an actionable error without camera permission',
    (tester) async {
      open =
          () async =>
              throw PlatformException(
                code: 'scan',
                message: 'PDF terkunci atau dilindungi.',
              );
      await tester.pumpWidget(const OtoScanApp());
      await tester.pumpAndSettle();
      expect(find.text('PDF terkunci atau dilindungi.'), findsOneWidget);
      expect(find.byTooltip('Tutup PDF'), findsOneWidget);
      expect(calls.where((c) => c.method == 'permission'), isEmpty);
      expect(tester.takeException(), isNull);
    },
  );
  testWidgets(
    'new intent during an in-flight open wins without showing stale document',
    (tester) async {
      final first = Completer<Map?>();
      open = () => first.future;
      await tester.pumpWidget(const OtoScanApp());
      await tester.pump();
      await tester.pump();
      open = () async => document('B');
      unawaited(
        messenger.handlePlatformMessage(
          NativeScan.channel.name,
          const StandardMethodCodec().encodeMethodCall(
            const MethodCall('pdfAvailable'),
          ),
          (_) {},
        ),
      );
      await tester.pump();
      first.complete(document('A'));
      await tester.pumpAndSettle();
      expect(find.text('Dokumen B.pdf'), findsOneWidget);
      expect(
        calls
            .where((c) => c.method == 'readerOpen')
            .every((c) => c.arguments['id'] == 'B'),
        isTrue,
      );
      expect(tester.takeException(), isNull);
    },
  );
}
