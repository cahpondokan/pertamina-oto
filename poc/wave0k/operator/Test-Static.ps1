$ErrorActionPreference='Stop'
$tokens=$null; $errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Guest-Oracle.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count) { throw 'PARSE_FAIL' }
$fn=$ast.Find({param($n) $n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Assert-Bound'},$true)
Invoke-Expression $fn.Extent.Text
$count=1
foreach($fixture in @(@(0,$false,$false),@(0,$true,$true),@(1,$false,$true),@(1048576,$false,$true),@(1048577,$true,$false),@(-1,$true,$false))) {
 $accepted=$true; try { Assert-Bound $fixture[0] $fixture[1] } catch { $accepted=$false }
 if($accepted -ne $fixture[2]) { throw 'BOUND_FIXTURE' }; $count++
}
Write-Output ('PASS '+$count+' parse/pure-bound checks; no engine/PDF/Sandbox execution')
