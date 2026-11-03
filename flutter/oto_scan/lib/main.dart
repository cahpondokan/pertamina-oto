import 'dart:async';
import 'dart:io';
import 'dart:math' as math;
import 'package:flutter/material.dart';
import 'package:flutter/foundation.dart'
    show listEquals, LicenseRegistry, LicenseEntryWithLineBreaks;
import 'package:flutter/services.dart';
import 'package:flutter/rendering.dart';
import 'dart:ui' as ui;
import 'native_scan.dart';
import 'scan_model.dart';
import 'crop_screen.dart';
import 'live_detection.dart';
import 'document_gallery.dart';
import 'document_actions.dart';
import 'pdf_reader.dart';
import 'privacy_screen.dart';
import 'app_config.dart';
export 'pdf_reader.dart' show PdfReader;

const red = Color(0xFFF5102E),
    navy = Color(0xFF09243C),
    canvas = Color(0xFFF0F1F3);
final proofKey = GlobalKey();
bool syntheticActive = false;
Future<void> proof(String state, {String? id}) async {
  if (!const bool.fromEnvironment('OTO_TEST_MODE') || !syntheticActive) return;
  await Future<void>.delayed(const Duration(milliseconds: 450));
  try {
    final boundary =
        proofKey.currentContext?.findRenderObject() as RenderRepaintBoundary?;
    if (boundary == null || boundary.debugNeedsPaint) return;
    final image = await boundary.toImage(pixelRatio: 1.5);
    final data = await image.toByteData(format: ui.ImageByteFormat.png);
    image.dispose();
    if (data != null)
      await NativeScan().call('proof', {
        'state': state,
        'id': id,
        'png': data.buffer.asUint8List(),
      });
  } catch (_) {}
}

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  LicenseRegistry.addLicense(() async* {
    yield LicenseEntryWithLineBreaks([
      'AndroidX',
      'Kotlin',
      'kotlinx.coroutines',
    ], await rootBundle.loadString('assets/THIRD_PARTY_NOTICES.txt'));
  });
  if (googleScannerEnabled) {
    LicenseRegistry.addLicense(() async* {
      yield LicenseEntryWithLineBreaks([
        'Google ML Kit',
      ], await rootBundle.loadString('assets/MLKIT_NOTICE.txt'));
    });
  }
  SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  SystemChrome.setSystemUIOverlayStyle(
    const SystemUiOverlayStyle(
      statusBarColor: Colors.transparent,
      systemNavigationBarColor: Colors.white,
      systemNavigationBarIconBrightness: Brightness.dark,
    ),
  );
  runApp(const OtoScanApp());
}

class OtoScanApp extends StatelessWidget {
  final bool useGoogleScanner;
  const OtoScanApp({super.key, this.useGoogleScanner = googleScannerEnabled});
  @override
  Widget build(BuildContext context) => MaterialApp(
    debugShowCheckedModeBanner: false,
    title: applicationName,
    builder: (_, child) => RepaintBoundary(key: proofKey, child: child!),
    theme: ThemeData(
      useMaterial3: true,
      scaffoldBackgroundColor: canvas,
      colorScheme: ColorScheme.fromSeed(
        seedColor: red,
        primary: red,
        onSurface: navy,
      ),
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.white,
        foregroundColor: navy,
        titleTextStyle: TextStyle(
          fontFamily: 'Roboto',
          fontSize: 19,
          fontWeight: FontWeight.w700,
          color: navy,
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size(48, 50),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(9)),
        ),
      ),
      iconButtonTheme: IconButtonThemeData(
        style: IconButton.styleFrom(minimumSize: const Size(48, 48)),
      ),
    ),
    home: ScannerHome(useGoogleScanner: useGoogleScanner),
  );
}

enum Stage { scan, pages, draft, library }

class ScannerHome extends StatefulWidget {
  final bool useGoogleScanner;
  const ScannerHome({super.key, this.useGoogleScanner = googleScannerEnabled});
  @override
  State<ScannerHome> createState() => _ScannerHomeState();
}

