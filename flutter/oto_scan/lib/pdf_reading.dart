import 'dart:async';
import 'package:flutter/painting.dart';
import 'package:flutter/foundation.dart';
import 'native_scan.dart';
import 'pdf_tiles.dart';

class PdfGeometry {
  PdfGeometry(this.count, {this.defaultRatio = 842 / 595});
  int count;
  double defaultRatio;
  final ratios = <int, double>{};
  double ratio(int page) => ratios[page] ?? defaultRatio;
  double top(int page, double width, double zoom) {
    var y = 12.0;
    for (var i = 0; i < page; i++) {
      y += width * ratio(i) + 12;
    }
    return y * zoom;
  }

  double extent(double width, double zoom) =>
      top(count, width, zoom) + 12 * zoom;

  int pageAt(double offset, double width, double zoom) {
    var y = 12.0;
    final logical = offset / zoom;
    for (var i = 0; i < count; i++) {
      y += width * ratio(i) + 12;
      if (y > logical) return i;
    }
    return count - 1;
  }
}

class PdfRaster {
  PdfRaster(this.bytes, this.requested, this.detail, this.width, this.height);
  final Uint8List bytes;
  final int requested, width, height;
  final bool detail;
  int get decodedBytes => width * height * 4;
}

/// One native render at a time. Viewport changes replace the remaining work;
/// visible low-resolution pages precede detail and one next-page prefetch.
class PdfReading extends ChangeNotifier {
  PdfReading(this.native, this.document)
    : geometry = PdfGeometry(document['pages'] as int),
      session = 'r${DateTime.now().microsecondsSinceEpoch}_${_serial++}' {
    unawaited(_open());
  }
  static int _serial = 0;
  final NativeScan native;
  final Map document;
  final String session;
  final PdfGeometry geometry;
  final rasters = <int, PdfRaster>{};
  final errors = <int, String>{};
  final refresh = <int>{};
  final tiles = <String, PdfTile>{};
  final tileErrors = <String>{};
  List<PdfTile> wantedTiles = [];
  int focusPage = 0;
  bool ready = false, closed = false, rendering = false;
  String? openError;
  int first = 0, last = 0, screenWidth = 1080;
  double detailScale = 1;
  int requests = 0, cacheHits = 0, discarded = 0, peakRasterBytes = 0;

  Future<void> _open() async {
    try {
      final meta = await native.call<Map>('readerOpen', {
        'session': session,
        'id': document['id'],
        'external': document['external'] == true,
      });
      if (closed) return;
      if (meta == null) throw const FormatException();
      final count = meta['pages'] as int;
      if (count < 1 || count > 10000) throw const FormatException();
      geometry.count = count;
      geometry.defaultRatio = (meta['height'] as num) / (meta['width'] as num);
      if (!geometry.defaultRatio.isFinite || geometry.defaultRatio <= 0)
        throw const FormatException();
      ready = true;
      notifyListeners();
      unawaited(_pump());
    } catch (_) {
      if (!closed) {
        openError = 'PDF belum dapat dibuka. Kembali lalu coba lagi.';
        notifyListeners();
      }
    }
  }

  void viewport(
    int first,
    int last,
    int width,
    double scale, {
    int? focus,
    Map<int, Rect> regions = const {},
  }) {
    if (closed) return;
    this.first = first.clamp(0, geometry.count - 1);
    this.last = last.clamp(this.first, geometry.count - 1);
    screenWidth = ((width.clamp(384, 1200) / 128).ceil() * 128).clamp(
      384,
      1200,
    );
    detailScale = scale.clamp(1, 6);
    focusPage = (focus ?? first).clamp(this.first, this.last);
    final fullWidth = ((width * detailScale / 128).ceil() * 128).clamp(
      384,
      12000,
    );
    wantedTiles =
        detailScale > 1.15
            ? [
              for (final p in [
                focusPage,
                for (var p = this.first; p <= this.last; p++)
                  if (p != focusPage) p,
              ])
                if (regions[p] != null)
                  ...visibleTiles(p, fullWidth, geometry.ratio(p), regions[p]!),
            ].take(24).toList()
            : [];
    final keys = wantedTiles.map((tile) => tile.key).toSet();
    tileErrors.removeWhere((key) => !keys.contains(key));
    _trim();
    unawaited(_pump());
  }

  (int, int, bool)? _next() {
    for (var i = first; i <= last; i++) {
      if (!errors.containsKey(i) && (rasters[i]?.requested ?? 0) < screenWidth)
        return (i, screenWidth, false);
    }
    // The visible portions of every page get detail; a previous page's small
    // tail cannot monopolize the render queue. Whole-page images stay as a base.
    for (final tile in wantedTiles) {
      if (!tiles.containsKey(tile.key) && !tileErrors.contains(tile.key))
        return (tile.page, tile.fullWidth, true);
    }
    final next = last + 1;
    if (next < geometry.count &&
        !errors.containsKey(next) &&
        !rasters.containsKey(next))
      return (next, screenWidth, false);
    return null;
  }

