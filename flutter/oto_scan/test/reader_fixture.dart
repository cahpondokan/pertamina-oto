import 'dart:io';
import 'dart:typed_data';

final Uint8List readerFixtureBytes = File('assets/oto.png').readAsBytesSync();
Map<String, Object> readerMeta([int pages = 3]) => {
  'pages': pages,
  'width': 595,
  'height': 842,
};
Map<String, Object> readerRaster({Uint8List? bytes, bool hit = false}) => {
  'bytes': bytes ?? readerFixtureBytes,
  'width': 595,
  'height': 842,
  'rasterWidth': 1080,
  'rasterHeight': 1528,
  'cacheHit': hit,
};
