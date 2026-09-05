# Safe Evidence Contract

Phase 0 validation writes generated evidence to `artifacts/evidence/<run-id>/`. The directory is runtime output and must remain excluded from source control.

`New-EvidenceManifest.ps1` creates a content-addressed `manifest.json` containing only:

- schema, run, task, decision, commit, and status identifiers;
- relative artifact names;
- byte counts and SHA-256 digests;
- a UTC generation timestamp.

The manifest never embeds test input, document content, credentials, absolute repository paths, or raw environment values. Pipeline logs replace known repository, user-profile, and temporary-directory prefixes before they are written. A manifest is referential evidence, not human approval and not Production attestation.

Remote retention, signing, and immutable object storage remain pending IT/Azure governance. Under `LOCAL-BOOT-001`, the approved local evidence root is `D:\OTO\pertamina-oto\artifacts\evidence`; this literal is documentation only and is not hard-coded into scripts.

`Test-EvidenceManifest.ps1` verifies the schema, commit binding, relative-path rule, and file digest in an isolated temporary fixture.
