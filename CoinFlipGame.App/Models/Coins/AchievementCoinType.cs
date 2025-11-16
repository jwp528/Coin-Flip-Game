using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Achievement themed coins
/// </summary>
public class AchievementCoinType : CoinType
{
    public override string Name { get; set; } = "Achievements";
    public override string BasePath { get; set; } = "/img/coins/AI/Achievements";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "10Flips.png",
        "25Flips.png",
        "50Flips.png",
        "100Flips.png",
        "500Flips.png",
        "1000Flips.png",
        "Completionist.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "10Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 10, 
                Description = "Flip 10 times",
                Rarity = UnlockRarity.Common
            }
        },
        { 
            "25Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 25, 
                Description = "Flip 25 times",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "50Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 50, 
                Description = "Flip 50 times",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "100Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 100, 
                Description = "Flip 100 times",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "500Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 500,
                Description = "Flip 500 times",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "1000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 1000,
                Description = "Flip 1000 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        { 
            "Completionist.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnMultipleCoins, 
                RequiredCount = 1,
                RequiredCoinPaths = new List<string>
                {
                    // JP Logo coins
                    "/img/coins/logo.png",
                    
                    // Zodiak coins
                    "/img/coins/AI/Zodiak/Gemini.png",
                    "/img/coins/AI/Zodiak/Rat.png",
                    "/img/coins/AI/Zodiak/Ram.png",
                    "/img/coins/AI/Zodiak/Dog.png",
                    "/img/coins/AI/Zodiak/Tauros.png",
                    "/img/coins/AI/Zodiak/Rabbit.png",
                    "/img/coins/AI/Zodiak/Rooster.png",
                    "/img/coins/AI/Zodiak/Pig.png",
                    "/img/coins/AI/Zodiak/Dragon.png",
                    "/img/coins/AI/Zodiak/Dragon_Rare.png",
                    
                    // Achievement coins (excluding self)
                    "/img/coins/AI/Achievements/10Flips.png",
                    "/img/coins/AI/Achievements/25Flips.png",
                    "/img/coins/AI/Achievements/50Flips.png",
                    "/img/coins/AI/Achievements/100Flips.png"
                },
                Description = "Unlock all other coins in the game",
                Rarity = UnlockRarity.Legendary
            }
        }
    };
}
