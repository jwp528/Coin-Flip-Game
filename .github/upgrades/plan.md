# .NET 10.0 Upgrade Plan

## Executive Summary

### Selected Strategy
**Big Bang Strategy** - All projects upgraded simultaneously in single operation.

**Rationale**: 
- 6 projects (small solution)
- All currently on .NET 8.0 (except SQL project on .NET Framework 4.7.2)
- Clear dependency structure with shared libraries at foundation
- Most packages have target framework versions available
- Manageable scope allows for atomic upgrade

### Scope
- **Total Projects**: 6 projects
- **Current State**: 5 projects on .NET 8.0, 1 SQL project on .NET Framework 4.7.2
- **Target State**: All projects on .NET 10.0

### Target State
After migration, the solution will consist of:
- **CoinFlipGame.App** (Blazor WebAssembly): net8.0 → net10.0
- **CoinFlipGame.Api** (Azure Functions): net8.0 → net10.0
- **CoinFlipGame.ImageUploader** (Console): net8.0 → net10.0
- **CoinFlipGame.Lib** (Class Library): net8.0 → net10.0
- **CoinFlipGame.Shared** (Class Library): net8.0 → net10.0
- **CoinFlipGame.DB** (SQL Project): ⚠️ **Requires SDK-style conversion** from .NET Framework 4.7.2 → net10.0

### Complexity Assessment
**Overall: Medium**

**Justification:**
- ✅ Small solution with clear dependencies
- ✅ Most packages have straightforward upgrade paths
- ⚠️ Azure Functions Worker packages require major version jump (v1 → v2)
- ⚠️ SQL project requires SDK-style conversion
- ⚠️ .NET 10.0 is in preview - potential for unexpected breaking changes
- ✅ No security vulnerabilities flagged

### Critical Issues
- **SQL Project Conversion**: CoinFlipGame.DB.sqlproj must be converted to SDK-style before framework upgrade
- **Azure Functions Worker v2**: Breaking changes expected in upgrade from 1.x to 2.x
- **Preview Framework**: .NET 10.0 is preview - expect potential instability

### Recommended Approach
Big Bang approach with all projects upgraded atomically in dependency order within a single unified operation.

---

## Migration Strategy

### 2.1 Approach Selection

**Chosen Strategy**: Big Bang Strategy

**Justification**: 
- Small solution (6 projects) is ideal for atomic upgrade
- Clear dependency graph: Shared/Lib projects → Api/App projects → ImageUploader (standalone)
- All SDK-style projects except SQL project (which needs conversion)
- Unified .NET 8.0 baseline simplifies target selection
- Test quickly after all changes applied
- Single commit enables easy rollback if needed

**Strategy-Specific Considerations**:
- All project files and packages updated in single coordinated operation
- Build verification after all changes ensures cohesive state
- Dependency order maintained within atomic operation
- Preview framework requires cautious testing

### 2.2 Dependency-Based Ordering

**Migration Order** (all executed in single atomic operation):
1. **Foundation Layer**: CoinFlipGame.Shared, CoinFlipGame.Lib (no dependencies)
2. **Application Layer**: CoinFlipGame.Api (depends on Shared + Lib), CoinFlipGame.App (depends on Shared)
3. **Standalone Tools**: CoinFlipGame.ImageUploader (no dependencies), CoinFlipGame.DB (separate concern)

**Critical Path**: Shared → Api/App (both depend on Shared)

**SQL Project Note**: CoinFlipGame.DB.sqlproj is isolated (no dependencies/dependants), making SDK conversion lower risk.

### 2.3 Parallel vs Sequential Execution

**Within Atomic Operation**:
- Shared and Lib can be conceptually parallel (no interdependency)
- Api and App must follow Shared (dependency constraint)
- ImageUploader can be done anytime (fully independent)
- DB conversion can be done first or last (isolated)

**Strategy Considerations**: 
Big Bang executes all as single batch, but order matters for:
- Restore operations (dependencies first)
- Build verification (foundation before consumers)

---

## Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

```
Leaf Nodes (No Dependencies):
├── CoinFlipGame.Shared (depended on by 2 projects)
├── CoinFlipGame.Lib (depended on by 1 project)
├── CoinFlipGame.ImageUploader (standalone)
└── CoinFlipGame.DB (standalone, needs SDK conversion)

Consumer Nodes:
├── CoinFlipGame.Api (depends on: Shared, Lib)
└── CoinFlipGame.App (depends on: Shared)
```

**Critical Constraint**: Shared must be upgraded before Api and App.

### 3.2 Project Groupings

#### Phase 0: Preparation
- **Verify .NET 10.0 SDK Installation**
- **Convert CoinFlipGame.DB.sqlproj** to SDK-style (if keeping in solution)

#### Phase 1: Atomic Upgrade (All Projects Simultaneously)
**Foundation Projects** (0 dependencies):
- CoinFlipGame.Shared
- CoinFlipGame.Lib
- CoinFlipGame.ImageUploader
- CoinFlipGame.DB (after SDK conversion)

**Consumer Projects** (depend on foundation):
- CoinFlipGame.Api (→ Shared, Lib)
- CoinFlipGame.App (→ Shared)

**Strategy-Specific Grouping**: All projects upgraded in single operation, respecting dependency order during restore/build.

---

## Project-by-Project Migration Plans

### Project: CoinFlipGame.Shared

**Current State**
- Target Framework: net8.0
- Project Type: Class Library
- Dependencies: None
- Dependants: CoinFlipGame.Api, CoinFlipGame.App
- Package Count: 0
- LOC: 29

