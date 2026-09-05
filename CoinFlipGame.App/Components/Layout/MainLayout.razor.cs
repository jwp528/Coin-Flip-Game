using Microsoft.AspNetCore.Components;
using CoinFlipGame.App.Services;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private UpdateService UpdateService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private UnlockProgressService UnlockProgress { get; set; } = default!;

    private bool showUpdateModal = false;
    private bool isUpdating = false;
    private System.Threading.Timer? updateCheckTimer;
    private const int UPDATE_CHECK_INTERVAL_MS = 1 * 60 * 1000; // 10 minutes
    private DotNetObjectReference<MainLayout>? _self;
    private IJSObjectReference? _progressSync;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Cache the current version if not already cached
            await UpdateService.CacheCurrentVersionAsync();
            
            // Initial update check
            await CheckForUpdates();
            
            // Start periodic update check timer
            StartUpdateCheckTimer();

            try
            {
                _self = DotNetObjectReference.Create(this);
                var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/progressSync.js");
                _progressSync = await module.InvokeAsync<IJSObjectReference>("subscribeVisibility", _self);
            }
            catch (JSException)
            {
            }
        }
    }

    [JSInvokable]
    public Task OnVisibilityHidden()
    {
        UnlockProgress.PausePlayTime();
        return UnlockProgress.FlushCloudAsync();
    }

    [JSInvokable]
    public Task OnVisibilityShown()
    {
        UnlockProgress.ResumePlayTime();
        return Task.CompletedTask;
    }

    private void StartUpdateCheckTimer()
    {
        updateCheckTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await CheckForUpdates();
            });
        }, null, UPDATE_CHECK_INTERVAL_MS, UPDATE_CHECK_INTERVAL_MS);
    }

    private async Task CheckForUpdates()
    {
        // Only check if update modal is not already showing
        if (!showUpdateModal)
        {
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

    public void Dispose()
    {
        updateCheckTimer?.Dispose();
        _self?.Dispose();
        _ = DisposeJsAsync();
    }

    private async Task DisposeJsAsync()
    {
        try
        {
            if (_progressSync is not null)
                await _progressSync.InvokeVoidAsync("dispose");
        }
        catch (JSException) { }
    }
}

