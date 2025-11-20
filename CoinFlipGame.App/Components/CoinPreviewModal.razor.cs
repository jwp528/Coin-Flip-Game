using CoinFlipGame.App.Models;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;

namespace CoinFlipGame.App.Components;

public partial class CoinPreviewModal : IDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public CoinImage? CoinImage { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    private ElementReference previewCoinElement;
    private bool isDragging = false;
    private double startX = 0;
    private double startY = 0;
    private double currentX = 0;
    private double currentY = 0;
    private double rotationX = 15;  // Default starting rotation
    private double rotationY = -15; // Default starting rotation
    private const double MAX_ROTATION_X = 75.0;  // Limit vertical rotation to prevent full inversion
    private const double MAX_ROTATION_Y = 75.0;  // Limit horizontal rotation
    private DotNetObjectReference<CoinPreviewModal>? dotNetRef;
    private bool hasBeenVisible = false;

    protected override async Task OnParametersSetAsync()
    {
        // Reset rotation when modal is opened
        if (IsVisible && !hasBeenVisible)
        {
            rotationX = 15;
            rotationY = -15;
            hasBeenVisible = true;
        }
        else if (!IsVisible)
        {
            hasBeenVisible = false;
            // Reset rotation when modal closes
            rotationX = 15;
            rotationY = -15;
        }

        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Setup document-level event listeners
            dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("coinPreviewModal.setupEvents", dotNetRef);
        }
    }

    [JSInvokable]
    public void HandlePointerMove(double clientX, double clientY)
    {
        if (!isDragging) return;

        currentX = clientX;
        currentY = clientY;

        double deltaX = currentX - startX;
        double deltaY = currentY - startY;

        // Convert movement to rotation with limits to prevent inversion
        rotationY = Math.Clamp(deltaX / 2, -MAX_ROTATION_Y, MAX_ROTATION_Y);
        rotationX = Math.Clamp(-deltaY / 2, -MAX_ROTATION_X, MAX_ROTATION_X);

        StateHasChanged();
    }

    [JSInvokable]
    public void HandlePointerUp()
    {
        isDragging = false;
    }

    private void OnPointerDown(PointerEventArgs e)
    {
        isDragging = true;
        startX = e.ClientX;
        startY = e.ClientY;
        currentX = e.ClientX;
        currentY = e.ClientY;
    }

    private void OnPointerMove(PointerEventArgs e)
    {
        if (!isDragging) return;

        currentX = e.ClientX;
        currentY = e.ClientY;

        double deltaX = currentX - startX;
        double deltaY = currentY - startY;

        // Convert movement to rotation with limits to prevent inversion
        rotationY = Math.Clamp(deltaX / 2, -MAX_ROTATION_Y, MAX_ROTATION_Y);
        rotationX = Math.Clamp(-deltaY / 2, -MAX_ROTATION_X, MAX_ROTATION_X);

        StateHasChanged();
    }

    private void OnPointerUp(PointerEventArgs e)
    {
        isDragging = false;
    }

    private void OnPointerCancel(PointerEventArgs e)
    {
        isDragging = false;
    }

    private string GetCoinTransform()
    {
        return $"transform: rotateX({rotationX:F2}deg) rotateY({rotationY:F2}deg);";
    }

    private string GetShineTransform()
    {
        double baseX = 20.0;
        double baseY = 20.0;
        double shineX = baseX - (rotationY * 1.5);
        double shineY = baseY + (rotationX * 1.5);

        shineX = Math.Clamp(shineX, -10, 50);
        shineY = Math.Clamp(shineY, -10, 50);

        return $"transform: translate3d({shineX:F2}%, {shineY:F2}%, 0);";
    }

    private string FormatUnlockDate(DateTime unlockDate)
    {
        var local = unlockDate.ToLocalTime();
        var now = DateTime.Now;
        var diff = now - local;

        if (diff.TotalMinutes < 1)
            return "Just now";
        else if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes == 1 ? "" : "s")} ago";
        else if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours == 1 ? "" : "s")} ago";
        else if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} day{((int)diff.TotalDays == 1 ? "" : "s")} ago";
        else
            return local.ToString("MMM dd, yyyy");
    }

    private string FormatUnlockType(Models.Unlocks.UnlockConditionType type)
    {
        return type switch
        {
            Models.Unlocks.UnlockConditionType.None => "Always Available",
            Models.Unlocks.UnlockConditionType.TotalFlips => "Total Flips Required",
            Models.Unlocks.UnlockConditionType.HeadsFlips => "Heads Flips Required",
            Models.Unlocks.UnlockConditionType.TailsFlips => "Tails Flips Required",
            Models.Unlocks.UnlockConditionType.Streak => "Streak Achievement",
            Models.Unlocks.UnlockConditionType.LandOnCoin => "Land on Specific Coin",
            Models.Unlocks.UnlockConditionType.RandomChance => "Random Unlock",
            Models.Unlocks.UnlockConditionType.LandOnMultipleCoins => "Collect Multiple Coins",
            _ => "Unknown"
        };
    }

    public void Dispose()
    {
        dotNetRef?.Dispose();
    }
}
