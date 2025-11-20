using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using Microsoft.AspNetCore.Components;

namespace CoinFlipGame.App.Components;

public partial class AchievementAnimation
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public CoinImage? UnlockedCoin { get; set; }

    [Parameter]
    public EventCallback OnDismiss { get; set; }

    private string GetRarityClass()
    {
        return UnlockedCoin?.UnlockCondition?.Rarity.ToString().ToLower() ?? "common";
    }

    private string GetRarityTitle()
    {
        var rarity = UnlockedCoin?.UnlockCondition?.Rarity ?? UnlockRarity.Common;
        return rarity switch
        {
            UnlockRarity.Common => "Coin Unlocked!",
            UnlockRarity.Uncommon => "Uncommon Coin Unlocked!",
            UnlockRarity.Rare => "Rare Coin Unlocked!",
            UnlockRarity.Legendary => "LEGENDARY COIN UNLOCKED!",
            _ => "Coin Unlocked!"
        };
    }

    private async Task Dismiss()
    {
        await OnDismiss.InvokeAsync();
    }
}
