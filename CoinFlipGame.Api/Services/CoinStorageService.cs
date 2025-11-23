using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace CoinFlipGame.Api.Services;

public class CoinStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<CoinStorageService> _logger;
    private readonly IMemoryCache _cache;
    private readonly string _containerName;
    private readonly TimeSpan _cacheExpiration;

    public CoinStorageService(
        IConfiguration configuration,
        ILogger<CoinStorageService> logger,
        IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
        
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:CoinImagesContainer"] ?? "coin-images";
        
        // Cache expiration - default to 1 hour, configurable
        var cacheExpirationMinutes = configuration.GetValue<int?>("AzureStorage:CacheExpirationMinutes") ?? 60;
        _cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Azure Storage connection string not configured");
            _blobServiceClient = null!;
        }
        else
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger.LogInformation("Azure Storage initialized for container: {ContainerName} with cache expiration: {CacheExpiration}", 
                _containerName, _cacheExpiration);
        }
    }

    /// <summary>
    /// Get the URL for a coin image (cached)
    /// </summary>
    public async Task<string?> GetCoinImageUrlAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"coin_url_{blobName}";

        // Try to get from cache first
        if (_cache.TryGetValue<string>(cacheKey, out var cachedUrl))
        {
            _logger.LogDebug("Cache hit for coin image URL: {BlobName}", blobName);
            return cachedUrl;
        }

        try
        {
            if (_blobServiceClient == null)
            {
                _logger.LogWarning("Storage client not initialized");
                return null;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                var url = blobClient.Uri.ToString();
                
                // Cache the URL
                _cache.Set(cacheKey, url, _cacheExpiration);
                _logger.LogDebug("Cached coin image URL: {BlobName}", blobName);
                
                return url;
            }

            _logger.LogWarning("Blob not found: {BlobName}", blobName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coin image URL for {BlobName}", blobName);
            return null;
        }
    }

    /// <summary>
    /// List all coin images in the container (cached)
    /// </summary>
    public async Task<List<string>> ListCoinImagesAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"coin_list_{prefix ?? "all"}";

        // Try to get from cache first
        if (_cache.TryGetValue<List<string>>(cacheKey, out var cachedList))
        {
            _logger.LogDebug("Cache hit for coin images list with prefix: {Prefix}", prefix ?? "none");
            return cachedList;
        }

        try
        {
            if (_blobServiceClient == null)
            {
                _logger.LogWarning("Storage client not initialized");
                return new List<string>();
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobNames = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken))
            {
                blobNames.Add(blobItem.Name);
            }

            // Cache the list
            _cache.Set(cacheKey, blobNames, _cacheExpiration);
            _logger.LogInformation("Found and cached {Count} blobs with prefix: {Prefix}", blobNames.Count, prefix ?? "none");
            
            return blobNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing coin images");
            return new List<string>();
        }
    }

    /// <summary>
    /// Upload a coin image to blob storage (invalidates cache)
    /// </summary>
    public async Task<string?> UploadCoinImageAsync(string blobName, Stream content, string contentType = "image/png", CancellationToken cancellationToken = default)
    {
        try
        {
            if (_blobServiceClient == null)
            {
                _logger.LogWarning("Storage client not initialized");
                return null;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);
            
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            }, cancellationToken);

            var url = blobClient.Uri.ToString();

            // Invalidate list cache entries (they might be outdated now)
            InvalidateListCache();
            
            // Cache the URL for the newly uploaded blob
            var cacheKey = $"coin_url_{blobName}";
            _cache.Set(cacheKey, url, _cacheExpiration);

            _logger.LogInformation("Uploaded and cached blob: {BlobName}", blobName);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading coin image: {BlobName}", blobName);
            return null;
        }
    }

    /// <summary>
    /// Delete a coin image from blob storage (invalidates cache)
    /// </summary>
    public async Task<bool> DeleteCoinImageAsync(string blobName, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_blobServiceClient == null)
            {
                _logger.LogWarning("Storage client not initialized");
                return false;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var result = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            
            if (result.Value)
            {
                // Invalidate cache for this blob
                var cacheKey = $"coin_url_{blobName}";
                _cache.Remove(cacheKey);
                
                // Invalidate list cache entries
                InvalidateListCache();
                
                _logger.LogInformation("Deleted blob and invalidated cache: {BlobName}", blobName);
            }
            else
            {
                _logger.LogWarning("Blob not found for deletion: {BlobName}", blobName);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting coin image: {BlobName}", blobName);
            return false;
        }
    }

    /// <summary>
    /// Check if a coin image exists (cached)
    /// </summary>
    public async Task<bool> CoinImageExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"coin_exists_{blobName}";

        // Try to get from cache first
        if (_cache.TryGetValue<bool>(cacheKey, out var cachedExists))
        {
            _logger.LogDebug("Cache hit for coin image exists check: {BlobName}", blobName);
            return cachedExists;
        }

        try
        {
            if (_blobServiceClient == null)
            {
                return false;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync(cancellationToken);
            
            // Cache the result
            _cache.Set(cacheKey, exists.Value, _cacheExpiration);
            
            return exists.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if coin image exists: {BlobName}", blobName);
            return false;
        }
    }

    /// <summary>
    /// Download a coin image as a stream (cached in memory)
    /// </summary>
    public async Task<Stream?> DownloadCoinImageAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"coin_stream_{blobName}";

        // Try to get from cache first
        if (_cache.TryGetValue<byte[]>(cacheKey, out var cachedBytes))
        {
            _logger.LogDebug("Cache hit for coin image stream: {BlobName}", blobName);
            return new MemoryStream(cachedBytes);
        }

        try
        {
            if (_blobServiceClient == null)
            {
                _logger.LogWarning("Storage client not initialized");
                return null;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                _logger.LogWarning("Blob not found: {BlobName}", blobName);
                return null;
            }

            var download = await blobClient.DownloadAsync(cancellationToken);
            
            // Read the stream into a byte array for caching
            using var memoryStream = new MemoryStream();
            await download.Value.Content.CopyToAsync(memoryStream, cancellationToken);
            var imageBytes = memoryStream.ToArray();
            
            // Cache the byte array (with shorter expiration for image data to manage memory)
            var imageCacheExpiration = TimeSpan.FromMinutes(Math.Min(_cacheExpiration.TotalMinutes, 30));
            _cache.Set(cacheKey, imageBytes, imageCacheExpiration);
            
            _logger.LogDebug("Downloaded and cached coin image: {BlobName}", blobName);
            
            // Return a new stream from the cached bytes
            return new MemoryStream(imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading coin image: {BlobName}", blobName);
            return null;
        }
    }

    /// <summary>
    /// Clear all cached coin data
    /// </summary>
    public void ClearCache()
    {
        // Note: IMemoryCache doesn't have a built-in clear all method
        // You'd need to track keys or use a different caching solution for this
        // For now, we'll log that cache should expire naturally
        _logger.LogInformation("Cache clear requested - items will expire naturally based on TTL");
    }

    /// <summary>
    /// Invalidate list cache entries (helper method)
    /// </summary>
    private void InvalidateListCache()
    {
        // Invalidate common list cache keys
        // In a production app, you might want to track all cache keys more systematically
        _cache.Remove("coin_list_all");
        _logger.LogDebug("Invalidated list cache entries");
    }
}
