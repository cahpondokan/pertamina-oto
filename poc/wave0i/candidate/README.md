# Wave 0I offline candidate build

Agent 2 / POC-PDF-001 / FEAT-PDF-005. Baseline f6fa411bdc5415099d6a74ea356a5faab224df1f. Authority P2-W0H-EXEC-001 remains valid; Agent 0 is operator. This lane implements build/staging source and runs static tests only. It does not build, load the engine, generate a PDF, or authorize full candidate admission.

## Commands and integration contract

Run from the integrated worktree with existing PowerShell 7:

    pwsh -NoProfile -File poc/wave0i/candidate/Test-Static.ps1
    pwsh -NoProfile -File poc/wave0i/candidate/Build-Offline.ps1 -Mode Verify

Verify is the default. It reads and hashes source, notice, installed files and selected ZIP members; it neither writes staged assets nor launches a process. Missing or changed bytes stop the operation. A changed lock requires code review and an updated embedded digest; there is no resolver or automatic lock refresh.

After root's review, this concrete command builds and stages using only existing bytes (NOT executed by this lane):

    & ./poc/wave0i/candidate/Build-Offline.ps1 -Mode Build -OutputDirectory (Join-Path $env:TEMP 'OTO-W0I-PDFsharp-build-001')

The output directory must be new. If it already exists or a build fails, do not reuse it; root selects a new run directory after diagnosing the prior result. No cleanup or overwrite is performed. DotnetRoot and EvidenceRoot can relocate the same exact bytes; versions and digests cannot be selected through arguments.

Output interface for the environment lane:

| Path relative to new output directory | Purpose |
|---|---|
| guest/dotnet/dotnet.exe | Private x64 host, with host/fxr/10.0.11 and shared/Microsoft.NETCore.App/10.0.11 |
| guest/app/MergeSmoke.dll | Compiled runner, only present after successful compiler operation |
| guest/app/*.dll | Twelve explicitly selected candidate dependency DLLs |
| guest/app/MergeSmoke.runtimeconfig.json | .NET 10.0.11, rollForward disabled |
| guest/app/MergeSmoke.deps.json | Explicit app-local dependency list; no probing directories |
| guest/notices/ | Bound PDFsharp MIT packet, actual Microsoft package licenses/third-party notices, installed .NET notices |
| bundle.manifest.json | Every guest-relative file's SHA256 and byte count, input-lock digest, BUILT_NOT_EXECUTED status |
| build/command.json, compiler.rsp, compiler.log | Actual compiler invocation/response digest and diagnostics; retained on host, not required in guest |

Agent 1/root should copy ONLY guest plus the corresponding bundle manifest into the narrow reviewed input staging area. Do not map the source repository, SDK installation, raw intake, build directory, or broad TEMP directory. Recheck manifest file hashes before guest execution. Root's environment supervisor must use the private guest host explicitly, with a clean environment and no inherited DOTNET_STARTUP_HOOKS, profiler, additional-deps or shared-store overrides.

Example guest invocation after copying guest contents to C:\OTO-Smoke\candidate:

    C:\OTO-Smoke\candidate\dotnet\dotnet.exe C:\OTO-Smoke\candidate\app\MergeSmoke.dll C:\OTO-Smoke\output\CAB.partial.pdf C:\OTO-Smoke\input\C.pdf C:\OTO-Smoke\input\A.pdf C:\OTO-Smoke\input\B.pdf

Arguments are ordered, all input files must be frozen generator output, and output must not exist. AB, CAB, AA and AC are defined in poc/wave0h/corpus/spec.json. Source is unchanged from Wave 0H: max three arguments, three pages and 64 KiB per input, post-write 256 KiB check. The checks after parsing/writing are not resource containment. Root must apply external supervision and independent oracle before calling any result valid.

## Exact toolchain and member binding

inputs.lock.json records 365 installed files: 191 runtime/host files, 167 .NET 10.0 reference assemblies, five compiler files and two notice files. Installed paths are under C:\Program Files\dotnet: SDK 10.0.400 Roslyn/bincore, packs/Microsoft.NETCore.App.Ref/10.0.11/ref/net10.0, host/fxr/10.0.11 and shared/Microsoft.NETCore.App/10.0.11. Static PE header inspection confirmed x64 dotnet.exe, hostfxr.dll and coreclr.dll. Runtime staging is 80,370,072 bytes before candidate/notice bytes: the existing framework tree plus host, not a full SDK, installer, global installation or hand-trimmed runtime.

The four previously acquired packages are PDFsharp 6.2.4, Logging.Abstractions 8.0.3, DependencyInjection.Abstractions 8.0.2 and Pkcs 8.0.1. All archive hashes and twelve chosen member hashes are checked afresh. The existing nine PDFsharp lib/net10.0 DLLs and Logging/DI net8.0 DLLs are retained; Pkcs uses only runtimes/win/lib/net8.0/System.Security.Cryptography.Pkcs.dll for both metadata reference and runtime, avoiding duplicate same-name assemblies. This conservatively retains the reviewed PDFsharp set; it does not claim an experimentally proven minimum load closure.

The compiler is invoked directly through private dotnet exec with --fx-version 10.0.11. /noconfig is passed outside the response file, and /nostdlib+, /platform:x64 and explicit references are used. The response file avoids Windows command-length limits and rejects quote/newline injection. No MSBuild, csproj, product project, restore, package targets, analyzer, source generator, NuGet feed or download command is used. No build server is used. The compiler process gets a cleared environment and a 60-second timeout with process-tree kill on expiry. This compiler timeout is not the guest engine supervisor.

## Verification and limits

Test-Static.ps1: eight checks PASS, exit 0. Includes real-byte binding and negative text-only ZIP, missing/hash/length/path/response tests. static-test-results.json retains the actual JSON result. No .NET executable, compiler, PDF parser, provider or generator was run. Parsing PowerShell and ZIP metadata and hashing DLL bytes does not load those candidate assemblies.

Actual build success, dependency resolution, native runtime startup, guest loader behavior, offline/egress proof and engine outcomes remain unmeasured. Windows system DLLs and OS identity must be bound by the environment lane. Reparse/relative-path checks and CreateNew reduce accidental escape/overwrite; they are not race-proof against a hostile same-account process, and fresh output directories require private staging. Partial builds are not valid bundles: bundle.manifest.json is only emitted after compiler exit 0 and output existence. Hashes identify bytes, not publisher authenticity or a shipped upstream security fix.

Wave 0H parser/provenance restrictions persist. This package closes source preparation for staging/build; it does not close execution, oracle, malformed-input, cancellation, visual fidelity, atomic Save As, UAT or Phase 2 exit gates. ZERO_ADDITIONAL_COST.
