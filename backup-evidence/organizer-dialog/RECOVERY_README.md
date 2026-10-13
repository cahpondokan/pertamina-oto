# Verified development package recovery

This is a small recovery overlay, not a standalone installer. The final local package is still complete and unchanged. Almost all runtime files are identical to the previous verified backup. Keeping that complete base plus this overlay avoids uploading the same runtime repeatedly.

Keep BOTH private draft backups:

1. Base `OTO-20261013-114257-465ae8c8.zip`, GitHub asset ID 635962911, in https://github.com/cahpondokan/pertamina-oto/releases/tag/untagged-fc63c8b9716b0824d57c . Base SHA256: DF59A98415EA9BA77B5ABAFA5A3EF5501345BE8F9C6E3EF0D7FB9992D0A62BC8.
2. This recovery archive for build 20261013-142328-c5c96e5f. Obtain its SHA256 from the GitHub asset digest or verified completion report.

Download both while signed into the private repository. Extract only Restore-Backup.ps1 from this archive. Run it with -BaseZip, -RecoveryZip, -ExpectedRecoverySha256 and a NEW -Output directory. It rejects existing output, checks both archive identities, safely reconstructs app/ and manifest.json, and checks every final file. It does not install or change the active application.

Final product: 0.3.0 / Pengembangan. Final manifest SHA256: 2C2C71B84813A980810A9B5435DA71BF7C859D8FE4C285E147B870F4D33A6C4C. Expected final payload count: 1336. The five overlaid files are desktop DLL, PDB, PRI, unchanged original Biblo 031 image, and build-info.json. Source, documentation and all original Biblo collections are independently preserved on backup/v0.3.0-20261013-organizer-dialog.

Do not delete the complete base while this recovery archive depends on it. Use the established verified local installer/update workflow for any later activation; this backup does not claim Production readiness.
