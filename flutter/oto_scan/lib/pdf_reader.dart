import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'native_scan.dart';
import 'document_actions.dart';
import 'pdf_reading.dart';
import 'pdf_canvas.dart';

class PdfReader extends StatefulWidget {
  const PdfReader({
    super.key,
    required this.document,
    required this.native,
    this.onClose,
  });
  final Map document;
  final NativeScan native;
  final VoidCallback? onClose;
  @override
  State<PdfReader> createState() => _PdfReaderState();
}

class _PdfReaderState extends State<PdfReader> {
  late Map document = Map.of(widget.document);
  late final reading = PdfReading(widget.native, widget.document);
  bool busy = false, changed = false, pendingBack = false;
  String? error;
  bool get external => document['external'] == true;

  @override
  void dispose() {
    reading.dispose();
    super.dispose();
  }

  void back() {
    if (busy) {
      setState(() => pendingBack = true);
      return;
    }
    if (widget.onClose != null) {
      widget.onClose!();
    } else {
      Navigator.pop(context, changed);
    }
  }

  Future<void> run(Future<void> Function() action) async {
    if (busy) return;
    setState(() {
      busy = true;
      error = null;
    });
    var succeeded = false;
    try {
      await action();
      succeeded = true;
    } on PlatformException catch (e) {
      if (mounted)
        setState(
          () => error = e.message ?? 'Proses belum berhasil. Coba lagi.',
        );
    } catch (_) {
      if (mounted) setState(() => error = 'Proses belum berhasil. Coba lagi.');
    }
    if (!mounted) return;
    setState(() => busy = false);
    if (pendingBack && succeeded) back();
    pendingBack = false;
  }

  Future<void> menu(String action) async {
    if (action == 'rename') {
      final name = await askDocumentName(context, document['name']);
      if (name == null || !mounted) return;
      await run(() async {
        final renamed = await widget.native.call<Map>('rename', {
          'id': document['id'],
          'name': name,
        });
        if (mounted && renamed != null)
          setState(() {
            document = renamed;
            changed = true;
          });
      });
    } else if (action == 'delete') {
      if (!await confirmDeleteDocuments(context, 1) || !mounted) return;
      await run(() async {
        await widget.native.call('deleteDocuments', {
          'ids': [document['id']],
        });
        changed = true;
        pendingBack = true;
      });
    } else if (action == 'export') {
      await run(() async {
        final saved = await widget.native.call<bool>('export', {
          'id': document['id'],
          'name': document['name'],
        });
        if (mounted && saved == true)
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(const SnackBar(content: Text('Salinan disimpan')));
      });
    }
  }

  @override
  Widget build(BuildContext context) => PopScope(
    canPop: false,
    onPopInvokedWithResult: (didPop, _) {
      if (!didPop) back();
    },
    child: Scaffold(
      appBar: AppBar(
        leading: IconButton(
          tooltip: external ? 'Tutup PDF' : 'Kembali',
          onPressed: back,
          icon: const Icon(Icons.arrow_back),
        ),
        title: Text(
          document['name'],
          maxLines: 1,
          overflow: TextOverflow.ellipsis,
        ),
        actions: [
          if (!external)
            PopupMenuButton<String>(
              enabled: !busy,
              tooltip: 'Tindakan dokumen',
              onSelected: menu,
              itemBuilder:
                  (_) => const [
                    PopupMenuItem(
                      value: 'export',
                      child: Text('Simpan salinan'),
                    ),
                    PopupMenuItem(value: 'rename', child: Text('Ganti nama')),
                    PopupMenuItem(value: 'delete', child: Text('Hapus')),
                  ],
            ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            if (busy) const LinearProgressIndicator(),
            if (pendingBack)
              const Padding(
                padding: EdgeInsets.all(8),
                child: Text('Menyelesaikan proses sebelum kembali…'),
              ),
            if (error != null)
              Padding(
                padding: const EdgeInsets.all(12),
                child: Text(error!, style: const TextStyle(color: Colors.red)),
              ),
            Expanded(
              child: PdfCanvas(
                reading: reading,
                preview: widget.document['preview'] as String?,
              ),
            ),
            if (!external)
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 0, 16, 12),
                child: SizedBox(
                  width: double.infinity,
                  child: FilledButton.icon(
                    onPressed:
                        busy
                            ? null
                            : () => run(
                              () => shareDocuments(widget.native, [
                                document['id'] as String,
                              ]),
                            ),
                    icon: const Icon(Icons.share_outlined),
                    label: const Text('Kirim'),
                  ),
                ),
              ),
          ],
        ),
      ),
    ),
  );
}
