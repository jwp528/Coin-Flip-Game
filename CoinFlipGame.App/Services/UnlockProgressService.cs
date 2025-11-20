using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using Blazored.LocalStorage;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service to track and manage coin unlock progress
/// </summary>
public class UnlockProgressService
{
    private const string StorageKey = "coinUnlockProgress";
    private readonly ILocalStorageService? _localStorage;
    private readonly CoinService _coinService;
    private Dictionary<string, int> _coinLandCounts = new();
    private HashSet<string> _randomUnlockedCoins = new();
    private HashSet<string> _notificationShownFor = new();
    private Dictionary<string, DateTime> _coinUnlockTimestamps = new();
    // Track consecutive characteristic landings for each unlock condition
    // Key: coin path (the coin to unlock), Value: current consecutive count
    private Dictionary<string, int> _characteristicConsecutiveCounts = new();
    private int _totalFlips = 0;
    private int _headsFlips = 0;
    private int _tailsFlips = 0;
    private int _longestStreak = 0;
    private int _longestHeadsStreak = 0;
    private int _longestTailsStreak = 0;
    private Random _random = new();
    private bool _isInitialized = false;
    
    public UnlockProgressService(ILocalStorageService localStorage, CoinService coinService)
    {
        _localStorage = localStorage;
        _coinService = coinService;
    }
    
    /// <summary>
    /// Initialize and load progress from localStorage
    /// Must be called from a component's OnAfterRenderAsync
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;
            
