using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CoinFlipGame.Lib.Models.Enums;

namespace CoinFlipGame.Lib.Models.Entities;

/// <summary>
/// Represents a coin in the database with its unlock criteria, prerequisites, and effects
/// </summary>
[Table("Coins", Schema = "CFG")]
public class Coin
{
    /// <summary>
    /// Unique identifier for the coin
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the coin
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Flavor text description of the coin
    /// </summary>
    [MaxLength(500)]
    public string? FlavorText { get; set; }

    /// <summary>
    /// Description of how to unlock this coin
    /// </summary>
    [MaxLength(500)]
    public string? UnlockDescription { get; set; }

    /// <summary>
    /// Full local path to the coin image (e.g., /img/coins/AI/...)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The type/category of this coin
    /// </summary>
    [Required]
    public CoinTypes CoinType { get; set; }

    /// <summary>
    /// Display name for the category/type
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Rarity level of the coin (Common, Uncommon, Rare, Legendary)
    /// </summary>
    [MaxLength(50)]
    public string? Rarity { get; set; }

    /// <summary>
    /// Whether this coin is always unlocked (no unlock criteria)
    /// </summary>
    public bool IsAlwaysUnlocked { get; set; }

    /// <summary>
    /// JSON property storing unlock criteria details
    /// Maps to UnlockCondition class in C#
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? UnlockCriteria { get; set; }

    /// <summary>
    /// JSON property storing prerequisites that must be met before unlock criteria is evaluated
    /// Maps to List&lt;UnlockCondition&gt; class in C#
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Prerequisites { get; set; }

    /// <summary>
    /// JSON property storing special effects this coin has when active
    /// Maps to CoinEffect class in C#
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? Effects { get; set; }

    /// <summary>
    /// When this coin was added to the database
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this coin was last modified
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether this coin is active/enabled in the game
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional sort order for displaying coins
    /// </summary>
    public int? SortOrder { get; set; }
}
