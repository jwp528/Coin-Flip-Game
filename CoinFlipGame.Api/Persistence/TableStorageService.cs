using Azure;
using Azure.Data.Tables;

namespace CoinFlipGame.Api.Persistence;

public sealed class TableStorageService
{
    public const string PlayerAccountsTable = "PlayerAccounts";
    public const string ExternalIdentitiesTable = "ExternalIdentities";
    public const string PlayerSessionsTable = "PlayerSessions";
    public const string PlayerProgressTable = "PlayerProgress";

    private static readonly string[] AllTableNames =
    {
        PlayerAccountsTable,
        ExternalIdentitiesTable,
        PlayerSessionsTable,
        PlayerProgressTable
    };

    private readonly TableServiceClient _serviceClient;

    public TableStorageService(TableServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public async Task EnsureTablesExistAsync(CancellationToken cancellationToken = default)
    {
        foreach (var tableName in AllTableNames)
            await _serviceClient.CreateTableIfNotExistsAsync(tableName, cancellationToken);
    }

    public TableClient GetTable(string tableName) => _serviceClient.GetTableClient(tableName);

    public Task<PlayerAccountEntity?> GetPlayerAccountAsync(string accountId, CancellationToken ct = default) =>
        TryGetAsync<PlayerAccountEntity>(PlayerAccountsTable, accountId, PlayerAccountEntity.ProfileRowKey, ct);

    public async Task AddPlayerAccountAsync(PlayerAccountEntity entity, CancellationToken ct = default) =>
        await GetTable(PlayerAccountsTable).AddEntityAsync(entity, ct);

    public async Task UpsertPlayerAccountAsync(PlayerAccountEntity entity, CancellationToken ct = default) =>
        await GetTable(PlayerAccountsTable).UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);

    public async Task DeletePlayerAccountAsync(string accountId, CancellationToken ct = default)
    {
        try
        {
            await GetTable(PlayerAccountsTable).DeleteEntityAsync(accountId, PlayerAccountEntity.ProfileRowKey, ETag.All, ct);
        }
        catch (RequestFailedException ex) when (ex.Status == 404) { }
    }

    public Task<PlayerSessionEntity?> GetPlayerSessionAsync(string tokenId, CancellationToken ct = default) =>
        TryGetAsync<PlayerSessionEntity>(PlayerSessionsTable, tokenId, "Session", ct);

    public async Task UpsertPlayerSessionAsync(PlayerSessionEntity entity, CancellationToken ct = default) =>
        await GetTable(PlayerSessionsTable).UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);

    public async Task DeletePlayerSessionAsync(string tokenId, CancellationToken ct = default)
    {
        try
        {
            await GetTable(PlayerSessionsTable).DeleteEntityAsync(tokenId, "Session", ETag.All, ct);
        }
        catch (RequestFailedException ex) when (ex.Status == 404) { }
    }

    public Task<ExternalIdentityEntity?> GetExternalIdentityAsync(string issuerHash, string subjectHash, CancellationToken ct = default) =>
        TryGetAsync<ExternalIdentityEntity>(ExternalIdentitiesTable, issuerHash, subjectHash, ct);

    public async Task AddExternalIdentityAsync(ExternalIdentityEntity entity, CancellationToken ct = default) =>
        await GetTable(ExternalIdentitiesTable).AddEntityAsync(entity, ct);

    public async Task UpdateExternalIdentityAsync(ExternalIdentityEntity entity, CancellationToken ct = default) =>
        await GetTable(ExternalIdentitiesTable).UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Merge, ct);

    public async Task DeleteExternalIdentityAsync(string partitionKey, string rowKey, CancellationToken ct = default) =>
        await GetTable(ExternalIdentitiesTable).DeleteEntityAsync(partitionKey, rowKey, ETag.All, ct);

    public async Task<List<ExternalIdentityEntity>> GetExternalIdentitiesForAccountAsync(string accountId, CancellationToken ct = default)
    {
        var results = new List<ExternalIdentityEntity>();
        await foreach (var entity in GetTable(ExternalIdentitiesTable)
            .QueryAsync<ExternalIdentityEntity>(x => x.AccountId == accountId, cancellationToken: ct))
        {
            results.Add(entity);
        }

        return results;
    }

    public Task<PlayerProgressEntity?> GetPlayerProgressAsync(string accountId, CancellationToken ct = default) =>
        TryGetAsync<PlayerProgressEntity>(PlayerProgressTable, accountId, PlayerProgressEntity.ProgressRowKey, ct);

    public async Task AddPlayerProgressAsync(PlayerProgressEntity entity, CancellationToken ct = default) =>
        await GetTable(PlayerProgressTable).AddEntityAsync(entity, ct);

    public async Task<bool> TryUpdatePlayerProgressAsync(PlayerProgressEntity entity, CancellationToken ct = default)
    {
        try
        {
            await GetTable(PlayerProgressTable).UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace, ct);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            return false;
        }
    }

    public async Task UpsertPlayerProgressAsync(PlayerProgressEntity entity, CancellationToken ct = default) =>
        await GetTable(PlayerProgressTable).UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);

    private async Task<T?> TryGetAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken ct)
        where T : class, ITableEntity, new()
    {
        try
        {
            var response = await GetTable(tableName).GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: ct);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
