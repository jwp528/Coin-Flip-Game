using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Base class for coin type definitions
/// </summary>
public abstract class CoinType
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
    
    /// <summary>
    /// Get the list of coin file names for this coin type
    /// Override this method to define coins for each type
    /// </summary>
    public virtual List<string> GetCoinFiles() => new();
    
    /// <summary>
    /// Get unlock conditions for coins in this type
    /// Key is the file name (e.g., "Gemini.png")
    /// Override this method to define unlock conditions
    /// </summary>
    public virtual Dictionary<string, UnlockCondition> GetUnlockConditions() => new();
}
