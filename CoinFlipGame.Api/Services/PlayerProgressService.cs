using System.Text.Json;
using Azure;
using CoinFlipGame.Api.Functions;
using CoinFlipGame.Api.Persistence;
using CoinFlipGame.Shared;
using CoinFlipGame.Shared.Dtos;

namespace CoinFlipGame.Api.Services;

public sealed class PlayerProgressService
{
    private readonly TableStorageService _storage;

    public PlayerProgressService(TableStorageService storage)
    {
        _storage = storage;
    }

    public async Task<PlayerProgressDto> GetAsync(string accountId, CancellationToken ct = default)
    {
        var entity = await _storage.GetPlayerProgressAsync(accountId, ct);
        return entity is null ? new PlayerProgressDto() : ToDto(entity);
    }

    public async Task<PlayerProgressDto> MergeAndSaveAsync(string accountId, PlayerProgressDto incoming, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var existing = await _storage.GetPlayerProgressAsync(accountId, ct);
            var merged = PlayerProgressMerger.Merge(existing is null ? null : ToDto(existing), incoming);
            var entity = FromDto(accountId, merged);
            entity.UpdatedUtc = DateTimeOffset.UtcNow;

            try
            {
                if (existing is null)
                {
                    await _storage.AddPlayerProgressAsync(entity, ct);
                    return merged;
                }

                entity.ETag = existing.ETag;
                if (await _storage.TryUpdatePlayerProgressAsync(entity, ct))
                    return merged;
            }
            catch (RequestFailedException ex) when (ex.Status is 409 or 412)
            {
                // Retry against the latest row.
            }
        }

        var fallback = await _storage.GetPlayerProgressAsync(accountId, ct);
        var lastWrite = PlayerProgressMerger.Merge(fallback is null ? null : ToDto(fallback), incoming);
        var upsert = FromDto(accountId, lastWrite);
        upsert.UpdatedUtc = DateTimeOffset.UtcNow;
        await _storage.UpsertPlayerProgressAsync(upsert, ct);
        return lastWrite;
    }

    private static PlayerProgressDto ToDto(PlayerProgressEntity entity) => new()
    {
        TotalFlips = entity.TotalFlips,
        HeadsFlips = entity.HeadsFlips,
        TailsFlips = entity.TailsFlips,
        LongestStreak = entity.LongestStreak,
        LongestHeadsStreak = entity.LongestHeadsStreak,
        LongestTailsStreak = entity.LongestTailsStreak,
        CoinLandCounts = DeserializeMap(entity.CoinLandCountsJson),
        RandomUnlockedCoins = DeserializeList(entity.RandomUnlockedCoinsJson),
        NotificationShownFor = DeserializeList(entity.NotificationShownForJson),
        CoinUnlockTimestamps = DeserializeTimestamps(entity.CoinUnlockTimestampsJson),
        CharacteristicConsecutiveCounts = DeserializeMap(entity.CharacteristicConsecutiveCountsJson),
        TotalPlayTimeSeconds = entity.TotalPlayTimeSeconds,
        UnlockedAchievements = DeserializeList(entity.UnlockedAchievementsJson)
    };

    private static PlayerProgressEntity FromDto(string accountId, PlayerProgressDto dto) => new()
    {
        PartitionKey = accountId,
        TotalFlips = dto.TotalFlips,
        HeadsFlips = dto.HeadsFlips,
        TailsFlips = dto.TailsFlips,
        LongestStreak = dto.LongestStreak,
        LongestHeadsStreak = dto.LongestHeadsStreak,
        LongestTailsStreak = dto.LongestTailsStreak,
        CoinLandCountsJson = JsonSerializer.Serialize(dto.CoinLandCounts ?? new Dictionary<string, int>(), JsonDefaults.Options),
        RandomUnlockedCoinsJson = JsonSerializer.Serialize(dto.RandomUnlockedCoins ?? new List<string>(), JsonDefaults.Options),
        NotificationShownForJson = JsonSerializer.Serialize(dto.NotificationShownFor ?? new List<string>(), JsonDefaults.Options),
        CoinUnlockTimestampsJson = JsonSerializer.Serialize(dto.CoinUnlockTimestamps ?? new Dictionary<string, DateTime>(), JsonDefaults.Options),
        CharacteristicConsecutiveCountsJson = JsonSerializer.Serialize(dto.CharacteristicConsecutiveCounts ?? new Dictionary<string, int>(), JsonDefaults.Options),
        TotalPlayTimeSeconds = dto.TotalPlayTimeSeconds,
        UnlockedAchievementsJson = JsonSerializer.Serialize(dto.UnlockedAchievements ?? new List<string>(), JsonDefaults.Options)
    };

    private static Dictionary<string, int> DeserializeMap(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, int>>(json, JsonDefaults.Options)
                   ?? new Dictionary<string, int>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, int>();
        }
    }

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonDefaults.Options) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static Dictionary<string, DateTime> DeserializeTimestamps(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json, JsonDefaults.Options)
                   ?? new Dictionary<string, DateTime>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, DateTime>();
        }
    }
}
