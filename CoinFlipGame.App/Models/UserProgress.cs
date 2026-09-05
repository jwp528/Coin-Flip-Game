using CoinFlipGame.Shared.Dtos;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Represents user progress data to be saved to local storage
/// </summary>
public class UserProgress
{
    /// <summary>
    /// Total number of flips performed
    /// </summary>
    public int TotalFlips { get; set; }
    
    /// <summary>
    /// Total number of heads flips
    /// </summary>
    public int HeadsFlips { get; set; }
    
    /// <summary>
    /// Total number of tails flips
    /// </summary>
    public int TailsFlips { get; set; }
    
    /// <summary>
    /// Longest streak achieved (any side - legacy)
    /// </summary>
    public int LongestStreak { get; set; }
    
    /// <summary>
    /// Longest heads streak achieved
    /// </summary>
    public int LongestHeadsStreak { get; set; }
    
    /// <summary>
    /// Longest tails streak achieved
    /// </summary>
    public int LongestTailsStreak { get; set; }
    
    /// <summary>
    /// Dictionary tracking how many times each coin has been landed on
    /// Key: coin path, Value: count
    /// </summary>
    public Dictionary<string, int> CoinLandCounts { get; set; } = new();
    
    /// <summary>
    /// List of coins unlocked via random chance
    /// </summary>
    public List<string> RandomUnlockedCoins { get; set; } = new();
    
    /// <summary>
    /// List of coins that have already shown unlock notifications
    /// </summary>
    public List<string> NotificationShownFor { get; set; } = new();
    
    /// <summary>
    /// Dictionary tracking when each coin was unlocked
    /// Key: coin path, Value: unlock timestamp (UTC)
    /// </summary>
    public Dictionary<string, DateTime> CoinUnlockTimestamps { get; set; } = new();
    
    /// <summary>
    /// Dictionary tracking consecutive characteristic-based landings
    /// Key: coin path (the coin to unlock), Value: current consecutive count
    /// </summary>
    public Dictionary<string, int> CharacteristicConsecutiveCounts { get; set; } = new();

    public PlayerProgressDto ToDto() => new()
    {
        TotalFlips = TotalFlips,
        HeadsFlips = HeadsFlips,
        TailsFlips = TailsFlips,
        LongestStreak = LongestStreak,
        LongestHeadsStreak = LongestHeadsStreak,
        LongestTailsStreak = LongestTailsStreak,
        CoinLandCounts = new Dictionary<string, int>(CoinLandCounts),
        RandomUnlockedCoins = RandomUnlockedCoins.ToList(),
        NotificationShownFor = NotificationShownFor.ToList(),
        CoinUnlockTimestamps = new Dictionary<string, DateTime>(CoinUnlockTimestamps),
        CharacteristicConsecutiveCounts = new Dictionary<string, int>(CharacteristicConsecutiveCounts)
    };

    public static UserProgress FromDto(PlayerProgressDto dto) => new()
    {
        TotalFlips = dto.TotalFlips,
        HeadsFlips = dto.HeadsFlips,
        TailsFlips = dto.TailsFlips,
        LongestStreak = dto.LongestStreak,
        LongestHeadsStreak = dto.LongestHeadsStreak,
        LongestTailsStreak = dto.LongestTailsStreak,
        CoinLandCounts = dto.CoinLandCounts ?? new Dictionary<string, int>(),
        RandomUnlockedCoins = dto.RandomUnlockedCoins ?? new List<string>(),
        NotificationShownFor = dto.NotificationShownFor ?? new List<string>(),
        CoinUnlockTimestamps = dto.CoinUnlockTimestamps ?? new Dictionary<string, DateTime>(),
        CharacteristicConsecutiveCounts = dto.CharacteristicConsecutiveCounts ?? new Dictionary<string, int>()
    };
}

/// <summary>
/// Represents user coin selection preferences
/// </summary>
public class CoinSelectionPreferences
{
    /// <summary>
    /// Path to the selected heads coin image
    /// </summary>
    public string SelectedHeadsImage { get; set; } = "/img/coins/logo.png";
    
    /// <summary>
    /// Path to the selected tails coin image
    /// </summary>
    public string SelectedTailsImage { get; set; } = "/img/coins/logo.png";
    
    /// <summary>
    /// Whether heads should show random coins
    /// </summary>
    public bool IsHeadsRandom { get; set; } = true;
    
    /// <summary>
    /// Whether tails should show random coins
    /// </summary>
    public bool IsTailsRandom { get; set; } = true;
}
