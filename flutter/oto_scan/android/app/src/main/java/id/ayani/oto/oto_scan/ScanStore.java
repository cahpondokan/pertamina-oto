package id.ayani.oto.oto_scan;

import android.content.Context;
import android.graphics.*;
import android.graphics.pdf.*;
import android.media.ExifInterface;
import android.os.ParcelFileDescriptor;
import android.util.AtomicFile;
import org.json.*;
import java.io.*;
import java.nio.file.Files;
import java.util.*;

final class ScanStore {
  final Context context;
  final File root,drafts,documents;
  ScanStore(Context c) {
    context=c;root=new File(c.getNoBackupFilesDir(),"oto-scan");
    drafts=new File(root,"drafts");documents=new File(root,"documents");
    drafts.mkdirs();documents.mkdirs();
  }
  static Map<String,Object> map(Object... pairs){
    Map<String,Object> m=new HashMap<>();for(int i=0;i<pairs.length;i+=2)m.put((String)pairs[i],pairs[i+1]);return m;
  }
  File owned(String path) throws IOException {
    File f=new File(path).getCanonicalFile();
    if(!f.getParentFile().equals(drafts.getCanonicalFile())||!f.isFile())throw new IOException("Foto sesi tidak ditemukan.");
    return f;
  }
  File document(String id) throws IOException {
    if(id==null||!id.matches("[a-f0-9-]{36}"))throw new IOException("Dokumen tidak ditemukan.");
    File f=new File(documents,id+".pdf");if(!f.isFile())throw new IOException("Dokumen tidak ditemukan.");return f;
  }
  void writeAtomic(File target,byte[] bytes) throws IOException {
    AtomicFile a=new AtomicFile(target);FileOutputStream out=null;
    try{out=a.startWrite();out.write(bytes);a.finishWrite(out);}
    catch(IOException e){if(out!=null)a.failWrite(out);throw e;}
  }
  void jpeg(Bitmap b,File path) throws IOException {
    AtomicFile a=new AtomicFile(path);FileOutputStream out=null;
    try{out=a.startWrite();if(!b.compress(Bitmap.CompressFormat.JPEG,98,out))throw new IOException();a.finishWrite(out);}
    catch(IOException e){if(out!=null)a.failWrite(out);throw e;}
  }
  void png(Bitmap b,File path) throws IOException {
    AtomicFile a=new AtomicFile(path);FileOutputStream out=null;
    try{out=a.startWrite();if(!b.compress(Bitmap.CompressFormat.PNG,100,out))throw new IOException();a.finishWrite(out);}
    catch(IOException e){if(out!=null)a.failWrite(out);throw e;}
  }
  static long pixelBudget(){return ScanQuality.pixelBudget(Runtime.getRuntime().maxMemory());}
  Bitmap decode(byte[] bytes) throws IOException {
    BitmapFactory.Options o=new BitmapFactory.Options();o.inJustDecodeBounds=true;BitmapFactory.decodeByteArray(bytes,0,bytes.length,o);
    if(o.outWidth<2||o.outHeight<2||(long)o.outWidth*o.outHeight>60_000_000)throw new IOException("Ukuran foto tidak didukung.");
    o.inSampleSize=ScanQuality.sampleSize(o.outWidth,o.outHeight,pixelBudget());
    o.inJustDecodeBounds=false;o.inPreferredConfig=Bitmap.Config.ARGB_8888;
    Bitmap source=BitmapFactory.decodeByteArray(bytes,0,bytes.length,o);if(source==null)throw new IOException("Foto tidak terbaca.");
    ExifInterface exif=new ExifInterface(new ByteArrayInputStream(bytes));
    int rotation=exif.getAttributeInt(ExifInterface.TAG_ORIENTATION,1);Matrix m=new Matrix();
    switch(rotation){
      case 2:m.setScale(-1,1);break;case 3:m.setRotate(180);break;case 4:m.setScale(1,-1);break;
      case 5:m.setRotate(90);m.postScale(-1,1);break;case 6:m.setRotate(90);break;
      case 7:m.setRotate(-90);m.postScale(-1,1);break;case 8:m.setRotate(-90);break;
    }
    Bitmap oriented=Bitmap.createBitmap(source,0,0,source.getWidth(),source.getHeight(),m,true);
    if(oriented!=source)source.recycle();return oriented;
  }
  static double[] detect(Bitmap b){
    return detect(b,null);
  }
  static double[] detect(Bitmap b,DetectionTarget target){
    // Segment a bounded raster, then read only narrow edge neighbourhoods from
    // the original bitmap. No full 12MP int[] copy alongside two full bitmaps.
    double scale=Math.min(1,480.0/Math.max(b.getWidth(),b.getHeight()));
    Bitmap small=Bitmap.createScaledBitmap(b,Math.max(8,(int)Math.round(b.getWidth()*scale)),Math.max(8,(int)Math.round(b.getHeight()*scale)),true);
    double[] q;
    try{int w=small.getWidth(),h=small.getHeight();int[] pixels=new int[w*h];small.getPixels(pixels,0,w,0,0,w,h);q=DocumentDetector.detectOutline(pixels,w,h,target);}
    finally{if(small!=b)small.recycle();}
    return q==null?null:DocumentDetector.refine(b::getPixel,b.getWidth(),b.getHeight(),q);
  }
  static double[] detectPreview(Bitmap b){
    int w=b.getWidth(),h=b.getHeight();int[] pixels=new int[w*h];b.getPixels(pixels,0,w,0,0,w,h);
    return DocumentDetector.detectPreview(pixels,w,h);
  }
  static void normalize(Bitmap b) throws IOException {
    int w=b.getWidth(),h=b.getHeight();
    AdaptiveDocument.normalize(w,h,(y,row)->b.getPixels(row,0,w,0,y,w,1),(y,row)->b.setPixels(row,0,w,0,y,w,1));
  }
  static void enhance(Bitmap b) throws IOException {
    int w=b.getWidth(),h=b.getHeight();
    AdaptiveDocument.enhance(w,h,(y,row)->b.getPixels(row,0,w,0,y,w,1),(y,row)->b.setPixels(row,0,w,0,y,w,1));
  }
  static List<List<Double>> list(double[] q){
    List<List<Double>> result=new ArrayList<>();for(int i=0;i<4;i++)result.add(Arrays.asList(q[i*2],q[i*2+1]));return result;
  }
  static double[] quad(Object raw){
    List<?> list=(List<?>)raw;if(list.size()!=4)throw new IllegalArgumentException();
    double[] q=new double[8];for(int i=0;i<4;i++){List<?> p=(List<?>)list.get(i);q[2*i]=((Number)p.get(0)).doubleValue();q[2*i+1]=((Number)p.get(1)).doubleValue();}
    if(!DocumentDetector.valid(q))throw new IllegalArgumentException("Sudut potong tidak valid.");return q;
  }
  Map<String,Object> capture(byte[] bytes) throws Exception {
    return capture(bytes,null,1,1);
  }
  Map<String,Object> capture(byte[] bytes,DetectionTarget target,int vw,int vh) throws Exception {
    return capture(bytes,target,vw,vh,0);
  }
  Map<String,Object> capture(byte[] bytes,DetectionTarget target,int vw,int vh,int turns) throws Exception {
    if(bytes.length>30_000_000)throw new IOException("Foto terlalu besar.");
    Bitmap b=decode(bytes);try{return process(b,target==null?null:target.toOrientedImage(b.getWidth(),b.getHeight(),vw,vh,turns),bytes);}finally{b.recycle();}
  }
  Map<String,Object> process(Bitmap b) throws Exception {
    return process(b,null);
  }
  Map<String,Object> process(Bitmap b,DetectionTarget target) throws Exception { return process(b,target,null); }
  Map<String,Object> process(Bitmap b,DetectionTarget target,byte[] original) throws Exception {
    if(root.getUsableSpace()<100_000_000)throw new IOException("Ruang penyimpanan HP tidak cukup.");
    File source=new File(drafts,UUID.randomUUID()+"-source.jpg");if(original!=null)writeAtomic(source,original);else jpeg(b,source);
    double[] q=detect(b,target);boolean detected=q!=null;
    boolean proposed=!detected&&target!=null&&target.previous!=null;
    if(q==null)q=proposed?target.previous.clone():new double[]{.04,.04,.96,.04,.96,.96,.04,.96};
    // Uncertain detection keeps the full photo until the user confirms a crop.
    Bitmap output=detected?rectify(b,q):b;
    try {
      int width=output.getWidth(),height=output.getHeight();
      final Bitmap measured=output;
      boolean blurred=AdaptiveDocument.blurred(width,height,(y,row)->measured.getPixels(row,0,width,0,y,width,1));
      if(detected)enhance(output);
      File image=new File(drafts,UUID.randomUUID()+"-page.png");png(output,image);
      return map("source",source.getPath(),"image",image.getPath(),"width",output.getWidth(),"height",output.getHeight(),
        "corners",list(q),"detected",detected,"proposed",proposed,"aspect","free","turns",0,"filter","clear","qualityWarning",blurred?"Tulisan tampak kurang tajam. Tahan HP lalu ambil ulang bila perlu.":null);
    } finally {if(output!=b)output.recycle();}
  }
  static Bitmap rectify(Bitmap source,double[] q) throws IOException {
    return rectify(source,q,"free");
  }
  static Bitmap rectify(Bitmap source,double[] q,String aspect) throws IOException {
    return rectify(source,q,aspect,0);
  }
  static Bitmap rectify(Bitmap source,double[] q,String aspect,int turns) throws IOException {
    if(!DocumentDetector.valid(q))throw new IOException("Empat sudut harus membentuk kotak.");
    float[] src=new float[8];for(int i=0;i<4;i++){src[i*2]=(float)(q[i*2]*(source.getWidth()-1));src[i*2+1]=(float)(q[i*2+1]*(source.getHeight()-1));}
    int[] dimensions=PageGeometry.cropSize(Math.max(distance(src,0,1),distance(src,3,2)),Math.max(distance(src,0,3),distance(src,1,2)),aspect,turns);
    int w=dimensions[0],h=dimensions[1];
    if(w<32||h<32)throw new IOException("Area potong terlalu kecil.");
    // Perspective can enlarge the output rectangle beyond the source area.
    // Bound memory by reducing both dimensions together, without stretching.
    double scale=Math.min(1,Math.sqrt(pixelBudget()/(double)((long)w*h)));
    w=Math.max(32,(int)(w*scale));h=Math.max(32,(int)(h*scale));
    Matrix matrix=new Matrix();if(!matrix.setPolyToPoly(src,0,new float[]{0,0,w-1,0,w-1,h-1,0,h-1},0,4))throw new IOException("Perspektif tidak valid.");
    Bitmap target=Bitmap.createBitmap(w,h,Bitmap.Config.ARGB_8888);Canvas c=new Canvas(target);c.drawColor(Color.WHITE);
    c.drawBitmap(source,matrix,new Paint(Paint.ANTI_ALIAS_FLAG|Paint.FILTER_BITMAP_FLAG));return target;
  }
  static double distance(float[] p,int a,int b){return Math.hypot(p[a*2]-p[b*2],p[a*2+1]-p[b*2+1]);}
  Map<String,Object> edit(Map<?,?> args) throws Exception {
    File source=owned((String)args.get("source"));double[] q=quad(args.get("corners"));
    int turns=((Number)args.get("turns")).intValue();if(turns<0||turns>3)throw new IOException();
    String filter=(String)args.get("filter");if(!Arrays.asList("clear","color","gray","bw").contains(filter))throw new IOException();
    String aspect=args.get("aspect")==null?"free":(String)args.get("aspect");if(!PageGeometry.validMode(aspect))throw new IOException("Rasio tidak valid.");
    Bitmap original=decode(Files.readAllBytes(source.toPath()));
    Bitmap cropped;try{cropped=rectify(original,q,aspect,turns);}finally{original.recycle();}
    try{
      if(turns!=0){Matrix m=new Matrix();m.setRotate(turns*90);Bitmap rotated=Bitmap.createBitmap(cropped,0,0,cropped.getWidth(),cropped.getHeight(),m,true);if(rotated!=cropped)cropped.recycle();cropped=rotated;}
      if(filter.equals("clear")){
        if(!cropped.isMutable()){Bitmap mutable=cropped.copy(Bitmap.Config.ARGB_8888,true);cropped.recycle();cropped=mutable;}
        enhance(cropped);
      }else if(!filter.equals("color")){
        if(!cropped.isMutable()){Bitmap mutable=cropped.copy(Bitmap.Config.ARGB_8888,true);cropped.recycle();cropped=mutable;}
        int w=cropped.getWidth(),h=cropped.getHeight();final Bitmap selected=cropped;
        if(filter.equals("bw")){
          normalize(cropped);
          AdaptiveDocument.binarize(w,h,(y,row)->selected.getPixels(row,0,w,0,y,w,1),(y,row)->selected.setPixels(row,0,w,0,y,w,1));
        }else{
          int[] row=new int[w];for(int y=0;y<h;y++){cropped.getPixels(row,0,w,0,y,w,1);for(int x=0;x<w;x++){int v=ScanQuality.luminance(row[x]);row[x]=Color.rgb(v,v,v);}cropped.setPixels(row,0,w,0,y,w,1);}
        }
      }
      File image=new File(drafts,UUID.randomUUID()+"-page.png");png(cropped,image);
      return map("source",source.getPath(),"image",image.getPath(),"width",cropped.getWidth(),"height",cropped.getHeight(),"corners",list(q),"detected",args.get("detected"),"proposed",false,"aspect",aspect,"turns",turns,"filter",filter,"qualityWarning",args.get("qualityWarning"),"orientationWarning",args.get("orientationWarning"));
    }finally{cropped.recycle();}
  }
  void checkpoint(String text) throws Exception {
    if(text.length()>100_000)throw new IOException();
    JSONArray pages=new JSONArray(text);if(pages.length()>20)throw new IOException();
    Set<String> retained=new HashSet<>();
    for(int i=0;i<pages.length();i++){
      JSONObject p=pages.getJSONObject(i);
      retained.add(owned(p.getString("source")).getName());retained.add(owned(p.getString("image")).getName());
    }
    writeAtomic(new File(root,"draft.json"),text.getBytes(java.nio.charset.StandardCharsets.UTF_8));
    File[] files=drafts.listFiles();if(files!=null)for(File f:files)if(f.isFile()&&!retained.contains(f.getName()))f.delete();
  }
  String recover() throws Exception {
    DocumentBatch.recover(documents);
    AtomicFile file=new AtomicFile(new File(root,"draft.json"));
    if(!file.getBaseFile().exists()&&!new File(root,"draft.json.bak").exists())return "[]";
    String text=new String(file.readFully(),java.nio.charset.StandardCharsets.UTF_8);JSONArray pages=new JSONArray(text);
    for(int i=0;i<pages.length();i++){JSONObject p=pages.getJSONObject(i);owned(p.getString("source"));owned(p.getString("image"));}
    return text;
  }
  Map<String,Object> save(String name,List<?> paths) throws Exception {
    if(paths.isEmpty()||paths.size()>20)throw new IOException("Isi dokumen harus 1–20 halaman.");
    if(root.getUsableSpace()<100_000_000)throw new IOException("Ruang penyimpanan HP tidak cukup.");
    String id=UUID.randomUUID().toString();File stage=new File(documents,id+".part"),target=new File(documents,id+".pdf");
    String safe=name.replaceAll("[\\p{Cntrl}/\\\\]"," ").trim();if(safe.isEmpty()||safe.length()>80)throw new IOException("Nama dokumen tidak valid.");
    try{
      try(FileOutputStream output=new FileOutputStream(stage)){
        CompactPdf pdf=new CompactPdf(output,paths.size());
        for(Object path:paths){
          File f=owned((String)path);Bitmap source=BitmapFactory.decodeFile(f.getPath());
          if(source==null)throw new IOException("Halaman tidak terbaca.");
          Bitmap b=source;
          try{
            if((long)source.getWidth()*source.getHeight()>12_000_000){
              int[] bounded=CompactPdf.dimensions(source.getWidth(),source.getHeight());
              b=Bitmap.createScaledBitmap(source,bounded[0],bounded[1],true);source.recycle();source=b;
            }
            final Bitmap original=source;int sw=source.getWidth(),sh=source.getHeight();
            byte[] mono=CompactPdf.monochrome(sw,sh,(y,row)->original.getPixels(row,0,sw,0,y,sw,1));
            if(mono!=null){pdf.page(sw,sh,mono,true);continue;}
            int[] size=CompactPdf.dimensions(source.getWidth(),source.getHeight());
            // Keep real captured detail up to the export bound. Never upscale.
            // Draft originals are released only after the durable save checkpoint.
            if(size[0]!=source.getWidth()||size[1]!=source.getHeight()){
              b=Bitmap.createScaledBitmap(source,size[0],size[1],true);source.recycle();
            }
            ByteArrayOutputStream jpeg=new ByteArrayOutputStream();
            if(!b.compress(Bitmap.CompressFormat.JPEG,CompactPdf.JPEG_QUALITY,jpeg))throw new IOException("Kompresi halaman gagal.");
            byte[] fallback=jpeg.toByteArray();
            final Bitmap export=b;int ew=b.getWidth(),eh=b.getHeight();
            AdaptiveDocument.Rows rows=(y,row)->export.getPixels(row,0,ew,0,y,ew,1);
            if(DocumentCompression.paperLike(ew,eh,rows)){
              byte[] lossless=DocumentCompression.lossless(ew,eh,rows);
              if(lossless.length<=fallback.length){pdf.documentPage(ew,eh,lossless,null,null);continue;}
              Bitmap background=Bitmap.createBitmap(ew,eh,Bitmap.Config.ARGB_8888);
              try{
                DocumentCompression.Layers layers=DocumentCompression.split(ew,eh,rows,(y,row)->background.setPixels(row,0,ew,0,y,ew,1));
                ByteArrayOutputStream base=new ByteArrayOutputStream();
                if(!background.compress(Bitmap.CompressFormat.JPEG,CompactPdf.JPEG_QUALITY,base))throw new IOException("Kompresi latar gagal.");
                byte[] bg=base.toByteArray();
                if((long)layers.foreground.length+layers.alpha.length+bg.length+600<fallback.length){pdf.documentPage(ew,eh,layers.foreground,bg,layers.alpha);continue;}
              }finally{background.recycle();}
            }
            pdf.page(ew,eh,fallback,false);
          }finally{if(b!=source)b.recycle();if(!source.isRecycled())source.recycle();}
        }
        pdf.finish();output.getFD().sync();
      }
      try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(stage,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer reader=new PdfRenderer(fd)){
        if(reader.getPageCount()!=paths.size())throw new IOException("Validasi PDF gagal.");
        for(int i=0;i<reader.getPageCount();i++){
          try(PdfRenderer.Page p=reader.openPage(i)){Bitmap sample=Bitmap.createBitmap(120,160,Bitmap.Config.ARGB_8888);try{p.render(sample,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);}finally{sample.recycle();}}
        }
      }
      JSONObject meta=new JSONObject();meta.put("name",safe+".pdf");meta.put("pages",paths.size());meta.put("created",System.currentTimeMillis());
      writeAtomic(new File(documents,id+".json"),meta.toString().getBytes(java.nio.charset.StandardCharsets.UTF_8));
      Files.move(stage.toPath(),target.toPath()); // no replacement; same-filesystem publication
      return info(id,false,true);
    }finally{if(stage.exists())stage.delete();}
  }
  Map<String,Object> info(String id,boolean preview) throws Exception {
    return info(id,preview,false);
  }
  Map<String,Object> info(String id,boolean preview,boolean metadataOnly) throws Exception {
    File f=document(id);String name="Scan.pdf";int pages=0;long created=f.lastModified();
    File meta=new File(documents,id+".json");
    if(meta.isFile()){
      JSONObject data=new JSONObject(new String(new AtomicFile(meta).readFully(),java.nio.charset.StandardCharsets.UTF_8));
      name=data.optString("name",name);pages=data.optInt("pages",0);created=data.optLong("created",created);
    }
    // The library reads lightweight saved metadata. Opening still validates PDF.
    if(!metadataOnly||pages<1||pages>20){
      try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(f,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer reader=new PdfRenderer(fd)){pages=reader.getPageCount();}
    }
    Map<String,Object> m=map("id",id,"name",name,"pages",pages,"bytes",f.length(),"created",created);
    if(preview)m.put("preview",render(id,0));return m;
  }
  List<Map<String,Object>> list() throws Exception {
    DocumentBatch.recover(documents);
    File[] files=documents.listFiles((dir,name)->name.endsWith(".pdf"));List<Map<String,Object>> list=new ArrayList<>();
    if(files==null)return list;Arrays.sort(files,Comparator.comparingLong(File::lastModified).reversed());
    for(File f:files){try{list.add(info(f.getName().replace(".pdf",""),false,true));}catch(Exception ignored){}}
    return list;
  }
  Map<String,Object> rename(String id,String name) throws Exception {
    Map<String,Object> current=info(id,false,true);
    String safe=name==null?"":name.replaceAll("[\\p{Cntrl}/\\\\]"," ").trim();
    if(safe.toLowerCase(Locale.ROOT).endsWith(".pdf"))safe=safe.substring(0,safe.length()-4).trim();
    if(safe.isEmpty()||safe.length()>80)throw new IOException("Nama dokumen tidak valid.");
    JSONObject meta=new JSONObject();meta.put("name",safe+".pdf");meta.put("pages",current.get("pages"));meta.put("created",current.get("created"));
    writeAtomic(new File(documents,id+".json"),meta.toString().getBytes(java.nio.charset.StandardCharsets.UTF_8));
    current.put("name",safe+".pdf");return current;
  }
  int deleteDocuments(List<?> input) throws Exception {
    List<String> ids=DocumentBatch.ids(input);int count=DocumentBatch.delete(documents,ids);
    File[] cached=context.getCacheDir().listFiles();
    if(cached!=null)for(File file:cached)for(String id:ids){
      if(file.getName().equals("gallery-v1-"+id+".jpg")||file.getName().matches("preview-"+id+"-[0-9]+(-detail)?\\.(png|jpg)")){file.delete();break;}
    }
    return count;
  }
  String thumbnail(String id,boolean refresh) throws Exception {
    File source=document(id);
    File cached=new File(context.getCacheDir(),"gallery-v1-"+id+".jpg");
    if(!refresh&&cached.isFile()&&cached.length()>0&&cached.lastModified()>=source.lastModified())return cached.getPath();
    try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(source,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer reader=new PdfRenderer(fd);PdfRenderer.Page page=reader.openPage(0)){
      double scale=600.0/Math.max(page.getWidth(),page.getHeight());
      Bitmap bitmap=Bitmap.createBitmap(Math.max(1,(int)(page.getWidth()*scale)),Math.max(1,(int)(page.getHeight()*scale)),Bitmap.Config.ARGB_8888);
      try{bitmap.eraseColor(Color.WHITE);page.render(bitmap,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);jpeg(bitmap,cached);return cached.getPath();}
      finally{bitmap.recycle();}
    }
  }
  String render(String id,int index) throws Exception {return render(id,index,false);}
  String render(String id,int index,boolean detail) throws Exception {
    File f=document(id);
    try(ParcelFileDescriptor fd=ParcelFileDescriptor.open(f,ParcelFileDescriptor.MODE_READ_ONLY);PdfRenderer reader=new PdfRenderer(fd)){
      if(index<0||index>=reader.getPageCount())throw new IOException();
      try(PdfRenderer.Page p=reader.openPage(index)){
        // A separate 8MP reader raster preserves text detail when zooming. The
        // small result thumbnail must never overwrite this detailed cache.
        double scale=detail?Math.min(3800.0/Math.max(p.getWidth(),p.getHeight()),Math.sqrt(8_000_000.0/(p.getWidth()*(double)p.getHeight()))):Math.min(1400.0/p.getWidth(),1800.0/p.getHeight());
        Bitmap b=Bitmap.createBitmap(Math.max(1,(int)(p.getWidth()*scale)),Math.max(1,(int)(p.getHeight()*scale)),Bitmap.Config.ARGB_8888);
        try{b.eraseColor(Color.WHITE);p.render(b,null,null,PdfRenderer.Page.RENDER_MODE_FOR_DISPLAY);
          File file=new File(context.getCacheDir(),"preview-"+id+"-"+index+(detail?"-detail.png":".jpg"));if(detail)png(b,file);else jpeg(b,file);return file.getPath();}
        finally{b.recycle();}
      }
    }
  }
  Map<String,Object> synthetic() throws Exception {
    Bitmap b=Bitmap.createBitmap(900,1200,Bitmap.Config.ARGB_8888);Canvas c=new Canvas(b);c.drawColor(Color.rgb(52,57,61));
    Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);p.setColor(Color.WHITE);
    Path paper=new Path();paper.moveTo(155,95);paper.lineTo(780,145);paper.lineTo(820,1080);paper.lineTo(90,1115);paper.close();c.drawPath(paper,p);
    p.setColor(Color.rgb(15,40,62));p.setTextSize(38);p.setFakeBoldText(true);c.drawText("DOKUMEN UJI",250,245,p);
    p.setTextSize(21);p.setFakeBoldText(false);c.drawText("Fixture sintetis - bukan foto kamera",200,300,p);
    for(int y=370;y<1000;y+=65)c.drawLine(200,y,720,y,p);
    try{return process(b);}finally{b.recycle();}
  }
}
