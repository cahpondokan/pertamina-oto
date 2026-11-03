package id.ayani.oto.oto_scan;

import android.content.Context;
import android.graphics.Bitmap;
import android.net.Uri;
import java.io.*;
import java.util.*;

/** Import already-cropped SDK pages without a second detector or enhancement. */
final class GooglePageImport {
  static final int MAX_BYTES = 30_000_000;
  private final Context context;
  private final ScanStore store;
  GooglePageImport(Context context, ScanStore store) { this.context=context; this.store=store; }

  List<Map<String,Object>> importPages(List<Uri> uris, int limit) throws Exception {
    if (limit<1 || limit>20 || uris==null || uris.isEmpty() || uris.size()>limit)
      throw new IOException("Jumlah halaman scan tidak valid.");
    if (store.root.getUsableSpace()<100_000_000L)
      throw new IOException("Ruang penyimpanan HP tidak cukup.");
    List<File> created=new ArrayList<>();
    List<Map<String,Object>> pages=new ArrayList<>();
    try {
      for (Uri uri:uris) {
        byte[] bytes=read(uri);
        Bitmap image=store.decode(bytes);
        try {
          String id=UUID.randomUUID().toString();
          File source=new File(store.drafts,id+"-source.jpg");
          File page=new File(store.drafts,id+"-page.png");
          created.add(source); created.add(page);
          store.writeAtomic(source,bytes);
          store.png(image,page);
          pages.add(ScanStore.map("source",source.getPath(),"image",page.getPath(),
              "width",image.getWidth(),"height",image.getHeight(),
              "corners",ScanStore.list(new double[]{0,0,1,0,1,1,0,1}),
              "detected",true,"proposed",false,"aspect","free","turns",0,"filter","color"));
        } finally { image.recycle(); }
      }
      return pages;
    } catch (Exception e) {
      // All-or-nothing: only remove this import's newly created private files.
      for(File file:created) { file.delete(); new File(file.getPath()+".bak").delete(); }
      throw e;
    }
  }

  private byte[] read(Uri uri) throws IOException {
    if(uri==null)throw new IOException("Halaman scan tidak ditemukan.");
    if("file".equals(uri.getScheme())) {
      File file=new File(uri.getPath()).getCanonicalFile();
      String path=file.getPath();
      boolean allowed=path.startsWith(context.getCacheDir().getCanonicalPath()+File.separator)
          || path.startsWith(context.getFilesDir().getCanonicalPath()+File.separator);
      if(!allowed)throw new IOException("Lokasi hasil scan tidak diizinkan.");
    } else if(!"content".equals(uri.getScheme())) {
      throw new IOException("Lokasi hasil scan tidak diizinkan.");
    }
    try(InputStream input=context.getContentResolver().openInputStream(uri);
        ByteArrayOutputStream output=new ByteArrayOutputStream()) {
      if(input==null)throw new IOException("Halaman scan tidak dapat dibaca.");
      byte[] buffer=new byte[65536]; int count,total=0;
      while((count=input.read(buffer))!=-1) {
        total+=count;if(total>MAX_BYTES)throw new IOException("Halaman scan terlalu besar.");
        output.write(buffer,0,count);
      }
      if(total<4)throw new IOException("Halaman scan kosong.");
      return output.toByteArray();
    }
  }
}
