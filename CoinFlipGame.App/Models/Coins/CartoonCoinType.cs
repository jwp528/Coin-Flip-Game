using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Cartoon themed coins (placeholder for future expansion)
/// </summary>
public class CartoonCoinType : CoinType
{
    public override string Name { get; set; } = "Cartoon";
    public override string BasePath { get; set; } = "/img/coins/AI/Cartoon";
    public override string Category { get; set; } = "AI";
    
    // Placeholder - add coin files here when ready
    public override List<string> GetCoinFiles() => new();
    
    // Add unlock conditions here when coins are added
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new();
}
