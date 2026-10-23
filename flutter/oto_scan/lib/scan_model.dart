import 'dart:ui';

class ScanPage {
  final String source, image, filter, aspect;
  final int width, height, turns;
  final List<Offset> corners;
  final bool detected;
  final String? qualityWarning;
  final String? orientationWarning;
  final bool proposed;
  const ScanPage({
    required this.source,
    required this.image,
    required this.width,
    required this.height,
    required this.corners,
    required this.detected,
    this.turns = 0,
    this.filter = 'color',
    this.qualityWarning,
    this.orientationWarning,
    this.proposed = false,
    this.aspect = 'free',
  });
  factory ScanPage.fromMap(Map<dynamic, dynamic> m) => ScanPage(
    source: m['source'],
    image: m['image'],
    width: m['width'],
    height: m['height'],
    corners: [
      for (final p in m['corners'])
        Offset((p[0] as num).toDouble(), (p[1] as num).toDouble()),
    ],
    detected: m['detected'] == true,
    turns: m['turns'] ?? 0,
    filter: m['filter'] ?? 'color',
    qualityWarning: m['qualityWarning'],
    orientationWarning: m['orientationWarning'],
    proposed: m['proposed'] == true,
    aspect: m['aspect'] ?? 'free',
  );
  Map<String, dynamic> toMap() => {
    'source': source,
    'image': image,
    'width': width,
    'height': height,
    'corners': [
      for (final p in corners) [p.dx, p.dy],
    ],
    'detected': detected,
    'turns': turns,
    'filter': filter,
    if (qualityWarning != null) 'qualityWarning': qualityWarning,
    if (orientationWarning != null) 'orientationWarning': orientationWarning,
    'proposed': proposed,
    'aspect': aspect,
  };
}

bool validQuad(List<Offset> q) {
  if (q.length != 4 ||
      q.any(
        (p) =>
            !p.dx.isFinite ||
            !p.dy.isFinite ||
            p.dx < 0 ||
            p.dx > 1 ||
            p.dy < 0 ||
            p.dy > 1,
      ))
    return false;
  double area = 0;
  for (int i = 0; i < 4; i++) {
    final a = q[i], b = q[(i + 1) % 4], c = q[(i + 2) % 4];
    if ((b.dx - a.dx) * (c.dy - b.dy) - (b.dy - a.dy) * (c.dx - b.dx) < .0001)
      return false;
    area += a.dx * b.dy - a.dy * b.dx;
  }
  return area > .0008;
}

List<T> moved<T>(List<T> items, int from, int to) {
  final copy = [...items], item = copy.removeAt(from);
  copy.insert(to, item);
  return copy;
}
