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
    Shaved,
    
    /// <summary>
    /// Enhances the opposite side's effect (or adds to streak if no effect)
    /// </summary>
    Combo,
    
    /// <summary>
    /// Increases random unlock chance modifier
    /// </summary>
    Luck,
    
    /// <summary>
    /// Always lands on heads (100% bias towards heads)
    /// </summary>
    AlwaysHeads,
    
    /// <summary>
    /// Always lands on tails (100% bias towards tails)
    /// </summary>
    AlwaysTails
}

/// <summary>
/// Combo multiplier type
/// </summary>
public enum ComboType
{
    /// <summary>
    /// Adds to the effect value (e.g., +10% bias becomes +15% with +5% combo)
    /// </summary>
    Additive,
    
    /// <summary>
    /// Multiplies the effect value (e.g., +10% bias becomes +15% with 1.5x combo)
    /// </summary>
    Multiplicative
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
    
    /// <summary>
    /// Combo type for Combo effects (Additive or Multiplicative)
    /// </summary>
    public ComboType ComboType { get; set; } = ComboType.Additive;
    
    /// <summary>
    /// Combo multiplier value
    /// For Additive: added to effect (e.g., +0.05 adds 5% to bias)
    /// For Multiplicative: multiplies effect (e.g., 1.5 = 150% of original)
    /// For no effect on opposite side: adds to current streak
    /// </summary>
    public double ComboMultiplier { get; set; } = 0.05;
    
    /// <summary>
    /// Luck modifier value
    /// For Additive: added to unlock chance (e.g., +0.05 adds 5% to unlock chance)
    /// For Multiplicative: multiplies unlock chance (e.g., 1.5 = 150% of original)
    /// </summary>
    public double LuckModifier { get; set; } = 0.05;
    
    /// <summary>
    /// Luck modifier type for Luck effects (Additive or Multiplicative)
    /// </summary>
    public ComboType LuckModifierType { get; set; } = ComboType.Additive;
}
