using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Persisted game achievement (separate from coin unlocks).
/// Icons use Unicode escapes so the source file stays ASCII-safe (UTF-8, no BOM).
/// </summary>
public record GameAchievement(
    string Id,
    string Title,
    string Description,
    UnlockRarity Rarity,
    string Icon = "\u2728");

public static class GameAchievements
{
    public const string FirstFlipId = "first_flip";
    public const string Play1mId = "play_1m";
    public const string Play5mId = "play_5m";
    public const string Play15mId = "play_15m";
    public const string Play1hId = "play_1h";

    // Coin, stopwatch, hourglass, hourglass done, crown
    public static readonly GameAchievement FirstFlip = new(
        FirstFlipId,
        "First Flip!",
        "The coin leaves your hand. Chance is now in play.",
        UnlockRarity.Common,
        "\U0001FA99");

    public static readonly GameAchievement Play1m = new(
        Play1mId,
        "One Minute In",
        "A minute of flips. The rhythm is starting to stick.",
        UnlockRarity.Common,
        "\u23F1\uFE0F");

    public static readonly GameAchievement Play5m = new(
        Play5mId,
        "Five-Minute Run",
        "Five minutes of air time. You're settling in.",
        UnlockRarity.Uncommon,
        "\u23F3");

    public static readonly GameAchievement Play15m = new(
        Play15mId,
        "Quarter Hour",
        "Fifteen minutes of chance. The coin knows your name.",
        UnlockRarity.Rare,
        "\u231B");

    public static readonly GameAchievement Play1h = new(
        Play1hId,
        "Hour of Fortune",
        "A full hour in the spin. Legendary patience.",
        UnlockRarity.Legendary,
        "\U0001F451");

    public static IReadOnlyList<GameAchievement> All { get; } =
    [
        FirstFlip,
        Play1m,
        Play5m,
        Play15m,
        Play1h
    ];

    public static GameAchievement? Find(string id) =>
        All.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.Ordinal));
}
