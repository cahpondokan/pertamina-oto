# Wave 0E-B independent security review

Agent 6; run P02-20260905-1434; 2026-09-07 Asia/Jakarta.
Baseline dd003ea20ec8e00b96c265907560f66957783a5d; branch task/P2-A6-wave0e-review. Allowed path poc/reviews/security/wave0e only. ZERO_ADDITIONAL_COST.

Recommendation: STATIC_INTAKE_VERIFIED_WITH_LIMITATIONS; BLOCKED_SECURITY for restore/build/component or document execution. This is a review recommendation, not human admission or a confirmed exploit finding. Exact byte integrity is verified; execution-freeze packet may be prepared but cannot be marked complete while the blockers below remain.

## Authority and reviewed evidence

Read AGENTS.md, Shared Agent Contract, Phase 2 prompt, Project State, Decision Register and Wave 0E task card. Explicit Wave 0E-B dispatch specifies dd003ea, superseding the earlier Wave 0E-A checkpoint for this review. Agent 0 subsequently assigned this worktree/branch at exact dd003ea. Read tracked dependency-artifacts README/results/inventories/signature logs/inspection script and actual archive target XML; review qpdf blocker report only. Do not rerun Inspect-Artifacts.ps1: it writes outside this lane and performs signature commands. The independent script here performs only byte/ZIP/PE/XML reads and emits JSON.

Trace: FEAT-PDF-005; TECH-DEC-005; TECH-DEC-006; P2-W0-DEP-002; P2-W0-QPDF-ACQ-001; P2-D01; P2-D03; PRE-004; PRE-005; PRE-006; PRE-008; PRE-009; PRE-010; PRE-011; PRE-012; PDF001-E00/E04/E05/E08/E09/E10/E11/E12.

## Reproducible verification

```text
python -B poc/reviews/security/wave0e/verify_static.py <approved-agent-2-dependency-intake-root> poc/intake/dependency-artifacts
```

Executed with installed Python standard library; exit 0. Output static-evidence.json. No extraction, assembly/analyzer/generator loading, PDF parsing, MSBuild evaluation, network request, restore or build. All three exact hashes are hard-coded in verifier; it compares SHA512 with frozen acquisition catalog assertion, every entry size/hash with tracked intake, ZIP CRC reads, duplicate case-folded paths, traversal/root/colon/backslash/trailing-dot-space/symlink checks, selected net8.0 dependency records, build XML tags and PE import directories. Reads all DLLs as bytes, including localized resources; no runtime is initialized from them. Hashes repeated after inspection.

| Exact package | Bytes | SHA256 before = after | Entries |
|---|---:|---|---:|
| Microsoft.Extensions.Logging.Abstractions 8.0.3 | 802188 | E4C498D5A13051B4577A148F1D8C3470167215C507E2392069B75DC61322BB74 | 67 |
| System.Security.Cryptography.Pkcs 8.0.1 | 1078389 | 28C348909DF243FE4EE962213E3C8021AAECBC8321929DBA03A69BCB92A49DE5 | 31 |
| Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2 | 278862 | 51F2DF1100245F10DA54F0BB7E813F277155117777D4FBBAB902214E27372606 | 26 |

124/124 entries match tracked inventory. SHA512 catalog comparisons PASS for all three. These authenticate consistency with frozen intake, not independent source-build reproducibility. Exact-package checks are not a generic hostile archive validator. PE ordinary import directories list mscoree.dll; CLR metadata P/Invoke (ImplMap), delay imports, reflection/computed names, AssemblyRef closure and IL call graphs are not decoded. Therefore do not interpret the import list as complete native/runtime closure. String matches are vocabulary evidence only, not reached methods or actual network activity.

## Findings by severity / admission impact

### High — E-SEC-01: build-time activation must be explicitly admitted

Logging contains generator DLLs for Roslyn 3.11, 4.0 and 4.4, plus resources. Byte indicators include ISourceGenerator (3.11) and IIncrementalGenerator (4.x). Three buildTransitive target variants gather package analyzers, remove Roslyn 4.x on older capability detection, or remove analyzers when DisableMicrosoftExtensionsLoggingSourceGenerator is true. They hook ResolvePackageDependenciesForBuild/ResolveNuGetPackageAssets. Acquisition is not approval to run them in a compiler process.

All inspected target XML has no Import, UsingTask or Exec element. DI and Pkcs targets contain InitialTargets compatibility Warning behavior; Logging also has unsupported-framework Warning targets. This narrows observed activation surface; it does not establish full build safety. No switch or warning suppression changed. Before any build, freeze exact asset/import/analyzer policy and prove actual resolved graph matches it. If excluding generators is proposed, review that configuration and its functional effect explicitly; do not silently disable a control to obtain PASS. Selected Logging->DI nuspec carries exclude=Build,Analyzers, but that is not proof that Logging's own generators or every transitive build asset is inactive.

### High — E-SEC-02: trust freshness and vulnerability disposition incomplete

