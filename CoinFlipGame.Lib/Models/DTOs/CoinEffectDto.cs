namespace CoinFlipGame.Lib.Models.DTOs;

/// <summary>
/// Types of effects that coins can have when active
/// </summary>
public enum CoinEffectType
{
    None = 0,
    AutoClick = 1,
    Weighted = 2,
    Shaved = 3,
    Combo = 4,
    Luck = 5
}

/// <summary>
/// Combo multiplier type
/// </summary>
public enum ComboType
{
    Additive = 0,
    Multiplicative = 1
}

/// <summary>
/// Coin effect data structure for JSON storage
/// </summary>
public class CoinEffectDto
{
    public CoinEffectType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AutoClickInterval { get; set; } = 1000;
    public double BiasStrength { get; set; } = 0.1;
    public ComboType ComboType { get; set; }
    public double ComboMultiplier { get; set; } = 0.05;
    public double LuckModifier { get; set; } = 0.05;
    public ComboType LuckModifierType { get; set; }
}
