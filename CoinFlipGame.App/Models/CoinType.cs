namespace CoinFlipGame.App.Models;

/// <summary>
/// Base class for coin type definitions
/// </summary>
public class CoinType
{
    /// <summary>
    /// Display name of the coin type
    /// </summary>
    public virtual string Name { get; set; } = "Default";
    
    /// <summary>
    /// Base path relative to wwwroot for coin images
    /// </summary>
    public virtual string BasePath { get; set; } = "/img/coins";
    
    /// <summary>
    /// Category identifier for grouping
    /// </summary>
    public virtual string Category { get; set; } = "Default";
    
    /// <summary>
    /// Full path to the coin type folder
    /// </summary>
    public string GetFullPath() => $"{BasePath}";
}

/// <summary>
/// Default JP Logo coin type
/// </summary>
public class JpLogoCoinType : CoinType
{
    public override string Name { get; set; } = "JP Logo";
    public override string BasePath { get; set; } = "/img/coins";
    public override string Category { get; set; } = "Default";
}

/// <summary>
/// Zodiak themed coins
/// </summary>
public class ZodiakCoinType : CoinType
{
    public override string Name { get; set; } = "Zodiak";
    public override string BasePath { get; set; } = "/img/coins/AI/Zodiak";
    public override string Category { get; set; } = "AI";
}

/// <summary>
/// Cartoon themed coins (placeholder for future expansion)
/// </summary>
public class CartoonCoinType : CoinType
{
    public override string Name { get; set; } = "Cartoon";
    public override string BasePath { get; set; } = "/img/coins/AI/Cartoon";
    public override string Category { get; set; } = "AI";
}

/// <summary>
/// Represents a specific coin image
/// </summary>
public class CoinImage
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public CoinType Type { get; set; } = new();
    public string DisplayName => Name.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
}
