import 'dart:async';
import 'dart:io';
import 'dart:math' as math;
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'pdf_reading.dart';

class _PageAnchor {
  const _PageAnchor(this.page, this.fraction);
  final int page;
  final Offset fraction;
}

/// A single gesture surface controls a lazy vertical list. Zoom changes page
/// layout, so one-finger vertical movement remains continuous across boundaries.
class PdfCanvas extends StatefulWidget {
  const PdfCanvas({super.key, required this.reading, this.preview});
  final PdfReading reading;
  final String? preview;
  @override
  State<PdfCanvas> createState() => _PdfCanvasState();
}

class _PdfCanvasState extends State<PdfCanvas>
    with SingleTickerProviderStateMixin {
  final scroll = ScrollController();
  late final AnimationController fling;
  double zoom = 1, panX = 0, width = 1, height = 1, density = 1;
  double startZoom = 1;
  _PageAnchor? pinchAnchor;
  Offset pinchFocal = Offset.zero;
  bool pinching = false, positionScheduled = false;
  int gesturePointers = 0;
  double? pendingOffset;
  int page = 0;
  Timer? detailTimer;
  double detailScale = 1;
  bool scheduled = false;
  int anchorPage = 0;
  double anchorFraction = 0, recordedTop = 0, recordedHeight = 0;
  PdfReading get reading => widget.reading;
  double get baseWidth => math.max(1, width - 24);
  double get offset => pendingOffset ?? (scroll.hasClients ? scroll.offset : 0);

  _PageAnchor anchorAt(Offset focal) {
    final p = reading.geometry.pageAt(offset + focal.dy, baseWidth, zoom);
    final pageWidth = baseWidth * zoom;
    return _PageAnchor(
      p,
      Offset(
        (focal.dx - 12 - panX) / pageWidth,
        (offset + focal.dy - reading.geometry.top(p, baseWidth, zoom)) /
            (pageWidth * reading.geometry.ratio(p)),
      ),
    );
  }

  double anchoredOffset(_PageAnchor anchor, Offset focal) =>
      reading.geometry.top(anchor.page, baseWidth, zoom) +
      baseWidth *
          zoom *
          reading.geometry.ratio(anchor.page) *
          anchor.fraction.dy -
      focal.dy;

  // Coalesce gesture/render updates so an older post-frame callback cannot
  // restore an obsolete focal position after the newest layout is ready.
  void positionAfterLayout(double target) {
    pendingOffset = target;
    if (positionScheduled) return;
    positionScheduled = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      positionScheduled = false;
      if (!mounted) return;
      final to = pendingOffset;
      pendingOffset = null;
      if (to != null) move(to);
      viewport();
    });
  }

  @override
  void initState() {
    super.initState();
    fling = AnimationController.unbounded(vsync: this)
      ..addListener(() => move(fling.value));
    scroll.addListener(viewport);
    reading.addListener(updated);
  }

  void updated() {
    if (mounted) {
      final nextTop = reading.geometry.top(anchorPage, baseWidth, 1);
      final nextHeight = baseWidth * reading.geometry.ratio(anchorPage);
      final reanchor =
          recordedHeight > 0 &&
          ((nextTop - recordedTop).abs() > .1 ||
              (nextHeight - recordedHeight).abs() > .1);
      final activePinch = pinching && pinchAnchor != null;
      final target =
          activePinch
              ? anchoredOffset(pinchAnchor!, pinchFocal)
              : (nextTop + nextHeight * anchorFraction) * zoom;
      if (reanchor || activePinch) fling.stop();
      setState(() {});
      if (reanchor || activePinch) positionAfterLayout(target);
      scheduleViewport();
    }
  }

  void scheduleViewport() {
    if (scheduled) return;
    scheduled = true;
    WidgetsBinding.instance.addPostFrameCallback((_) {
      scheduled = false;
      if (mounted) viewport();
    });
  }

  void viewport() {
    if (!mounted || width <= 1 || pendingOffset != null) return;
    final first = reading.geometry.pageAt(offset, baseWidth, zoom);
    anchorPage = first;
    recordedTop = reading.geometry.top(first, baseWidth, 1);
    recordedHeight = baseWidth * reading.geometry.ratio(first);
    anchorFraction = (offset / zoom - recordedTop) / recordedHeight;
    final last = reading.geometry.pageAt(offset + height, baseWidth, zoom);
    final current = reading.geometry.pageAt(
      offset + height / 2,
      baseWidth,
      zoom,
    );
    if (current != page) setState(() => page = current);
    final pageWidth = baseWidth * zoom;
    final regions = <int, Rect>{};
    for (var p = first; p <= last; p++) {
      final top = reading.geometry.top(p, baseWidth, zoom);
      final pageHeight = pageWidth * reading.geometry.ratio(p);
      regions[p] = Rect.fromLTRB(
        (-(12 + panX) / pageWidth).clamp(0, 1),
        ((offset - top) / pageHeight).clamp(0, 1),
        ((width - 12 - panX) / pageWidth).clamp(0, 1),
        ((offset + height - top) / pageHeight).clamp(0, 1),
      );
    }
    reading.viewport(
      first,
      last,
      (baseWidth * density).ceil(),
      detailScale,
      focus: current,
      regions: regions,
    );
  }

  void move(double to) {
    if (!scroll.hasClients) return;
    scroll.jumpTo(to.clamp(0, scroll.position.maxScrollExtent));
  }

  void settleDetail() {
    detailTimer?.cancel();
    detailTimer = Timer(const Duration(milliseconds: 180), () {
      if (!mounted) return;
      if (fling.isAnimating) {
        settleDetail();
        return;
      }
      detailScale = zoom;
      viewport();
    });
  }

  void resetZoom() {
    fling.stop();
    detailTimer?.cancel();
    final anchor = anchorAt(Offset.zero);
    setState(() {
      zoom = 1;
      panX = 0;
      detailScale = 1;
    });
    positionAfterLayout(anchoredOffset(anchor, Offset.zero));
  }

  @override
  void dispose() {
    detailTimer?.cancel();
    reading.removeListener(updated);
    scroll.dispose();
    fling.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) => LayoutBuilder(
    builder: (context, bounds) {
      width = bounds.maxWidth;
      height = bounds.maxHeight;
      density = MediaQuery.devicePixelRatioOf(context);
      scheduleViewport();
      return Stack(
        children: [
          Positioned.fill(
            child: Semantics(
              label: 'Halaman PDF, gulir ke bawah',
              onScrollUp: () => move(offset - height * .8),
              onScrollDown: () => move(offset + height * .8),
              child: Listener(
                onPointerDown: (_) => fling.stop(),
                onPointerSignal: (event) {
                  if (event is PointerScrollEvent)
                    GestureBinding.instance.pointerSignalResolver.register(
                      event,
                      (_) => move(offset + event.scrollDelta.dy),
                    );
                },
                child: GestureDetector(
                  key: const ValueKey('pdf-pages'),
                  behavior: HitTestBehavior.opaque,
                  onScaleStart: (d) {
                    fling.stop();
                    detailTimer?.cancel();
                    detailScale = 1;
                    viewport();
                    startZoom = zoom;
                    gesturePointers = d.pointerCount;
                    pinching = d.pointerCount > 1;
                    pinchFocal = d.localFocalPoint;
                    pinchAnchor = anchorAt(pinchFocal);
                  },
                  onScaleUpdate: (d) {
                    gesturePointers = math.max(gesturePointers, d.pointerCount);
                    if (d.pointerCount > 1) {
                      pinching = true;
                      pinchFocal = d.localFocalPoint;
                      final next = (startZoom * d.scale).clamp(1.0, 6.0);
                      setState(() {
                        zoom = next;
                        panX = (pinchFocal.dx -
                                12 -
                                pinchAnchor!.fraction.dx * baseWidth * zoom)
                            .clamp(-baseWidth * (zoom - 1), 0.0);
                        detailScale = 1;
                      });
                      positionAfterLayout(
                        anchoredOffset(pinchAnchor!, pinchFocal),
                      );
                    } else {
                      if (pendingOffset != null) {
                        positionAfterLayout(offset - d.focalPointDelta.dy);
                      } else {
                        move(offset - d.focalPointDelta.dy);
                      }
                      if (zoom > 1)
                        setState(
                          () =>
                              panX = (panX + d.focalPointDelta.dx).clamp(
                                -baseWidth * (zoom - 1),
                                0.0,
                              ),
                        );
                    }
                  },
                  onScaleEnd: (d) {
                    pinching = false;
                    final velocity = -d.velocity.pixelsPerSecond.dy;
                    if (gesturePointers == 1 &&
                        d.pointerCount == 0 &&
                        velocity.abs() > 80 &&
                        scroll.hasClients)
                      fling.animateWith(
                        ClampingScrollSimulation(
                          position: offset,
                          velocity: velocity,
                        ),
                      );
                    settleDetail();
                  },
                  child: ClipRect(
                    child: ListView.builder(
                      key: const ValueKey('pdf-vertical-list'),
                      controller: scroll,
                      physics: const NeverScrollableScrollPhysics(),
                      cacheExtent: height * .5,
                      padding: EdgeInsets.only(
                        top: 12 * zoom,
                        // Retain the unused viewport space as a bottom margin
                        // so a short landscape page stays under the fingers.
                        bottom:
                            12 * zoom +
                            math.max(
                              0,
                              height - reading.geometry.extent(baseWidth, 1),
                            ),
                      ),
                      itemCount: reading.geometry.count,
                      itemBuilder: (_, index) {
                        final pageWidth = baseWidth * zoom;
                        final pageHeight =
                            pageWidth * reading.geometry.ratio(index);
                        return SizedBox(
                          height: pageHeight + 12 * zoom,
                          child: Stack(
                            clipBehavior: Clip.none,
                            children: [
                              Positioned(
                                left: 12 + panX,
                                top: 0,
                                width: pageWidth,
                                height: pageHeight,
                                child: ColoredBox(
                                  color: Colors.white,
                                  child: _PageImage(
                                    key: ValueKey('pdf-page-$index'),
                                    reading: reading,
                                    index: index,
                                    preview: index == 0 ? widget.preview : null,
                                    decodeWidth: (baseWidth * density).ceil(),
                                  ),
                                ),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
                  ),
                ),
              ),
            ),
          ),
          Positioned(
            right: 12,
            top: 10,
            child: IgnorePointer(
              child: DecoratedBox(
                decoration: BoxDecoration(
                  color: const Color(0xDB09243C),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Padding(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 10,
                    vertical: 5,
                  ),
                  child: Text(
                    '${page + 1} / ${reading.geometry.count}',
                    style: const TextStyle(color: Colors.white, fontSize: 12),
                  ),
                ),
              ),
            ),
          ),
          if (zoom > 1.01)
            Positioned(
              left: 12,
              top: 4,
              child: IconButton.filledTonal(
                tooltip: 'Pas lebar layar',
                onPressed: resetZoom,
                icon: const Icon(Icons.fit_screen),
              ),
            ),
        ],
      );
    },
  );
}

class _PageImage extends StatefulWidget {
  const _PageImage({
    super.key,
    required this.reading,
    required this.index,
    required this.preview,
    required this.decodeWidth,
  });
  final PdfReading reading;
  final int index, decodeWidth;
  final String? preview;
  @override
  State<_PageImage> createState() => _PageImageState();
}

class _PageImageState extends State<_PageImage> {
  bool hasFrame = false;
  @override
  Widget build(BuildContext context) {
    final reading = widget.reading, index = widget.index;
    final preview = widget.preview, decodeWidth = widget.decodeWidth;
    final image = reading.rasters[index];
    final error = reading.openError ?? reading.errors[index];
    Widget waiting() => Center(
      child:
          error != null
              ? TextButton(
                onPressed:
                    reading.openError != null
                        ? null
                        : () => reading.retry(index),
                child: Text(error),
              )
              : Text(
                'Halaman ${index + 1}',
                style: const TextStyle(color: Colors.grey),
              ),
    );
    Widget seed() =>
        preview == null
            ? waiting()
            : Image.file(
              File(preview),
              fit: BoxFit.contain,
              cacheWidth: decodeWidth.clamp(384, 1200),
              errorBuilder: (_, __, ___) => waiting(),
            );
    if (image == null) {
      hasFrame = false;
      if (error != null && preview != null) {
        return Stack(
          fit: StackFit.expand,
          children: [
            seed(),
            Align(
              alignment: Alignment.bottomCenter,
              child: ColoredBox(
                color: const Color(0xEFFFFFFF),
                child: waiting(),
              ),
            ),
          ],
        );
      }
      return seed();
    }
    final base = Image.memory(
      image.bytes,
      fit: BoxFit.fill,
      gaplessPlayback: true,
      filterQuality: FilterQuality.medium,
      frameBuilder: (_, child, frame, synchronous) {
        if (frame != null || synchronous) hasFrame = true;
        return hasFrame ? child : seed();
      },
      errorBuilder:
          (_, __, ___) => TextButton(
            onPressed: () => reading.retry(index),
            child: const Text('Muat ulang halaman'),
          ),
    );
    return LayoutBuilder(
      builder:
          (_, bounds) => Stack(
            fit: StackFit.expand,
            children: [
              base,
              for (final tile in reading.tiles.values.where(
                (tile) => tile.page == index,
              ))
                Positioned(
                  left: tile.x / tile.fullWidth * bounds.maxWidth,
                  top: tile.y / tile.fullWidth * bounds.maxWidth,
                  width: tile.width / tile.fullWidth * bounds.maxWidth,
                  height: tile.height / tile.fullWidth * bounds.maxWidth,
                  child: Image.memory(
                    tile.bytes!,
                    fit: BoxFit.fill,
                    filterQuality: FilterQuality.low,
                    gaplessPlayback: true,
                    errorBuilder:
                        (_, __, ___) => TextButton(
                          onPressed: () => reading.retry(index),
                          child: const Text('Muat ulang detail'),
                        ),
                  ),
                ),
              if (reading.tileErrors.isNotEmpty && index == reading.focusPage)
                Align(
                  alignment: Alignment.bottomCenter,
                  child: TextButton(
                    onPressed: () => reading.retry(index),
                    child: const Text('Muat ulang detail'),
                  ),
                ),
            ],
          ),
    );
  }
}
