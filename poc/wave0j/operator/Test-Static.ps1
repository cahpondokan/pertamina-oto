$ErrorActionPreference='Stop'
$tokens=$null; $errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Guest-Smoke.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count) { throw 'PARSE_FAIL' }
$function=$ast.Find({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Assert-Outcome'},$true)
Invoke-Expression $function.Extent.Text
$count=1
foreach($fixture in @(@(0,1,$true,$true),@(0,262144,$true,$true),@(0,0,$true,$false),@(0,262145,$true,$false),@(1,100,$true,$false),@(0,100,$false,$false))) {
    $accepted=$true
    try { Assert-Outcome $fixture[0] $fixture[1] $fixture[2] } catch {$accepted=$false}
    if($accepted -ne $fixture[3]) { throw 'OUTCOME_FIXTURE_FAIL' }
    $count++
}
Write-Output ('PASS '+$count+' syntax/pure outcome checks; no engine, file corpus, or Sandbox execution')
