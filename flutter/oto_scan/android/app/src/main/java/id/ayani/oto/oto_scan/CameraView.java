package id.ayani.oto.oto_scan;

import android.Manifest;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.graphics.*;
import android.hardware.camera2.*;
import android.media.Image;
import android.media.ImageReader;
import android.os.*;
import android.util.Size;
import android.view.*;
import android.widget.FrameLayout;
import io.flutter.plugin.common.MethodChannel;
import io.flutter.plugin.platform.PlatformView;
import java.nio.ByteBuffer;
import java.util.*;

final class CameraView implements PlatformView,TextureView.SurfaceTextureListener {
  final MainActivity activity;
  final FrameLayout container;
  final TextureView texture;
  final OrientationEventListener orientationListener;
  volatile int deviceTurns=-1;
  volatile boolean orientationAvailable=false;
  final HandlerThread thread=new HandlerThread("oto-camera");
  final Handler handler;
  CameraDevice camera;CameraCaptureSession session;CaptureRequest.Builder preview;
  ImageReader reader,analysisReader;Surface surface;Size previewSize;int rotation=90;
  volatile int analysisWidth,analysisHeight;
  volatile boolean analysisEnabled=true,analysisFailed=false,cameraFailed=false;
  static final class AnalysisSample {
    final PreviewPixels.Frame frame;final long time;
    AnalysisSample(PreviewPixels.Frame frame,long time){this.frame=frame;this.time=time;}
  }
  volatile AnalysisSample latest;
  long lastAnalysis;
  volatile boolean closed=false;volatile MethodChannel.Result pending;
  int afMode=CaptureRequest.CONTROL_AF_MODE_OFF,lastAf=-1,lastAe=-1;
  final CaptureReadiness readiness=new CaptureReadiness();
  long captureStarted;boolean uncertainFocus;
  Rect sensorBounds;int maxAfRegions,maxAeRegions,focusEpoch;
  android.hardware.camera2.params.MeteringRectangle[] focusRegion;
  DetectionTarget captureTarget;int captureViewWidth,captureViewHeight,captureTurns;
  CaptureSelection captureSelection;boolean captureOrientationKnown,matchingGeometry=true;
  boolean highQualityEdges=false,minimalNoise=false,shotSent=false;
  final CameraCaptureSession.CaptureCallback focusCallback=new CameraCaptureSession.CaptureCallback(){
    @Override public void onCaptureCompleted(CameraCaptureSession s,CaptureRequest r,TotalCaptureResult data){
      Integer state=data.get(CaptureResult.CONTROL_AF_STATE);if(state!=null)lastAf=state;
      Integer ae=data.get(CaptureResult.CONTROL_AE_STATE);if(ae!=null)lastAe=ae;
      maybeTakeStill();
    }
  };
  CameraView(MainActivity a) {
    activity=a;container=new FrameLayout(a);texture=new TextureView(a);
    orientationListener=new OrientationEventListener(a){
      @Override public void onOrientationChanged(int degrees){deviceTurns=ScanGeometry.deviceTurns(degrees,deviceTurns);orientationAvailable=degrees>=0;}
    };
    if(orientationListener.canDetectOrientation())orientationListener.enable();
    container.setBackgroundColor(Color.BLACK);container.setClipChildren(true);
    container.addView(texture,new FrameLayout.LayoutParams(-1,-1));
    thread.start();handler=new Handler(thread.getLooper());texture.setSurfaceTextureListener(this);
    container.addOnLayoutChangeListener((v,l,t,r,b,ol,ot,or,ob)->layout());
  }
  @Override public View getView(){return container;}
  @Override public void onSurfaceTextureAvailable(SurfaceTexture t,int w,int h){open();}
  @Override public void onSurfaceTextureSizeChanged(SurfaceTexture t,int w,int h){layout();}
  @Override public boolean onSurfaceTextureDestroyed(SurfaceTexture t){close();return true;}
  @Override public void onSurfaceTextureUpdated(SurfaceTexture t){}
  @SuppressWarnings("MissingPermission")
  void open(){
    if(closed||activity.checkSelfPermission(Manifest.permission.CAMERA)!=PackageManager.PERMISSION_GRANTED)return;
    try{
      CameraManager manager=(CameraManager)activity.getSystemService(Context.CAMERA_SERVICE);String id=null;
      for(String candidate:manager.getCameraIdList()){
        CameraCharacteristics c=manager.getCameraCharacteristics(candidate);
        if(Objects.equals(c.get(CameraCharacteristics.LENS_FACING),CameraCharacteristics.LENS_FACING_BACK)){id=candidate;break;}
      }
      if(id==null)throw new IllegalStateException();
      CameraCharacteristics c=manager.getCameraCharacteristics(id);
      rotation=c.get(CameraCharacteristics.SENSOR_ORIENTATION);
      sensorBounds=c.get(CameraCharacteristics.SENSOR_INFO_ACTIVE_ARRAY_SIZE);
      Integer afRegions=c.get(CameraCharacteristics.CONTROL_MAX_REGIONS_AF),aeRegions=c.get(CameraCharacteristics.CONTROL_MAX_REGIONS_AE);
      maxAfRegions=afRegions==null?0:afRegions;maxAeRegions=aeRegions==null?0:aeRegions;
      int[] focus=c.get(CameraCharacteristics.CONTROL_AF_AVAILABLE_MODES);
      afMode=has(focus,CaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE)?CaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE:has(focus,CaptureRequest.CONTROL_AF_MODE_AUTO)?CaptureRequest.CONTROL_AF_MODE_AUTO:CaptureRequest.CONTROL_AF_MODE_OFF;
      highQualityEdges=has(c.get(CameraCharacteristics.EDGE_AVAILABLE_EDGE_MODES),CaptureRequest.EDGE_MODE_HIGH_QUALITY);
      minimalNoise=has(c.get(CameraCharacteristics.NOISE_REDUCTION_AVAILABLE_NOISE_REDUCTION_MODES),CaptureRequest.NOISE_REDUCTION_MODE_MINIMAL);
      android.hardware.camera2.params.StreamConfigurationMap sizes=c.get(CameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
      // Keep the sensor's 4:3 field at up to 12MP, bounded by application memory.
      Size photo=chooseMatching(sizes.getOutputSizes(ImageFormat.JPEG),(long)(ScanStore.pixelBudget()*1.03),new Size(4,3));
      previewSize=chooseMatching(sizes.getOutputSizes(SurfaceTexture.class),1_400_000,photo);
      texture.getSurfaceTexture().setDefaultBufferSize(previewSize.getWidth(),previewSize.getHeight());layout();
      surface=new Surface(texture.getSurfaceTexture());
      reader=ImageReader.newInstance(photo.getWidth(),photo.getHeight(),ImageFormat.JPEG,2);
      reader.setOnImageAvailableListener(r->{
        try(Image image=r.acquireLatestImage()){
          if(image==null)return;ByteBuffer buffer=image.getPlanes()[0].getBuffer();byte[] bytes=new byte[buffer.remaining()];buffer.get(bytes);
          final DetectionTarget selected=captureTarget;final int vw=captureViewWidth,vh=captureViewHeight,turns=captureTurns;
          final boolean uncertain=uncertainFocus,known=captureOrientationKnown;
          activity.runOnUiThread(()->{MethodChannel.Result result=pending;pending=null;if(result!=null)activity.processPhoto(bytes,result,selected,vw,vh,uncertain,turns,known);});
        }catch(Exception e){failCapture();}
      },handler);
      Size analysis=chooseMatching(sizes.getOutputSizes(ImageFormat.YUV_420_888),640L*480,photo);
      matchingGeometry=sameRatio(photo,previewSize)&&sameRatio(photo,analysis);
      // Do not advertise a guide in a different field of view. Still detection
      // remains available when a device cannot supply matching streams.
      if(!matchingGeometry){analysisEnabled=false;analysisFailed=true;}
      analysisReader=ImageReader.newInstance(analysis.getWidth(),analysis.getHeight(),ImageFormat.YUV_420_888,2);
      analysisReader.setOnImageAvailableListener(this::analysisImage,handler);
      manager.openCamera(id,new CameraDevice.StateCallback(){
        @Override public void onOpened(CameraDevice device){if(closed){device.close();return;}camera=device;configure();}
        @Override public void onDisconnected(CameraDevice device){device.close();camera=null;cameraFailed=true;failCapture();}
        @Override public void onError(CameraDevice device,int error){device.close();camera=null;cameraFailed=true;failCapture();}
      },handler);
    }catch(Exception e){cameraFailed=true;failCapture();}
  }
  static Size choose(Size[] sizes,long limit){
    Size best=sizes[0];long bestArea=0;
    for(Size s:sizes){long area=(long)s.getWidth()*s.getHeight();if(area<=limit&&area>bestArea){best=s;bestArea=area;}else if(bestArea==0&&area<(long)best.getWidth()*best.getHeight())best=s;}
    return best;
  }
  static boolean has(int[] values,int value){if(values!=null)for(int v:values)if(v==value)return true;return false;}
  static boolean sameRatio(Size a,Size b){return Math.abs(a.getWidth()/(double)a.getHeight()-b.getWidth()/(double)b.getHeight())<.01;}
  static Size chooseMatching(Size[] sizes,long limit,Size photo){
    Size best=null;long area=0;
    for(Size s:sizes){long n=(long)s.getWidth()*s.getHeight();
      if(n<=limit&&n>area&&Math.abs(s.getWidth()/(double)s.getHeight()-photo.getWidth()/(double)photo.getHeight())<.01){best=s;area=n;}}
    return best!=null?best:choose(sizes,limit);
  }
  void layout(){
    if(previewSize==null||container.getWidth()==0||container.getHeight()==0)return;
    if(analysisWidth!=container.getWidth()||analysisHeight!=container.getHeight())latest=null;
    analysisWidth=container.getWidth();analysisHeight=container.getHeight();
    // TextureView already applies SENSOR_ORIENTATION. Rotate neither the View nor
    // its snapshot again. Correct only its non-uniform default stretch, keeping
    // the child inside Flutter's platform-view bounds (portrait activity).
    boolean swapped=rotation%180!=0;
    double rotatedW=swapped?previewSize.getHeight():previewSize.getWidth(),rotatedH=swapped?previewSize.getWidth():previewSize.getHeight();
    double scale=Math.max(container.getWidth()/rotatedW,container.getHeight()/rotatedH);
    Matrix transform=new Matrix();
    transform.setScale((float)(rotatedW*scale/container.getWidth()),(float)(rotatedH*scale/container.getHeight()),container.getWidth()/2f,container.getHeight()/2f);
    texture.setTransform(transform);
  }
  @SuppressWarnings("deprecation")
  void configure(){
    try{
      List<Surface> outputs=new ArrayList<>(Arrays.asList(surface,reader.getSurface()));
      if(analysisEnabled)outputs.add(analysisReader.getSurface());
      camera.createCaptureSession(outputs,new CameraCaptureSession.StateCallback(){
        @Override public void onConfigured(CameraCaptureSession s){
          if(closed){s.close();return;}session=s;
          try{preview=camera.createCaptureRequest(CameraDevice.TEMPLATE_PREVIEW);preview.addTarget(surface);
            if(analysisEnabled)preview.addTarget(analysisReader.getSurface());
            preview.set(CaptureRequest.CONTROL_AF_MODE,afMode);
            preview.set(CaptureRequest.CONTROL_AE_MODE,CaptureRequest.CONTROL_AE_MODE_ON);session.setRepeatingRequest(preview.build(),focusCallback,handler);}
          catch(Exception e){cameraFailed=true;failCapture();}
        }
        @Override public void onConfigureFailed(CameraCaptureSession s){
          s.close();if(closed)return;
          if(analysisEnabled){analysisEnabled=false;analysisFailed=true;configure();}
          else{cameraFailed=true;failCapture();}
        }
      },handler);
    }catch(Exception e){
      if(!closed&&analysisEnabled){analysisEnabled=false;analysisFailed=true;configure();}
      else{cameraFailed=true;failCapture();}
    }
  }
  void analysisImage(ImageReader input){
    try(Image image=input.acquireLatestImage()){
      if(image==null||closed||!analysisEnabled)return;
      long now=SystemClock.elapsedRealtime();int vw=analysisWidth,vh=analysisHeight;
      if(now-lastAnalysis<200||vw<1||vh<1)return;
      lastAnalysis=now;Rect crop=image.getCropRect();Image.Plane[] p=image.getPlanes();
      if(crop.left!=0||crop.top!=0||crop.width()!=image.getWidth()||crop.height()!=image.getHeight()){
        matchingGeometry=false;analysisFailed=true;latest=null;return;
      }
      PreviewPixels.Frame frame=PreviewPixels.convert(
        new PreviewPixels.Plane(p[0].getBuffer(),p[0].getRowStride(),p[0].getPixelStride()),
        new PreviewPixels.Plane(p[1].getBuffer(),p[1].getRowStride(),p[1].getPixelStride()),
        new PreviewPixels.Plane(p[2].getBuffer(),p[2].getRowStride(),p[2].getPixelStride()),
        crop.left,crop.top,crop.width(),crop.height(),rotation,vw,vh);
      if(!closed){readiness.observe(frame.pixels,frame.width,frame.height,now);latest=new AnalysisSample(frame,now);analysisFailed=false;maybeTakeStill();}
    }catch(Exception e){analysisFailed=true;latest=null;}
  }
  AnalysisSample analysisSample(){
    AnalysisSample value=latest;
    return !closed&&value!=null&&SystemClock.elapsedRealtime()-value.time<1500?value:null;
  }
  String analysisStatus(){return closed||cameraFailed||analysisFailed||!analysisEnabled?"unavailable":"waiting";}
  void torch(boolean enabled) throws Exception {
    if(session==null||preview==null)throw new IllegalStateException("Kamera belum siap.");
    preview.set(CaptureRequest.FLASH_MODE,enabled?CaptureRequest.FLASH_MODE_TORCH:CaptureRequest.FLASH_MODE_OFF);
    session.setRepeatingRequest(preview.build(),focusCallback,handler);
  }
  void capture(MethodChannel.Result result){
    capture(result,null);
  }
  void capture(MethodChannel.Result result,DetectionTarget target){
    capture(result,target,0);
  }
  void capture(MethodChannel.Result result,DetectionTarget target,long frame){
    if(closed||session==null||camera==null||pending!=null){result.error("camera","Kamera belum siap. Coba lagi.",null);return;}
    captureViewWidth=analysisWidth;captureViewHeight=analysisHeight;
    captureSelection=new CaptureSelection(target,frame,SystemClock.elapsedRealtime(),captureViewWidth,captureViewHeight,matchingGeometry);
    // Suppress callbacks from the previous preview until this request is armed.
    shotSent=true;pending=result;
    handler.post(()->{
      captureStarted=SystemClock.elapsedRealtime();uncertainFocus=false;shotSent=false;
      try{
        double x=.5,y=.5;
        if(target!=null&&target.point!=null){x=target.point[0];y=target.point[1];}
        else if(target!=null&&target.previous!=null){x=0;y=0;for(int i=0;i<4;i++){x+=target.previous[2*i]/4;y+=target.previous[2*i+1]/4;}}
        if(sensorBounds!=null){
          int[] r=ScanGeometry.metering(x,y,analysisWidth,analysisHeight,previewSize.getWidth(),previewSize.getHeight(),rotation,sensorBounds.left,sensorBounds.top,sensorBounds.width(),sensorBounds.height());
          focusRegion=new android.hardware.camera2.params.MeteringRectangle[]{new android.hardware.camera2.params.MeteringRectangle(new Rect(r[0],r[1],r[2],r[3]),900)};
          applyFocusRegion(preview);
        }
        if(afMode!=CaptureRequest.CONTROL_AF_MODE_OFF){
          lastAf=-1;
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_CANCEL);session.capture(preview.build(),null,handler);
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_START);session.capture(preview.build(),focusCallback,handler);
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_IDLE);
        }
        session.setRepeatingRequest(preview.build(),focusCallback,handler);
      }catch(Exception ignored){}
      handler.postDelayed(()->{if(pending==result&&!shotSent)maybeTakeStill();},150);
      // Never silently treat NOT_FOCUSED_LOCKED as success. Slow/fixed hardware
      // still has a bounded escape, with a visible advisory on the captured page.
      handler.postDelayed(()->{if(pending==result&&!shotSent){uncertainFocus=!focused()||(analysisSample()!=null&&!readiness.steady(SystemClock.elapsedRealtime()));takeStill();}},1400);
    });
    new Handler(Looper.getMainLooper()).postDelayed(()->{if(pending==result){pending=null;result.error("camera","Kamera tidak merespons. Ambil ulang.",null);}},12000);
  }
  boolean focused(){return afMode==CaptureRequest.CONTROL_AF_MODE_OFF||lastAf==CaptureResult.CONTROL_AF_STATE_PASSIVE_FOCUSED||lastAf==CaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED;}
  void maybeTakeStill(){
    if(pending==null||shotSent||closed)return;
    boolean exposure=lastAe<0||lastAe==CaptureResult.CONTROL_AE_STATE_CONVERGED||lastAe==CaptureResult.CONTROL_AE_STATE_LOCKED||lastAe==CaptureResult.CONTROL_AE_STATE_FLASH_REQUIRED;
    if(readiness.ready(SystemClock.elapsedRealtime(),captureStarted,focused(),exposure,analysisSample()!=null))takeStill();
  }
  void takeStill(){
    if(closed||pending==null||shotSent)return;shotSent=true;
    try{
        int physical=deviceTurns;captureOrientationKnown=physical>=0&&orientationAvailable;captureTurns=Math.max(0,physical);
        captureTarget=captureSelection.forPhoto(analysisWidth,analysisHeight,matchingGeometry);
        CaptureRequest.Builder shot=camera.createCaptureRequest(CameraDevice.TEMPLATE_STILL_CAPTURE);
        shot.addTarget(reader.getSurface());shot.set(CaptureRequest.JPEG_ORIENTATION,ScanGeometry.jpegOrientation(rotation,captureTurns));
        shot.set(CaptureRequest.JPEG_QUALITY,(byte)98);
        shot.set(CaptureRequest.CONTROL_AF_MODE,afMode);
        applyFocusRegion(shot);
        if(highQualityEdges)shot.set(CaptureRequest.EDGE_MODE,CaptureRequest.EDGE_MODE_HIGH_QUALITY);
        if(minimalNoise)shot.set(CaptureRequest.NOISE_REDUCTION_MODE,CaptureRequest.NOISE_REDUCTION_MODE_MINIMAL);
        shot.set(CaptureRequest.FLASH_MODE,preview.get(CaptureRequest.FLASH_MODE));
        session.capture(shot.build(),new CameraCaptureSession.CaptureCallback(){
          @Override public void onCaptureCompleted(CameraCaptureSession s,CaptureRequest r,TotalCaptureResult data){unlockFocus();}
          @Override public void onCaptureFailed(CameraCaptureSession s,CaptureRequest r,CaptureFailure failure){failCapture();}
        },handler);
      }catch(Exception e){failCapture();}
  }
  void unlockFocus(){
    if(closed||session==null||afMode==CaptureRequest.CONTROL_AF_MODE_OFF)return;
    try{preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
      session.capture(preview.build(),null,handler);preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_IDLE);
      session.setRepeatingRequest(preview.build(),focusCallback,handler);
    }catch(Exception ignored){}
  }
  void applyFocusRegion(CaptureRequest.Builder request){
    if(focusRegion==null)return;
    if(maxAfRegions>0)request.set(CaptureRequest.CONTROL_AF_REGIONS,focusRegion);
    if(maxAeRegions>0)request.set(CaptureRequest.CONTROL_AE_REGIONS,focusRegion);
  }
  void focus(double x,double y,MethodChannel.Result result){
    if(!DetectionTarget.unit(x)||!DetectionTarget.unit(y)){result.error("focus","Posisi fokus tidak valid.",null);return;}
    handler.post(()->{
      if(closed||session==null||camera==null||sensorBounds==null||pending!=null){activity.runOnUiThread(()->result.error("focus","Kamera belum siap untuk fokus.",null));return;}
      try{
        int[] r=ScanGeometry.metering(x,y,analysisWidth,analysisHeight,previewSize.getWidth(),previewSize.getHeight(),rotation,sensorBounds.left,sensorBounds.top,sensorBounds.width(),sensorBounds.height());
        focusRegion=new android.hardware.camera2.params.MeteringRectangle[]{new android.hardware.camera2.params.MeteringRectangle(new Rect(r[0],r[1],r[2],r[3]),900)};
        applyFocusRegion(preview);
        final int epoch=++focusEpoch;
        if(afMode!=CaptureRequest.CONTROL_AF_MODE_OFF){
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
          session.capture(preview.build(),null,handler);
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_START);
          session.capture(preview.build(),focusCallback,handler);
          preview.set(CaptureRequest.CONTROL_AF_TRIGGER,CaptureRequest.CONTROL_AF_TRIGGER_IDLE);
          handler.postDelayed(()->{if(!closed&&pending==null&&focusEpoch==epoch)unlockFocus();},1800);
        }
        session.setRepeatingRequest(preview.build(),focusCallback,handler);
        activity.runOnUiThread(()->result.success(maxAfRegions>0&&afMode!=CaptureRequest.CONTROL_AF_MODE_OFF));
      }catch(Exception e){activity.runOnUiThread(()->result.error("focus","Fokus objek belum dapat diarahkan.",null));}
    });
  }
  // getBitmap includes TextureView's transform; use the displayed aspect ratio
  // so live corners map directly to the Flutter overlay without another rotation.
  Bitmap snapshot(){
    if(closed||!texture.isAvailable()||previewSize==null||session==null)return null;
    int w=container.getWidth(),h=container.getHeight();if(w<1||h<1)return null;
    double scale=320.0/Math.max(w,h);
    return texture.getBitmap(Math.max(2,(int)Math.round(w*scale)),Math.max(2,(int)Math.round(h*scale)));
  }
  void failCapture(){activity.runOnUiThread(()->{if(pending!=null){MethodChannel.Result r=pending;pending=null;r.error("camera","Kamera tidak dapat mengambil foto. Coba lagi.",null);}});}
  void close(){
    if(closed)return;closed=true;orientationListener.disable();latest=null;failCapture();
    handler.post(()->{if(session!=null)session.close();if(camera!=null)camera.close();if(reader!=null)reader.close();if(analysisReader!=null)analysisReader.close();if(surface!=null)surface.release();thread.quitSafely();});
  }
  @Override public void dispose(){close();if(activity.cameraView==this)activity.cameraView=null;}
}
