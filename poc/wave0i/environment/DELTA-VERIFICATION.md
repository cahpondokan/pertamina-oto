# Integration correction

Baseline 2f1ff3413a8105f5531e52eb05fd49de3528c1d7.
Corrected dotnet path, cleared/allowlisted child environment, and added explicit
A2 schemaVersion bundle plus supplementary files adapter to schema staging.
Candidate/oracle unchanged. No child/guest/build/restore executed.

Test: pwsh -NoProfile -File poc/wave0i/environment/Test-Static.ps1
Exit0; 12/12 PASS. Added ProcessStartInfo-only environment canary removal,
three-file combined adapter, duplicate rejection and wrong schema rejection.
No actual process start in tests. Prior supervision/quarantine limitations remain.
Root's successful second no-map CLI probe is reported operator evidence, not
reproduced here. Apply this delta before integrated staging/child review.
