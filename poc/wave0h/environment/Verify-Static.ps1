$ErrorActionPreference='Stop'
$path = Join-Path $PSScriptRoot 'benign-observation.wsb'
$settings=[Xml.XmlReaderSettings]::new()
$settings.DtdProcessing=[Xml.DtdProcessing]::Prohibit
$settings.XmlResolver=$null
$reader=[Xml.XmlReader]::Create($path,$settings)
try { $xml=[Xml.XmlDocument]::new(); $xml.XmlResolver=$null; $xml.Load($reader) } finally { $reader.Dispose() }
$expected=@{Networking='Disable';ClipboardRedirection='Disable';PrinterRedirection='Disable';AudioInput='Disable';VideoInput='Disable';vGPU='Disable';ProtectedClient='Enable';MemoryInMB='4096'}
if ($xml.DocumentElement.Name -cne 'Configuration' -or $xml.DocumentElement.SelectNodes('*').Count -ne 9) { throw 'CONFIG_SHAPE' }
foreach($key in $expected.Keys) {
    $nodes=$xml.SelectNodes('/Configuration/'+$key)
    if($nodes.Count -ne 1 -or $nodes[0].InnerText -cne $expected[$key]) { throw 'CONTROL_MISMATCH' }
}
if($xml.SelectNodes('//MappedFolders|//MappedFolder').Count -ne 0) { throw 'HOST_MAPPING' }
$command=[string]$xml.Configuration.LogonCommand.Command
$prefix='powershell.exe -NoLogo -NoProfile -NoExit -EncodedCommand '
if(-not $command.StartsWith($prefix,[StringComparison]::Ordinal)) { throw 'COMMAND_SHAPE' }
$decoded=[Text.Encoding]::Unicode.GetString([Convert]::FromBase64String($command.Substring($prefix.Length)))
$source=[IO.File]::ReadAllText((Join-Path $PSScriptRoot 'Observe-Guest.ps1'))
if($decoded.Replace("`r`n","`n") -cne $source.Replace("`r`n","`n")) { throw 'SCRIPT_BINDING' }
$tokens=$null; $errors=$null
$null=[Management.Automation.Language.Parser]::ParseInput($decoded,[ref]$tokens,[ref]$errors)
if($errors.Count -gt 0) { throw 'SCRIPT_SYNTAX' }
if($decoded -match 'Start-Process|Invoke-WebRequest|Restart-Computer|Set-Content|Get-Credential|Get-ChildItem') { throw 'UNEXPECTED_OPERATION' }
Write-Output 'STATIC=PASS; CONTROLS=8; HOST_MAPPINGS=0; SCRIPT_BINDING=PASS; SCRIPT_SYNTAX=PASS; GUEST_EXECUTED=false'
