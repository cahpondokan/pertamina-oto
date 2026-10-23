package id.ayani.oto.oto_scan;

import android.app.*;
import android.content.*;
import android.graphics.*;
import android.os.*;
import android.view.*;
import android.view.accessibility.AccessibilityNodeInfo;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.util.*;
import org.json.*;

/** Same-signature test APK only. Never packaged in the store app. */
final class ReleaseEmulatorChecks {
  final ScanInstrumentation test;
  final ArrayList<String> passed=new ArrayList<>();
  MainActivity activity; File out; ScanStore store;
  ReleaseEmulatorChecks(ScanInstrumentation test){this.test=test;}
  void check(boolean ok,String label){if(!ok)throw new AssertionError(label);passed.add(label);}
  String label(AccessibilityNodeInfo n){return String.valueOf(n.getText())+" "+String.valueOf(n.getContentDescription())+" "+String.valueOf(n.getTooltipText());}
  AccessibilityNodeInfo find(AccessibilityNodeInfo n,String value,boolean edit){
    if(n==null)return null;
    if(n.isVisibleToUser()&&(edit?n.isEditable():label(n).contains(value)))return n;
    for(int i=0;i<n.getChildCount();i++){AccessibilityNodeInfo result=find(n.getChild(i),value,edit);if(result!=null)return result;}
    return null;
  }
  AccessibilityNodeInfo exact(AccessibilityNodeInfo n,String value){
    if(n==null)return null;
    if(n.isVisibleToUser()&&(value.contentEquals(String.valueOf(n.getText()))||value.contentEquals(String.valueOf(n.getContentDescription()))||value.contentEquals(String.valueOf(n.getTooltipText()))))return n;
    for(int i=0;i<n.getChildCount();i++){AccessibilityNodeInfo hit=exact(n.getChild(i),value);if(hit!=null)return hit;}
    return null;
  }
  AccessibilityNodeInfo waitNode(String value,boolean edit)throws Exception{
    long end=SystemClock.uptimeMillis()+18000;
    while(SystemClock.uptimeMillis()<end){
      AccessibilityNodeInfo root=test.getUiAutomation().getRootInActiveWindow();
      AccessibilityNodeInfo n=edit?find(root,value,true):exact(root,value);
      if(n==null&&!edit)n=find(root,value,false);
      if(n!=null)return n;SystemClock.sleep(150);
    }
    throw new AssertionError("UI not found: "+value);
  }
  void tap(String value)throws Exception{
    AccessibilityNodeInfo node=waitNode(value,false);Rect bounds=new Rect();node.getBoundsInScreen(bounds);
    if(!node.performAction(AccessibilityNodeInfo.ACTION_CLICK)){
      long t=SystemClock.uptimeMillis();
      for(int action:new int[]{MotionEvent.ACTION_DOWN,MotionEvent.ACTION_UP}){
        MotionEvent e=MotionEvent.obtain(t,SystemClock.uptimeMillis(),action,bounds.centerX(),bounds.centerY(),0);
        e.setSource(InputDevice.SOURCE_TOUCHSCREEN);test.getUiAutomation().injectInputEvent(e,true);e.recycle();
      }
    }
    SystemClock.sleep(450);
  }
  void type(String value)throws Exception{
    Bundle args=new Bundle();args.putCharSequence(AccessibilityNodeInfo.ACTION_ARGUMENT_SET_TEXT_CHARSEQUENCE,value);
    check(waitNode("text field",true).performAction(AccessibilityNodeInfo.ACTION_SET_TEXT,args),"text entered: "+value);
    SystemClock.sleep(500);
  }
  void back(){test.sendKeyDownUpSync(KeyEvent.KEYCODE_BACK);SystemClock.sleep(500);}
  void waitShare(String packageName)throws Exception{
    long end=SystemClock.uptimeMillis()+18000;
    while(SystemClock.uptimeMillis()<end){
      AccessibilityNodeInfo root=test.getUiAutomation().getRootInActiveWindow();
      if(root!=null&&!packageName.contentEquals(root.getPackageName())&&find(root,"Sharing",false)!=null)return;
      SystemClock.sleep(150);
    }
    throw new AssertionError("Android share chooser did not become ready");
  }
  void pinch(){
    long start=SystemClock.uptimeMillis();
    MotionEvent.PointerProperties[] props=new MotionEvent.PointerProperties[2];
    MotionEvent.PointerCoords[] coords=new MotionEvent.PointerCoords[2];
    for(int i=0;i<2;i++){props[i]=new MotionEvent.PointerProperties();props[i].id=i;props[i].toolType=MotionEvent.TOOL_TYPE_FINGER;coords[i]=new MotionEvent.PointerCoords();coords[i].pressure=1;coords[i].size=1;}
    for(int step=0;step<24;step++){
      float spread=80+Math.max(0,Math.min(20,step-1))*12;
      for(int i=0;i<2;i++){int side=i==0?-1:1;coords[i].x=540+side*spread;coords[i].y=950+side*spread*.45f;}
      int action=step==0?MotionEvent.ACTION_DOWN:step==1?MotionEvent.ACTION_POINTER_DOWN|(1<<8):step==22?MotionEvent.ACTION_POINTER_UP|(1<<8):step==23?MotionEvent.ACTION_UP:MotionEvent.ACTION_MOVE;
      int count=step==0||step==23?1:2;
      MotionEvent e=MotionEvent.obtain(start,SystemClock.uptimeMillis(),action,count,props,coords,0,0,1,1,0,0,InputDevice.SOURCE_TOUCHSCREEN,0);
      test.getUiAutomation().injectInputEvent(e,true);e.recycle();SystemClock.sleep(24);
    }
  }
  void dump(AccessibilityNodeInfo n,StringBuilder b,int depth){
    if(n==null)return;b.append(" ".repeat(depth)).append(label(n)).append(" [").append(n.getClassName()).append("]\n");
    for(int i=0;i<n.getChildCount();i++)dump(n.getChild(i),b,depth+1);
  }
  void screenshot(String name)throws Exception{
    SystemClock.sleep(700);
    Bitmap image=test.getUiAutomation().takeScreenshot();if(image==null)throw new IOException("Screenshot unavailable");
    try(OutputStream stream=new FileOutputStream(new File(out,name+".png"))){image.compress(Bitmap.CompressFormat.PNG,100,stream);}finally{image.recycle();}
    StringBuilder tree=new StringBuilder();dump(test.getUiAutomation().getRootInActiveWindow(),tree,0);
    Files.write(new File(out,name+".txt").toPath(),tree.toString().getBytes(StandardCharsets.UTF_8));
  }
  Map<String,Object> fixture(boolean landscape,boolean small)throws Exception{
    int w=landscape?1280:900,h=landscape?900:1200;
    Bitmap b=Bitmap.createBitmap(w,h,Bitmap.Config.ARGB_8888);Canvas c=new Canvas(b);c.drawColor(Color.rgb(52,57,61));
    RectF paper=small?new RectF(255,440,645,685):new RectF(70,70,w-70,h-70);
    Paint p=new Paint(Paint.ANTI_ALIAS_FLAG);p.setColor(Color.WHITE);c.drawRect(paper,p);
    p.setColor(Color.rgb(9,36,60));p.setTypeface(Typeface.create("sans-serif",Typeface.BOLD));p.setTextSize(small?23:36);
    c.drawText(small?"KARTU CONTOH":"CATATAN RAPAT",paper.left+28,paper.top+60,p);
    p.setTypeface(Typeface.create("sans-serif",Typeface.NORMAL));p.setTextSize(small?17:24);
    c.drawText("Dokumen sintetis untuk pengujian",paper.left+28,paper.top+100,p);
    if(!small){
      c.drawText("Tanggal: 22 Oktober 2026",paper.left+28,paper.top+150,p);
      for(int i=0;i<7;i++){float y=paper.top+220+i*54;if(y>paper.bottom-30)break;c.drawText((i+1)+". Periksa dokumen, simpan PDF, lalu bagikan.",paper.left+28,y,p);}
    }
    double[] q={paper.left/w,paper.top/h,paper.right/w,paper.top/h,paper.right/w,paper.bottom/h,paper.left/w,paper.bottom/h};
    try{return store.process(b,small?new DetectionTarget(new double[]{.5,.5},q):null);}finally{b.recycle();}
  }
  void run(){
    Bundle result=new Bundle();JSONObject report=new JSONObject();
    try{
      check(Build.HARDWARE.contains("ranchu")||Build.HARDWARE.contains("goldfish"),"emulator hardware guard");
      Context base=test.getTargetContext();out=new File(base.getExternalFilesDir("qa"),"play-release");out.mkdirs();store=new ScanStore(base);
      check(store.list().isEmpty(),"fresh emulator app data, no user documents");
      Map<String,Object> portrait=fixture(false,false),landscape=fixture(true,false),card=fixture(false,true);
      check(Boolean.TRUE.equals(portrait.get("detected")),"portrait automatically cropped");
      check(Boolean.TRUE.equals(landscape.get("detected")),"landscape automatically cropped");
      check(((Number)landscape.get("width")).intValue()>((Number)landscape.get("height")).intValue(),"landscape remains landscape");
      check(Boolean.TRUE.equals(card.get("detected"))&&((Number)card.get("width")).intValue()<450,"small card guide produces actual crop");
      store.save("Kartu contoh",Collections.singletonList((String)card.get("image")));
      store.save("Catatan landscape",Collections.singletonList((String)landscape.get("image")));
      store.checkpoint(new JSONArray(Arrays.asList(portrait,landscape)).toString());
      Intent intent=new Intent().setComponent(new ComponentName(base,MainActivity.class)).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
      activity=(MainActivity)test.startActivitySync(intent);
      test.runOnMainSync(()->activity.getWindow().clearFlags(WindowManager.LayoutParams.FLAG_SECURE));
      tap("Lanjutkan scan");waitNode("Simpan PDF",false);screenshot("01-pages");
      tap("Simpan PDF");type("Catatan rapat");tap("Simpan");
      waitNode("Kirim",false);SystemClock.sleep(2000);screenshot("02-reader");
      check(new JSONArray(store.recover()).length()==0,"save clears scan draft");
      check(store.list().size()==3,"save adds one PDF to gallery");
      for(Map<String,Object> d:store.list())if(d.get("name").equals("Catatan rapat.pdf")){
        check(((Number)d.get("pages")).intValue()==2,"saved mixed-orientation PDF has two pages");
        test.export(store.document((String)d.get("id")),new File(out,"mixed-orientation.pdf"));
      }
      pinch();waitNode("Pas lebar layar",false);screenshot("02b-zoom");
      check(true,"two-finger zoom works in emulator");tap("Pas lebar layar");
      tap("Kirim");
      waitShare(base.getPackageName());
      AccessibilityNodeInfo share=test.getUiAutomation().getRootInActiveWindow();
      check(share!=null&&!base.getPackageName().contentEquals(share.getPackageName()),"share opens Android chooser");
      screenshot("03-share");back();waitNode("Kirim",false);back();
      waitNode("Dokumen Saya",false);screenshot("04-gallery");
      check(find(test.getUiAutomation().getRootInActiveWindow(),"Simpan PDF",false)==null,"back after save lands on gallery");
      type("landscape");waitNode("Catatan landscape",false);screenshot("05-search");
      tap("Catatan landscape");waitNode("Kirim",false);screenshot("06-landscape");back();
      waitNode("Dokumen Saya",false);
      if(find(test.getUiAutomation().getRootInActiveWindow(),"Hapus pencarian",false)!=null)tap("Hapus pencarian");
      tap("Pilih");tap("Catatan rapat");tap("Catatan landscape");
      waitNode("Kirim (2)",false);screenshot("07-multi-select");tap("Kirim (2)");
      waitShare(base.getPackageName());
      share=test.getUiAutomation().getRootInActiveWindow();check(share!=null&&!base.getPackageName().contentEquals(share.getPackageName()),"multi-document share opens Android chooser");back();
      tap("Selesai");tap("Privasi & tentang");waitNode("Kebijakan privasi",false);screenshot("08-privacy");
      check(true,"offline privacy page available");back();
      back();waitNode("OTO Scan",false);
      long cameraDeadline=SystemClock.uptimeMillis()+20000;
      boolean[] preview={false};
      while(SystemClock.uptimeMillis()<cameraDeadline){test.runOnMainSync(()->preview[0]=activity.cameraView!=null&&activity.cameraView.session!=null);if(preview[0])break;SystemClock.sleep(200);}
      check(preview[0],"emulator virtual camera preview starts");
      check(new JSONArray(store.recover()).length()==0,"new scan starts with an empty draft");
      screenshot("09-virtual-camera");
      report.put("success",true);result.putString("stream",passed.size()+" emulator release checks PASS\n");
    }catch(Throwable e){
      try{report.put("success",false);report.put("failure",e.toString());if(out!=null)screenshot("failure");}catch(Exception ignored){}
      result.putString("stream","FAIL after "+passed.size()+": "+e);
    }finally{
      if(activity!=null)test.runOnMainSync(()->activity.getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE));
      try{report.put("passed",passed);report.put("count",passed.size());if(out!=null)Files.write(new File(out,"checks.json").toPath(),report.toString(2).getBytes(StandardCharsets.UTF_8));}catch(Exception ignored){}
    }
    test.finish(report.optBoolean("success")?Activity.RESULT_OK:Activity.RESULT_CANCELED,result);
  }
}
