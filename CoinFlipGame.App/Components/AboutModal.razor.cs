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

    [Inject]
    private AccountService Account { get; set; } = default!;

    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

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
    private bool accountBusy = false;
    private string accountMessage = "";

    private async Task HandleClearCache()
    {
        isClearingCache = true;
        updateCheckMessage = "";
        StateHasChanged();

        try
        {
            // User-initiated: silent SW check, then cache-clear + reload.
            // Never raise a game-wide blocking update modal.
            await UpdateService.CheckForServiceWorkerUpdate();
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

    private async Task HandleSignIn()
    {
        accountBusy = true;
        accountMessage = "";
        try
        {
            await Account.StartSignInAsync();
        }
        catch (Exception ex)
        {
            accountMessage = ex.Message;
            accountBusy = false;
        }
    }

    private async Task HandleLink()
    {
        accountBusy = true;
        accountMessage = "";
        try
        {
            await Account.StartLinkAsync();
        }
        catch (Exception ex)
        {
            accountMessage = ex.Message;
            accountBusy = false;
        }
    }

    private async Task HandleSignOut()
    {
        accountBusy = true;
        try
        {
            await Account.SignOutAsync();
            accountMessage = "Signed out. Progress stays on this device.";
        }
        catch (Exception ex)
        {
            accountMessage = ex.Message;
        }
        finally
        {
            accountBusy = false;
            StateHasChanged();
        }
    }

    private async Task ConfirmReset()
    {
        isClearing = true;
        StateHasChanged();

        try
        {
            await UnlockProgress.ResetProgressAsync();
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
