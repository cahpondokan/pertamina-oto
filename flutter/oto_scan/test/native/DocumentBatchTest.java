package id.ayani.oto.oto_scan;
import java.io.*;
import java.nio.file.*;
import java.util.*;

public final class DocumentBatchTest {
  static int checks;
  static String a="00000000-0000-0000-0000-000000000001",b="00000000-0000-0000-0000-000000000002",c="00000000-0000-0000-0000-000000000003";
  static void check(boolean v,String name){if(!v)throw new AssertionError(name);checks++;}
  interface Throwing{void run()throws Exception;}
  static void rejects(Throwing task)throws Exception{try{task.run();throw new AssertionError("accepted invalid operation");}catch(IOException expected){checks++;}}
  static void fixture(File dir,String id)throws Exception{Files.writeString(new File(dir,id+".pdf").toPath(),"synthetic PDF "+id);Files.writeString(new File(dir,id+".json").toPath(),"synthetic metadata "+id);}
  public static void main(String[] args)throws Exception{
    File root=Files.createTempDirectory("oto-batch-fixture-").toFile();
    try{
      check(DocumentBatch.ids(Arrays.asList(a,b,a)).equals(Arrays.asList(a,b)),"dedupe preserves order");
      rejects(()->DocumentBatch.ids(null));rejects(()->DocumentBatch.ids(Collections.emptyList()));
      rejects(()->DocumentBatch.ids(Arrays.asList(a,"../outside")));
      rejects(()->DocumentBatch.ids(Arrays.asList(a,3)));
      rejects(()->DocumentBatch.ids(Collections.nCopies(101,a)));
      fixture(root,a);fixture(root,b);fixture(root,c);
      rejects(()->DocumentBatch.delete(root,Arrays.asList(a,"00000000-0000-0000-0000-000000000099")));
      check(new File(root,a+".pdf").isFile(),"missing second document leaves first intact");
      final int[] moves={0};
      rejects(()->DocumentBatch.delete(root,Arrays.asList(a,b),(from,to)->{if(++moves[0]==3)throw new IOException("injected failure");DocumentBatch.move(from,to);}));
      check(Files.readString(new File(root,a+".pdf").toPath()).equals("synthetic PDF "+a),"failed batch restores exact PDF bytes");
      check(new File(root,a+".json").isFile()&&new File(root,b+".pdf").isFile(),"metadata and second document preserved");
      File stage=new File(root,".delete-"+UUID.randomUUID());stage.mkdir();
      DocumentBatch.move(new File(root,a+".pdf"),new File(stage,a+".pdf"));
      DocumentBatch.recover(root);
      check(new File(root,a+".pdf").isFile()&&!stage.exists(),"process interruption without marker recovers");
      check(DocumentBatch.delete(root,Arrays.asList(a,b,a))==2,"delete exactly unique selection");
      check(!new File(root,a+".pdf").exists()&&!new File(root,b+".json").exists(),"selected files removed");
      check(new File(root,c+".pdf").isFile(),"unselected document unchanged");
      stage=new File(root,".delete-"+UUID.randomUUID());stage.mkdir();
      DocumentBatch.move(new File(root,c+".pdf"),new File(stage,c+".pdf"));new File(stage,"COMMITTED").createNewFile();
      DocumentBatch.recover(root);
      check(!new File(root,c+".pdf").exists()&&!stage.exists(),"committed cleanup never resurrects deleted PDF");
      System.out.println("PASS "+checks+" document selection/delete checks (local synthetic files only)");
    }finally{try(var files=Files.walk(root.toPath())){files.sorted(Comparator.reverseOrder()).forEach(path->{try{Files.delete(path);}catch(IOException e){throw new UncheckedIOException(e);}});}}
  }
}
