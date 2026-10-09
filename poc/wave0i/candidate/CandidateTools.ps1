Set-StrictMode -Version Latest

function Get-BytesHash([byte[]] $Bytes) {
    [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($Bytes))
}

function Get-SafePath([string] $Root, [string] $Relative) {
    if ([string]::IsNullOrWhiteSpace($Relative) -or $Relative -notmatch '^[A-Za-z0-9_.-]+(/[A-Za-z0-9_.-]+)*$' -or
        @($Relative.Split('/') | Where-Object { $_ -in '.', '..' }).Count) { throw 'UNSAFE_RELATIVE_PATH' }
    $base = [IO.Path]::GetFullPath($Root).TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    $result = [IO.Path]::GetFullPath([IO.Path]::Combine($base, $Relative))
    if (-not $result.StartsWith($base, [StringComparison]::OrdinalIgnoreCase)) { throw 'OUTSIDE_ROOT' }
    Assert-NoReparse $result
    $result
}

function Assert-NoReparse([string] $Path) {
    if ($Path.IndexOfAny([char[]]@('"', [char]13, [char]10)) -ge 0) { throw 'UNSAFE_PATH_CHARACTERS' }
    $cursor = [IO.Path]::GetFullPath($Path)
    if ($cursor.StartsWith('\\') -or $cursor -notmatch '^[A-Za-z]:\\') { throw 'LOCAL_DRIVE_REQUIRED' }
    while ($cursor) {
        if (Test-Path -LiteralPath $cursor) {
            if ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'REPARSE_PATH_REJECTED'
            }
        }
        $parent = [IO.Path]::GetDirectoryName($cursor)
        if ($parent -eq $cursor) { break }
        $cursor = $parent
    }
}

function Assert-Bytes([byte[]] $Bytes, $Record) {
    if ($Record.sha256 -notmatch '^[A-Fa-f0-9]{64}$' -or (Get-BytesHash $Bytes) -ne $Record.sha256) {
        throw 'HASH_MISMATCH'
    }
    if ($Record.PSObject.Properties.Name -contains 'bytes' -and $Bytes.LongLength -ne $Record.bytes) {
        throw 'SIZE_MISMATCH'
    }
}

function Read-BoundFile([string] $Path, $Record) {
    Assert-NoReparse $Path
    if (-not [IO.File]::Exists($Path)) { throw 'MISSING_FILE' }
    $bytes = [IO.File]::ReadAllBytes($Path)
    Assert-Bytes $bytes $Record
    return ,$bytes
}

function Write-NewBytes([string] $Path, [byte[]] $Bytes) {
    Assert-NoReparse $Path
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($Path)) | Out-Null
    $file = [IO.File]::Open($Path, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write, [IO.FileShare]::None)
    try { $file.Write($Bytes, 0, $Bytes.Length) } finally { $file.Dispose() }
}

function Write-NewText([string] $Path, [string] $Text) {
    Write-NewBytes $Path ([Text.Encoding]::UTF8.GetBytes($Text))
}