**Target State**
- Target Framework: net10.0
- No package updates required

**Migration Steps**
1. **Prerequisites**: None (leaf node)
2. **Framework Update**: Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in CoinFlipGame.Shared.csproj
3. **Package Updates**: None required
4. **Expected Breaking Changes**: Minimal (simple shared models library)
5. **Code Modifications**: None anticipated
6. **Testing Strategy**: Build verification, dependant projects (Api, App) will validate integration
7. **Validation Checklist**:
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Dependant projects (Api, App) still reference correctly

---

### Project: CoinFlipGame.Lib

**Current State**
- Target Framework: net8.0
- Project Type: Class Library
- Dependencies: None
- Dependants: CoinFlipGame.Api
- Package Count: 0
- LOC: 251

**Target State**
- Target Framework: net10.0
- No package updates required

**Migration Steps**
1. **Prerequisites**: None (leaf node)
2. **Framework Update**: Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in CoinFlipGame.Lib.csproj
3. **Package Updates**: None required
4. **Expected Breaking Changes**: Minimal (utility library)
5. **Code Modifications**: None anticipated
6. **Testing Strategy**: Build verification, Api project will validate integration
7. **Validation Checklist**:
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Dependant project (Api) still references correctly

---

### Project: CoinFlipGame.App (Blazor WebAssembly)

**Current State**
- Target Framework: net8.0
- Project Type: Blazor WebAssembly (AspNetCore)
- Dependencies: CoinFlipGame.Shared
- Dependants: None
- Package Count: 3
- LOC: 6,219

**Target State**
- Target Framework: net10.0
- Updated Packages: 2 (Blazored.LocalStorage compatible)

**Migration Steps**
1. **Prerequisites**: CoinFlipGame.Shared upgraded to net10.0
2. **Framework Update**: Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in CoinFlipGame.App.csproj
3. **Package Updates**:
   - `Microsoft.AspNetCore.Components.WebAssembly`: 8.0.* → 10.0.0
   - `Microsoft.AspNetCore.Components.WebAssembly.DevServer`: 8.0.* → 10.0.0
   - `Blazored.LocalStorage`: 4.5.0 (compatible, no update needed)
4. **Expected Breaking Changes**:
   - **Blazor WebAssembly**: Potential changes to component lifecycle, rendering, or JS interop
   - **Preview Framework**: Monitor for breaking changes in .NET 10.0 preview releases
   - **LocalStorage**: Should remain compatible (no update required)
5. **Code Modifications**:
   - Review `Program.cs` for service registration changes
   - Check `Components/App.razor` for router/layout changes
   - Verify `JSInterop` calls remain compatible
   - Test all Blazor components for rendering issues
6. **Testing Strategy**:
   - Build verification
   - Manual UI testing (coin flip functionality)
   - Verify local storage operations
   - Test JS interop features (haptics, audio, particles)
   - Validate responsive design
7. **Validation Checklist**:
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Application runs in browser
   - [ ] Coin flip mechanics work correctly
   - [ ] Local storage saves/loads preferences
   - [ ] JS interop features functional (haptics, sound, particles)
   - [ ] All modals and UI interactions work

---

### Project: CoinFlipGame.Api (Azure Functions)

**Current State**
- Target Framework: net8.0
- Project Type: Azure Functions (isolated worker)
- Dependencies: CoinFlipGame.Shared, CoinFlipGame.Lib
- Dependants: None
- Package Count: 6
- LOC: 807

**Target State**
- Target Framework: net10.0
- Updated Packages: 4 (Azure.Storage.Blobs compatible)

**Migration Steps**
1. **Prerequisites**: CoinFlipGame.Shared and CoinFlipGame.Lib upgraded to net10.0
2. **Framework Update**: Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in CoinFlipGame.Api.csproj
3. **Package Updates**:
   - `Microsoft.Azure.Functions.Worker`: **1.21.0 → 2.51.0** (MAJOR UPDATE ⚠️)
   - `Microsoft.Azure.Functions.Worker.Sdk`: **1.17.2 → 2.0.7** (MAJOR UPDATE ⚠️)
   - `Microsoft.Azure.Functions.Worker.Extensions.Http`: 3.1.0 → 3.3.0
   - `Microsoft.EntityFrameworkCore.SqlServer`: 8.* → 10.0.0
   - `Microsoft.Extensions.Caching.Memory`: 8.* → 10.0.0
   - `Azure.Storage.Blobs`: 12.* (compatible, no update needed)
4. **Expected Breaking Changes**:
   - **Azure Functions Worker v2**: Major version jump with breaking changes:
     - Worker initialization and configuration changes
     - Middleware pipeline changes
     - Dependency injection container changes
     - HTTP trigger binding changes
   - **Entity Framework Core 10.0**: Check for:
     - LINQ query translation changes
     - Database provider compatibility
     - Migration generation
   - **ASP.NET Core 10.0 Integration**: HTTP abstractions may have changed
5. **Code Modifications**:
   - Update `Program.cs` for Functions Worker v2 initialization
   - Review all HTTP trigger functions for binding changes
   - Update middleware registration if applicable
   - Verify DI container configuration
   - Check EF Core context configuration
   - Test database migrations
6. **Testing Strategy**:
   - Build verification
   - Local Azure Functions runtime testing
   - Test all HTTP endpoints
   - Verify database connectivity
   - Test blob storage operations
   - Validate caching behavior