class _ScannerHomeState extends State<ScannerHome> with WidgetsBindingObserver {
  final native = NativeScan();
  final gallerySearch = TextEditingController();
  PageStorageBucket galleryBucket = PageStorageBucket();
  Map? savedAwaitingFinish;
  List<String> savedImagePaths = [];
  bool get finishingSavedScan =>
      savedAwaitingFinish != null &&
      listEquals(savedImagePaths, [for (final page in pages) page.image]);
  late final galleryThumbnails = GalleryThumbnails(native);
  Stage libraryOrigin = Stage.scan;
  final selectedDocuments = <String>{};
  bool backQueued = false, selectionMode = false, exiting = false;
  bool checkingLaunch = true;
  bool externalMode = false, openingPdf = false, pdfQueued = false;
  int externalGeneration = 0;
  Map? externalDocument;
  String? externalError;
  List<ScanPage> pages = [];
  List<Map> documents = [];
  Stage stage = Stage.scan;
  int selected = 0, cameraEpoch = 0;
  int? retakeIndex;
  bool busy = false, camera = false, torch = false, detecting = false;
  bool recoveryFailed = false;
  String? error;
  String progress = '';
  List<Offset>? liveCorners;
  Offset? targetPoint, focusPoint;
  int selectionEpoch = 0;
  String focusHint = 'Ketuk kertas untuk memilih dan fokus';
  Timer? focusMarker;
  final liveDetection = LiveDetection();
  final frameClock = Stopwatch()..start();
  Timer? detector;
  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    NativeScan.channel.setMethodCallHandler((call) async {
      if (call.method == 'pdfAvailable') {
        pdfQueued = true;
        await _openIncomingPdf();
      }
    });
    _initialize();
  }

  @override
  void dispose() {
    NativeScan.channel.setMethodCallHandler(null);
    gallerySearch.dispose();
    detector?.cancel();
    focusMarker?.cancel();
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(AppLifecycleState state) {
    if (widget.useGoogleScanner) return;
    if (state == AppLifecycleState.resumed &&
        !checkingLaunch &&
        !externalMode &&
        stage == Stage.scan &&
        !camera) {
      _startCamera();
    } else if (state != AppLifecycleState.resumed) {
      _stopCamera();
    }
  }

  Future<void> _initialize() async {
    if (checkingLaunch) {
      final incoming = await native.call<bool>('hasPendingPdf') ?? false;
      if (!mounted) return;
      setState(() => checkingLaunch = false);
      if (incoming || pdfQueued) {
        pdfQueued = true;
        await _openIncomingPdf();
        return;
      }
    }
    recoveryFailed = true;
    await _guard('Memulihkan halaman…', () async {
      pages = await native.recover();
      if (pages.isNotEmpty) stage = Stage.draft;
      recoveryFailed = false;
    });
    if (!recoveryFailed && stage == Stage.scan && mounted) await _startCamera();
  }

  Future<void> _openIncomingPdf() async {
    if (!mounted || checkingLaunch || openingPdf || busy || !pdfQueued) return;
    openingPdf = true;
    final generation = ++externalGeneration;
    _stopCamera();
    Navigator.of(context).popUntil((route) => route.isFirst);
    setState(() {
      externalMode = true;
      externalDocument = null;
      externalError = null;
    });
    try {
      do {
        pdfQueued = false;
        final document = await native.call<Map>('openPendingPdf');
        if (!mounted || generation != externalGeneration) {
          if (document != null) {
            await native.call('closeExternal', {'id': document['id']});
          }
          return;
        }
        if (!pdfQueued)
          setState(() {
            externalDocument = document;
            externalError =
                document == null
                    ? 'PDF tidak tersedia. Buka kembali dari aplikasi asal.'
                    : null;
          });
      } while (pdfQueued);
    } on PlatformException catch (e) {
      if (mounted && generation == externalGeneration) {
        setState(() => externalError = e.message ?? 'PDF tidak dapat dibuka.');
      }
    } catch (_) {
      if (mounted && generation == externalGeneration) {
        setState(
          () =>
              externalError =
                  'PDF tidak dapat dibuka. Buka kembali dari aplikasi asal.',
        );
      }
    } finally {
      if (mounted && generation == externalGeneration) {
        setState(() => openingPdf = false);
        if (pdfQueued) unawaited(_openIncomingPdf());
      }
    }
  }

  void _closeIncomingPdf() {
    exiting = true;
    externalGeneration++;
    pdfQueued = false;
    openingPdf = false;
    unawaited(
      native
          .call('closeExternal', {'id': externalDocument?['id']})
          .catchError((_) => null),
    );
    unawaited(SystemNavigator.pop());
  }

  Widget _incomingBody() {
    final document = externalDocument;
    if (document != null && !openingPdf) {
      return PdfReader(
        key: ValueKey(document['id']),
        document: document,
        native: native,
        onClose: _closeIncomingPdf,
      );
    }
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (!didPop) _closeIncomingPdf();
      },
      child: Scaffold(
        appBar: AppBar(
          title: const Text('Buka PDF'),
          leading: IconButton(
            tooltip: 'Tutup PDF',
            onPressed: _closeIncomingPdf,
            icon: const Icon(Icons.arrow_back),
          ),
        ),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child:
                openingPdf
                    ? const Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        CircularProgressIndicator(),
                        SizedBox(height: 16),
                        Text('Membuka PDF…'),
                      ],
                    )
                    : Text(
                      externalError ?? 'PDF tidak dapat dibuka.',
                      textAlign: TextAlign.center,
                    ),
          ),
        ),
      ),
    );
  }

  Future<void> _guard(String label, Future<void> Function() action) async {
    if (busy || !mounted) return;
    setState(() {
      busy = true;
      progress = label;
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
      if (mounted)
        setState(
          () =>
              error =
                  'Proses belum berhasil. Halaman sebelumnya tetap tersimpan.',
        );
    } finally {
      if (mounted) {
        setState(() => busy = false);
        if (pdfQueued) {
          backQueued = false;
          unawaited(_openIncomingPdf());
        } else if (backQueued) {
          backQueued = false;
          if (succeeded) unawaited(_back());
        }
        if (stage != Stage.scan) proof(stage.name);
      }
    }
  }

  Future<void> _startCamera() async {
    if (checkingLaunch || externalMode || exiting) return;
    if (recoveryFailed) {
      await _initialize();
      return;
    }
    if (!mounted || busy || stage != Stage.scan) return;
    if (widget.useGoogleScanner) {
      await _googleCapture();
      return;
    }
    final epoch = ++cameraEpoch;
    try {
      final allowed = await native.call<bool>('permission') ?? false;
      if (!mounted || epoch != cameraEpoch || stage != Stage.scan) return;
      setState(() {
        camera = allowed;
        liveDetection.start(frameClock.elapsedMilliseconds);
        error = allowed ? null : 'Izinkan kamera untuk memindai dokumen.';
      });
      if (allowed) {
        detector?.cancel();
        detector = Timer.periodic(
          const Duration(milliseconds: 250),
          (_) => _detect(),
        );
      }
    } catch (_) {
      if (mounted)
        setState(() => error = 'Kamera belum tersedia. Ketuk Coba kamera.');
    }
  }

  void _stopCamera() {
    cameraEpoch++;
    selectionEpoch++;
    focusMarker?.cancel();
    targetPoint = focusPoint = null;
    focusHint = 'Ketuk kertas untuk memilih dan fokus';
    detector?.cancel();
    liveDetection.start(frameClock.elapsedMilliseconds);
    if (mounted)
      setState(() {
        camera = false;
        liveCorners = null;
        torch = false;
      });
  }

  Future<void> _detect() async {
    if (detecting || busy || !camera || stage != Stage.scan) return;
    detecting = true;
    final epoch = cameraEpoch;
    final selection = selectionEpoch;
    try {
      final response = await native
          .call<Map>('detect', _selection())
          .timeout(const Duration(seconds: 2));
      if (mounted &&
          camera &&
          stage == Stage.scan &&
          epoch == cameraEpoch &&
          selection == selectionEpoch)
        setState(() {
          liveDetection.receive(response, frameClock.elapsedMilliseconds);
          liveCorners = liveDetection.corners;
          if (targetPoint != null && liveCorners != null) {
            targetPoint = liveCorners!.reduce((a, b) => a + b) / 4;
          }
        });
    } catch (_) {
      if (mounted &&
          camera &&
          stage == Stage.scan &&
          epoch == cameraEpoch &&
          selection == selectionEpoch)
        setState(() {
          liveDetection.fail();
          liveCorners = null;
        });
    } finally {
      detecting = false;
    }
  }

  Future<void> _restartCamera() async {
    _stopCamera();
    await WidgetsBinding.instance.endOfFrame;
    if (mounted && stage == Stage.scan) await _startCamera();
  }

  Map<String, dynamic> _selection() => {
    if (targetPoint != null) 'target': [targetPoint!.dx, targetPoint!.dy],
    if (liveCorners != null && liveDetection.cornersFrame != null)
      'previousFrame': liveDetection.cornersFrame,
    if (liveCorners != null)
      'previous': [
        for (final p in liveCorners!) [p.dx, p.dy],
      ],
  };

  Future<void> _selectObject(Offset point) async {
    if (!camera || busy) return;
    final epoch = ++selectionEpoch;
    focusMarker?.cancel();
    setState(() {
      targetPoint = focusPoint = point;
      liveDetection.start(frameClock.elapsedMilliseconds);
      liveCorners = null;
      focusHint = 'Objek dipilih · mengarahkan fokus…';
    });
    focusMarker = Timer(const Duration(seconds: 2), () {
      if (mounted && epoch == selectionEpoch) setState(() => focusPoint = null);
    });
    try {
      final directed = await native
          .call<bool>('focus', {'x': point.dx, 'y': point.dy})
          .timeout(const Duration(seconds: 2));
      if (mounted && epoch == selectionEpoch)
        setState(
          () =>
              focusHint =
                  directed == true
                      ? 'Fokus diarahkan · ketuk objek lain untuk ganti'
                      : 'Objek dipilih · fokus mengikuti kemampuan kamera',
        );
    } catch (_) {
      if (mounted && epoch == selectionEpoch)
        setState(() => focusHint = 'Objek dipilih · ketuk lagi untuk fokus');
    }
  }

  Future<void> _capture({bool synthetic = false}) async {
    if (recoveryFailed) return;
    syntheticActive = synthetic;
    if (pages.length >= 20 && retakeIndex == null) {
      setState(() => error = 'Maksimal 20 halaman per dokumen.');
      return;
    }
    var needsCrop = false;
    await _guard('Mendeteksi sisi dan memotong dokumen…', () async {
      var page =
          synthetic
              ? await native.synthetic()
              : await native.capture(_selection());
      _stopCamera();
      needsCrop = !page.detected;
      final next = [...pages];
      if (retakeIndex != null) {
        next[retakeIndex!] = page;
        selected = retakeIndex!;
      } else {
        next.add(page);
        selected = next.length - 1;
      }
      await native.checkpoint(next);
      pages = next;
      retakeIndex = null;
      stage = Stage.pages;
    });
    if (stage == Stage.scan) await _startCamera();
    if (needsCrop &&
        error == null &&
        mounted &&
        stage == Stage.pages &&
        !externalMode)
      await _edit();
    if (mounted && error == null && stage == Stage.pages && pages.isNotEmpty) {
      final warning = pages[selected].qualityWarning;
      if (warning != null)
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(warning),
            duration: const Duration(seconds: 6),
          ),
        );
    }
  }

  Future<void> _googleCapture() async {
    if (!mounted || busy || recoveryFailed || externalMode) return;
    final replace = retakeIndex;
    final limit = replace == null ? 20 - pages.length : 1;
    if (limit < 1) {
      setState(() => error = 'Maksimal 20 halaman per dokumen.');
      return;
    }
    await _guard('Menyiapkan pemindai dan hasil scan…', () async {
      final scanned = await native.googleScan(limit);
      if (!mounted) return;
      if (scanned == null) {
        retakeIndex = null;
        if (pages.isNotEmpty) stage = Stage.pages;
        return;
      }
      final next = [...pages];
      if (replace != null) {
        next[replace] = scanned.single;
      } else {
        next.addAll(scanned);
      }
      await native.checkpoint(next);
      pages = next;
      selected = replace ?? next.length - scanned.length;
      retakeIndex = null;
      stage = Stage.pages;
    });
  }

  Future<void> _edit() async {
    final edited = await Navigator.of(context).push<ScanPage>(
      MaterialPageRoute(
        builder: (_) => CropScreen(page: pages[selected], native: native),
      ),
    );
    if (edited != null && mounted)
      await _guard('Menyimpan perubahan…', () async {
        final next = [...pages]..[selected] = edited;
        await native.checkpoint(next);
        pages = next;
      });
  }

  Future<void> _delete() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder:
          (c) => AlertDialog(
            title: const Text('Hapus halaman ini?'),
            content: const Text('Halaman lain tetap tersimpan.'),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(c, false),
                child: const Text('Batal'),
              ),
              TextButton(
                onPressed: () => Navigator.pop(c, true),
                child: const Text('Hapus'),
              ),
            ],
          ),
    );
    if (confirmed != true || !mounted) return;
    await _guard('Menghapus halaman…', () async {
      final next = [...pages]..removeAt(selected);
      await native.checkpoint(next);
      pages = next;
      selected = math.max(0, math.min(selected, pages.length - 1));
      if (pages.isEmpty) stage = Stage.scan;
    });
    if (stage == Stage.scan) await _startCamera();
  }

  Future<void> _save() async {
    if (busy || pages.isEmpty) return;
    String? title;
    // If draft cleanup failed after a successful save, retry completion without
    // creating a duplicate PDF. Editing the pages starts a separate save.
    if (!finishingSavedScan) {
      title = await askDocumentName(
        context,
        'Scan ${DateTime.now().day}-${DateTime.now().month}-${DateTime.now().year}',
        title: 'Simpan PDF',
      );
      if (title == null || !mounted) return;
      savedAwaitingFinish = null;
    }
    Map? saved;
    var completed = false, returnRequested = false;
    await _guard('Membuat dan memeriksa PDF…', () async {
      if (!finishingSavedScan) {
        final paths = [for (final page in pages) page.image];
        final result = await native.call<Map>('save', {
          'name': title,
          'pages': paths,
        });
        if (result == null ||
            result['id'] is! String ||
            result['pages'] != paths.length ||
            (result['bytes'] as num? ?? 0) <= 0) {
          throw const FormatException('PDF save not confirmed');
        }
        savedAwaitingFinish = result;
        savedImagePaths = paths;
      }
      saved = Map.of(savedAwaitingFinish!)..remove('preview');
      // Draft checkpoint cleanup deletes source photos. Seed the reader using a
      // separate gallery thumbnail, never a path that is about to be deleted.
      try {
        saved!['preview'] = await galleryThumbnails.load(saved!['id']);
      } catch (_) {
        /* The reader can render the verified PDF directly. */
      }
      var listed = documents;
      try {
        listed = (await native.call<List>('list'))!.cast<Map>();
      } catch (_) {
        error =
            'PDF tersimpan. Daftar dokumen lain belum diperbarui; buka kembali Dokumen Saya.';
      }
      try {
        await native.checkpoint([]);
      } catch (_) {
        throw PlatformException(
          code: 'finish_save',
          message:
              'PDF sudah tersimpan. Ketuk Selesaikan penyimpanan untuk menutup sesi scan.',
        );
      }
      pages = [];
      selected = 0;
      retakeIndex = null;
      savedAwaitingFinish = null;
      savedImagePaths = [];
      selectedDocuments.clear();
      selectionMode = false;
      gallerySearch.clear();
      galleryBucket = PageStorageBucket();
      documents = [
        saved!,
        for (final doc in listed)
          if (doc['id'] != saved!['id']) doc,
      ];
      libraryOrigin = Stage.scan;
      stage = Stage.library;
      completed = true;
      // A Back request during saving now lands on the gallery, without stepping
      // through it into the scanner when _guard completes.
      returnRequested = backQueued || pdfQueued;
      backQueued = false;
    });
    if (completed && saved != null && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Tersimpan di HP'),
          duration: Duration(seconds: 2),
          behavior: SnackBarBehavior.floating,
          margin: EdgeInsets.fromLTRB(16, 96, 16, 96),
        ),
      );
      if (!returnRequested && !externalMode && stage == Stage.library)
        await _openDocument(saved!);
    }
  }

  Future<void> _library() async {
    if (stage != Stage.library) libraryOrigin = stage;
    _stopCamera();
    await _guard('Membuka Dokumen Saya…', () async {
      documents = (await native.call<List>('list'))!.cast<Map>();
      stage = Stage.library;
    });
  }

  void _scan({int? replace}) {
    retakeIndex = replace;
    setState(() => stage = Stage.scan);
    _startCamera();
  }

  Future<void> _newScan() async {
    if (pages.isNotEmpty) {
      final confirmed = await showDialog<bool>(
        context: context,
        builder:
            (c) => AlertDialog(
              title: const Text('Mulai scan baru?'),
              content: const Text(
                'Draf scan saat ini akan dibuang. PDF yang sudah disimpan tetap ada di Dokumen Saya.',
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.pop(c, false),
                  child: const Text('Batal'),
                ),
                FilledButton(
                  onPressed: () => Navigator.pop(c, true),
                  child: const Text('Scan baru'),
                ),
              ],
            ),
      );
      if (confirmed != true || !mounted) return;
    }
    await _guard('Menyiapkan scan…', () async {
      await native.checkpoint([]);
      pages = [];
      selected = 0;
      retakeIndex = null;
      savedAwaitingFinish = null;
      savedImagePaths = [];
      stage = Stage.scan;
    });
    if (stage == Stage.scan) _startCamera();
  }

  Future<void> _back() async {
    if (busy) {
      setState(() {
        backQueued = true;
        progress = 'Menyelesaikan proses sebelum kembali…';
      });
      return;
    }
    if (stage == Stage.library && selectionMode) {
      setState(() {
        selectedDocuments.clear();
        selectionMode = false;
      });
      return;
    }
    if (stage == Stage.library) {
      setState(() => stage = libraryOrigin);
      if (stage == Stage.scan) _startCamera();
    } else if (stage == Stage.pages) {
      _scan();
    } else {
      _stopCamera();
      exiting = true;
      // Every capture/edit already has a durable checkpoint. Never replace it
      // with an empty draft when recovery failed.
      await SystemNavigator.pop();
    }
  }

  Future<void> _openDocument(Map document) async {
    FocusScope.of(context).unfocus();
    final changed = await Navigator.push<bool>(
      context,
      MaterialPageRoute(
        builder:
            (_) => PdfReader(
              document: {
                ...document,
                'preview':
                    document['preview'] ??
                    galleryThumbnails.peek(document['id']),
              },
              native: native,
            ),
      ),
    );
    if (changed == true && mounted && stage == Stage.library) {
      await _guard('Memperbarui dokumen…', () async {
        documents = (await native.call<List>('list'))!.cast<Map>();
      });
    }
  }

  void _toggleDocument(Map document) {
    FocusScope.of(context).unfocus();
    final id = document['id'] as String;
    if (!selectedDocuments.contains(id) && selectedDocuments.length >= 100) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Pilih maksimal 100 dokumen dalam satu waktu.'),
        ),
      );
      return;
    }
    setState(() {
      selectionMode = true;
      if (!selectedDocuments.remove(id)) selectedDocuments.add(id);
    });
  }

  Future<void> _deleteDocuments() async {
    final ids = selectedDocuments.toList();
    if (ids.isEmpty ||
        !await confirmDeleteDocuments(context, ids.length) ||
        !mounted)
      return;
    await _guard('Menghapus dokumen…', () async {
      await native.call('deleteDocuments', {'ids': ids});
      documents = documents.where((d) => !ids.contains(d['id'])).toList();
      selectedDocuments.clear();
      selectionMode = false;
    });
  }

  Widget _draftBody() => Center(
    child: SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Icons.history, size: 52),
          const SizedBox(height: 18),
          const Text(
            'Lanjutkan scan sebelumnya?',
            style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
            textAlign: TextAlign.center,
          ),
          const SizedBox(height: 8),
          Text('${pages.length} halaman tersimpan sebagai draf.'),
          const SizedBox(height: 24),
          FilledButton(
            onPressed: () => setState(() => stage = Stage.pages),
            child: const Text('Lanjutkan scan'),
          ),
          TextButton(onPressed: _newScan, child: const Text('Scan baru')),
          TextButton(onPressed: _library, child: const Text('Dokumen Saya')),
        ],
      ),
    ),
  );

  @override
  Widget build(BuildContext context) {
    if (externalMode) return _incomingBody();
    if (checkingLaunch)
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    final scanning = stage == Stage.scan;
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, _) {
        if (!didPop) _back();
      },
      child: Scaffold(
        backgroundColor: scanning ? const Color(0xFF151817) : canvas,
        appBar: AppBar(
          backgroundColor: scanning ? const Color(0xFF151817) : Colors.white,
          foregroundColor: scanning ? Colors.white : navy,
          leading:
              scanning
                  ? IconButton(
                    tooltip: 'Dokumen Saya',
                    onPressed: busy ? null : _library,
                    icon: const Icon(Icons.folder_outlined),
                  )
                  : IconButton(
                    tooltip: 'Kembali',
                    onPressed: _back,
                    icon: const Icon(Icons.arrow_back),
                  ),
          title:
              scanning
                  ? Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Image.asset('assets/oto.png', width: 31, height: 31),
                      const SizedBox(width: 10),
                      Flexible(
                        child: Text(
                          applicationName,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            color: Colors.white,
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  )
                  : Text(switch (stage) {
                    Stage.pages => '${pages.length} halaman',
                    Stage.draft => 'Draf scan',
                    Stage.library when selectionMode =>
                      '${selectedDocuments.length} dipilih',
                    _ => 'Dokumen Saya',
                  }),
          actions:
              scanning
                  ? widget.useGoogleScanner
                      ? [
                        IconButton(
                          tooltip: 'Privasi & tentang',
                          onPressed:
                              busy
                                  ? null
                                  : () => Navigator.of(context).push(
                                    MaterialPageRoute<void>(
                                      builder:
                                          (_) => const PrivacyScreen(
                                            googleScanner: true,
                                          ),
                                    ),
                                  ),
                          icon: const Icon(Icons.info_outline),
                        ),
                      ]
                      : [
                        IconButton(
                          tooltip: torch ? 'Matikan lampu' : 'Nyalakan lampu',
                          onPressed:
                              !camera || busy
                                  ? null
                                  : () async {
                                    await _guard('Mengatur lampu…', () async {
                                      await native.call('torch', {
                                        'enabled': !torch,
                                      });
                                      torch = !torch;
                                    });
                                  },
                          icon: Icon(torch ? Icons.flash_on : Icons.flash_off),
                        ),
                      ]
                  : stage == Stage.library
                  ? [
                    IconButton(
                      tooltip: 'Privasi & tentang',
                      onPressed:
                          busy
                              ? null
                              : () => Navigator.of(context).push(
                                MaterialPageRoute<void>(
                                  builder:
                                      (_) => PrivacyScreen(
                                        googleScanner: widget.useGoogleScanner,
                                      ),
                                ),
                              ),
                      icon: const Icon(Icons.info_outline),
                    ),
                    TextButton(
                      onPressed:
                          busy
                              ? null
                              : () => setState(() {
                                selectionMode = !selectionMode;
                                selectedDocuments.clear();
                              }),
                      child: Text(selectionMode ? 'Selesai' : 'Pilih'),
                    ),
                  ]
                  : stage == Stage.pages
                  ? [
                    PopupMenuButton<String>(
                      tooltip: 'Pilihan scan',
                      onSelected: (value) {
                        if (value == 'library') {
                          _library();
                        } else {
                          _newScan();
                        }
                      },
                      enabled: !busy,
                      itemBuilder:
                          (_) => const [
                            PopupMenuItem(
                              value: 'library',
                              child: Text('Dokumen Saya'),
                            ),
                            PopupMenuItem(
                              value: 'new',
                              child: Text('Scan baru'),
                            ),
                          ],
                    ),
                  ]
                  : null,
        ),
        body: SafeArea(
          top: false,
          child: Stack(
            children: [
              Column(
                children: [
                  if (error != null)
                    Material(
                      color: const Color(0xFFFFE7E7),
                      child: Padding(
                        padding: const EdgeInsets.all(12),
                        child: Text(
                          error!,
                          style: const TextStyle(color: Color(0xFF99162A)),
                        ),
                      ),
                    ),
                  Expanded(
                    child: switch (stage) {
                      Stage.scan =>
                        widget.useGoogleScanner ? _googleBody() : _cameraBody(),
                      Stage.pages => _pagesBody(),
                      Stage.draft => _draftBody(),
                      Stage.library => _libraryBody(),
                    },
                  ),
                ],
              ),
              if (busy)
                Positioned.fill(
                  child: ColoredBox(
                    color: Colors.black54,
                    child: Center(
                      child: Card(
                        child: Padding(
                          padding: const EdgeInsets.all(24),
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              const CircularProgressIndicator(),
                              const SizedBox(height: 18),
                              Text(progress),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _googleBody() => Center(
    child: SingleChildScrollView(
      padding: const EdgeInsets.all(28),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Image.asset('assets/oto.png', width: 96, height: 96),
          const SizedBox(height: 24),
          const Text(
            'Scan dokumen lebih mudah',
            textAlign: TextAlign.center,
            style: TextStyle(
              color: Colors.white,
              fontSize: 24,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 12),
          const Text(
            'Gunakan mode Otomatis di kamera. Tampilkan seluruh tepi dokumen dan tahan HP hingga foto diambil.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.white, fontSize: 16),
          ),
          const SizedBox(height: 10),
          const Text(
            'Potong, luruskan, dan jernihkan di pemindai Google. Ketuk Selesai untuk menyusun halaman dan menyimpan PDF di OTO.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.white70),
          ),
          const SizedBox(height: 24),
          FilledButton.icon(
            onPressed: busy ? null : _startCamera,
            icon: const Icon(Icons.document_scanner_outlined),
            label: Text(
              retakeIndex == null ? 'Mulai scan' : 'Ambil ulang halaman',
            ),
          ),
          if (pages.isNotEmpty)
            TextButton(
              onPressed: busy ? null : _showPages,
              child: Text(
                'Lihat ${pages.length} halaman',
                style: const TextStyle(color: Colors.white),
              ),
            ),
          const SizedBox(height: 16),
          const Text(
            'Pemakaian pertama memerlukan internet untuk menyiapkan pemindai Google.',
            textAlign: TextAlign.center,
            style: TextStyle(color: Colors.white70, fontSize: 12),
          ),
        ],
      ),
    ),
  );

  Widget _cameraBody() => Column(
    children: [
      Expanded(
        child: Stack(
          fit: StackFit.expand,
          children: [
            if (camera)
              AndroidView(
                key: ValueKey(cameraEpoch),
                viewType: 'id.ayani.oto/camera',
              )
            else
              Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(
                      Icons.document_scanner_outlined,
                      size: 68,
                      color: Colors.white54,
                    ),
                    const SizedBox(height: 18),
                    TextButton(
                      onPressed: busy ? null : _startCamera,
                      child: const Text('Coba kamera'),
                    ),
                  ],
                ),
              ),
            IgnorePointer(
              child: CustomPaint(
                painter: CornerPainter(liveCorners, guide: true),
              ),
            ),
            if (camera)
              LayoutBuilder(
                builder:
                    (context, size) => GestureDetector(
                      key: const ValueKey('camera-focus-target'),
                      behavior: HitTestBehavior.opaque,
                      onTapUp:
                          (event) => _selectObject(
                            Offset(
                              (event.localPosition.dx / size.maxWidth).clamp(
                                0,
                                1,
                              ),
                              (event.localPosition.dy / size.maxHeight).clamp(
                                0,
                                1,
                              ),
                            ),
                          ),
                      child: Stack(
                        children: [
                          if (focusPoint != null)
                            Positioned(
                              left: focusPoint!.dx * size.maxWidth - 22,
                              top: focusPoint!.dy * size.maxHeight - 22,
                              child: IgnorePointer(
                                child: Container(
                                  width: 44,
                                  height: 44,
                                  decoration: BoxDecoration(
                                    border: Border.all(
                                      color: Colors.amber,
                                      width: 2,
                                    ),
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                ),
                              ),
                            ),
                        ],
                      ),
                    ),
              ),
            Positioned(
              top: 12,
              left: 18,
              right: 18,
              child: IgnorePointer(
                child: Text(
                  focusHint,
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 12,
                    shadows: [Shadow(blurRadius: 5, color: Colors.black)],
                  ),
                ),
              ),
            ),
            Positioned(
              left: 24,
              right: 24,
              bottom: 18,
              child: Center(
                child: GestureDetector(
                  onTap:
                      liveDetection.failed && camera && !busy
                          ? _restartCamera
                          : null,
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                      horizontal: 15,
                      vertical: 9,
                    ),
                    decoration: BoxDecoration(
                      color: Colors.black54,
                      borderRadius: BorderRadius.circular(30),
                    ),
                    child: Text(
                      liveDetection.message,
                      textAlign: TextAlign.center,
                      style: const TextStyle(color: Colors.white, fontSize: 12),
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
      Padding(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 16),
        child: Column(
          children: [
            Text(
              retakeIndex == null
                  ? 'Dokumen'
                  : 'Ambil ulang halaman ${retakeIndex! + 1}',
              style: const TextStyle(color: Colors.white70, fontSize: 12),
            ),
            const SizedBox(height: 12),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                SizedBox(
                  width: 74,
                  child: TextButton(
                    onPressed: pages.isEmpty || busy ? null : _showPages,
                    child: Text(
                      '${pages.length} halaman',
                      style: const TextStyle(color: Colors.white, fontSize: 12),
                    ),
                  ),
                ),
                Semantics(
                  label: 'Ambil foto dokumen',
                  button: true,
                  child: GestureDetector(
                    onTap: !camera || busy ? null : _capture,
                    child: Container(
                      width: 76,
                      height: 76,
                      padding: const EdgeInsets.all(5),
                      decoration: BoxDecoration(
                        shape: BoxShape.circle,
                        border: Border.all(color: Colors.white, width: 2),
                      ),
                      child: const DecoratedBox(
                        decoration: BoxDecoration(
                          color: Colors.white,
                          shape: BoxShape.circle,
                        ),
                      ),
                    ),
                  ),
                ),
                SizedBox(
                  width: 74,
                  child: TextButton(
                    onPressed: busy || pages.isEmpty ? null : _showPages,
                    child: const Text(
                      'Selesai',
                      style: TextStyle(color: Colors.white, fontSize: 12),
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    ],
  );
  void _showPages() {
    retakeIndex = null;
    _stopCamera();
    setState(() => stage = Stage.pages);
  }

  Widget _pageImage(String path, {bool zoom = false}) => Padding(
    padding: const EdgeInsets.all(16),
    child: InteractiveViewer(
      key: ValueKey(path),
      panEnabled: zoom,
      scaleEnabled: zoom,
      minScale: 1,
      maxScale: 6,
      child: Image.file(
        File(path),
        fit: BoxFit.contain,
        filterQuality: FilterQuality.high,
        errorBuilder:
            (_, __, ___) =>
                const Center(child: Text('Pratinjau tidak tersedia')),
      ),
    ),
  );
  Widget _pagesBody() {
    if (pages.isEmpty) return const Center(child: Text('Belum ada halaman.'));
    return Column(
      children: [
        Expanded(
          child: GestureDetector(
            key: const ValueKey('edit-page'),
            behavior: HitTestBehavior.opaque,
            onTap: busy ? null : _edit,
            child: _pageImage(pages[selected].image, zoom: true),
          ),
        ),
        Padding(
          padding: const EdgeInsets.only(bottom: 8),
          child: Text(
            pages[selected].detected
                ? 'Sudah dipotong otomatis · ketuk untuk rapikan'
                : 'Ketuk halaman untuk rapikan',
            style: const TextStyle(fontSize: 12, color: Color(0xFF667080)),
          ),
        ),
        if (pages[selected].orientationWarning != null)
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Text(
              pages[selected].orientationWarning!,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 12, color: Color(0xFF667080)),
            ),
          ),
        SizedBox(
          height: 98,
          child: ReorderableListView.builder(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 14),
            itemCount: pages.length,
            onReorder: (from, to) async {
              if (busy) return;
              if (to > from) to--;
              await _guard('Menyusun halaman…', () async {
                final next = moved(pages, from, to);
                await native.checkpoint(next);
                pages = next;
                selected = to;
              });
            },
            itemBuilder:
                (_, i) => GestureDetector(
                  key: ValueKey(pages[i].source),
                  onTap: () => setState(() => selected = i),
                  child: Container(
                    width: 72,
                    margin: const EdgeInsets.all(4),
                    padding: const EdgeInsets.all(4),
                    decoration: BoxDecoration(
                      color: Colors.white,
                      border: Border.all(
                        color: i == selected ? red : const Color(0xFFDADBDE),
                        width: i == selected ? 2 : 1,
                      ),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Stack(
                      children: [
                        Positioned.fill(
                          child: Image.file(
                            File(pages[i].image),
                            fit: BoxFit.contain,
                            cacheWidth: 160,
                          ),
                        ),
                        CircleAvatar(
                          radius: 10,
                          backgroundColor: i == selected ? red : Colors.black54,
                          child: Text(
                            '${i + 1}',
                            style: const TextStyle(
                              fontSize: 11,
                              color: Colors.white,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
          ),
        ),
        const Text(
          'Tahan lalu geser thumbnail untuk mengubah urutan',
          style: TextStyle(fontSize: 10, color: Color(0xFF667080)),
        ),
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
          children: [
            _tool(Icons.add_circle_outline, 'Tambah', _scan),
            _tool(Icons.crop, 'Rapikan', _edit),
            _tool(Icons.camera_alt_outlined, 'Ambil ulang', () {
              _scan(replace: selected);
            }),
            _tool(Icons.delete_outline, 'Hapus', _delete),
          ],
        ),
        _bottomButton(
          finishingSavedScan ? 'Selesaikan penyimpanan' : 'Simpan PDF',
          _save,
          caption: 'Disimpan di HP Anda',
        ),
      ],
    );
  }

  Widget _tool(IconData icon, String label, VoidCallback tap) => TextButton(
    onPressed: busy ? null : tap,
    style: TextButton.styleFrom(
      foregroundColor: navy,
      minimumSize: const Size(48, 58),
    ),
    child: Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(icon, size: 23),
        const SizedBox(height: 3),
        Text(label, style: const TextStyle(fontSize: 11)),
      ],
    ),
  );
  Widget _bottomButton(
    String title,
    VoidCallback tap, {
    String? caption,
  }) => Padding(
    padding: const EdgeInsets.fromLTRB(16, 4, 16, 16),
    child: Column(
      children: [
        if (caption != null)
          Padding(
            padding: const EdgeInsets.only(bottom: 7),
            child: Text(
              caption,
              style: const TextStyle(fontSize: 11, color: Color(0xFF667080)),
            ),
          ),
        SizedBox(
          width: double.infinity,
          child: FilledButton(onPressed: busy ? null : tap, child: Text(title)),
        ),
      ],
    ),
  );
  Widget _libraryBody() => Column(
    children: [
      Expanded(
        child: PageStorage(
          bucket: galleryBucket,
          child: DocumentGallery(
            documents: documents,
            search: gallerySearch,
            thumbnails: galleryThumbnails,
            selectedIds: selectedDocuments,
            selectionMode: selectionMode,
            onToggle: _toggleDocument,
            onOpen: _openDocument,
          ),
        ),
      ),
      if (selectionMode)
        Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              Expanded(
                child: OutlinedButton.icon(
                  onPressed:
                      busy || selectedDocuments.isEmpty
                          ? null
                          : _deleteDocuments,
                  icon: const Icon(Icons.delete_outline),
                  label: const Text('Hapus'),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: FilledButton.icon(
                  onPressed:
                      busy || selectedDocuments.isEmpty
                          ? null
                          : () {
                            final ids = selectedDocuments.toList();
                            _guard(
                              'Menyiapkan PDF untuk dikirim…',
                              () => shareDocuments(native, ids),
                            );
                          },
                  icon: const Icon(Icons.share_outlined),
                  label: Text('Kirim (${selectedDocuments.length})'),
                ),
              ),
            ],
          ),
        )
      else
        _bottomButton('Scan baru', _newScan),
      if (const bool.fromEnvironment('OTO_TEST_MODE'))
        Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: TextButton(
            onPressed: () => _capture(synthetic: true),
            child: const Text('Uji sintetis · bukan foto kamera'),
          ),
        ),
    ],
  );
}
