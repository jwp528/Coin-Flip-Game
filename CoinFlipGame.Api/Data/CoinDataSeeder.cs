using CoinFlipGame.Api.Data.Helpers;
using CoinFlipGame.Lib.Models.DTOs;
using CoinFlipGame.Lib.Models.Entities;
using CoinFlipGame.Lib.Models.Enums;

namespace CoinFlipGame.Api.Data;

/// <summary>
/// Sample data seeder for testing the database
/// This demonstrates how to create coins with JSON properties
/// </summary>
public static class CoinDataSeeder
{
    /// <summary>
    /// Get sample coins for seeding
    /// </summary>
    public static List<Coin> GetSampleCoins()
    {
        var coins = new List<Coin>();

        // 1. JP Logo - Always unlocked
        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "JP Logo",
            FlavorText = "The original. Where it all began.",
            Path = "/img/coins/logo.png",
            CoinType = CoinTypes.JpLogo,
            Category = "Default",
            IsAlwaysUnlocked = true,
            SortOrder = 1
        });

        // 2. Gemini - Total Flips unlock
        var geminiUnlock = new UnlockConditionDto
        {
            Type = UnlockConditionType.TotalFlips,
            RequiredCount = 10,
            Description = "Complete 10 total flips",
            FlavorText = "Twin-faced and ever-changing, the Gemini represents duality in all things. Heads or tails? Why not both?",
            Rarity = UnlockRarity.Common
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "Gemini",
            FlavorText = "Twin-faced and ever-changing, the Gemini represents duality in all things.",
            UnlockDescription = "Complete 10 total flips",
            Path = "/img/coins/AI/Zodiak/Gemini.png",
            CoinType = CoinTypes.Zodiak,
            Category = "AI",
            Rarity = "Common",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(geminiUnlock),
            SortOrder = 2
        });

        // 3. Ram - Heads flips unlock
        var ramUnlock = new UnlockConditionDto
        {
            Type = UnlockConditionType.HeadsFlips,
            RequiredCount = 10,
            Description = "Land heads 10 times",
            FlavorText = "Bold and headstrong, the Ram charges forward without hesitation. Victory favors the brave.",
            Rarity = UnlockRarity.Common
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "Ram",
            FlavorText = "Bold and headstrong, the Ram charges forward without hesitation.",
            UnlockDescription = "Land heads 10 times",
            Path = "/img/coins/AI/Zodiak/Ram.png",
            CoinType = CoinTypes.Zodiak,
            Category = "AI",
            Rarity = "Common",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(ramUnlock),
            SortOrder = 3
        });

        // 4. Rabbit - Random chance with prerequisites
        var rabbitPrerequisite = new UnlockConditionDto
        {
            Type = UnlockConditionType.TailsFlips,
            RequiredCount = 25,
            Description = "Must have landed tails 25 times"
        };

        var rabbitUnlock = new UnlockConditionDto
        {
            Type = UnlockConditionType.RandomChance,
            UnlockChance = 0.07,
            Description = "7% chance to unlock per flip (requires 25 tails flips first)",
            FlavorText = "Swift and elusive, the Rabbit appears only to those who have walked the shadowed path.",
            Rarity = UnlockRarity.Uncommon,
            Prerequisites = new List<UnlockConditionDto> { rabbitPrerequisite }
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "Rabbit",
            FlavorText = "Swift and elusive, the Rabbit appears only to those who have walked the shadowed path.",
            UnlockDescription = "7% chance to unlock per flip (requires 25 tails flips first)",
            Path = "/img/coins/AI/Zodiak/Rabbit.png",
            CoinType = CoinTypes.Zodiak,
            Category = "AI",
            Rarity = "Uncommon",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(rabbitUnlock),
            Prerequisites = CoinJsonHelper.SerializePrerequisites(new List<UnlockConditionDto> { rabbitPrerequisite }),
            SortOrder = 4
        });

        // 5. 10 Flips Achievement
        var tenFlipsUnlock = new UnlockConditionDto
        {
            Type = UnlockConditionType.TotalFlips,
            RequiredCount = 10,
            Description = "Complete 10 total flips",
            FlavorText = "Every journey begins with a single flip. Ten marks your first steps into the unknown.",
            Rarity = UnlockRarity.Common
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "10 Flips",
            FlavorText = "Every journey begins with a single flip. Ten marks your first steps into the unknown.",
            UnlockDescription = "Complete 10 total flips",
            Path = "/img/coins/AI/Achievements/10Flips.png",
            CoinType = CoinTypes.Achievement,
            Category = "AI",
            Rarity = "Common",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(tenFlipsUnlock),
            SortOrder = 5
        });

        // 6. Auto-clicker coin with effect
        var autoClickEffect = new CoinEffectDto
        {
            Type = CoinEffectType.AutoClick,
            Description = "Automatically clicks the coin every second",
            AutoClickInterval = 1000
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "Auto Clicker",
            FlavorText = "Why flip manually when machines can do it for you?",
            UnlockDescription = "Complete 100 total flips",
            Path = "/img/coins/Powers/AutoClicker.png",
            CoinType = CoinTypes.Powers,
            Category = "Powers",
            Rarity = "Rare",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(new UnlockConditionDto
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 100,
                Description = "Complete 100 total flips",
                Rarity = UnlockRarity.Rare
            }),
            Effects = CoinJsonHelper.SerializeEffects(autoClickEffect),
            SortOrder = 6
        });

        // 7. Weighted coin with bias effect
        var weightedEffect = new CoinEffectDto
        {
            Type = CoinEffectType.Weighted,
            Description = "Increases chance of landing this side up by 10%",
            BiasStrength = 0.1
        };

        coins.Add(new Coin
        {
            Id = Guid.NewGuid(),
            Name = "Weighted Coin",
            FlavorText = "Not all coins are created equal. This one tips the scales in your favor.",
            UnlockDescription = "Achieve a 10-heads streak",
            Path = "/img/coins/Powers/Weighted.png",
            CoinType = CoinTypes.Powers,
            Category = "Powers",
            Rarity = "Legendary",
            UnlockCriteria = CoinJsonHelper.SerializeUnlockCriteria(new UnlockConditionDto
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 10,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 10-heads streak",
                Rarity = UnlockRarity.Legendary
            }),
            Effects = CoinJsonHelper.SerializeEffects(weightedEffect),
            SortOrder = 7
        });

        return coins;
    }

    /// <summary>
    /// Seed the database with sample coins (if empty)
    /// </summary>
    public static async Task SeedAsync(CoinFlipGameDbContext context)
    {
        // Check if any coins already exist
        if (context.Coins.Any())
        {
            return; // Database already seeded
        }

        var sampleCoins = GetSampleCoins();
        await context.Coins.AddRangeAsync(sampleCoins);
        await context.SaveChangesAsync();
    }
}
