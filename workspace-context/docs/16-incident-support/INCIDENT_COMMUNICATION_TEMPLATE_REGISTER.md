# REGISTER TEMPLATE KOMUNIKASI INCIDENT
## PERTAMINA OTO — Office Tools Organizer

> **Versi:** 1.0  
> **Tanggal:** 1 September 2026  
> **Status:** FINAL REGISTER — `REQUIRES_POLICY_CONFIRMATION`

---

# 1. Aturan

- Gunakan Bahasa Indonesia sederhana, factual, need-to-know, dan permission-scoped.
- Jangan memasukkan credential, content, daftar user, raw diagnostic, exploit, root cause belum terverifikasi, atau target waktu yang belum disetujui.
- Kanal, sender authority, recipient, cadence, dan approval tetap memerlukan konfirmasi Pertamina.

# 2. Template

| ID | Penggunaan | Teks minimum |
|---|---|---|
| `COM-ACK-001` | Acknowledgement | “Laporan `[reference]` telah diterima. Gejala yang dipahami: `[safe summary]`. Pemeriksaan dampak sedang dilakukan. Jangan kirim password, token, atau isi dokumen.” |
| `COM-INFO-001` | Request information | “Mohon `[platform/version/time/Error Code]`. Jangan kirim content atau data sensitif. Preview screenshot sebelum mengirim.” |
| `COM-UPDATE-001` | Investigation update | “Fakta terverifikasi: `[facts]`. Dampak: `[impact]`. Penyebab masih `[unknown/under investigation]`. Tindakan aman: `[action]`.” |
| `COM-WORKAROUND-001` | Workaround | “Gunakan `[validated steps]` hanya pada `[scope/version]`. Hentikan bila hasil berbeda. Jangan melewati permission/policy.” |
| `COM-DELAY-001` | Delayed resolution | “Pemulihan belum dinyatakan selesai karena `[safe reason]`. Tindakan aman tetap `[action]`. Update mengikuti checkpoint resmi.” |
| `COM-MONITOR-001` | Monitoring | “Pemulihan awal telah diterapkan pada `[scope]`; stabilitas dan hasil masih diverifikasi. Status belum `RESOLVED`.” |
| `COM-RESOLVED-001` | Resolved | “Gangguan telah dipulihkan dan verifikasi awal lulus pada `[scope]`. Monitoring/closure masih dilengkapi.” |
| `COM-CLOSED-001` | Closed | “Incident ditutup setelah verification, communication, evidence, linked record, risk, dan required review lengkap.” |
| `COM-REOPEN-001` | Reopened | “Incident dibuka kembali karena `[reason]`. Impact, severity, priority, dan owner dinilai ulang.” |
| `COM-SEC-HOLD-001` | Security holding | “Laporan berpotensi terkait keamanan/privasi sedang ditinjau. Detail dibatasi. Ikuti hanya instruksi resmi dan jangan kirim data sensitif.” |
| `COM-INTERNAL-001` | Internal handoff | Incident ID, confirmed/suspected facts, environment/scope, impact, owner, containment, unknowns, requested decision, dan safe evidence refs |
| `COM-PROVIDER-001` | Provider escalation | Approved provider profile/case, affected capability/version, safe error/time/impact, action tried, dan decision requested; tanpa content/secret |

# 3. Catatan Komunikasi

Simpan reference, audience class, sender function, timestamp, template/version, approval bila wajib, dan safe summary. Jangan menyimpan payload sensitif pada Audit atau Incident Record umum.
