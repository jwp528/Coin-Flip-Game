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
    
    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;
    
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
    private string faceShowing = "/img/coins/logo.png"; // The current face displayed
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
    private double velocityY = 0;
    private double velocityX = 0;
    private DateTime? lastPointerTime = null;
    
    private const double MAX_ROTATION = 45.0;
    private const double DRAG_THRESHOLD = 50.0;
    private const double VELOCITY_THRESHOLD = 3.0;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("initParticleSystem");
            await JSRuntime.InvokeVoidAsync("initCoinPhysics");
            
            // Load available coins with unlock conditions
            availableCoins = await LoadCoinsWithConditions();
            
            // Set initial face to heads
            faceShowing = selectedHeadsImage;
            StateHasChanged();
        }
    }
    
    private async Task<Dictionary<CoinType, List<CoinImage>>> LoadCoinsWithConditions()
    {
        var coins = await CoinService.GetAllAvailableCoinsAsync();
        
        // Apply unlock conditions to specific coins
        foreach (var (coinType, coinList) in coins)
        {
            if (coinType is ZodiakCoinType)
            {
                // Example: Lock zodiac coins behind conditions
                foreach (var coin in coinList)
                {
                    if (coin.Name == "Gemini.png")
                    {
                        coin.UnlockCondition = new UnlockCondition
                        {
                            Type = UnlockConditionType.TotalFlips,
                            RequiredCount = 10,
                            Description = "Flip 10 times to unlock"
                        };
                    }
                    else if (coin.Name == "Ram.png")
                    {
                        coin.UnlockCondition = new UnlockCondition
                        {
                            Type = UnlockConditionType.HeadsFlips,
                            RequiredCount = 20,
                            Description = "Get 20 heads to unlock"
                        };
                    }
                    else if (coin.Name == "Tauros.png")
                    {
                        coin.UnlockCondition = new UnlockCondition
                        {
                            Type = UnlockConditionType.Streak,
                            RequiredCount = 5,
                            Description = "Get a 5 streak to unlock"
                        };
                    }
                }
            }
        }
        
        return coins;
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
        // Check if coin is unlocked
        if (!UnlockProgress.IsUnlocked(coin))
            return; // Don't allow selection of locked coins
        
        if (selectingFor == "heads")
        {
            selectedHeadsImage = coin.Path;
            // Update face if currently showing heads
            if (faceShowing == selectedHeadsImage || headsCount + tailsCount == 0)
            {
                faceShowing = selectedHeadsImage;
            }
        }
        else if (selectingFor == "tails")
        {
            selectedTailsImage = coin.Path;
            // Update face if currently showing tails
            if (faceShowing == selectedTailsImage)
            {
                faceShowing = selectedTailsImage;
            }
        }
        
        CloseCoinSelector();
    }
    
    private string GetCoinTransform()
    {
        if (isFlipping)
        {
            return "";
        }
        
        // Apply drag rotation
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
        
        rotationX = 0;
        rotationY = 0;
        velocityX = 0;
        velocityY = 0;
        
        await JSRuntime.InvokeVoidAsync("coinDragHandler.startDrag");
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
        rotationX = Math.Clamp(-offsetY * 0.25, -MAX_ROTATION, MAX_ROTATION);
        
        await JSRuntime.InvokeVoidAsync("coinDragHandler.updateTransform", rotationX, rotationY);
        await JSRuntime.InvokeVoidAsync("coinPhysics.updateDrag", rotationX, rotationY, offsetX, offsetY);
    }
    
    private async void OnPointerUp(PointerEventArgs e)
    {
        if (!isDragging || isFlipping) return;
        
        isDragging = false;
        double deltaY = currentY - startY;
        double speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
        
        if (Math.Abs(deltaY) > DRAG_THRESHOLD || speed > VELOCITY_THRESHOLD)
        {
            await FlipCoin(speed);
        }
        else
        {
            // Return to neutral position
            rotationX = 0;
            rotationY = 0;
            await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform");
            StateHasChanged();
        }
    }
    
    private async void OnPointerCancel(PointerEventArgs e)
    {
        isDragging = false;
        // Return to neutral position
        rotationX = 0;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform");
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
        
        // Clear any drag transforms and ensure neutral starting position
        rotationX = 0;
        rotationY = 0;
        await JSRuntime.InvokeVoidAsync("coinDragHandler.clearTransform");
        
        Random random = new Random();
        bool isHeads = random.Next(2) == 0;
        string result = isHeads ? "heads" : "tails";
        flipResult = isHeads ? "flip-heads" : "flip-tails";
        
        // Trigger particles at coin position
        await JSRuntime.InvokeVoidAsync("triggerSparkle", coinCenterX, coinCenterY, 15);
        await JSRuntime.InvokeVoidAsync("playFlipSound");
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "medium");
        
        StateHasChanged();
        
        await Task.Delay(600); // Wait for animation to complete
        
        // Remove animation class and FORCE neutral rotation
        flipResult = "";
        rotationX = 0;  // FORCE neutral X rotation
        rotationY = 0;  // FORCE neutral Y rotation
        
        // Update the face showing based on result
        string landedCoinPath = isHeads ? selectedHeadsImage : selectedTailsImage;
        faceShowing = landedCoinPath;
        
        // Update counts and streaks
        if (isHeads)
            headsCount++;
        else
            tailsCount++;
            
        UpdateStreak(result);
        
        // Track coin landing for unlock progress
        UnlockProgress.TrackCoinLanding(landedCoinPath, isHeads, currentStreak);
        
        // Explicitly reset transform via JS to ensure neutral position
        await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform");
        
        StateHasChanged();
        
        showLandingFlash = true;
        StateHasChanged();
        
        // Trigger landing effects
        await JSRuntime.InvokeVoidAsync("triggerParticleBurst", coinCenterX, coinCenterY, 20, new { });
        await JSRuntime.InvokeVoidAsync("playLandSound");
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "heavy");
        
        // Set isFlipping to false BEFORE checking achievements so player can continue flipping
        isFlipping = false;
        
        await Task.Delay(500);
        showLandingFlash = false;
        StateHasChanged();
        
        // Check achievements AFTER gameplay is unlocked (non-blocking for player)
        _ = CheckAchievements();
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
            
            // Auto-dismiss after 4 seconds
            await Task.Delay(4000);
            showAchievement = false;
            StateHasChanged();
        }
    }
    
    private void DismissAchievement()
    {
        showAchievement = false;
        StateHasChanged();
    }
}
