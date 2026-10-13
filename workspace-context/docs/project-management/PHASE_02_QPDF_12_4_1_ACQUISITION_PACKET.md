# Phase 2 — qpdf 12.4.1 Acquisition-Only Packet

| Field | Frozen value |
|---|---|
| Checked | 2026-09-06 01:47:23 +07:00 |
| Upstream release | `https://github.com/qpdf/qpdf/releases/tag/v12.4.1` |
| Release tag/commit observation | `v12.4.1` / `c37f83a`; signed tag shown verified by GitHub |
| Exact asset | `qpdf-12.4.1-msvc64.zip` |
| Exact official asset URL | `https://github.com/qpdf/qpdf/releases/download/v12.4.1/qpdf-12.4.1-msvc64.zip` |
| Upstream-advertised SHA-256 | `3cd016cd433ef7232e42f4c13348a49cc14907a3c7278ef4f99120593126f7a6` |
| Upstream-displayed size/time | 26.9 MB / 2026-08-27T23:56:31Z |
| Proposed use | Structural oracle candidate only |
| Cost boundary | `ZERO_ADDITIONAL_COST` |
| Current authority | `NOT_AUTHORIZED_FOR_ACQUISITION_OR_EXECUTION` |

## Primary-source check

The official GitHub release page identifies qpdf 12.4.1 and states that the release
was signed. The official expanded-assets endpoint exposes the exact Windows MSVC x64
ZIP, its displayed SHA-256, size, and timestamp. The upstream download guidance says
the Windows ZIP can be extracted to a chosen location and identifies MSVC64 as the
normal 64-bit Windows command-line distribution.

References:

- `https://github.com/qpdf/qpdf/releases/tag/v12.4.1`
- `https://github.com/qpdf/qpdf/releases/expanded_assets/v12.4.1`
- `https://github.com/qpdf/qpdf/blob/main/README-what-to-download.md`

This metadata freeze is not proof of downloaded bytes, signature chain, bundled
dependency licenses, vulnerability disposition, or safe execution. It does not
authorize download, extraction, launch, PDF processing, product integration, or
redistribution.

## Proposed bounded human decision

Authorize acquisition-only of `qpdf-12.4.1-msvc64.zip` from the exact official asset
URL above into the Phase 2 evidence intake directory. Accept only the no-cost
Apache-2.0/open-source evaluation route. Compare the downloaded SHA-256 against the
upstream-advertised value, record source/time/size and redirects, inventory archive
paths/DLLs/licenses/notices/signatures/SBOM, and stop on any mismatch or paid
requirement. Do not execute qpdf, process PDFs, restore/build, add it to product
references, or acquire additional components under this decision.

