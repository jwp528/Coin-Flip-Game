namespace CoinFlipGame.Shared.Dtos;

public sealed record ExternalLoginRequest(string AccessToken, string? DisplayName = null);

public sealed record LinkExternalIdentityRequest(string AccessToken);

public sealed record AccountAuthResponse(
    bool Success,
    string? ErrorCode,
    string? Error,
    string? SessionToken,
    PlayerProfileDto? Profile);

public sealed record AccountActionResponse(
    bool Success,
    string? ErrorCode,
    string? Error);

public sealed record PlayerProfileDto(
    string AccountId,
    string DisplayName,
    DateTimeOffset? SessionExpiresUtc,
    bool ExternalIdentityLinked);

public sealed record PlayerMeResponse(
    string AccountId,
    string DisplayName,
    DateTimeOffset SessionExpiresUtc,
    IReadOnlyList<LinkedIdentityDto> Identities);

public sealed record LinkedIdentityDto(
    string IdentityId,
    string Provider,
    string DisplayName,
    string? Identifier,
    DateTimeOffset LinkedUtc);

public sealed record AccountIdentityStatusResponse(IReadOnlyList<LinkedIdentityDto> Identities);
