using CoinFlipGame.App.Models;

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
    
    /// <summary>
    /// For LandOnMultipleCoins type - if true, RequiredCoinPaths will be dynamically populated
    /// with all unlockable coins in the game (excluding the coin with this condition).
    /// This makes "collect all" achievements automatically track new coins without manual updates.
    /// </summary>
    public bool UseDynamicCoinList { get; set; } = false;
    
    // ===== Properties for LandOnCoinsWithCharacteristics =====
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - defines how to filter coins
    /// </summary>
    public CoinCharacteristicFilter CharacteristicFilter { get; set; } = CoinCharacteristicFilter.SpecificCoins;
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - specific unlock condition type to filter by
    /// Used when CharacteristicFilter = UnlockConditionType
    /// </summary>
    public UnlockConditionType? FilterUnlockConditionType { get; set; }
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - specific effect type to filter by
    /// Used when CharacteristicFilter = EffectType
    /// </summary>
    public CoinEffectType? FilterEffectType { get; set; }
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - prerequisite count for filtering
    /// Used when CharacteristicFilter = PrerequisiteCountEquals/GreaterThan/LessThan
    /// </summary>
    public int FilterPrerequisiteCount { get; set; } = 0;
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - side requirement (heads/tails/both/either)
    /// Determines which side(s) the matching coins must be on
    /// </summary>
    public SideRequirement SideRequirement { get; set; } = SideRequirement.Either;
    
    /// <summary>
    /// For LandOnCoinsWithCharacteristics type - number of consecutive times required
    /// Must land on matching coins X times in a row
    /// </summary>
    public int ConsecutiveCount { get; set; } = 1;
}
