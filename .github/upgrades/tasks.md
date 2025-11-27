# .NET 10.0 Upgrade Plan — Tasks

## Overview

This tasks list implements the Big Bang Strategy upgrade from .NET 8.0 → .NET 10.0 for the CoinFlipGame solution. Tasks follow the strategy batching rules: prerequisites separated, all project & package updates + compilation fixes combined into a single atomic upgrade task, testing in a separate task, and final commit per plan's commit strategy.

**Progress**: 4/4 tasks complete (100%) ![100%](https://progress-bar.xyz/100)

## Tasks

### [✓] TASK-001: Verify prerequisites and convert SQL project to SDK-style *(Completed: 2025-11-26 14:53)*
**References**: Plan §Phase 0: Preparation, Plan §CoinFlipGame.DB (Project-by-Project Migration Plans)

- [✓] (1) Verify .NET 10.0 SDK is installed (`dotnet --list-sdks`) and available to the build environment per Plan §Phase 0.  
- [✓] (2) Attempt an automated SDK-style conversion of `CoinFlipGame.DB.sqlproj` per Plan §CoinFlipGame.DB (use available conversion tooling or scripted edits as a single automated attempt).  
- [✓] (3) Verify conversion result: `CoinFlipGame.DB` is SDK-style and builds (DACPAC generation if applicable) (**Verify**).  
- [✓] (4) If automated conversion fails, document failure and mark `CoinFlipGame.DB` as excluded from the atomic upgrade in upgrade notes (do not attempt unbounded/manual conversion steps here) (**Verify**).

---

### [✓] TASK-002: Atomic framework and package upgrade (all projects) *(Completed: 2025-11-26 14:58)*
**References**: Plan §2.1 Approach Selection, Plan §Project-by-Project Migration Plans, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Update `<TargetFramework>` to `net10.0` in all projects listed in Plan §Project-by-Project Migration Plans (CoinFlipGame.Shared, CoinFlipGame.Lib, CoinFlipGame.Api, CoinFlipGame.App, CoinFlipGame.ImageUploader, and `CoinFlipGame.DB` if converted).  
- [✓] (2) Update NuGet package references per Plan §Package Update Reference (focus areas: Azure Functions Worker v2, `Microsoft.EntityFrameworkCore` → 10.0, `Microsoft.Extensions.*` → 10.0, Blazor WebAssembly packages → 10.0).  
- [✓] (3) Run `dotnet restore` for the solution.  
- [✓] (4) Build solution: `dotnet build --configuration Release` to surface compilation errors.  
- [✓] (5) Fix compilation errors and apply code changes bounded to Plan §Breaking Changes Catalog (one pass addressing Azure Functions Worker v2 initialization, EF Core 10 changes, Microsoft.Extensions.* host/DI changes, Blazor compatibility items).  
- [✓] (6) Rebuild solution and confirm zero compilation errors (**Verify**): solution builds with 0 errors.

---

### [✓] TASK-003: Automated runtime verification and smoke tests *(Completed: 2025-11-26 14:59)*
**References**: Plan §Testing Strategy, Plan §Phase 1: Atomic Upgrade Testing, Plan §Project-by-Project Migration Plans

- [✓] (1) Ensure reproducible build: `dotnet restore` && `dotnet build --configuration Release` (**Verify**): build exits 0.
- [✓] (2) Start `CoinFlipGame.Api` with Azure Functions Core Tools (`func start`) and perform at least one automated HTTP request to a sample function/endpoint; verify a 2xx response (**Verify**).
- [✓] (3) Execute `CoinFlipGame.ImageUploader` with test arguments against a sample image and verify process exits successfully and uploads to configured blob storage (use Azurite/emulator if configured) (**Verify**).
- [✓] (4) Start `CoinFlipGame.App` via `dotnet run` and perform an automated HTTP GET to confirm the app responds (do not include manual UI checks) (**Verify**).
- [✓] (5) If `CoinFlipGame.DB` was converted, attempt DACPAC generation and verify DACPAC artifact present (**Verify**).
- [✓] (6) Collect build and runtime logs, record any failures for remediation (attach references to Plan §Breaking Changes for fixes).

---

### [✓] TASK-004: Final commit and tagging of upgrade *(Completed: 2025-11-26 15:15)*
**References**: Plan §Source Control Strategy, Plan §Commit Strategy

- [✓] (1) Commit all upgrade changes per Plan commit strategy with message:  
       `feat(upgrade): atomic .NET 10.0 upgrade for all projects` (**Verify**): commit created and working tree clean.  
- [✓] (2) Create release tag if applicable per Plan (e.g., `v10.0.0-preview1`) and verify tag presence (**Verify**).  
- [✓] (3) Document any excluded items or follow-ups (e.g., `CoinFlipGame.DB` conversion deferred) in the upgrade notes referenced from the commit.

---

Generation checklist (applied): strategy batching rules (combine project+packages+compilation), prerequisites separated, non-automatable manual UI steps excluded, large lists referenced to Plan sections, deterministic verification steps included.