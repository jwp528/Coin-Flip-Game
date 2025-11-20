using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CoinFlipGame.App.Services;
using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using Blazored.LocalStorage;

namespace CoinFlipGame.App.Components.Pages;

public class WindowDimensions
{
    public double Width { get; set; }
    public double Height { get; set; }
}

public partial class Home : ComponentBase, IDisposable
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    
    [Inject]
    private CoinService CoinService { get; set; } = default!;
    
    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;
    
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;
    
    [Inject]
    private ILogger<Home> Logger { get; set; } = default!;

    [Parameter]
    public string? referrer { get; set; }

    private const string CoinSelectionKey = "coinSelectionPreferences";
    private const string SoundPreferenceKey = "soundEnabled";
    private const string FirstTimeKey = "hasSeenGame";
    private const string ReferrerAppliedKey = "referrerBonusApplied";
    
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
    
    // UI state
    private bool showAboutModal = false;
    private bool isSoundEnabled = true;
    private bool showFirstTimeHint = false;
    
    // Coin customization state
    private bool showCoinSelector = false;
    private string selectingFor = ""; // "heads" or "tails"
    private string selectedHeadsImage = "/img/coins/Random.png";
    private string selectedTailsImage = "/img/coins/Random.png";
    private bool isHeadsRandom = true; // Default to random
    private bool isTailsRandom = true; // Default to random
    private string faceShowing = "/img/coins/logo.png"; // The current face displayed
    private Dictionary<CoinType, List<CoinImage>>? availableCoins;

    private bool showCustomizeTip = true;
    
    // Super flip state
    private bool isSuperFlipCharging = false;
    private bool isSuperFlipReady = false;
    private DateTime? chargeStartTime = null;
    private CancellationTokenSource? chargeCancellationTokenSource = null;
    
    // Unlock achievement state
    private bool showUnlockAchievement = false;
    private CoinImage? currentlyUnlockedCoin = null;
    private Queue<CoinImage> pendingUnlockAchievements = new Queue<CoinImage>();
    
    // Coin preview modal state
    private bool showCoinPreview = false;
    private CoinImage? previewCoin = null;
    
    // Auto-click effect state
    private System.Threading.Timer? autoClickTimer = null;
    private bool isAutoClickActive = false;
    
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
            
            // Initialize UnlockProgressService
            await UnlockProgress.InitializeAsync();
            
            // Load coin selection preferences
            await LoadCoinSelectionPreferencesAsync();
            
            // Load sound preference
            await LoadSoundPreferenceAsync();
            
            // Check if first time user
            await CheckFirstTimeUser();
            
            // Load available coins with unlock conditions BEFORE applying referrer bonus
            availableCoins = await LoadCoinsWithConditions();
            
            // Apply referrer bonus if applicable (after coins are loaded)
            await ApplyReferrerBonusAsync();
            
            // Set initial face to heads
            faceShowing = selectedHeadsImage;
            
            // Load user progress stats into local state
            headsCount = UnlockProgress.GetHeadsFlips();
            tailsCount = UnlockProgress.GetTailsFlips();
            longestStreak = UnlockProgress.GetLongestStreak();
            
            StateHasChanged();
        }
    }
    
    private async Task<Dictionary<CoinType, List<CoinImage>>> LoadCoinsWithConditions()
    {
        var coins = await CoinService.GetAllAvailableCoinsAsync();
        return coins;
    }
    
    private async Task CheckFirstTimeUser()
    {
        try
        {
            var hasSeenGame = await LocalStorage.GetItemAsync<bool>(FirstTimeKey);
            if (!hasSeenGame && headsCount == 0 && tailsCount == 0)
            {
                showFirstTimeHint = true;
                await LocalStorage.SetItemAsync(FirstTimeKey, true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking first time user");
        }
    }
    
    private async Task LoadSoundPreferenceAsync()
    {
        try
        {
            var soundEnabled = await LocalStorage.GetItemAsync<bool?>(SoundPreferenceKey);
            isSoundEnabled = soundEnabled ?? true; // Default to enabled
            
            // Sync the sound state with the JavaScript audio system
            await JSRuntime.InvokeVoidAsync("setSoundEnabled", isSoundEnabled);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading sound preference");
            isSoundEnabled = true;
        }
    }
    
    private async Task ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        await LocalStorage.SetItemAsync(SoundPreferenceKey, isSoundEnabled);
        await JSRuntime.InvokeVoidAsync("setSoundEnabled", isSoundEnabled);
        StateHasChanged();
    }
    
    private void OpenAboutModal()
    {
        showAboutModal = true;
    }
    
    private void CloseAboutModal()
    {
        showAboutModal = false;
    }
    
    private async Task HandleDataCleared()
    {
        // Reload the page after data is cleared
        await JSRuntime.InvokeVoidAsync("location.reload");
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
    
    private async void SelectCoin(CoinImage coin)
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
        
        // IMPORTANT: Force a state update BEFORE checking auto-click
        // This ensures the new coin selection is fully applied
        await SaveCoinSelectionPreferencesAsync();
        StateHasChanged();
        
        // Update auto-click state AFTER state has been saved
        // This ensures effects are read from the newly selected coins
        UpdateAutoClickState();        
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
        double progress = Math.Min(100, (elapsed / GameSettings.SUPER_FLIP_CHARGE_TIME) * 100);
        return progress;
    }
    
    private async Task OnPointerDown(PointerEventArgs e)
    {
        if (isFlipping) return;
        
        // User is interacting - pause auto-click temporarily
        if (isAutoClickActive)
        {
            StopAutoClick();
        }
        
        // Hide first time hint once user interacts
        if (showFirstTimeHint)
        {
            showFirstTimeHint = false;
            StateHasChanged();
        }
        
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
        
        // Start super flip charging (fire and forget is OK here since it has its own error handling)
        _ = StartSuperFlipCharge();
        
        try
        {
            // Preload sounds on first interaction (fire and forget)
            _ = JSRuntime.InvokeVoidAsync("preloadGameSounds");
            
            await JSRuntime.InvokeVoidAsync("coinDragHandler.startDrag");
            await JSRuntime.InvokeVoidAsync("coinPhysics.startDrag", coinCenterX, coinCenterY);
            
            if (isSoundEnabled)
            {
                await JSRuntime.InvokeVoidAsync("triggerHaptic", "light");
            }
        }
        catch (JSException)
        {
            // JS call failed, continue anyway
        }
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
        
        // Restart auto-click after user interaction
        UpdateAutoClickState();
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
        
        // Get active coin effects
        var headsEffect = GetActiveCoinEffect(selectedHeadsImage);
        var tailsEffect = GetActiveCoinEffect(selectedTailsImage);
        
        // Apply coin effect bias to flip probability
        Random random = new Random();
        double flipValue = random.NextDouble();
        bool isHeads = ApplyCoinEffectBias(flipValue, headsEffect, tailsEffect);
        string result = isHeads ? "heads" : "tails";
        flipResult = isHeads ? (isSuperFlip ? "flip-heads super-flip" : "flip-heads") : (isSuperFlip ? "flip-tails super-flip" : "flip-tails");
        
        // Trigger particles at coin position (more particles for super flip)
        int particleCount = isSuperFlip ? 30 : 15;
        await JSRuntime.InvokeVoidAsync("triggerSparkle", coinCenterX, coinCenterY, particleCount);
        await JSRuntime.InvokeVoidAsync("playFlipSound");
        
        if (isSoundEnabled)
        {
            await JSRuntime.InvokeVoidAsync("triggerHaptic", "medium");
            
            // Add special haptic for super flip
            if (isSuperFlip)
            {
                await JSRuntime.InvokeVoidAsync("triggerHaptic", "super-flip");
            }
        }
        
        StateHasChanged();
        
        // Wait for animation duration (shorter for super flip)
        int animationDuration = isSuperFlip ? GameSettings.SUPER_FLIP_ANIMATION_DURATION : GameSettings.NORMAL_FLIP_ANIMATION_DURATION;
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
        
        // Update counts and streaks
        if (isHeads)
            headsCount++;
        else
            tailsCount++;
            
        UpdateStreak(result);
        
        // Apply combo streak bonus if applicable (adds to streak counter, not probability)
        ApplyComboStreakBonus(headsEffect, tailsEffect);
        
        // Track coin landing for unlock progress and check for newly unlocked coins
        var allCoins = GetAllCoinsFlat();
        var newlyUnlocked = UnlockProgress.TrackCoinLanding(landedCoinPath, isHeads, currentStreak, allCoins, selectedHeadsImage, selectedTailsImage);
        
        // Try random unlocks based on the landed coin (with double chance for super flip)
        double unlockMultiplier = isSuperFlip ? GameSettings.SUPER_FLIP_UNLOCK_MULTIPLIER : 1.0;
        // Pass both selected coin paths to enable double chance when both faces match the required coin
        var randomUnlocked = UnlockProgress.TryRandomUnlocks(landedCoinPath, allCoins, unlockMultiplier, selectedHeadsImage, selectedTailsImage);
        newlyUnlocked.AddRange(randomUnlocked);
        
        // If a coin was unlocked, show it as the landed face for added effect
        if (newlyUnlocked != null && newlyUnlocked.Any())
        {
            // Use the first unlocked coin as the display face
            landedCoinPath = newlyUnlocked.First().Path;
        }
        
        // Update the face showing based on result (or unlocked coin)
        faceShowing = landedCoinPath;
        
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
        await JSRuntime.InvokeVoidAsync("triggerHaptic", "landing");
        
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
            
            // Play coin unlock sound
            try
            {
                await JSRuntime.InvokeVoidAsync("playCoinUnlockSound");
            }
            catch (JSException)
            {
                // Sound failed to play, continue anyway
            }
            
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
    
    private void OpenUnlockedCoinPreview()
    {
        if (currentlyUnlockedCoin != null)
        {
            // Set the preview coin and open the modal
            previewCoin = currentlyUnlockedCoin;
            showCoinPreview = true;
            
            // Dismiss the unlock notification
            showUnlockAchievement = false;
            
            StateHasChanged();
        }
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
        
        try
        {
            if (availableCoins == null)
                return allCoins;
            
            foreach (var coinTypeGroup in availableCoins.Values)
            {
                if (coinTypeGroup != null && coinTypeGroup.Any())
                {
                    allCoins.AddRange(coinTypeGroup.Where(c => c != null));
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in GetAllCoinsFlat");
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
    
    private async void SelectRandomOption()
    {
        if (selectingFor == "heads")
        {
            isHeadsRandom = true;
            // Set to Random.png to enable characteristic tracking for random coins
            selectedHeadsImage = "/img/coins/Random.png";
        }
        else if (selectingFor == "tails")
        {
            isTailsRandom = true;
            // Set to Random.png to enable characteristic tracking for random coins
            selectedTailsImage = "/img/coins/Random.png";
        }
        
        await SaveCoinSelectionPreferencesAsync();
        // Don't auto-close drawer - let user close it manually
        StateHasChanged();
    }
    
    private void HandleCoinLongPress(CoinImage coin)
    {
        previewCoin = coin;
        showCoinPreview = true;
        StateHasChanged();
    }
    
    private void CloseCoinPreview()
    {
        showCoinPreview = false;
        previewCoin = null;
    }
    
    private CoinImage? GetRandomCoinByRarity()
    {
        try
        {
            // Get all unlocked coins that have unlock conditions or effects (i.e., "assigned" coins)
            // This prevents unassigned default coins like logo.png or Random.png from being selected
            var unlockedCoins = GetAllCoinsFlat()
                .Where(c => c != null && 
                           UnlockProgress.IsUnlocked(c) && 
                           (c.UnlockCondition != null || c.Effect != null))
                .ToList();
            
            if (unlockedCoins == null || !unlockedCoins.Any())
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
            }).Where(wc => wc != null && wc.Coin != null).ToList();
            
            if (weightedCoins == null || !weightedCoins.Any())
                return unlockedCoins.FirstOrDefault();
            
            double totalWeight = weightedCoins.Sum(wc => wc.Weight);
            
            // Handle edge case where total weight might be 0
            if (totalWeight <= 0)
                return weightedCoins.FirstOrDefault()?.Coin;
            
            Random random = new Random();
            double randomValue = random.NextDouble() * totalWeight;
            
            // Select coin based on weighted random
            double cumulativeWeight = 0;
            foreach (var weightedCoin in weightedCoins)
            {
                if (weightedCoin == null || weightedCoin.Coin == null)
                    continue;
                    
                cumulativeWeight += weightedCoin.Weight;
                if (randomValue <= cumulativeWeight)
                {
                    return weightedCoin.Coin;
                }
            }
            
            // Fallback to first coin (shouldn't happen, but prevents index out of range)
            return weightedCoins.FirstOrDefault()?.Coin;
        }
        catch (Exception ex)
        {
            // Log error for debugging
            Logger.LogError(ex, "Error in GetRandomCoinByRarity");
            // Return null to fall back to selected coin
            return null;
        }
    }
    
    private async Task StartSuperFlipCharge()
    {
        try
        {
            chargeStartTime = DateTime.Now;
            isSuperFlipCharging = true;
            isSuperFlipReady = false;
            
            // Cancel any existing charge timer
            chargeCancellationTokenSource?.Cancel();
            chargeCancellationTokenSource?.Dispose();
            chargeCancellationTokenSource = new CancellationTokenSource();
            
            var token = chargeCancellationTokenSource.Token;
            
            // Update UI every 200ms for smooth progress bar with reduced overhead
            // Changed from 50ms (20 updates/sec) to 200ms (5 updates/sec) = 75% reduction in re-renders
            while (isSuperFlipCharging && !token.IsCancellationRequested)
            {
                if (!chargeStartTime.HasValue)
                    break;
                    
                double elapsed = (DateTime.Now - chargeStartTime.Value).TotalMilliseconds;
                
                if (elapsed >= GameSettings.SUPER_FLIP_CHARGE_TIME && !isSuperFlipReady)
                {
                    isSuperFlipReady = true;
                    isSuperFlipCharging = false;
                    
                    try
                    {
                        // Trigger visual shake and haptic feedback
                        if (!token.IsCancellationRequested)
                        {
                            await JSRuntime.InvokeVoidAsync("coinDragHandler.startShake");
                            await JSRuntime.InvokeVoidAsync("triggerHaptic", "super-ready");
                        }
                    }
                    catch (JSException)
                    {
                        // JS call failed, ignore and continue
                    }
                    catch (ObjectDisposedException)
                    {
                        // Component disposed, ignore
                    }
                    
                    await InvokeAsync(StateHasChanged);
                    break;
                }
                
                // Update UI every 200ms to reduce re-render overhead (was 50ms)
                // Progress bar still appears smooth but with 75% fewer state updates
                await InvokeAsync(StateHasChanged);
                await Task.Delay(200, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed, clean up
            isSuperFlipCharging = false;
            isSuperFlipReady = false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in StartSuperFlipCharge");
            isSuperFlipCharging = false;
            isSuperFlipReady = false;
        }
    }
    
    private void StopSuperFlipCharge()
    {
        try
        {
            chargeCancellationTokenSource?.Cancel();
            chargeCancellationTokenSource?.Dispose();
            chargeCancellationTokenSource = null;
            isSuperFlipCharging = false;
            isSuperFlipReady = false;
            chargeStartTime = null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in StopSuperFlipCharge");
        }
    }
    
    private async Task SaveCoinSelectionPreferencesAsync()
    {
        try
        {
            var preferences = new CoinSelectionPreferences
            {
                SelectedHeadsImage = selectedHeadsImage,
                SelectedTailsImage = selectedTailsImage,
                IsHeadsRandom = isHeadsRandom,
                IsTailsRandom = isTailsRandom
            };
            
            await LocalStorage.SetItemAsync(CoinSelectionKey, preferences);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving coin selection preferences");
        }
    }
    
    private async Task LoadCoinSelectionPreferencesAsync()
    {
        try
        {
            var preferences = await LocalStorage.GetItemAsync<CoinSelectionPreferences>(CoinSelectionKey);
            
            if (preferences != null)
            {
                selectedHeadsImage = preferences.SelectedHeadsImage;
                selectedTailsImage = preferences.SelectedTailsImage;
                isHeadsRandom = preferences.IsHeadsRandom;
                isTailsRandom = preferences.IsTailsRandom;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading coin selection preferences");
        }
    }
    
    private async Task ApplyReferrerBonusAsync()
    {
        try
        {
            // Check if referrer parameter is provided and not empty
            if (string.IsNullOrWhiteSpace(referrer))
                return;
            
            // Check if referrer bonus has already been applied
            var bonusApplied = await LocalStorage.GetItemAsync<bool>(ReferrerAppliedKey);
            if (bonusApplied)
            {
                Logger.LogInformation("Referrer bonus already applied");
                return;
            }
            
            // Check if user already has flips (don't apply bonus if they've already played)
            if (UnlockProgress.GetTotalFlips() > 0)
            {
                Logger.LogInformation("User already has flips, not applying referrer bonus");
                return;
            }
            
            // Apply 10 flips: 5 heads and 5 tails
            var allCoins = GetAllCoinsFlat();
            string defaultCoin = "/img/coins/logo.png";
            
            // Apply 5 heads flips
            for (int i = 0; i < 5; i++)
            {
                UnlockProgress.TrackCoinLanding(defaultCoin, true, 1, allCoins);
            }
            
            // Apply 5 tails flips
            for (int i = 0; i < 5; i++)
            {
                UnlockProgress.TrackCoinLanding(defaultCoin, false, 1, allCoins);
            }
            
            // Mark referrer bonus as applied
            await LocalStorage.SetItemAsync(ReferrerAppliedKey, true);
            
            Logger.LogInformation("Referrer bonus applied: 10 flips (5 heads, 5 tails)");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying referrer bonus");
        }
    }
    
    /// <summary>
    /// Get the coin effect for a specific coin path
    /// </summary>
    private CoinEffect? GetActiveCoinEffect(string coinPath)
    {
        try
        {
            if (string.IsNullOrEmpty(coinPath) || availableCoins == null)
                return null;
            
            // Find the coin in available coins
            foreach (var coinTypeGroup in availableCoins.Values)
            {
                var coin = coinTypeGroup.FirstOrDefault(c => c.Path == coinPath);
                if (coin != null)
                {
                    return coin.Effect;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting active coin effect");
        }
        
        return null;
    }
    
    /// <summary>
    /// Apply coin effect bias to determine flip result
    /// </summary>
    /// <param name="flipValue">Random value between 0 and 1</param>
    /// <param name="headsEffect">Effect for heads coin</param>
    /// <param name="tailsEffect">Effect for tails coin</param>
    /// <returns>True if heads, false if tails</returns>
    private bool ApplyCoinEffectBias(double flipValue, CoinEffect? headsEffect, CoinEffect? tailsEffect)
    {
        // Check if EITHER side has AlwaysHeads effect (Headmaster coin)
        bool hasAlwaysHeads = headsEffect?.Type == CoinEffectType.AlwaysHeads || tailsEffect?.Type == CoinEffectType.AlwaysHeads;
        
        // Check if EITHER side has AlwaysTails effect (TailMaster coin)
        bool hasAlwaysTails = headsEffect?.Type == CoinEffectType.AlwaysTails || tailsEffect?.Type == CoinEffectType.AlwaysTails;
        
        // If both are equipped, cancel out to 50/50
        if (hasAlwaysHeads && hasAlwaysTails)
        {
            Logger.LogInformation("Both AlwaysHeads and AlwaysTails equipped - effects cancel to 50/50");
            return flipValue < 0.5; // Normal 50/50 flip
        }
        
        // If only AlwaysHeads is equipped (on either side), always return heads
        if (hasAlwaysHeads)
        {
            Logger.LogInformation("AlwaysHeads effect active - forcing heads result");
            return true; // Always heads
        }
        
        // If only AlwaysTails is equipped (on either side), always return tails
        if (hasAlwaysTails)
        {
            Logger.LogInformation("AlwaysTails effect active - forcing tails result");
            return false; // Always tails
        }
        
        // No always-effects, proceed with normal bias logic
        double headsProbability = 0.5;
        double headsBias = 0.0;
        double tailsBias = 0.0;
        
        // Get enhanced effects after applying combo modifiers
        var enhancedHeadsEffect = ApplyComboEnhancement(headsEffect, tailsEffect);
        var enhancedTailsEffect = ApplyComboEnhancement(tailsEffect, headsEffect);
        
        // Calculate bias for heads coin
        if (enhancedHeadsEffect != null)
        {
            switch (enhancedHeadsEffect.Type)
            {
                case CoinEffectType.Weighted:
                    // Weighted means the coin is heavy on this side, so it lands DOWN (opposite side up)
                    headsBias = -enhancedHeadsEffect.BiasStrength;
                    break;
                case CoinEffectType.Shaved:
                    // Shaved means this side is shaved/lighter, so it lands UP more often
                    headsBias = enhancedHeadsEffect.BiasStrength;
                    break;
            }
        }
        
        // Calculate bias for tails coin
        if (enhancedTailsEffect != null)
        {
            switch (enhancedTailsEffect.Type)
            {
                case CoinEffectType.Weighted:
                    // Weighted tails means heavy on tails side, lands DOWN (heads up)
                    tailsBias = enhancedTailsEffect.BiasStrength;
                    break;
                case CoinEffectType.Shaved:
                    // Shaved tails means lighter on tails side, lands UP more (tails up, heads down)
                    tailsBias = -enhancedTailsEffect.BiasStrength;
                    break;
            }
        }
        
        // Apply combo streak boost if applicable (when opposite side has no effect)
        // If both sides have combo, they cancel out - no streak boost
        double streakBoost = 0.0;
        
        bool bothAreCombo = headsEffect?.Type == CoinEffectType.Combo && tailsEffect?.Type == CoinEffectType.Combo;
        
        if (!bothAreCombo)
        {
            // Check if heads has combo and tails has no effect
            if (headsEffect?.Type == CoinEffectType.Combo && 
                (tailsEffect == null || tailsEffect.Type == CoinEffectType.None))
            {
                streakBoost += ApplyComboStreakBoost(headsEffect, lastResult == "heads");
            }
            
            // Check if tails has combo and heads has no effect
            if (tailsEffect?.Type == CoinEffectType.Combo && 
                (headsEffect == null || headsEffect.Type == CoinEffectType.None))
            {
                streakBoost += ApplyComboStreakBoost(tailsEffect, lastResult == "tails");
            }
        }
        
        // Apply final biases and streak boost to probability
        headsProbability += headsBias + tailsBias + streakBoost;
        
        // Clamp probability between 0.1 and 0.9 to avoid guaranteed outcomes
        headsProbability = Math.Clamp(headsProbability, 0.1, 0.9);
        
        return flipValue < headsProbability;
    }
    
    /// <summary>
    /// Apply combo enhancement to an effect based on opposite side's combo
    /// If both sides have combo effects, they cancel each other out
    /// </summary>
    private CoinEffect? ApplyComboEnhancement(CoinEffect? effect, CoinEffect? oppositeEffect)
    {
        // If both sides have combo, they cancel out - no enhancement
        if (effect?.Type == CoinEffectType.Combo && oppositeEffect?.Type == CoinEffectType.Combo)
        {
            return effect;
        }
        
        // If no effect or opposite side doesn't have combo, return original
        if (effect == null || oppositeEffect?.Type != CoinEffectType.Combo)
        {
            return effect;
        }
        
        // Only enhance Weighted, Shaved, and AutoClick effects
        if (effect.Type != CoinEffectType.Weighted && 
            effect.Type != CoinEffectType.Shaved && 
            effect.Type != CoinEffectType.AutoClick)
        {
            return effect;
        }
        
        // Create enhanced copy of the effect
        var enhanced = new CoinEffect
        {
            Type = effect.Type,
            Description = effect.Description,
            BiasStrength = effect.BiasStrength,
            AutoClickInterval = effect.AutoClickInterval
        };
        
        // Apply combo enhancement based on type
        if (oppositeEffect.ComboType == ComboType.Additive)
        {
            // Additive: add combo multiplier to bias or reduce auto-click interval
            if (effect.Type == CoinEffectType.Weighted || effect.Type == CoinEffectType.Shaved)
            {
                enhanced.BiasStrength += oppositeEffect.ComboMultiplier;
            }
            else if (effect.Type == CoinEffectType.AutoClick)
            {
                // For additive, reduce interval by a fixed amount (e.g., 100ms per 0.05 multiplier)
                int reduction = (int)(oppositeEffect.ComboMultiplier * 2000); // 0.05 -> 100ms reduction
                enhanced.AutoClickInterval = Math.Max(100, effect.AutoClickInterval - reduction);
            }
        }
        else // Multiplicative
        {
            // Multiplicative: multiply bias or divide auto-click interval
            if (effect.Type == CoinEffectType.Weighted || effect.Type == CoinEffectType.Shaved)
            {
                enhanced.BiasStrength *= oppositeEffect.ComboMultiplier;
            }
            else if (effect.Type == CoinEffectType.AutoClick)
            {
                // For multiplicative, divide interval (2x -> 1/2 = 0.5 of original time, i.e., twice as fast)
                enhanced.AutoClickInterval = (int)(effect.AutoClickInterval / oppositeEffect.ComboMultiplier);
                enhanced.AutoClickInterval = Math.Max(100, enhanced.AutoClickInterval); // Min 100ms
            }
        }
        
        return enhanced;
    }
    
    /// <summary>
    /// Calculate streak boost probability adjustment from combo effect
    /// This only affects probability when opposite side HAS an effect
    /// </summary>
    private double ApplyComboStreakBoost(CoinEffect comboEffect, bool favorCurrentStreak)
    {
        if (!favorCurrentStreak || string.IsNullOrEmpty(lastResult))
            return 0.0;
        
        double boost = 0.0;
        
        if (comboEffect.ComboType == ComboType.Additive)
        {
            // Additive: add the multiplier value (e.g., +0.03 = +3% to continue streak)
            boost = comboEffect.ComboMultiplier;
        }
        else // Multiplicative
        {
            // Multiplicative: increase probability by percentage of current 50% base
            // e.g., 2x means 50% * (2 - 1.0) = 50% boost
            boost = 0.5 * (comboEffect.ComboMultiplier - 1.0);
        }
        
        // Adjust sign based on which side should be favored
        if (lastResult == "tails")
        {
            boost = -boost; // Negative means favor tails (decrease heads probability)
        }
        
        return boost;
    }
    
    /// <summary>
    /// Apply combo streak bonus directly to the streak counter
    /// This is called AFTER the flip when opposite side has NO effect
    /// Moai (Additive 0.03): Adds 3 to streak (0.03 * 100 = 3)
    /// DragonSamurai (Multiplicative 2x): Multiplies current streak by multiplier (streak * 2)
    /// </summary>
    private void ApplyComboStreakBonus(CoinEffect? headsEffect, CoinEffect? tailsEffect)
    {
        try
        {
            // Only apply if one side has combo and other has no effect
            // If both sides have combo, they cancel out
            bool headsHasCombo = headsEffect?.Type == CoinEffectType.Combo;
            bool tailsHasCombo = tailsEffect?.Type == CoinEffectType.Combo;
            
            // Both combo coins cancel each other out
            if (headsHasCombo && tailsHasCombo)
            {
                Logger.LogInformation("Both sides have combo - effects cancel out, no streak bonus applied");
                return;
            }
            
            bool headsHasEffect = headsEffect != null && 
                (headsEffect.Type == CoinEffectType.Weighted || 
                 headsEffect.Type == CoinEffectType.Shaved || 
                 headsEffect.Type == CoinEffectType.AutoClick);
            
            bool tailsHasEffect = tailsEffect != null && 
                (tailsEffect.Type == CoinEffectType.Weighted || 
                 tailsEffect.Type == CoinEffectType.Shaved || 
                 tailsEffect.Type == CoinEffectType.AutoClick);
            
            // Check if heads has combo and tails has no effect
            if (headsHasCombo && !tailsHasEffect && headsEffect != null)
            {
                if (headsEffect.ComboType == ComboType.Additive)
                {
                    // Moai (Additive 0.03): adds 3 to streak
                    // Convert percentage to whole number: 0.03 * 100 = 3
                    int streakBonus = (int)Math.Round(headsEffect.ComboMultiplier * 100);
                    currentStreak += streakBonus;
                    
                    Logger.LogInformation($"Combo (Additive) streak bonus: +{streakBonus} (new streak: {currentStreak})");
                }
                else // Multiplicative (DragonSamurai)
                {
                    // DragonSamurai (Multiplicative 2x): multiplies current streak
                    int oldStreak = currentStreak;
                    currentStreak = (int)Math.Round(currentStreak * headsEffect.ComboMultiplier);
                    
                    Logger.LogInformation($"Combo (Multiplicative) streak bonus: {oldStreak} * {headsEffect.ComboMultiplier} = {currentStreak}");
                }
                
                // Update longest streak if needed
                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }
            }
            
            // Check if tails has combo and heads has no effect
            if (tailsHasCombo && !headsHasEffect && tailsEffect != null)
            {
                if (tailsEffect.ComboType == ComboType.Additive)
                {
                    // Moai (Additive 0.03): adds 3 to streak
                    int streakBonus = (int)Math.Round(tailsEffect.ComboMultiplier * 100);
                    currentStreak += streakBonus;
                    
                    Logger.LogInformation($"Combo (Additive) streak bonus: +{streakBonus} (new streak: {currentStreak})");
                }
                else // Multiplicative (DragonSamurai)
                {
                    // DragonSamurai (Multiplicative 2x): multiplies current streak
                    int oldStreak = currentStreak;
                    currentStreak = (int)Math.Round(currentStreak * tailsEffect.ComboMultiplier);
                    
                    Logger.LogInformation($"Combo (Multiplicative) streak bonus: {oldStreak} * {tailsEffect.ComboMultiplier} = {currentStreak}");
                }
                
                // Update longest streak if needed
                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying combo streak bonus");
        }
    }
    
    /// <summary>
    /// Update auto-click state based on active coins
    /// </summary>
    private void UpdateAutoClickState()
    {
        try
        {
            // Check if either active coin has auto-click effect
            var headsEffect = GetActiveCoinEffect(selectedHeadsImage);
            var tailsEffect = GetActiveCoinEffect(selectedTailsImage);
            
            // Apply combo enhancements to effects
            var enhancedHeadsEffect = ApplyComboEnhancement(headsEffect, tailsEffect);
            var enhancedTailsEffect = ApplyComboEnhancement(tailsEffect, headsEffect);
            
            bool shouldAutoClick = (enhancedHeadsEffect?.Type == CoinEffectType.AutoClick) || 
                                   (enhancedTailsEffect?.Type == CoinEffectType.AutoClick);
            
            if (shouldAutoClick && !isAutoClickActive)
            {
                // Get the auto-click interval from the enhanced effect
                int interval = enhancedHeadsEffect?.Type == CoinEffectType.AutoClick 
                    ? enhancedHeadsEffect.AutoClickInterval 
                    : (enhancedTailsEffect?.AutoClickInterval ?? 1000);
                
                StartAutoClick(interval);
            }
            else if (!shouldAutoClick && isAutoClickActive)
            {
                StopAutoClick();
            }
            else if (shouldAutoClick && isAutoClickActive)
            {
                // Auto-click is active but interval might have changed due to combo
                int newInterval = enhancedHeadsEffect?.Type == CoinEffectType.AutoClick 
                    ? enhancedHeadsEffect.AutoClickInterval 
                    : (enhancedTailsEffect?.AutoClickInterval ?? 1000);
                
                // Restart with new interval if it changed
                StartAutoClick(newInterval);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating auto-click state");
        }
    }
    
    /// <summary>
    /// Start auto-click timer
    /// </summary>
    private void StartAutoClick(int intervalMs)
    {
        try
        {
            StopAutoClick(); // Stop existing timer if any
            
            isAutoClickActive = true;
            autoClickTimer = new System.Threading.Timer(async _ =>
            {
                if (!isFlipping && !isDragging && isAutoClickActive)
                {
                    await InvokeAsync(async () =>
                    {
                        // Trigger a flip
                        await HandleClick();
                    });
                }
            }, null, intervalMs, intervalMs);
            
            Logger.LogInformation($"Auto-click started with interval {intervalMs}ms");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting auto-click");
        }
    }
    
    /// <summary>
    /// Stop auto-click timer
    /// </summary>
    private void StopAutoClick()
    {
        try
        {
            if (autoClickTimer != null)
            {
                autoClickTimer.Dispose();
                autoClickTimer = null;
                isAutoClickActive = false;
                Logger.LogInformation("Auto-click stopped");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping auto-click");
        }
    }
    
    /// <summary>
    /// Dispose resources when component is destroyed
    /// </summary>
    public void Dispose()
    {
        try
        {
            // Stop auto-click timer
            StopAutoClick();
            
            // Stop super flip charging
            StopSuperFlipCharge();
            
            Logger.LogInformation("Home component disposed");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error disposing Home component");
        }
    }
}
