import 'dart:io';
import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'native_scan.dart';

/// Keep only paths/futures here; Flutter's image cache bounds decoded images.
class GalleryThumbnails {
  GalleryThumbnails(this.native);
  final NativeScan native;
  final _pending = <String, Future<String>>{};
  final _paths = <String, String>{};
  String? peek(String id) => _paths[id];

  Future<String> load(String id, {bool refresh = false}) {
    if (refresh) _pending.remove(id);
    return _pending.putIfAbsent(id, () async {
      final path = await native.call<String>('thumbnail', {
        'id': id,
        'refresh': refresh,
      });
      if (path == null || path.isEmpty) throw const FormatException();
      _paths[id] = path;
      return path;
    });
  }
}

List<Map> searchDocuments(List<Map> documents, String query) {
  final words = query.trim().toLowerCase().split(RegExp(r'\s+'));
  return documents.where((document) {
    final name = (document['name'] as String).toLowerCase();
    return words.every(name.contains);
  }).toList();
}

String _size(num bytes) =>
    bytes < 1024 * 1024
        ? '${(bytes / 1024).ceil()} KB'
        : '${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB';

String _date(Object? milliseconds) {
  if (milliseconds is! num || milliseconds <= 0) return '';
  final date = DateTime.fromMillisecondsSinceEpoch(milliseconds.toInt());
  const months = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'Mei',
    'Jun',
    'Jul',
    'Agu',
    'Sep',
    'Okt',
    'Nov',
    'Des',
  ];
  return '${date.day} ${months[date.month - 1]} ${date.year}';
}

class DocumentGallery extends StatelessWidget {
  const DocumentGallery({
    super.key,
    required this.documents,
    required this.search,
    required this.thumbnails,
    required this.onOpen,
    this.selectedIds = const {},
    this.selectionMode = false,
    this.onToggle,
  });
  final List<Map> documents;
  final TextEditingController search;
  final GalleryThumbnails thumbnails;
  final ValueChanged<Map> onOpen;
  final ValueChanged<Map>? onToggle;
  final Set<String> selectedIds;
  final bool selectionMode;

  @override
  Widget build(
    BuildContext context,
  ) => ValueListenableBuilder<TextEditingValue>(
    valueListenable: search,
    builder: (context, value, _) {
      final filtered = searchDocuments(documents, value.text);
      final searching = value.text.trim().isNotEmpty;
      return Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 16, 16, 0),
            child: TextField(
              key: const ValueKey('gallery-search'),
              controller: search,
              textInputAction: TextInputAction.search,
              onSubmitted: (_) => FocusScope.of(context).unfocus(),
              decoration: InputDecoration(
                hintText: 'Cari nama dokumen…',
                prefixIcon: const Icon(Icons.search),
                suffixIcon:
                    value.text.isEmpty
                        ? null
                        : IconButton(
                          tooltip: 'Hapus pencarian',
                          onPressed: search.clear,
                          icon: const Icon(Icons.close),
                        ),
                filled: true,
                fillColor: Colors.white,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide: BorderSide.none,
                ),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.fromLTRB(18, 14, 18, 12),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    searching
                        ? '${filtered.length} dokumen ditemukan'
                        : '${documents.length} dokumen',
                    style: const TextStyle(fontWeight: FontWeight.w600),
                  ),
                ),
                if (!searching)
                  const Text('Terbaru', style: TextStyle(fontSize: 12)),
              ],
            ),
          ),
          Expanded(
            child:
                documents.isEmpty
                    ? const _GalleryEmpty(
                      icon: Icons.photo_library_outlined,
                      title: 'Belum ada dokumen',
                      message: 'Hasil scan Anda akan muncul di sini.',
                    )
                    : filtered.isEmpty
                    ? _GalleryEmpty(
                      icon: Icons.search_off,
                      title: 'Dokumen tidak ditemukan',
                      message: 'Coba nama atau kata kunci lain.',
                      action: TextButton(
                        onPressed: search.clear,
                        child: const Text('Tampilkan semua'),
                      ),
                    )
                    : LayoutBuilder(
                      builder: (context, constraints) {
                        final scale =
                            MediaQuery.textScalerOf(context).scale(14) / 14;
                        final columns = math.max(
                          1,
                          ((constraints.maxWidth - 20) / 160).floor(),
                        );
                        final width =
                            (constraints.maxWidth - 32 - (columns - 1) * 12) /
                            columns;
                        return GridView.builder(
                          key: PageStorageKey(
                            'gallery-${value.text.trim().toLowerCase()}',
                          ),
                          padding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
                          keyboardDismissBehavior:
                              ScrollViewKeyboardDismissBehavior.onDrag,
                          cacheExtent: 160,
                          gridDelegate:
                              SliverGridDelegateWithFixedCrossAxisCount(
                                crossAxisCount: columns,
                                crossAxisSpacing: 12,
                                mainAxisSpacing: 12,
                                mainAxisExtent: width * 1.18 + 24 + 76 * scale,
                              ),
                          itemCount: filtered.length,
                          itemBuilder: (context, index) {
                            final document = filtered[index];
                            return _DocumentCard(
                              key: ValueKey(document['id']),
                              document: document,
                              thumbnails: thumbnails,
                              selected: selectedIds.contains(document['id']),
                              selectionMode: selectionMode,
                              onLongPress:
                                  onToggle == null
                                      ? null
                                      : () => onToggle!(document),
                              onTap: () {
                                FocusScope.of(context).unfocus();
                                if (selectionMode) {
                                  onToggle?.call(document);
                                } else {
                                  onOpen(document);
                                }
                              },
                            );
                          },
                        );
                      },
                    ),
          ),
        ],
      );
    },
  );
}

