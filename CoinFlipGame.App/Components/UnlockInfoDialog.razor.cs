using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;

namespace CoinFlipGame.App.Components;

public partial class UnlockInfoDialog
{
    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public CoinImage? CoinImage { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

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
            UnlockRarity.Rare => "??? Rare",
            UnlockRarity.Legendary => "???? Legendary",
            _ => "Common"
        };
    }

    private double CalculateProgressPercentage()
    {
        if (CoinImage?.UnlockCondition == null) return 0;

        return CoinImage.UnlockCondition.Type switch
        {
            UnlockConditionType.TotalFlips => Math.Min(100, (UnlockProgress.GetTotalFlips() / (double)CoinImage.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.HeadsFlips => Math.Min(100, (UnlockProgress.GetHeadsFlips() / (double)CoinImage.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.TailsFlips => Math.Min(100, (UnlockProgress.GetTailsFlips() / (double)CoinImage.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.Streak => Math.Min(100, (UnlockProgress.GetLongestStreak() / (double)CoinImage.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.LandOnCoin => CoinImage.UnlockCondition.RequiredCoinPath != null
                ? Math.Min(100, (UnlockProgress.GetCoinLandCount(CoinImage.UnlockCondition.RequiredCoinPath) / (double)CoinImage.UnlockCondition.RequiredCount) * 100)
                : 0,
            UnlockConditionType.LandOnMultipleCoins => CalculateLandOnMultipleCoinsProgress(CoinImage.UnlockCondition),
            UnlockConditionType.RandomChance => CalculateRandomChanceProgress(CoinImage.UnlockCondition),
            _ => 0
        };
    }

    private double CalculateLandOnMultipleCoinsProgress(UnlockCondition condition)
    {
        if (condition.RequiredCoinPaths == null || !condition.RequiredCoinPaths.Any())
            return 0;

        int completedCoins = 0;
        foreach (var coinPath in condition.RequiredCoinPaths)
        {
            if (UnlockProgress.GetCoinLandCount(coinPath) >= condition.RequiredCount)
            {
                completedCoins++;
            }
        }

        return Math.Min(100, (completedCoins / (double)condition.RequiredCoinPaths.Count) * 100);
    }

    private double CalculateRandomChanceProgress(UnlockCondition condition)
    {
        // For random chance coins, show prerequisite progress if any
        if (condition.Prerequisites != null && condition.Prerequisites.Any())
        {
            var prereq = condition.Prerequisites[0]; // Show first prerequisite progress

            return prereq.Type switch
            {
                UnlockConditionType.TotalFlips => Math.Min(100, (UnlockProgress.GetTotalFlips() / (double)prereq.RequiredCount) * 100),
                UnlockConditionType.HeadsFlips => Math.Min(100, (UnlockProgress.GetHeadsFlips() / (double)prereq.RequiredCount) * 100),
                UnlockConditionType.TailsFlips => Math.Min(100, (UnlockProgress.GetTailsFlips() / (double)prereq.RequiredCount) * 100),
                UnlockConditionType.LandOnCoin => prereq.RequiredCoinPath != null
                    ? Math.Min(100, (UnlockProgress.GetCoinLandCount(prereq.RequiredCoinPath) / (double)prereq.RequiredCount) * 100)
                    : 0,
                _ => 0
            };
        }

        return 0; // No progress bar for pure random chance
    }
}
