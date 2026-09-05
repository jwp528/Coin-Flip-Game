using Azure;
using Azure.Data.Tables;

namespace CoinFlipGame.Api.Persistence;

public sealed class PlayerProgressEntity : ITableEntity
{
    public const string ProgressRowKey = "Progress";
    public string PartitionKey { get; set; } = string.Empty; // AccountId
    public string RowKey { get; set; } = ProgressRowKey;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public int TotalFlips { get; set; }
    public int HeadsFlips { get; set; }
    public int TailsFlips { get; set; }
    public int LongestStreak { get; set; }
    public int LongestHeadsStreak { get; set; }
    public int LongestTailsStreak { get; set; }
    public string CoinLandCountsJson { get; set; } = "{}";
    public string RandomUnlockedCoinsJson { get; set; } = "[]";
    public string NotificationShownForJson { get; set; } = "[]";
    public string CoinUnlockTimestampsJson { get; set; } = "{}";
    public string CharacteristicConsecutiveCountsJson { get; set; } = "{}";
    public int TotalPlayTimeSeconds { get; set; }
    public string UnlockedAchievementsJson { get; set; } = "[]";
    public DateTimeOffset UpdatedUtc { get; set; }
}
