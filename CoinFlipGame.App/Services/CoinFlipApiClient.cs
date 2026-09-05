using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoinFlipGame.Shared.Dtos;

namespace CoinFlipGame.App.Services;

public sealed class CoinFlipApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly AccountSessionStore _sessions;
    private readonly DeviceIdService _devices;

    public CoinFlipApiClient(HttpClient http, AccountSessionStore sessions, DeviceIdService devices)
    {
        _http = http;
        _sessions = sessions;
        _devices = devices;
    }

    public Task<AccountAuthResponse> ExchangeExternalLoginAsync(ExternalLoginRequest request, CancellationToken ct = default) =>
        SendAsync<ExternalLoginRequest, AccountAuthResponse>(HttpMethod.Post, "player/external/login", request, ct);

    public Task<AccountActionResponse> LinkExternalIdentityAsync(LinkExternalIdentityRequest request, CancellationToken ct = default) =>
        SendAsync<LinkExternalIdentityRequest, AccountActionResponse>(HttpMethod.Post, "player/external/link", request, ct);

    public Task<AccountIdentityStatusResponse?> GetExternalIdentitiesAsync(CancellationToken ct = default) =>
        GetAsync<AccountIdentityStatusResponse>("player/external/identities", ct);

    public Task<PlayerMeResponse?> GetMeAsync(CancellationToken ct = default) =>
        GetAsync<PlayerMeResponse>("player/me", ct);

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        using var request = await CreateRequestAsync(HttpMethod.Post, "player/logout");
        await _http.SendAsync(request, ct);
    }

    public Task<PlayerProgressDto?> GetProgressAsync(CancellationToken ct = default) =>
        GetAsync<PlayerProgressDto>("player/progress", ct);

    public async Task<PlayerProgressDto?> PutProgressAsync(PlayerProgressDto progress, CancellationToken ct = default)
    {
        try
        {
            return await SendAsync<PlayerProgressDto, PlayerProgressDto>(HttpMethod.Put, "player/progress", progress, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        using var request = await CreateRequestAsync(HttpMethod.Get, path);
        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return default;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest body, CancellationToken ct)
        where TResponse : class
    {
        using var request = await CreateRequestAsync(method, path);
        request.Content = JsonContent.Create(body, options: JsonOptions);
        using var response = await _http.SendAsync(request, ct);
        var payload = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct);
        if (payload is not null)
            return payload;

        if (typeof(TResponse) == typeof(AccountAuthResponse))
            return (TResponse)(object)new AccountAuthResponse(false, "unexpected-response", "Sign-in could not be completed.", null, null);
        if (typeof(TResponse) == typeof(AccountActionResponse))
            return (TResponse)(object)new AccountActionResponse(false, "unexpected-response", "The request could not be completed.");

        throw new HttpRequestException($"Unexpected API response from {path} (HTTP {(int)response.StatusCode}).");
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        var token = await _sessions.GetLatestAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var deviceId = await _devices.GetDeviceIdAsync();
        request.Headers.TryAddWithoutValidation("X-User-Id", deviceId);
        return request;
    }
}
