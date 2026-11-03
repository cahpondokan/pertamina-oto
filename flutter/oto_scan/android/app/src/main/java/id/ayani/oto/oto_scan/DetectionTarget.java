package id.ayani.oto.oto_scan;

/** Selection changes ranking, never the four-edge evidence required by a detector. */
public final class DetectionTarget {
  final double[] point,previous;
  final boolean confirmationRequired;
  public DetectionTarget(double[] point,double[] previous){
    this(point,previous,false);
  }
  private DetectionTarget(double[] point,double[] previous,boolean confirmationRequired){
    if(point!=null&&(point.length!=2||!unit(point[0])||!unit(point[1])))throw new IllegalArgumentException("Invalid target");
    if(previous!=null&&!DocumentDetector.valid(previous))throw new IllegalArgumentException("Invalid reference");
    this.point=point==null?null:point.clone();this.previous=previous==null?null:previous.clone();this.confirmationRequired=confirmationRequired;
  }
  static DetectionTarget manual(){return new DetectionTarget(null,null,true);}
  static boolean unit(double x){return Double.isFinite(x)&&x>=0&&x<=1;}
  static boolean contains(double[] q,double x,double y){
    for(int i=0;i<4;i++){int j=(i+1)%4;if((q[2*j]-q[2*i])*(y-q[2*i+1])-(q[2*j+1]-q[2*i+1])*(x-q[2*i])<-.000001)return false;}
    return true;
  }
  static double area(double[] q){double sum=0;for(int i=0;i<4;i++){int j=(i+1)%4;sum+=q[2*i]*q[2*j+1]-q[2*j]*q[2*i+1];}return Math.abs(sum)/2;}
  double rank(double[] q,double evidence){
    if(point!=null&&!contains(q,point[0],point[1]))return 0;
    if(previous==null)return point==null?evidence:evidence/Math.pow(area(q),1.5);
    // Compare cyclic corner identities: the first corner can change near 45 degrees.
    double distance=Double.POSITIVE_INFINITY;
    for(int shift=0;shift<4;shift++){double d=0;for(int i=0;i<4;i++){int j=(i+shift)%4;d+=Math.hypot(q[2*i]-previous[2*j],q[2*i+1]-previous[2*j+1])/4;}distance=Math.min(distance,d);}
    double ratio=Math.abs(Math.log(area(q)/area(previous)));
    if(distance>.12||ratio>Math.log(2))return 0;
    return evidence*4*Math.exp(-12*distance-2*ratio);
  }
  DetectionTarget atShutter(long frame,long now){
    if(previous==null||ScanGeometry.fresh(frame,now))return this;
    // Reject a guide already stale when the user presses the shutter. The
    // accepted target is only a search hint: the still must verify four edges.
    return manual();
  }
  DetectionTarget toOrientedImage(int imageWidth,int imageHeight,int viewWidth,int viewHeight,int turns){
    DetectionTarget mapped=toImage(turns%2==0?imageWidth:imageHeight,turns%2==0?imageHeight:imageWidth,viewWidth,viewHeight);
    return new DetectionTarget(mapped.point==null?null:ScanGeometry.rotatePoint(mapped.point[0],mapped.point[1],turns),ScanGeometry.rotateQuad(mapped.previous,turns),confirmationRequired);
  }
  DetectionTarget toImage(int imageWidth,int imageHeight,int viewWidth,int viewHeight){
    double[] p=point==null?null:ScanGeometry.viewToImage(point[0],point[1],imageWidth,imageHeight,viewWidth,viewHeight);
    double[] q=previous==null?null:previous.clone();
    if(q!=null)for(int i=0;i<4;i++){double[] v=ScanGeometry.viewToImage(q[2*i],q[2*i+1],imageWidth,imageHeight,viewWidth,viewHeight);q[2*i]=v[0];q[2*i+1]=v[1];}
    return new DetectionTarget(p,q,confirmationRequired);
  }
}
