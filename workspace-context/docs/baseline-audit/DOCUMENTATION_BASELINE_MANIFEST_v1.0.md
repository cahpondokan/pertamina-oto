# DOCUMENTATION BASELINE MANIFEST
## PERTAMINA OTO

> Baseline ID: `PERTAMINA-OTO-DOCS-BASELINE-1.0`  
> Tanggal: 2 September 2026  
> Status: **CANDIDATE — HUMAN APPROVAL REQUIRED**  
> Candidate tag: `docs-baseline-v1.0`  
> Feature authority: Dokumen 03 v1.3 — 550 Feature ID unik  
> Primary surface: 33 Primary PDF Tool Cards

---

# 1. Aturan Baseline

1. Tepat satu `Current=YES` untuk setiap Document ID.
2. Path, filename, version, ukuran, dan SHA-256 harus cocok.
3. Working file, agent output, prototype, audit input, dan archive bukan source of truth.
4. Divergent duplicate dengan filename/version sama harus diberi status superseded/archive.
5. `FINAL` hanya menyatakan status dokumen; bukan implementation/test/UAT/Production/handover status.
6. Setiap perubahan byte menghasilkan hash baru dan Change Control entry.
7. Tidak ada approval manusia yang dapat diberikan oleh Codex.

---

# 2. Current Canonical Set

| ID | Canonical path | Versi | Status | Bytes | SHA-256 | Current |
|---|---|---:|---|---:|---|---|
| `DOC-01` | `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` | 3.4 | FINAL | 49,603 | `D13DDC9CD3259BAD7C7889635DB44DDDCC47FB228A62CC3B8CCE203AC23E1E1B` | YES |
| `DOC-02` | `02_REQUIREMENT_SPECIFICATION_FINAL_v1.5_PERTAMINA_OTO.md` | 1.5 | FINAL | 57,634 | `24986DC7D4D369935CA7F8D382F6AB2AE88464E9F640AB5CFD726B7F761B1F11` | YES |
| `DOC-03` | `03_FEATURE_CATALOG_FINAL_v1.3_PERTAMINA_OTO.md` | 1.3 | FINAL | 187,018 | `B908B1E6E45D84AD26EBB4E8504F1B0FAA7AD96A6C85FB5C92EE0BC580429A45` | YES |
| `DOC-04` | `04_SOLUTION_DESIGN_FINAL_v1.2_PERTAMINA_OTO.md` | 1.2 | FINAL | 35,082 | `9C973809CE1083C4BC36EA87371BC31D5CAD4C95684531357B6ECC4665C31A5D` | YES |
| `DOC-05` | `05_TECHNICAL_REFERENCE_FINAL_v1.2_PERTAMINA_OTO.md` | 1.2 | FINAL | 19,641 | `A2D212402396F4DB26A62F3A1B756D4477988AF73A9D84F4171C35029E4854BD` | YES |
| `DOC-06` | `docs/06-api/06_API_DOCUMENTATION_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 62,104 | `A9D1DB2E327152C9944589286D49CFFB48CBD787C7921ACAB3CA8DAEDCAABDC3` | YES |
| `DOC-07` | `docs/07-database/07_DATABASE_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 63,514 | `4786BFC5C632F619DCFF89004C314202A39C84A66576CD8AD5C577B66B0D6738` | YES |
| `DOC-08` | `docs/08-security/08_SECURITY_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 81,859 | `DF9C8BA8B70B224206918096FC2ACBE990C452AA932F27871FB36B282F6F404F` | YES |
| `DOC-09` | `docs/09-role-matrix/09_ROLE_MATRIX_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 70,674 | `DA5F13B4C2E330D755D79C3F143FB270C75B046CFBDC438791ECD70EC8EDEC5B` | YES |
| `DOC-10` | `docs/10-audit-trail/10_AUDIT_TRAIL_DESIGN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 61,735 | `C2926933268E315625F943C2DA3C97648BBF91973089C7CF3D1AF81895AD57BE` | YES |
| `DOC-11` | `docs/11-unit-test/11_UNIT_TEST_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 55,205 | `AD1E9323961C64380C863795A2E20767D4EBA67999C4F28DEA9A0B578C596E14` | YES |
| `DOC-12` | `docs/12-test-plan/12_TEST_PLAN_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 82,038 | `A4449E43F877327FA645C3D02E1875958345F727DDC2FA576E88D7F8C419586D` | YES |
| `DOC-13` | `docs/13-uat/13_UAT_DOCUMENT_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL | 37,489 | `13C1495361FD65969BF5CB38D50E79DD99CE045DE03B6FF9FA24B75386B6DB51` | YES |
| `DOC-14` | `docs/14-user-manual/14_USER_MANUAL_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 35,964 | `15BE8D47A4FC7D7442488180C188B42E8369270F5E905F051F09D57B37D407EB` | YES |
| `DOC-15` | `docs/15-admin-manual/15_ADMIN_MANUAL_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 49,581 | `2B0285DF799539E028F611540EC6FAE65F1CEC8746A5137A21D9955AAEAA2520` | YES |
| `DOC-16` | `docs/16-incident-support/16_INCIDENT_SUPPORT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 27,948 | `CF821E9D9A6502921951858FDD54EE2DDD93CAB26FF07A76FB904B0DE66E8F85` | YES |
| `DOC-17` | `docs/17-deployment/17_DEPLOYMENT_GUIDE_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 28,053 | `D66287E4ABB41BFBD6D711F2A0A906D125716FDDBC457F3E238FFB623C04D03E` | YES |
| `DOC-18` | `docs/18-application-cartography/18_APPLICATION_CARTOGRAPHY_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 39,287 | `1EB8D28DE56016A92AE8609E1298791AB33174084C3A7C1B4C1D3F33F0FBAB83` | YES |
| `DOC-19` | `docs/19-handover/19_HANDOVER_CHECKLIST_FINAL_v1.0_PERTAMINA_OTO.md` | 1.0 | FINAL pra-implementasi | 27,362 | `7280D200F1A3F7A6F4A70DC2CE8F8D0913622822552C26954BA15C752DAD6ECC` | YES |
| `MASTER` | `MASTER_PROJECT_SUMMARY.md` | 4.3 | Current mutable index | 37,358 | `BC9306D583822E97E54F7610155B4FD8457B9A776EAAD29216255A8FD5AE4BAF` | YES — index, bukan domain authority |

---

# 3. Source of Truth Hierarchy

```text
Human-approved Pertamina policy / controlled user decision
  ↓
