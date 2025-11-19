using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Combo themed coins that enhance the opposite side's effects
/// </summary>
public class ComboCoinType : CoinType
{
    public override string Name { get; set; } = "Combo";
    public override string BasePath { get; set; } = "/img/coins/AI/Combo";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "DragonSamurai.png",
        "Moai.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "DragonSamurai.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 150, 
                Description = "Flip 150 times",
                Rarity = UnlockRarity.Rare
            } 
        },
        { 
            "Moai.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 300, 
                Description = "Flip 300 times",
                Rarity = UnlockRarity.Rare
            } 
        }
    };
    
    /// <summary>
    /// Get coin effects for Combo coins
    /// </summary>
    public Dictionary<string, CoinEffect> GetCoinEffects() => new()
    {
        {
            "DragonSamurai.png", new CoinEffect
            {
                Type = CoinEffectType.Combo,
                Description = "Adds +3% to opposite side's effect (bias/auto-click speed) or +3% to streak",
                ComboType = ComboType.Additive,
                ComboMultiplier = 0.03
            }
        },
        {
            "Moai.png", new CoinEffect
            {
                Type = CoinEffectType.Combo,
                Description = "Multiplies opposite side's effect by 2x (bias/auto-click speed) or boosts streak by 100%",
                ComboType = ComboType.Multiplicative,
                ComboMultiplier = 2
            }
        }
    };
}
