namespace CoinFlipGame.App.Models;

/// <summary>
/// Represents a specific coin image
/// </summary>
public class CoinImage
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public CoinType Type { get; set; } = null!;
    public string DisplayName => Name.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
    
    /// <summary>
    /// Unlock condition for this coin (null = always unlocked)
    /// </summary>
    public UnlockCondition? UnlockCondition { get; set; }
    
    /// <summary>
    /// Number of times this coin has been landed on
    /// </summary>
    public int TimesLandedOn { get; set; }
}
