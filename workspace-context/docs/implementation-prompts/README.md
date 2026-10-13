# PERTAMINA OTO — Codex Multi-Agent Prompt Pack

> Router perintah singkat berada di `../../AGENTS.md`; status dan tata kelola phase berada di `../project-management/`. Contoh: `kerjakan phase 0 sesuai rencana`.

> Versi: 1.0  
> Tanggal: 5 September 2026  
> Status: EXECUTION PROMPT PACK — setiap phase tetap tunduk pada human gate  
> Cakupan: Phase 0 sampai Phase 11, Agent 0 sampai Agent 7

## Tujuan

Paket ini menyediakan prompt siap pakai untuk menjalankan satu phase sebagai durable goal, membagi pekerjaan independen kepada agen spesialis, menunggu handoff mereka, lalu meminta Agent 0 mengaudit dan mengintegrasikan hasil. Angka phase atau persentase tidak pernah menggantikan exit evidence.

Gunakan satu phase per eksekusi. Jangan menjalankan Phase 0–11 sebagai satu goal tanpa checkpoint karena setiap phase memiliki entry gate, human decision, dan stop condition berbeda.

## Sumber berwenang

Semua agen wajib membaca sumber yang disebut pada prompt phase dan selalu memperlakukan dokumen berikut sebagai baseline:

- `01_BUSINESS_PROCESS_FINAL_v3.4_PERTAMINA_OTO.md` sampai `19_HANDOVER_CHECKLIST_FINAL_v1.0_PERTAMINA_OTO.md`;
- `MASTER_PROJECT_SUMMARY.md`;
- `docs/baseline-audit/IMPLEMENTATION_ROADMAP_v1.0.md`;
- `docs/baseline-audit/IMPLEMENTATION_READINESS_FINAL_v1.0.md`;
- `docs/baseline-audit/OPEN_ITEM_AND_CONFIRMATION_REGISTER.md`;
- `00_SHARED_AGENT_CONTRACT.md` dalam folder ini.

## Identitas agen

| Agen | Keahlian utama |
|---|---|
| Agent 0 | Principal Architect, Master Integrator, coordinator dan evidence auditor |
| Agent 1 | Senior .NET Platform, repository, build dan dependency engineering |
| Agent 2 | Senior PDF/document engine, domain dan application engineering |
| Agent 3 | Senior WinUI desktop, UI/UX dan accessibility engineering |
| Agent 4 | Senior backend, API, database, storage dan offline processing |
| Agent 5 | Senior mobile, web dan cross-platform engineering |
| Agent 6 | Principal QA, application security, performance dan reliability |
| Agent 7 | Senior DevOps/SRE, release, operations dan evidence engineering |

## Indeks prompt

| Phase | Berkas | Agen pekerja aktif |
|---:|---|---|
| 0 | `PHASE_00_BOOTSTRAP.md` | 1, 2, 6, 7 |
| 1 | `PHASE_01_ARCHITECTURE_FOUNDATION.md` | 1, 2, 3, 4, 6, 7 |
| 2 | `PHASE_02_PROOF_OF_CAPABILITY.md` | 1–7 |
| 3 | `PHASE_03_MERGE_PDF.md` | 1, 2, 3, 4, 6, 7 |
| 4 | `PHASE_04_CORE_LOCAL_PDF.md` | 1, 2, 3, 4, 6, 7 |
| 5 | `PHASE_05_CONVERSION_OCR.md` | 1–7 |
| 6 | `PHASE_06_EDIT_SECURITY.md` | 1, 2, 3, 4, 6, 7 |
| 7 | `PHASE_07_PDF_INTELLIGENCE.md` | 1, 2, 3, 4, 6, 7 |
| 8 | `PHASE_08_MOBILE_WEB.md` | 1–7 |
| 9 | `PHASE_09_HARDENING_UAT_PILOT.md` | 1–7 |
| 10 | `PHASE_10_PRODUCTION_READINESS.md` | 1–7 |
| 11 | `PHASE_11_LAUNCH_HANDOVER.md` | 1–7 |

## Cara menggunakan

1. Human Owner mengisi seluruh placeholder pada bagian `Decision block` prompt Agent 0.
2. Buka task Agent 0 dan tempel prompt `/goal` phase yang dipilih.
3. Agent 0 melakukan preflight dan hanya mendispatch prompt agen yang dinyatakan aktif.
4. Agen bekerja pada branch/worktree dan allowed paths terpisah.
5. Agent 0 menunggu seluruh agen aktif, membaca handoff, mengaudit diff dan menjalankan integration gate.
6. Human Integrator memberi otorisasi merge atau keputusan yang memang membutuhkan manusia.
7. Agent 0 menutup phase hanya jika semua exit evidence lulus. Phase berikutnya memakai hasil phase sebelumnya sebagai entry evidence.

Jika memakai task utama terpisah di sidebar, tempel prompt pekerja ke task yang sesuai dan kirimkan path handoff/commit kepada Agent 0. Jika memakai subagent dalam satu task, Agent 0 harus menyebut delegasi secara eksplisit dan menunggu semua hasil sebelum integrasi.

## Batas konkurensi

Jumlah agen pada tabel adalah jumlah peran yang diperlukan, bukan jaminan semua proses aktif pada saat yang sama. Agent 0 harus menggunakan concurrency yang benar-benar tersedia, mendahulukan task read-only/independen, dan menjalankan write-heavy task dalam gelombang ketika ownership path berpotensi beririsan.

## Referensi pola Codex

- Durable goals: https://learn.chatgpt.com/use-cases/follow-goals
- Subagents: https://learn.chatgpt.com/docs/agent-configuration/subagents
