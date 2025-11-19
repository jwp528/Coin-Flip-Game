namespace CoinFlipGame.Api.Models;

/// <summary>
/// Tracks the application version and build information
/// This should be kept in sync with the client-side AppVersion
/// </summary>
public class AppVersion
{
    /// <summary>
    /// Current application version
    /// </summary>
    public const string Version = "1.3.0";
    
    /// <summary>
    /// Build timestamp - updated on each build
    /// Format: yyyyMMddHHmmss
    /// </summary>
    public const string BuildTime = "20250119000000";
    
    /// <summary>
    /// Full version string including build time
    /// </summary>
    public static string FullVersion => $"{Version}+{BuildTime}";
}
