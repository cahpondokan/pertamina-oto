Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
function Export-SafeTrx([string]$Path) {
    [xml]$raw = Get-Content -Raw -LiteralPath $Path
    $counter = $raw.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
    if ($null -eq $counter) { throw 'TRX_ERROR=counters_missing' }
    $v = [ordered]@{}
    foreach ($key in @('total','executed','passed','failed','error','aborted')) {
        $n = 0
        if (-not [int]::TryParse($counter.GetAttribute($key),[ref]$n) -or $n -lt 0) { throw 'TRX_ERROR=invalid_counter' }
        $v[$key] = $n
    }
    $attributes = ($v.GetEnumerator() | ForEach-Object { '{0}="{1}"' -f $_.Key,$_.Value }) -join ' '
    [IO.File]::WriteAllText($Path,"<TestRun><ResultSummary><Counters $attributes /></ResultSummary></TestRun>`n")
    if ($v.total -le 0 -or $v.executed -le 0 -or $v.passed -ne $v.executed -or ($v.failed+$v.error+$v.aborted) -ne 0) { throw 'TRX_ERROR=zero_or_failed_project' }
}
function Write-Phase1Manifest([string]$Root) {
    $s = Get-Content -Raw (Join-Path $Root 'pipeline-summary.json') | ConvertFrom-Json
    $files = @(Get-ChildItem $Root -File -Recurse | Where-Object Name -ne 'manifest.json' | Sort-Object FullName | ForEach-Object {
        [ordered]@{ path=[IO.Path]::GetRelativePath($Root,$_.FullName).Replace('\','/'); bytes=$_.Length; sha256=(Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
    })
    [ordered]@{ schemaVersion=2; runId=$s.runId; commit=$s.commit; taskIds=$s.taskIds; decisionIds=$s.decisionIds; status=$s.status; files=$files } | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $Root 'manifest.json') -Encoding utf8NoBOM
}
function Assert-Phase1Manifest([string]$Root,[string]$RunId,[string]$Commit) {
    $m = Get-Content -Raw (Join-Path $Root 'manifest.json') | ConvertFrom-Json
    $s = Get-Content -Raw (Join-Path $Root 'pipeline-summary.json') | ConvertFrom-Json
    if ($m.schemaVersion -ne 2 -or $m.runId -cne $RunId -or $m.commit -cne $Commit -or $s.runId -cne $RunId -or $s.commit -cne $Commit -or $m.status -cne $s.status) { throw 'MANIFEST_ERROR=binding' }
    foreach ($field in @('taskIds','decisionIds')) { if (@($m.$field).Count -eq 0 -or ($m.$field -join ',') -cne ($s.$field -join ',')) { throw 'MANIFEST_ERROR=ids' } }
    $actual = @(Get-ChildItem $Root -Recurse -File | Where-Object Name -ne 'manifest.json')
    if ($actual.Count -ne @($m.files).Count) { throw 'MANIFEST_ERROR=file_count' }
    $seen = @{}
    foreach ($e in $m.files) {
        if ($e.path -match '(^/|\\|:|(^|/)\.\.(/|$))' -or $seen.ContainsKey($e.path)) { throw 'MANIFEST_ERROR=path' }
        $seen[$e.path] = $true
        $path = Join-Path $Root $e.path
        $item = Get-Item -LiteralPath $path
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'MANIFEST_ERROR=reparse_point' }
        if ($item.Length -ne $e.bytes -or (Get-FileHash $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $e.sha256) { throw 'MANIFEST_ERROR=tamper' }
    }
}
