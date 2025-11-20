# CoinFlipGame.Database

SQL Server Database Project for the Coin Flip Game.

## Structure

```
CoinFlipGame.Database/
??? Schema/           # Schema definitions
?   ??? CFG.sql      # CFG schema creation
??? Tables/          # Table definitions
?   ??? Coins.sql    # Coins table with indexes
??? Data/            # Data scripts
?   ??? SampleCoins.sql  # Sample coin data
??? Scripts/         # Deployment scripts
    ??? Deploy.sql   # Master deployment script
```

## Deployment Options

### Option 1: Using SQL Server Management Studio (SSMS)

1. Open SQL Server Management Studio
2. Connect to your target database server
3. Open `Scripts\Deploy.sql`
4. Enable SQLCMD Mode: **Query ? SQLCMD Mode**
5. Execute the script

### Option 2: Using SQLCMD Command Line

From the `CoinFlipGame.Database` directory:

```bash
sqlcmd -S (localdb)\mssqllocaldb -d CoinFlipGameDb -i Scripts\Deploy.sql
```

For SQL Server Express:
```bash
sqlcmd -S .\SQLEXPRESS -d CoinFlipGameDb -i Scripts\Deploy.sql
```

For Azure SQL or remote server:
```bash
sqlcmd -S your-server.database.windows.net -d CoinFlipGameDb -U your-username -P your-password -i Scripts\Deploy.sql
```

### Option 3: Run Individual Scripts

If you prefer to run scripts individually:

```sql
-- 1. Create Schema
:r .\Schema\CFG.sql

-- 2. Create Tables
:r .\Tables\Coins.sql

-- 3. Insert Sample Data (optional)
:r .\Data\SampleCoins.sql
```

### Option 4: Using PowerShell

```powershell
# Set variables
$Server = "(localdb)\mssqllocaldb"
$Database = "CoinFlipGameDb"
$ScriptPath = ".\Scripts\Deploy.sql"

# Run deployment
Invoke-Sqlcmd -ServerInstance $Server -Database $Database -InputFile $ScriptPath
```

## Creating the Database

If the database doesn't exist yet, create it first:

```sql
CREATE DATABASE CoinFlipGameDb
GO
```

Or using SQLCMD:
```bash
sqlcmd -S (localdb)\mssqllocaldb -Q "CREATE DATABASE CoinFlipGameDb"
```

## Schema: CFG

All database objects are created under the `CFG` schema to keep them organized and separate from the default `dbo` schema.

## Tables

### CFG.Coins

Stores coin data with JSON properties.

**Columns:**
- `Id` (UNIQUEIDENTIFIER) - Primary key
- `Name` (NVARCHAR(100)) - Coin name
- `FlavorText` (NVARCHAR(500)) - Descriptive text
- `UnlockDescription` (NVARCHAR(500)) - How to unlock
- `Path` (NVARCHAR(500)) - Image path (unique)
- `CoinType` (INT) - Type enum value
- `Category` (NVARCHAR(100)) - Category name
- `Rarity` (NVARCHAR(50)) - Rarity level
- `IsAlwaysUnlocked` (BIT) - Default unlock flag
- `UnlockCriteria` (NVARCHAR(MAX)) - JSON unlock data
- `Prerequisites` (NVARCHAR(MAX)) - JSON prerequisites
- `Effects` (NVARCHAR(MAX)) - JSON effects data
- `CreatedAt` (DATETIME2) - Creation timestamp (UTC)
- `UpdatedAt` (DATETIME2) - Last update timestamp
- `IsActive` (BIT) - Active flag
- `SortOrder` (INT) - Display order

**Indexes:**
- `PK_Coins` - Primary key on `Id`
- `IX_Coins_CoinType` - Index on `CoinType`
- `IX_Coins_Path` - Unique index on `Path`
- `IX_Coins_Name` - Index on `Name`
- `IX_Coins_IsActive` - Index on `IsActive`
- `IX_Coins_Category` - Index on `Category`
- `IX_Coins_IsActive_CoinType` - Composite index with includes

## Sample Data

The `Data\SampleCoins.sql` script inserts 10 sample coins demonstrating:
- Always unlocked coins (JP Logo)
- Total flips unlock (Gemini, 10 Flips Achievement)
- Heads/Tails flips unlock (Ram, Tauros)
- Streak unlock (3 Head Streak, Weighted Coin)
- Random chance unlock (Rabbit)
- Coins with effects (Auto Clicker, Weighted, Lucky)

**Note:** Sample data is only inserted if the table is empty. It won't duplicate data on subsequent runs.

## Modifying the Database

### Adding New Tables

1. Create a new `.sql` file in the `Tables\` directory
2. Add table creation script with idempotent checks
3. Add to `Scripts\Deploy.sql`

### Adding Stored Procedures

1. Create `.sql` files in the `StoredProcedures\` directory
2. Use `CREATE OR ALTER PROCEDURE` for idempotency
3. Add to deployment script if needed

### Schema Changes

To modify existing tables:

1. Create an ALTER script in `Tables\` directory
2. Name it descriptively (e.g., `Coins_AddNewColumn.sql`)
3. Use idempotent checks (IF NOT EXISTS)
4. Add to deployment script

Example:
```sql
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('[CFG].[Coins]') 
               AND name = 'NewColumn')
BEGIN
    ALTER TABLE [CFG].[Coins]
    ADD [NewColumn] NVARCHAR(100) NULL
END
GO
```

## Connection Strings

### Local Development (LocalDB)
```
Server=(localdb)\mssqllocaldb;Database=CoinFlipGameDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

### SQL Server Express
```
Server=.\SQLEXPRESS;Database=CoinFlipGameDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

### Azure SQL
```
Server=tcp:yourserver.database.windows.net,1433;Database=CoinFlipGameDb;User ID=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Verification Queries

After deployment, verify the setup:

```sql
-- Check schema exists
SELECT * FROM sys.schemas WHERE name = 'CFG'

-- Check table exists
SELECT * FROM sys.tables WHERE name = 'Coins' AND schema_id = SCHEMA_ID('CFG')

-- Check indexes
SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('[CFG].[Coins]')

-- View sample data
SELECT 
    Name, 
    CoinType, 
    Category, 
    Rarity,
    IsAlwaysUnlocked,
    CASE WHEN UnlockCriteria IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasUnlockCriteria,
    CASE WHEN Effects IS NOT NULL THEN 'Yes' ELSE 'No' END AS HasEffects
FROM [CFG].[Coins]
ORDER BY SortOrder
```

## Best Practices

1. **Always use SQLCMD mode** when running deployment scripts
2. **Test on development first** before deploying to production
3. **Backup the database** before running schema changes
4. **Use idempotent scripts** - all scripts check if objects exist before creating
5. **Version control** - Keep all SQL scripts in source control

## Troubleshooting

### "Must declare the scalar variable" error
- Make sure SQLCMD Mode is enabled in SSMS
- Or use `sqlcmd` command line tool

### "Object already exists" error
- Scripts are idempotent and should handle this
- Check for typos in object names

### "Cannot find the file" error
- Ensure you're running from the correct directory
- Use absolute paths in the `:r` commands if needed

## Integration with API

The API project (`CoinFlipGame.Api`) uses Entity Framework Core to read from this database. The connection string in `local.settings.json` should point to the database created by these scripts.
