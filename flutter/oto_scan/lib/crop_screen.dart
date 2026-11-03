import 'dart:io';
import 'dart:math' as math;
import 'dart:ui' as ui;
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart';
import 'native_scan.dart';
import 'scan_model.dart';
import 'main.dart' show proof;

const cropRed = Color(0xFFF5102E);

class CornerPainter extends CustomPainter {
  final List<Offset>? corners;
  final bool guide;
  CornerPainter(this.corners, {this.guide = false});
  @override
  void paint(Canvas c, Size s) {
    final points =
        corners ??
        const [
          Offset(.12, .1),
          Offset(.88, .1),
          Offset(.88, .9),
          Offset(.12, .9),
        ];
    final q = [for (final p in points) Offset(p.dx * s.width, p.dy * s.height)];
    final paint =
        Paint()
          ..color = corners == null ? Colors.white54 : cropRed
          ..strokeWidth = 2
          ..style = PaintingStyle.stroke;
    if (!guide || corners != null) {
      final path = Path()..moveTo(q[0].dx, q[0].dy);
      for (final p in q.skip(1)) {
        path.lineTo(p.dx, p.dy);
      }
      path.close();
      c.drawPath(path, paint);
    } else {
      for (var i = 0; i < 4; i++) {
        for (final other in [q[(i + 1) % 4], q[(i + 3) % 4]]) {
          final delta = other - q[i];
          c.drawLine(q[i], q[i] + delta / delta.distance * 22, paint);
        }
      }
    }
    if (!guide)
      for (final p in q) {
        c.drawCircle(p, 7, Paint()..color = cropRed);
        c.drawCircle(
          p,
          7,
          Paint()
            ..color = Colors.white
            ..style = PaintingStyle.stroke
            ..strokeWidth = 2,
        );
      }
  }

  @override
  bool shouldRepaint(CornerPainter old) => true;
}

class CropScreen extends StatefulWidget {
  final ScanPage page;
  final NativeScan native;
  const CropScreen({super.key, required this.page, required this.native});
  @override
  State<CropScreen> createState() => _CropScreenState();
}

class _CropScreenState extends State<CropScreen> {
  late List<Offset> corners = [...widget.page.corners];
  late int turns = widget.page.turns;
  late String filter = widget.page.filter;
  late String aspect = widget.page.aspect;
  int? dragging;
  bool busy = false, askingBack = false;
  String? error;
  int w = 1, h = 1;
  @override
  void initState() {
    super.initState();
    _dimensions();
  }

  Future<void> _dimensions() async {
    try {
      final bytes = await File(widget.page.source).readAsBytes();
      final codec = await ui.instantiateImageCodec(bytes);
      final frame = await codec.getNextFrame();
      if (mounted)
        setState(() {
          w = frame.image.width;
          h = frame.image.height;
        });
      frame.image.dispose();
      codec.dispose();
      proof('crop');
    } catch (_) {
      if (mounted)
        setState(
          () => error = 'Foto sumber tidak tersedia. Ambil ulang halaman.',
        );
    }
  }

  Future<void> _back() async {
    if (busy || askingBack) return;
    final dirty =
        turns != widget.page.turns ||
        filter != widget.page.filter ||
        aspect != widget.page.aspect ||
        !listEquals(corners, widget.page.corners);
    if (!dirty) {
      Navigator.pop(context);
      return;
    }
    askingBack = true;
    final choice = await showDialog<String>(
      context: context,
      builder:
          (c) => AlertDialog(
            title: const Text('Simpan perubahan potongan?'),
            content: const Text(
              'Pilih apa yang dilakukan dengan perubahan pada halaman ini.',
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(c),
                child: const Text('Batal kembali'),
              ),
              TextButton(
                onPressed: () => Navigator.pop(c, 'discard'),
                child: const Text('Buang perubahan'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(c, 'apply'),
                child: const Text('Terapkan'),
              ),
            ],
          ),
    );
    askingBack = false;
    if (!mounted) return;
    if (choice == 'discard') Navigator.pop(context);
    if (choice == 'apply') await _apply();
  }

  Future<void> _apply() async {
    if (!validQuad(corners)) {
      setState(() => error = 'Pastikan empat sudut tidak bersilangan.');
      return;
    }
    setState(() => busy = true);
    try {
      final p = await widget.native.edit(widget.page, {
        'corners': [
          for (final p in corners) [p.dx, p.dy],
        ],
        'turns': turns,
        'filter': filter,
        'aspect': aspect,
      });
      if (mounted) Navigator.pop(context, p);
    } catch (_) {
      if (mounted)
        setState(() {
          busy = false;
          error = 'Belum berhasil memotong. Geser sudut lalu coba lagi.';
        });
    }
  }

