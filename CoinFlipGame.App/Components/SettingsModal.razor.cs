using Microsoft.AspNetCore.Components;

namespace CoinFlipGame.App.Components;

public partial class SettingsModal
{
    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public bool IsSoundEnabled { get; set; }

    [Parameter]
    public EventCallback OnToggleSound { get; set; }

    [Parameter]
    public bool IsHapticsEnabled { get; set; }

    [Parameter]
    public bool IsHapticsSupported { get; set; }

    [Parameter]
    public EventCallback OnToggleHaptics { get; set; }

    [Parameter]
    public EventCallback OnOpenAbout { get; set; }

    [Parameter]
    public bool ShowInstallApp { get; set; }

    [Parameter]
    public bool CanInstallApp { get; set; }

    [Parameter]
    public string InstallAppLabel { get; set; } = "Install app";

    [Parameter]
    public string InstallAppHint { get; set; } = "Play full-screen on your home screen";

    [Parameter]
    public EventCallback OnInstallApp { get; set; }

    private bool showUnsupportedToast;
    private CancellationTokenSource? toastCts;

    protected override void OnParametersSet()
    {
        if (!IsVisible)
            showUnsupportedToast = false;
    }

    private Task HandleToggleSound() => OnToggleSound.InvokeAsync();

    private async Task HandleToggleHaptics()
    {
        if (!IsHapticsSupported)
        {
            await ShowHapticsUnavailableToast();
            return;
        }

        await OnToggleHaptics.InvokeAsync();
    }

    private async Task ShowHapticsUnavailableToast()
    {
        toastCts?.Cancel();
        toastCts?.Dispose();
        toastCts = new CancellationTokenSource();
        var token = toastCts.Token;

        showUnsupportedToast = true;
        try
        {
            await Task.Delay(2500, token);
            if (!token.IsCancellationRequested)
                showUnsupportedToast = false;
        }
        catch (TaskCanceledException)
        {
        }
    }

    private Task HandleOpenAbout() => OnOpenAbout.InvokeAsync();

    private Task HandleInstallApp() => OnInstallApp.InvokeAsync();
}
