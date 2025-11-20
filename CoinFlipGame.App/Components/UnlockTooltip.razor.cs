using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using Microsoft.AspNetCore.Components;

namespace CoinFlipGame.App.Components;

public partial class UnlockTooltip
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public CoinImage? CoinImage { get; set; }

    [Parameter]
    public string? ProgressText { get; set; }

    [Parameter]
    public double ProgressPercentage { get; set; }

    [Parameter]
    public double X { get; set; }

    [Parameter]
    public double Y { get; set; }

    private string GetPositionStyle()
    {
        return $"left: {X}px; top: {Y}px;";
    }

    private string GetRarityClass()
    {
        return CoinImage?.UnlockCondition?.Rarity.ToString().ToLower() ?? "common";
    }

    private string GetRarityLabel()
    {
        return CoinImage?.UnlockCondition?.Rarity switch
        {
            UnlockRarity.Common => "? Common",
            UnlockRarity.Uncommon => "?? Uncommon",
            UnlockRarity.Rare => "?? Rare",
            UnlockRarity.Legendary => "?? Legendary",
            _ => "Common"
        };
    }
}
