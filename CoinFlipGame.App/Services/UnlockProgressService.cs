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
    private Dictionary<string, int> _coinLandCounts = new();
    private HashSet<string> _randomUnlockedCoins = new();
    private HashSet<string> _notificationShownFor = new();
    private int _totalFlips = 0;
    private int _headsFlips = 0;
    private int _tailsFlips = 0;
    private int _longestStreak = 0;
    private int _longestHeadsStreak = 0;
    private int _longestTailsStreak = 0;
    private Random _random = new();
    private bool _isInitialized = false;
    
    public UnlockProgressService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
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
    public List<CoinImage> TrackCoinLanding(string coinPath, bool isHeads, int currentStreak, List<CoinImage> allCoins)
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
                }
                
                if (justUnlocked)
                {
                    newlyUnlocked.Add(coin);
                    _notificationShownFor.Add(coin.Path);
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
            UnlockConditionType.LandOnMultipleCoins => CheckLandOnMultipleCoinsCondition(condition),
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
    /// Try to randomly unlock coins when using a specific coin
    /// Returns list of coins that were newly unlocked
    /// </summary>
    public List<CoinImage> TryRandomUnlocks(string usedCoinPath, List<CoinImage> allCoins, double unlockChanceMultiplier = 1.0)
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
                        // Apply unlock chance multiplier for super flips
                        double effectiveChance = coin.UnlockCondition.UnlockChance * unlockChanceMultiplier;
                        
                        // Roll for unlock
                        if (_random.NextDouble() <= effectiveChance)
                        {
                            _randomUnlockedCoins.Add(coin.Path);
                            _notificationShownFor.Add(coin.Path);
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
    
    private bool CheckLandOnMultipleCoinsCondition(UnlockCondition condition)
    {
        if (condition.RequiredCoinPaths == null || !condition.RequiredCoinPaths.Any())
            return false;
        
        // Check that ALL required coins have been landed on the required number of times
        foreach (var coinPath in condition.RequiredCoinPaths)
        {
            if (!_coinLandCounts.TryGetValue(coinPath, out int count) || count < condition.RequiredCount)
            {
                return false; // This coin hasn't been landed on enough times
            }
        }
        
        return true; // All coins have been landed on enough times
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
            UnlockConditionType.LandOnMultipleCoins => GetLandOnMultipleCoinsProgress(coin.UnlockCondition),
            _ => "Locked"
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
    
    private string GetLandOnMultipleCoinsProgress(UnlockCondition condition)
    {
        if (condition.RequiredCoinPaths == null || !condition.RequiredCoinPaths.Any())
            return "Invalid condition";
        
        int completedCoins = 0;
        foreach (var coinPath in condition.RequiredCoinPaths)
        {
            if (_coinLandCounts.TryGetValue(coinPath, out int count) && count >= condition.RequiredCount)
            {
                completedCoins++;
            }
        }
        
        return $"{completedCoins}/{condition.RequiredCoinPaths.Count} coins completed ({condition.RequiredCount} each)";
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
                NotificationShownFor = _notificationShownFor.ToList()
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
        _totalFlips = 0;
        _headsFlips = 0;
        _tailsFlips = 0;
        _longestStreak = 0;
        _longestHeadsStreak = 0;
        _longestTailsStreak = 0;
        await SaveProgressAsync();
    }
}