class _GalleryEmpty extends StatelessWidget {
  const _GalleryEmpty({
    required this.icon,
    required this.title,
    required this.message,
    this.action,
  });
  final IconData icon;
  final String title, message;
  final Widget? action;

  @override
  Widget build(BuildContext context) => Center(
    child: SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 44, color: const Color(0xFF7D8A98)),
          const SizedBox(height: 14),
          Text(
            title,
            textAlign: TextAlign.center,
            style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600),
          ),
          const SizedBox(height: 6),
          Text(message, textAlign: TextAlign.center),
          if (action != null) action!,
        ],
      ),
    ),
  );
}

class _DocumentCard extends StatelessWidget {
  const _DocumentCard({
    super.key,
    required this.document,
    required this.thumbnails,
    required this.onTap,
    required this.selected,
    required this.selectionMode,
    this.onLongPress,
  });
  final Map document;
  final GalleryThumbnails thumbnails;
  final VoidCallback onTap;
  final VoidCallback? onLongPress;
  final bool selected, selectionMode;

  @override
  Widget build(BuildContext context) => Semantics(
    selected: selectionMode ? selected : null,
    label: selectionMode ? (selected ? 'Dipilih' : 'Belum dipilih') : null,
    child: Material(
      color: selected ? const Color(0xFFFFEBEE) : Colors.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
        side: BorderSide(
          color: selected ? const Color(0xFFF5102E) : Colors.transparent,
          width: 2,
        ),
      ),
      clipBehavior: Clip.antiAlias,
      child: InkWell(
        onTap: onTap,
        onLongPress: onLongPress,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Expanded(
              child: Stack(
                fit: StackFit.expand,
                children: [
                  ColoredBox(
                    color: const Color(0xFFE5E9EE),
                    child: Padding(
                      padding: const EdgeInsets.all(10),
                      child: _Thumbnail(
                        id: document['id'],
                        thumbnails: thumbnails,
                      ),
                    ),
                  ),
                  if (selectionMode)
                    Positioned(
                      top: 6,
                      right: 6,
                      child: IgnorePointer(
                        child: Container(
                          decoration: const BoxDecoration(
                            color: Colors.white,
                            shape: BoxShape.circle,
                          ),
                          padding: const EdgeInsets.all(3),
                          child: Icon(
                            selected
                                ? Icons.check_circle
                                : Icons.radio_button_unchecked,
                            color:
                                selected
                                    ? const Color(0xFFF5102E)
                                    : const Color(0xFF667080),
                          ),
                        ),
                      ),
                    ),
                  Positioned(
                    right: 8,
                    bottom: 8,
                    child: DecoratedBox(
                      decoration: BoxDecoration(
                        color: const Color(0xE609243C),
                        borderRadius: BorderRadius.circular(6),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 7,
                          vertical: 4,
                        ),
                        child: Text(
                          '${document['pages']} hal · PDF',
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 10,
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(12, 10, 12, 12),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    document['name'],
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 5),
                  Text(
                    [
                      if (_date(document['created']).isNotEmpty)
                        _date(document['created']),
                      _size(document['bytes'] as num),
                    ].join(' · '),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontSize: 11,
                      color: Color(0xFF667080),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    ),
  );
}

class _Thumbnail extends StatefulWidget {
  const _Thumbnail({required this.id, required this.thumbnails});
  final String id;
  final GalleryThumbnails thumbnails;
  @override
  State<_Thumbnail> createState() => _ThumbnailState();
}

class _ThumbnailState extends State<_Thumbnail> {
  late Future<String> image;
  @override
  void initState() {
    super.initState();
    image = widget.thumbnails.load(widget.id);
  }

  void retry() => setState(() {
    image = widget.thumbnails.load(widget.id, refresh: true);
  });

  Widget unavailable() => Center(
    child: IconButton(
      tooltip: 'Muat ulang pratinjau',
      onPressed: retry,
      icon: const Icon(Icons.refresh, color: Color(0xFF667080)),
    ),
  );

  @override
  Widget build(BuildContext context) => FutureBuilder<String>(
    future: image,
    builder: (context, snapshot) {
      if (snapshot.connectionState != ConnectionState.done) {
        return const Center(
          child: SizedBox(
            width: 22,
            height: 22,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
        );
      }
      if (snapshot.hasError || !snapshot.hasData) return unavailable();
      return Image.file(
        File(snapshot.data!),
        fit: BoxFit.contain,
        cacheWidth: 480,
        filterQuality: FilterQuality.medium,
        errorBuilder: (_, __, ___) => unavailable(),
      );
    },
  );
}
