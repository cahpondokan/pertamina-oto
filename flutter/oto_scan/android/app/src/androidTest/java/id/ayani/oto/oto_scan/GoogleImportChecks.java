package id.ayani.oto.oto_scan;

import android.app.Activity;
import android.content.*;
import android.graphics.*;
import android.graphics.pdf.PdfRenderer;
import android.media.ExifInterface;
import android.net.Uri;
import android.os.*;
import java.io.*;
import java.nio.file.Files;
import java.util.*;
import org.json.*;

/** Synthetic adapter regression only; does not claim to test Google's detector. */
final class GoogleImportChecks {
  final ScanInstrumentation t;
  GoogleImportChecks(ScanInstrumentation t){this.t=t;}
  File fixture(Context c,int width,int height,int rotation)throws Exception{
    File f=new File(c.getCacheDir(),"mlkit-fixture-"+UUID.randomUUID()+".jpg");
    Bitmap b=Bitmap.createBitmap(width,height,Bitmap.Config.ARGB_8888);
    Canvas canvas=new Canvas(b);canvas.drawColor(Color.WHITE);
    Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);p.setColor(Color.BLACK);p.setTextSize(32);
    canvas.drawText("OTO SCAN - DOKUMEN UJI",32,64,p);
    p.setTextSize(21);
    for(int y=112;y<height-24;y+=36)canvas.drawText("Tulisan uji 0123456789 | ABCDEFGHIJ",32,y,p);
    p.setColor(Color.rgb(20,100,190));canvas.drawRect(0,0,14,height,p);
    try(OutputStream stream=new FileOutputStream(f)){b.compress(Bitmap.CompressFormat.JPEG,98,stream);}finally{b.recycle();}
    if(rotation!=1){ExifInterface exif=new ExifInterface(f);exif.setAttribute(ExifInterface.TAG_ORIENTATION,String.valueOf(rotation));exif.saveAttributes();}
    return f;
  }
  void run(){
    Bundle result=new Bundle();JSONObject report=new JSONObject();List<File> fixtures=new ArrayList<>();
    try{
      if(!Build.HARDWARE.contains("ranchu")&&!Build.HARDWARE.contains("goldfish"))throw new AssertionError("Emulator only");
      Context base=t.getTargetContext();
      File isolated=new File(base.getNoBackupFilesDir(),"qa-mlkit-"+UUID.randomUUID());isolated.mkdirs();
      ScanStore store=new ScanStore(new ContextWrapper(base){@Override public File getNoBackupFilesDir(){return isolated;}});
      GooglePageImport importer=new GooglePageImport(base,store);
      fixtures.add(fixture(base,1240,1754,1));fixtures.add(fixture(base,1754,1240,1));
      fixtures.add(fixture(base,1000,630,1));fixtures.add(fixture(base,1240,1754,6));
      List<Uri> uris=new ArrayList<>();for(File f:fixtures)uris.add(Uri.fromFile(f));
      List<Map<String,Object>> pages=importer.importPages(uris,20);
      t.check(pages.size()==4,"four pages imported transactionally");
      List<String> images=new ArrayList<>();
      for(int i=0;i<pages.size();i++){
        Map<String,Object> page=pages.get(i);images.add((String)page.get("image"));
        t.check(Arrays.equals(Files.readAllBytes(fixtures.get(i).toPath()),Files.readAllBytes(new File((String)page.get("source")).toPath())),"source JPEG "+i+" preserved byte for byte");
        int w=((Number)page.get("width")).intValue(),h=((Number)page.get("height")).intValue();
        int expectedW=i==0?1240:i==2?1000:1754,expectedH=i==0?1754:i==2?630:1240;
        t.check(w==expectedW&&h==expectedH,"orientation and geometry retained for page "+i);
        Bitmap actual=BitmapFactory.decodeFile((String)page.get("image"));
        Bitmap expected=BitmapFactory.decodeFile(fixtures.get(i).getPath());
        if(i==3){Matrix m=new Matrix();m.setRotate(90);Bitmap rotated=Bitmap.createBitmap(expected,0,0,expected.getWidth(),expected.getHeight(),m,true);expected.recycle();expected=rotated;}
        t.check(actual.sameAs(expected),"no second crop or lossy enhancement on page "+i);actual.recycle();expected.recycle();
      }
      t.check(((Number)pages.get(2).get("width")).intValue()==1000,"small card keeps its complete returned crop");
      File draftFolder=new File((String)pages.get(0).get("image")).getParentFile();
      int before=Objects.requireNonNull(draftFolder.list()).length;
      File bad=new File(base.getCacheDir(),"bad-mlkit-"+UUID.randomUUID()+".jpg");fixtures.add(bad);Files.write(bad.toPath(),new byte[]{1,2,3,4,5});
      t.rejects(()->importer.importPages(Arrays.asList(uris.get(0),Uri.fromFile(bad)),20),"corrupt second page rejects batch");
      t.check(Objects.requireNonNull(draftFolder.list()).length==before,"failed import rolls back only its new files");
      t.rejects(()->importer.importPages(Collections.emptyList(),20),"empty result rejected");
      t.rejects(()->importer.importPages(uris,1),"retake page limit enforced");
      t.rejects(()->importer.importPages(Collections.nCopies(21,uris.get(0)),20),"over 20 pages rejected");
      t.rejects(()->importer.importPages(Collections.singletonList(Uri.parse("https://example.invalid/page.jpg")),20),"network URI rejected without fetching");
      t.rejects(()->importer.importPages(Collections.singletonList(Uri.fromFile(new File("/sdcard/unknown.jpg"))),20),"unowned file URI rejected");
      String draft=new JSONArray(pages).toString();store.checkpoint(draft);t.check(store.recover().equals(draft),"Google pages survive draft checkpoint");
      Map<String,Object> doc=store.save("Uji ML Kit Adapter",images);String id=(String)doc.get("id");
      t.check(((Number)doc.get("pages")).intValue()==4,"own compressor creates valid four-page PDF");
      try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(store.document(id),ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer pdf=new PdfRenderer(fd)){
        for(int i=0;i<4;i++)try(PdfRenderer.Page p=pdf.openPage(i)){
          double expected=((Number)pages.get(i).get("width")).doubleValue()/((Number)pages.get(i).get("height")).doubleValue();
          t.check(Math.abs((double)p.getWidth()/p.getHeight()-expected)<0.01,"PDF aspect ratio retained for page "+i);
          Bitmap render=Bitmap.createBitmap(1200,1200,Bitmap.Config.ARGB_8888);render.eraseColor(Color.WHITE);p.render(render,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);render.recycle();
        }
      }
      t.check(store.list().size()==1,"own gallery indexes generated PDF");
      t.check(store.document(id).length()<2_000_000,"synthetic four-page PDF below 2 MB");
      File out=base.getExternalFilesDir("qa");out.mkdirs();t.export(store.document(id),new File(out,"mlkit-adapter.pdf"));
      report.put("pdfBytes",store.document(id).length());report.put("pages",pages);report.put("success",true);
      result.putString("stream",t.passed.size()+" ML Kit import checks PASS\n");
    }catch(Throwable e){
      try{report.put("success",false);report.put("failure",e.toString());}catch(Exception ignored){}
      result.putString("stream","FAIL after "+t.passed.size()+": "+e);
    }finally{
      for(File f:fixtures)f.delete();
      try{report.put("passed",t.passed);File out=new File(t.getTargetContext().getExternalFilesDir("qa"),"mlkit-import-checks.json");out.getParentFile().mkdirs();Files.write(out.toPath(),report.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));}catch(Exception ignored){}
    }
    t.finish(report.optBoolean("success")?Activity.RESULT_OK:Activity.RESULT_CANCELED,result);
  }
}