function Read-ZipMember($Archive, [string] $Name, $Record) {
    $entries = @($Archive.Entries | Where-Object { $_.FullName.Equals($Name, [StringComparison]::OrdinalIgnoreCase) })
    if ($entries.Count -ne 1 -or $entries[0].FullName -cne $Name) { throw 'MISSING_OR_DUPLICATE_MEMBER' }
    if ($entries[0].Length -le 0 -or $entries[0].Length -gt 16MB) { throw 'MEMBER_SIZE_REJECTED' }
    $inputStream = $entries[0].Open()
    $memory = [IO.MemoryStream]::new()
    try {
        $buffer = [byte[]]::new(8192)
        while (($count = $inputStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
            if ($memory.Length + $count -gt 16MB) { throw 'MEMBER_EXPANSION_REJECTED' }
            $memory.Write($buffer, 0, $count)
        }
        $bytes = $memory.ToArray()
        if ($bytes.LongLength -ne $entries[0].Length) { throw 'MEMBER_LENGTH_MISMATCH' }
        if ($null -ne $Record) { Assert-Bytes $bytes $Record }
        return ,$bytes
    } finally { $inputStream.Dispose(); $memory.Dispose() }
}

function Test-Package($Package, [string] $EvidenceRoot, [string] $AppDirectory = '', [string] $NoticeDirectory = '') {
    $path = Get-SafePath $EvidenceRoot $Package.path
    $archiveBytes = Read-BoundFile $path $Package
    $memory = [IO.MemoryStream]::new($archiveBytes, $false)
    $zip = [IO.Compression.ZipArchive]::new($memory, [IO.Compression.ZipArchiveMode]::Read)
    try {
        foreach ($member in $Package.members) {
            $bytes = Read-ZipMember $zip $member.path $member
            if ($AppDirectory) {
                $destination = Get-SafePath $AppDirectory ([IO.Path]::GetFileName($member.path))
                Write-NewBytes $destination $bytes
            }
        }
        if ($Package.id -ne 'PDFsharp') {
            foreach ($name in @('LICENSE.TXT', 'THIRD-PARTY-NOTICES.TXT')) {
                $bytes = Read-ZipMember $zip $name $null
                if ($NoticeDirectory) {
                    Write-NewBytes (Get-SafePath $NoticeDirectory ($Package.id + '/' + $name)) $bytes
                }
            }
        }
    } finally { $zip.Dispose(); $memory.Dispose() }
}

function New-CompilerArguments([string] $Compiler, [string] $Source, [string] $Output, [string[]] $References) {
    if ($References.Count -eq 0) { throw 'EMPTY_REFERENCES' }
    $argsList = [Collections.Generic.List[string]]::new()
    foreach ($item in @('exec', '--fx-version', '10.0.11', $Compiler, '/noconfig', '/nostdlib+',
            '/target:exe', '/platform:x64', '/langversion:latest', '/deterministic+', '/optimize+', '/debug-',
            '/utf8output', ('/out:' + $Output))) { $argsList.Add($item) }
    foreach ($reference in $References) { $argsList.Add('/reference:' + $reference) }
    $argsList.Add($Source)
    $argsList.ToArray()
}

function New-RuntimeConfig {
    @{runtimeOptions=@{tfm='net10.0';framework=@{name='Microsoft.NETCore.App';version='10.0.11'};
        rollForward='Disable';configProperties=@{'System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization'=$false}}}
}

function New-CompilerResponse([string[]] $Arguments) {
    # First four arguments belong to dotnet exec; /noconfig must remain outside the response file.
    if ($Arguments.Count -lt 7 -or $Arguments[4] -ne '/noconfig') { throw 'INVALID_COMPILER_ARGUMENTS' }
    ($Arguments[5..($Arguments.Count - 1)] | ForEach-Object {
        if ($_.IndexOfAny([char[]]@('"', [char]13, [char]10)) -ge 0) { throw 'RESPONSE_INJECTION_REJECTED' }
        '"' + $_ + '"'
    }) -join [Environment]::NewLine
}

function New-AppDeps([string[]] $Libraries) {
    $runtimeAssets = [ordered]@{'MergeSmoke.dll'=@{}}
    foreach ($name in $Libraries) {
        if ($name -notmatch '^[A-Za-z0-9_.-]+\.dll$' -or $runtimeAssets.Contains($name)) { throw 'INVALID_RUNTIME_ASSET' }
        $runtimeAssets[$name] = @{}
    }
    @{runtimeTarget=@{name='.NETCoreApp,Version=v10.0';signature=''};compilationOptions=@{};
      targets=@{'.NETCoreApp,Version=v10.0'=@{'MergeSmoke/1.0.0'=@{runtime=$runtimeAssets}}};
      libraries=@{'MergeSmoke/1.0.0'=@{type='project';serviceable=$false;sha512=''}}}
}
