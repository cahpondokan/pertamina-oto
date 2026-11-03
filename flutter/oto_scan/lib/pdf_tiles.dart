import 'dart:math' as math;
import 'dart:ui';
import 'dart:typed_data';

/// Tile coordinates are in the full page raster at [fullWidth], not screen pixels.
class PdfTile {
  PdfTile(
    this.page,
    this.fullWidth,
    this.x,
    this.y,
    this.width,
    this.height, {
    this.bytes,
  });
  final int page, fullWidth, x, y, width, height;
  final Uint8List? bytes;
  String get key => '$page-$fullWidth-$x-$y-$width-$height';
  int get decodedBytes => bytes == null ? 0 : width * height * 4;
  PdfTile loaded(Uint8List data) =>
      PdfTile(page, fullWidth, x, y, width, height, bytes: data);
  Map<String, int> get arguments => {
    'x': x,
    'y': y,
    'width': width,
    'height': height,
  };
}

List<PdfTile> visibleTiles(int page, int fullWidth, double ratio, Rect region) {
  const edge = 512;
  final fullHeight = (fullWidth * ratio).ceil();
  final left = (region.left.clamp(0, 1) * fullWidth).floor();
  final top = (region.top.clamp(0, 1) * fullHeight).floor();
  final right = (region.right.clamp(0, 1) * fullWidth).ceil();
  final bottom = (region.bottom.clamp(0, 1) * fullHeight).ceil();
  final result = <PdfTile>[];
  if (right <= left || bottom <= top) return result;
  for (var y = top ~/ edge * edge; y < bottom; y += edge) {
    for (var x = left ~/ edge * edge; x < right; x += edge) {
      // One shared pixel covers fractional placement seams on Flutter's canvas.
      result.add(
        PdfTile(
          page,
          fullWidth,
          x,
          y,
          math.min(edge + 1, fullWidth - x),
          math.min(edge + 1, fullHeight - y),
        ),
      );
    }
  }
  final cx = (left + right) / 2, cy = (top + bottom) / 2;
  double distance(PdfTile tile) =>
      math.pow(tile.x + tile.width / 2 - cx, 2).toDouble() +
      math.pow(tile.y + tile.height / 2 - cy, 2).toDouble();
  result.sort((a, b) => distance(a).compareTo(distance(b)));
  return result;
}
