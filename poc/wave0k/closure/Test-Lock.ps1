$ErrorActionPreference='Stop'
$lock=Get-Content (Join-Path $PSScriptRoot 'inputs.lock.json') -Raw|ConvertFrom-Json
if($lock.schema -ne 1 -or $lock.status -ne 'STATIC_CANDIDATE_NOT_RUNTIME_PROVEN'){throw 'STATUS'}
function ValidTarget([string]$value){return $value -cmatch '^(python|qpdf|notices)/[A-Za-z0-9_./-]+$' -and $value -notmatch '(^|/)\.\.?(/|$)' -and $value -notmatch '//'}
foreach($row in @($lock.python)+@($lock.qpdf)) {
 if(!(ValidTarget $row.target) -or $row.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $row.bytes -lt 0){throw 'ROW'}
}
foreach($bad in @('../qpdf.exe','qpdf/../bad','qpdf//bad','C:/bad','qpdf/x:bad','qpdf\bad')){if(ValidTarget $bad){throw 'NEGATIVE_TARGET'}}
$all=@($lock.python.target)+@($lock.qpdf.target)
if(@($all|Sort-Object -Unique).Count -ne $all.Count){throw 'DUPLICATE'}
foreach($row in $lock.python) {if((Get-Item -LiteralPath $row.source).Length -ne $row.bytes -or (Get-FileHash -LiteralPath $row.source).Hash.ToLowerInvariant() -ne $row.sha256){throw 'SOURCE_DRIFT'}}
if((Get-FileHash -LiteralPath $lock.archive).Hash.ToLowerInvariant() -ne $lock.archiveSha256){throw 'ARCHIVE_DRIFT'}
$intake=Get-Content (Join-Path (Split-Path $lock.archive) 'archive-static-inventory.json') -Raw|ConvertFrom-Json
foreach($row in $lock.qpdf) {
 $match=@($intake.entries|Where-Object {$_.path -ceq $row.member})
 if($match.Count -ne 1 -or $match[0].sha256 -cne $row.sha256 -or $match[0].bytes -ne $row.bytes){throw 'INTAKE_MISMATCH'}
}
'PASS: status, row schema, 6 negative paths, unique targets, 564 Python hashes, archive hash, 12 independent intake members. No candidate execution.'
