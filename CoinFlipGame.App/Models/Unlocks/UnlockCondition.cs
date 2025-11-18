namespace CoinFlipGame.App.Models.Unlocks;

/// <summary>
/// Specifies which side a streak must be on
/// </summary>
public enum StreakSide
{
    Heads,
    Tails
}

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
    /// Example: 0.00005 = 0.005% chance, 0.07 = 7% chance
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
    
    /// <summary>
    /// Prerequisites that must be met before this unlock condition is checked.
    /// All prerequisites must be satisfied for the main condition to be evaluated.
    /// Example: Rabbit requires TailsFlips >= 25 before checking random chance
    /// </summary>
    public List<UnlockCondition>? Prerequisites { get; set; }
    
    /// <summary>
    /// For RandomChance types - if true, the random roll only happens when the required coin is actively selected
    /// Example: Dragon_Rare only rolls when Dragon coin is selected as heads/tails
    /// </summary>
    public bool RequiresActiveCoin { get; set; } = false;
    
    /// <summary>
    /// For Streak type - specifies which side (Heads or Tails) the streak must be on.
    /// If null, any streak counts (legacy behavior).
    /// </summary>
    public StreakSide? StreakSide { get; set; } = null;
}
