using Microsoft.JSInterop;
using CoinFlipGame.Shared;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service for managing app updates and cache invalidation
/// </summary>
public class UpdateService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ApiVersionService _apiVersionService;
    private const string CACHED_VERSION_KEY = "app_cached_version";
    
    public UpdateService(IJSRuntime jsRuntime, ApiVersionService apiVersionService)
    {
        _jsRuntime = jsRuntime;
        _apiVersionService = apiVersionService;
    }
    
    /// <summary>
    /// Cache the current app version if not already cached
    /// Should be called on app startup
    /// </summary>
    public async Task CacheCurrentVersionAsync()
    {
        try
        {
            var cachedVersion = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CACHED_VERSION_KEY);
            
            // If no version is cached, cache the current version
            if (string.IsNullOrEmpty(cachedVersion))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CACHED_VERSION_KEY, AppVersion.FullVersion);
            }
        }
        catch
        {
            // Silent fail - if caching fails, we'll just compare against current version
        }
    }
    
    /// <summary>
    /// Check if an update is available by comparing API version with cached client version
    /// Only shows update prompt if API is reachable AND versions differ
    /// </summary>
    public async Task<bool> IsUpdateAvailable()
    {
        try
        {
            // Get the API version
            var apiVersion = await _apiVersionService.GetVersionAsync();
            
            // If API is unavailable or returns null, silently continue without update prompt
            // This prevents the modal from showing when testing locally without API
            if (apiVersion == null)
            {
                Console.WriteLine("Update check: API unavailable - skipping update check");
                return false;
            }
            
            // Get cached version (fallback to current version if not cached)
            var cachedVersion = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CACHED_VERSION_KEY);
            var versionToCompare = string.IsNullOrEmpty(cachedVersion) ? AppVersion.FullVersion : cachedVersion;
            
            // Compare API version with cached version
            var isUpdateAvailable = apiVersion.FullVersion != versionToCompare;
            
            Console.WriteLine($"Update check: API={apiVersion.FullVersion}, Cached={versionToCompare}, UpdateAvailable={isUpdateAvailable}");
            
            return isUpdateAvailable;
        }
        catch (Exception ex)
        {
            // If check fails for any reason (network error, timeout, etc.), 
            // silently continue without showing update prompt
            Console.WriteLine($"Update check: Exception occurred - {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Update the cached version to the current version
    /// Called after user updates the app
    /// </summary>
    public async Task UpdateCachedVersionAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CACHED_VERSION_KEY, AppVersion.FullVersion);
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
            // Update cached version before reloading
            await UpdateCachedVersionAsync();
            
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
