param([ValidateRange(1024,65535)][int]$Port = 5266)
$ErrorActionPreference = 'Stop'
$previewBase = "http://127.0.0.1:$Port"
$checks = [System.Collections.Generic.List[object]]::new()
foreach ($route in @('/', '/favorit', '/alat/merge', '/alat/organize', '/panduan')) {
    $response = Invoke-WebRequest -Uri ($previewBase + $route) -TimeoutSec 15
    if ($response.StatusCode -ne 200) { throw "Route failed: $route" }
    if ($response.Headers['Content-Security-Policy'] -notmatch "connect-src 'none'") { throw 'Missing restrictive CSP' }
    if ($response.Headers['X-Content-Type-Options'] -ne 'nosniff') { throw 'Missing nosniff' }
    $checks.Add([pscustomobject]@{Check="GET $route with privacy headers";Status='PASS'})
}
foreach ($route in @('/alat/edit','/alat/not-a-tool','/api/process')) {
    $response = Invoke-WebRequest -Uri ($previewBase + $route) -SkipHttpErrorCheck -TimeoutSec 15
    if ($response.StatusCode -ne 404) { throw "Unexpected route enabled: $route" }
    $checks.Add([pscustomobject]@{Check="GET $route = 404";Status='PASS'})
}
$post = Invoke-WebRequest -Uri ($previewBase + '/') -Method Post -Body 'synthetic-test-only' -SkipHttpErrorCheck -TimeoutSec 15
if ($post.StatusCode -ne 405) { throw 'POST must be disabled in UI preview' }
$checks.Add([pscustomobject]@{Check='POST disabled (405)';Status='PASS'})
$assetFiles = Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot '../wwwroot') -File -Recurse
foreach ($asset in $assetFiles) {
    $relativeAsset = [System.IO.Path]::GetRelativePath((Join-Path $PSScriptRoot '../wwwroot'), $asset.FullName).Replace('\','/')
    $response = Invoke-WebRequest -Uri ($previewBase + '/' + $relativeAsset) -Method Head -TimeoutSec 15
    if ($response.StatusCode -ne 200) { throw "Missing asset: $relativeAsset" }
    $checks.Add([pscustomobject]@{Check="HEAD /$relativeAsset";Status='PASS'})
}
$checks | ConvertTo-Json -Depth 3
