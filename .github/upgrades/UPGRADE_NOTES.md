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
**Target Framework**: .NET 10.0  
**Strategy**: Big Bang (atomic upgrade)  
**Projects Upgraded**: 5 of 6 projects
- ? CoinFlipGame.Shared
- ? CoinFlipGame.Lib  
- ? CoinFlipGame.Api
- ? CoinFlipGame.App
- ? CoinFlipGame.ImageUploader
- ?? CoinFlipGame.DB (excluded)
