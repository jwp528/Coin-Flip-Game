using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Random/Mystery themed coins
/// </summary>
public class RandomCoinType : CoinType
{
    public override string Name { get; set; } = "Random";
    public override string BasePath { get; set; } = "/img/coins/AI/Random";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "River.png",
        "Winter.png",
        "City.png",
        "Chaos.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "River.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.02,
                Description = "2% random unlock on any flip",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Winter.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.06,
                Description = "6% random unlock on any flip",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        { 
            "City.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance, 
                UnlockChance = 0.05,
                Description = "5% random unlock on any flip",
                Rarity = UnlockRarity.Rare
            } 
        },
        {
            "Chaos.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 5% chance
                Description = "5% random unlock on any flip",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TotalFlips,
                        RequiredCount = 200,
                        Description = "Flip 200 times"
                    }
                }
            }
        }
    };
}
