using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Default JP Logo coin type
/// </summary>
public class JpLogoCoinType : CoinType
{
    public override string Name { get; set; } = "JP Logo";
    public override string BasePath { get; set; } = "/img/coins";
    public override string Category { get; set; } = "Default";

    public override List<string> GetCoinFiles() => new()
    {
        "logo.png"
    };

    // No unlock conditions needed - default logo is always unlocked
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new();
}
