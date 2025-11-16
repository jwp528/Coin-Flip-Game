using CoinFlipGame.App.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service to manage coin types and available coin images
/// </summary>
public class CoinService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly List<CoinType> _coinTypes;
    private readonly Dictionary<string, List<CoinImage>> _coinImageCache;
    
    public CoinService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        _coinTypes = new List<CoinType>
        {
            new JpLogoCoinType(),
            new ZodiakCoinType(),
            new AchievementCoinType(),
            new RandomCoinType(),
            new LeatherCoinType()
        };
        _coinImageCache = new Dictionary<string, List<CoinImage>>();
    }
    
    /// <summary>
    /// Get all registered coin types
    /// </summary>
    public IEnumerable<CoinType> GetAllCoinTypes() => _coinTypes;
    
    /// <summary>
    /// Get coin types by category
    /// </summary>
    public IEnumerable<CoinType> GetCoinTypesByCategory(string category)
    {
        return _coinTypes.Where(ct => ct.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get all unique categories
    /// </summary>
    public IEnumerable<string> GetCategories()
    {
        return _coinTypes.Select(ct => ct.Category).Distinct().OrderBy(c => c);
    }
    
    /// <summary>
    /// Register a new coin type
    /// </summary>
    public void RegisterCoinType(CoinType coinType)
    {
        if (!_coinTypes.Any(ct => ct.Name == coinType.Name && ct.BasePath == coinType.BasePath))
        {
            _coinTypes.Add(coinType);
        }
    }
    
    /// <summary>
    /// Get available coin images for a specific coin type
    /// </summary>
    public async Task<List<CoinImage>> GetAvailableCoinsAsync(CoinType coinType)
    {
        // Check cache first
        string cacheKey = $"{coinType.Name}_{coinType.BasePath}";
        if (_coinImageCache.ContainsKey(cacheKey))
        {
            return _coinImageCache[cacheKey];
        }
        
        try
        {
            // Get files from the coin type's registry
            var files = coinType.GetCoinFiles();
            
            // Get unlock conditions from the coin type
            var unlockConditions = coinType.GetUnlockConditions();
            
            var coinImages = files
                .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .Select(fileName =>
                {
                    var path = $"{coinType.BasePath}/{fileName}";
                    var coinImage = new CoinImage
                    {
                        Name = fileName,
                        Path = path,
                        Type = coinType
                    };
                    
                    // Apply unlock condition if one exists for this coin
                    if (unlockConditions.TryGetValue(fileName, out var unlockCondition))
                    {
                        coinImage.UnlockCondition = unlockCondition;
                    }
                    
                    return coinImage;
                })
                .ToList();
            
            _coinImageCache[cacheKey] = coinImages;
            return coinImages;
        }
        catch
        {
            // If error occurs, return empty list
            var emptyList = new List<CoinImage>();
            _coinImageCache[cacheKey] = emptyList;
            return emptyList;
        }
    }
    
    /// <summary>
    /// Get all available coins across all types
    /// </summary>
    public async Task<Dictionary<CoinType, List<CoinImage>>> GetAllAvailableCoinsAsync()
    {
        var result = new Dictionary<CoinType, List<CoinImage>>();
        
        foreach (var coinType in _coinTypes)
        {
            var coins = await GetAvailableCoinsAsync(coinType);
            if (coins.Any())
            {
                result[coinType] = coins;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Clear the image cache (useful if files are added/removed)
    /// </summary>
    public void ClearCache()
    {
        _coinImageCache.Clear();
    }
    
    /// <summary>
    /// Get a specific coin type by name
    /// </summary>
    public CoinType? GetCoinTypeByName(string name)
    {
        return _coinTypes.FirstOrDefault(ct => ct.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
