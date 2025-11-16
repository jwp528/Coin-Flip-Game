using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Leather/Material themed coins
/// </summary>
public class LeatherCoinType : CoinType
{
    public override string Name { get; set; } = "Leather";
    public override string BasePath { get; set; } = "/img/coins/AI/Leather";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "House.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "House.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 250, 
                Description = "Flip 250 times",
                Rarity = UnlockRarity.Uncommon
            } 
        }
    };
}
