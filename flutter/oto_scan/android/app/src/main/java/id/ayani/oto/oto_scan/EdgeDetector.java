package id.ayani.oto.oto_scan;

import java.util.*;

/** Bounded line-based fallback for paper whose exposure cannot form a closed threshold region. */
final class EdgeDetector {
  static final class Line {
    double a,b,rho,position;int votes;boolean vertical;
    Line(double a,double b,double rho,int votes,int w,int h){
      vertical=Math.abs(a)>=Math.abs(b);
      if((vertical?a:b)<0){a=-a;b=-b;rho=-rho;}
      this.a=a;this.b=b;this.rho=rho;this.votes=votes;
      position=vertical?(rho-b*h/2)/a:(rho-a*w/2)/b;
    }
  }
  static double[] detect(int[] rgb,int w,int h){
    return detect(rgb,w,h,null);
  }
  static double[] detect(int[] rgb,int w,int h,DetectionTarget target){
    if(w<16||h<16)return null;
    int[] gx=new int[w*h],gy=new int[w*h];
    int diagonal=(int)Math.ceil(Math.hypot(w,h)),bins=diagonal*2+1;
    int[] accumulator=new int[180*bins];double[] cos=new double[180],sin=new double[180];
    for(int a=0;a<180;a++){cos[a]=Math.cos(Math.toRadians(a));sin[a]=Math.sin(Math.toRadians(a));}
    for(int y=1;y<h-1;y++)for(int x=1;x<w-1;x++){
      int at=y*w+x,best=0;
      for(int shift=0;shift<=16;shift+=8){
        int a=(rgb[at-w-1]>>shift)&255,b=(rgb[at-w]>>shift)&255,c=(rgb[at-w+1]>>shift)&255;
        int d=(rgb[at-1]>>shift)&255,f=(rgb[at+1]>>shift)&255;
        int g=(rgb[at+w-1]>>shift)&255,j=(rgb[at+w]>>shift)&255,k=(rgb[at+w+1]>>shift)&255;
        int dx=-a+c-2*d+2*f-g+k,dy=-a-2*b-c+g+2*j+k,magnitude=dx*dx+dy*dy;
        if(magnitude>best){best=magnitude;gx[at]=dx;gy[at]=dy;}
      }
      if(best<36*36)continue;
      int normal=(int)Math.round(Math.toDegrees(Math.atan2(gy[at],gx[at])));
      for(int delta=-3;delta<=3;delta++){
        int angle=((normal+delta)%180+180)%180;
        int rho=(int)Math.round(x*cos[angle]+y*sin[angle])+diagonal;
        accumulator[angle*bins+rho]++;
      }
    }
    List<Line> peaks=new ArrayList<>();
    int minimum=Math.max(14,Math.min(w,h)/10);
    for(int a=0;a<180;a++)for(int r=1;r<bins-1;r++){
      int n=accumulator[a*bins+r];if(n<minimum)continue;
      if(n<accumulator[a*bins+r-1]||n<=accumulator[a*bins+r+1])continue;
      if(a>0&&n<accumulator[(a-1)*bins+r])continue;
      if(a<179&&n<accumulator[(a+1)*bins+r])continue;
      peaks.add(new Line(cos[a],sin[a],r-diagonal,n,w,h));
    }
    peaks.sort((a,b)->Integer.compare(b.votes,a.votes));
    List<Line> vertical=new ArrayList<>(),horizontal=new ArrayList<>();
    for(Line line:peaks){
      List<Line> selected=line.vertical?vertical:horizontal;
      int length=line.vertical?w:h;
      if(selected.size()>=12||line.position<2||line.position>length-3)continue;
      boolean duplicate=false;int nearby=0;
      for(Line old:selected){
        if(Math.abs(old.position-line.position)<5&&old.a*line.a+old.b*line.b>.996)duplicate=true;
        if((int)(old.position*4/length)==(int)(line.position*4/length))nearby++;
      }
      if(!duplicate&&nearby<4)selected.add(line);
      if(vertical.size()==12&&horizontal.size()==12)break;
    }
    vertical.sort(Comparator.comparingDouble(l->l.position));horizontal.sort(Comparator.comparingDouble(l->l.position));
    double bestScore=0;double[] best=null;
    for(int l=0;l<vertical.size();l++)for(int r=l+1;r<vertical.size();r++){
      if(vertical.get(r).position-vertical.get(l).position<w*.18)continue;
      for(int t=0;t<horizontal.size();t++)for(int b=t+1;b<horizontal.size();b++){
        if(horizontal.get(b).position-horizontal.get(t).position<h*.18)continue;
        double[] q=new double[8];
        if(!intersection(vertical.get(l),horizontal.get(t),q,0)||
           !intersection(vertical.get(r),horizontal.get(t),q,2)||
           !intersection(vertical.get(r),horizontal.get(b),q,4)||
           !intersection(vertical.get(l),horizontal.get(b),q,6))continue;
        boolean bounded=true;double area=0;
        for(int i=0;i<4;i++){int j=(i+1)%4;bounded&=q[i*2]>=2&&q[i*2]<w-3&&q[i*2+1]>=2&&q[i*2+1]<h-3;area+=q[i*2]*q[j*2+1]-q[i*2+1]*q[j*2];}
        area/=2;if(!bounded||area<w*h*.08||area>w*h*.93)continue;
        double[] normalized=q.clone();for(int i=0;i<4;i++){normalized[i*2]/=w-1;normalized[i*2+1]/=h-1;}
        if(!DocumentDetector.valid(normalized))continue;
        double contrast=0;boolean supported=true;int strongSides=0;
        double[][] polarity=new double[4][3];double[] strengths=new double[4];
        for(int i=0;i<4;i++){
          double evidence=support(rgb,gx,gy,w,h,q,i,polarity[i]);strengths[i]=evidence;
          if(evidence<0){supported=false;break;}contrast+=evidence/4;if(evidence>=11)strongSides++;
        }
        // A line drawn inside a page has similar pixels on both sides and
        // fails this step-edge check, even when its Sobel response is strong.
        if(!supported||contrast<11||strongSides<3)continue;
        // Joining the left edge of another sheet reverses its inside/outside
        // colour step. Strong sides must describe the same paper boundary.
        for(int i=0;i<4;i++)for(int j=i+1;j<4;j++)if(strengths[i]>=11&&strengths[j]>=11){
          double dot=0,aa=0,bb=0;for(int k=0;k<3;k++){dot+=polarity[i][k]*polarity[j][k];aa+=polarity[i][k]*polarity[i][k];bb+=polarity[j][k]*polarity[j][k];}
          if(dot<-.25*Math.sqrt(aa*bb))supported=false;
        }
        if(!supported)continue;
        double score=area*(.75+.25*Math.min(1,contrast/60));
        if(target!=null)score=target.rank(normalized,score);
        if(score>bestScore){bestScore=score;best=normalized;}
      }
    }
    return best;
  }
  static boolean intersection(Line a,Line b,double[] q,int offset){
    double det=a.a*b.b-a.b*b.a;if(Math.abs(det)<.25)return false;
    q[offset]=(a.rho*b.b-a.b*b.rho)/det;q[offset+1]=(a.a*b.rho-a.rho*b.a)/det;return true;
  }
  static double support(int[] rgb,int[] gx,int[] gy,int w,int h,double[] q,int side,double[] polarity){
    int j=(side+1)%4;double ax=q[side*2],ay=q[side*2+1],dx=q[j*2]-ax,dy=q[j*2+1]-ay;
    double length=Math.hypot(dx,dy),nx=-dy/length,ny=dx/length,total=0;int supported=0;
    double[] signed=new double[3];
    double[] differences=new double[13];
    for(int i=1;i<=13;i++){
      double x=ax+dx*i/14,y=ay+dy*i/14;boolean aligned=false;
      for(int k=-2;k<=2;k++){
        int xx=(int)Math.round(x+nx*k),yy=(int)Math.round(y+ny*k);
        if(xx<1||xx>=w-1||yy<1||yy>=h-1)continue;
        int at=yy*w+xx;double magnitude=Math.hypot(gx[at],gy[at]);
        if(magnitude>=24&&Math.abs(gx[at]*nx+gy[at]*ny)>=magnitude*.85)aligned=true;
      }
      int inside=at(rgb,w,h,x+nx*3.5,y+ny*3.5),outside=at(rgb,w,h,x-nx*3.5,y-ny*3.5);
      double sum=0;for(int shift=0;shift<=16;shift+=8){int d=((inside>>shift)&255)-((outside>>shift)&255);sum+=d*d;signed[shift/8]+=d;}
      double difference=Math.sqrt(sum/3);total+=difference;differences[i-1]=difference;
      if(aligned&&difference>=5)supported++;
    }
    double coherence=Math.sqrt((signed[0]*signed[0]+signed[1]*signed[1]+signed[2]*signed[2])/3);
    System.arraycopy(signed,0,polarity,0,3);
    Arrays.sort(differences);
    return supported>=9&&coherence>=total*.7?differences[6]:-1;
  }
  static int at(int[] p,int w,int h,double x,double y){return p[Math.max(0,Math.min(h-1,(int)Math.round(y)))*w+Math.max(0,Math.min(w-1,(int)Math.round(x)))];}
}