        await LoadProgressAsync();
        _isInitialized = true;
    }
    
    /// <summary>
    /// Track a coin landing and check for newly unlocked coins
    /// Returns list of coins that were newly unlocked as a result of this landing
    /// </summary>
    /// <param name="coinPath">Path of the coin that was landed on</param>
    /// <param name="isHeads">Whether the landed coin was on heads side</param>
    /// <param name="currentStreak">Current streak count</param>
    /// <param name="allCoins">All available coins</param>
    /// <param name="headsCoinPath">The coin selected for heads face (for characteristic tracking)</param>
    /// <param name="tailsCoinPath">The coin selected for tails face (for characteristic tracking)</param>
    public List<CoinImage> TrackCoinLanding(string coinPath, bool isHeads, int currentStreak, List<CoinImage> allCoins, string? headsCoinPath = null, string? tailsCoinPath = null)
    {
        try
        {
            // Track total flips
            _totalFlips++;
            
            // Track heads/tails
            if (isHeads)
            {
                _headsFlips++;
                // Update heads streak
                if (currentStreak > _longestHeadsStreak)
                    _longestHeadsStreak = currentStreak;
            }
            else
            {
                _tailsFlips++;
                // Update tails streak
                if (currentStreak > _longestTailsStreak)
                    _longestTailsStreak = currentStreak;
            }
            
            // Track overall streak (any side)
            if (currentStreak > _longestStreak)
                _longestStreak = currentStreak;
            
            // Track specific coin landings
            if (!_coinLandCounts.ContainsKey(coinPath))
                _coinLandCounts[coinPath] = 0;
            
            _coinLandCounts[coinPath]++;
            
            // Get the landed coin's data
            CoinImage? landedCoin = allCoins?.FirstOrDefault(c => c.Path == coinPath);
            
            // Update characteristic-based consecutive tracking
            UpdateCharacteristicConsecutiveTracking(landedCoin, isHeads, headsCoinPath, tailsCoinPath, allCoins);
            
            _ = SaveProgressAsync(); // Fire and forget
            
            // Check for newly unlocked coins
            return CheckNewlyUnlockedCoins(allCoins ?? new List<CoinImage>());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TrackCoinLanding: {ex.Message}");
            return new List<CoinImage>();
        }
    }
    
    /// <summary>
    /// Check which coins have been newly unlocked based on current progress
    /// </summary>
    private List<CoinImage> CheckNewlyUnlockedCoins(List<CoinImage> allCoins)
    {
        var newlyUnlocked = new List<CoinImage>();
        
        try
        {
            if (allCoins == null || !allCoins.Any())
                return newlyUnlocked;
            
            foreach (var coin in allCoins)
            {
                if (coin == null)
                    continue;
                    
                // Skip if already unlocked or no unlock condition
                if (coin.UnlockCondition == null)
                    continue;
                
                // Skip if we've already shown notification for this coin
                if (_notificationShownFor.Contains(coin.Path))
                    continue;
                
                // Check if coin is now unlocked
                if (!IsUnlocked(coin))
                    continue;
                
                // Don't show notification for random chance coins here - they're handled in TryRandomUnlocks
                if (coin.UnlockCondition.Type == UnlockConditionType.RandomChance)
                    continue;
                
                // Check if this is the exact moment the coin was unlocked
                bool justUnlocked = false;
                
                switch (coin.UnlockCondition.Type)
                {
                    case UnlockConditionType.TotalFlips:
                        justUnlocked = _totalFlips == coin.UnlockCondition.RequiredCount;
                        break;
                    case UnlockConditionType.HeadsFlips:
                        justUnlocked = _headsFlips == coin.UnlockCondition.RequiredCount;
                        break;
                    case UnlockConditionType.TailsFlips:
                        justUnlocked = _tailsFlips == coin.UnlockCondition.RequiredCount;
                        break;
                    case UnlockConditionType.Streak:
                        // Check if streak side is specified
                        if (coin.UnlockCondition.StreakSide.HasValue)
                        {
                            if (coin.UnlockCondition.StreakSide.Value == Models.Unlocks.StreakSide.Heads)
                                justUnlocked = _longestHeadsStreak == coin.UnlockCondition.RequiredCount;
                            else
                                justUnlocked = _longestTailsStreak == coin.UnlockCondition.RequiredCount;
                        }
                        else
                        {
                            // Legacy: any streak
                            justUnlocked = _longestStreak == coin.UnlockCondition.RequiredCount;
                        }
                        break;
                    case UnlockConditionType.LandOnCoin:
                        if (!string.IsNullOrEmpty(coin.UnlockCondition.RequiredCoinPath))
                        {
                            justUnlocked = _coinLandCounts.TryGetValue(coin.UnlockCondition.RequiredCoinPath, out int count)
                                && count == coin.UnlockCondition.RequiredCount;
                        }
                        break;
                    case UnlockConditionType.LandOnMultipleCoins:
                        if (coin.UnlockCondition.RequiredCoinPaths != null && coin.UnlockCondition.RequiredCoinPaths.Any())
                        {
                            // Check if all required coins just reached the required count
                            justUnlocked = coin.UnlockCondition.RequiredCoinPaths.Any(path =>
                                !string.IsNullOrEmpty(path) && 
                                _coinLandCounts.TryGetValue(path, out int count) && 
                                count == coin.UnlockCondition.RequiredCount);
                        }
                        break;
                    
                    case UnlockConditionType.LandOnCoinsWithCharacteristics:
                        // Check if consecutive count just reached the required amount
                        if (_characteristicConsecutiveCounts.TryGetValue(coin.Path, out int charCount))
                        {
                            justUnlocked = charCount == coin.UnlockCondition.ConsecutiveCount;
                        }
                        break;
                }
                
                if (justUnlocked)
                {
                    newlyUnlocked.Add(coin);
                    // Mark notification as shown
                    if (!_notificationShownFor.Contains(coin.Path))
                    {
                        _notificationShownFor.Add(coin.Path);
                    }
                    // Record unlock timestamp
                    if (!_coinUnlockTimestamps.ContainsKey(coin.Path))
                    {
                        _coinUnlockTimestamps[coin.Path] = DateTime.UtcNow;
                    }
                }
            }
            
            if (newlyUnlocked.Any())
            {
                _ = SaveProgressAsync(); // Fire and forget
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckNewlyUnlockedCoins: {ex.Message}");
        }
        
        return newlyUnlocked;
    }
    
    /// <summary>
    /// Check if a coin is unlocked based on its conditions
    /// </summary>
    public bool IsUnlocked(CoinImage coin)
    {
        if (coin.UnlockCondition == null)
            return true; // No condition = always unlocked
        
        // Check if this is a compound condition with prerequisites
        if (coin.UnlockCondition.Prerequisites != null && coin.UnlockCondition.Prerequisites.Any())
        {
            // All prerequisites must be satisfied
            foreach (var prerequisite in coin.UnlockCondition.Prerequisites)
            {
                if (!IsConditionMet(prerequisite, coin.Path))
                {
                    return false; // Prerequisite not met
                }
            }
        }
        
        // If prerequisites are met (or none exist), check the main condition
        return IsConditionMet(coin.UnlockCondition, coin.Path);
    }
    
    /// <summary>
    /// Check if a specific unlock condition is met
    /// </summary>
    private bool IsConditionMet(UnlockCondition condition, string coinPath = "")
    {
        return condition.Type switch
        {
            UnlockConditionType.None => true,
            UnlockConditionType.TotalFlips => _totalFlips >= condition.RequiredCount,
            UnlockConditionType.HeadsFlips => _headsFlips >= condition.RequiredCount,
            UnlockConditionType.TailsFlips => _tailsFlips >= condition.RequiredCount,
            UnlockConditionType.Streak => CheckStreakCondition(condition),
            UnlockConditionType.LandOnCoin => CheckLandOnCoinCondition(condition),
            UnlockConditionType.RandomChance => _randomUnlockedCoins.Contains(coinPath),
            UnlockConditionType.LandOnMultipleCoins => CheckLandOnMultipleCoinsCondition(condition, coinPath),
            UnlockConditionType.LandOnCoinsWithCharacteristics => CheckCharacteristicCondition(condition, coinPath),
            _ => false
        };
    }
    
    private bool CheckStreakCondition(UnlockCondition condition)
    {
        if (condition.StreakSide.HasValue)
        {
            if (condition.StreakSide.Value == Models.Unlocks.StreakSide.Heads)
                return _longestHeadsStreak >= condition.RequiredCount;
            else
                return _longestTailsStreak >= condition.RequiredCount;
        }
        else
        {
            // Legacy: any streak
            return _longestStreak >= condition.RequiredCount;
        }
    }
    
    /// <summary>
    /// Check if characteristic-based consecutive condition is met
    /// </summary>
    private bool CheckCharacteristicCondition(UnlockCondition condition, string coinPath)
    {
        if (!_characteristicConsecutiveCounts.TryGetValue(coinPath, out int consecutiveCount))
            return false;
        
        return consecutiveCount >= condition.ConsecutiveCount;
    }
    
    /// <summary>
    /// Try to randomly unlock coins when using a specific coin
    /// Returns list of coins that were newly unlocked
    /// </summary>
    /// <param name="usedCoinPath">The coin path that was landed on</param>
    /// <param name="allCoins">List of all available coins</param>
    /// <param name="unlockChanceMultiplier">Base unlock chance multiplier (e.g., for super flips)</param>
    /// <param name="headsCoinPath">The coin selected for heads face (optional, for double chance check)</param>
    /// <param name="tailsCoinPath">The coin selected for tails face (optional, for double chance check)</param>
    public List<CoinImage> TryRandomUnlocks(string usedCoinPath, List<CoinImage> allCoins, double unlockChanceMultiplier = 1.0, string? headsCoinPath = null, string? tailsCoinPath = null)
    {
        try
        {
            var newlyUnlocked = new List<CoinImage>();
            
            if (allCoins == null || !allCoins.Any())
                return newlyUnlocked;
            
            foreach (var coin in allCoins)
            {
                if (coin == null)
                    continue;
                    
                // Skip if already unlocked
                if (IsUnlocked(coin))
                    continue;
                
                // Check if this is a random unlock coin
                if (coin.UnlockCondition?.Type == UnlockConditionType.RandomChance)
                {
                    // Check prerequisites first
                    if (coin.UnlockCondition.Prerequisites != null && coin.UnlockCondition.Prerequisites.Any())
                    {
                        bool prerequisitesMet = true;
                        foreach (var prerequisite in coin.UnlockCondition.Prerequisites)
                        {
                            if (prerequisite == null || !IsConditionMet(prerequisite, coin.Path))
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                        
                        if (!prerequisitesMet)
                            continue; // Prerequisites not met, skip this coin
                    }
                    
                    // For coins that require active selection (like Dragon_Rare)
                    bool shouldRoll = false;
                    if (coin.UnlockCondition.RequiresActiveCoin)
                    {
                        // Only roll if the required coin is being used
                        shouldRoll = coin.UnlockCondition.RequiredCoinPath == usedCoinPath;
                    }
                    else
                    {
                        // For coins without active requirement, always roll if prerequisites are met
                        shouldRoll = true;
                    }
                    
                    if (shouldRoll)
                    {
                        // Start with base unlock chance multiplier (e.g., super flip bonus)
                        double effectiveMultiplier = unlockChanceMultiplier;
                        
                        // Check if both heads and tails are set to the required coin for double chance
                        if (coin.UnlockCondition.RequiresActiveCoin && 
                            !string.IsNullOrEmpty(coin.UnlockCondition.RequiredCoinPath) &&
                            !string.IsNullOrEmpty(headsCoinPath) &&
                            !string.IsNullOrEmpty(tailsCoinPath))
                        {
                            // If both faces match the required coin, double the multiplier
                            if (headsCoinPath == coin.UnlockCondition.RequiredCoinPath && 
                                tailsCoinPath == coin.UnlockCondition.RequiredCoinPath)
                            {
                                effectiveMultiplier *= 2.0; // Double the chance!
                            }
                        }
                        
                        // Apply effective multiplier to unlock chance
                        double effectiveChance = coin.UnlockCondition.UnlockChance * effectiveMultiplier;
                        
                        // Roll for unlock
                        if (_random.NextDouble() <= effectiveChance)
                        {
                            _randomUnlockedCoins.Add(coin.Path);
                            // Mark notification as shown for random unlocks
                            if (!_notificationShownFor.Contains(coin.Path))
                            {
                                _notificationShownFor.Add(coin.Path);
                            }
                            // Record unlock timestamp
                            if (!_coinUnlockTimestamps.ContainsKey(coin.Path))
                            {
                                _coinUnlockTimestamps[coin.Path] = DateTime.UtcNow;
                            }
                            newlyUnlocked.Add(coin);
                        }
                    }
                }
            }
            
            if (newlyUnlocked.Any())
            {
                _ = SaveProgressAsync(); // Fire and forget
            }
            
            return newlyUnlocked;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in TryRandomUnlocks: {ex.Message}");
            return new List<CoinImage>();
        }
    }
    
    /// <summary>
    /// Manually unlock a coin (for testing or special events)
    /// </summary>
    public async Task UnlockCoinAsync(string coinPath)
    {
        if (!_randomUnlockedCoins.Contains(coinPath))
        {
            _randomUnlockedCoins.Add(coinPath);
            // Don't mark notification as shown for manual unlocks
            // This allows notifications to show if triggered later
            await SaveProgressAsync();
        }
    }
    
    private bool CheckLandOnCoinCondition(UnlockCondition condition)
    {
        if (string.IsNullOrEmpty(condition.RequiredCoinPath))
            return false;
        
        return _coinLandCounts.TryGetValue(condition.RequiredCoinPath, out int count) 
            && count >= condition.RequiredCount;
    }
    
    private bool CheckLandOnMultipleCoinsCondition(UnlockCondition condition, string currentCoinPath = "")
    {
        var requiredPaths = condition.RequiredCoinPaths;
        
        // If UseDynamicCoinList is true, we need to fetch all coins dynamically
        if (condition.UseDynamicCoinList)
        {
            // Get all coins synchronously from cache (they should already be loaded)
            // We'll populate the condition with all coins except the current one
            var allPaths = _coinService.GetAllUnlockableCoinPathsAsync(currentCoinPath).GetAwaiter().GetResult();
            requiredPaths = allPaths;
        }
        
        if (requiredPaths == null || !requiredPaths.Any())
            return false;
        
        // Check that ALL required coins have been landed on the required number of times
        foreach (var coinPath in requiredPaths)
        {
            if (!_coinLandCounts.TryGetValue(coinPath, out int count) || count < condition.RequiredCount)
            {
                return false; // This coin hasn't been landed on enough times
            }
        }
        
        return true; // All coins have been landed on enough times
    }
    
    /// <summary>
    /// Update consecutive characteristic landing tracking for all pending unlock conditions
    /// </summary>
    private void UpdateCharacteristicConsecutiveTracking(CoinImage? landedCoin, bool isHeads, string? headsCoinPath, string? tailsCoinPath, List<CoinImage>? allCoins)
    {
        if (allCoins == null || !allCoins.Any())
            return;
        
        // Check all coins with characteristic-based unlock conditions
        foreach (var unlockableCoin in allCoins)
        {
            if (unlockableCoin?.UnlockCondition?.Type == UnlockConditionType.LandOnCoinsWithCharacteristics)
            {
                // Check if landed coin matches the characteristic
                bool matches = DoesCoinMatchCharacteristic(landedCoin, unlockableCoin.UnlockCondition, isHeads, headsCoinPath, tailsCoinPath, allCoins);
                
                if (matches)
                {
                    // Increment consecutive count for this unlock condition
                    if (!_characteristicConsecutiveCounts.ContainsKey(unlockableCoin.Path))
                        _characteristicConsecutiveCounts[unlockableCoin.Path] = 0;
                    
                    _characteristicConsecutiveCounts[unlockableCoin.Path]++;
                }
                else
                {
                    // Reset consecutive count if doesn't match
                    _characteristicConsecutiveCounts[unlockableCoin.Path] = 0;
                }
            }
        }
    }
    
    /// <summary>
    /// Check if a coin matches the characteristic filter
    /// </summary>
    private bool DoesCoinMatchCharacteristic(CoinImage? landedCoin, UnlockCondition condition, bool isHeads, string? headsCoinPath, string? tailsCoinPath, List<CoinImage> allCoins)
    {
        if (landedCoin == null)
            return false;
        
        // Check if a specific coin is required to be active (like RandomChance unlock conditions)
        if (condition.RequiresActiveCoin && !string.IsNullOrEmpty(condition.RequiredCoinPath))
        {
            // Verify the required coin is active based on side requirement
            bool requiredCoinActive = CheckRequiredCoinActive(condition.RequiredCoinPath, condition.SideRequirement, headsCoinPath, tailsCoinPath);
            if (!requiredCoinActive)
                return false;
        }
        
        // Check side requirement
        bool sideMatches = CheckSideRequirement(landedCoin.Path, condition.SideRequirement, isHeads, headsCoinPath, tailsCoinPath);
        if (!sideMatches)
            return false;
        
        // Check characteristic filter
        switch (condition.CharacteristicFilter)
        {
            case CoinCharacteristicFilter.SpecificCoins:
                return condition.RequiredCoinPaths != null && condition.RequiredCoinPaths.Contains(landedCoin.Path);
            
            case CoinCharacteristicFilter.UnlockConditionType:
                return condition.FilterUnlockConditionType.HasValue && 
                       landedCoin.UnlockCondition?.Type == condition.FilterUnlockConditionType.Value;
            
            case CoinCharacteristicFilter.EffectType:
                return condition.FilterEffectType.HasValue && 
                       landedCoin.Effect?.Type == condition.FilterEffectType.Value;
            
            case CoinCharacteristicFilter.HasAnyEffect:
                return landedCoin.Effect != null && landedCoin.Effect.Type != CoinEffectType.None;
            
            case CoinCharacteristicFilter.HasAnyUnlockCondition:
                return landedCoin.UnlockCondition != null && landedCoin.UnlockCondition.Type != UnlockConditionType.None;
            
            case CoinCharacteristicFilter.PrerequisiteCountEquals:
                int prereqCount = landedCoin.UnlockCondition?.Prerequisites?.Count ?? 0;
                return prereqCount == condition.FilterPrerequisiteCount;
            
            case CoinCharacteristicFilter.PrerequisiteCountGreaterThan:
                int prereqCountGt = landedCoin.UnlockCondition?.Prerequisites?.Count ?? 0;
                return prereqCountGt > condition.FilterPrerequisiteCount;
            
            case CoinCharacteristicFilter.PrerequisiteCountLessThan:
                int prereqCountLt = landedCoin.UnlockCondition?.Prerequisites?.Count ?? 0;
                return prereqCountLt < condition.FilterPrerequisiteCount;
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if the side requirement is met for a coin
    /// </summary>
    private bool CheckSideRequirement(string coinPath, SideRequirement requirement, bool isHeads, string? headsCoinPath, string? tailsCoinPath)
    {
        // Special handling for Random.png - it acts as a wildcard for any random coin
        const string randomCoinPath = "/img/coins/Random.png";
        
        switch (requirement)
        {
            case SideRequirement.Either:
                // Coin can be on either side
                // If heads or tails is set to Random.png, any coin from the random pool matches
                bool matchesHeads = coinPath == headsCoinPath || 
                                   (headsCoinPath == randomCoinPath && IsRandomCoin(coinPath));
                bool matchesTails = coinPath == tailsCoinPath || 
                                   (tailsCoinPath == randomCoinPath && IsRandomCoin(coinPath));
                return matchesHeads || matchesTails;
            
            case SideRequirement.Both:
                // Coin must be on both sides simultaneously
                // If both sides are Random.png, any random coin matches
                bool matchesHeadsBoth = coinPath == headsCoinPath || 
                                       (headsCoinPath == randomCoinPath && IsRandomCoin(coinPath));
                bool matchesTailsBoth = coinPath == tailsCoinPath || 
                                       (tailsCoinPath == randomCoinPath && IsRandomCoin(coinPath));
                return matchesHeadsBoth && matchesTailsBoth;
            
            case SideRequirement.HeadsOnly:
                // Coin must be on heads side
                return isHeads && (coinPath == headsCoinPath || 
                                  (headsCoinPath == randomCoinPath && IsRandomCoin(coinPath)));
            
            case SideRequirement.TailsOnly:
                // Coin must be on tails side
                return !isHeads && (coinPath == tailsCoinPath || 
                                   (tailsCoinPath == randomCoinPath && IsRandomCoin(coinPath)));
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if a required coin is currently active based on the side requirement
    /// Used for RequiresActiveCoin + RequiredCoinPath in characteristic-based unlocks
    /// </summary>
    private bool CheckRequiredCoinActive(string requiredCoinPath, SideRequirement sideRequirement, string? headsCoinPath, string? tailsCoinPath)
    {
        switch (sideRequirement)
        {
            case SideRequirement.Either:
                // Required coin must be on either heads OR tails
                return requiredCoinPath == headsCoinPath || requiredCoinPath == tailsCoinPath;
            
            case SideRequirement.Both:
                // Required coin must be on BOTH heads AND tails
                return requiredCoinPath == headsCoinPath && requiredCoinPath == tailsCoinPath;
            
            case SideRequirement.HeadsOnly:
                // Required coin must be on heads
                return requiredCoinPath == headsCoinPath;
            
            case SideRequirement.TailsOnly:
                // Required coin must be on tails
                return requiredCoinPath == tailsCoinPath;
            
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if a coin path belongs to the random coin pool
    /// Random coins are any coins with unlock conditions or effects (excluding logo.png and Random.png itself)
    /// </summary>
    private bool IsRandomCoin(string coinPath)
    {
        // Random.png itself is not a random coin (it's the selector)
        if (coinPath == "/img/coins/Random.png" || coinPath == "/img/coins/logo.png")
            return false;
        
        // A coin is part of the random pool if it has unlock conditions or effects
        // This matches the logic in GetRandomCoinByRarity()
        return true; // If we're tracking it and it's not logo/Random, it's in the pool
    }
    
    /// <summary>
    /// Get number of times a coin has been landed on
    /// </summary>
    public int GetCoinLandCount(string coinPath)
    {
        return _coinLandCounts.TryGetValue(coinPath, out int count) ? count : 0;
    }
    
    /// <summary>
    /// Get total flips
    /// </summary>
    public int GetTotalFlips() => _totalFlips;
    
    /// <summary>
    /// Get heads flips
    /// </summary>
    public int GetHeadsFlips() => _headsFlips;
    
    /// <summary>
    /// Get tails flips
    /// </summary>
    public int GetTailsFlips() => _tailsFlips;
    
    /// <summary>
    /// Get longest streak
    /// </summary>
    public int GetLongestStreak() => _longestStreak;
    
    /// <summary>
    /// Get longest heads streak
    /// </summary>
    public int GetLongestHeadsStreak() => _longestHeadsStreak;
    
    /// <summary>
    /// Get longest tails streak
    /// </summary>
    public int GetLongestTailsStreak() => _longestTailsStreak;
    
    /// <summary>
    /// Get unlock timestamp for a coin
    /// </summary>
    public DateTime? GetUnlockTimestamp(string coinPath)
    {
        return _coinUnlockTimestamps.TryGetValue(coinPath, out DateTime timestamp) ? timestamp : null;
    }
    
    /// <summary>
    /// Get progress description for a locked coin
    /// </summary>
    public string GetProgressDescription(CoinImage coin)
    {
        if (coin.UnlockCondition == null || IsUnlocked(coin))
            return "Unlocked";
        
        return coin.UnlockCondition.Type switch
        {
            UnlockConditionType.TotalFlips => $"{_totalFlips}/{coin.UnlockCondition.RequiredCount} flips",
            UnlockConditionType.HeadsFlips => $"{_headsFlips}/{coin.UnlockCondition.RequiredCount} heads",
            UnlockConditionType.TailsFlips => $"{_tailsFlips}/{coin.UnlockCondition.RequiredCount} tails",
            UnlockConditionType.Streak => GetStreakProgress(coin.UnlockCondition),
            UnlockConditionType.LandOnCoin => GetLandOnCoinProgress(coin.UnlockCondition),
            UnlockConditionType.RandomChance => $"Random unlock ({coin.UnlockCondition.UnlockChance * 100:F3}% chance)",
            UnlockConditionType.LandOnMultipleCoins => GetLandOnMultipleCoinsProgress(coin.UnlockCondition, coin.Path),
            UnlockConditionType.LandOnCoinsWithCharacteristics => GetCharacteristicProgress(coin.UnlockCondition, coin.Path),
            _ => "Locked"
        };
    }
    
    private string GetCharacteristicProgress(UnlockCondition condition, string coinPath)
    {
        int current = _characteristicConsecutiveCounts.TryGetValue(coinPath, out int count) ? count : 0;
        string filterDesc = GetCharacteristicFilterDescription(condition);
        return $"{current}/{condition.ConsecutiveCount} consecutive ({filterDesc})";
    }
    
    private string GetCharacteristicFilterDescription(UnlockCondition condition)
    {
        return condition.CharacteristicFilter switch
        {
            CoinCharacteristicFilter.SpecificCoins => $"{condition.RequiredCoinPaths?.Count ?? 0} specific coins",
            CoinCharacteristicFilter.UnlockConditionType => $"{condition.FilterUnlockConditionType} unlock coins",
            CoinCharacteristicFilter.EffectType => $"{condition.FilterEffectType} effect coins",
            CoinCharacteristicFilter.HasAnyEffect => "coins with any effect",
            CoinCharacteristicFilter.HasAnyUnlockCondition => "coins with unlock conditions",
            CoinCharacteristicFilter.PrerequisiteCountEquals => $"coins with {condition.FilterPrerequisiteCount} prereqs",
            CoinCharacteristicFilter.PrerequisiteCountGreaterThan => $"coins with >{condition.FilterPrerequisiteCount} prereqs",
            CoinCharacteristicFilter.PrerequisiteCountLessThan => $"coins with <{condition.FilterPrerequisiteCount} prereqs",
            _ => "matching coins"
        };
    }
    
    private string GetStreakProgress(UnlockCondition condition)
    {
        if (condition.StreakSide.HasValue)
        {
            if (condition.StreakSide.Value == Models.Unlocks.StreakSide.Heads)
                return $"{_longestHeadsStreak}/{condition.RequiredCount} heads streak";
            else
                return $"{_longestTailsStreak}/{condition.RequiredCount} tails streak";
        }
        else
        {
            return $"{_longestStreak}/{condition.RequiredCount} streak";
        }
    }
    
    private string GetLandOnCoinProgress(UnlockCondition condition)
    {
        if (string.IsNullOrEmpty(condition.RequiredCoinPath))
            return "Invalid condition";
        
        int current = GetCoinLandCount(condition.RequiredCoinPath);
        return $"{current}/{condition.RequiredCount} times";
    }
    
    private string GetLandOnMultipleCoinsProgress(UnlockCondition condition, string currentCoinPath = "")
    {
        var requiredPaths = condition.RequiredCoinPaths;
        
        // If UseDynamicCoinList is true, fetch all coins dynamically
        if (condition.UseDynamicCoinList)
        {
            var allPaths = _coinService.GetAllUnlockableCoinPathsAsync(currentCoinPath).GetAwaiter().GetResult();
            requiredPaths = allPaths;
        }
        
        if (requiredPaths == null || !requiredPaths.Any())
            return "Invalid condition";
        
        int completedCoins = 0;
        foreach (var coinPath in requiredPaths)
        {
            if (_coinLandCounts.TryGetValue(coinPath, out int count) && count >= condition.RequiredCount)
            {
                completedCoins++;
            }
        }
        
        return $"{completedCoins}/{requiredPaths.Count} coins completed ({condition.RequiredCount} each)";
    }
    
    private async Task SaveProgressAsync()
    {
        try
        {
            if (_localStorage == null)
                return;
                
            var progress = new UserProgress
            {
                TotalFlips = _totalFlips,
                HeadsFlips = _headsFlips,
                TailsFlips = _tailsFlips,
                LongestStreak = _longestStreak,
                LongestHeadsStreak = _longestHeadsStreak,
                LongestTailsStreak = _longestTailsStreak,
                CoinLandCounts = _coinLandCounts,
                RandomUnlockedCoins = _randomUnlockedCoins.ToList(),
                NotificationShownFor = _notificationShownFor.ToList(),
                CoinUnlockTimestamps = _coinUnlockTimestamps,
                CharacteristicConsecutiveCounts = _characteristicConsecutiveCounts
            };
            
            await _localStorage.SetItemAsync(StorageKey, progress);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving progress: {ex.Message}");
        }
    }
    
    private async Task LoadProgressAsync()
    {
        try
        {
            if (_localStorage == null)
                return;
                
            var progress = await _localStorage.GetItemAsync<UserProgress>(StorageKey);
            
            if (progress != null)
            {
                _totalFlips = progress.TotalFlips;
                _headsFlips = progress.HeadsFlips;
                _tailsFlips = progress.TailsFlips;
                _longestStreak = progress.LongestStreak;
                _longestHeadsStreak = progress.LongestHeadsStreak;
                _longestTailsStreak = progress.LongestTailsStreak;
                _coinLandCounts = progress.CoinLandCounts ?? new Dictionary<string, int>();
                _randomUnlockedCoins = progress.RandomUnlockedCoins?.ToHashSet() ?? new HashSet<string>();
                _notificationShownFor = progress.NotificationShownFor?.ToHashSet() ?? new HashSet<string>();
                _coinUnlockTimestamps = progress.CoinUnlockTimestamps ?? new Dictionary<string, DateTime>();
                _characteristicConsecutiveCounts = progress.CharacteristicConsecutiveCounts ?? new Dictionary<string, int>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading progress: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reset all progress (for testing or user request)
    /// </summary>
    public async Task ResetProgressAsync()
    {
        _coinLandCounts.Clear();
        _randomUnlockedCoins.Clear();
        _notificationShownFor.Clear();
        _coinUnlockTimestamps.Clear();
        _characteristicConsecutiveCounts.Clear();
        _totalFlips = 0;
        _headsFlips = 0;
        _tailsFlips = 0;
        _longestStreak = 0;
        _longestHeadsStreak = 0;
        _longestTailsStreak = 0;
        await SaveProgressAsync();
    }
}
