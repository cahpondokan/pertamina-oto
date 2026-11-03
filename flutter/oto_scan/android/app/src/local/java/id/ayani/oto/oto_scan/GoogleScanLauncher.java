package id.ayani.oto.oto_scan;

import android.content.Intent;
import io.flutter.plugin.common.MethodChannel;

/** Production local-scanner build does not link or acquire the Google SDK. */
final class GoogleScanLauncher {
  GoogleScanLauncher(MainActivity activity) {}
  void start(int limit, MethodChannel.Result result) {
    result.error("unavailable", "Pemindai Google tersedia di OTO Scan Uji.", null);
  }
  boolean onActivityResult(int request, int code, Intent data) { return false; }
  void close() {}
}
