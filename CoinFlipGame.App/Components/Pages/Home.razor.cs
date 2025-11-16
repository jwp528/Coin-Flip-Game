using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CoinFlipGame.App.Services;
using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;

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
    private bool isHeadsRandom = true; // Default to random
    private bool isTailsRandom = true; // Default to random
    private string faceShowing = "/img/coins/logo.png"; // The current face displayed
    private Dictionary<CoinType, List<CoinImage>>? availableCoins;
    private bool showCustomizeTip = true;
    
    // Super flip state
    private bool isSuperFlipCharging = false;
    private bool isSuperFlipReady = false;
    private DateTime? chargeStartTime = null;
    private const double SUPER_FLIP_CHARGE_TIME = 1500; // 1.5 seconds (reduced from 3 seconds)
    private const double SUPER_FLIP_UNLOCK_MULTIPLIER = 2.0; // Double unlock chance
    private CancellationTokenSource? chargeCancellationTokenSource = null;
    
    // Unlock achievement state
    private bool showUnlockAchievement = false;
    private CoinImage? currentlyUnlockedCoin = null;
    private Queue<CoinImage> pendingUnlockAchievements = new Queue<CoinImage>();
    
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
            isHeadsRandom = false; // Disable random when specific coin selected
            // Update face if currently showing heads
            if (faceShowing == selectedHeadsImage || headsCount + tailsCount == 0)
            {
                faceShowing = selectedHeadsImage;
            }
        }
        else if (selectingFor == "tails")
        {
            selectedTailsImage = coin.Path;
            isTailsRandom = false; // Disable random when specific coin selected
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
    
    private double GetChargeProgress()
    {
        if (!chargeStartTime.HasValue)
            return 0;
        
        double elapsed = (DateTime.Now - chargeStartTime.Value).TotalMilliseconds;
        double progress = Math.Min(100, (elapsed / SUPER_FLIP_CHARGE_TIME) * 100);
        return progress;
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
        
        // Start super flip charging
        StartSuperFlipCharge();
        
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
        
        // Stop super flip charging
        bool wasSuperFlipReady = isSuperFlipReady;
        StopSuperFlipCharge();
        
        double deltaY = currentY - startY;
        double speed = Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
        
        if (Math.Abs(deltaY) > DRAG_THRESHOLD || speed > VELOCITY_THRESHOLD)
        {
            await FlipCoin(speed, wasSuperFlipReady);
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
        StopSuperFlipCharge();
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
    
    private async Task FlipCoin(double velocity = 5.0, bool isSuperFlip = false)
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
        flipResult = isHeads ? (isSuperFlip ? "flip-heads super-flip" : "flip-heads") : (isSuperFlip ? "flip-tails super-flip" : "flip-tails");
        
        // Trigger particles at coin position (more particles for super flip)
        int particleCount = isSuperFlip ? 30 : 15;
        await JSRuntime.InvokeVoidAsync("triggerSparkle", coinCenterX, coinCenterY, particleCount);
        await JSRuntime.InvokeVoidAsync("playFlipSound");
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "medium");
        
        StateHasChanged();
        
        // Wait for animation duration (shorter for super flip)
        int animationDuration = isSuperFlip ? 750 : 600;
        await Task.Delay(animationDuration);
        
        // Remove animation class and FORCE neutral rotation
        flipResult = "";
        rotationX = 0;  // FORCE neutral X rotation
        rotationY = 0;  // FORCE neutral Y rotation
        
        // Determine coin image to show - apply random selection if enabled
        string landedCoinPath;
        if (isHeads && isHeadsRandom)
        {
            // Select random coin by rarity for heads
            var randomCoin = GetRandomCoinByRarity();
            landedCoinPath = randomCoin != null ? randomCoin.Path : selectedHeadsImage;
        }
        else if (!isHeads && isTailsRandom)
        {
            // Select random coin by rarity for tails
            var randomCoin = GetRandomCoinByRarity();
            landedCoinPath = randomCoin != null ? randomCoin.Path : selectedTailsImage;
        }
        else
        {
            // Use selected coin images
            landedCoinPath = isHeads ? selectedHeadsImage : selectedTailsImage;
        }
        
        // Update the face showing based on result
        faceShowing = landedCoinPath;
        
        // Update counts and streaks
        if (isHeads)
            headsCount++;
        else
            tailsCount++;
            
        UpdateStreak(result);
        
        // Track coin landing for unlock progress and check for newly unlocked coins
        var allCoins = GetAllCoinsFlat();
        var newlyUnlocked = UnlockProgress.TrackCoinLanding(landedCoinPath, isHeads, currentStreak, allCoins);
        
        // Try random unlocks based on the landed coin (with double chance for super flip)
        double unlockMultiplier = isSuperFlip ? SUPER_FLIP_UNLOCK_MULTIPLIER : 1.0;
        var randomUnlocked = UnlockProgress.TryRandomUnlocks(landedCoinPath, allCoins, unlockMultiplier);
        newlyUnlocked.AddRange(randomUnlocked);
        
        // Queue up any newly unlocked coins for achievement display
        foreach (var unlockedCoin in newlyUnlocked)
        {
            pendingUnlockAchievements.Enqueue(unlockedCoin);
        }
        
        // Explicitly reset transform via JS to ensure neutral position
        await JSRuntime.InvokeVoidAsync("coinDragHandler.resetTransform");
        
        StateHasChanged();
        
        showLandingFlash = true;
        StateHasChanged();
        
        // Trigger landing effects (bigger burst for super flip)
        int burstCount = isSuperFlip ? 40 : 20;
        await JSRuntime.InvokeVoidAsync("triggerParticleBurst", coinCenterX, coinCenterY, burstCount, new { });
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "heavy");
        
        // Set isFlipping to false BEFORE checking achievements so player can continue flipping
        isFlipping = false;
        
        await Task.Delay(500);
        showLandingFlash = false;
        StateHasChanged();
        
        // Check achievements AFTER gameplay is unlocked (non-blocking for player)
        _ = CheckAchievements();
        
        // Show unlock achievements if any are pending
        _ = ShowPendingUnlockAchievements();
    }
    
    private async Task ShowPendingUnlockAchievements()
    {
        // Process achievements one at a time
        while (pendingUnlockAchievements.Count > 0)
        {
            currentlyUnlockedCoin = pendingUnlockAchievements.Dequeue();
            showUnlockAchievement = true;
            StateHasChanged();
            
            // Wait for user to dismiss this achievement
            while (showUnlockAchievement)
            {
                await Task.Delay(100);
            }
            
            // Small delay between multiple achievements
            if (pendingUnlockAchievements.Count > 0)
            {
                await Task.Delay(300);
            }
        }
        
        currentlyUnlockedCoin = null;
    }
    
    private void DismissUnlockAchievement()
    {
        showUnlockAchievement = false;
        StateHasChanged();
    }
    
    private string GetRarityClass(UnlockRarity rarity)
    {
        return rarity switch
        {
            UnlockRarity.Common => "common",
            UnlockRarity.Uncommon => "uncommon",
            UnlockRarity.Rare => "rare",
            UnlockRarity.Legendary => "legendary",
            _ => "common"
        };
    }
    
    private List<CoinImage> GetAllCoinsFlat()
    {
        var allCoins = new List<CoinImage>();
        if (availableCoins != null)
        {
            foreach (var coinTypeGroup in availableCoins.Values)
            {
                allCoins.AddRange(coinTypeGroup);
            }
        }
        return allCoins;
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
    
    private void SelectRandomOption()
    {
        if (selectingFor == "heads")
        {
            isHeadsRandom = true;
            // Set a question mark icon or keep current for display
            selectedHeadsImage = "/img/coins/logo.png"; // Fallback display
        }
        else if (selectingFor == "tails")
        {
            isTailsRandom = true;
            // Set a question mark icon or keep current for display
            selectedTailsImage = "/img/coins/logo.png"; // Fallback display
        }
        
        CloseCoinSelector();
    }
    
    private CoinImage? GetRandomCoinByRarity()
    {
        // Get all unlocked coins
        var unlockedCoins = GetAllCoinsFlat()
            .Where(c => UnlockProgress.IsUnlocked(c))
            .ToList();
        
        if (!unlockedCoins.Any())
            return null;
        
        // Calculate total weight based on rarity
        // Rarity weights: Common=1, Uncommon=0.5, Rare=0.25, Legendary=0.1
        var weightedCoins = unlockedCoins.Select(coin =>
        {
            double weight = (coin.UnlockCondition?.Rarity ?? UnlockRarity.Common) switch
            {
                UnlockRarity.Common => 1.0,
                UnlockRarity.Uncommon => 0.5,
                UnlockRarity.Rare => 0.25,
                UnlockRarity.Legendary => 0.1,
                _ => 1.0
            };
            return new { Coin = coin, Weight = weight };
        }).ToList();
        
        double totalWeight = weightedCoins.Sum(wc => wc.Weight);
        Random random = new Random();
        double randomValue = random.NextDouble() * totalWeight;
        
        // Select coin based on weighted random
        double cumulativeWeight = 0;
        foreach (var weightedCoin in weightedCoins)
        {
            cumulativeWeight += weightedCoin.Weight;
            if (randomValue <= cumulativeWeight)
            {
                return weightedCoin.Coin;
            }
        }
        
        // Fallback to last coin (shouldn't happen)
        return weightedCoins.Last().Coin;
    }
    
    private async void StartSuperFlipCharge()
    {
        chargeStartTime = DateTime.Now;
        isSuperFlipCharging = true;
        isSuperFlipReady = false;
        
        // Cancel any existing charge timer
        chargeCancellationTokenSource?.Cancel();
        chargeCancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            // Update UI every 100ms for smooth progress bar
            while (isSuperFlipCharging && !chargeCancellationTokenSource.Token.IsCancellationRequested)
            {
                double elapsed = (DateTime.Now - chargeStartTime.Value).TotalMilliseconds;
                
                if (elapsed >= SUPER_FLIP_CHARGE_TIME && !isSuperFlipReady)
                {
                    isSuperFlipReady = true;
                    isSuperFlipCharging = false;
                    
                    // Trigger visual shake and haptic feedback
                    await JSRuntime.InvokeVoidAsync("coinDragHandler.startShake");
                    await JSRuntime.InvokeVoidAsync("triggerHaptic", "heavy");
                    await InvokeAsync(StateHasChanged);
                    break;
                }
                
                await InvokeAsync(StateHasChanged);
                await Task.Delay(100, chargeCancellationTokenSource.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation is requested
        }
    }
    
    private void StopSuperFlipCharge()
    {
        chargeCancellationTokenSource?.Cancel();
        chargeCancellationTokenSource?.Dispose();
        chargeCancellationTokenSource = null;
        isSuperFlipCharging = false;
        isSuperFlipReady = false;
        chargeStartTime = null;
    }
}
