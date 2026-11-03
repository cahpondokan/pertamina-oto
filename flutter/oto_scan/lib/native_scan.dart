import 'dart:convert';
import 'package:flutter/services.dart';
import 'scan_model.dart';

class NativeScan {
  static const channel = MethodChannel('id.ayani.oto/scan');
  Future<T?> call<T>(String method, [Map<String, dynamic>? args]) =>
      channel.invokeMethod<T>(method, args);
  Future<ScanPage> capture([Map<String, dynamic>? selection]) async =>
      ScanPage.fromMap((await call<Map>('capture', selection))!);
  Future<ScanPage> synthetic() async =>
      ScanPage.fromMap((await call<Map>('synthetic'))!);
  Future<List<ScanPage>?> googleScan(int limit) async {
    final result = await call<List>('googleScan', {'limit': limit});
    if (result == null) return null;
    if (result.isEmpty || result.length > limit) {
      throw const FormatException('Invalid scanner page count');
    }
    return [for (final page in result) ScanPage.fromMap(page as Map)];
  }

  Future<ScanPage> edit(ScanPage page, Map<String, dynamic> changes) async =>
      ScanPage.fromMap(
        (await call<Map>('edit', {...page.toMap(), ...changes}))!,
      );
  Future<void> checkpoint(List<ScanPage> pages) async {
    await call('checkpoint', {
      'json': jsonEncode([for (final p in pages) p.toMap()]),
    });
  }

  Future<List<ScanPage>> recover() async {
    final raw = await call<String>('recover');
    return [for (final p in jsonDecode(raw ?? '[]')) ScanPage.fromMap(p)];
  }
}
