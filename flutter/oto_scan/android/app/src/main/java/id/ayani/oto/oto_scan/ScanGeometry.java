package id.ayani.oto.oto_scan;

/** Shared center-crop mapping for target selection, still photos and Camera2 metering. */
public final class ScanGeometry {
  public static int deviceTurns(int degrees,int previous){
    if(degrees<0)return previous;
    int next=((degrees+45)/90)%4;
    if(previous<0)return next;
    int distance=Math.abs(degrees-previous*90);distance=Math.min(distance,360-distance);
    return distance>55?next:previous;
  }
  public static int jpegOrientation(int sensor,int turns){return (sensor+Math.max(0,turns)*90)%360;}
  public static boolean fresh(long frame,long now){return frame>0&&now>=frame&&now-frame<=1200;}
  public static double[] rotatePoint(double x,double y,int turns){
    if(turns<0||turns>3)throw new IllegalArgumentException();
    return turns==1?new double[]{1-y,x}:turns==2?new double[]{1-x,1-y}:turns==3?new double[]{y,1-x}:new double[]{x,y};
  }
  public static double[] rotateQuad(double[] q,int turns){
    if(q==null)return null;
    double[] out=new double[8];
    for(int i=0;i<4;i++){int j=(i-turns+4)%4;double[] p=rotatePoint(q[2*j],q[2*j+1],turns);out[2*i]=p[0];out[2*i+1]=p[1];}
    return out;
  }
  public static double[] align(double[] q,double[] reference){
    if(q==null||reference==null)return q;
    double best=Double.POSITIVE_INFINITY;int start=0;
    for(int shift=0;shift<4;shift++){double d=0;for(int i=0;i<4;i++){int j=(i+shift)%4;d+=Math.hypot(q[2*j]-reference[2*i],q[2*j+1]-reference[2*i+1]);}if(d<best){best=d;start=shift;}}
    double[] out=new double[8];for(int i=0;i<4;i++){int j=(i+start)%4;out[2*i]=q[2*j];out[2*i+1]=q[2*j+1];}return out;
  }
  public static double[] viewToImage(double x,double y,int iw,int ih,int vw,int vh){
    if(!DetectionTarget.unit(x)||!DetectionTarget.unit(y)||Math.min(Math.min(iw,ih),Math.min(vw,vh))<=0)throw new IllegalArgumentException();
    double scale=Math.max(vw/(double)iw,vh/(double)ih),visibleW=vw/scale,visibleH=vh/scale;
    return new double[]{Math.max(0,Math.min(1,((iw-visibleW)/2+x*visibleW)/iw)),Math.max(0,Math.min(1,((ih-visibleH)/2+y*visibleH)/ih))};
  }
  public static int[] metering(double x,double y,int vw,int vh,int bufferWidth,int bufferHeight,int rotation,int left,int top,int width,int height){
    if(rotation%90!=0||width<2||height<2)throw new IllegalArgumentException();
    rotation=(rotation%360+360)%360;boolean swap=rotation%180!=0;
    double[] p=viewToImage(x,y,swap?bufferHeight:bufferWidth,swap?bufferWidth:bufferHeight,vw,vh);
    double sx=p[0],sy=p[1];
    if(rotation==90){sx=p[1];sy=1-p[0];}else if(rotation==180){sx=1-p[0];sy=1-p[1];}else if(rotation==270){sx=1-p[1];sy=p[0];}
    double cropW=Math.min(width,height*bufferWidth/(double)bufferHeight),cropH=Math.min(height,width*bufferHeight/(double)bufferWidth);
    int cx=(int)Math.round(left+(width-cropW)/2+sx*cropW),cy=(int)Math.round(top+(height-cropH)/2+sy*cropH);
    int rw=Math.max(2,(int)Math.round(cropW*.12)),rh=Math.max(2,(int)Math.round(cropH*.12));
    int l=Math.max(left,Math.min(left+width-rw,cx-rw/2)),t=Math.max(top,Math.min(top+height-rh,cy-rh/2));
    return new int[]{l,t,l+rw,t+rh};
  }
}
