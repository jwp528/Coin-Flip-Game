# .NET 10.0 Upgrade Notes

## Excluded Projects

### CoinFlipGame.DB.sqlproj
**Status**: Excluded from .NET 10.0 upgrade  
**Reason**: Complex SDK-style conversion required

**Details**:
- Current state: Classic SSDT project targeting .NETFramework v4.7.2
- Conversion complexity: SQL Server Database Projects (SSDT) require specialized tooling and don't have direct SDK-style equivalents
- Impact: None - Project is isolated with no dependencies or dependants
- Recommendation: Keep on current .NET Framework or migrate separately using Microsoft.Build.Sql SDK when tooling matures

**Follow-up Actions**:
- Consider migration to Microsoft.Build.Sql SDK in separate initiative
- Evaluate standalone SQL tooling alternatives
- SQL project can remain functional on .NET Framework alongside upgraded .NET 10.0 projects

## Upgrade Summary

**Date**: 2025-01-26  
**Target Framework**: .NET 9.0 (downgraded from 10.0 for Azure Functions compatibility)  
**Strategy**: Big Bang (atomic upgrade)  
**Projects Upgraded**: 5 of 6 projects
- ? CoinFlipGame.Shared (net9.0)
- ? CoinFlipGame.Lib (net9.0)
- ? CoinFlipGame.Api (net9.0)
- ? CoinFlipGame.App (net10.0) 
- ? CoinFlipGame.ImageUploader (net10.0)
- ?? CoinFlipGame.DB (excluded)

## Post-Upgrade Adjustments

### Azure Functions Runtime Compatibility Issue
**Date**: 2025-11-26  
**Issue**: Azure Functions Core Tools v4.2.2 doesn't support .NET 10.0 preview

**Resolution**:
- Downgraded CoinFlipGame.Api, CoinFlipGame.Lib, and CoinFlipGame.Shared to .NET 9.0
- Updated packages to .NET 9.0 compatible versions:
  - Microsoft.EntityFrameworkCore.SqlServer: 10.0.0 ? 9.0.0
  - Microsoft.Extensions.Caching.Memory: 10.0.0 ? 9.0.0
- Kept CoinFlipGame.App and CoinFlipGame.ImageUploader on .NET 10.0 (no runtime conflicts)

**Verification**: ? Azure Functions now starts successfully on port 7071

**Recommendation**: 
- Keep this configuration until Azure Functions Core Tools adds .NET 10.0 support
- Monitor Azure Functions updates for .NET 10.0 compatibility
- Consider full .NET 10.0 upgrade when Azure Functions v5 is released

