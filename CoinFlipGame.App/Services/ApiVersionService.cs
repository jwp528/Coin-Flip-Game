using System.Text.Json;
using System.Text.Json.Serialization;
using CoinFlipGame.Shared;

namespace CoinFlipGame.App.Services;

/// <summary>
/// Service for communicating with the Azure Function API to check version information
/// </summary>
public class ApiVersionService
{
    private readonly HttpClient _httpClient;
    private const string VERSION_ENDPOINT = "/api/version";

    public ApiVersionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetch the current version from the API
    /// </summary>
    /// <returns>The API version response or null if the request fails</returns>
    public async Task<ApiVersionResponse?> GetVersionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(VERSION_ENDPOINT);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var versionInfo = JsonSerializer.Deserialize<ApiVersionResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return versionInfo;
        }
        catch
        {
            // Return null on any error (network, parsing, etc.)
            return null;
        }
    }

    /// <summary>
    /// Check if an update is available by comparing the API version with the client version
    /// </summary>
    /// <returns>True if update is available, false otherwise</returns>
    public async Task<bool> IsUpdateAvailableAsync()
    {
        var apiVersion = await GetVersionAsync();
        
        if (apiVersion == null)
        {
            // If API is unavailable, assume no update
            Console.WriteLine("API version check: API unavailable");
            return false;
        }

        // Compare full versions (version + build time)
        var isUpdateAvailable = apiVersion.FullVersion != AppVersion.FullVersion;
        Console.WriteLine($"API version check: API={apiVersion.FullVersion}, Client={AppVersion.FullVersion}, UpdateAvailable={isUpdateAvailable}");
        return isUpdateAvailable;
    }
}

/// <summary>
/// Response from the version API endpoint
/// </summary>
public class ApiVersionResponse
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("buildTime")]
    public string BuildTime { get; set; } = string.Empty;

    [JsonPropertyName("fullVersion")]
    public string FullVersion { get; set; } = string.Empty;
}
