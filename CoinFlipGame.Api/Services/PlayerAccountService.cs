using System.Security.Cryptography;
using System.Text;
using CoinFlipGame.Api.Functions;
using CoinFlipGame.Api.Persistence;
using CoinFlipGame.Shared.Dtos;
using Microsoft.Azure.Functions.Worker.Http;

namespace CoinFlipGame.Api.Services;

public sealed record AuthenticatedPlayer(PlayerAccountEntity Account, PlayerSessionEntity Session);

public sealed class PlayerAccountService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromDays(30);
    private readonly TableStorageService _storage;

    public PlayerAccountService(TableStorageService storage)
    {
        _storage = storage;
    }

    public async Task<AuthenticatedPlayer?> AuthenticateAsync(HttpRequestData request, CancellationToken ct = default)
    {
        var header = FunctionHttp.Header(request, "Authorization");
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = header[7..].Trim();
        var separator = token.IndexOf('.');
        if (separator <= 0 || separator == token.Length - 1)
            return null;

        var tokenId = token[..separator];
        var secret = token[(separator + 1)..];
        var session = await _storage.GetPlayerSessionAsync(tokenId, ct);
        if (session is null || session.ExpiresUtc <= DateTimeOffset.UtcNow || !FixedEquals(session.TokenHash, HashToken(secret)))
            return null;

        var account = await _storage.GetPlayerAccountAsync(session.AccountId, ct);
        return account is null ? null : new AuthenticatedPlayer(account, session);
    }

    public async Task<AccountAuthResponse> IssueSessionAsync(string accountId, CancellationToken ct = default)
    {
        var account = await _storage.GetPlayerAccountAsync(accountId, ct);
        if (account is null)
            return Failure("account-not-found", "That account no longer exists.");

        var (token, session) = await CreateSessionAsync(accountId, ct);
        return new AccountAuthResponse(true, null, null, token, ToProfile(account, session.ExpiresUtc));
    }

    public async Task MarkExternalIdentityLinkedAsync(PlayerAccountEntity account, CancellationToken ct = default)
    {
        account.ExternalIdentityLinked = true;
        account.UpdatedUtc = DateTimeOffset.UtcNow;
        await _storage.UpsertPlayerAccountAsync(account, ct);
    }

    public async Task DeleteSessionAsync(string tokenId, CancellationToken ct = default) =>
        await _storage.DeletePlayerSessionAsync(tokenId, ct);

    public PlayerProfileDto ToProfile(PlayerAccountEntity account, DateTimeOffset? sessionExpiry = null) =>
        new(account.PartitionKey, account.DisplayName, sessionExpiry, account.ExternalIdentityLinked);

    public async Task<(string Token, PlayerSessionEntity Session)> CreateSessionAsync(string accountId, CancellationToken ct)
    {
        var tokenId = Guid.NewGuid().ToString("N");
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var session = new PlayerSessionEntity
        {
            PartitionKey = tokenId,
            AccountId = accountId,
            TokenHash = HashToken(secret),
            CreatedUtc = now,
            ExpiresUtc = now + SessionLifetime
        };
        await _storage.UpsertPlayerSessionAsync(session, ct);
        return ($"{tokenId}.{secret}", session);
    }

    public static AccountAuthResponse Failure(string code, string error) =>
        new(false, code, error, null, null);

    private static string HashToken(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool FixedEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(left), Encoding.ASCII.GetBytes(right));
}
