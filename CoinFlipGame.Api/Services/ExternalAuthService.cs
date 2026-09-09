using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Azure;
using CoinFlipGame.Api.Persistence;
using CoinFlipGame.Shared.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoinFlipGame.Api.Services;

public sealed class ExternalAuthService
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> RateWindows = new();
    private static OpenIdMetadata? _metadata;

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClients;
    private readonly TableStorageService _storage;
    private readonly PlayerAccountService _accounts;
    private readonly ILogger<ExternalAuthService> _logger;

    public ExternalAuthService(
        IConfiguration configuration,
        IHttpClientFactory httpClients,
        TableStorageService storage,
        PlayerAccountService accounts,
        ILogger<ExternalAuthService> logger)
    {
        _configuration = configuration;
        _httpClients = httpClients;
        _storage = storage;
        _accounts = accounts;
        _logger = logger;
    }

    public bool Enabled => _configuration.GetValue<bool>("ExternalAuth:Enabled");

    public async Task<AccountAuthResponse> LoginAsync(ExternalLoginRequest request, string deviceId, CancellationToken ct)
    {
        if (!Allow($"login:{deviceId}", 10, TimeSpan.FromMinutes(5)))
            return PlayerAccountService.Failure("rate-limited", "Too many sign-in attempts. Try again shortly.");

        var identity = await ValidateAsync(request.AccessToken, ct);
        if (identity is null)
            return PlayerAccountService.Failure("invalid-external-token", "Social sign-in could not be verified.");

        var existing = await GetMappingAsync(identity, ct);
        if (existing is not null)
        {
            await RefreshMappingAsync(existing, identity, ct);
            return await _accounts.IssueSessionAsync(existing.AccountId, ct);
        }

        return await CreateAccountAsync(identity, request.DisplayName, ct);
    }

    public async Task<AccountActionResponse> LinkAsync(AuthenticatedPlayer player, LinkExternalIdentityRequest request, CancellationToken ct)
    {
        if (!Allow($"link:{player.Account.PartitionKey}", 5, TimeSpan.FromMinutes(10)))
            return new AccountActionResponse(false, "rate-limited", "Too many linking attempts. Try again shortly.");

        var identity = await ValidateAsync(request.AccessToken, ct);
        if (identity is null)
            return new AccountActionResponse(false, "invalid-external-token", "Social sign-in could not be verified.");

        var existing = await GetMappingAsync(identity, ct);
        if (existing is not null && existing.AccountId != player.Account.PartitionKey)
            return new AccountActionResponse(false, "identity-already-linked", "This social identity is linked to another account.");

        if (existing is null && !await TryAddMappingAsync(identity, player.Account.PartitionKey, ct))
            return new AccountActionResponse(false, "identity-already-linked", "This social identity is linked to another account.");

        if (existing is not null)
            await RefreshMappingAsync(existing, identity, ct);

        await _accounts.MarkExternalIdentityLinkedAsync(player.Account, ct);
        _logger.LogInformation("External identity linked for provider {Provider}", identity.Provider);
        return new AccountActionResponse(true, null, null);
    }

    public async Task<AccountIdentityStatusResponse> GetIdentityStatusAsync(AuthenticatedPlayer player, CancellationToken ct)
    {
        var mappings = await _storage.GetExternalIdentitiesForAccountAsync(player.Account.PartitionKey, ct);
        var identities = mappings
            .Select(mapping =>
            {
                var provider = ProviderKey(mapping.Provider);
                return new LinkedIdentityDto(
                    IdentityId(mapping),
                    provider,
                    ProviderDisplayName(provider),
                    string.IsNullOrWhiteSpace(mapping.DisplayIdentifier) ? null : mapping.DisplayIdentifier,
                    mapping.LinkedUtc);
            })
            .ToArray();
        return new AccountIdentityStatusResponse(identities);
    }

    public async Task<PlayerMeResponse> GetMeAsync(AuthenticatedPlayer player, CancellationToken ct)
    {
        var identities = await GetIdentityStatusAsync(player, ct);
        return new PlayerMeResponse(
            player.Account.PartitionKey,
            player.Account.DisplayName,
            player.Session.ExpiresUtc,
            identities.Identities);
    }

    public async Task<AccountActionResponse> UnlinkIdentityAsync(AuthenticatedPlayer player, string identityId, CancellationToken ct)
    {
        if (identityId.Length != 128 || identityId.Any(character => !Uri.IsHexDigit(character)))
            return new AccountActionResponse(false, "identity-not-found", "That sign-in method is no longer connected.");

        ExternalIdentityEntity mapping;
        try
        {
            mapping = (await _storage.GetTable(TableStorageService.ExternalIdentitiesTable)
                .GetEntityAsync<ExternalIdentityEntity>(identityId[..64], identityId[64..], cancellationToken: ct)).Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new AccountActionResponse(false, "identity-not-found", "That sign-in method is no longer connected.");
        }

        if (!string.Equals(mapping.AccountId, player.Account.PartitionKey, StringComparison.Ordinal))
            return new AccountActionResponse(false, "identity-not-found", "That sign-in method is no longer connected.");

        var mappings = await _storage.GetExternalIdentitiesForAccountAsync(player.Account.PartitionKey, ct);
        if (mappings.Count <= 1)
            return new AccountActionResponse(false, "last-sign-in-method", "Add another sign-in method before unlinking this one.");

        await _storage.DeleteExternalIdentityAsync(mapping.PartitionKey, mapping.RowKey, ct);
        _logger.LogInformation("External identity unlinked for provider {Provider}", mapping.Provider);
        return new AccountActionResponse(true, null, null);
    }

    private async Task<ValidatedExternalIdentity?> ValidateAsync(string accessToken, CancellationToken ct)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(accessToken))
            return null;

        var authority = _configuration["ExternalAuth:Authority"]?.TrimEnd('/');
        var audience = _configuration["ExternalAuth:Audience"];
        if (!Uri.TryCreate(authority, UriKind.Absolute, out var authorityUri) ||
            authorityUri.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(audience))
        {
            return null;
        }

        var parts = accessToken.Split('.');
        if (parts.Length != 3)
            return null;

        JsonDocument header;
        JsonDocument payload;
        try
        {
            header = JsonDocument.Parse(Decode(parts[0]));
            payload = JsonDocument.Parse(Decode(parts[1]));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }

        using (header)
        using (payload)
        {
            if (!header.RootElement.TryGetProperty("alg", out var algorithm) || algorithm.GetString() != "RS256")
                return null;

            var kid = header.RootElement.TryGetProperty("kid", out var kidValue) ? kidValue.GetString() : null;
            if (string.IsNullOrWhiteSpace(kid))
                return null;

            var metadata = await GetMetadataAsync(authority!, forceRefresh: false, ct);
            var key = metadata.Keys.FirstOrDefault(x => x.Kid == kid);
            if (key is null)
            {
                metadata = await GetMetadataAsync(authority!, forceRefresh: true, ct);
                key = metadata.Keys.FirstOrDefault(x => x.Kid == kid);
            }

            if (key is null || !Verify(parts[0], parts[1], parts[2], key))
                return null;

            var root = payload.RootElement;
            var issuer = root.TryGetProperty("iss", out var iss) ? iss.GetString() : null;
            if (!string.Equals(issuer?.TrimEnd('/'), metadata.Issuer.TrimEnd('/'), StringComparison.Ordinal))
                return null;
            if (!HasAudience(root, audience!))
                return null;

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!root.TryGetProperty("exp", out var exp) || !exp.TryGetInt64(out var expires) || expires < now - 120)
                return null;
            if (root.TryGetProperty("nbf", out var nbf) && (!nbf.TryGetInt64(out var notBefore) || notBefore > now + 120))
                return null;

            var subject = root.TryGetProperty("sub", out var sub) ? sub.GetString() : null;
            if (string.IsNullOrWhiteSpace(subject))
                return null;

            var provider = root.TryGetProperty("idp", out var idp) ? idp.GetString() ?? "entra" : "entra";
            return new ValidatedExternalIdentity(issuer!, subject, provider, ReadIdentifier(root));
        }
    }

    private async Task<OpenIdMetadata> GetMetadataAsync(string authority, bool forceRefresh, CancellationToken ct)
    {
        if (!forceRefresh && _metadata is { ExpiresUtc: var expires } cached && expires > DateTimeOffset.UtcNow)
            return cached;

        await RefreshLock.WaitAsync(ct);
        try
        {
            if (!forceRefresh && _metadata is { ExpiresUtc: var secondExpiry } second && secondExpiry > DateTimeOffset.UtcNow)
                return second;

            var client = _httpClients.CreateClient(nameof(ExternalAuthService));
            using var discovery = await client.GetAsync($"{authority}/.well-known/openid-configuration", ct);
            discovery.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await discovery.Content.ReadAsStringAsync(ct));
            var issuer = document.RootElement.GetProperty("issuer").GetString()!;
            var jwksUri = document.RootElement.GetProperty("jwks_uri").GetString()!;
            if (!Uri.TryCreate(jwksUri, UriKind.Absolute, out var keysUri) || keysUri.Scheme != Uri.UriSchemeHttps)
                throw new InvalidOperationException("External auth JWKS endpoint must use HTTPS.");

            using var keysResponse = await client.GetAsync(keysUri, ct);
            keysResponse.EnsureSuccessStatusCode();
            using var keysDocument = JsonDocument.Parse(await keysResponse.Content.ReadAsStringAsync(ct));
            var keys = keysDocument.RootElement.GetProperty("keys").EnumerateArray()
                .Where(x => x.GetProperty("kty").GetString() == "RSA")
                .Select(x => new RsaKey(x.GetProperty("kid").GetString()!, x.GetProperty("n").GetString()!, x.GetProperty("e").GetString()!))
                .ToArray();
            return _metadata = new OpenIdMetadata(issuer, keys, DateTimeOffset.UtcNow.AddHours(12));
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task<ExternalIdentityEntity?> GetMappingAsync(ValidatedExternalIdentity identity, CancellationToken ct) =>
        await _storage.GetExternalIdentityAsync(Hash(identity.Issuer), Hash(identity.Subject), ct);

    private async Task<bool> TryAddMappingAsync(ValidatedExternalIdentity identity, string accountId, CancellationToken ct)
    {
        try
        {
            await _storage.AddExternalIdentityAsync(new ExternalIdentityEntity
            {
                PartitionKey = Hash(identity.Issuer),
                RowKey = Hash(identity.Subject),
                AccountId = accountId,
                Provider = identity.Provider,
                DisplayIdentifier = identity.Identifier ?? string.Empty,
                LinkedUtc = DateTimeOffset.UtcNow
            }, ct);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            return false;
        }
    }

    private async Task<AccountAuthResponse> CreateAccountAsync(
        ValidatedExternalIdentity identity,
        string? requestedDisplayName,
        CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var accountId = Guid.NewGuid().ToString("N");
        var displayName = ResolveDisplayName(requestedDisplayName, identity.Identifier);
        var account = new PlayerAccountEntity
        {
            PartitionKey = accountId,
            DisplayName = displayName,
            DateClaimedUtc = now,
            ExternalOnly = true,
            ExternalIdentityLinked = true,
            UpdatedUtc = now
        };

        await _storage.AddPlayerAccountAsync(account, ct);
        if (await TryAddMappingAsync(identity, accountId, ct))
        {
            _logger.LogInformation("External account created for provider {Provider}", identity.Provider);
            return await _accounts.IssueSessionAsync(accountId, ct);
        }

        await _storage.DeletePlayerAccountAsync(accountId, ct);
        var concurrent = await GetMappingAsync(identity, ct);
        if (concurrent is not null)
            return await _accounts.IssueSessionAsync(concurrent.AccountId, ct);

        return PlayerAccountService.Failure("identity-already-linked", "This sign-in identity is already linked to another account.");
    }

    private async Task RefreshMappingAsync(ExternalIdentityEntity mapping, ValidatedExternalIdentity identity, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(identity.Identifier) ||
            string.Equals(mapping.DisplayIdentifier, identity.Identifier, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        mapping.DisplayIdentifier = identity.Identifier;
        mapping.Provider = identity.Provider;
        await _storage.UpdateExternalIdentityAsync(mapping, ct);
    }

    private static string ResolveDisplayName(string? requested, string? identifier)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            var trimmed = requested.Trim();
            return trimmed.Length <= 32 ? trimmed : trimmed[..32];
        }

        if (!string.IsNullOrWhiteSpace(identifier) && identifier.Contains('@'))
        {
            var local = identifier.Split('@')[0];
            if (!string.IsNullOrWhiteSpace(local))
                return local.Length <= 32 ? local : local[..32];
        }

        return "Player";
    }

    private static string IdentityId(ExternalIdentityEntity mapping) => mapping.PartitionKey + mapping.RowKey;

    private static string ProviderKey(string provider) =>
        provider.Contains("google", StringComparison.OrdinalIgnoreCase) ? "google" :
        provider.Contains("apple", StringComparison.OrdinalIgnoreCase) ? "apple" :
        provider.Contains("live", StringComparison.OrdinalIgnoreCase) ||
        provider.Contains("microsoft", StringComparison.OrdinalIgnoreCase) ? "microsoft" :
        "entra";

    private static string ProviderDisplayName(string provider) => provider switch
    {
        "google" => "Google",
        "apple" => "Apple",
        "microsoft" => "Microsoft",
        _ => "Account"
    };

    private static bool Verify(string header, string payload, string signature, RsaKey key)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters { Modulus = Decode(key.Modulus), Exponent = Decode(key.Exponent) });
            return rsa.VerifyData(
                Encoding.ASCII.GetBytes($"{header}.{payload}"),
                Decode(signature),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            return false;
        }
    }

    private static bool HasAudience(JsonElement payload, string expected)
    {
        if (!payload.TryGetProperty("aud", out var aud))
            return false;

        return aud.ValueKind switch
        {
            JsonValueKind.String => aud.GetString() == expected,
            JsonValueKind.Array => aud.EnumerateArray().Any(x => x.GetString() == expected),
            _ => false
        };
    }

    private static string? ReadIdentifier(JsonElement payload)
    {
        foreach (var claim in new[] { "email", "preferred_username" })
        {
            if (payload.TryGetProperty(claim, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                value.GetString() is { } identifier &&
                identifier.Contains('@'))
            {
                return identifier.Trim().ToLowerInvariant();
            }
        }

        if (payload.TryGetProperty("emails", out var emails) && emails.ValueKind == JsonValueKind.Array)
        {
            foreach (var value in emails.EnumerateArray())
            {
                if (value.ValueKind == JsonValueKind.String &&
                    value.GetString() is { } identifier &&
                    identifier.Contains('@'))
                {
                    return identifier.Trim().ToLowerInvariant();
                }
            }
        }

        return null;
    }

    private static byte[] Decode(string value) =>
        Convert.FromBase64String(value.Replace('-', '+').Replace('_', '/').PadRight((value.Length + 3) / 4 * 4, '='));

    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static bool Allow(string key, int limit, TimeSpan window)
    {
        var queue = RateWindows.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        lock (queue)
        {
            var cutoff = DateTimeOffset.UtcNow - window;
            while (queue.Count > 0 && queue.Peek() <= cutoff)
                queue.Dequeue();
            if (queue.Count >= limit)
                return false;
            queue.Enqueue(DateTimeOffset.UtcNow);
            return true;
        }
    }

    private sealed record ValidatedExternalIdentity(string Issuer, string Subject, string Provider, string? Identifier);
    private sealed record RsaKey(string Kid, string Modulus, string Exponent);
    private sealed record OpenIdMetadata(string Issuer, IReadOnlyList<RsaKey> Keys, DateTimeOffset ExpiresUtc);
}
