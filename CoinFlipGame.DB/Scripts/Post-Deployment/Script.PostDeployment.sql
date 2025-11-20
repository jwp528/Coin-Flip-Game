-- =============================================
-- Sample Data: CFG.Coins
-- Description: Insert sample coins demonstrating various unlock types and effects
-- =============================================

-- Only insert if table is empty
IF NOT EXISTS (SELECT 1 FROM [CFG].[Coins])
BEGIN
    PRINT 'Inserting sample coin data...'

    -- 1. JP Logo - Always unlocked (no unlock criteria)
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [Path], [CoinType], [Category], [IsAlwaysUnlocked], [SortOrder])
    VALUES 
        (NEWID(), 'JP Logo', 'The original. Where it all began.', '/img/coins/logo.png', 0, 'Default', 1, 1)

    -- 2. Gemini - Total Flips unlock
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [SortOrder])
    VALUES 
        (NEWID(), 'Gemini', 
         'Twin-faced and ever-changing, the Gemini represents duality in all things.', 
         'Complete 10 total flips',
         '/img/coins/AI/Zodiak/Gemini.png', 
         1, 'AI', 'Common',
         N'{
           "type": 1,
           "requiredCount": 10,
           "description": "Complete 10 total flips",
           "flavorText": "Twin-faced and ever-changing, the Gemini represents duality in all things. Heads or tails? Why not both?",
           "rarity": 0
         }',
         2)

    -- 3. Ram - Heads flips unlock
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [SortOrder])
    VALUES 
        (NEWID(), 'Ram', 
         'Bold and headstrong, the Ram charges forward without hesitation.', 
         'Land heads 10 times',
         '/img/coins/AI/Zodiak/Ram.png', 
         1, 'AI', 'Common',
         N'{
           "type": 2,
           "requiredCount": 10,
           "description": "Land heads 10 times",
           "flavorText": "Bold and headstrong, the Ram charges forward without hesitation. Victory favors the brave.",
           "rarity": 0
         }',
         3)

    -- 4. Tauros - Tails flips unlock
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [SortOrder])
    VALUES 
        (NEWID(), 'Tauros', 
         'Patient and steadfast, the bull knows that fortune comes to those who wait.', 
         'Land tails 10 times',
         '/img/coins/AI/Zodiak/Tauros.png', 
         1, 'AI', 'Common',
         N'{
           "type": 3,
           "requiredCount": 10,
           "description": "Land tails 10 times",
           "flavorText": "Patient and steadfast, the bull knows that fortune comes to those who wait for their moment.",
           "rarity": 0
         }',
         4)

    -- 5. Rabbit - Random chance with prerequisites
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [Prerequisites], [SortOrder])
    VALUES 
        (NEWID(), 'Rabbit', 
         'Swift and elusive, the Rabbit appears only to those who have walked the shadowed path.', 
         '7% chance to unlock per flip (requires 25 tails flips first)',
         '/img/coins/AI/Zodiak/Rabbit.png', 
         1, 'AI', 'Uncommon',
         N'{
           "type": 6,
           "unlockChance": 0.07,
           "description": "7% chance to unlock per flip (requires 25 tails flips first)",
           "flavorText": "Swift and elusive, the Rabbit appears only to those who have walked the shadowed path.",
           "rarity": 1,
           "prerequisites": [
             {
               "type": 3,
               "requiredCount": 25,
               "description": "Must have landed tails 25 times"
             }
           ]
         }',
         N'[
           {
             "type": 3,
             "requiredCount": 25,
             "description": "Must have landed tails 25 times"
           }
         ]',
         5)

    -- 6. 10 Flips Achievement
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [SortOrder])
    VALUES 
        (NEWID(), '10 Flips', 
         'Every journey begins with a single flip. Ten marks your first steps into the unknown.', 
         'Complete 10 total flips',
         '/img/coins/AI/Achievements/10Flips.png', 
         2, 'AI', 'Common',
         N'{
           "type": 1,
           "requiredCount": 10,
           "description": "Complete 10 total flips",
           "flavorText": "Every journey begins with a single flip. Ten marks your first steps into the unknown.",
           "rarity": 0
         }',
         6)

    -- 7. 3 Heads Streak Achievement
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [SortOrder])
    VALUES 
        (NEWID(), '3 Head Streak', 
         'Three heads in a row. Coincidence? Or is luck finally on your side?', 
         'Achieve a 3-heads streak',
         '/img/coins/AI/Achievements/3HeadStreak.png', 
         2, 'AI', 'Common',
         N'{
           "type": 4,
           "requiredCount": 3,
           "streakSide": 0,
           "description": "Achieve a 3-heads streak",
           "flavorText": "Three heads in a row. Coincidence? Or is luck finally on your side?",
           "rarity": 0
         }',
         7)

    -- 8. Auto Clicker - Coin with AutoClick effect
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [Effects], [SortOrder])
    VALUES 
        (NEWID(), 'Auto Clicker', 
         'Why flip manually when machines can do it for you?', 
         'Complete 100 total flips',
         '/img/coins/Powers/AutoClicker.png', 
         5, 'Powers', 'Rare',
         N'{
           "type": 1,
           "requiredCount": 100,
           "description": "Complete 100 total flips",
           "rarity": 2
         }',
         N'{
           "type": 1,
           "description": "Automatically clicks the coin every second",
           "autoClickInterval": 1000
         }',
         8)

    -- 9. Weighted Coin - Coin with Weighted effect
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [Effects], [SortOrder])
    VALUES 
        (NEWID(), 'Weighted Coin', 
         'Not all coins are created equal. This one tips the scales in your favor.', 
         'Achieve a 10-heads streak',
         '/img/coins/Powers/Weighted.png', 
         5, 'Powers', 'Legendary',
         N'{
           "type": 4,
           "requiredCount": 10,
           "streakSide": 0,
           "description": "Achieve a 10-heads streak",
           "rarity": 3
         }',
         N'{
           "type": 2,
           "description": "Increases chance of landing this side up by 10%",
           "biasStrength": 0.1
         }',
         9)

    -- 10. Lucky Coin - Coin with Luck effect
    INSERT INTO [CFG].[Coins] 
        ([Id], [Name], [FlavorText], [UnlockDescription], [Path], [CoinType], [Category], [Rarity], [UnlockCriteria], [Effects], [SortOrder])
    VALUES 
        (NEWID(), 'Lucky Coin', 
         'Fortune favors the bold. This coin radiates an aura of serendipity.', 
         'Complete 500 total flips',
         '/img/coins/Powers/Lucky.png', 
         5, 'Powers', 'Legendary',
         N'{
           "type": 1,
           "requiredCount": 500,
           "description": "Complete 500 total flips",
           "rarity": 3
         }',
         N'{
           "type": 5,
           "description": "Increases random unlock chances by 50%",
           "luckModifier": 0.5,
           "luckModifierType": 1
         }',
         10)

    PRINT 'Sample coin data inserted successfully. Total: 10 coins'
END
ELSE
BEGIN
    PRINT 'Sample data not inserted - coins already exist in database.'
END
GO
