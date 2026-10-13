param([string]$Id,[string]$State='VERIFIED',[string]$Issue='')
Add-Type -AssemblyName System.Drawing
$root = Split-Path $PSScriptRoot -Parent
$manifest = Get-Content -Raw (Join-Path $root 'manifest.json') | ConvertFrom-Json
$item = $manifest.items | Where-Object id -eq $Id
$path = Join-Path $root $item.filename
$bmp = [System.Drawing.Bitmap]::new($path)
Add-Type -TypeDefinition 'public static class AlphaScan04 { public static long[] Scan(byte[] b,int w,int h,int stride){long z=0,s=0,minx=w,miny=h,maxx=-1,maxy=-1;for(int y=0;y<h;y++)for(int x=0;x<w;x++){byte a=b[y*stride+x*4+3];if(a==0)z++;if(a==255)s++;if(a>0){minx=System.Math.Min(minx,x);miny=System.Math.Min(miny,y);maxx=System.Math.Max(maxx,x);maxy=System.Math.Max(maxy,y);}}return new long[]{z,s,minx,miny,maxx,maxy};}}'
$rect=[System.Drawing.Rectangle]::new(0,0,$bmp.Width,$bmp.Height)
$data=$bmp.LockBits($rect,[System.Drawing.Imaging.ImageLockMode]::ReadOnly,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$bytes=[byte[]]::new($data.Stride*$data.Height)
[System.Runtime.InteropServices.Marshal]::Copy($data.Scan0,$bytes,0,$bytes.Length)
$stats=[AlphaScan04]::Scan($bytes,$bmp.Width,$bmp.Height,$data.Stride)
$bmp.UnlockBits($data)
$zero,$solid,$minx,$miny,$maxx,$maxy=$stats
$preview=[System.Drawing.Bitmap]::new(480,300)
$g=[System.Drawing.Graphics]::FromImage($preview)
$g.Clear([System.Drawing.Color]::White)
$g.FillRectangle([System.Drawing.Brushes]::MidnightBlue,0,150,480,150)
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$offset=10
foreach($size in @(64,96,128)){ $g.DrawImage($bmp,$offset,10,$size,$size);$g.DrawImage($bmp,$offset,160,$size,$size);$offset += $size+25 }
$preview.Save((Join-Path $PSScriptRoot ($Id+'-small.png')),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$preview.Dispose()
$entry=[ordered]@{id=$Id;filename=$item.filename;state=$State;dimensions=@($bmp.Width,$bmp.Height);alphaVerified=($zero -gt 0 -and $solid -gt 0);transparentPixels=$zero;opaquePixels=$solid;foregroundBounds=@($minx,$miny,$maxx,$maxy);visualReviewed=$true;smallPreviewCreated=$true;smallVisualReviewed=$false;fileHash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash;issue=$Issue}
$bmp.Dispose()
$statusPath=Join-Path $root 'task-04-status.json'
if(Test-Path $statusPath){$status=Get-Content -Raw $statusPath|ConvertFrom-Json}else{$status=[pscustomobject]@{lane=4;state='ACTIVE';items=@($manifest.items|Where-Object lane -eq 4|ForEach-Object{[pscustomobject]@{id=$_.id;filename=$_.filename;state='PENDING'}})}}
$status.items=@($status.items|ForEach-Object{if($_.id -eq $Id){[pscustomobject]$entry}else{$_}})
$status|ConvertTo-Json -Depth 12|Set-Content -LiteralPath $statusPath -Encoding UTF8
$entry|ConvertTo-Json -Compress
