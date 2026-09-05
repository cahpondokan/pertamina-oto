param([switch]$SelfTest)
$ErrorActionPreference = 'Stop'
function Assert-Template([string]$Text) {
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($Text), $settings)
    try {
        $doc = [System.Xml.XmlDocument]::new()
        $doc.XmlResolver = $null
        $doc.Load($reader)
    } catch { throw 'INVALID_XML' } finally { $reader.Dispose() }
    $expected = [ordered]@{
        Networking='Disable'; vGPU='Disable'; AudioInput='Disable'; VideoInput='Disable'
        PrinterRedirection='Disable'; ClipboardRedirection='Disable'; ProtectedClient='Enable'
    }
    $root = $doc.DocumentElement
    if ($root.Name -cne 'Configuration' -or $root.Attributes.Count -ne 0) { throw 'INVALID_ROOT' }
    $children = @($root.ChildNodes | Where-Object NodeType -eq Element)
    if ($children.Count -ne 8) { throw 'INVALID_SETTING_COUNT' }
    foreach ($key in $expected.Keys) {
        $nodes = @($root.SelectNodes($key))
        if ($nodes.Count -ne 1 -or $nodes[0].InnerText -cne $expected[$key] -or
            $nodes[0].Attributes.Count -ne 0 -or @($nodes[0].SelectNodes('*')).Count -ne 0) { throw 'INVALID_SETTING' }
    }
    $maps = @($root.SelectNodes('MappedFolders'))
    if ($maps.Count -ne 1 -or $maps[0].Attributes.Count -ne 0 -or $maps[0].SelectNodes('*').Count -ne 1) { throw 'INVALID_MAPPING' }
    $map = $maps[0].SelectSingleNode('MappedFolder')
    if ($null -eq $map -or $map.Attributes.Count -ne 0 -or $map.SelectNodes('*').Count -ne 3) { throw 'INVALID_MAPPING' }
    $fields = @{HostFolder='__APPROVED_READONLY_INPUT_ROOT__'; SandboxFolder='C:\PocInput'; ReadOnly='true'}
    foreach ($key in $fields.Keys) {
        $nodes = @($map.SelectNodes($key))
        if ($nodes.Count -ne 1 -or $nodes[0].InnerText -cne $fields[$key] -or
            $nodes[0].Attributes.Count -ne 0 -or $nodes[0].SelectNodes('*').Count -ne 0) { throw 'INVALID_MAPPING_FIELD' }
    }
}
$template = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'offline.wsb.template') -Raw
Assert-Template $template
if ($SelfTest) {
    $cases = @(
        $template.Replace('<Networking>Disable', '<Networking>Enable'),
        $template.Replace('<ReadOnly>true', '<ReadOnly>false'),
        $template.Replace('<ClipboardRedirection>Disable', '<ClipboardRedirection>Default'),
        $template.Replace('</Configuration>', '<LogonCommand><Command>blocked</Command></LogonCommand></Configuration>'),
        $template.Replace('</Configuration>', '<Networking>Disable</Networking></Configuration>'),
        $template.Replace('__APPROVED_READONLY_INPUT_ROOT__', 'C:\'),
        $template.Replace('<ProtectedClient>Enable', '<ProtectedClient>Disable'),
        $template.Replace('<AudioInput>Disable', '<AudioInput>Enable'),
        $template.Replace('<VideoInput>Disable', '<VideoInput>Enable'),
        $template.Replace('<PrinterRedirection>Disable', '<PrinterRedirection>Enable'),
        $template.Replace('<vGPU>Disable', '<vGPU>Enable'),
        $template.Replace('<Configuration>', '<!DOCTYPE Configuration [<!ENTITY x SYSTEM "file:///blocked">]><Configuration>'),
        $template.Replace('<ReadOnly>true</ReadOnly>', ''),
        $template.Replace('<Configuration>', '<Configuration extra="blocked">')
    )
    $passed = 1
    foreach ($case in $cases) {
        $rejected = $false
        try { Assert-Template $case } catch { $rejected = $true }
        if (-not $rejected) { throw 'NEGATIVE_CASE_ACCEPTED' }
        $passed++
    }
    Write-Output "DISCOVERED=$passed PASSED=$passed FAILED=0"
}
Write-Output 'TEMPLATE_STATIC_VALIDATION=PASS; LAUNCH_AUTHORIZED=false; NETWORK_PROVEN=false'

