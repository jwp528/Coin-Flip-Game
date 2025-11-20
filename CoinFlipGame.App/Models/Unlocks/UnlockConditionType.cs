namespace CoinFlipGame.App.Models.Unlocks;

/// <summary>
/// Unlock condition types for coin images
/// </summary>
public enum UnlockConditionType
{
    None,           // Always unlocked
    TotalFlips,     // Unlock after X total flips
    HeadsFlips,     // Unlock after X heads flips
    TailsFlips,     // Unlock after X tails flips
    Streak,         // Unlock after X streak
    LandOnCoin,     // Unlock after landing on a specific coin X times
    RandomChance,   // Random chance to unlock when a specific coin is used
    LandOnMultipleCoins, // Unlock after landing on multiple specific coins X times each
    LandOnCoinsWithCharacteristics // Unlock after landing on coins matching certain characteristics X times in a row
}

/// <summary>
/// Types of filters for coin characteristics
/// </summary>
public enum CoinCharacteristicFilter
{
    SpecificCoins,                    // Filter by exact coin paths
    UnlockConditionType,              // Filter by unlock condition type
    EffectType,                       // Filter by effect type
    HasAnyEffect,                     // Coins with any effect (not None)
    HasAnyUnlockCondition,            // Coins with any unlock condition (not None)
    PrerequisiteCountEquals,          // Filter by exact number of prerequisites
    PrerequisiteCountGreaterThan,     // Coins with > X prerequisites
    PrerequisiteCountLessThan         // Coins with < X prerequisites
}

/// <summary>
/// Side requirement for characteristic-based unlock conditions
/// </summary>
public enum SideRequirement
{
    Either,      // Coin can be on heads OR tails (default)
    Both,        // Coin must be on BOTH heads AND tails simultaneously
    HeadsOnly,   // Coin must be on heads only
    TailsOnly    // Coin must be on tails only
}
