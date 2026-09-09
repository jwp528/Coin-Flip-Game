using Azure;
using Azure.Data.Tables;

namespace CoinFlipGame.Api.Persistence;

public sealed class PlayerAccountEntity : ITableEntity
{
    public const string ProfileRowKey = "Profile";
    public string PartitionKey { get; set; } = string.Empty; // AccountId
    public string RowKey { get; set; } = ProfileRowKey;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset DateClaimedUtc { get; set; }
    public bool ExternalOnly { get; set; } = true;
    public bool ExternalIdentityLinked { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
}

public sealed class PlayerSessionEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // token id
    public string RowKey { get; set; } = "Session";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset ExpiresUtc { get; set; }
}
