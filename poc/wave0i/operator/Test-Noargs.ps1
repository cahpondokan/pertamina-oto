$ErrorActionPreference='Stop'
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Invoke-Noargs.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'SYNTAX'}
$definition=$ast.Find({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Read-Sessions'},$true)
if($null -eq $definition){throw 'FUNCTION_MISSING'}
# Execute only this pure JSON reader, not the operator or CLI.
. ([scriptblock]::Create($definition.Extent.Text))
$count=0
$empty=Read-Sessions '{"WindowsSandboxEnvironments":[]}'
if($empty.WindowsSandboxEnvironments.Count -ne 0){throw 'EMPTY_LIST'};$count++
$one=Read-Sessions '{"WindowsSandboxEnvironments":[{"Id":"00000000-0000-0000-0000-000000000001"}]}'
if($one.WindowsSandboxEnvironments.Count -ne 1){throw 'SINGLE_LIST'};$count++
foreach($bad in @('null','{}','{"WindowsSandboxEnvironments":null}','{"WindowsSandboxEnvironments":{}}','{"WindowsSandboxEnvironments":[],"other":1}','{"WindowsSandboxEnvironments":[{}]}','{"WindowsSandboxEnvironments":[{"Id":"bad"}]}','{"WindowsSandboxEnvironments":[{"Id":"00000000-0000-0000-0000-000000000001"},{"Id":"00000000-0000-0000-0000-000000000001"}]}')){
    $rejected=$false
    try{$null=Read-Sessions $bad}catch{$rejected=$true}
    if(-not $rejected){throw 'NEGATIVE_ACCEPTED'};$count++
}
$null=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Guest-Noargs.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'GUEST_SYNTAX'};$count++
Write-Output "NOARGS_STATIC_TESTS=$count; PASS=$count; CLI_EXECUTED=false"
