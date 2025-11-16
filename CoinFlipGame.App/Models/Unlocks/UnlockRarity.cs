namespace CoinFlipGame.App.Models.Unlocks;

/// <summary>
/// Rarity levels for coin unlocks, determines the achievement animation intensity
/// </summary>
public enum UnlockRarity
{
    /// <summary>
    /// Common unlock - simple animation
    /// </summary>
    Common,
    
    /// <summary>
    /// Uncommon unlock - moderate animation
    /// </summary>
    Uncommon,
    
    /// <summary>
    /// Rare unlock - impressive animation
    /// </summary>
    Rare,
    
    /// <summary>
    /// Legendary unlock - spectacular animation
    /// </summary>
    Legendary
}
