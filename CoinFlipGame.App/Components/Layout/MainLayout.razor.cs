using Microsoft.AspNetCore.Components;
using CoinFlipGame.App.Services;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Layout;

public partial class MainLayout
{
    [Inject] private UpdateService UpdateService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool showUpdatePrompt = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Check for updates on first load
            var updateAvailable = await UpdateService.IsUpdateAvailable();
            
            if (updateAvailable)
            {
                showUpdatePrompt = true;
                StateHasChanged();
            }
        }
    }

    private async Task UpdateApp()
    {
        await UpdateService.ClearCacheAndReload();
    }

    private async Task DismissUpdate()
    {
        showUpdatePrompt = false;
        await UpdateService.MarkVersionAsSeen();
        StateHasChanged();
    }
}