  Future<void> _pump() async {
    if (!ready || rendering || closed) return;
    rendering = true;
    try {
      while (!closed) {
        final task = _next();
        if (task == null) break;
        final (page, width, detail) = task;
        final tile =
            detail
                ? wantedTiles.firstWhere(
                  (tile) =>
                      !tiles.containsKey(tile.key) &&
                      !tileErrors.contains(tile.key),
                )
                : null;
        try {
          requests++;
          final result = await native.call<Map>('readerRender', {
            'session': session,
            'page': page,
            'width': width,
            'detail': detail,
            if (tile != null) 'tile': tile.arguments,
            'refresh': refresh.remove(page),
          });
          if (closed) return;
          if (result == null ||
              result['bytes'] is! Uint8List ||
              (result['bytes'] as Uint8List).isEmpty)
            throw const FormatException();
          if (result['cacheHit'] == true) cacheHits++;
          final ratio = (result['height'] as num) / (result['width'] as num);
          if (!ratio.isFinite || ratio <= 0) throw const FormatException();
          geometry.ratios[page] = ratio;
          if (tile != null) {
            if (!wantedTiles.any((value) => value.key == tile.key)) {
              discarded++;
              continue;
            }
            tiles[tile.key] = tile.loaded(result['bytes']);
            _trim();
            peakRasterBytes =
                peakRasterBytes > rasterBytes ? peakRasterBytes : rasterBytes;
            notifyListeners();
            continue;
          }
          if (page < first - 1 || page > last + 1) {
            discarded++;
            continue;
          }
          final old = rasters.remove(page);
          if (old != null) unawaited(MemoryImage(old.bytes).evict());
          rasters[page] = PdfRaster(
            result['bytes'],
            width,
            detail,
            result['rasterWidth'],
            result['rasterHeight'],
          );
          _trim();
          peakRasterBytes =
              peakRasterBytes > rasterBytes ? peakRasterBytes : rasterBytes;
          notifyListeners();
        } catch (_) {
          if (closed) return;
          if (tile != null) {
            tileErrors.add(tile.key);
          } else {
            errors[page] = 'Halaman belum dapat ditampilkan.';
          }
          notifyListeners();
        }
      }
    } finally {
      rendering = false;
    }
  }

  int get rasterBytes =>
      rasters.values.fold<int>(0, (sum, image) => sum + image.decodedBytes) +
      tiles.values.fold<int>(0, (sum, tile) => sum + tile.decodedBytes);
  void _trim() {
    final wanted = wantedTiles.map((tile) => tile.key).toSet();
    // Retain tiles during gestures; discard stale zoom levels after settling.
    for (final key in tiles.keys.toList()) {
      final tile = tiles[key]!;
      if (tile.page < first ||
          tile.page > last ||
          (detailScale > 1.15 && !wanted.contains(key))) {
        tiles.remove(key);
        unawaited(MemoryImage(tile.bytes!).evict());
      }
    }
    // Retain nearby screen pages; detailed pages are released after leaving view.
    for (final page in rasters.keys.toList()) {
      final image = rasters[page]!;
      if (page < first - 1 || page > last + 1) {
        rasters.remove(page);
        unawaited(MemoryImage(image.bytes).evict());
      }
    }
    for (final page in rasters.keys.toList()) {
      if (rasterBytes <= 48 * 1024 * 1024) break;
      if (page >= first && page <= last + 1) continue;
      final image = rasters.remove(page)!;
      unawaited(MemoryImage(image.bytes).evict());
    }
  }

  void retry(int page) {
    errors.remove(page);
    tileErrors.clear();
    for (final key in tiles.keys.toList()) {
      if (tiles[key]!.page == page) {
        unawaited(MemoryImage(tiles.remove(key)!.bytes!).evict());
      }
    }
    refresh.add(page);
    final old = rasters.remove(page);
    if (old != null) unawaited(MemoryImage(old.bytes).evict());
    unawaited(_pump());
    notifyListeners();
  }

  @override
  void dispose() {
    closed = true;
    for (final image in rasters.values) {
      unawaited(MemoryImage(image.bytes).evict());
    }
    rasters.clear();
    for (final tile in tiles.values) {
      unawaited(MemoryImage(tile.bytes!).evict());
    }
    tiles.clear();
    wantedTiles.clear();
    unawaited(
      native.call('readerClose', {'session': session}).catchError((_) => null),
    );
    super.dispose();
  }
}
