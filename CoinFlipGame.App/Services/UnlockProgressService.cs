using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using System.Text.Json;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service to track and manage coin unlock progress
/// </summary>
public class UnlockProgressService
{
    private const string StorageKey = "coinUnlockProgress";
    private Dictionary<string, int> _coinLandCounts = new();
    private HashSet<string> _randomUnlockedCoins = new(); // Track coins unlocked via random chance
    private HashSet<string> _notificationShownFor = new(); // Track which coins we've already shown notifications for
    private int _totalFlips = 0;
    private int _headsFlips = 0;
    private int _tailsFlips = 0;
    private int _longestStreak = 0;
    private Random _random = new();
    
    public UnlockProgressService()
    {
        LoadProgress();
    }
    
    /// <summary>
    /// Track a coin landing and check for newly unlocked coins
    /// Returns list of coins that were newly unlocked as a result of this landing
    /// </summary>
    public List<CoinImage> TrackCoinLanding(string coinPath, bool isHeads, int currentStreak, List<CoinImage> allCoins)
    {
        // Track total flips
        _totalFlips++;
        
        // Track heads/tails
        if (isHeads)
            _headsFlips++;
        else
            _tailsFlips++;
        
        // Track streak
        if (currentStreak > _longestStreak)
            _longestStreak = currentStreak;
        
        // Track specific coin landings
        if (!_coinLandCounts.ContainsKey(coinPath))
            _coinLandCounts[coinPath] = 0;
        
        _coinLandCounts[coinPath]++;
        
        SaveProgress();
        
        // Check for newly unlocked coins
        return CheckNewlyUnlockedCoins(allCoins);
    }
    
    /// <summary>
    /// Check which coins have been newly unlocked based on current progress
    /// </summary>
    private List<CoinImage> CheckNewlyUnlockedCoins(List<CoinImage> allCoins)
    {
        var newlyUnlocked = new List<CoinImage>();
        
        foreach (var coin in allCoins)
        {
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
                    justUnlocked = _longestStreak == coin.UnlockCondition.RequiredCount;
                    break;
                case UnlockConditionType.LandOnCoin:
                    if (!string.IsNullOrEmpty(coin.UnlockCondition.RequiredCoinPath))
                    {
                        justUnlocked = _coinLandCounts.TryGetValue(coin.UnlockCondition.RequiredCoinPath, out int count)
                            && count == coin.UnlockCondition.RequiredCount;
                    }
                    break;
                case UnlockConditionType.LandOnMultipleCoins:
                    if (coin.UnlockCondition.RequiredCoinPaths != null)
                    {
                        // Check if all required coins just reached the required count
                        justUnlocked = coin.UnlockCondition.RequiredCoinPaths.Any(path =>
                            _coinLandCounts.TryGetValue(path, out int count) && count == coin.UnlockCondition.RequiredCount);
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
            SaveProgress();
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
            UnlockConditionType.Streak => _longestStreak >= condition.RequiredCount,
            UnlockConditionType.LandOnCoin => CheckLandOnCoinCondition(condition),
            UnlockConditionType.RandomChance => _randomUnlockedCoins.Contains(coinPath),
            UnlockConditionType.LandOnMultipleCoins => CheckLandOnMultipleCoinsCondition(condition),
            _ => false
        };
    }
    
    /// <summary>
    /// Try to randomly unlock coins when using a specific coin
    /// Returns list of coins that were newly unlocked
    /// </summary>
    public List<CoinImage> TryRandomUnlocks(string usedCoinPath, List<CoinImage> allCoins, double unlockChanceMultiplier = 1.0)
    {
        var newlyUnlocked = new List<CoinImage>();
        
        foreach (var coin in allCoins)
        {
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
                        if (!IsConditionMet(prerequisite, coin.Path))
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
            SaveProgress();
        }
        
        return newlyUnlocked;
    }
    
    /// <summary>
    /// Manually unlock a coin (for testing or special events)
    /// </summary>
    public void UnlockCoin(string coinPath)
    {
        if (!_randomUnlockedCoins.Contains(coinPath))
        {
            _randomUnlockedCoins.Add(coinPath);
            SaveProgress();
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
            UnlockConditionType.Streak => $"{_longestStreak}/{coin.UnlockCondition.RequiredCount} streak",
            UnlockConditionType.LandOnCoin => GetLandOnCoinProgress(coin.UnlockCondition),
            UnlockConditionType.RandomChance => $"Random unlock ({coin.UnlockCondition.UnlockChance * 100:F3}% chance)",
            UnlockConditionType.LandOnMultipleCoins => GetLandOnMultipleCoinsProgress(coin.UnlockCondition),
            _ => "Locked"
        };
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
    
    private void SaveProgress()
    {
        try
        {
            var data = new
            {
                TotalFlips = _totalFlips,
                HeadsFlips = _headsFlips,
                TailsFlips = _tailsFlips,
                LongestStreak = _longestStreak,
                CoinLandCounts = _coinLandCounts,
                RandomUnlockedCoins = _randomUnlockedCoins.ToList(),
                NotificationShownFor = _notificationShownFor.ToList()
            };
            
            var json = JsonSerializer.Serialize(data);
            // Note: In WebAssembly, we'd use localStorage via JSInterop
            // For now, this is just in-memory during the session
        }
        catch
        {
            // Silent fail for now
        }
    }
    
    private void LoadProgress()
    {
        try
        {
            // Note: In WebAssembly, we'd load from localStorage via JSInterop
            // For now, start fresh each session
        }
        catch
        {
            // Silent fail - start with defaults
        }
    }
    
    /// <summary>
    /// Reset all progress (for testing or user request)
    /// </summary>
    public void ResetProgress()
    {
        _coinLandCounts.Clear();
        _randomUnlockedCoins.Clear();
        _notificationShownFor.Clear();
        _totalFlips = 0;
        _headsFlips = 0;
        _tailsFlips = 0;
        _longestStreak = 0;
        SaveProgress();
    }
}
