using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CoinFlipGame.App.Components.Pages;

public partial class Home : ComponentBase
{
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
        if (isDragging || (rotationX != 0 || rotationY != 0))
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
    
    private void OnPointerDown(PointerEventArgs e)
    {
        if (isFlipping) return;
        
        isDragging = true;
        startY = e.ClientY;
        currentY = e.ClientY;
        startX = e.ClientX;
        currentX = e.ClientX;
        
        coinCenterX = e.ClientX;
        coinCenterY = e.ClientY;
        
        // Keep the base rotation (landed side), only reset the Y rotation
        rotationX = baseRotationX;
        rotationY = 0;
    }
    
    private void OnPointerMove(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        currentY = e.ClientY;
        currentX = e.ClientX;
        
        double offsetX = currentX - coinCenterX;
        double offsetY = currentY - coinCenterY;
        
        // Invert Y rotation based on which side the coin is on
        // When on tails (180°), the rotation direction should be reversed
        double rotationMultiplier = baseRotationX == 180 ? -1 : 1;
        
        rotationY = Math.Clamp(offsetX * 0.2 * rotationMultiplier, -MAX_ROTATION, MAX_ROTATION);
        // Apply tilt on top of the base rotation (landed side)
        rotationX = Math.Clamp(baseRotationX + (-offsetY * 0.2), baseRotationX - MAX_ROTATION, baseRotationX + MAX_ROTATION);
        
        StateHasChanged();
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
            // Return to the landed side position
            rotationX = baseRotationX;
            rotationY = 0;
            StateHasChanged();
        }
    }
    
    private void OnPointerCancel(PointerEventArgs e)
    {
        isDragging = false;
        // Return to the landed side position
        rotationX = baseRotationX;
        rotationY = 0;
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
        rotationX = 0;
        rotationY = 0;
        
        Random random = new Random();
        bool isHeads = random.Next(2) == 0;
        flipResult = isHeads ? "flip-heads" : "flip-tails";
        
        StateHasChanged();
        
        // Wait for animation to complete (1.2 seconds now)
        await Task.Delay(1200);
        
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
        
        // Keep the coin on the landed side by maintaining the final rotation
        // Remove the animation class but apply the final static rotation
        flipResult = "";
        baseRotationX = isHeads ? 0 : 180; // Store the landed side
        rotationX = baseRotationX; // 0 for heads, 180 for tails
        rotationY = 0;
        StateHasChanged();
    }
}
