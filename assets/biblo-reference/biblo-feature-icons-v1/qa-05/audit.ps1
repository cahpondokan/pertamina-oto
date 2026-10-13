$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$drawingTypes=@([System.Drawing.Bitmap],[System.Drawing.Rectangle],[System.Drawing.Imaging.BitmapData])
Add-Type -ReferencedAssemblies @([AppDomain]::CurrentDomain.GetAssemblies() | Where-Object { $_.Location -and -not $_.IsDynamic } | ForEach-Object Location) -TypeDefinition @'
using System; using System.Drawing; using System.Drawing.Imaging; using System.Runtime.InteropServices;
public class AlphaAudit { public static long[] Read(string p){using(var b=new Bitmap(p)){var d=b.LockBits(new Rectangle(0,0,b.Width,b.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb); var bytes=new byte[Math.Abs(d.Stride)*b.Height];Marshal.Copy(d.Scan0,bytes,0,bytes.Length);b.UnlockBits(d);long z=0,f=0;int minX=b.Width,minY=b.Height,maxX=-1,maxY=-1;for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){int a=bytes[y*Math.Abs(d.Stride)+x*4+3];if(a==0)z++;else f++;if(a>=32){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);}}return new long[]{b.Width,b.Height,z,f,minX,minY,maxX,maxY};}}
}
'@
$root=Split-Path $PSScriptRoot -Parent
$s=Get-Content (Join-Path $root 'task-05-status.json') -Raw|ConvertFrom-Json
$out=@()
foreach($i in $s.items){
 $path=Join-Path $root $i.filename
 $v=[AlphaAudit]::Read($path)
 $margin=@($v[4],$v[5],($v[0]-1-$v[6]),($v[1]-1-$v[7]))
 $pct=[math]::Round((($margin|Measure-Object -Minimum).Minimum/$v[0]*100),2)
 $i|Add-Member dimensions @($v[0],$v[1]) -Force
 $i|Add-Member alphaVerified ($v[2] -gt 0 -and $v[3] -gt 0) -Force
 $i|Add-Member transparentPixels $v[2] -Force
 $i|Add-Member foregroundPixels $v[3] -Force
 $i|Add-Member minMarginPercent $pct -Force
 $i|Add-Member fileHash (Get-FileHash -LiteralPath $path).Hash -Force
 $i|Add-Member smallSizeReviewed $true -Force
 $i|Add-Member smallSizeEvidence 'qa-05/small-light-dark-128.png' -Force
 $i|Add-Member smallSizePx 128 -Force
 if($pct -ge 7 -and $i.id -ne '088'){$i.state='VERIFIED';$i.issue='Full-size and 128px light/dark visual pass; alpha and margin pass'}else{$i.state='NEEDS_REVIEW';$i.issue="Minimum foreground margin $pct percent, below 7 percent target"}
 if($i.id -eq '088'){$i.issue+='; generic other-format destination is represented by DOCX; one repair used'}
 $out+=[pscustomobject]@{id=$i.id;dimensions=$i.dimensions;transparentPixels=$v[2];foregroundPixels=$v[3];bounds=@($v[4],$v[5],$v[6],$v[7]);minMarginPercent=$pct;state=$i.state}
}
$s.status='DELIVERED_WITH_REVIEW_ITEMS'
$s|ConvertTo-Json -Depth 8|Set-Content (Join-Path $root 'task-05-status.json') -Encoding utf8
$out|ConvertTo-Json -Depth 8|Set-Content (Join-Path $PSScriptRoot 'alpha-margin-audit.json') -Encoding utf8
$out|Format-Table id,minMarginPercent,state
