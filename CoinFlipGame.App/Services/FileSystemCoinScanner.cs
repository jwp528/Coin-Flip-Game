using CoinFlipGame.App.Models;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service to scan the filesystem for coin images that aren't mapped in CoinTypes
/// </summary>
public class FileSystemCoinScanner
{
    private readonly IJSRuntime _jsRuntime;
    private const string COINS_BASE_PATH = "/img/coins";
    
    public FileSystemCoinScanner(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    /// <summary>
    /// Scan the coins directory and find all PNG files that aren't registered in any CoinType
    /// </summary>
    public async Task<List<CoinImage>> GetUnmappedCoinsAsync(Dictionary<CoinType, List<CoinImage>> registeredCoins)
    {
        try
        {
            // Get all registered coin paths for comparison (exact file paths)
            var registeredPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var coinTypeGroup in registeredCoins.Values)
            {
                foreach (var coin in coinTypeGroup)
                {
                    // Normalize the path for comparison
                    registeredPaths.Add(coin.Path.Replace("\\", "/").TrimStart('/'));
                }
            }
            
            // Call JavaScript to scan the filesystem
            var allCoinPaths = await _jsRuntime.InvokeAsync<string[]>("scanCoinDirectory");
            
            // Find unmapped coins (files that exist but aren't in any CoinType)
            var unmappedCoins = new List<CoinImage>();
            
            foreach (var path in allCoinPaths)
            {
                // Normalize path for comparison
                var normalizedPath = path.Replace("\\", "/").TrimStart('/');
                
                // Skip if this exact file path is already registered
                if (registeredPaths.Contains(normalizedPath))
                    continue;
                
                // Skip special files
                var fileName = Path.GetFileName(path);
                if (fileName.Equals("Random.png", StringComparison.OrdinalIgnoreCase) ||
                    fileName.Equals("logo.png", StringComparison.OrdinalIgnoreCase))
                    continue;
                
                // Create unmapped coin image
                unmappedCoins.Add(new CoinImage
                {
                    Name = fileName,
                    Path = path,
                    Type = new UnmappedCoinType { FilePath = path }
                });
            }
            
            return unmappedCoins;
        }
        catch
        {
            // If scanning fails, return empty list
            return new List<CoinImage>();
        }
    }
}

/// <summary>
/// Special coin type for unmapped/unregistered coins
/// </summary>
public class UnmappedCoinType : CoinType
{
    public string FilePath { get; set; } = "";
    
    public override string Name { get; set; } = "Unmapped";
    public override string Category { get; set; } = "Unmapped";
    public override string BasePath { get; set; } = "/img/coins";
}
