$ErrorActionPreference='Stop'
$base='http://127.0.0.1:5266'
$homepageResponse=Invoke-WebRequest $base
# Catalog-only preview renders the active browser tools in .home-active and unavailable tools in .home-upcoming.
$visibleMain=[regex]::Match($homepageResponse.Content,'(?s)<main\b.*?</main>').Value
if(-not $visibleMain){throw 'Rendered main page missing'}
$active=[regex]::Matches($visibleMain,'<article class="tool-card(?: featured| )?" data-id=').Count
$inactive=[regex]::Matches($visibleMain,'class="tool-card planned"').Count
if($active -ne 10 -or $inactive -ne 8){throw 'Incorrect activation catalog'}
if($homepageResponse.Content -match 'class="sidebar"'){throw 'Navigation sidebar returned'}
foreach($route in @('merge','organize','split','remove','extract','rotate','images-pdf','numbers','watermark','crop')){
 $r=Invoke-WebRequest "$base/alat/$route"
 if($r.StatusCode -ne 200 -or -not $r.Content.Contains('data-browser-tool="' + $route + '"')){throw "Workspace invalid: $route"}
}
$post=Invoke-WebRequest "$base/alat/merge" -Method Post -Body 'SYNTHETIC-NON-DOCUMENT' -SkipHttpErrorCheck
if($post.StatusCode -ne 405){throw 'POST must be rejected'}
$notReady=Invoke-WebRequest "$base/alat/ocr" -SkipHttpErrorCheck
if($notReady.StatusCode -ne 404){throw 'Unimplemented tool exposed'}
$worker=Invoke-WebRequest "$base/js/pdf-worker.js"
$csp=$worker.Headers['Content-Security-Policy'] -join ''
if($csp -notmatch "connect-src 'none'" -or $csp -notmatch "worker-src 'self'"){throw 'Missing worker privacy CSP'}
[pscustomobject]@{active=$active;inactive=$inactive;routes='10 x 200';post=$post.StatusCode;inactiveRoute=$notReady.StatusCode;workerConnect='none';noNavigationSidebar=$true} | ConvertTo-Json
