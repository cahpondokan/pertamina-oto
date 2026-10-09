$ErrorActionPreference='Stop'
$base=(Resolve-Path '.').Path
$out=Join-Path $base 'artifacts/user-guide-20261008'
$audit=Join-Path $base 'artifacts/window-minimum-20261008/native-tested/ui-regression-audit'
$m=Get-Content "$out/manifest.json" -Raw|ConvertFrom-Json
$specs=@()
foreach($v in @(@('home','Beranda'),@('home-loaded','Beranda — dokumen contoh'),@('catalog','Semua Alat'),@('reader-landing','Pembaca PDF — buka dokumen'),@('favorites','Favorit'),@('history','Riwayat'),@('secured','File Diamankan'),@('assistant','Assistant'),@('admin','Administrasi'),@('help','Bantuan'),@('settings','Pengaturan dan update'),@('result','Hasil tersimpan'))){$specs+=[pscustomobject]@{file="1920-$($v[0]).png";id=$v[0];title=$v[1];description='Halaman aplikasi dari audit client WinUI final. Lihat label dan status pada gambar.'}}
foreach($t in $m.tools){$specs+=[pscustomobject]@{file="1920-empty-$($t.id).png";id="feature-$($t.id)";title=$t.title;description=$t.description}}
$caps=@();Add-Type -AssemblyName System.Drawing
foreach($s in $specs){
 $src=Join-Path $audit $s.file;$dst=Join-Path "$out/raw" $s.file
 $img=[Drawing.Bitmap]::FromFile($src);$crop=$null
 if($s.id -eq 'settings'){$rect=[Drawing.Rectangle]::new(0,0,$img.Width,700);$safe=$img.Clone($rect,$img.PixelFormat);$safe.Save($dst,[Drawing.Imaging.ImageFormat]::Png);$safe.Dispose();$crop='Privacy crop: top700px only; default output folder below excluded. Original retained in task04 evidence.'}else{Copy-Item -LiteralPath $src -Destination $dst}
 $w=$img.Width;$h=$(if($crop){700}else{$img.Height});$img.Dispose()
 $caps+=[ordered]@{id=$s.id;title=$s.title;description=$s.description;mode='audit-client-render';path=('raw/'+$s.file);width=$w;height=$h;imageSha256=(Get-FileHash $dst).Hash;sourceImageSha256=(Get-FileHash $src).Hash;sourcePath=$src;build='20261009-035333-e218f12d';dllSha256='053781FA261C1DD8873748AE8CE0390CA1E92BC640AB94AB1BB699E3F833E9EF';nativeFrameIncluded=$false;clientDip=@(1920,930);physicalMonitorClaim=$false;fixture='Synthetic / empty local audit profile';privacyCrop=$crop;provenance='native-commands.json --audit-ui-regression, final tested DLL hash checked; RenderTargetBitmap root at1920x930, not native maximize';visualReview='PENDING_CONTACT_REVIEW'}
}
$caps+=@($m.captures|Where-Object mode -ne 'audit-client-render')
$caps|ConvertTo-Json -Depth 12|Set-Content -Encoding utf8 "$out/captures.json"
$m.captures=$caps;$m.stageA='ALL_33_TOOL_PAGE_RENDERS_AND_CORE_NAVIGATION_IMPORTED';$m|ConvertTo-Json -Depth 14|Set-Content -Encoding utf8 "$out/manifest.json"
for($start=0;$start -lt $caps.Count;$start+=16){$canvas=[Drawing.Bitmap]::new(1920,1200);$g=[Drawing.Graphics]::FromImage($canvas);$g.Clear([Drawing.Color]::White);$font=[Drawing.Font]::new('Arial',11);for($j=0;$j -lt 16 -and $start+$j -lt $caps.Count;$j++){$c=$caps[$start+$j];$im=[Drawing.Image]::FromFile((Join-Path $out $c.path));$x=($j%4)*480;$y=[math]::Floor($j/4)*300;$g.DrawString($c.title,$font,[Drawing.Brushes]::Navy,$x+8,$y+5);$r=[math]::Min(464/$im.Width,260/$im.Height);$g.DrawImage($im,[single]($x+8),[single]($y+30),[single]($im.Width*$r),[single]($im.Height*$r));$im.Dispose()};$canvas.Save("$out/ppt-build/contact-$([int]($start/16)+1).png",[Drawing.Imaging.ImageFormat]::Png);$font.Dispose();$g.Dispose();$canvas.Dispose()}
Write-Output "Imported $($caps.Count) views, original pixels retained except explicit settings privacy crop."
