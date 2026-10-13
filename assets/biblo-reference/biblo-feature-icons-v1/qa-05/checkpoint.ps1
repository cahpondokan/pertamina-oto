param([string]$Id,[string]$Source,[string]$Prompt,[string]$Issue='Small-size light/dark QA pending; full-size identity, direction, labels reviewed')
$root=Split-Path $PSScriptRoot -Parent
$s=Get-Content -LiteralPath (Join-Path $root 'task-05-status.json') -Raw|ConvertFrom-Json
$i=$s.items|Where-Object id -eq $Id
$dest=Join-Path $root $i.filename
if(Test-Path -LiteralPath $dest){throw 'Existing destination; preserve it'}
Copy-Item -LiteralPath $Source -Destination $dest
Add-Type -AssemblyName System.Drawing
$b=[System.Drawing.Bitmap]::FromFile($dest)
$i.state='NEEDS_REVIEW'
$i|Add-Member dimensions @($b.Width,$b.Height) -Force
$i|Add-Member alphaVerified ($b.GetPixel(0,0).A -eq 0 -and $b.GetPixel([int]($b.Width/2),[int]($b.Height/2)).A -gt 0) -Force
$i|Add-Member visualReviewed $true -Force
$i|Add-Member issue $Issue -Force
$i|Add-Member fileHash (Get-FileHash -LiteralPath $dest).Hash -Force
$b.Dispose()
$s|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $root 'task-05-status.json') -Encoding utf8
$p=@(Get-Content -LiteralPath (Join-Path $root 'task-05-prompts.json') -Raw|ConvertFrom-Json)
$p+=[pscustomobject]@{id=$Id;prompt=$Prompt;source=$Source}
$p|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $root 'task-05-prompts.json') -Encoding utf8