  @override
  Widget build(BuildContext context) => PopScope(
    canPop: false,
    onPopInvokedWithResult: (didPop, _) {
      if (!didPop) _back();
    },
    child: Scaffold(
      appBar: AppBar(
        title: const Text('Rapikan'),
        leading: IconButton(
          tooltip: 'Kembali',
          onPressed: _back,
          icon: const Icon(Icons.arrow_back),
        ),
      ),
      body: SafeArea(
        top: false,
        child: Column(
          children: [
            if (error != null)
              Padding(
                padding: const EdgeInsets.all(10),
                child: Text(error!, style: const TextStyle(color: cropRed)),
              ),
            Expanded(
              child: LayoutBuilder(
                builder: (_, bounds) {
                  final scale = math.min(
                    (bounds.maxWidth - 24) / (turns.isEven ? w : h),
                    (bounds.maxHeight - 24) / (turns.isEven ? h : w),
                  );
                  final size = Size(w * scale, h * scale);
                  return Center(
                    child: RotatedBox(
                      quarterTurns: turns,
                      child: SizedBox(
                        width: size.width,
                        height: size.height,
                        child: GestureDetector(
                          onPanStart:
                              busy
                                  ? null
                                  : (d) {
                                    var best = 44.0;
                                    dragging = null;
                                    for (var i = 0; i < 4; i++) {
                                      final dist =
                                          (Offset(
                                                    corners[i].dx * size.width,
                                                    corners[i].dy * size.height,
                                                  ) -
                                                  d.localPosition)
                                              .distance;
                                      if (dist < best) {
                                        best = dist;
                                        dragging = i;
                                      }
                                    }
                                  },
                          onPanUpdate:
                              busy
                                  ? null
                                  : (d) {
                                    if (dragging == null) return;
                                    final next = [...corners];
                                    next[dragging!] = Offset(
                                      (d.localPosition.dx / size.width).clamp(
                                        0.0,
                                        1.0,
                                      ),
                                      (d.localPosition.dy / size.height).clamp(
                                        0.0,
                                        1.0,
                                      ),
                                    );
                                    if (validQuad(next))
                                      setState(() => corners = next);
                                  },
                          onPanEnd: (_) => dragging = null,
                          child: Stack(
                            fit: StackFit.expand,
                            children: [
                              Image.file(
                                File(widget.page.source),
                                fit: BoxFit.fill,
                              ),
                              CustomPaint(painter: CornerPainter(corners)),
                            ],
                          ),
                        ),
                      ),
                    ),
                  );
                },
              ),
            ),
            Text(
              widget.page.proposed
                  ? 'Usulan dari preview belum terverifikasi. Sesuaikan empat sudut.'
                  : 'Geser empat sudut sesuai tepi objek',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 12),
            ),
            PopupMenuButton<String>(
              key: const ValueKey('crop-aspect'),
              tooltip: 'Rasio hasil',
              enabled: !busy,
              initialValue: aspect,
              onSelected: (value) => setState(() => aspect = value),
              itemBuilder:
                  (_) => const [
                    PopupMenuItem(
                      value: 'free',
                      child: Text('Bebas · perkiraan dari foto'),
                    ),
                    PopupMenuItem(value: 'a4', child: Text('A4 potret')),
                    PopupMenuItem(
                      value: 'a4-landscape',
                      child: Text('A4 landscape'),
                    ),
                    PopupMenuItem(
                      value: 'card',
                      child: Text('Kartu landscape'),
                    ),
                    PopupMenuItem(
                      value: 'card-portrait',
                      child: Text('Kartu potret'),
                    ),
                  ],
              child: Padding(
                padding: const EdgeInsets.all(8),
                child: Text(
                  'Rasio: ${switch (aspect) {
                    'a4' => 'A4 potret',
                    'a4-landscape' => 'A4 landscape',
                    'card' => 'Kartu landscape',
                    'card-portrait' => 'Kartu potret',
                    _ => 'Bebas (perkiraan foto)',
                  }}',
                ),
              ),
            ),
            Wrap(
              alignment: WrapAlignment.spaceEvenly,
              children: [
                TextButton.icon(
                  onPressed:
                      busy
                          ? null
                          : () => setState(
                            () => corners = [...widget.page.corners],
                          ),
                  icon: const Icon(Icons.crop),
                  label: const Text('Potong'),
                ),
                TextButton.icon(
                  onPressed:
                      busy
                          ? null
                          : () => setState(() {
                            turns = (turns + 1) % 4;
                            aspect = switch (aspect) {
                              'a4' => 'a4-landscape',
                              'a4-landscape' => 'a4',
                              'card' => 'card-portrait',
                              'card-portrait' => 'card',
                              _ => 'free',
                            };
                          }),
                  icon: const Icon(Icons.rotate_right),
                  label: Text('Putar ${turns * 90}°'),
                ),
                PopupMenuButton<String>(
                  enabled: !busy,
                  tooltip: 'Filter',
                  initialValue: filter,
                  onSelected: (f) => setState(() => filter = f),
                  itemBuilder:
                      (_) => [
                        const PopupMenuItem(
                          value: 'clear',
                          child: Text('Jernih'),
                        ),
                        const PopupMenuItem(
                          value: 'color',
                          child: Text('Warna asli'),
                        ),
                        const PopupMenuItem(
                          value: 'gray',
                          child: Text('Abu-abu'),
                        ),
                        const PopupMenuItem(
                          value: 'bw',
                          child: Text('Hitam putih'),
                        ),
                      ],
                  child: Padding(
                    padding: const EdgeInsets.all(14),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        const Icon(Icons.auto_fix_high, color: cropRed),
                        const SizedBox(width: 6),
                        Text(switch (filter) {
                          'clear' => 'Jernih',
                          'gray' => 'Abu-abu',
                          'bw' => 'H/P',
                          _ => 'Warna',
                        }),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(16, 4, 16, 16),
              child: SizedBox(
                width: double.infinity,
                child: FilledButton(
                  onPressed: busy ? null : _apply,
                  child: Text(busy ? 'Memotong…' : 'Gunakan halaman'),
                ),
              ),
            ),
          ],
        ),
      ),
    ),
  );
}
