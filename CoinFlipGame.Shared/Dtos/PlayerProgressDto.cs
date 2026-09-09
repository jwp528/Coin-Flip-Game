namespace CoinFlipGame.Shared.Dtos;

/// <summary>
/// Cloud/local player progress. Field names match the client <c>UserProgress</c> model.
/// </summary>
public sealed class PlayerProgressDto
{
    public int TotalFlips { get; set; }
    public int HeadsFlips { get; set; }
    public int TailsFlips { get; set; }
    public int LongestStreak { get; set; }
    public int LongestHeadsStreak { get; set; }
    public int LongestTailsStreak { get; set; }
    public Dictionary<string, int> CoinLandCounts { get; set; } = new();
    public List<string> RandomUnlockedCoins { get; set; } = new();
    public List<string> NotificationShownFor { get; set; } = new();
    public Dictionary<string, DateTime> CoinUnlockTimestamps { get; set; } = new();
    public Dictionary<string, int> CharacteristicConsecutiveCounts { get; set; } = new();
    public int TotalPlayTimeSeconds { get; set; }
    public List<string> UnlockedAchievements { get; set; } = new();
}
