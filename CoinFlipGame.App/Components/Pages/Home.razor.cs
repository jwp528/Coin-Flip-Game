using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CoinFlipGame.App.Services;
using CoinFlipGame.App.Models;

namespace CoinFlipGame.App.Components.Pages;

public class WindowDimensions
{
    public double Width { get; set; }
    public double Height { get; set; }
}

public partial class Home : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    [Inject]
    private CoinService CoinService { get; set; } = default!;
    
    private ElementReference coinElement;
    private bool isFlipping = false;
    private bool isDragging = false;
    private bool showLandingFlash = false;
    private string flipResult = "";
    private int headsCount = 0;
    private int tailsCount = 0;
    private int currentStreak = 0;
    private int longestStreak = 0;
    private string lastResult = "";
    private bool showAchievement = false;
    private string achievementText = "";
    
    // Coin customization state
    private bool showCoinSelector = false;
    private string selectingFor = ""; // "heads" or "tails"
    private string selectedHeadsImage = "/img/coins/logo.png";
    private string selectedTailsImage = "/img/coins/logo.png";
    private Dictionary<CoinType, List<CoinImage>>? availableCoins;
    private bool showCustomizeTip = true;
    
    private double startY = 0;
    private double currentY = 0;
    private double startX = 0;
    private double currentX = 0;
    private double coinCenterX = 0;
    private double coinCenterY = 0;
    private double rotationX = 0;
    private double rotationY = 0;
    private double baseRotationX = 0; // Store the coin's base rotation (0 for heads, 180 for tails)
    private double velocityY = 0;
    private double velocityX = 0;
    private DateTime? lastPointerTime = null;
    
    private const double MAX_ROTATION = 45.0;
    private const double DRAG_THRESHOLD = 50.0;
    private const double VELOCITY_THRESHOLD = 3.0;
    private const double FRICTION = 0.95;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initParticleSystem");
            await JSRuntime.InvokeVoidAsync("initCoinPhysics");
            
            // Load available coins
            availableCoins = await CoinService.GetAllAvailableCoinsAsync();
            StateHasChanged();
        }
    }
    
    private void OpenCoinSelector(string side)
    {
        selectingFor = side;
        showCoinSelector = true;
        showCustomizeTip = false; // Hide tip once user interacts
    }
    
    private void CloseCoinSelector()
    {
        showCoinSelector = false;
        selectingFor = "";
    }
    
    private void SelectCoin(CoinImage coin)
    {
        if (selectingFor == "heads")
        {
            selectedHeadsImage = coin.Path;
        }
        else if (selectingFor == "tails")
        {
            selectedTailsImage = coin.Path;
        }
        
        CloseCoinSelector();
    }
    
    private string GetCoinDisplayName(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return "Default";
            
        // Extract filename from path
        var fileName = imagePath.Split('/').LastOrDefault() ?? "Default";
        
        // Remove extension and format
        return fileName.Replace(".png", "")
                      .Replace(".jpg", "")
                      .Replace(".jpeg", "")
                      .Replace("logo", "JP Logo");
    }
    
    private string GetCoinTransform()
    {
        if (isFlipping)
        {
            return "";
        }
        
        // When dragging, apply the drag rotation
        if (isDragging && (rotationX != 0 || rotationY != 0))
        {
            return $"transform: rotateX({rotationX:F2}deg) rotateY({rotationY:F2}deg);";
        }
        
        return "";
    }
    
    private string GetCurrentCoinImage()
    {
        // Show tails when rotationX is 180, otherwise show heads
        return (rotationX == 180) ? selectedTailsImage : selectedHeadsImage;
    }
    
    private string GetShineTransform()
    {
        if (isFlipping || (!isDragging && rotationX == 0 && rotationY == 0))
        {
            return "";
        }
        
        double baseX = 20.0;
        double baseY = 20.0;
        double shineX = baseX - (rotationY * 1.5);
        double shineY = baseY + (rotationX * 1.5);
        
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
        lastPointerTime = DateTime.Now;
        
        coinCenterX = e.ClientX;
        coinCenterY = e.ClientY;
        
        // Store the current rotation as the base (0 for heads, 180 for tails)
        baseRotationX = rotationX;
        rotationY = 0;
        velocityX = 0;
        velocityY = 0;
        
        await JSRuntime.InvokeVoidAsync("coinPhysics.startDrag", coinCenterX, coinCenterY);
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "light");
    }
    
    private async Task OnPointerMove(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        double prevY = currentY;
        double prevX = currentX;
        currentY = e.ClientY;
        currentX = e.ClientX;
        
        if (lastPointerTime.HasValue)
        {
            double timeDelta = (DateTime.Now - lastPointerTime.Value).TotalMilliseconds;
            if (timeDelta > 0)
            {
                velocityY = (currentY - prevY) / timeDelta * 50;
                velocityX = (currentX - prevX) / timeDelta * 50;
            }
        }
        lastPointerTime = DateTime.Now;
        
        double offsetX = currentX - coinCenterX;
        double offsetY = currentY - coinCenterY;
        
        rotationY = Math.Clamp(offsetX * 0.25, -MAX_ROTATION, MAX_ROTATION);
        // Add the drag rotation to the base rotation (preserves heads/tails state)
        double dragRotationX = Math.Clamp(-offsetY * 0.25, -MAX_ROTATION, MAX_ROTATION);
        rotationX = baseRotationX + dragRotationX;
        
        await JSRuntime.InvokeVoidAsync("coinPhysics.updateDrag", rotationX, rotationY, offsetX, offsetY);
    }
    
    private async void OnPointerUp(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        isDragging = false;
        double deltaY = currentY - startY;
        double deltaX = currentX - startX;
        double speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
        
        if (Math.Abs(deltaY) > DRAG_THRESHOLD || speed > VELOCITY_THRESHOLD)
        {
            await FlipCoin(speed);
        }
        else
        {
            // Return to the base rotation (keeps coin on its current side)
            rotationX = baseRotationX;
            rotationY = 0;
            await JSRuntime.InvokeVoidAsync("coinPhysics.resetTransform");
            StateHasChanged();
        }
    }
    
    private async void OnPointerCancel(PointerEventArgs e)
    {
        isDragging = false;
        // Return to the base rotation (keeps coin on its current side)
        rotationX = baseRotationX;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinPhysics.resetTransform");
        StateHasChanged();
    }
    
    private async Task HandleClick()
    {
        if (isFlipping || isDragging) return;
        
        // Set a default center position if not set yet
        if (coinCenterX == 0 || coinCenterY == 0)
        {
            var windowSize = await JSRuntime.InvokeAsync<WindowDimensions>("getWindowDimensions");
            coinCenterX = windowSize.Width / 2;
            coinCenterY = windowSize.Height / 2;
        }
        
        await FlipCoin(5.0);
    }
    
    private async Task FlipCoin(double velocity = 5.0)
    {
        isFlipping = true;
        
        // Ensure we have a center position
        if (coinCenterX == 0 || coinCenterY == 0)
        {
            var windowSize = await JSRuntime.InvokeAsync<WindowDimensions>("getWindowDimensions");
            coinCenterX = windowSize.Width / 2;
            coinCenterY = windowSize.Height / 2;
        }
        
        rotationX = 0;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinPhysics.clearTransform");
        
        Random random = new Random();
        bool isHeads = random.Next(2) == 0;
        string result = isHeads ? "heads" : "tails";
        flipResult = isHeads ? "flip-heads" : "flip-tails";
        
        // Trigger particles at coin position
        await JSRuntime.InvokeVoidAsync("triggerSparkle", coinCenterX, coinCenterY, 15);
        await JSRuntime.InvokeVoidAsync("playFlipSound");
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "medium");
        
        StateHasChanged();
        
        await Task.Delay(600); // Updated to match 0.6s animation duration
        
        // Remove animation class but keep the final rotation
        flipResult = "";
        
        // Set final rotation based on result
        // Heads = rotateX(0deg) - show front face
        // Tails = rotateX(180deg) - show back face
        if (isHeads)
        {
            rotationX = 0;  // Show heads face
            baseRotationX = 0;
        }
        else
        {
            rotationX = 180;  // Show tails face
            baseRotationX = 180;
        }
        rotationY = 0;
        
        StateHasChanged();
        
        showLandingFlash = true;
        StateHasChanged();
        
        // Update counts and streaks
        if (isHeads)
            headsCount++;
        else
            tailsCount++;
            
        UpdateStreak(result);
        await CheckAchievements();
        
        // Trigger landing effects (pass empty object for options)
        await JSRuntime.InvokeVoidAsync("triggerParticleBurst", coinCenterX, coinCenterY, 20, new { });
        await JSRuntime.InvokeVoidAsync("playLandSound");
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "heavy");
        
        isFlipping = false;
        
        await Task.Delay(500);
        showLandingFlash = false;
        StateHasChanged();
    }
    
    private void UpdateStreak(string result)
    {
        if (result == lastResult)
        {
            currentStreak++;
            if (currentStreak > longestStreak)
            {
                longestStreak = currentStreak;
            }
        }
        else
        {
            currentStreak = 1;
        }
        lastResult = result;
    }
    
    private async Task CheckAchievements()
    {
        string achievement = "";
        
        if (currentStreak == 5)
            achievement = "?? 5 in a row!";
        else if (currentStreak == 10)
            achievement = "???? 10 streak! Incredible!";
        else if (currentStreak == 20)
            achievement = "?????? 20 STREAK! LEGENDARY!";
        else if (headsCount + tailsCount == 10)
            achievement = "?? First 10 flips!";
        else if (headsCount + tailsCount == 50)
            achievement = "? 50 flips milestone!";
        else if (headsCount + tailsCount == 100)
            achievement = "?? 100 flips! Master flipper!";
            
        if (!string.IsNullOrEmpty(achievement))
        {
            achievementText = achievement;
            showAchievement = true;
            StateHasChanged();
            
            // Trigger confetti at center of screen
            await JSRuntime.InvokeVoidAsync("triggerConfetti", coinCenterX, coinCenterY, 60);
            await JSRuntime.InvokeVoidAsync("playAchievementSound");
            
            await Task.Delay(3000);
            showAchievement = false;
            StateHasChanged();
        }
    }
}
