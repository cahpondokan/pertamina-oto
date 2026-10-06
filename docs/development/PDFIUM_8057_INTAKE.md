# PDFium 8057 controlled intake

Date: 2026-09-18. User explicitly authorized candidate download and testing. Status: ACQUIRED, NOT ADMITTED TO PRODUCT.

## Exact candidate

- Distributor: bblanchon/pdfium-binaries, release chromium/8057, published 2026-09-14.
- Asset: https://github.com/bblanchon/pdfium-binaries/releases/download/chromium/8057/pdfium-win-x64.tgz
- GitHub API asset digest matched downloaded SHA256: e307d519e42f2e69b1b531f0c2a32dffcdf3891ec0eba60328ba51a57cec01ed.
- Archive size: 3821600 bytes.
- DLL SHA256: 55e7ebef29a1ec9523d1adb8b260a73e7dfb0f64d3f0285121d20ecd6148ef18.
- Location: artifacts/pdfium-intake-8057, separate from all app builds. No installer run and no product package reference added.
- VERSION: 155.0.8057.0. args.gn: standalone release Windows x64, V8=false, XFA=false, partition allocator=false.
- Archive entry names checked for absolute paths and parent traversal before extraction. This is not a complete archive security audit.
- Authenticode: NotSigned. Digest proves correspondence to distributor asset metadata, not independent reproducible-build provenance or malware clearance.

## Source and security evidence

Release declares branch chromium/8057. Branch endpoint returned commit a5a7089234f121990b336b3841008009dca143bf. This is source declaration/correspondence evidence, not independently reproduced binary attestation.

Source query: https://pdfium.googlesource.com/pdfium/+log/refs/heads/chromium/8057?format=JSON&n=300

The 300-entry branch history contained these commits matching the bug identifiers cited in the previously examined 17 September Chrome advisory:

- 0edc2c51443c64b3589c2a919a0c1076c9d5547b: Improve error handling in libjpeg encoding; Bug 556853443.
- cae4b1b6b08628f43d3dcfe8d722cbea8f9064b0: Copy MCC record count in OpenJPEG tile setup; Bug 541707261.

These matches are stronger evidence than version ordering, but not a comprehensive vulnerability assessment. Mapping of the earlier September advisory remains incomplete. No claim of absence of vulnerabilities is made.

## Licenses and remaining gate

Package MIT notice and core PDFium redistribution conditions read. The package includes 14 third-party/core notice files in licenses/. Full transitive license admission has NOT completed; retain all notices with any future distribution.

Native C# binding, actual text replacement/round-trip, font rejection, crash/cancel, independent render/extraction, and UI integration tests are NOT RUN against this candidate. DLL has not been executed this intake. Product Edit PDF remains disabled. No user PDFs touched. Existing design preview remains illustrative only.

Next: complete remaining security/license mapping, then isolated native synthetic tests before any product promotion. No further download authorization is needed for this exact acquired candidate; this record does not substitute for required human release/security approval.
