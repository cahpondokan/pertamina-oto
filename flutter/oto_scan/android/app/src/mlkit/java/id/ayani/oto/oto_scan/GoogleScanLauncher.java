package id.ayani.oto.oto_scan;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import com.google.android.gms.common.api.ApiException;
import com.google.mlkit.vision.documentscanner.GmsDocumentScannerOptions;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanning;
import com.google.mlkit.vision.documentscanner.GmsDocumentScanningResult;
import io.flutter.plugin.common.MethodChannel;
import java.util.ArrayList;
import java.util.List;

/** The Google-owned camera provides automatic/manual capture and FULL cleanup. */
final class GoogleScanLauncher {
  static final int REQUEST = 73;
  private final MainActivity activity;
  private MethodChannel.Result pending;
  private int limit;
  private boolean closed;

  GoogleScanLauncher(MainActivity activity) { this.activity = activity; }

  void start(int pageLimit, MethodChannel.Result result) {
    if (closed || pending != null) {
      result.error("scan_busy", "Pemindai masih terbuka. Selesaikan atau batalkan scan dahulu.", null);
      return;
    }
    if (pageLimit < 1 || pageLimit > 20) {
      result.error("scan_limit", "Maksimal 20 halaman per dokumen.", null);
      return;
    }
    pending = result;
    limit = pageLimit;
    GmsDocumentScannerOptions options = new GmsDocumentScannerOptions.Builder()
        .setGalleryImportAllowed(true)
        .setPageLimit(pageLimit)
        .setResultFormats(GmsDocumentScannerOptions.RESULT_FORMAT_JPEG)
        .setScannerMode(GmsDocumentScannerOptions.SCANNER_MODE_FULL)
        .build();
    // SDK 16.0.0 has no public capture-mode setter. Auto/manual is controlled
    // by its camera UI; do not invent an application switch that cannot work.
    GmsDocumentScanning.getClient(options).getStartScanIntent(activity)
        .addOnSuccessListener(sender -> {
          if (closed || pending != result || activity.isFinishing()) return;
          try { activity.startIntentSenderForResult(sender, REQUEST, null, 0, 0, 0); }
          catch (Exception e) { fail("Pemindai belum dapat dibuka. Coba lagi."); }
        })
        .addOnFailureListener(e -> {
          if (closed || pending != result) return;
          String message = "Pemindai Google belum siap. Aktifkan internet untuk unduhan awal, perbarui Google Play services, lalu coba lagi.";
          if (e instanceof ApiException && ((ApiException)e).getStatusCode() == 17)
            message = "Pemindai Google tidak tersedia pada perangkat ini. Periksa Google Play services dan dukungan perangkat.";
          fail(message);
        });
  }

  boolean onActivityResult(int request, int code, Intent data) {
    if (request != REQUEST) return false;
    MethodChannel.Result result = pending;
    pending = null;
    if (result == null || closed) return true;
    if (code == Activity.RESULT_CANCELED) { result.success(null); return true; }
    if (code != Activity.RESULT_OK || data == null) {
      result.error("scan_result", "Hasil scan belum tersedia. Halaman sebelumnya tetap tersimpan.", null);
      return true;
    }
    try {
      GmsDocumentScanningResult scanned = GmsDocumentScanningResult.fromActivityResultIntent(data);
      if (scanned == null || scanned.getPages() == null || scanned.getPages().isEmpty()
          || scanned.getPages().size() > limit) throw new IllegalArgumentException();
      List<Uri> uris = new ArrayList<>();
      for (GmsDocumentScanningResult.Page page : scanned.getPages()) uris.add(page.getImageUri());
      activity.async(result, () -> new GooglePageImport(activity, activity.store).importPages(uris, limit));
    } catch (Exception e) {
      result.error("scan_result", "Hasil scan tidak valid. Silakan ambil ulang.", null);
    }
    return true;
  }

  private void fail(String message) {
    MethodChannel.Result result = pending;
    pending = null;
    if (result != null) result.error("google_scanner", message, null);
  }
  void close() { closed = true; fail("Sesi pemindai ditutup. Silakan buka kembali."); }
}