7. **Validation Checklist**:
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Azure Functions runtime starts locally
   - [ ] All HTTP endpoints respond correctly
   - [ ] Database queries execute successfully
   - [ ] Blob storage operations work
   - [ ] Caching functions correctly

---

### Project: CoinFlipGame.ImageUploader

**Current State**
- Target Framework: net8.0
- Project Type: Console Application (DotNetCoreApp)
- Dependencies: None
- Dependants: None
- Package Count: 8
- LOC: 437

**Target State**
- Target Framework: net10.0
- Updated Packages: 6 (Azure.Storage.Blobs and SixLabors.ImageSharp compatible)

**Migration Steps**
1. **Prerequisites**: None (standalone project)
2. **Framework Update**: Change `<TargetFramework>net8.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in CoinFlipGame.ImageUploader.csproj
3. **Package Updates**:
   - `Microsoft.Extensions.Caching.Memory`: 8.* → 10.0.0
   - `Microsoft.Extensions.Configuration`: 8.* → 10.0.0
   - `Microsoft.Extensions.Configuration.Json`: 8.* → 10.0.0
   - `Microsoft.Extensions.DependencyInjection`: 8.* → 10.0.0
   - `Microsoft.Extensions.Logging`: 8.* → 10.0.0
   - `Microsoft.Extensions.Logging.Console`: 8.* → 10.0.0
   - `Azure.Storage.Blobs`: 12.* (compatible, no update needed)
   - `SixLabors.ImageSharp`: 3.* (compatible, no update needed)
4. **Expected Breaking Changes**:
   - **Microsoft.Extensions.*** packages: Check for:
     - Configuration builder changes
     - DI container registration changes
     - Logging provider updates
   - **Console App Host**: May have hosting model changes
5. **Code Modifications**:
   - Review `Program.cs` for configuration and DI setup
   - Verify logging configuration
   - Test image processing operations
   - Validate blob upload functionality
6. **Testing Strategy**:
   - Build verification
   - Run console application with test images
   - Verify image processing
   - Test blob storage upload
   - Validate logging output
7. **Validation Checklist**:
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] Console application executes successfully
   - [ ] Images processed correctly
   - [ ] Blob uploads succeed
   - [ ] Logging outputs as expected

---

### Project: CoinFlipGame.DB (SQL Database Project)

**Current State**
- Target Framework: .NETFramework,Version=v4.7.2
- Project Type: Classic SQL Project (sqlproj)
- SDK-style: **False** ⚠️
- Dependencies: None
- Dependants: None
- LOC: 0 (SQL scripts)

**Target State**
- Target Framework: net10.0
- SDK-style: **True** (must be converted)

**Migration Steps**
1. **Prerequisites**: Backup current project file and SQL scripts
2. **SDK-Style Conversion**: 
   - **Option A**: Use automated conversion tools (if available for SQL projects)
   - **Option B**: Manually convert to SDK-style .sqlproj format
   - **Option C**: Consider migration to SQL Database Project (Microsoft.Build.Sql) SDK
3. **Framework Update**: After conversion, set `<TargetFramework>net10.0</TargetFramework>`
4. **Package Updates**: SQL projects typically don't have NuGet packages; verify tooling compatibility
5. **Expected Breaking Changes**:
   - Project build targets may change
   - SQL deployment tools compatibility
   - IDE integration (Visual Studio or Azure Data Studio)
6. **Code Modifications**:
   - SQL scripts should remain unchanged
   - Project structure may need reorganization
7. **Testing Strategy**:
   - Build verification
   - Verify SQL scripts included correctly
   - Test DACPAC generation (if applicable)
   - Validate deployment tooling
8. **Validation Checklist**:
   - [ ] Project converted to SDK-style successfully
   - [ ] Project builds without errors
   - [ ] All SQL scripts included in build
   - [ ] DACPAC generated correctly (if applicable)
   - [ ] Deployment tools compatible

**Important Note**: SQL projects have special considerations. If conversion proves complex, consider:
- Keeping on current .NET Framework (separate from rest of solution)
- Migrating to Microsoft.Build.Sql SDK format
- Using standalone SQL tooling outside Visual Studio solution

---

## Package Update Reference

### Common Package Updates (affecting multiple projects)

| Package | Current | Target | Projects Affected | Update Reason |
|---------|---------|--------|-------------------|---------------|
| Microsoft.Extensions.Caching.Memory | 8.* | 10.0.0 | 2 projects (Api, ImageUploader) | Framework compatibility |

### Category-Specific Updates

#### Blazor WebAssembly (CoinFlipGame.App)
| Package | Current | Target | Update Reason |
|---------|---------|--------|---------------|
| Microsoft.AspNetCore.Components.WebAssembly | 8.0.* | 10.0.0 | Framework compatibility |
| Microsoft.AspNetCore.Components.WebAssembly.DevServer | 8.0.* | 10.0.0 | Development tooling |

#### Azure Functions (CoinFlipGame.Api)
| Package | Current | Target | Update Reason |
|---------|---------|--------|---------------|
| Microsoft.Azure.Functions.Worker | 1.21.0 | 2.51.0 | **MAJOR UPDATE** - Framework compatibility |
| Microsoft.Azure.Functions.Worker.Sdk | 1.17.2 | 2.0.7 | **MAJOR UPDATE** - Build tooling |
| Microsoft.Azure.Functions.Worker.Extensions.Http | 3.1.0 | 3.3.0 | HTTP binding support |
| Microsoft.EntityFrameworkCore.SqlServer | 8.* | 10.0.0 | Data access compatibility |

#### Console Application (CoinFlipGame.ImageUploader)
| Package | Current | Target | Update Reason |
|---------|---------|--------|---------------|
| Microsoft.Extensions.Configuration | 8.* | 10.0.0 | Configuration framework |
| Microsoft.Extensions.Configuration.Json | 8.* | 10.0.0 | JSON configuration support |
| Microsoft.Extensions.DependencyInjection | 8.* | 10.0.0 | DI container |
| Microsoft.Extensions.Logging | 8.* | 10.0.0 | Logging framework |
| Microsoft.Extensions.Logging.Console | 8.* | 10.0.0 | Console logging provider |

### Compatible Packages (No Update Required)
| Package | Current | Projects |
|---------|---------|----------|
| Azure.Storage.Blobs | 12.* | Api, ImageUploader |
| Blazored.LocalStorage | 4.5.0 | App |
| SixLabors.ImageSharp | 3.* | ImageUploader |

---

## Breaking Changes Catalog

### .NET 10.0 Framework Breaking Changes

**General (All Projects)**:
- ⚠️ **Preview Status**: .NET 10.0 is in preview - expect potential breaking changes between preview releases
- **API Removals**: Check for obsolete APIs removed in .NET 10.0
- **Behavior Changes**: Runtime behavior may differ from .NET 8.0
- **TFM Changes**: Ensure all tooling recognizes `net10.0` TFM

**Specific Areas to Watch**:
1. **Collections**: LINQ and collection API changes
2. **Serialization**: JSON serialization behavior
3. **Networking**: HTTP client and networking stack changes
4. **Threading**: Task and async/await enhancements

### Blazor WebAssembly (CoinFlipGame.App)

**Microsoft.AspNetCore.Components.WebAssembly 10.0**:
- **Component Lifecycle**: Potential changes to lifecycle methods
- **Rendering**: Enhanced rendering pipeline may affect custom components
- **JS Interop**: IJSRuntime interface changes or new methods
- **Router**: Routing configuration or navigation behavior
- **Authentication**: If using auth, check for provider changes

**Areas Requiring Attention**:
- `Home.razor.cs`: Heavy use of JSInterop - verify all methods
- Component lifecycle hooks (`OnAfterRenderAsync`, etc.)
- State management and `StateHasChanged` timing
- `CancellationTokenSource` usage patterns

### Azure Functions Worker v2 (CoinFlipGame.Api)

**Microsoft.Azure.Functions.Worker 1.x → 2.x**:
- **BREAKING**: Initialization model changed - `Program.cs` structure
- **BREAKING**: Middleware pipeline registration syntax
- **BREAKING**: Dependency injection container differences
- **HTTP Triggers**: Binding model may have changed
- **Function Context**: Context object properties/methods
- **Logging**: Integration with ILogger may differ

**Expected Code Changes**:
1. Update `Program.cs`:
   ```csharp
   // OLD (v1.x):
   var host = new HostBuilder()
       .ConfigureFunctionsWorkerDefaults()
       .Build();
   
   // NEW (v2.x) - likely pattern:
   var host = new HostBuilder()
       .ConfigureFunctionsWebApplication()
       .Build();
   ```

2. HTTP Trigger Changes:
   - Check `HttpRequestData` and `HttpResponseData` usage
   - Verify routing attributes
   - Update response creation patterns

**Microsoft.EntityFrameworkCore.SqlServer 10.0**:
- **Query Translation**: LINQ queries may translate differently
- **Migration Changes**: Check generated migrations
- **Provider Behavior**: SQL Server provider enhancements
- **Connection Handling**: Connection resiliency changes

### Console Application (CoinFlipGame.ImageUploader)

**Microsoft.Extensions.*** 10.0 Packages:
- **Configuration**: Builder API changes
- **Dependency Injection**: Container registration patterns
- **Logging**: Provider configuration syntax
- **Host Builder**: Generic host initialization

**Potential Changes**:
- `Program.cs` structure for configuration and DI
- Service registration syntax
- Logging provider setup

### SQL Database Project (CoinFlipGame.DB)

**SDK-Style Conversion**:
- **Project Structure**: File organization changes
- **Build Targets**: MSBuild target differences
- **Tooling**: Visual Studio/CLI compatibility
- **DACPAC**: Package generation process

---

## Testing Strategy

### Phase-by-Phase Testing

#### Phase 0: Pre-Upgrade Validation
**Before making any changes**:
- [ ] Verify current solution builds successfully on .NET 8.0
- [ ] Document current test baseline (if tests exist)
- [ ] Ensure all projects can be restored and built
- [ ] Create backup branch or tag

#### Phase 1: Atomic Upgrade Testing
**After all projects updated (single operation)**:
1. **Restore Dependencies**: `dotnet restore`
2. **Build Solution**: `dotnet build --configuration Release`
3. **Validation Checkpoints**:
   - [ ] Zero build errors across all projects
   - [ ] Zero build warnings (review any warnings)
   - [ ] All projects resolve dependencies correctly
   - [ ] No package version conflicts

#### Phase 2: Application Testing

**CoinFlipGame.App (Blazor WebAssembly)**:
1. **Build and Run**:
   ```bash
   cd CoinFlipGame.App
   dotnet run
   ```
2. **Manual Testing Scenarios**:
   - [ ] Application loads in browser without errors
   - [ ] Coin flip animation works
   - [ ] Drag interactions functional
   - [ ] Super flip charging mechanism works
   - [ ] Sound toggle functions correctly
   - [ ] Haptic feedback works (on supported devices)
   - [ ] Local storage saves preferences
   - [ ] Modal dialogs open/close correctly
   - [ ] Coin selector drawer works
   - [ ] Achievement notifications appear
   - [ ] Referrer bonus logic functions
3. **Console Checks**:
   - [ ] No JavaScript errors in browser console
   - [ ] No Blazor framework errors
   - [ ] JSInterop calls succeed

**CoinFlipGame.Api (Azure Functions)**:
1. **Local Testing**:
   ```bash
   cd CoinFlipGame.Api
   func start
   ```
2. **Endpoint Testing**:
   - [ ] Functions runtime starts successfully
   - [ ] HTTP endpoints respond
   - [ ] Database connections succeed
   - [ ] Blob storage operations work
   - [ ] Caching functions correctly
   - [ ] Error handling works
3. **Integration Testing** (if App depends on Api):
   - [ ] App can call Api endpoints
   - [ ] Data flows correctly
   - [ ] Authentication works (if applicable)

**CoinFlipGame.ImageUploader (Console)**:
1. **Execution Test**:
   ```bash
   cd CoinFlipGame.ImageUploader
   dotnet run -- [args]
   ```
2. **Validation**:
   - [ ] Application executes without errors
   - [ ] Images processed correctly
   - [ ] Blob uploads succeed
   - [ ] Logging output correct
   - [ ] Exit codes appropriate

**CoinFlipGame.Lib & CoinFlipGame.Shared**:
- Validated implicitly through consuming projects (Api, App)
- [ ] No build warnings
- [ ] Dependencies resolved correctly

**CoinFlipGame.DB (SQL Project)**:
1. **Build Verification**:
   - [ ] Project builds successfully
   - [ ] DACPAC generated (if applicable)
2. **Deployment Test** (optional):
   - [ ] Scripts deploy to test database
   - [ ] Schema matches expectations

### Smoke Tests

**Quick Validation After Upgrade** (minimal tests to confirm basic functionality):

1. **Solution Level**:
   ```bash
   dotnet restore
   dotnet build --configuration Release
   ```
   - Exit code 0 = success

2. **Blazor App**:
   - Load in browser
   - Perform one coin flip
   - Check for errors

3. **Azure Functions**:
   - Start local runtime
   - Call one endpoint
   - Verify response

4. **Console App**:
   - Run with minimal arguments
   - Verify execution completes

### Comprehensive Validation

**Before Marking Upgrade Complete**:

1. **Automated Tests** (if present):
   - [ ] All unit tests pass
   - [ ] All integration tests pass
   - [ ] Code coverage maintained

2. **Performance Benchmarks**:
   - [ ] Application startup time acceptable
   - [ ] API response times within thresholds
   - [ ] Blazor rendering performance adequate

3. **Security Scans**:
   - [ ] No new security vulnerabilities
   - [ ] Package vulnerability scan clean
   - [ ] Authentication/authorization intact (if applicable)

4. **End-to-End Scenarios**:
   - [ ] Complete user workflows function correctly
   - [ ] Data persists correctly
   - [ ] Error scenarios handled appropriately

---

## Risk Management

### High-Risk Changes

| Project | Risk | Severity | Mitigation |
|---------|------|----------|------------|
| CoinFlipGame.Api | Azure Functions Worker v1 → v2 | **HIGH** | - Review v2 migration guide thoroughly<br>- Test all HTTP endpoints<br>- Verify DI and middleware<br>- Have rollback plan ready |
| CoinFlipGame.App | Blazor WebAssembly on .NET 10 Preview | **MEDIUM-HIGH** | - Test all JSInterop calls<br>- Verify component lifecycle<br>- Test on multiple browsers<br>- Monitor preview release notes |
| CoinFlipGame.DB | SDK-style conversion | **MEDIUM** | - Backup project first<br>- Consider keeping on .NET Framework if conversion problematic<br>- Test DACPAC generation |
| All Projects | .NET 10.0 Preview Status | **MEDIUM** | - Acknowledge preview instability<br>- Don't deploy to production<br>- Be prepared for preview updates |
| CoinFlipGame.Api | Entity Framework Core 10.0 | **MEDIUM** | - Test all database queries<br>- Verify migrations<br>- Check LINQ translations<br>- Backup database for testing |

### Risk Mitigation

**For Azure Functions Worker v2**:
1. Read official migration documentation: [https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
2. Create feature branch for testing before merging to main
3. Test locally with Azure Storage Emulator/Azurite
4. Verify all HTTP bindings and triggers
5. Check DI service registrations
6. Test error handling and logging

**For .NET 10.0 Preview**:
1. Subscribe to .NET 10 preview release notes
2. Test on dedicated development machines only
3. Don't deploy preview builds to production
4. Be prepared to update packages as previews release
5. Report breaking changes to .NET team if encountered

**For SQL Project**:
1. Attempt SDK conversion in isolated branch first
2. If conversion fails/complex, consider keeping on .NET Framework
3. Evaluate Microsoft.Build.Sql SDK as alternative
4. Consult SQL tooling documentation

**For Blazor WebAssembly**:
1. Test on Chrome, Firefox, Edge, Safari
2. Verify mobile browser compatibility
3. Check Web Assembly size and load time
4. Monitor browser console for errors
5. Test offline/PWA functionality (if applicable)

### Contingency Plans

#### If Primary Plan Encounters Blocking Issues:

**Scenario 1: Azure Functions Worker v2 Breaking Changes Too Severe**
- **Fallback**: Stay on Azure Functions Worker v1.x temporarily
- **Action**: Pin packages to v1.x, upgrade only framework to net10.0
- **Risk**: May encounter compatibility issues with .NET 10.0
- **Timeline**: Defer Functions Worker v2 until stable documentation available

**Scenario 2: .NET 10.0 Preview Instability**
- **Fallback**: Downgrade to .NET 9.0 (STS, stable)
- **Action**: Change all `<TargetFramework>` to `net9.0`
- **Risk**: Lose access to .NET 10.0 features
- **Timeline**: Wait for .NET 10.0 RC or RTM before attempting again

**Scenario 3: SQL Project SDK Conversion Fails**
- **Fallback**: Keep SQL project on .NET Framework 4.7.2
- **Action**: Exclude from atomic upgrade, manage separately
- **Risk**: Solution has mixed .NET versions
- **Timeline**: Migrate SQL project in separate phase when tooling mature

**Scenario 4: Multiple Projects Fail to Build**
- **Fallback**: Rollback entire upgrade using git
- **Action**: `git reset --hard <pre-upgrade-commit>`
- **Risk**: Lost upgrade progress
- **Timeline**: Reassess approach, consider incremental migration

**Scenario 5: Blazor App Runtime Errors**
- **Fallback**: Isolate and rollback Blazor app only
- **Action**: Revert CoinFlipGame.App and CoinFlipGame.Shared to net8.0
- **Risk**: Mixed framework versions in solution
- **Timeline**: Debug Blazor issues separately before full upgrade

### Rollback Procedures

**Recommended Source Control Strategy**:
- Commit after each major milestone
- Use descriptive commit messages
- Tag important checkpoints

**Rollback Steps**:

1. **If Issues During Upgrade** (before testing):
   ```bash
   git reset --hard HEAD~1  # Undo last commit
   # or
   git checkout <branch-before-upgrade>
   ```

2. **If Issues After Testing** (after commits):
   ```bash
   git revert <commit-range>  # Create revert commits
   # or
   git reset --hard <pre-upgrade-tag>
   git push --force origin upgrade-to-NET10  # If already pushed
   ```

3. **Partial Rollback** (specific project):
   ```bash
   git checkout <pre-upgrade-commit> -- CoinFlipGame.Api/CoinFlipGame.Api.csproj
   git commit -m "Rollback CoinFlipGame.Api to net8.0"
   ```

4. **Clean Workspace After Rollback**:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

---

## Source Control Strategy

### Branching Strategy
- **Main upgrade branch**: `upgrade-to-NET10` (already created)
- **Source branch**: `main`
- **Integration**: Merge `upgrade-to-NET10` → `main` after successful testing

### Commit Strategy
**Big Bang Strategy Recommendation**: Single comprehensive commit for atomic upgrade

**Commit Structure**:
```
Upgrade all projects to .NET 10.0

