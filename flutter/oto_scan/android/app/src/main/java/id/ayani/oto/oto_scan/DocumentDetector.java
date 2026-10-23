package id.ayani.oto.oto_scan;

import java.util.*;

/** Bounded, first-party local contour detector. No per-pixel object allocations. */
public final class DocumentDetector {
  private static final class P {
    final int x,y; P(int x,int y){this.x=x;this.y=y;}
  }
  public static double[] detect(int[] pixels,int sourceWidth,int sourceHeight) {
    return detect(pixels,sourceWidth,sourceHeight,400,true,false,null);
  }
  public static double[] detectPreview(int[] pixels,int w,int h){return detectPreview(pixels,w,h,null);}
  public static double[] detectPreview(int[] pixels,int w,int h,DetectionTarget target){return detect(pixels,w,h,320,false,true,target);}
  public static double[] detectOutline(int[] pixels,int w,int h){return detectOutline(pixels,w,h,null);}
  public static double[] detectOutline(int[] pixels,int w,int h,DetectionTarget target){return detect(pixels,w,h,480,false,false,target);}
  private static double[] detect(int[] pixels,int sourceWidth,int sourceHeight,int limit,boolean refine,boolean quick,DetectionTarget target) {
    if(target!=null&&target.confirmationRequired)return null;
    if(sourceWidth<8||sourceHeight<8||pixels.length!=sourceWidth*sourceHeight)return null;
    double[] best=null;
    // A selected/prior object is searched locally first. Every ROI still requires
    // the same four-side evidence; the preview outline itself is never a crop.
    double scale=Math.min(1,limit/(double)Math.max(sourceWidth,sourceHeight));
    int w=Math.max(8,(int)Math.round(sourceWidth*scale)),h=Math.max(8,(int)Math.round(sourceHeight*scale));
    int[] small=pixels;
    if(w!=sourceWidth||h!=sourceHeight){small=new int[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)small[y*w+x]=pixels[(int)Math.round(y*(sourceHeight-1)/(double)(h-1))*sourceWidth+(int)Math.round(x*(sourceWidth-1)/(double)(w-1))];}
    if(target!=null&&target.previous!=null){
      double l=1,t=1,r=0,b=0;for(int i=0;i<4;i++){l=Math.min(l,target.previous[2*i]);r=Math.max(r,target.previous[2*i]);t=Math.min(t,target.previous[2*i+1]);b=Math.max(b,target.previous[2*i+1]);}
      double mx=Math.max(.02,(r-l)*.35),my=Math.max(.02,(b-t)*.35);
      best=region(small,w,h,l-mx,t-my,r+mx,b+my,target,limit);
    }else if(target!=null&&target.point!=null){
      for(double span:new double[]{.25,.4,.65}){
        double x=target.point[0],y=target.point[1];best=region(small,w,h,x-span/2,y-span/2,x+span/2,y+span/2,target,limit);
        if(best!=null)break;
      }
    }
    if(best==null)best=detectRaster(pixels,sourceWidth,sourceHeight,limit,false,quick,target);
    if(best==null&&target==null){
      // Bounded overlapping tiles find small untapped cards, without weakening
      // whole-frame area/contrast gates or admitting a clipped ROI background.
      double score=0;
      for(double cy:new double[]{.5,.25,.75})for(double cx:new double[]{.5,.25,.75}){
        double[] q=region(small,w,h,cx-.25,cy-.25,cx+.25,cy+.25,null,limit);
        if(q!=null&&DetectionTarget.area(q)>score){best=q;score=DetectionTarget.area(q);}
      }
    }
    if(best!=null&&target!=null)best=ScanGeometry.align(best,target.previous);
    return best==null||!refine?best:refine((x,y)->pixels[y*sourceWidth+x],sourceWidth,sourceHeight,best);
  }
  private static double[] region(int[] pixels,int w,int h,double left,double top,double right,double bottom,DetectionTarget target,int limit){
    int l=Math.max(0,(int)Math.floor(left*(w-1))),t=Math.max(0,(int)Math.floor(top*(h-1))),r=Math.min(w-1,(int)Math.ceil(right*(w-1))),b=Math.min(h-1,(int)Math.ceil(bottom*(h-1)));
    int rw=r-l+1,rh=b-t+1;if(rw<24||rh<24)return null;
    double[] point=null;
    if(target!=null&&target.point!=null){point=new double[]{(target.point[0]*(w-1)-l)/(rw-1),(target.point[1]*(h-1)-t)/(rh-1)};if(!DetectionTarget.unit(point[0])||!DetectionTarget.unit(point[1]))return null;}
    int[] crop=new int[rw*rh];for(int y=0;y<rh;y++)System.arraycopy(pixels,(t+y)*w+l,crop,y*rw,rw);
    double[] q=detectRaster(crop,rw,rh,limit,false,false,point==null?null:new DetectionTarget(point,null));
    if(q==null)return null;
    for(int i=0;i<4;i++){q[2*i]=(l+q[2*i]*(rw-1))/(w-1);q[2*i+1]=(t+q[2*i+1]*(rh-1))/(h-1);}
    if(!valid(q)||(target!=null&&target.rank(q,1)<=0))return null;
    return q;
  }
  private static double[] detectRaster(int[] pixels,int sourceWidth,int sourceHeight,int limit,boolean refine,boolean quick,DetectionTarget target) {
    if(sourceWidth<8 || sourceHeight<8 || pixels.length != sourceWidth*sourceHeight) return null;
    int step=Math.max(1,(int)Math.ceil(Math.max(sourceWidth,sourceHeight)/(double)limit));
    int w=(sourceWidth-1)/step+1,h=(sourceHeight-1)/step+1;
    int[] gray=new int[w*h],blueRed=new int[w*h],redGreen=new int[w*h],rgb=new int[w*h];
    for(int y=0;y<h;y++) for(int x=0;x<w;x++) {
      int p=pixels[y*step*sourceWidth+x*step];
      int v=(299*((p>>16)&255)+587*((p>>8)&255)+114*(p&255))/1000;
      int i=y*w+x;gray[i]=v;rgb[i]=p;
      blueRed[i]=Math.max(0,Math.min(255,128+(p&255)-((p>>16)&255)));
      redGreen[i]=Math.max(0,Math.min(255,128+((p>>16)&255)-((p>>8)&255)));
    }
    double bestScore=0; double[] best=null;
    boolean[] visited=new boolean[gray.length];int[] queue=new int[gray.length];
    // A cool white page may be darker than a warm desk. Segment luminance and
    // chroma independently, then require real RGB contrast on all four edges.
    for(int[] channel:new int[][]{gray,blueRed,redGreen}){
    int[] hist=new int[256];for(int v:channel)hist[v]++;
    int min=0,max=255;while(min<255&&hist[min]==0)min++;while(max>0&&hist[max]==0)max--;
    if(max-min<8)continue;
    double total=0;for(int i=0;i<256;i++)total+=i*hist[i];
    double sum=0,bestVariance=0;int count=0,threshold=min;
    for(int i=min;i<max;i++){
      count+=hist[i];sum+=i*hist[i];int foreground=gray.length-count;
      if(count==0||foreground==0)continue;
      double diff=sum/count-(total-sum)/foreground,variance=count*(double)foreground*diff*diff;
      if(variance>bestVariance){bestVariance=variance;threshold=i;}
    }
    int[] thresholds=Arrays.stream(new int[]{threshold,threshold+(max-threshold)/5,threshold+(max-threshold)/2,min+(threshold-min)/2,min+(max-min)*2/3,max-20,max-1,min}).distinct().toArray();
    // Both light paper and dark rectangular objects; clipped background is always rejected.
    for(int polarity=0;polarity<2;polarity++) for(int split:thresholds){
      if(split<min||split>=max)continue;
      Arrays.fill(visited,false);
      for(int seed=0;seed<gray.length;seed++){
        if(visited[seed]||!inside(channel[seed],split,polarity))continue;
        int head=0,tail=1;queue[0]=seed;visited[seed]=true;
        ArrayList<P> boundary=new ArrayList<>();boolean clipped=false;
        while(head<tail){
          int index=queue[head++],x=index%w,y=index/w;
          if(!clipped&&(x<=1||y<=1||x>=w-2||y>=h-2)){clipped=true;boundary.clear();}
          boolean edge=false;
          for(int n=0;n<4;n++){
            // Avoid allocating two neighbour arrays for every visited pixel.
            if((n==0&&x==0)||(n==1&&x==w-1)||(n==2&&y==0)||(n==3&&y==h-1)){edge=true;continue;}
            int next=index+(n==0?-1:n==1?1:n==2?-w:w);
            if(!inside(channel[next],split,polarity)){edge=true;continue;}
            if(!visited[next]){visited[next]=true;queue[tail++]=next;}
          }
          if(edge&&!clipped)boundary.add(new P(x,y));
        }
        if(clipped||tail<gray.length*.008||boundary.size()<4)continue;
        List<P> hull=hull(boundary);if(hull.size()<4)continue;
        double ha=area(hull);if(ha<gray.length*.08||ha>gray.length*.93)continue;
        List<P> poly=new ArrayList<>(hull);
        while(poly.size()>4){
          int remove=0;double smallest=Double.MAX_VALUE;
          for(int i=0;i<poly.size();i++){
            double a=Math.abs(cross(poly.get((i+poly.size()-1)%poly.size()),poly.get(i),poly.get((i+1)%poly.size())));
            if(a<smallest){smallest=a;remove=i;}
          }
          poly.remove(remove);
        }
        double fit=area(poly)/ha;if(fit<.92)continue;
        int start=0;for(int i=1;i<4;i++)if(poly.get(i).x+poly.get(i).y<poly.get(start).x+poly.get(start).y)start=i;
        P[] q=new P[4];double cx=0,cy=0;
        for(int i=0;i<4;i++){q[i]=poly.get((start+i)%4);cx+=q[i].x/4.0;cy+=q[i].y/4.0;}
        double contrast=0;int supported=0;
        for(int side=0;side<4;side++){
          double sc=0;int coherent=0;P a=q[side],b=q[(side+1)%4];
          for(int sample=1;sample<=7;sample++){
            double t=sample/8.0,x=a.x*(1-t)+b.x*t,y=a.y*(1-t)+b.y*t;
            double dx=cx-x,dy=cy-y,len=Math.hypot(dx,dy);dx/=len;dy/=len;
            double difference=contrast(at(rgb,w,h,x+dx*3,y+dy*3),at(rgb,w,h,x-dx*3,y-dy*3));
            sc+=difference;if(difference>=4)coherent++;
          }
          sc/=7;if(sc>=4.5&&(coherent>=5||(sc>=24&&coherent>=3)))supported++;contrast+=sc;
        }
        contrast/=4;if(supported!=4||contrast<11)continue;
        double[] out=new double[8];
        for(int i=0;i<4;i++){
          double dx=cx-q[i].x,dy=cy-q[i].y,len=Math.hypot(dx,dy);
          out[i*2]=(q[i].x+dx/len*1.5)*step/(sourceWidth-1);
          out[i*2+1]=(q[i].y+dy/len*1.5)*step/(sourceHeight-1);
        }
        if(!valid(out))continue;
        // A large, strong, four-sided sheet needs no further colour passes for
        // the live guide. Still photos always compare every candidate.
        if(target==null&&quick&&ha>gray.length*.45&&fit>=.97&&contrast>=24)return out;
        double score=area(poly)*(fit*.6+Math.min(1,contrast/80)*.4);
        if(target!=null)score=target.rank(out,score);
        if(score>bestScore){bestScore=score;best=out;}
      }
    }
    }
    if(best==null){
      // Preserve near-limit detail for line fitting instead of halving an
      // entire frame just because its longest side exceeds the limit slightly.
      if(step==1)best=EdgeDetector.detect(rgb,w,h,target);
      else{
        double scale=limit/(double)Math.max(sourceWidth,sourceHeight);
        int ew=Math.max(8,(int)Math.round(sourceWidth*scale)),eh=Math.max(8,(int)Math.round(sourceHeight*scale));
        int[] edgePixels=new int[ew*eh];
        for(int y=0;y<eh;y++)for(int x=0;x<ew;x++){
          int sx=(int)Math.round(x*(sourceWidth-1)/(double)(ew-1)),sy=(int)Math.round(y*(sourceHeight-1)/(double)(eh-1));
          edgePixels[y*ew+x]=pixels[sy*sourceWidth+sx];
        }
        best=EdgeDetector.detect(edgePixels,ew,eh,target);
      }
    }
    return best==null||!refine?best:refine((x,y)->pixels[y*sourceWidth+x],sourceWidth,sourceHeight,best);
  }
  private static double contrast(int a,int b){
    double dr=((a>>16)&255)-((b>>16)&255),dg=((a>>8)&255)-((b>>8)&255),db=(a&255)-(b&255);
    return Math.sqrt((dr*dr+dg*dg+db*db)/3);
  }
  // First-party A1 refinement: robust line fitting close to the winning contour.
  // A small source-pixel inset excludes the desk/shadow without a large margin.
  public interface PixelReader {int pixel(int x,int y);}
  public static double[] refine(PixelReader pixels,int w,int h,double[] q){
    if(!valid(q))return null;
    double[][] corners=new double[4][2],lines=new double[4][4];
    for(int i=0;i<4;i++){corners[i][0]=q[2*i]*(w-1);corners[i][1]=q[2*i+1]*(h-1);}
    int radius=Math.max(4,Math.max(w,h)/65),gap=Math.max(3,Math.max(w,h)/400);
    for(int i=0;i<4;i++){
      double[] a=corners[i],b=corners[(i+1)%4];double dx=b[0]-a[0],dy=b[1]-a[1],len=Math.hypot(dx,dy),nx=-dy/len,ny=dx/len;
      List<double[]> offsets=new ArrayList<>();
      for(int sample=1;sample<=13;sample++){
        double t=sample/14.0,x=a[0]+dx*t,y=a[1]+dy*t,best=0;int offset=0;
        for(int k=-radius;k<=radius;k++){
          double score=contrast(at(pixels,w,h,x+nx*(k+gap),y+ny*(k+gap)),at(pixels,w,h,x+nx*(k-gap),y+ny*(k-gap)))/(1+Math.abs(k)*.005);
          if(score>best){best=score;offset=k;}
        }
        if(best>=5)offsets.add(new double[]{t,offset});
      }
      if(offsets.size()<8)return q;
      offsets.sort(Comparator.comparingDouble(p->p[1]));double median=offsets.get(offsets.size()/2)[1];
      offsets.removeIf(p->Math.abs(p[1]-median)>Math.max(4,radius*.6));if(offsets.size()<7)return q;
      double mt=0,mo=0;for(double[] p:offsets){mt+=p[0]/offsets.size();mo+=p[1]/offsets.size();}
      double numerator=0,denominator=0;for(double[] p:offsets){numerator+=(p[0]-mt)*(p[1]-mo);denominator+=(p[0]-mt)*(p[0]-mt);}
      // The gradient aperture is not a crop margin. Reserve only 1–3 pixels;
      // adding the whole aperture cuts too far into otherwise sharp corners.
      double slope=numerator/denominator,start=mo-slope*mt+1+Math.min(2,Math.max(w,h)/700),end=start+slope;
      lines[i]=new double[]{a[0]+nx*start,a[1]+ny*start,b[0]+nx*end,b[1]+ny*end};
    }
    double[] result=new double[8];
    for(int i=0;i<4;i++){
      double[] a=lines[(i+3)%4],b=lines[i];double ux=a[2]-a[0],uy=a[3]-a[1],vx=b[2]-b[0],vy=b[3]-b[1],det=ux*vy-uy*vx;
      if(Math.abs(det)<1)return q;
      double t=((b[0]-a[0])*vy-(b[1]-a[1])*vx)/det,x=a[0]+t*ux,y=a[1]+t*uy;
      if(Math.hypot(x-corners[i][0],y-corners[i][1])>radius*1.8)return q;
      result[2*i]=x/(w-1);result[2*i+1]=y/(h-1);
    }
    return valid(result)?result:q;
  }
  private static boolean inside(int v,int split,int p){return p==0?v>split:v<=split;}
  private static int at(int[] g,int w,int h,double x,double y){return g[Math.max(0,Math.min(h-1,(int)Math.round(y)))*w+Math.max(0,Math.min(w-1,(int)Math.round(x)))];}
  private static int at(PixelReader p,int w,int h,double x,double y){return p.pixel(Math.max(0,Math.min(w-1,(int)Math.round(x))),Math.max(0,Math.min(h-1,(int)Math.round(y))));}
  private static double cross(P a,P b,P c){return (b.x-a.x)*(double)(c.y-a.y)-(b.y-a.y)*(double)(c.x-a.x);}
  private static double area(List<P> p){double a=0;for(int i=0;i<p.size();i++){P b=p.get((i+1)%p.size()),c=p.get(i);a+=c.x*b.y-c.y*b.x;}return Math.abs(a)/2;}
  private static List<P> hull(List<P> points){
    points.sort(Comparator.comparingInt((P p)->p.x).thenComparingInt(p->p.y));
    ArrayList<P> h=new ArrayList<>();
    for(P p:points){while(h.size()>=2&&cross(h.get(h.size()-2),h.get(h.size()-1),p)<=0)h.remove(h.size()-1);h.add(p);}
    int lower=h.size();
    for(int i=points.size()-2;i>=0;i--){P p=points.get(i);while(h.size()>lower&&cross(h.get(h.size()-2),h.get(h.size()-1),p)<=0)h.remove(h.size()-1);h.add(p);}
    h.remove(h.size()-1);return h;
  }
  public static boolean valid(double[] q){
    if(q==null||q.length!=8)return false;
    for(double x:q)if(!Double.isFinite(x)||x<0||x>1)return false;
    double area=0;
    for(int i=0;i<4;i++){
      int a=i*2,b=(i+1)%4*2,c=(i+2)%4*2;
      if((q[b]-q[a])*(q[c+1]-q[b+1])-(q[b+1]-q[a+1])*(q[c]-q[b])<.0001)return false;
      area+=q[a]*q[b+1]-q[a+1]*q[b];
    }
    return area>.0008;
  }
}
