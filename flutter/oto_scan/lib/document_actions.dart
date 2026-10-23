import 'package:flutter/material.dart';
import 'native_scan.dart';

Future<bool> confirmDeleteDocuments(BuildContext context, int count) async =>
    await showDialog<bool>(
      context: context,
      builder:
          (c) => AlertDialog(
            title: Text('Hapus $count dokumen?'),
            content: const Text(
              'PDF yang dipilih akan dihapus dari Dokumen Saya. Tindakan ini tidak dapat dibatalkan.',
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(c, false),
                child: const Text('Batal'),
              ),
              FilledButton(
                onPressed: () => Navigator.pop(c, true),
                child: const Text('Hapus'),
              ),
            ],
          ),
    ) ==
    true;

Future<String?> askDocumentName(
  BuildContext context,
  String current, {
  String title = 'Ganti nama',
}) async {
  final controller = TextEditingController(
    text: current.replaceFirst(RegExp(r'\.pdf$', caseSensitive: false), ''),
  );
  final result = await showDialog<String>(
    context: context,
    builder:
        (c) => AlertDialog(
          title: Text(title),
          content: TextField(
            controller: controller,
            autofocus: true,
            maxLength: 80,
            decoration: const InputDecoration(
              labelText: 'Nama dokumen',
              suffixText: '.pdf',
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(c),
              child: const Text('Batal'),
            ),
            FilledButton(
              onPressed: () {
                if (controller.text.trim().isNotEmpty)
                  Navigator.pop(c, controller.text.trim());
              },
              child: const Text('Simpan'),
            ),
          ],
        ),
  );
  // Dialog exit animation still uses the controller for this frame.
  Future<void>.delayed(const Duration(seconds: 1), controller.dispose);
  return result;
}

Future<void> shareDocuments(NativeScan native, Iterable<String> ids) =>
    native.call('shareMany', {'ids': ids.toList()});
