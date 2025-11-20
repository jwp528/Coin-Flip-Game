namespace CoinFlipGame.Lib.Models.DTOs;

/// <summary>
/// Unlock condition types (mirrored from App project for database storage)
/// </summary>
public enum UnlockConditionType
{
    None = 0,
    TotalFlips = 1,
    HeadsFlips = 2,
    TailsFlips = 3,
    Streak = 4,
    LandOnCoin = 5,
    RandomChance = 6,
    LandOnMultipleCoins = 7,
    LandOnCoinsWithCharacteristics = 8
}

/// <summary>
/// Specifies which side a streak must be on
/// </summary>
public enum StreakSide
{
    Heads = 0,
    Tails = 1
}

/// <summary>
/// Rarity levels for unlocks
/// </summary>
public enum UnlockRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Legendary = 3
}

/// <summary>
/// Types of filters for coin characteristics
/// </summary>
public enum CoinCharacteristicFilter
{
    SpecificCoins = 0,
    UnlockConditionType = 1,
    EffectType = 2,
    HasAnyEffect = 3,
    HasAnyUnlockCondition = 4,
    PrerequisiteCountEquals = 5,
    PrerequisiteCountGreaterThan = 6,
    PrerequisiteCountLessThan = 7
}

/// <summary>
/// Side requirement for characteristic-based unlock conditions
/// </summary>
public enum SideRequirement
{
    Either = 0,
    Both = 1,
    HeadsOnly = 2,
    TailsOnly = 3
}

/// <summary>
/// Unlock condition data structure for JSON storage
/// </summary>
public class UnlockConditionDto
{
    public UnlockConditionType Type { get; set; }
    public int RequiredCount { get; set; }
    public string? RequiredCoinPath { get; set; }
    public string Description { get; set; } = string.Empty;
    public string FlavorText { get; set; } = string.Empty;
    public double UnlockChance { get; set; }
    public List<string>? RequiredCoinPaths { get; set; }
    public UnlockRarity Rarity { get; set; }
    public List<UnlockConditionDto>? Prerequisites { get; set; }
    public bool RequiresActiveCoin { get; set; }
    public StreakSide? StreakSide { get; set; }
    public bool UseDynamicCoinList { get; set; }
    public CoinCharacteristicFilter CharacteristicFilter { get; set; }
    public UnlockConditionType? FilterUnlockConditionType { get; set; }
    public CoinEffectType? FilterEffectType { get; set; }
    public int FilterPrerequisiteCount { get; set; }
    public SideRequirement SideRequirement { get; set; }
    public int ConsecutiveCount { get; set; }
}
