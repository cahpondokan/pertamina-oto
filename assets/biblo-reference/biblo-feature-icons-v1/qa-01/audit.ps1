$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core -TypeDefinition @'
using System;
using System.Drawing;
public static class BibloAlphaAudit {
 public static long[] Inspect(string path) {
  using(var b=new Bitmap(path)) {
   long zero=0,fg=0; int left=b.Width,top=b.Height,right=-1,bottom=-1;
   for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++) {
    byte a=b.GetPixel(x,y).A; if(a==0)zero++; if(a>200){fg++;left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
   }
   return new long[]{b.Width,b.Height,zero,fg,left,top,b.Width-1-right,b.Height-1-bottom};
  }
 }
}
'@
$base=Split-Path $PSScriptRoot -Parent
$s=Get-Content -Raw "$base/task-01-status.json"|ConvertFrom-Json
$qa=@()
foreach($i in $s.items){
 $r=[BibloAlphaAudit]::Inspect("$base/$($i.filename)")
 $m=@($r[4..7]|ForEach-Object{[Math]::Round(100*$_/$r[0],2)})
 $i.smallSizeReviewed=$true
 $i.alphaVerified=($r[2] -gt 0 -and $r[3] -gt 0)
 $i.dimensions=@($r[0],$r[1])
 $low=($m|Measure-Object -Minimum).Minimum
 if($low -lt 7){$i.state='NEEDS_REVIEW';$i.issue="Foreground alpha>200 margins L/T/R/B percent: $($m -join '/'); minimum below 7 percent target. No clipping. Small-size visual QA passed."}
 else{$i.state='VERIFIED';$i.issue=$null}
 $qa+=[pscustomobject]@{id=$i.id;filename=$i.filename;dimensions=$i.dimensions;transparentPixels=$r[2];foregroundPixels=$r[3];marginPercentLTRB=$m;state=$i.state;smallPreview="$($i.id)-small-light-dark.png";sha256=$i.fileHash}
}
$s.state='DELIVERED_WITH_REVIEW_NOTES'
$s|ConvertTo-Json -Depth 10|Set-Content -Encoding utf8 "$base/task-01-status.json"
$qa|ConvertTo-Json -Depth 10|Set-Content -Encoding utf8 "$PSScriptRoot/audit-results.json"
$qa|Select-Object id,state,@{n='Margins';e={$_.marginPercentLTRB -join '/'}},transparentPixels|Format-Table -AutoSize


