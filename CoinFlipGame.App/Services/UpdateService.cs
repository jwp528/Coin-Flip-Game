using Microsoft.JSInterop;
using CoinFlipGame.App.Models;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service for managing app updates and cache invalidation
/// </summary>
public class UpdateService
{
    private readonly IJSRuntime _jsRuntime;
    private const string LAST_VERSION_KEY = "app_last_version";
    
    public UpdateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    /// <summary>
    /// Check if the app has been updated since last visit
    /// </summary>
    public async Task<bool> IsUpdateAvailable()
    {
        try
        {
            var lastVersion = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", LAST_VERSION_KEY);
            return lastVersion != AppVersion.FullVersion;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Mark the current version as seen
    /// </summary>
    public async Task MarkVersionAsSeen()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LAST_VERSION_KEY, AppVersion.FullVersion);
        }
        catch
        {
            // Silent fail
        }
    }
    
    /// <summary>
    /// Clear all application caches and reload
    /// </summary>
    public async Task ClearCacheAndReload()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.clearCachesAndReload");
        }
        catch
        {
            // Fallback to hard reload
            await _jsRuntime.InvokeVoidAsync("location.reload", true);
        }
    }
    
    /// <summary>
    /// Check for service worker updates
    /// </summary>
    public async Task CheckForServiceWorkerUpdate()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("window.checkForServiceWorkerUpdate");
        }
        catch
        {
            // Silent fail if service worker not available
        }
    }
    
    /// <summary>
    /// Get the current app version
    /// </summary>
    public string GetCurrentVersion() => AppVersion.Version;
    
    /// <summary>
    /// Get the full version with build time
    /// </summary>
    public string GetFullVersion() => AppVersion.FullVersion;
}
