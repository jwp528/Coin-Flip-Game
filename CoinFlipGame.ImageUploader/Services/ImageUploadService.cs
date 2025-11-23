using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace CoinFlipGame.ImageUploader.Services;

public class ImageUploadService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<ImageUploadService> _logger;
    private readonly string _containerName;
    private readonly int _targetWidth;
    private readonly int _targetHeight;
    private readonly int _jpegQuality;

    public ImageUploadService(
        IConfiguration configuration,
        ILogger<ImageUploadService> logger)
    {
        _logger = logger;
        
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:CoinImagesContainer"] ?? "coins";
        _targetWidth = configuration.GetValue<int>("ImageSettings:TargetWidth", 400);
        _targetHeight = configuration.GetValue<int>("ImageSettings:TargetHeight", 400);
        _jpegQuality = configuration.GetValue<int>("ImageSettings:JpegQuality", 85);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Storage connection string not configured");
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger.LogInformation("Azure Storage initialized for container: {ContainerName}", _containerName);
        _logger.LogInformation("Target image size: {Width}x{Height}, JPEG Quality: {Quality}", 
            _targetWidth, _targetHeight, _jpegQuality);
    }

    /// <summary>
    /// Process and upload a coin image
    /// </summary>
    public async Task<bool> ProcessAndUploadImageAsync(string sourceFilePath, string blobPath, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing: {SourceFile}", sourceFilePath);

            // Load and process the image
            using var image = await Image.LoadAsync(sourceFilePath, cancellationToken);
            
            var originalSize = image.Size;
            _logger.LogDebug("Original size: {Width}x{Height}", originalSize.Width, originalSize.Height);

            // Resize if needed
            if (originalSize.Width > _targetWidth || originalSize.Height > _targetHeight)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(_targetWidth, _targetHeight),
                    Mode = ResizeMode.Max // Maintains aspect ratio
                }));
                
                var newSize = image.Size;
                _logger.LogDebug("Resized to: {Width}x{Height}", newSize.Width, newSize.Height);
            }

            // Upload to blob storage
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobPath);

            // Determine format and content type
            var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            string contentType;
            
            using var memoryStream = new MemoryStream();
            
            if (extension == ".png")
            {
                // Save as PNG with compression
                await image.SaveAsPngAsync(memoryStream, new PngEncoder
                {
                    CompressionLevel = PngCompressionLevel.BestCompression
                }, cancellationToken);
                contentType = "image/png";
            }
            else
            {
                // Save as JPEG with quality setting
                await image.SaveAsJpegAsync(memoryStream, new JpegEncoder
                {
                    Quality = _jpegQuality
                }, cancellationToken);
                contentType = "image/jpeg";
            }

            memoryStream.Position = 0;
            var originalFileSize = new FileInfo(sourceFilePath).Length;
            var compressedSize = memoryStream.Length;
            var compressionRatio = (1 - (double)compressedSize / originalFileSize) * 100;

            _logger.LogInformation("Compressed: {Original} KB -> {Compressed} KB ({Ratio:F1}% reduction)",
                originalFileSize / 1024, compressedSize / 1024, compressionRatio);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType,
                CacheControl = "public, max-age=31536000" // Cache for 1 year
            };

            await blobClient.UploadAsync(memoryStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            }, cancellationToken);

            _logger.LogInformation("? Uploaded: {BlobPath}", blobPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to process and upload: {SourceFile}", sourceFilePath);
            return false;
        }
    }

    /// <summary>
    /// Process and upload all images from a directory, maintaining directory structure
    /// </summary>
    public async Task<(int Success, int Failed)> ProcessDirectoryAsync(string sourceDirectory, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            _logger.LogError("Source directory not found: {Directory}", sourceDirectory);
            return (0, 0);
        }

        var imageExtensions = new[] { ".png", ".jpg", ".jpeg" };
        var files = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
            .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .ToList();

        _logger.LogInformation("Found {Count} images to process", files.Count);

        int successCount = 0;
        int failedCount = 0;

        foreach (var file in files)
        {
            // Calculate relative path from source directory
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            
            // Convert Windows path separators to forward slashes for blob storage
            var blobPath = relativePath.Replace("\\", "/");

            var success = await ProcessAndUploadImageAsync(file, blobPath, cancellationToken);
            
            if (success)
                successCount++;
            else
                failedCount++;

            // Small delay to avoid throttling
            await Task.Delay(100, cancellationToken);
        }

        return (successCount, failedCount);
    }

    /// <summary>
    /// List all blobs in the container
    /// </summary>
    public async Task<List<string>> ListAllBlobsAsync(CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobs = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
        {
            blobs.Add(blobItem.Name);
        }

        return blobs;
    }

    /// <summary>
    /// Delete all blobs in the container (use with caution!)
    /// </summary>
    public async Task<int> ClearContainerAsync(CancellationToken cancellationToken = default)
    {
        var blobs = await ListAllBlobsAsync(cancellationToken);
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        int deletedCount = 0;

        foreach (var blobName in blobs)
        {
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            deletedCount++;
            _logger.LogInformation("Deleted: {BlobName}", blobName);
        }

        return deletedCount;
    }
}
