namespace CoinFlipGame.App.Models;

/// <summary>
/// Defines unlock conditions for a coin image
/// </summary>
public class UnlockCondition
{
    public UnlockConditionType Type { get; set; }
    public int RequiredCount { get; set; }
    public string? RequiredCoinPath { get; set; } // For LandOnCoin or RandomChance condition
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Chance to unlock (0.0 to 1.0) - Used for RandomChance type
    /// Example: 0.00005 = 0.005% chance
    /// </summary>
    public double UnlockChance { get; set; } = 0.0;
    
    /// <summary>
    /// List of required coin paths - Used for LandOnMultipleCoins type
    /// Each coin in this list must be landed on RequiredCount times
    /// </summary>
    public List<string>? RequiredCoinPaths { get; set; }
    
    /// <summary>
    /// Rarity level of the unlock, determines achievement animation style
    /// </summary>
    public UnlockRarity Rarity { get; set; } = UnlockRarity.Common;
}
