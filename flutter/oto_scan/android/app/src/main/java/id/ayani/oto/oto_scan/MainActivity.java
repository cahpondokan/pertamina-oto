package id.ayani.oto.oto_scan;
import android.Manifest;
import android.app.Activity;
import android.content.*;
import android.content.pm.PackageManager;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.*;
import android.view.View;
import android.view.WindowManager;
import io.flutter.embedding.android.FlutterActivity;
import io.flutter.embedding.engine.FlutterEngine;
import io.flutter.plugin.common.*;
import io.flutter.plugin.platform.*;
import java.io.*;
import java.nio.file.Files;
import java.util.*;
import java.util.concurrent.*;

public class MainActivity extends FlutterActivity {
  CameraView cameraView;
  ScanStore store;
  ExternalPdf externalPdf;
  PdfReaderEngine reader;
  GoogleScanLauncher googleScanner;
  final ExecutorService readerWorker=Executors.newSingleThreadExecutor();
  MethodChannel scanChannel;
  Intent pendingPdf;
  final ExecutorService worker=Executors.newSingleThreadExecutor();
  final java.util.concurrent.atomic.AtomicBoolean detecting=new java.util.concurrent.atomic.AtomicBoolean();
  MethodChannel.Result permissionResult,exportResult;
  String exportId;
  @Override protected void onCreate(Bundle state){
    super.onCreate(state);
    getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
  }
  @Override public void configureFlutterEngine(FlutterEngine engine){
    super.configureFlutterEngine(engine);store=new ScanStore(this);externalPdf=new ExternalPdf(this,store);reader=new PdfReaderEngine(this);googleScanner=new GoogleScanLauncher(this);
    if(Intent.ACTION_VIEW.equals(getIntent().getAction()))pendingPdf=new Intent(getIntent());
    engine.getPlatformViewsController().getRegistry().registerViewFactory("id.ayani.oto/camera",new PlatformViewFactory(StandardMessageCodec.INSTANCE){
      @Override public PlatformView create(Context context,int id,Object args){
        if(cameraView!=null)cameraView.close();cameraView=new CameraView(MainActivity.this);return cameraView;
      }
    });
    scanChannel=new MethodChannel(engine.getDartExecutor().getBinaryMessenger(),"id.ayani.oto/scan");
    scanChannel.setMethodCallHandler((call,result)->{
      try{
        switch(call.method){
          case "googleScan":{
            Number limit=call.argument("limit");
            googleScanner.start(limit==null?20:limit.intValue(),result);return;
          }
          case "readerOpen":{
            String token=call.argument("session"),id=call.argument("id");boolean ext=Boolean.TRUE.equals(call.argument("external"));
            readerAsync(result,()->reader.open(token,id,ext?externalPdf.fileFor(id):store.document(id),ext));return;
          }
          case "readerRender":readerAsync(result,()->reader.render(call.argument("session"),((Number)call.argument("page")).intValue(),((Number)call.argument("width")).intValue(),Boolean.TRUE.equals(call.argument("detail")),Boolean.TRUE.equals(call.argument("refresh")),call.argument("tile")));return;
          case "readerClose":readerAsync(result,()->{reader.close(call.argument("session"));return null;});return;
          case "hasPendingPdf":result.success(pendingPdf!=null);return;
          case "openPendingPdf":{
            Intent incoming=pendingPdf;pendingPdf=null;
            if(incoming==null){result.success(null);return;}
            async(result,()->externalPdf.open(incoming.getData()));return;
          }
          case "permission":
            if(checkSelfPermission(Manifest.permission.CAMERA)==PackageManager.PERMISSION_GRANTED){result.success(true);return;}
            if(permissionResult!=null){result.success(false);return;}
            permissionResult=result;requestPermissions(new String[]{Manifest.permission.CAMERA},71);return;
          case "capture":{if(cameraView==null)throw new IOException("Kamera belum siap.");
            Number frame=call.argument("previousFrame");cameraView.capture(result,target(call),frame==null?0:frame.longValue());return;}
          case "focus":if(cameraView==null)throw new IOException("Kamera belum siap.");
            cameraView.focus(((Number)call.argument("x")).doubleValue(),((Number)call.argument("y")).doubleValue(),result);return;
          case "detect":{
            DetectionTarget target=target(call);
            CameraView view=cameraView;CameraView.AnalysisSample sample=view==null?null:view.analysisSample();
            if(sample==null){result.success(ScanStore.map("status",view==null?"waiting":view.analysisStatus()));return;}
            if(!detecting.compareAndSet(false,true)){result.success(ScanStore.map("status","processing"));return;}
            try{async(result,()->{
              try{
                PreviewPixels.Frame frame=sample.frame;
                double[] q=DocumentDetector.detectPreview(frame.pixels,frame.width,frame.height,target);
                if(view.closed)return ScanStore.map("status","unavailable");
                return ScanStore.map("status","ready","corners",q==null?null:ScanStore.list(q),"frame",sample.time);
              }finally{detecting.set(false);}
            });}catch(Exception e){detecting.set(false);throw e;}return;
          }
          case "torch":if(cameraView==null)throw new IOException("Kamera belum siap.");cameraView.torch(Boolean.TRUE.equals(call.argument("enabled")));result.success(null);return;
          case "share":
          case "shareMany":{
            List<?> requested=call.method.equals("share")?Collections.singletonList(call.argument("id")):call.argument("ids");
            worker.execute(()->prepareShare(requested,result));return;
          }
          case "export":{
            if(exportResult!=null)throw new IOException("Dialog simpan masih terbuka.");
            exportId=call.argument("id");store.document(exportId);
            exportResult=result;
            Intent intent=new Intent(Intent.ACTION_CREATE_DOCUMENT).setType("application/pdf").addCategory(Intent.CATEGORY_OPENABLE);
            String title=call.argument("name");intent.putExtra(Intent.EXTRA_TITLE,title==null?"OTO Scan.pdf":title.replaceAll("[\\p{Cntrl}/\\\\]"," "));
            try{startActivityForResult(intent,72);}catch(Exception e){exportResult=null;exportId=null;throw e;}return;
          }
          default:async(result,()->{
            switch(call.method){
              case "recover":return store.recover();
              case "checkpoint":store.checkpoint(call.argument("json"));return null;
              case "edit":return store.edit((Map<?,?>)call.arguments);
              case "save":return store.save(call.argument("name"),call.argument("pages"));
              case "list":return store.list();
              case "rename":return store.rename(call.argument("id"),call.argument("name"));
              case "deleteDocuments":{List<?> ids=call.argument("ids");int count=store.deleteDocuments(ids);readerWorker.execute(()->reader.invalidate(ids));return count;}
              case "thumbnail":return store.thumbnail(call.argument("id"),Boolean.TRUE.equals(call.argument("refresh")));
              case "document":return store.info(call.argument("id"),true);
              case "render":return store.render(call.argument("id"),((Number)call.argument("page")).intValue(),Boolean.TRUE.equals(call.argument("detail")));
              case "renderExternal":return externalPdf.render(call.argument("id"),((Number)call.argument("page")).intValue());
              case "closeExternal":externalPdf.close(call.argument("id"));return null;
              case "synthetic":if(!BuildConfig.QA)throw new IOException();return store.synthetic();
              case "proof":{
                if(!BuildConfig.QA)throw new IOException();
                String state=call.argument("state");
                if(!Arrays.asList("pages","result","library","reader","crop").contains(state))throw new IOException();
                byte[] png=call.argument("png");if(png==null||png.length>8_000_000)throw new IOException();
                File folder=getExternalFilesDir("qa");folder.mkdirs();store.writeAtomic(new File(folder,state+".png"),png);
                String id=call.argument("id");if(id!=null)Files.copy(store.document(id).toPath(),new File(folder,"synthetic.pdf").toPath(),java.nio.file.StandardCopyOption.REPLACE_EXISTING);
                return null;
              }
              default:throw new IOException("Operasi tidak didukung.");
            }
          });
        }
      }catch(Exception e){result.error("scan",safeError(e),null);}
    });
  }
  void prepareShare(List<?> requested,MethodChannel.Result result){
    List<File> copies=new ArrayList<>();
    try{
      List<String> ids=DocumentBatch.ids(requested);
      List<File> sources=new ArrayList<>();List<String> names=new ArrayList<>();
      long bytes=0;
      for(String id:ids){File source=store.document(id);sources.add(source);bytes+=source.length();names.add((String)store.info(id,false,true).get("name"));}
      File folder=new File(getCacheDir(),"share");folder.mkdirs();
      File[] old=folder.listFiles();if(old!=null)for(File f:old)if(f.isFile()&&System.currentTimeMillis()-f.lastModified()>86400000L)f.delete();
      if(folder.getUsableSpace()<bytes+10_000_000L)throw new IOException("Ruang penyimpanan tidak cukup untuk menyiapkan kiriman.");
      ArrayList<Uri> uris=new ArrayList<>();
      for(int i=0;i<sources.size();i++){
        File copy=new File(folder,UUID.randomUUID()+".pdf");copies.add(copy);Files.copy(sources.get(i).toPath(),copy.toPath());
        File name=new File(folder,copy.getName()+".name");copies.add(name);
        store.writeAtomic(name,names.get(i).getBytes(java.nio.charset.StandardCharsets.UTF_8));
        uris.add(Uri.parse("content://"+getPackageName()+".share/share/"+copy.getName()));
      }
      runOnUiThread(()->{
        try{
          Intent send=new Intent(uris.size()==1?Intent.ACTION_SEND:Intent.ACTION_SEND_MULTIPLE).setType("application/pdf").addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
          if(uris.size()==1)send.putExtra(Intent.EXTRA_STREAM,uris.get(0));else send.putParcelableArrayListExtra(Intent.EXTRA_STREAM,uris);
          ClipData clip=ClipData.newRawUri("PDF",uris.get(0));for(int i=1;i<uris.size();i++)clip.addItem(new ClipData.Item(uris.get(i)));
          send.setClipData(clip);startActivity(Intent.createChooser(send,uris.size()==1?"Kirim PDF":"Kirim "+uris.size()+" PDF"));result.success(null);
        }catch(Exception e){for(File file:copies)file.delete();result.error("share","Pilihan berbagi tidak tersedia di HP ini.",null);}
      });
    }catch(Exception e){for(File file:copies)file.delete();runOnUiThread(()->result.error("share",safeError(e),null));}
  }
  @Override protected void onNewIntent(Intent intent){
    super.onNewIntent(intent);setIntent(intent);
    if(Intent.ACTION_VIEW.equals(intent.getAction())){
      pendingPdf=new Intent(intent);
      if(cameraView!=null)cameraView.close();
      if(scanChannel!=null)scanChannel.invokeMethod("pdfAvailable",null);
    }
  }
  static String safeError(Exception e){
    // Do not return native exceptions, raw paths or document content to logs/UI.
    if(e instanceof IOException&&e.getMessage()!=null&&!e.getMessage().contains("/")&&!e.getMessage().contains("\\")&&e.getMessage().length()<120)return e.getMessage();
    return "Proses belum berhasil. Coba lagi; dokumen sebelumnya tetap tersimpan.";
  }
  interface Work {Object run() throws Exception;}
  void async(MethodChannel.Result result,Work work){
    worker.execute(()->{try{Object value=work.run();runOnUiThread(()->result.success(value));}
      catch(Exception e){runOnUiThread(()->result.error("scan",safeError(e),null));}
      catch(OutOfMemoryError e){runOnUiThread(()->result.error("memory","Memori tidak cukup. Simpan lebih sedikit halaman.",null));}});
  }
  void readerAsync(MethodChannel.Result result,Work work){
    readerWorker.execute(()->{try{Object value=work.run();runOnUiThread(()->result.success(value));}
      catch(Exception e){runOnUiThread(()->result.error("reader",safeError(e),null));}
      catch(OutOfMemoryError e){runOnUiThread(()->result.error("memory","Memori tidak cukup untuk pratinjau. Tutup PDF lalu coba lagi.",null));}});
  }
  static DetectionTarget target(MethodCall call){
    List<?> p=call.argument("target");Object q=call.argument("previous");
    if(p==null&&q==null)return null;
    if(p!=null&&p.size()!=2)throw new IllegalArgumentException();
    return new DetectionTarget(p==null?null:new double[]{((Number)p.get(0)).doubleValue(),((Number)p.get(1)).doubleValue()},q==null?null:ScanStore.quad(q));
  }
  void processPhoto(byte[] bytes,MethodChannel.Result result){processPhoto(bytes,result,null,1,1,false,0,true);}
  void processPhoto(byte[] bytes,MethodChannel.Result result,DetectionTarget target,int vw,int vh,boolean uncertainFocus,int turns,boolean orientationKnown){async(result,()->{
    Map<String,Object> page=store.capture(bytes,target,vw,vh,turns);
    if(uncertainFocus&&page.get("qualityWarning")==null)page.put("qualityWarning","Fokus atau posisi HP belum stabil. Periksa tulisan; ambil ulang bila perlu.");
    if(!orientationKnown)page.put("orientationWarning","Orientasi HP belum terbaca. Periksa arah halaman dan gunakan Putar bila perlu.");
    return page;
  });}
  @Override public void onRequestPermissionsResult(int request,String[] permissions,int[] grants){
    super.onRequestPermissionsResult(request,permissions,grants);
    if(request==71&&permissionResult!=null){MethodChannel.Result r=permissionResult;permissionResult=null;r.success(grants.length>0&&grants[0]==PackageManager.PERMISSION_GRANTED);}
  }
  @Override protected void onActivityResult(int request,int code,Intent data){
    super.onActivityResult(request,code,data);
    if(googleScanner!=null&&googleScanner.onActivityResult(request,code,data))return;
    if(request==72&&exportResult!=null){
      MethodChannel.Result r=exportResult;exportResult=null;String id=exportId;exportId=null;
      if(code!=Activity.RESULT_OK||data==null||data.getData()==null){r.success(false);return;}
      Uri uri=data.getData();
      async(r,()->{try(InputStream in=new FileInputStream(store.document(id));OutputStream out=getContentResolver().openOutputStream(uri,"w")){
        if(out==null)throw new IOException("Tujuan simpan tidak tersedia.");byte[] buffer=new byte[65536];int count;while((count=in.read(buffer))!=-1)out.write(buffer,0,count);out.flush();return true;
      }});
    }
  }
  @Override protected void onPause(){if(cameraView!=null)cameraView.close();super.onPause();}
  @Override protected void onDestroy(){if(googleScanner!=null)googleScanner.close();if(cameraView!=null)cameraView.close();if(externalPdf!=null)worker.execute(()->externalPdf.dispose());worker.shutdown();if(reader!=null)readerWorker.execute(()->reader.close(null));readerWorker.shutdown();super.onDestroy();}
}
