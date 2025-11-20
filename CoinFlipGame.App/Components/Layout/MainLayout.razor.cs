using Microsoft.AspNetCore.Components;
using CoinFlipGame.App.Services;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Layout;

public partial class MainLayout
{
    [Inject] private UpdateService UpdateService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool showUpdateModal = false;
    private bool isUpdating = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Cache the current version if not already cached
            await UpdateService.CacheCurrentVersionAsync();
            
            // Check for updates from API
            var updateAvailable = await UpdateService.IsUpdateAvailable();
            
            if (updateAvailable)
            {
                showUpdateModal = true;
                StateHasChanged();
            }
        }
    }

    private async Task HandleUpdate()
    {
        isUpdating = true;
        StateHasChanged();
        
        // Clear cache and reload
        await UpdateService.ClearCacheAndReload();
    }
}
