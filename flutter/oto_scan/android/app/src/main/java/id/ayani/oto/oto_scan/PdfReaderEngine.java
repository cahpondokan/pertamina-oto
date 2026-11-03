package id.ayani.oto.oto_scan;
import android.content.Context;
import android.graphics.*;
import android.graphics.pdf.PdfRenderer;
import android.os.ParcelFileDescriptor;
import java.io.*;
import java.util.*;

/** All access is serialized on the dedicated reader worker, separate from thumbnails. */
final class PdfReaderEngine {
  final ReaderDiskCache cache;
  final Context context;
  PdfRenderer renderer;
  File source;
  String token,id,fingerprint;
  boolean external;
  PdfReaderEngine(Context context){this.context=context;cache=new ReaderDiskCache(new File(context.getCacheDir(),"reader-v1"));}
  Map<String,Object> open(String token,String id,File source,boolean external)throws Exception{
    close(null);
    if(token==null||!token.matches("[a-zA-Z0-9_-]{1,80}")||id==null||!id.matches("[a-zA-Z0-9-]{1,80}"))throw new IOException("Sesi PDF tidak valid.");
    ParcelFileDescriptor fd=ParcelFileDescriptor.open(source,ParcelFileDescriptor.MODE_READ_ONLY);
    try{renderer=new PdfRenderer(fd);}catch(Exception e){fd.close();throw e;}
    this.token=token;this.id=id;this.source=source;this.external=external;
    fingerprint=id+"-"+source.length()+"-"+source.lastModified();
    try{
      cache.trim();
      // Remove only obsolete large reader PNGs from pre-r11, never drafts/PDFs.
      File[] old=context.getCacheDir().listFiles((dir,name)->name.matches("preview-[a-f0-9-]{36}-[0-9]+-detail\\.png"));
      if(old!=null)for(File file:old)file.delete();
      int count=renderer.getPageCount();if(count<1||count>10000)throw new IOException("Jumlah halaman PDF tidak didukung.");
      try(PdfRenderer.Page first=renderer.openPage(0)){
        return ScanStore.map("pages",count,"width",first.getWidth(),"height",first.getHeight());
      }
    }catch(Exception e){close(token);throw e;}
  }
  Map<String,Object> render(String token,int page,int requested,boolean detail,boolean refresh,Map<?,?> tile)throws Exception{
    if(renderer==null||!Objects.equals(token,this.token))throw new IOException("Sesi PDF sudah ditutup.");
    if(page<0||page>=renderer.getPageCount())throw new IOException("Halaman tidak tersedia.");
    long start=System.nanoTime();
    try(PdfRenderer.Page current=renderer.openPage(page)){
      int w=current.getWidth(),h=current.getHeight();int[] size=ReaderPolicy.dimensions(w,h,Math.min(7200,Math.max(256,requested)),detail);
      int tx=0,ty=0;
      if(tile!=null){
        tx=((Number)tile.get("x")).intValue();ty=((Number)tile.get("y")).intValue();
        size=ReaderPolicy.tile(w,h,requested,tx,ty,((Number)tile.get("width")).intValue(),((Number)tile.get("height")).intValue());
      }
      String key=fingerprint+"-"+page+"-v2-"+size[0]+"x"+size[1]+(tile!=null?"-t"+requested+"-"+tx+"-"+ty:"-s");
      if(refresh)cache.remove(fingerprint+"-"+page+"-");
      byte[] bytes=null;
      if(!refresh)try{bytes=cache.get(key);}catch(IOException ignored){}
      boolean hit=bytes!=null;
      if(bytes==null){
        Bitmap bitmap=Bitmap.createBitmap(size[0],size[1],Bitmap.Config.ARGB_8888);
        try{
          bitmap.eraseColor(Color.WHITE);
          Matrix transform=null;
          if(tile!=null){transform=new Matrix();transform.setScale(requested/(float)w,requested/(float)w);transform.postTranslate(-tx,-ty);}
          current.render(bitmap,null,transform,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);
          ByteArrayOutputStream out=new ByteArrayOutputStream();
          if(!bitmap.compress(tile!=null?Bitmap.CompressFormat.PNG:Bitmap.CompressFormat.JPEG,tile!=null?100:94,out))throw new IOException("Pratinjau belum berhasil dibuat.");
          bytes=out.toByteArray();
        }finally{bitmap.recycle();}
        // A failed disposable cache must not prevent viewing the already-rendered page.
        try{cache.put(key,bytes);}catch(IOException ignored){}
      }
      return ScanStore.map("bytes",bytes,"width",w,"height",h,"rasterWidth",size[0],"rasterHeight",size[1],"cacheHit",hit,"elapsedMicros",(System.nanoTime()-start)/1000);
    }
  }
  void invalidate(List<?> ids){for(Object value:ids){String id=String.valueOf(value);if(id.matches("[a-f0-9-]{36}"))cache.remove(id+"-");}}
  void close(String requested){
    if(requested!=null&&!Objects.equals(requested,token))return;
    if(renderer!=null){renderer.close();renderer=null;}
    if(fingerprint!=null&&(external||source==null||!source.exists()))cache.remove(fingerprint+"-");
    token=null;id=null;source=null;fingerprint=null;
  }
}
