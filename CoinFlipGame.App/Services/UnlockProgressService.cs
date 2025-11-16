using CoinFlipGame.App.Models;
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
    /// Track a coin landing
    /// </summary>
    public void TrackCoinLanding(string coinPath, bool isHeads, int currentStreak)
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
    }
    
    /// <summary>
    /// Check if a coin is unlocked based on its conditions
    /// </summary>
    public bool IsUnlocked(CoinImage coin)
    {
        if (coin.UnlockCondition == null)
            return true; // No condition = always unlocked
        
        return coin.UnlockCondition.Type switch
        {
            UnlockConditionType.None => true,
            UnlockConditionType.TotalFlips => _totalFlips >= coin.UnlockCondition.RequiredCount,
            UnlockConditionType.HeadsFlips => _headsFlips >= coin.UnlockCondition.RequiredCount,
            UnlockConditionType.TailsFlips => _tailsFlips >= coin.UnlockCondition.RequiredCount,
            UnlockConditionType.Streak => _longestStreak >= coin.UnlockCondition.RequiredCount,
            UnlockConditionType.LandOnCoin => CheckLandOnCoinCondition(coin.UnlockCondition),
            UnlockConditionType.RandomChance => _randomUnlockedCoins.Contains(coin.Path),
            UnlockConditionType.LandOnMultipleCoins => CheckLandOnMultipleCoinsCondition(coin.UnlockCondition),
            _ => false
        };
    }
    
    /// <summary>
    /// Try to randomly unlock coins when using a specific coin
    /// Returns list of coins that were newly unlocked
    /// </summary>
    public List<CoinImage> TryRandomUnlocks(string usedCoinPath, List<CoinImage> allCoins)
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
                // Check if the required coin is being used
                if (coin.UnlockCondition.RequiredCoinPath == usedCoinPath)
                {
                    // Roll for unlock
                    if (_random.NextDouble() <= coin.UnlockCondition.UnlockChance)
                    {
                        _randomUnlockedCoins.Add(coin.Path);
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
                RandomUnlockedCoins = _randomUnlockedCoins.ToList()
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
        _totalFlips = 0;
        _headsFlips = 0;
        _tailsFlips = 0;
        _longestStreak = 0;
        SaveProgress();
    }
}
