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
    /// Longest streak achieved
    /// </summary>
    public int LongestStreak { get; set; }
    
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
