namespace CoinFlipGame.App.Models;

/// <summary>
/// Types of effects that coins can have when active
/// </summary>
public enum CoinEffectType
{
    /// <summary>
    /// No special effect
    /// </summary>
    None,
    
    /// <summary>
    /// Automatically clicks the coin at regular intervals
    /// </summary>
    AutoClick,
    
    /// <summary>
    /// Biases the coin flip towards landing on this side (weighted)
    /// </summary>
    Weighted,
    
    /// <summary>
    /// Biases the coin flip slightly away from landing on this side (shaved)
    /// </summary>
    Shaved
}

/// <summary>
/// Represents an effect that a coin can have when it is active
/// </summary>
public class CoinEffect
{
    /// <summary>
    /// Type of effect
    /// </summary>
    public CoinEffectType Type { get; set; } = CoinEffectType.None;
    
    /// <summary>
    /// Description of what the effect does
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Auto-click rate in milliseconds (for AutoClick effect)
    /// </summary>
    public int AutoClickInterval { get; set; } = 1000;
    
    /// <summary>
    /// Bias strength for Weighted/Shaved effects (0.0 to 1.0)
    /// Weighted: increases chance of landing this side up
    /// Shaved: decreases chance of landing this side up
    /// Example: 0.1 = 10% bias
    /// </summary>
    public double BiasStrength { get; set; } = 0.1;
}
