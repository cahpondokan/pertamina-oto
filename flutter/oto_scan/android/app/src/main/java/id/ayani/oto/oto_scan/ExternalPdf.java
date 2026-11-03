package id.ayani.oto.oto_scan;

import android.content.Context;
import android.database.Cursor;
import android.graphics.*;
import android.graphics.pdf.PdfRenderer;
import android.net.Uri;
import android.os.Environment;
import android.os.ParcelFileDescriptor;
import android.provider.OpenableColumns;
import java.io.*;
import java.nio.file.Files;
import java.util.*;

/** Temporary read-only PDF session. Calls are serialized on MainActivity's worker. */
final class ExternalPdf {
  final Context context;
  final File directory;
  final ScanStore store;
  String currentId;
  File currentFile;
  ExternalPdf(Context context,ScanStore store){
    this.context=context;this.store=store;
    // Separate activity instances/recreation must not remove each other's PDF.
    directory=new File(context.getCacheDir(),"external-reader-"+UUID.randomUUID());directory.mkdirs();
    File[] old=context.getCacheDir().listFiles((dir,name)->name.matches("external-reader-[a-f0-9-]{36}"));
    if(old!=null)for(File folder:old)if(folder.isDirectory()&&System.currentTimeMillis()-folder.lastModified()>86400000L){
      File[] files=folder.listFiles();if(files!=null)for(File file:files)if(file.isFile())file.delete();folder.delete();
    }
  }
  synchronized void clear(){
    File[] files=directory.listFiles();
    if(files!=null)for(File file:files)if(file.isFile())file.delete();
    currentId=null;currentFile=null;
  }
  synchronized Map<String,Object> open(Uri uri) throws Exception {
    if(uri==null)throw new IOException("Tidak ada PDF untuk dibuka.");
    String scheme=uri.getScheme();
    if(!"content".equals(scheme)&&!"file".equals(scheme))throw new IOException("Buka PDF lokal melalui aplikasi File atau Unduhan.");
    if("file".equals(scheme)){
      // A legacy file URI must never turn this exported activity into a reader
      // for arbitrary app-private files. Modern callers should grant content URIs.
      if(uri.getPath()==null)throw new IOException("Lokasi PDF tidak valid.");
      File file=new File(uri.getPath()).getCanonicalFile();
      File shared=Environment.getExternalStorageDirectory().getCanonicalFile();
      if(!file.toPath().startsWith(shared.toPath()))throw new IOException("Akses PDF tidak diizinkan. Buka dari aplikasi File.");
    }
    String name="Dokumen.pdf";
    if("content".equals(scheme)){
      try(Cursor cursor=context.getContentResolver().query(uri,new String[]{OpenableColumns.DISPLAY_NAME},null,null,null)){
        if(cursor!=null&&cursor.moveToFirst())name=PdfInput.name(cursor.getString(0));
      }catch(Exception ignored){} // Metadata is optional; opening still checks the grant.
    }else name=PdfInput.name(new File(uri.getPath()).getName());
    clear();
    String id=UUID.randomUUID().toString();
    File staging=new File(directory,id+".part"),target=new File(directory,id+".pdf");
    boolean published=false;
    try{
      if(directory.getUsableSpace()<PdfInput.MAX_BYTES+10_000_000L)throw new IOException("Ruang penyimpanan tidak cukup untuk membuka PDF.");
      try(InputStream input=context.getContentResolver().openInputStream(uri);FileOutputStream output=new FileOutputStream(staging)){
        PdfInput.copy(input,output,PdfInput.MAX_BYTES);output.getFD().sync();
      }catch(SecurityException e){throw new IOException("Izin membaca PDF tidak tersedia. Buka lagi dari aplikasi asal.");}
      int pages;
      try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(staging,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer renderer=new PdfRenderer(fd)){
        pages=renderer.getPageCount();
        if(pages<1||pages>10000)throw new IOException("Jumlah halaman PDF tidak didukung.");
      }catch(SecurityException e){throw new IOException("PDF terkunci atau dilindungi. Buka salinan tanpa kata sandi.");}
      catch(IOException e){throw new IOException("PDF rusak atau tidak dapat dibaca.");}
      Files.move(staging.toPath(),target.toPath());
      currentId=id;currentFile=target;published=true;
      return ScanStore.map("id",id,"name",name,"pages",pages,"bytes",target.length(),"external",true);
    }finally{staging.delete();if(!published)target.delete();}
  }
  synchronized File fileFor(String id)throws IOException{
    if(id==null||!id.equals(currentId)||currentFile==null||!currentFile.isFile())throw new IOException("Sesi PDF sudah ditutup.");
    return currentFile;
  }
  String render(String id,int index) throws Exception {
    if(id==null||!id.equals(currentId)||currentFile==null||!currentFile.isFile())throw new IOException("Sesi PDF sudah ditutup. Buka kembali dokumennya.");
    try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(currentFile,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer renderer=new PdfRenderer(fd)){
      if(index<0||index>=renderer.getPageCount())throw new IOException("Halaman tidak tersedia.");
      try(PdfRenderer.Page page=renderer.openPage(index)){
        int width=page.getWidth(),height=page.getHeight();
        if(width<=0||height<=0)throw new IOException("Ukuran halaman PDF tidak valid.");
        double scale=Math.min(3800.0/Math.max(width,height),Math.sqrt(8_000_000.0/((double)width*height)));
        Bitmap bitmap=Bitmap.createBitmap(Math.max(1,(int)(width*scale)),Math.max(1,(int)(height*scale)),Bitmap.Config.ARGB_8888);
        File result=new File(directory,id+"-"+index+".png");
        try{bitmap.eraseColor(Color.WHITE);page.render(bitmap,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);store.png(bitmap,result);}
        finally{bitmap.recycle();}
        // Keep only two page rasters; the source and previous page stay bounded.
        File[] rasters=directory.listFiles((dir,name)->name.endsWith(".png"));
        if(rasters!=null){Arrays.sort(rasters,Comparator.comparingLong(File::lastModified).reversed());for(int i=2;i<rasters.length;i++)rasters[i].delete();}
        return result.getPath();
      }
    }
  }
  synchronized void close(String id){if(id==null||id.equals(currentId))clear();}
  synchronized void dispose(){clear();directory.delete();}
}
