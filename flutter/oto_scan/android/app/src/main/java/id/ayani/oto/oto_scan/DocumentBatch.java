package id.ayani.oto.oto_scan;

import java.io.*;
import java.nio.file.Files;
import java.util.*;

/** Validated library IDs and recoverable, same-directory batch deletion. */
final class DocumentBatch {
  static List<String> ids(List<?> input) throws IOException {
    if(input==null||input.isEmpty()||input.size()>100)throw new IOException("Pilih 1–100 dokumen.");
    LinkedHashSet<String> unique=new LinkedHashSet<>();
    for(Object value:input){
      if(!(value instanceof String)||!((String)value).matches("[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}"))throw new IOException("Pilihan dokumen tidak valid.");
      unique.add((String)value);
    }
    return new ArrayList<>(unique);
  }
  interface Move {void run(File from,File to) throws IOException;}
  static void move(File from,File to) throws IOException {Files.move(from.toPath(),to.toPath());}
  static int delete(File documents,List<?> input) throws IOException {return delete(documents,input,DocumentBatch::move);}
  static int delete(File documents,List<?> input,Move mover) throws IOException {
    recover(documents);
    List<String> ids=ids(input);
    // Validate the whole selection before moving a single file.
    for(String id:ids)if(!new File(documents,id+".pdf").isFile())throw new IOException("Dokumen tidak ditemukan. Muat ulang galeri.");
    File stage=new File(documents,".delete-"+UUID.randomUUID());
    if(!stage.mkdir())throw new IOException("Belum berhasil menghapus dokumen.");
    try{
      for(String id:ids)for(String ext:new String[]{".pdf",".json",".json.bak"}){
        File source=new File(documents,id+ext);
        if(source.isFile())mover.run(source,new File(stage,source.getName()));
      }
      try(FileOutputStream marker=new FileOutputStream(new File(stage,"COMMITTED"))){marker.write(1);marker.getFD().sync();}
    }catch(IOException e){restore(documents,stage);throw new IOException("Penghapusan belum berhasil. Dokumen dipulihkan.",e);}
    clean(stage);return ids.size();
  }
  static void recover(File documents) throws IOException {
    File[] stages=documents.listFiles(f->f.isDirectory()&&f.getName().matches("\\.delete-[a-f0-9-]{36}"));
    if(stages==null)return;
    for(File stage:stages){if(new File(stage,"COMMITTED").isFile())clean(stage);else restore(documents,stage);}
  }
  private static boolean owned(File f){return f.isFile()&&f.getName().matches("[a-f0-9-]{36}\\.(pdf|json|json\\.bak)");}
  private static void restore(File documents,File stage) throws IOException {
    File[] files=stage.listFiles();if(files==null)throw new IOException("Pemulihan dokumen belum berhasil.");
    for(File file:files)if(owned(file))move(file,new File(documents,file.getName()));
    if(!stage.delete())throw new IOException("Pemulihan dokumen belum selesai.");
  }
  private static void clean(File stage){
    File[] files=stage.listFiles();if(files==null)return;
    boolean clean=true;for(File f:files)if(owned(f)){if(!f.delete())clean=false;}
    if(clean){new File(stage,"COMMITTED").delete();stage.delete();}
  }
}
