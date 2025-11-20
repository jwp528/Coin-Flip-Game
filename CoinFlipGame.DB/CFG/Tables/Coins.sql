-- =============================================
-- Create Table: CFG.Coins
-- Description: Stores coin data with JSON properties for unlock criteria, prerequisites, and effects
-- =============================================

CREATE TABLE [CFG].[Coins]
(
    -- Primary Key
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    
    -- Core Properties
    [Name] NVARCHAR(100) NOT NULL,
    [FlavorText] NVARCHAR(500) NULL,
    [UnlockDescription] NVARCHAR(500) NULL,
    [Path] NVARCHAR(500) NOT NULL,
    
    -- Classification
    [CoinType] NVARCHAR(50) NOT NULL,
    [Category] NVARCHAR(100) NULL,
    [Rarity] NVARCHAR(50) NULL,
    
    -- Unlock Settings
    [IsAlwaysUnlocked] BIT NOT NULL DEFAULT 0,
    
    -- JSON Properties (stored as NVARCHAR(MAX) for flexibility)
    [UnlockCriteria] NVARCHAR(MAX) NULL,
    [Prerequisites] NVARCHAR(MAX) NULL,
    [Effects] NVARCHAR(MAX) NULL,
    
    -- Metadata
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [SortOrder] INT NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_Coins] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

-- =============================================
-- Create Indexes for CFG.Coins
-- =============================================

-- Index on CoinType for filtering by type
CREATE NONCLUSTERED INDEX [IX_Coins_CoinType]
    ON [CFG].[Coins] ([CoinType]);
GO

-- Unique index on Path to ensure no duplicate paths
CREATE UNIQUE NONCLUSTERED INDEX [IX_Coins_Path]
    ON [CFG].[Coins] ([Path]);
GO

-- Index on Name for searching by name
CREATE NONCLUSTERED INDEX [IX_Coins_Name]
    ON [CFG].[Coins] ([Name]);
GO

-- Index on IsActive for filtering active coins
CREATE NONCLUSTERED INDEX [IX_Coins_IsActive]
    ON [CFG].[Coins] ([IsActive]);
GO

-- Index on Category for filtering by category
CREATE NONCLUSTERED INDEX [IX_Coins_Category]
    ON [CFG].[Coins] ([Category]);
GO

-- Composite index on IsActive and CoinType for common query patterns
CREATE NONCLUSTERED INDEX [IX_Coins_IsActive_CoinType]
    ON [CFG].[Coins] ([IsActive], [CoinType])
    INCLUDE ([Name], [Path], [Category]);
GO