- Update all project target frameworks: net8.0 → net10.0
- Update Blazor WebAssembly packages to 10.0.0
- Update Azure Functions Worker to v2.51.0 (major update)
- Update Entity Framework Core to 10.0.0
- Update Microsoft.Extensions.* packages to 10.0.0
- Convert CoinFlipGame.DB to SDK-style project
- Address breaking changes in Azure Functions Worker v2
- Verify all projects build successfully

BREAKING CHANGE: Azure Functions Worker v1 → v2
BREAKING CHANGE: .NET 10.0 preview may have unstable APIs

Projects affected:
- CoinFlipGame.App (Blazor WebAssembly)
- CoinFlipGame.Api (Azure Functions)
- CoinFlipGame.ImageUploader (Console)
- CoinFlipGame.Lib (Class Library)
- CoinFlipGame.Shared (Class Library)
- CoinFlipGame.DB (SQL Project - SDK conversion)

All projects build: ✅
All tests pass: [status]
Manual testing: [status]
```

**Alternative: Checkpoint Commits** (if atomic commit fails):
1. `feat: update project target frameworks to net10.0`
2. `feat: update Blazor WebAssembly packages to 10.0.0`
3. `feat: upgrade Azure Functions Worker to v2 with breaking changes`
4. `feat: update all Microsoft.Extensions packages`
5. `fix: address breaking changes and build errors`
6. `test: verify all projects and run tests`

### Commit Message Format
Follow Conventional Commits format:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Example**:
```
feat(upgrade): atomic .NET 10.0 upgrade for all projects

