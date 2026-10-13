# Catalog content widths — revision 5 handoff

Scope: approved local UI only; no phase completion, delivery, install or Production claim.
Worker owns CenteredHome.cs and new catalog panel/audit only. No subagents or builds ran.
Baseline branch task/core-functional-first, HEAD 84cbb0f538020003bcfb6583e94c2fcf6524f87c; existing dirty source preserved.
Final mockup inspected: C:/Users/cahpo/.codex/generated_images/01a12df3-d5a1-7a90-aaed-790dece50553/exec-65e559db-f0f7-4e30-b7d7-8ea15e8d180c.png.

## Baseline SHA256

- CenteredHome.cs: F8E166258EB52BB7729713ADB2F8E5410502CC1F0FB490F14AD7A6B741F59771
- DesignedHome.cs: FEF16201F474C57BF23D9EA8755FDA09358890AE126B0D6A0DDF64087008EDD3
- GroupedCatalog.cs: 140694C41C352E50E171CB50E432D488CE9641B09384B30820240E966F1D699F
- Before-source copy: artifacts/catalog-width-20261013/CenteredHome.before.cs (same baseline SHA256).

## Changed source

- src/Hosts/Pertamina.Oto.Desktop.App/CenteredHome.cs: centeredGrid becomes ContentCenteredCatalogPanel; LayoutCenteredCards invalidates measure instead of rebuilding uniform Grid columns. Launch padding becomes equal 35 DIP horizontal (15 text padding + 20 symmetric star reservation), with existing 24/18 vertical padding.
- src/Hosts/Pertamina.Oto.Desktop.App/ContentCenteredCatalogPanel.cs: measure natural unconstrained content; remeasure only when wider than viewport; greedy order-preserving wrap, center each individual row including last row; 12 DIP gap; collapsed children excluded. No global ResponsiveWrapPanel changes.
- src/Hosts/Pertamina.Oto.Desktop.App/CatalogContentWidthAudit.cs: DEBUG native audit at 1920,1480,1024,800,660 ×810; checks all16/order, variable widths, symmetric margins, gap, height, viewport bounds, text bounds/readability height, favorite state, search alias and security filter.
- docs/development/CATALOG_WIDTH_HANDOFF_20261013.md: this handoff.

Actual source trace: BuildDesignedHome -> BuildCenteredHome -> RefreshCenteredCards -> BuildCenteredCard. DesignedHome.cs and GroupedCatalog.cs do not need edits. Existing MainWindow references only Children/IsLoaded and LayoutCenteredCards; audit callers only use framework/panel members, so no MainWindow callsite edit required.

## Integrator-only Program.cs hunks

Inside existing DEBUG block containing homeAuditIndex, immediately after its declaration:

```csharp
var catalogWidthAuditIndex = Array.IndexOf(homeAuditArgs, "--audit-catalog-content-width");
if (catalogWidthAuditIndex >= 0 && catalogWidthAuditIndex + 1 < homeAuditArgs.Length)
    MainWindow.PrepareCenteredAudit(homeAuditArgs[catalogWidthAuditIndex + 1]);
```

Inside existing DEBUG dispatch block, before homeAuditIndex dispatch:

```csharp
if (catalogWidthAuditIndex >= 0 && catalogWidthAuditIndex + 1 < homeAuditArgs.Length)
{
    await ((MainWindow)window).AuditCatalogContentWidthAsync(homeAuditArgs[catalogWidthAuditIndex + 1]);
    Exit(); return;
}
```

Run exact integrated debug binary with `--audit-catalog-content-width <isolated-evidence-dir>` and existing `--audit-centered-home <separate-isolated-evidence-dir>` for screenshots and launch/favorite/navigation regression. No audit of this new code has run yet.

## Static verification and limits

- Read-only source reference scan confirmed no callers require Grid column APIs outside replaced method.
- `git diff --check`: exit0, existing tracked-file CRLF normalization warnings only.
- PowerShell exact text comparison of CenteredCopy mapping: PASS unchanged.
- PowerShell exact text comparison of source from `// Paint the full button surface` through file end: PASS unchanged. Launch, independent favorite, pointer/focus handlers and icons preserved.
- `git diff --no-index -- artifacts/catalog-width-20261013/CenteredHome.before.cs src/Hosts/Pertamina.Oto.Desktop.App/CenteredHome.cs`: exit1 means three intended differing hunks, not a verification failure.
- Build/runtime/visual/DPI evidence PENDING integrator. No dependency changes, product bump, user data/clipboard access, app launch/close, feed/install/publish or engine edits.
- Risk: WinUI natural measurement and responsive arrangement require native audit after combined build; fixed-height text must remain readable at minimum viewport. Geometry audit uses logical DIP and native resize; physical multimonitor/DPI UAT remains separate.

Worker source handoff ready; integrator retains ownership of final integration, build, runtime, shared docs and any delivery.