All three tracked signature logs report Successfully verified/exit 0 under NUGET_CERT_REVOCATION_MODE=offline, AND RevocationStatusUnknown for author and repository chains. This review rehashed artifacts and read logs; it did not repeat cryptographic verification or contact CRL/OCSP. The logs support their recorded outcome only; unknown revocation cannot become a fresh non-revoked assertion. Timestamp/certificate dates and signatures are not malware or vulnerability clearance.

No fresh scanner/advisory/affected-version evidence is in this review. Scanner and current vulnerability assessment NOT_RUN; no CVE-free claim and no invented vulnerability. Before execution obtain approved artifact-bound trust/revocation and current advisory/scanner disposition or explicit qualified human treatment of missing evidence. No paid scanner, upload, security-setting mutation or trust-policy bypass is authorized. Metadata-network verification, if separately allowed later, must be distinguished from zero-egress processing.

### High — E-SEC-03: Pkcs platform/network surface requires selected-runtime review

Pkcs runtimes/win/lib/net6.0, net7.0, net8.0 byte vocabulary includes crypt32.dll, BCrypt.dll, ncrypt.dll, kernel32.dll, DllImport/LibraryImport/Marshal, IntPtr/Unsafe, and HttpClient/Socket/WebRequest. Generic Pkcs assets also have some network vocabulary. These can represent metadata/resources/unreached paths; no allegation that Merge uploads documents is justified. Ordinary PE mscoree import alone misses managed interop. Freeze exact Windows RID asset and guest OS crypto/runtime identity; review reachable call/interop paths and later prove default-deny network with attempted-egress observation for the full process tree. No telemetry/certificate-fetch exception should silently weaken local processing invariants.

Logging and DI assets contain DllImport/Marshal vocabulary in some TFMs without proof of actual native invocation. This reinforces that string absence/presence is insufficient. No process-launch/dynamic-load/telemetry pattern matched this narrow scan; absence is not a security guarantee.

### Medium — E-SEC-04: package closure is consistent, deployment closure unresolved

Actual selected net8.0 nuspec confirms Logging->DI 8.0.2; Pkcs and DI selected groups are empty. No selected package/version surprise. Other framework groups remain non-authorized; do not union them or download extra dependencies. Native DLL names in Pkcs vocabulary are OS dependency indicators, not new package authority. Runtime/framework packs, RID selection, analyzer support assemblies, package asset pruning and external/OS closure still require explicit manifest/SBOM and later controlled validation. No product graph changed and no restore result exists.

### High — E-SEC-05: qpdf oracle intake remains blocked by approval review

At baseline poc/oracles/qpdf-12.4.1 records BLOCKED_BY_APPROVAL_REVIEW before process creation. No artifact/time/size/redirect observation, local hash, inventory, exact bundled license, signature or import evidence exists from that task. Expected hash is remote metadata only. No qpdf download attempted in this review. Do not try another downloader/host/tool as a bypass. Agent 0 must resolve the trusted authorization issue through the normal approval channel for the exact ZIP/URL/hash/acquisition-only scope. This is an acquisition authorization blocker, not evidence of qpdf vulnerability. Poppler/MuPDF must not silently replace it; independent real PDF oracle gate remains open.

## Next admission recommendation

Accept these three byte/inventory verification records into the intake evidence index after Agent 0 audit and Agent 7 license/notices review. Retain STATIC_INTAKE_VERIFIED_WITH_LIMITATIONS; do not infer APPROVED_ENGINE or executable admission.

Prepare a next packet that names exact package hashes and TFM/RID/runtime, reviewed build/analyzer activation policy, minimal asset closure/SBOM, qualified trust/revocation and vulnerability/scanner treatment, independent oracle status and every remaining environment/evidence gate. Restore/build permission must be an explicit later bounded decision: package targets/analyzers can execute before any PDF is opened. No such permission is given here.

For processing, freeze exact isolated guest and no-network controls, supervisor/cancel/timeout/process-tree policy, approved safety ceilings, corpus/expected properties, independent validator, evidence export/sanitization/access/retention and execution authority. Existing Sandbox template and serial harness are not proof of controls. qpdf acquisition issue and lack of renderer cannot be counted as PASS. No fabricated numeric targets; measurements remain NOT_RUN.

Stop on any hash/source/graph mismatch, activation beyond frozen policy, new package/cost, sensitive output, unknown trust disposition, unresolved oracle, restart/launch/build request outside authority or main-host hostile processing. Agent 0 coordinates remediation; human authority owns admission/resumption/risk decisions.

## Handoff and limitations

Changes only: REVIEW.md, verify_static.py, static-evidence.json in poc/reviews/security/wave0e. Cost NONE; ZERO_ADDITIONAL_COST preserved. No restart/Sandbox/component/PDF execution, no acquisition or paid service, no product/reference/central-record changes or main merge. Build/performance tests NOT_RUN by scope. Static verifier exit 0 both initial in-memory inspection and saved-script reproduction. Initial Git commands accidentally ran in documentation cwd and returned not-a-repository; corrected assigned-worktree commands verified exact baseline/branch and only review outputs. No failed check counted as PASS.

Recommended integration: Agent 0 audits this commit with Agent 7, binds exact input commit plus evidence, preserves qpdf blocked status, then presents bounded next decisions. Current findings are admission gaps and activation observations, not confirmed malicious behavior.
