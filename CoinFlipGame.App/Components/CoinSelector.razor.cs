using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CoinFlipGame.App.Components;

public partial class CoinSelector
{
    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public string Side { get; set; } = "heads";

    [Parameter]
    public Dictionary<CoinType, List<CoinImage>>? AvailableCoins { get; set; }

    [Parameter]
    public string? SelectedCoinPath { get; set; }

    [Parameter]
    public bool IsRandomSelected { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback<CoinImage> OnCoinSelected { get; set; }

    [Parameter]
    public EventCallback OnRandomSelected { get; set; }

    private bool showTooltip = false;
    private CoinImage? hoveredCoin = null;
    private string? tooltipProgressText = null;
    private double tooltipProgressPercentage = 0;
    private double tooltipX = 0;
    private double tooltipY = 0;
    private string? expandedCoinPath = null;

    private async Task HandleCoinClick(CoinImage coin, bool isUnlocked)
    {
        if (isUnlocked)
        {
            // Select the coin if it's unlocked
            HideTooltip();
            expandedCoinPath = null;
            await OnCoinSelected.InvokeAsync(coin);
        }
        else
        {
            // Toggle expansion for locked coins
            expandedCoinPath = expandedCoinPath == coin.Path ? null : coin.Path;
        }
    }

    private async Task HandleRandomSelection()
    {
        await OnRandomSelected.InvokeAsync();
    }

    private bool IsSelectedCoin(string coinPath)
    {
        return !string.IsNullOrEmpty(SelectedCoinPath) && SelectedCoinPath == coinPath && !IsRandomSelected;
    }

    private void HandleCoinHover(CoinImage coin, MouseEventArgs e, bool isHovering)
    {
        // Show tooltip for any locked coin with unlock conditions
        if (!UnlockProgress.IsUnlocked(coin) && coin.UnlockCondition != null)
        {
            if (isHovering)
            {
                hoveredCoin = coin;
                tooltipProgressText = UnlockProgress.GetProgressDescription(coin);
                tooltipProgressPercentage = CalculateProgressPercentage(coin);
                tooltipX = e.ClientX;
                tooltipY = e.ClientY + 20; // Offset below cursor
                showTooltip = true;
            }
        }
        else
        {
            // Hide tooltip if coin is unlocked
            HideTooltip();
        }
    }

    private void HideTooltip()
    {
        showTooltip = false;
        hoveredCoin = null;
    }

    private double CalculateProgressPercentage(CoinImage coin)
    {
        if (coin.UnlockCondition == null) return 0;

        return coin.UnlockCondition.Type switch
        {
            UnlockConditionType.TotalFlips => Math.Min(100, (UnlockProgress.GetTotalFlips() / (double)coin.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.HeadsFlips => Math.Min(100, (UnlockProgress.GetHeadsFlips() / (double)coin.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.TailsFlips => Math.Min(100, (UnlockProgress.GetTailsFlips() / (double)coin.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.Streak => Math.Min(100, (UnlockProgress.GetLongestStreak() / (double)coin.UnlockCondition.RequiredCount) * 100),
            UnlockConditionType.LandOnCoin => coin.UnlockCondition.RequiredCoinPath != null
                ? Math.Min(100, (UnlockProgress.GetCoinLandCount(coin.UnlockCondition.RequiredCoinPath) / (double)coin.UnlockCondition.RequiredCount) * 100)
                : 0,
            UnlockConditionType.LandOnMultipleCoins => CalculateLandOnMultipleCoinsProgress(coin.UnlockCondition),
            UnlockConditionType.RandomChance => CalculateRandomChanceProgress(coin.UnlockCondition),
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