01 Business Process
  ↓
02 Requirement Specification
  ↓
03 Feature Catalog — identity/count/name/scope authority
  ↓
04 Solution Design — architecture authority
  ↓
05 Technical Reference — technology authority
  ↓
06 API → 07 Data → 08 Security → 09 Role → 10 Audit
  ↓
11 Unit → 12 Test Plan → 13 UAT
  ↓
14 User Manual → 15 Admin Manual → 16 Incident
  ↓
17 Deployment → 18 Cartography → 19 Handover
  ↓
MASTER_PROJECT_SUMMARY.md — current index/status only
```

Downstream document tidak boleh mengubah parent authority secara diam-diam. Matrix/register baru hanya index dan evidence layer.

---

# 4. Duplicate dan Supersession Disposition

| Asset | Status | Current replacement/disposition |
|---|---|---|
| `PERTAMINA_OTO_CODEX_HANDOFF_DOC06_2026-08-24/` D01–D05 | `SUPERSEDED_SNAPSHOT` — divergent same-version content; D02/D03 memuat 546 | Current root D01–D05 pada §2; jangan dipakai sebagai baseline aktif |
| Root `07_DATABASE_DESIGN_FINAL...` | `ARCHIVE_CANDIDATE_IDENTICAL_DUPLICATE` | Canonical: `docs/07-database/...` |
| Root `08_SECURITY_DESIGN_FINAL...` | `ARCHIVE_CANDIDATE_IDENTICAL_DUPLICATE` | Canonical: `docs/08-security/...` |
| D06–D19 `WORKING` files | `SUPERSEDED_WORKING` | FINAL pada folder domain masing-masing |
| Master v2.2/v2.5 | `SUPERSEDED_MASTER` | Current: `MASTER_PROJECT_SUMMARY.md` v4.3 |
| Master v3.4/v4.0/v4.1 | `MISSING_REFERENCED_SNAPSHOT` | Rekonstruksi/pulihkan jika wajib sebagai evidence; jangan dikarang |
| Prototype UI | `PROTOTYPE_ONLY` | Bukan RC, source of truth, screenshot, atau release evidence |
| Agent reports | `NON_AUTHORITATIVE_AUDIT_INPUT` | Simpan sebagai audit evidence |

Tidak ada file yang dipindahkan atau dihapus oleh manifest ini.

---

# 5. Open Baseline Conditions

- Human owner/reviewer/approver belum diisi.
- Effective date D01–D04 belum dikonfirmasi.
- Immutable Master v4.3 snapshot belum dipublikasikan.
- Archive retention/legal-hold/disposal policy belum dikonfirmasi.
- Parent version/hash metadata belum tertanam pada seluruh child.
- Language audit acceptance masih pending controlled re-audit.

---

# 6. Baseline Validation Rules

```text
Canonical Feature rows                   = 550
Unique Feature IDs                       = 550
Primary PDF Tool Cards                   = 33
Active stale feature count 546           = 0
Current file per Document ID             = 1
Unqualified implementation/UAT/Prod PASS = forbidden
SignalR as source of truth               = forbidden
PAUSED as Job state                      = forbidden
Public AI endpoint                       = forbidden
PKI Digital Signature in scope           = forbidden
Hostinger as Production                  = forbidden
```

---

# 7. Human Approval Block

```text
Baseline ID      : PERTAMINA-OTO-DOCS-BASELINE-1.0
Candidate tag    : docs-baseline-v1.0
Prepared by      : Codex — documentation audit only
Owner            : TBC
Reviewer         : TBC
Approver         : TBC
Approval date    : TBC
Approval evidence: TBC
Status           : CANDIDATE
```

Codex boleh menyiapkan command commit/tag setelah repository resmi tersedia. Codex tidak melakukan approval, commit, atau tag dalam fase audit ini.
