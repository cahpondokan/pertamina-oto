# Multi-Agent Task, Branch, Worktree, and Handoff Workflow

## Governance

This workflow implements `BOOT-008` under `LOCAL-BOOT-001` and `CCR-015`. The default branch is `main`. Agent 0 is the AI master integrator; the workspace user is the provisional human Project/Product Owner and merge/promotion authority. Specialist agents never merge to `main`, approve their own work, create Production connections, or replace a human gate.

## Task-card readiness

Agent 0 must freeze a baseline commit and create a task card before dispatch. A ready card contains every field below with an exact value or explicit `N/A` plus rationale:

| Field | Required content |
|---|---|
| Task ID | Exact BOOT/Phase work item |
| Requirement IDs | Exact canonical IDs; no unbounded range shorthand |
| Feature IDs | Exact IDs or `N/A — no Feature in Phase 0` |
| Decision IDs | Approved gate/change references |
| Objective | One verifiable outcome |
| ProcessingMode | `LOCAL`, `SERVER`, or `N/A` |
| ServerDependency | Exact dependency or `NONE` |
| StorageImpact | Data/artifact paths and retention boundary |
| NetworkImpact | Required destinations or `NONE` |
| ConcurrencyRisk | Shared contracts, root files, or runtime locks |
| Allowed Paths | Exclusive writer paths |
| Do-Not-Touch Paths | Explicit protected paths |
| Dependencies | Upstream task/commit/evidence |
| Acceptance Criteria | Observable pass conditions |
| Tests | Exact commands and negative path |
| Security Constraints | Secret, data, log, and authorization limits |
| Performance Budget | Measurement or `N/A` with rationale |
| Expected Output | Code, test, document, and evidence artifacts |
| Branch | `task/<task-id>-<short-name>` |
| Worktree | One dedicated path for the task |
| Owner Lane | One named writer |
| Reviewer | Independent reviewer or Agent 0 audit lane |
| Stop Conditions | Approval, contract, credential, conflict, and safety stops |
| Handoff Evidence | Mandatory handoff fields and evidence location |

Missing fields mean the card is not dispatch-ready.

## Isolation and execution

1. Agent 0 records the baseline commit and creates one branch and one worktree per task.
2. The specialist confirms branch, worktree, clean status, baseline, approved decisions, allowed paths, and test plan before writing.
3. One shared path has one writer at a time. Root configuration, common contracts, package authority, and central registers are serialized.
4. Specialists preserve unrelated user/agent changes and never reset, rebase, force-push, or delete another lane's work.
5. Parallel work uses waves when slot capacity or dependencies require it. A later wave starts only when its inputs are stable enough to consume.
6. A specialist commits only its own allowed-path changes and returns a handoff; it does not merge.
7. Agent 0 waits for every active lane, rejects incomplete handoffs, audits diff/test/security/evidence, and integrates in dependency order.
8. Agent 0 runs the clean integrated pipeline on the candidate commit. A green specialist branch is not integrated proof.
9. The human merge authority reviews the Phase report and authorizes or rejects promotion to `main`.

## Stop and blocker protocol

A specialist stops only the affected work when it needs a new human approval, secret/credential, provider/license decision, unowned contract change, Production access, destructive action, or would overwrite another writer. It continues safe independent work where possible and reports:

- exact blocked task and criterion;
- current safe checkpoint/commit;
- owner and decision required;
- evidence already produced;
- paths that must remain untouched;
- exact command to resume.

No agent marks a blocked or partially integrated task complete.

## Required handoff

Every specialist returns:

```text
Agent / Task ID:
Objective:
Baseline commit/hash:
Branch / worktree:
Requirement / Feature / Decision IDs:
Files changed:
Behavior implemented:
Tests and exact results:
Security/privacy checks:
Performance/resource evidence:
Known limitations:
Open blockers and required human decision:
Evidence paths:
Recommended integration order:
```

Agent 0 ties accepted handoffs to the run record, integrated commit, full-pipeline run ID, and exit-gate evidence. Central project state, decision register, and execution log remain single-writer files owned by Agent 0.
