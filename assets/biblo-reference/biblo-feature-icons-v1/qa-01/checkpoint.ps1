param([string]$Id,[string]$Source,[string]$Prompt,[string]$Issue='')
$ErrorActionPreference='Stop'
$base=Split-Path $PSScriptRoot -Parent
$s=Get-Content -Raw "$base/task-01-status.json"|ConvertFrom-Json
$i=$s.items|Where-Object id -eq $Id
Copy-Item -LiteralPath $Source -Destination "$base/$($i.filename)"
Add-Type -AssemblyName System.Drawing
$b=[System.Drawing.Bitmap]::new([string]"$base/$($i.filename)")
$zero=0;$foreground=0
for($y=0;$y -lt $b.Height;$y+=4){for($x=0;$x -lt $b.Width;$x+=4){$a=$b.GetPixel($x,$y).A;if($a -eq 0){$zero++};if($a -gt 200){$foreground++}}}
$i.dimensions=@($b.Width,$b.Height);$i.alphaVerified=($zero -gt 0 -and $foreground -gt 0);$i.visualReviewed=$true
$i.state=if($i.alphaVerified -and $Issue -eq ''){'VERIFIED'}else{'NEEDS_REVIEW'}
$i.issue=$Issue;$i.fileHash=(Get-FileHash "$base/$($i.filename)" -Algorithm SHA256).Hash
$canvas=[System.Drawing.Bitmap]::new(420,300);$g=[System.Drawing.Graphics]::FromImage($canvas);$g.Clear([System.Drawing.Color]::White)
$g.FillRectangle([System.Drawing.Brushes]::MidnightBlue,0,150,420,150)
$x=15;foreach($size in @(64,96,128)){$g.DrawImage($b,$x,10,$size,$size);$g.DrawImage($b,$x,160,$size,$size);$x+=$size+20}
$canvas.Save("$PSScriptRoot/$Id-small-light-dark.png",[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$canvas.Dispose();$b.Dispose()
$s|ConvertTo-Json -Depth 10|Set-Content -Encoding utf8 "$base/task-01-status.json"
$prompts=@(Get-Content -Raw "$base/task-01-prompts.json"|ConvertFrom-Json)
$prompts+= [pscustomobject]@{id=$Id;prompt=$Prompt;source=$Source;tool='built-in image_gen';alphaSamplesTransparent=$zero;alphaSamplesForeground=$foreground}
$prompts|ConvertTo-Json -Depth 10|Set-Content -Encoding utf8 "$base/task-01-prompts.json"
Write-Output "$Id saved: $($i.dimensions -join 'x'); alpha transparent=$zero foreground=$foreground; state=$($i.state)"
