using System.Text.Json;
using CoinFlipGame.Lib.Models.DTOs;

namespace CoinFlipGame.Api.Data.Helpers;

/// <summary>
/// Helper class for serializing and deserializing JSON properties in the Coin entity
/// </summary>
public static class CoinJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serialize UnlockConditionDto to JSON string
    /// </summary>
    public static string? SerializeUnlockCriteria(UnlockConditionDto? unlockCriteria)
    {
        if (unlockCriteria == null) return null;
        return JsonSerializer.Serialize(unlockCriteria, JsonOptions);
    }

    /// <summary>
    /// Deserialize JSON string to UnlockConditionDto
    /// </summary>
    public static UnlockConditionDto? DeserializeUnlockCriteria(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<UnlockConditionDto>(json, JsonOptions);
    }

    /// <summary>
    /// Serialize list of UnlockConditionDto to JSON string
    /// </summary>
    public static string? SerializePrerequisites(List<UnlockConditionDto>? prerequisites)
    {
        if (prerequisites == null || prerequisites.Count == 0) return null;
        return JsonSerializer.Serialize(prerequisites, JsonOptions);
    }

    /// <summary>
    /// Deserialize JSON string to list of UnlockConditionDto
    /// </summary>
    public static List<UnlockConditionDto>? DeserializePrerequisites(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<List<UnlockConditionDto>>(json, JsonOptions);
    }

    /// <summary>
    /// Serialize CoinEffectDto to JSON string
    /// </summary>
    public static string? SerializeEffects(CoinEffectDto? effects)
    {
        if (effects == null) return null;
        return JsonSerializer.Serialize(effects, JsonOptions);
    }

    /// <summary>
    /// Deserialize JSON string to CoinEffectDto
    /// </summary>
    public static CoinEffectDto? DeserializeEffects(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<CoinEffectDto>(json, JsonOptions);
    }
}
