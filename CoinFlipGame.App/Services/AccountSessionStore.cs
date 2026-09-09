using Microsoft.JSInterop;

namespace CoinFlipGame.App.Services;

public sealed class AccountSessionStore
{
    private const string Key = "coinflip.account-session.v1";
    private readonly IJSRuntime _jsRuntime;
    private string? _token;
    private bool _loaded;

    public AccountSessionStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<string?> GetAsync()
    {
        if (!_loaded)
            await ReloadAsync();
        return _token;
    }

    public async ValueTask<string?> GetLatestAsync()
    {
        await ReloadAsync();
        return _token;
    }

    public async Task SetAsync(string? token)
    {
        _token = token;
        _loaded = true;
        try
        {
            if (string.IsNullOrWhiteSpace(token))
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", Key);
            else
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", Key, token);
        }
        catch (JSException) { }
    }

    private async Task ReloadAsync()
    {
        try
        {
            _token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", Key);
        }
        catch (JSException)
        {
            _token = null;
        }

        _loaded = true;
    }
}
