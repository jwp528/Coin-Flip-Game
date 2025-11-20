using CoinFlipGame.App.Models;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components;

public partial class CoinDrawer : IDisposable
{
    private const string RANDOM_COIN_IMAGE = "/img/coins/Random.png";
    private const double LONG_PRESS_DURATION = 500; // 500ms for long press
    private bool previousIsVisible = false;
    private bool isClosing = false;
    private string searchQuery = "";
    private string debouncedSearchQuery = ""; // The actual query used for filtering
    private System.Threading.Timer? searchDebounceTimer = null;
    private bool showUnlockInfo = false;
    private CoinImage? selectedLockedCoin = null;
    private DateTime? pointerDownTime = null;
    private CoinImage? pressedCoin = null;
    private System.Threading.Timer? longPressTimer = null;

    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

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

    [Parameter]
    public EventCallback<CoinImage> OnCoinLongPress { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Play sound when drawer visibility changes
        if (IsVisible != previousIsVisible)
        {
            try
            {
                if (IsVisible)
                {
                    await JSRuntime.InvokeVoidAsync("playDrawerOpenSound");
                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("playDrawerCloseSound");

                    // Clear particles when drawer closes to improve performance
                    await JSRuntime.InvokeVoidAsync("clearParticles");
                }
            }
            catch (Exception)
            {
                // Ignore JS interop errors (e.g., during prerendering)
            }

            previousIsVisible = IsVisible;
        }

        await base.OnParametersSetAsync();
    }

    private async Task HandleCoinClick(CoinImage coin, bool isUnlocked)
    {
        if (isUnlocked)
        {
            // Select the coin if it's unlocked
            await OnCoinSelected.InvokeAsync(coin);
        }
        else
        {
            // Show unlock info dialog for locked coins
            selectedLockedCoin = coin;
            showUnlockInfo = true;
        }
    }

    private void HandleCoinPointerDown(CoinImage coin, bool isUnlocked)
    {
        pointerDownTime = DateTime.Now;
        pressedCoin = coin;

        // Only start long press timer for unlocked coins
        if (isUnlocked)
        {
            longPressTimer?.Dispose();
            longPressTimer = new System.Threading.Timer(async _ =>
            {
                if (pressedCoin != null && pointerDownTime.HasValue)
                {
                    var elapsed = (DateTime.Now - pointerDownTime.Value).TotalMilliseconds;
                    if (elapsed >= LONG_PRESS_DURATION)
                    {
                        await InvokeAsync(async () =>
                        {
                            await OnCoinLongPress.InvokeAsync(pressedCoin);
                            pressedCoin = null;
                            pointerDownTime = null;
                        });
                    }
                }
                longPressTimer?.Dispose();
            }, null, (int)LONG_PRESS_DURATION, Timeout.Infinite);
        }
    }

    private void HandleCoinPointerUp()
    {
        // Cancel long press if pointer released before duration
        if (pointerDownTime.HasValue && pressedCoin != null)
        {
            var elapsed = (DateTime.Now - pointerDownTime.Value).TotalMilliseconds;
            bool isUnlocked = UnlockProgress.IsUnlocked(pressedCoin);

            // For unlocked coins: short click selects, long press shows preview
            // For locked coins: any click shows unlock info
            if (!isUnlocked || elapsed < LONG_PRESS_DURATION)
            {
                // This was a short click (or locked coin), handle normally
                _ = HandleCoinClick(pressedCoin, isUnlocked);
            }
        }

        pointerDownTime = null;
        pressedCoin = null;
        longPressTimer?.Dispose();
    }

    private void HandleCoinPointerCancel()
    {
        pointerDownTime = null;
        pressedCoin = null;
        longPressTimer?.Dispose();
    }

    private async Task HandleRandomSelection()
    {
        await OnRandomSelected.InvokeAsync();
    }

    private bool IsSelectedCoin(string coinPath)
    {
        return !string.IsNullOrEmpty(SelectedCoinPath) && SelectedCoinPath == coinPath && !IsRandomSelected;
    }

    private void CloseUnlockInfo()
    {
        showUnlockInfo = false;
        selectedLockedCoin = null;
    }

    private string GetShortName(string name)
    {
        // Truncate long names for better display
        return name.Length > 10 ? name.Substring(0, 10) + "..." : name;
    }

    private string GetEffectIcon(CoinEffectType effectType)
    {
        return effectType switch
        {
            CoinEffectType.AutoClick => "<i class=\"bi bi-arrow-clockwise\"></i>",
            CoinEffectType.Weighted => "<i class=\"bi bi-train-freight-front-fill\"></i>",
            CoinEffectType.Shaved => "<i class=\"bi bi-feather\"></i>",
            CoinEffectType.Combo => "<i class=\"bi bi-lightning-charge-fill\"></i>",
            _ => ""
        };
    }

    private string GetEffectBadgeClass(CoinEffectType effectType)
    {
        return effectType switch
        {
            CoinEffectType.AutoClick => "auto-click",
            CoinEffectType.Weighted => "weighted",
            CoinEffectType.Shaved => "shaved",
            CoinEffectType.Combo => "combo",
            _ => ""
        };
    }

    private string GetDrawerClasses()
    {
        if (isClosing)
            return "closing";
        if (IsVisible)
            return "open";
        return "";
    }

    private int GetUnlockedCount()
    {
        if (AvailableCoins == null) return 0;

        int count = 0;
        foreach (var coinTypeGroup in AvailableCoins.Values)
        {
            // Only count mapped coins (coins with unlock conditions or effects)
            count += coinTypeGroup
                .Where(coin => coin.UnlockCondition != null || coin.Effect != null)
                .Count(coin => UnlockProgress.IsUnlocked(coin));
        }
        return count;
    }

    private int GetTotalCount()
    {
        if (AvailableCoins == null) return 0;

        int count = 0;
        foreach (var coinTypeGroup in AvailableCoins.Values)
        {
            // Only count mapped coins (coins with unlock conditions or effects)
            count += coinTypeGroup.Count(coin => coin.UnlockCondition != null || coin.Effect != null);
        }
        return count;
    }

    private void ClearSearch()
    {
        searchQuery = "";
        debouncedSearchQuery = "";
        searchDebounceTimer?.Dispose();
        searchDebounceTimer = null;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? "";

        // Cancel previous timer
        searchDebounceTimer?.Dispose();

        // Set new timer - only update after 300ms of no typing
        searchDebounceTimer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(() =>
            {
                debouncedSearchQuery = searchQuery;
                StateHasChanged();
            });
        }, null, 300, Timeout.Infinite);
    }

    private bool MatchesSearch(CoinImage coin)
    {
        if (string.IsNullOrWhiteSpace(debouncedSearchQuery))  // Use debounced value
            return true;

        var query = debouncedSearchQuery.ToLower();
        return coin.DisplayName.ToLower().Contains(query) ||
               (coin.UnlockCondition?.Description?.ToLower().Contains(query) ?? false);
    }

    protected override void OnParametersSet()
    {
        // Detect when drawer is being closed
        if (previousIsVisible && !IsVisible)
        {
            isClosing = true;
            // Reset closing flag after animation completes
            Task.Delay(500).ContinueWith(_ =>
            {
                isClosing = false;
                InvokeAsync(StateHasChanged);
            });
        }
        else if (!previousIsVisible && IsVisible)
        {
            isClosing = false;
        }

        base.OnParametersSet();
    }

    public void Dispose()
    {
        longPressTimer?.Dispose();
        searchDebounceTimer?.Dispose();
    }
}
