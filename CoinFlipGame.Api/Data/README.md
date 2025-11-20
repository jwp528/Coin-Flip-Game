# Coin Flip Game Database

## Overview
This database stores coin data for the Coin Flip Game using a custom `CFG` schema.

## Schema: CFG

### Table: Coins

Stores all coin data with JSON properties for complex data structures.

#### Columns

| Column | Type | Description |
|--------|------|-------------|
| `Id` | GUID | Primary key, unique identifier |
| `Name` | nvarchar(100) | Display name of the coin |
| `FlavorText` | nvarchar(500) | Descriptive flavor text |
| `UnlockDescription` | nvarchar(500) | Description of how to unlock |
| `Path` | nvarchar(500) | Full local path (e.g., /img/coins/AI/...) |
| `CoinType` | int | Enum value representing coin type |
| `Category` | nvarchar(100) | Category/group name |
| `Rarity` | nvarchar(50) | Rarity level (Common, Uncommon, Rare, Legendary) |
| `IsAlwaysUnlocked` | bit | Whether coin is unlocked by default |
| `UnlockCriteria` | nvarchar(max) | JSON - unlock condition details |
| `Prerequisites` | nvarchar(max) | JSON - list of prerequisite conditions |
| `Effects` | nvarchar(max) | JSON - special effect details |
| `CreatedAt` | datetime2 | When record was created (UTC) |
| `UpdatedAt` | datetime2 | When record was last updated |
| `IsActive` | bit | Whether coin is enabled in the game |
| `SortOrder` | int | Optional display order |

#### Indexes
- `IX_Coins_CoinType` - Index on CoinType
- `IX_Coins_Path` - Unique index on Path
- `IX_Coins_Name` - Index on Name
- `IX_Coins_IsActive` - Index on IsActive
- `IX_Coins_Category` - Index on Category

## JSON Column Structures

### UnlockCriteria
Maps to `UnlockConditionDto` class in C#.

```json
{
  "type": 1,
  "requiredCount": 10,
  "requiredCoinPath": "/img/coins/example.png",
  "description": "Complete 10 flips",
  "flavorText": "Your journey begins...",
  "unlockChance": 0.05,
  "requiredCoinPaths": ["/img/coins/coin1.png", "/img/coins/coin2.png"],
  "rarity": 0,
  "prerequisites": null,
  "requiresActiveCoin": false,
  "streakSide": null,
  "useDynamicCoinList": false,
  "characteristicFilter": 0,
  "filterUnlockConditionType": null,
  "filterEffectType": null,
  "filterPrerequisiteCount": 0,
  "sideRequirement": 0,
  "consecutiveCount": 1
}
```

**UnlockConditionType Enum:**
- 0 = None
- 1 = TotalFlips
- 2 = HeadsFlips
- 3 = TailsFlips
- 4 = Streak
- 5 = LandOnCoin
- 6 = RandomChance
- 7 = LandOnMultipleCoins
- 8 = LandOnCoinsWithCharacteristics

### Prerequisites
Array of `UnlockConditionDto` objects (same structure as UnlockCriteria).

```json
[
  {
    "type": 2,
    "requiredCount": 25,
    "description": "Must have landed heads 25 times"
  }
]
```

### Effects
Maps to `CoinEffectDto` class in C#.

```json
{
  "type": 1,
  "description": "Auto-clicks the coin",
  "autoClickInterval": 1000,
  "biasStrength": 0.1,
  "comboType": 0,
  "comboMultiplier": 0.05,
  "luckModifier": 0.05,
  "luckModifierType": 0
}
```

**CoinEffectType Enum:**
- 0 = None
- 1 = AutoClick
- 2 = Weighted
- 3 = Shaved
- 4 = Combo
- 5 = Luck

## CoinTypes Enum

| Value | Name |
|-------|------|
| 0 | JpLogo |
| 1 | Zodiak |
| 2 | Achievement |
| 3 | Random |
| 4 | Leather |
| 5 | Powers |
| 6 | Combo |

## Connection String

**Local Development (LocalDB):**
```
Server=(localdb)\\mssqllocaldb;Database=CoinFlipGameDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

Add to `local.settings.json`:
```json
{
  "ConnectionStrings": {
    "CoinFlipGameDb": "Server=(localdb)\\mssqllocaldb;Database=CoinFlipGameDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

## Database Deployment

This project uses a separate **SQL Server Database Project** (`CoinFlipGame.Database`) for schema management instead of EF Core migrations.

### Deploying the Database

See the `CoinFlipGame.Database` project for SQL scripts and deployment instructions.

**Quick Deploy:**
1. Open SSMS
2. Enable SQLCMD Mode
3. Run `CoinFlipGame.Database\Scripts\Deploy.sql`

**Or use command line:**
```bash
sqlcmd -S (localdb)\mssqllocaldb -d CoinFlipGameDb -i ..\CoinFlipGame.Database\Scripts\Deploy.sql
```

## Usage Examples

### Working with JSON Columns in C#

```csharp
using CoinFlipGame.Api.Data.Helpers;
using CoinFlipGame.Api.Models.DTOs;
using CoinFlipGame.Api.Models.Entities;

// Creating a coin with unlock criteria
var unlockCriteria = new UnlockConditionDto
{
    Type = UnlockConditionType.TotalFlips,
    RequiredCount = 10,
    Description = "Complete 10 total flips",
    FlavorText = "Every journey begins with a single flip.",
    Rarity = UnlockRarity.Common
};

var coin = new Coin
{
    Id = Guid.NewGuid(),
    Name = "Gemini",
    FlavorText = "Twin-faced and ever-changing",
    Path = "/img/coins/AI/Zodiak/Gemini.png",
    CoinType = CoinTypes.Zodiak,
    Category = "AI",
    Rarity = "Common",
    UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(unlockCriteria)
};

// Reading unlock criteria from a coin
var criteria = CoinJsonHelper.DeserializeUnlockCriteria(coin.UnlockCriteria);
```

## Notes

- All DateTime values are stored in UTC
- The CFG schema is used for all tables to keep database organized
- JSON columns allow flexible storage of complex nested data structures
- Helper classes (`CoinJsonHelper`) provide easy serialization/deserialization
- The database is designed to support future expansion without schema changes
