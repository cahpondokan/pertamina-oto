# Wave0K bounded PDFsharp compatibility revision

Agent3, same baseline/task branch as export HANDOFF. Root reports first actual AB guest observer exit32: qpdf version/observation stdout available, stderr empty, result absent. Reported actual JSON has catalog /Metadata and page /Group additions. This is a controlled source revision for a fresh stage/run, not retroactive PASS or blanket suppression. Agent3 did not read/process PDF, run engine, or alter prior evidence. Root alone owns new stage, independent review and execution.

New observe.py copies historical wave0i observer with exactly two optional structural extensions:

- Catalog /Metadata must be an indirect stream with exactly dict/data, exactly /Type=/Metadata and /Subtype=/XML. Data strict base64, nonempty UTF8 <=8192 decoded bytes, no NUL or case-insensitive literal <!DOCTYPE / <!ENTITY declarations. No XML parser, external resolution, rendering or semantic validation is performed. Arbitrary inert UTF8 may satisfy this metadata-only boundary: METADATA_NOT_ASSESSED. No active-content or XML validity claim.
- Each optional /Group must be a direct object with exactly /CS=/DeviceRGB and /S=/Transparency. Indirect groups, extra keys, alternate values rejected. This is compatibility recognition, not transparency/fidelity validation.

All catalog unknown keys, page whitelist, ordered unique flat page tree, media geometry, single content stream, exact Helvetica font and entire marker text grammar remain unchanged. Metadata is never considered page evidence. Case marker/count expectations unchanged. DTD rejection is only an inert-text additional guard, not a general security detector. Unreferenced objects/security/fidelity remain unassessed as before.

Tests: existing28 observer tests copied unchanged plus9 compatibility tests =37 PASS, 0.094sec, exit0, command `C:\Python314\python.exe -B -m unittest discover -s poc/wave0k/oracle -p 'test_*.py' -v`. New tests include all4 cases, bad/indirect/extra group, unknowncatalog, metadata type/filter/reference/base64/UTF8/size/NUL/DTD/entity, wrong markers despite matching metadata, explicit non-XML semantic limitation. Popen mocked/forbidden, JSON/text only.

Consumer fixed relative trusted source now wave0k/oracle/observe.py; tests import that revision. Existing17 consumer tests PASS 0.739sec, exit0. Root must freeze NEW observer SHA and expected SHA; old source and existing rejected raw evidence must remain historical. Source/test success does not itself accept actual output or authorize engine admission.

Files new: observe.py, test_observe.py, test_compatibility.py, this handoff. Existing lane changes: ../export/consume.py, test_consume.py, HANDOFF.md. No commit, no cost, no acquisition. Integration order: independent source review, hash-identical new staging, frozen manifest/expected, fresh guest execution, disposal, exact export consumption and operator-ledger review. Runtime authenticity still not proven by JSON alone; output hash guest asserted; fidelity NOT_ASSESSED.
