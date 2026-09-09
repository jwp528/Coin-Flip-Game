using Microsoft.JSInterop;

namespace CoinFlipGame.App.Services;

public sealed class DeviceIdService
{
    private const string StorageKey = "coinflip.deviceId";
    private readonly IJSRuntime _jsRuntime;
    private string? _cachedDeviceId;

    public DeviceIdService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<string> GetDeviceIdAsync()
    {
        if (_cachedDeviceId is not null)
            return _cachedDeviceId;

        try
        {
            var existing = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(existing))
            {
                _cachedDeviceId = existing;
                return existing;
            }

            var newId = Guid.NewGuid().ToString("N");
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, newId);
            _cachedDeviceId = newId;
            return newId;
        }
        catch (JSException)
        {
            return _cachedDeviceId ??= Guid.NewGuid().ToString("N");
        }
    }
}
