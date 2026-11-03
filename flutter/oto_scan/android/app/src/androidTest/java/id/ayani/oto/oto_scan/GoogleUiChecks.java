package id.ayani.oto.oto_scan;
import android.app.*;
import android.content.*;
import android.net.Uri;
import android.os.*;
import android.view.WindowManager;
import java.io.*;
import java.nio.file.Files;
import java.util.*;
import org.json.*;

/** Exercise OTO UI after importing synthetic SDK-shaped results. */
final class GoogleUiChecks {
  final ScanInstrumentation t;
  GoogleUiChecks(ScanInstrumentation t){this.t=t;}
  void run(){
    ReleaseEmulatorChecks ui=new ReleaseEmulatorChecks(t);Bundle result=new Bundle();JSONObject report=new JSONObject();
    List<File> inputs=new ArrayList<>();
    try{
      ui.check(Build.HARDWARE.contains("ranchu")||Build.HARDWARE.contains("goldfish"),"emulator guard");
      Context c=t.getTargetContext();ui.out=new File(c.getExternalFilesDir("qa"),"mlkit-ui");ui.out.mkdirs();
      ui.store=new ScanStore(c);ui.check(ui.store.list().isEmpty()&&new JSONArray(ui.store.recover()).length()==0,"fresh isolated trial app data");
      GoogleImportChecks fixtures=new GoogleImportChecks(t);
      inputs.add(fixtures.fixture(c,1240,1754,1));inputs.add(fixtures.fixture(c,1754,1240,1));
      List<Uri> uris=new ArrayList<>();for(File f:inputs)uris.add(Uri.fromFile(f));
      List<Map<String,Object>> pages=new GooglePageImport(c,ui.store).importPages(uris,20);
      ui.store.checkpoint(new JSONArray(pages).toString());
      ui.activity=(MainActivity)t.startActivitySync(new Intent().setComponent(new ComponentName(c,MainActivity.class)).addFlags(Intent.FLAG_ACTIVITY_NEW_TASK));
      t.runOnMainSync(()->ui.activity.getWindow().clearFlags(WindowManager.LayoutParams.FLAG_SECURE));
      ui.tap("Lanjutkan scan");ui.waitNode("Simpan PDF",false);ui.screenshot("01-pages");
      ui.tap("Simpan PDF");ui.type("Uji MLKit Potret Landscape");ui.tap("Simpan");
      ui.waitNode("Kirim",false);ui.screenshot("02-reader");
      ui.check(new JSONArray(ui.store.recover()).length()==0,"save clears Google scan draft");
      ui.check(ui.store.list().size()==1,"saved document appears in own gallery");
      Map<String,Object> doc=ui.store.list().get(0);ui.check(((Number)doc.get("pages")).intValue()==2,"saved mixed-orientation PDF has two pages");
      t.export(ui.store.document((String)doc.get("id")),new File(ui.out,"mixed.pdf"));
      ui.pinch();ui.waitNode("Pas lebar layar",false);ui.screenshot("03-zoom");ui.check(true,"existing PDF reader zoom works");ui.tap("Pas lebar layar");
      ui.tap("Kirim");ui.waitShare(c.getPackageName());ui.screenshot("04-share");ui.check(true,"share opens chooser without sending");ui.back();ui.waitNode("Kirim",false);ui.back();
      ui.waitNode("Dokumen Saya",false);ui.screenshot("05-gallery");
      ui.check(ui.find(t.getUiAutomation().getRootInActiveWindow(),"Simpan PDF",false)==null,"back after save returns to gallery");
      ui.type("Landscape");ui.waitNode("Uji MLKit Potret Landscape",false);ui.screenshot("06-search");
      ui.tap("Uji MLKit Potret Landscape");ui.waitNode("Kirim",false);ui.check(true,"gallery search opens PDF directly");ui.back();
      ui.tap("Privasi & tentang");ui.waitNode("Kebijakan privasi",false);ui.screenshot("07-privacy");ui.check(true,"trial privacy available offline");ui.back();
      report.put("success",true);result.putString("stream",ui.passed.size()+" ML Kit downstream UI checks PASS\n");
    }catch(Throwable e){try{report.put("success",false);report.put("failure",e.toString());if(ui.out!=null)ui.screenshot("failure");}catch(Exception ignored){}result.putString("stream","FAIL after "+ui.passed.size()+": "+e);}
    finally{
      for(File f:inputs)f.delete();
      if(ui.activity!=null)t.runOnMainSync(()->ui.activity.getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE));
      try{report.put("passed",ui.passed);if(ui.out!=null)Files.write(new File(ui.out,"checks.json").toPath(),report.toString(2).getBytes(java.nio.charset.StandardCharsets.UTF_8));}catch(Exception ignored){}
    }
    t.finish(report.optBoolean("success")?Activity.RESULT_OK:Activity.RESULT_CANCELED,result);
  }
}