Update all projects from .NET 8.0 to .NET 10.0 in single operation.
Includes major package updates and breaking change mitigations.

- All project target frameworks: net10.0
- Azure Functions Worker: v2.51.0
- Entity Framework Core: 10.0.0
- Blazor WebAssembly: 10.0.0
- Microsoft.Extensions.*: 10.0.0

BREAKING CHANGE: Azure Functions Worker v2 initialization model changed
BREAKING CHANGE: .NET 10.0 preview APIs may be unstable

Tested: [x] Build [ ] Tests [ ] Manual
```

### Review and Merge Process

**PR/MR Requirements**:
- [ ] All projects build successfully without errors
- [ ] No new warnings introduced (or all justified)
- [ ] Breaking changes documented
- [ ] Testing checklist completed
- [ ] Rollback plan documented

**Review Checklist**:
- [ ] Target framework changes verified in all .csproj files
- [ ] Package versions correct and consistent
- [ ] Azure Functions Worker v2 migration addressed
- [ ] Blazor app tested in browser
- [ ] SQL project conversion successful (or excluded)
- [ ] No TODO/FIXME comments left unresolved
- [ ] Commit messages clear and descriptive

**Merge Criteria**:
- [ ] PR approved by reviewer(s)
- [ ] CI/CD pipeline passes (if configured)
- [ ] Manual testing completed successfully
- [ ] Documentation updated (README, changelog)
- [ ] Team informed of breaking changes

**Integration Validation**:
1. Merge `upgrade-to-NET10` → `main`
2. Verify `main` branch builds
3. Tag release: `v10.0.0-preview1` (or appropriate version)
4. Update documentation with .NET 10.0 requirements

---

## Success Criteria

### Technical Success Criteria

**Build Success**:
- [ ] All projects migrated to target framework net10.0
- [ ] All packages updated to specified versions
- [ ] Zero build errors across entire solution
- [ ] Zero new build warnings (or all justified and documented)

**Runtime Success**:
- [ ] CoinFlipGame.App (Blazor) runs without errors in browser
- [ ] All HTTP endpoints in CoinFlipGame.Api respond correctly
- [ ] CoinFlipGame.ImageUploader executes successfully
- [ ] No runtime exceptions during normal operation

**Dependency Success**:
- [ ] All package dependencies resolve correctly
- [ ] No package version conflicts
- [ ] No security vulnerabilities in dependencies
- [ ] Project references build in correct order

### Quality Criteria

**Code Quality**:
- [ ] Code compiles without warnings
- [ ] No code smells introduced during migration
- [ ] Breaking changes addressed with proper patterns
- [ ] Error handling remains robust

**Testing**:
- [ ] All automated tests pass (if present)
- [ ] Manual testing scenarios completed
- [ ] Integration between projects verified
- [ ] Performance within acceptable thresholds

**Documentation**:
- [ ] README.md updated with .NET 10.0 requirements
- [ ] Breaking changes documented
- [ ] Upgrade notes created for team
- [ ] Known issues listed (if any)

### Process Criteria

**Source Control**:
- [ ] Big Bang Strategy principles followed throughout migration
- [ ] All changes committed with clear messages
- [ ] Atomic upgrade commit created (or logical checkpoint commits)
- [ ] Branch ready for merge to main

**Team Readiness**:
- [ ] Team informed of .NET 10.0 preview status
- [ ] SDK installation instructions provided
- [ ] Breaking changes communicated
- [ ] Rollback procedures understood

**Deployment Readiness**:
- [ ] CI/CD pipeline updated for .NET 10.0 (if applicable)
- [ ] Deployment scripts updated
- [ ] Environment configuration verified
- [ ] Monitoring/logging validated

### Preview-Specific Criteria

**Given .NET 10.0 Preview Status**:
- [ ] Acknowledge solution is on preview framework
- [ ] Document plan for tracking preview updates
- [ ] Establish process for updating to RC/RTM
- [ ] Identify preview-only environments (no production use)
- [ ] Set expectations for potential instability

### Scenario-Specific Success Criteria

**Azure Functions Worker v2**:
- [ ] All HTTP triggers functional
- [ ] Middleware pipeline working
- [ ] Dependency injection configured correctly
- [ ] Logging integrated properly

**Blazor WebAssembly**:
- [ ] Application loads without errors
- [ ] JSInterop calls succeed
- [ ] Component lifecycle intact
- [ ] Browser compatibility verified

**SQL Project**:
- [ ] SDK-style conversion complete (or exclusion justified)
- [ ] DACPAC generation successful (if applicable)
- [ ] Database deployment tested

---

## Implementation Timeline

### Estimated Effort

| Phase | Tasks | Estimated Time | Complexity | Risk Level |
|-------|-------|---------------|------------|------------|
| **Phase 0: Preparation** | SDK verification, SQL conversion | 30-60 min | Low | Low |
| **Phase 1: Atomic Upgrade** | All framework + package updates, restore, build | 45-90 min | Medium | Medium-High |
| **Phase 2: Fix Compilation Errors** | Address breaking changes, fix builds | 60-180 min | High | High |
| **Phase 3: Testing** | Manual testing all projects | 90-180 min | Medium | Medium |
| **Total Estimated Time** | | **3.5-7.5 hours** | | |

**Factors Affecting Timeline**:
- ✅ Small solution size (6 projects) reduces complexity
- ⚠️ Azure Functions Worker v2 migration may take longer if breaking changes severe
- ⚠️ .NET 10.0 preview unknowns may cause delays
- ⚠️ SQL project SDK conversion difficulty uncertain
- ✅ Clear dependency graph simplifies coordination

### Phase Durations

#### Phase 0: Preparation (30-60 minutes)
**Tasks**:
1. Verify .NET 10.0 SDK installed (10 min)
2. Backup current state / Create tag (5 min)
3. Attempt SQL project SDK conversion (15-45 min)
   - If successful: Continue
   - If problematic: Document and defer

**Deliverables**: 
- .NET 10.0 SDK verified
- Backup/tag created
- SQL project converted or exclusion justified

#### Phase 1: Atomic Upgrade (45-90 minutes)
**Tasks** (all in single operation):
1. Update all 6 project target frameworks to net10.0 (10 min)
2. Update all package references to specified versions (15 min)
3. Restore dependencies: `dotnet restore` (5 min)
4. Build solution: `dotnet build` (10 min)
5. Review build output for errors/warnings (15-50 min)

**Deliverables**:
- All projects updated to net10.0
- All packages updated
- Build attempted (may have errors)

#### Phase 2: Fix Compilation Errors (60-180 minutes)
**Tasks**:
1. Address Azure Functions Worker v2 breaking changes (30-90 min)
   - Update `Program.cs` initialization
   - Fix HTTP trigger bindings
   - Update middleware registration
2. Fix Blazor WebAssembly issues (if any) (15-30 min)
3. Address any Microsoft.Extensions.* changes (10-20 min)
4. Resolve Entity Framework Core issues (if any) (10-30 min)
5. Fix any remaining compilation errors (10-30 min)
6. Rebuild solution: `dotnet build --configuration Release` (5 min)

**Deliverables**:
- Zero compilation errors
- Zero warnings (or all justified)
- Clean release build

#### Phase 3: Testing (90-180 minutes)
**Tasks**:
1. **Blazor App Testing** (40-80 min):
   - Run application locally
   - Test coin flip mechanics
   - Verify JSInterop functionality
   - Test local storage
   - Browser compatibility check
2. **Azure Functions Testing** (30-60 min):
   - Start Functions runtime locally
   - Test all endpoints
   - Verify database operations
   - Check blob storage
3. **Console App Testing** (10-20 min):
   - Execute with test parameters
   - Verify image processing
   - Check blob uploads
4. **Integration Testing** (10-20 min):
   - Test App → Api communication (if applicable)
   - End-to-end scenario validation

**Deliverables**:
- All manual tests passed
- No runtime errors discovered
- Integration verified

### Buffer Time
**Recommended**: Add 50% buffer for unexpected issues
- Base estimate: 3.5-7.5 hours
- With buffer: **5-11 hours total**

**Reasons for Buffer**:
- .NET 10.0 preview unknowns
- Azure Functions Worker v2 migration complexity
- SQL project conversion uncertainty
- Potential hidden breaking changes

### Resource Requirements

**Developer Skills**:
- **Required**: .NET/C# proficiency, Blazor knowledge
- **Recommended**: Azure Functions experience, EF Core familiarity
- **Helpful**: SQL project tooling knowledge

**Tools**:
- Visual Studio 2022 (latest preview) or Visual Studio Code
- .NET 10.0 SDK
- Azure Functions Core Tools (latest)
- Git for source control
- Azure Storage Emulator or Azurite (for Functions testing)

**Team Capacity**:
- Primary developer: 1 person (full-time for 1-2 days)
- Reviewer/tester: 1 person (2-4 hours for review)
- Subject matter expert (Azure Functions): Available for consultation

---

## Planning Principles Applied

### Dependency-First Ordering
✅ **Applied**: Migration order respects dependency graph (Shared/Lib → Api/App → ImageUploader)

### Data-Driven Decisions
✅ **Based on assessment.md**:
- Exact current/target framework versions used
- Specific package version numbers from analysis
- Project metrics (LOC, dependencies) referenced
- Compatibility flags from assessment

### Completeness Over Assumptions
✅ **All packages addressed**:
- Compatible packages explicitly noted (no update needed)
- All recommended updates included in plan
- No packages skipped without justification

### Specificity
✅ **Plan provides exact details**:
- Specific version numbers for all packages
- File paths to project files
- Exact TargetFramework values
- Code examples for breaking changes

### Actionability
✅ **Every step executable**:
- Clear instructions for project updates
- Validation checklists after each change
- Specific commands to run (`dotnet build`, etc.)
- Testing procedures defined

### Risk Awareness
✅ **High-risk items identified**:
- Azure Functions Worker v2 flagged as major update
- .NET 10.0 preview status acknowledged
- SQL project SDK conversion complexity noted
- Mitigation strategies provided for each risk

### Incremental Progress
✅ **Big Bang Strategy with atomic execution**:
- All changes in single coordinated operation
- Solution remains buildable after atomic upgrade
- Testing phases ensure validation

### Strategy Adherence
✅ **Big Bang Strategy principles followed**:
- All projects upgraded simultaneously (respecting dependencies)
- Single comprehensive commit recommended
- Atomic operation ensures consistency
- Testing after complete upgrade

### Source Control Integration
✅ **Source control strategy defined**:
- Branching strategy documented
- Commit message format specified
- Review/merge criteria provided
- Rollback procedures outlined

### Planning vs Execution Separation
✅ **Plan documents "what", not "how mechanically"**:
- Imperative language used ("Update the package", "Change the target framework")
- Focus on specifications and requirements
- Executor will handle mechanical implementation

---

## Conclusion

This plan provides a comprehensive roadmap for upgrading the CoinFlipGame solution from .NET 8.0 to .NET 10.0 using the Big Bang Strategy. The atomic upgrade approach is well-suited to this small solution with clear dependencies.

**Key Success Factors**:
1. **Preview Awareness**: .NET 10.0 is preview - expect potential instability
2. **Azure Functions Focus**: Worker v2 migration is highest risk area
3. **Thorough Testing**: Manual testing critical given preview status
4. **Rollback Readiness**: Have clear rollback plan before starting
5. **SQL Project Flexibility**: Be prepared to defer SQL project if conversion problematic

**Recommended Next Steps**:
1. Review this plan with team
2. Schedule dedicated time block (full day recommended)
3. Ensure .NET 10.0 SDK installed
4. Create backup/tag before starting
5. Execute atomic upgrade
6. Test thoroughly
7. Document lessons learned for future preview upgrades

**Post-Upgrade**:
- Monitor .NET 10.0 preview releases for breaking changes
- Plan for updates to RC and RTM when available
- Share feedback with .NET team if issues encountered
- Update CI/CD for .NET 10.0 (if applicable)

Good luck with the upgrade! 🚀
