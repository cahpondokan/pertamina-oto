# App entry points are retained by Android manifest / Flutter embedding.
# No broad keep or warning suppression is required.
# Preserve only the first-party adapter members exercised by the separately
# signed Android instrumentation. No SDK/library-wide keep rules.
-keepclassmembers class id.ayani.oto.oto_scan.CameraView {
    android.graphics.Bitmap snapshot();
    <fields>;
}
-keepclassmembers class id.ayani.oto.oto_scan.ScanStore {
    <init>(android.content.Context);
    java.util.Map synthetic();
    java.util.Map process(android.graphics.Bitmap,id.ayani.oto.oto_scan.DetectionTarget);
    java.lang.String recover();
    void checkpoint(java.lang.String);
    java.util.Map edit(java.util.Map);
    java.util.Map save(java.lang.String,java.util.List);
    java.io.File document(java.lang.String);
    java.io.File owned(java.lang.String);
    java.lang.String render(java.lang.String,int);
    java.util.List list();
}
-keepclassmembers class id.ayani.oto.oto_scan.DetectionTarget {
    <init>(double[],double[]);
}
-keepclassmembers class id.ayani.oto.oto_scan.GooglePageImport {
    <init>(android.content.Context,id.ayani.oto.oto_scan.ScanStore);
    java.util.List importPages(java.util.List,int);
}
