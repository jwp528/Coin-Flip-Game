using CoinFlipGame.Shared.Dtos;

namespace CoinFlipGame.Shared;

/// <summary>
/// Merge policy for player progress: max of numeric counters/streaks, union of
/// unlock lists/maps, earliest unlock timestamp, max consecutive counts.
/// Never wipes harder progress.
/// </summary>
public static class PlayerProgressMerger
{
    public static PlayerProgressDto Merge(PlayerProgressDto? left, PlayerProgressDto? right)
    {
        var a = left ?? new PlayerProgressDto();
        var b = right ?? new PlayerProgressDto();
        return new PlayerProgressDto
        {
            TotalFlips = Math.Max(a.TotalFlips, b.TotalFlips),
            HeadsFlips = Math.Max(a.HeadsFlips, b.HeadsFlips),
            TailsFlips = Math.Max(a.TailsFlips, b.TailsFlips),
            LongestStreak = Math.Max(a.LongestStreak, b.LongestStreak),
            LongestHeadsStreak = Math.Max(a.LongestHeadsStreak, b.LongestHeadsStreak),
            LongestTailsStreak = Math.Max(a.LongestTailsStreak, b.LongestTailsStreak),
            CoinLandCounts = MaxMap(a.CoinLandCounts, b.CoinLandCounts),
            RandomUnlockedCoins = UnionList(a.RandomUnlockedCoins, b.RandomUnlockedCoins),
            NotificationShownFor = UnionList(a.NotificationShownFor, b.NotificationShownFor),
            CoinUnlockTimestamps = EarliestMap(a.CoinUnlockTimestamps, b.CoinUnlockTimestamps),
            CharacteristicConsecutiveCounts = MaxMap(a.CharacteristicConsecutiveCounts, b.CharacteristicConsecutiveCounts),
            TotalPlayTimeSeconds = Math.Max(a.TotalPlayTimeSeconds, b.TotalPlayTimeSeconds),
            UnlockedAchievements = UnionList(a.UnlockedAchievements, b.UnlockedAchievements)
        };
    }

    private static Dictionary<string, int> MaxMap(Dictionary<string, int>? left, Dictionary<string, int>? right)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (left is not null)
        {
            foreach (var pair in left)
                result[pair.Key] = pair.Value;
        }

        if (right is not null)
        {
            foreach (var pair in right)
            {
                result[pair.Key] = result.TryGetValue(pair.Key, out var existing)
                    ? Math.Max(existing, pair.Value)
                    : pair.Value;
            }
        }

        return result;
    }

    private static Dictionary<string, DateTime> EarliestMap(
        Dictionary<string, DateTime>? left,
        Dictionary<string, DateTime>? right)
    {
        var result = new Dictionary<string, DateTime>(StringComparer.Ordinal);
        if (left is not null)
        {
            foreach (var pair in left)
                result[pair.Key] = ToUtc(pair.Value);
        }

        if (right is not null)
        {
            foreach (var pair in right)
            {
                var candidate = ToUtc(pair.Value);
                result[pair.Key] = result.TryGetValue(pair.Key, out var existing)
                    ? (candidate < existing ? candidate : existing)
                    : candidate;
            }
        }

        return result;
    }

    private static List<string> UnionList(List<string>? left, List<string>? right)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (left is not null)
        {
            foreach (var item in left)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    set.Add(item);
            }
        }

        if (right is not null)
        {
            foreach (var item in right)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    set.Add(item);
            }
        }

        return set.ToList();
    }

    private static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
