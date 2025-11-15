using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    private ElementReference coinElement;
    private bool isFlipping = false;
    private bool isDragging = false;
    private bool showLandingFlash = false;
    private string flipResult = "";
    private int headsCount = 0;
    private int tailsCount = 0;
    
    private double startY = 0;
    private double currentY = 0;
    private double startX = 0;
    private double currentX = 0;
    private double coinCenterX = 0;
    private double coinCenterY = 0;
    private double rotationX = 0;
    private double rotationY = 0;
    private double baseRotationX = 0; // Stores the landed side rotation
    
    private const double MAX_ROTATION = 40.0;
    private const double DRAG_THRESHOLD = 60.0;
    
    private string GetCoinTransform()
    {
        if (isFlipping)
        {
            return "";
        }
        if (isDragging)
        {
            // During drag, transform is handled by JS for smoothness
            return "";
        }
        if (rotationX != 0 || rotationY != 0)
        {
            return $"transform: rotateX({rotationX:F2}deg) rotateY({rotationY:F2}deg);";
        }
        return "";
    }
    
    private string GetShineTransform()
    {
        if (isFlipping || (!isDragging && rotationX == 0 && rotationY == 0))
        {
            return ""; // Use default CSS position when idle or flipping
        }
        
        // Position shine based on coin rotation - only update during drag
        double baseX = 20.0;
        double baseY = 20.0;
        
        // Move shine opposite to rotation for lighting effect
        double shineX = baseX - (rotationY * 1.5);
        double shineY = baseY + (rotationX * 1.5);
        
        // Clamp to keep shine visible within coin
        shineX = Math.Clamp(shineX, -10, 50);
        shineY = Math.Clamp(shineY, -10, 50);
        
        return $"transform: translate3d({shineX:F2}%, {shineY:F2}%, 0);";
    }
    
    private async Task OnPointerDown(PointerEventArgs e)
    {
        if (isFlipping) return;
        
        isDragging = true;
        startY = e.ClientY;
        currentY = e.ClientY;
        startX = e.ClientX;
        currentX = e.ClientX;
        
        coinCenterX = e.ClientX;
        coinCenterY = e.ClientY;
        
        // Reset to neutral position at start of drag
        rotationX = 0;
        rotationY = 0;
        
        // Initialize JS drag handling
        await JSRuntime.InvokeVoidAsync("coinDragHandler.startDrag", coinCenterX, coinCenterY, 0);
    }
    
    private async Task OnPointerMove(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        currentY = e.ClientY;
        currentX = e.ClientX;
        
        double offsetX = currentX - coinCenterX;
        double offsetY = currentY - coinCenterY;
        
        rotationY = Math.Clamp(offsetX * 0.2, -MAX_ROTATION, MAX_ROTATION);
        rotationX = Math.Clamp(-offsetY * 0.2, -MAX_ROTATION, MAX_ROTATION);
        
        // Use JS to update transform directly without Blazor re-render
        await JSRuntime.InvokeVoidAsync("coinDragHandler.updateTransform", rotationX, rotationY);
    }
    
    private async void OnPointerUp(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        isDragging = false;
        
        double deltaY = currentY - startY;
        
        if (Math.Abs(deltaY) > DRAG_THRESHOLD)
        {
            await FlipCoin();
        }
        else
        {
            // Return to neutral position
            rotationX = 0;
            rotationY = 0;
            await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform", 0, 0);
            StateHasChanged();
        }
    }
    
    private async void OnPointerCancel(PointerEventArgs e)
    {
        isDragging = false;
        // Return to neutral position
        rotationX = 0;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform", 0, 0);
        StateHasChanged();
    }
    
    private async Task HandleClick()
    {
        if (isFlipping || isDragging) return;
        
        await FlipCoin();
    }
    
    private async Task FlipCoin()
    {
        isFlipping = true;
        
        // Clear any previous rotation state before starting flip
        rotationX = 0;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinDragHandler.clearTransform");
        
        Random random = new Random();
        bool isHeads = random.Next(2) == 0;
        flipResult = isHeads ? "flip-heads" : "flip-tails";
        
        StateHasChanged();
        
        // Wait for animation to complete (1.2 seconds)
        await Task.Delay(1200);
        
        // Remove the animation class first to prevent any transition conflicts
        flipResult = "";
        
        // Reset to neutral position (centered)
        baseRotationX = 0;
        rotationX = 0;
        rotationY = 0;
        
        // Clear all inline transform styles to ensure neutral position
        await JSRuntime.InvokeVoidAsync("coinDragHandler.clearTransform");
        
        // Show landing flash
        showLandingFlash = true;
        StateHasChanged();
        
        // Update counts
        if (isHeads)
            headsCount++;
        else
            tailsCount++;
        
        isFlipping = false;
        
        // Remove flash after animation completes
        await Task.Delay(500);
        showLandingFlash = false;
        StateHasChanged();
    }
}
