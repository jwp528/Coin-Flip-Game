using Blazored.LocalStorage;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components;

public partial class AboutModal : IDisposable
{
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private UpdateService UpdateService { get; set; } = default!;

    [Inject]
    private ApiVersionService ApiVersionService { get; set; } = default!;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    [Parameter]
    public EventCallback OnDataCleared { get; set; }

    private bool showResetConfirm = false;
    private bool isClearing = false;
    private bool isClearingCache = false;
    private string updateCheckMessage = "";

    private async Task HandleClearCache()
    {
        isClearingCache = true;
        StateHasChanged();

        try
        {
            // Always clear cache and reload when user clicks the button
            // Users may want to clear cache for performance/troubleshooting regardless of version
            await UpdateService.ClearCacheAndReload();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
            updateCheckMessage = "Error clearing cache";
            isClearingCache = false;
            StateHasChanged();

            // Clear error message after 3 seconds
            await Task.Delay(3000);
            updateCheckMessage = "";
            StateHasChanged();
        }
    }

    private void ShowResetConfirmation()
    {
        showResetConfirm = true;
        StateHasChanged();
    }

    private void CancelReset()
    {
        showResetConfirm = false;
        StateHasChanged();
    }

    private async Task ConfirmReset()
    {
        isClearing = true;
        StateHasChanged();

        try
        {
            await LocalStorage.ClearAsync();
            await JSRuntime.InvokeVoidAsync("location.reload");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing data: {ex.Message}");
            isClearing = false;
            showResetConfirm = false;
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
