import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:oto_scan/app_config.dart';
import 'package:oto_scan/privacy_screen.dart';

void main() {
  test('Google scanner can use the main application identity', () {
    expect(isMLKitTrial, isFalse);
    expect(applicationName, 'OTO Scan');
    expect(googlePrivacy, contains('Google menerima data teknis SDK'));
    expect(googlePrivacy, isNot(contains('Versi uji terpasang terpisah')));
  });

  testWidgets('Google release shows its SDK disclosure and original app name', (
    tester,
  ) async {
    await tester.pumpWidget(
      const MaterialApp(home: PrivacyScreen(googleScanner: true)),
    );
    expect(find.text('OTO Scan'), findsOneWidget);
    expect(find.text('OTO Scan Uji'), findsNothing);
    expect(find.text(googlePrivacy), findsOneWidget);
    expect(find.text(privacyPolicy), findsNothing);
  });
}
