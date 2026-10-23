package id.ayani.oto.oto_scan;
import android.app.*;
import android.content.*;
import android.graphics.*;
import android.os.*;
import java.io.*;
import java.nio.file.*;
import java.util.*;
import org.json.*;

public final class ScanInstrumentation extends Instrumentation {
  Bundle options;
  final ArrayList<String> passed=new ArrayList<>();
  void check(boolean condition,String label){if(!condition)throw new AssertionError(label);passed.add(label);}
  interface Checked {void run() throws Exception;}
  void export(File from,File to) throws IOException {
    // Files.copy preserves private source mode 0600 on Android's emulated
    // external storage; create QA copies explicitly without altering originals.
    if(to.exists()&&!to.delete())throw new IOException("Cannot replace QA copy");
    try(InputStream in=new FileInputStream(from);OutputStream out=new FileOutputStream(to)){in.transferTo(out);}
  }
  void rejects(Checked action,String label) throws Exception {
    boolean rejected=false;try{action.run();}catch(Exception e){rejected=true;}check(rejected,label);
  }
  @Override public void onCreate(Bundle args){super.onCreate(args);options=args;start();}
  @Override public void onStart(){
    if(options!=null&&options.getString("mode","").equals("release-ui")){
      new ReleaseEmulatorChecks(this).run();return;
    }
    if(options!=null&&!options.getString("mode","native").equals("native")){inspect();return;}
    Bundle result=new Bundle();JSONObject report=new JSONObject();
    try{
      Context base=getTargetContext();
      File isolated=new File(base.getNoBackupFilesDir(),"qa-native-"+UUID.randomUUID());isolated.mkdirs();
      ScanStore store=new ScanStore(new ContextWrapper(base){@Override public File getNoBackupFilesDir(){return isolated;}});
      Map<String,Object> p=store.synthetic();
      check(Boolean.TRUE.equals(p.get("detected")),"synthetic perspective automatically detected");
      check((int)p.get("width")<900&&(int)p.get("height")<1200,"output excludes full camera frame");
      Bitmap cropped=BitmapFactory.decodeFile((String)p.get("image"));
      int[] pixels=new int[cropped.getWidth()*cropped.getHeight()];
      cropped.getPixels(pixels,0,cropped.getWidth(),0,0,cropped.getWidth(),cropped.getHeight());
      int desk=0;for(int pixel:pixels)if(Math.abs(Color.red(pixel)-52)<12&&Math.abs(Color.green(pixel)-57)<12&&Math.abs(Color.blue(pixel)-61)<12)desk++;
      check(desk<pixels.length*.002,"rectified image desk pixels below 0.2 percent");
      check(Color.red(cropped.getPixel(10,10))>220,"top left corner is paper");
      cropped.recycle();
      String checkpoint=new JSONArray(Collections.singletonList(p)).toString();store.checkpoint(checkpoint);
      ScanStore reopened=new ScanStore(new ContextWrapper(base){@Override public File getNoBackupFilesDir(){return isolated;}});
      check(reopened.recover().equals(checkpoint),"draft restored from disk");
      Map<String,Object> edit=new HashMap<>(p);edit.put("turns",1);edit.put("filter","gray");
      Map<String,Object> gray=store.edit(edit);
      check((int)gray.get("width")==((Number)p.get("height")).intValue()&&(int)gray.get("height")==((Number)p.get("width")).intValue(),"rotation changes actual output dimensions");
      Bitmap gb=BitmapFactory.decodeFile((String)gray.get("image"));int g=gb.getPixel(gb.getWidth()/2,gb.getHeight()/2);gb.recycle();
      check(Math.abs(Color.red(g)-Color.green(g))<=2,"grayscale applied");
      edit.put("filter","bw");Map<String,Object> bw=store.edit(edit);
      check(new File((String)bw.get("image")).isFile(),"black white after rotation succeeds");
      List<String> images=Arrays.asList((String)p.get("image"),(String)gray.get("image"),(String)bw.get("image"));
      Map<String,Object> doc=store.save("Uji Flutter",images);String id=(String)doc.get("id");
      check((int)doc.get("pages")==3,"PDF validates three pages");
      check(store.document(id).length()>1000,"PDF has nonempty content");
      for(int i=0;i<3;i++)check(new File(store.render(id,i)).isFile(),"PDF page "+i+" renders");
      check(store.list().size()==1,"saved PDF listed");
      check(reopened.list().size()==1,"saved PDF visible after store restart");
      byte[] before=Files.readAllBytes(store.document(id).toPath());
      Map<String,Object> second=store.save("Uji Flutter",Collections.singletonList((String)p.get("image")));
      check(!id.equals(second.get("id")),"duplicate name uses unique document ID");
      check(Arrays.equals(before,Files.readAllBytes(store.document(id).toPath())),"duplicate save never overwrites first PDF");
      rejects(()->store.owned("/sdcard/unknown.jpg"),"external source rejected");
      rejects(()->store.document("../draft.json"),"path traversal rejected");
      rejects(()->store.render(id,3),"out of range PDF page rejected");
      rejects(()->store.save("",images),"empty document name rejected");
      rejects(()->store.save("Empty",Collections.emptyList()),"empty PDF rejected");
      rejects(()->store.save("Oversize",Collections.nCopies(21,(String)p.get("image"))),"over 20 pages rejected");
      edit.put("corners",Arrays.asList(Arrays.asList(0,0),Arrays.asList(1,1),Arrays.asList(1,0),Arrays.asList(0,1)));
      rejects(()->store.edit(edit),"crossed manual crop rejected");
      File export=new File(base.getExternalFilesDir("qa"),"native-synthetic.pdf");export.getParentFile().mkdirs();export(store.document(id),export);
      report.put("passed",passed);report.put("count",passed.size());report.put("success",true);report.put("deskPixels",desk);report.put("pixels",pixels.length);
      result.putString("stream",passed.size()+" native checks PASS\n");
    }catch(Throwable e){
      try{report.put("passed",passed);report.put("success",false);report.put("failure",e.getClass().getSimpleName()+": "+String.valueOf(e.getMessage()));}catch(Exception ignored){}
      result.putString("stream","FAIL after "+passed.size()+": "+e.getClass().getSimpleName()+" "+e.getMessage());
    }finally{
      try{File out=new File(getTargetContext().getExternalFilesDir("qa"),"native-checks.json");out.getParentFile().mkdirs();Files.write(out.toPath(),report.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));}catch(Exception ignored){}
    }
    finish(report.optBoolean("success")?Activity.RESULT_OK:Activity.RESULT_CANCELED,result);
  }
  // Same-signature instrumentation only. No test export route in the release app.
  void inspect(){
    Bundle result=new Bundle();MainActivity activity=null;
    try{
      Context base=getTargetContext();File out=base.getExternalFilesDir("qa");out.mkdirs();
      String mode=options.getString("mode");
      if(mode.equals("preview")){
        Intent intent=new Intent().setComponent(new ComponentName(base,MainActivity.class)).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        activity=(MainActivity)startActivitySync(intent);final MainActivity a=activity;
        for(int i=0;i<75;i++){if(a.cameraView!=null&&a.cameraView.session!=null)break;SystemClock.sleep(200);}
        SystemClock.sleep(1200);
        Bitmap[] shot=new Bitmap[1];JSONObject info=new JSONObject();
        runOnMainSync(()->{
          CameraView c=a.cameraView;if(c==null||c.session==null)return;
          shot[0]=c.snapshot();
          try{info.put("sensorRotation",c.rotation);info.put("viewRotation",c.texture.getRotation());
            info.put("bufferWidth",c.previewSize.getWidth());info.put("bufferHeight",c.previewSize.getHeight());
            info.put("viewWidth",c.container.getWidth());info.put("viewHeight",c.container.getHeight());
            float[] values=new float[9];c.texture.getTransform(new Matrix()).getValues(values);info.put("transform",new JSONArray(values));
          }catch(Exception e){throw new RuntimeException(e);}
        });
        if(shot[0]==null)throw new IOException("Camera preview unavailable");
        try(OutputStream f=new FileOutputStream(new File(out,"preview.png"))){shot[0].compress(Bitmap.CompressFormat.PNG,100,f);}shot[0].recycle();
        Files.write(new File(out,"preview-geometry.json").toPath(),info.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));
        runOnMainSync(()->a.getWindow().clearFlags(android.view.WindowManager.LayoutParams.FLAG_SECURE));
        SystemClock.sleep(500);Bitmap screen=getUiAutomation().takeScreenshot();
        if(screen!=null){try(OutputStream f=new FileOutputStream(new File(out,"preview-screen.png"))){screen.compress(Bitmap.CompressFormat.PNG,100,f);}screen.recycle();}
      }else if(mode.equals("inspect")){
        ScanStore store=new ScanStore(base);JSONArray draft=new JSONArray(store.recover());
        if(draft.length()>0){JSONObject p=draft.getJSONObject(draft.length()-1);
          export(store.owned(p.getString("source")),new File(out,"camera-source.jpg"));
          export(store.owned(p.getString("image")),new File(out,"camera-crop.jpg"));
          Files.write(new File(out,"camera-page.json").toPath(),p.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));
        }else{
          File folder=new File(base.getNoBackupFilesDir(),"oto-scan/drafts");
          File[] sources=folder.listFiles((dir,name)->name.endsWith("-source.jpg"));
          if(sources!=null&&sources.length>0){Arrays.sort(sources,Comparator.comparingLong(File::lastModified).reversed());
            export(sources[0],new File(out,"camera-source.jpg"));
          }
        }
        for(Map<String,Object> d:store.list())if(String.valueOf(d.get("name")).startsWith("Uji Flutter Kamera")){
          export(store.document((String)d.get("id")),new File(out,"camera-saved.pdf"));break;
        }
      }else throw new IllegalArgumentException("Unknown test mode");
      result.putString("stream",mode+" evidence saved\n");
    }catch(Throwable e){result.putString("stream","FAIL "+e.getClass().getSimpleName()+": "+e.getMessage());}
    finally{if(activity!=null){final MainActivity a=activity;runOnMainSync(()->a.getWindow().addFlags(android.view.WindowManager.LayoutParams.FLAG_SECURE));}}
    finish(result.getString("stream","").startsWith("FAIL")?Activity.RESULT_CANCELED:Activity.RESULT_OK,result);
  }
}
