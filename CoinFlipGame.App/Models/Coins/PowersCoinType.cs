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
        "Heavy.png",
        "Lucky.png",
        "Slots.png"
    };

    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        {
            "Digital_Ox.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 100,
                Description = "Complete 100 total flips",
                FlavorText = "Mechanical precision meets ancient tradition. Never tire, never falter, never stop flipping.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "DragonCore.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                Description = "Achieve a 5-flip streak on any side",
                FlavorText = "Within this coin beats the heart of a dragon. Feel its power surge with each consecutive flip.",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Heavy.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 6,
                Description = "Achieve a 6-flip streak on any side",
                FlavorText = "Dense as a collapsing star, this coin pulls destiny toward its weighted side.",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Slots.png", new UnlockCondition
            {
                Type = UnlockConditionType.LandOnCoinsWithCharacteristics,
                ConsecutiveCount = 10,
                CharacteristicFilter = CoinCharacteristicFilter.UnlockConditionType,
                FilterUnlockConditionType = UnlockConditionType.RandomChance,
                SideRequirement = SideRequirement.Both,
                RequiresActiveCoin = true,
                RequiredCoinPath = "/img/coins/Random.png",
                Description = "Land on random-chance coins 10 consecutive times while Random is set on both sides",
                FlavorText = "Triple sevens. The house always wins, but sometimes... just sometimes... luck defies all odds.",
                Rarity = UnlockRarity.Legendary
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
