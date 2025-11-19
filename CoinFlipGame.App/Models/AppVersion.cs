namespace CoinFlipGame.App.Models;

/// <summary>
/// Tracks the application version and build information
/// </summary>
public class AppVersion
{
    /// <summary>
    /// Current application version
    /// </summary>
    public const string Version = "1.2.0";
    
    /// <summary>
    /// Build timestamp - updated on each build
    /// Format: yyyyMMddHHmmss
    /// </summary>
    public const string BuildTime = "20250118220000";
    
    /// <summary>
    /// Gets the cache busting query parameter
    /// </summary>
    public static string CacheBuster => $"v={Version}-{BuildTime}";
    
    /// <summary>
    /// Full version string including build time
    /// </summary>
    public static string FullVersion => $"{Version}+{BuildTime}";
}
