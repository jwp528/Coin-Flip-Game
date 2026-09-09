using Microsoft.AspNetCore.Components;
using CoinFlipGame.App.Services;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Layout;

public partial class MainLayout : IDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private UnlockProgressService UnlockProgress { get; set; } = default!;

    private DotNetObjectReference<MainLayout>? _self;
    private IJSObjectReference? _progressSync;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
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

    public void Dispose()
    {
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
