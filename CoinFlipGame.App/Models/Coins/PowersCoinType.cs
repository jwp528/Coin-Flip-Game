using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Powers themed coins with special effects
/// </summary>
public class PowersCoinType : CoinType
{
    public override string Name { get; set; } = "Powers";
    public override string BasePath { get; set; } = "/img/coins/AI/Powers";
    public override string Category { get; set; } = "AI";

    public override List<string> GetCoinFiles() => new()
    {
        "Digital_Ox.png",
        "DragonCore.png",
        "Heavy.png"
    };

    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        {
            "Digital_Ox.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 100,
                Description = "Flip 100 times",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "DragonCore.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                Description = "Soar on the wings of a dragon!",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Heavy.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 6,
                Description = "oooh So heavy!",
                Rarity = UnlockRarity.Common
            }
        }
    };
    
    /// <summary>
    /// Get effects for each coin in this type
    /// </summary>
    public Dictionary<string, CoinEffect> GetCoinEffects() => new()
    {
        {
            "Digital_Ox.png", new CoinEffect
            {
                Type = CoinEffectType.AutoClick,
                Description = "Automatically flips once per second",
                AutoClickInterval = 1000 // 1 click per second
            }
        },
        {
            "DragonCore.png", new CoinEffect
            {
                Type = CoinEffectType.Shaved,
                Description = "Shaved - biased to land this side up more often",
                BiasStrength = 0.20 // 20% bias towards landing UP
            }
        },
        {
            "Heavy.png", new CoinEffect
            {
                Type = CoinEffectType.Weighted,
                Description = "Weighted - heavy side lands down (opposite side shows)",
                BiasStrength = 0.40 // 40% bias towards landing DOWN
            }
        }
    };
}
