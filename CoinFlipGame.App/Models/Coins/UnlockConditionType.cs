namespace CoinFlipGame.App.Models;

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
    LandOnMultipleCoins // Unlock after landing on multiple specific coins X times each
}
