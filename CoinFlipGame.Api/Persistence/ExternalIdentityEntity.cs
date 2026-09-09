using Azure;
using Azure.Data.Tables;

namespace CoinFlipGame.Api.Persistence;

public sealed class ExternalIdentityEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // SHA256 issuer
    public string RowKey { get; set; } = string.Empty; // SHA256 subject
    public string AccountId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string DisplayIdentifier { get; set; } = string.Empty;
    public DateTimeOffset LinkedUtc { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
