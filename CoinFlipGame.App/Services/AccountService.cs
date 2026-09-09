using CoinFlipGame.Shared.Dtos;

namespace CoinFlipGame.App.Services;

public sealed class AccountService
{
    private readonly AccountSessionStore _sessions;
    private readonly CoinFlipApiClient _api;
    private readonly UnlockProgressService _progress;
    private readonly ExternalSignInService _externalAuth;

    public AccountService(
        AccountSessionStore sessions,
        CoinFlipApiClient api,
        UnlockProgressService progress,
        ExternalSignInService externalAuth)
    {
        _sessions = sessions;
        _api = api;
        _progress = progress;
        _externalAuth = externalAuth;
    }

    public bool Enabled => _externalAuth.Enabled;
    public bool IsSignedIn { get; private set; }
    public PlayerProfileDto? Profile { get; private set; }
    public event Action? Changed;

    public async Task InitializeAsync()
    {
        var token = await _sessions.GetAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            IsSignedIn = false;
            Profile = null;
            return;
        }

        var me = await _api.GetMeAsync();
        if (me is null)
        {
            await _sessions.SetAsync(null);
            IsSignedIn = false;
            Profile = null;
            Notify();
            return;
        }

        IsSignedIn = true;
        Profile = new PlayerProfileDto(me.AccountId, me.DisplayName, me.SessionExpiresUtc, true);
        Notify();
    }

    public Task StartSignInAsync() => _externalAuth.StartAsync("login");

    public Task StartLinkAsync() => _externalAuth.StartAsync("link");

    public async Task ApplyLoginAsync(AccountAuthResponse response)
    {
        if (!response.Success || string.IsNullOrWhiteSpace(response.SessionToken))
            return;

        await _sessions.SetAsync(response.SessionToken);
        IsSignedIn = true;
        Profile = response.Profile;
        Notify();
    }

    public async Task SignOutAsync()
    {
        await _progress.FlushCloudAsync();
        try
        {
            await _api.LogoutAsync();
        }
        catch
        {
            // Keep local sign-out even if the API is unreachable.
        }

        await _sessions.SetAsync(null);
        IsSignedIn = false;
        Profile = null;
        _progress.DetachCloud();
        Notify();
    }

    private void Notify() => Changed?.Invoke();
}
