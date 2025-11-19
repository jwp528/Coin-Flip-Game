namespace CoinFlipGame.App.Models;

/// <summary>
/// Centralized game settings configuration
/// </summary>
public static class GameSettings
{
    /// <summary>
    /// Super flip charge time in milliseconds
    /// Default: 1500ms (1.5 seconds)
    /// </summary>
    public const double SUPER_FLIP_CHARGE_TIME = 750;
    
    /// <summary>
    /// Unlock chance multiplier for super flips
    /// Default: 3.0 (3x unlock chance)
    /// </summary>
    public const double SUPER_FLIP_UNLOCK_MULTIPLIER = 3.0;
    
    /// <summary>
    /// Animation duration for super flip in milliseconds
    /// </summary>
    public const int SUPER_FLIP_ANIMATION_DURATION = 750;
    
    /// <summary>
    /// Animation duration for normal flip in milliseconds
    /// </summary>
    public const int NORMAL_FLIP_ANIMATION_DURATION = 600;
}
