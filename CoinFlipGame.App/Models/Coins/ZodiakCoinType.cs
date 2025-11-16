namespace CoinFlipGame.App.Models;

/// <summary>
/// Zodiak themed coins
/// </summary>
public class ZodiakCoinType : CoinType
{
    public override string Name { get; set; } = "Zodiak";
    public override string BasePath { get; set; } = "/img/coins/AI/Zodiak";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "Gemini.png",
        "Ram.png",
        "Tauros.png",
        "Rooster.png",
        "Pig.png",
        "Dragon.png",
        "Dragon_Rare.png"  // Rare variant
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "Gemini.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 10, 
                Description = "Flip 10 times to unlock",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Ram.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.HeadsFlips, 
                RequiredCount = 5, 
                Description = "Get 5 heads to unlock",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Tauros.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TailsFlips, 
                RequiredCount = 5, 
                Description = "Get 5 tails to unlock",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Rooster.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.Streak, 
                RequiredCount = 3, 
                Description = "Get a 3-flip streak to unlock",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        { 
            "Pig.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnCoin, 
                RequiredCount = 10,
                RequiredCoinPath = "/img/coins/AI/Zodiak/Ram.png",
                Description = "Use Ram coin 10 times to unlock",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        { 
            "Dragon.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnMultipleCoins, 
                RequiredCount = 10,
                RequiredCoinPaths = new List<string>
                {
                    "/img/coins/AI/Zodiak/Gemini.png",
                    "/img/coins/AI/Zodiak/Ram.png",
                    "/img/coins/AI/Zodiak/Tauros.png",
                    "/img/coins/AI/Zodiak/Rooster.png",
                    "/img/coins/AI/Zodiak/Pig.png"
                },
                Description = "Use each other Zodiak coin 10 times to unlock",
                Rarity = UnlockRarity.Rare
            } 
        },
        { 
            "Dragon_Rare.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance, 
                RequiredCount = 1,
                RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon.png",
                UnlockChance = 0.00005, // 0.005% chance
                Description = "Ultra rare! 0.005% chance when using Dragon coin",
                Rarity = UnlockRarity.Legendary
            } 
        }
    };
}
