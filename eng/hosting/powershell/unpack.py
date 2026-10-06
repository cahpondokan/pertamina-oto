#!/usr/bin/env python3
"""Verify and unpack the user-owned upload before any sudo invocation."""
import hashlib
import pathlib
import shutil
import sys
import tarfile

def unpack(archive, destination, expected):
    archive, destination = pathlib.Path(archive).absolute(), pathlib.Path(destination).absolute()
    if destination.name != 'contents' or destination.parent != archive.parent:
        raise ValueError('Unexpected extraction target')
    for item in (archive, *archive.parents, destination):
        if item.is_symlink():
            raise ValueError('Symlink extraction target')
    checksum = hashlib.sha256()
    with archive.open('rb') as source:
        for block in iter(lambda: source.read(1024 * 1024), b''):
            checksum.update(block)
    if checksum.hexdigest() != expected:
        raise ValueError('Upload checksum mismatch')
    with tarfile.open(archive, 'r:gz') as source:
        members = source.getmembers()
        seen, total = set(), 0
        if not 0 < len(members) <= 15000:
            raise ValueError('Invalid member count')
        for member in members:
            name = pathlib.PurePosixPath(member.name)
            total += member.size
            if (name.is_absolute() or '..' in name.parts or '\\' in member.name or ':' in member.name
                    or not (member.isfile() or member.isdir()) or str(name) in seen
                    or total > 2 * 1024 ** 3 or member.size < 0):
                raise ValueError('Unsafe archive member')
            seen.add(str(name))
        destination.mkdir(exist_ok=True, mode=0o700)
        for member in members:
            target = destination.joinpath(*pathlib.PurePosixPath(member.name).parts)
            for item in (target, *target.parents):
                if item.is_symlink():
                    raise ValueError('Symlink inside output')
            if member.isdir():
                target.mkdir(parents=True, exist_ok=True)
            else:
                target.parent.mkdir(parents=True, exist_ok=True)
                with source.extractfile(member) as input_file, target.open('wb') as output_file:
                    shutil.copyfileobj(input_file, output_file, 1024 * 1024)
                if target.stat().st_size != member.size:
                    raise ValueError('Incomplete extraction')
                target.chmod(0o600)

if __name__ == '__main__':
    if len(sys.argv) != 4:
        sys.exit('Expected archive, destination, SHA256')
    unpack(*sys.argv[1:])
    print('PASS upload checksum and bounded extraction')
