namespace CoinFlipGame.Shared;

/// <summary>
/// Tracks the application version and build information
/// This is shared between the client app and API
/// </summary>
public class AppVersion
{
    /// <summary>
    /// Current application version
    /// </summary>
    public const string Version = "1.5.0";
    
    /// <summary>
    /// Build timestamp - updated on each build
    /// Format: yyyyMMddHHmmss
    /// </summary>
    public const string BuildTime = "20250120000000";
    
    /// <summary>
    /// Gets the cache busting query parameter
    /// </summary>
    public static string CacheBuster => $"v={Version}-{BuildTime}";
    
    /// <summary>
    /// Full version string including build time
    /// </summary>
    public static string FullVersion => $"{Version}+{BuildTime}";
}
