# Coin Flip Game - Image Uploader

A console utility to compress and upload coin images to Azure Blob Storage.

## Features

- **Image Compression**: Automatically resizes images to 400x400 (configurable)
- **Smart Compression**: Uses optimal compression for PNG and JPEG formats
- **Maintains Directory Structure**: Preserves your folder organization in blob storage
- **Batch Processing**: Processes all images in the source directory
- **Progress Tracking**: Real-time feedback on compression ratios and upload status
- **Interactive Mode**: Easy-to-use menu system
- **Command Line Mode**: Automate with CLI commands

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "AzureStorage": {
    "ConnectionString": "YOUR_CONNECTION_STRING",
    "CoinImagesContainer": "coins"
  },
  "ImageSettings": {
    "TargetWidth": 400,
    "TargetHeight": 400,
    "JpegQuality": 85
  },
  "SourcePaths": {
    "CoinImagesDirectory": "../CoinFlipGame.App/wwwroot/img/coins"
  }
}
```

## Usage

### Interactive Mode (Recommended for first-time use)

```bash
dotnet run
```

Then follow the on-screen menu:
1. Upload and compress coin images
2. List uploaded blobs
3. Clear all blobs
4. Exit

### Command Line Mode

**Upload images:**
```bash
dotnet run upload
```

**List all blobs:**
```bash
dotnet run list
```

**Clear all blobs (CAUTION!):**
```bash
dotnet run clear
```

**Show help:**
```bash
dotnet run help
```

## What It Does

1. **Scans** the source directory for all `.png`, `.jpg`, and `.jpeg` files
2. **Loads** each image using ImageSharp
3. **Resizes** images larger than 400x400 (maintains aspect ratio)
4. **Compresses** images with optimal settings:
   - PNG: Best compression level
   - JPEG: 85% quality
5. **Uploads** to Azure Blob Storage with proper content types
6. **Reports** compression ratios and success/failure status

## Example Output

```
Processing: Zodiak/Gemini.png
Original size: 1024x1024
Resized to: 400x400
Compressed: 256 KB -> 45 KB (82.4% reduction)
? Uploaded: AI/Zodiak/Gemini.png
```

## Benefits

- **Cost Savings**: Smaller images = lower storage costs + faster transfers
- **Performance**: 400x400 is perfect for web display
- **Bandwidth**: Reduced image sizes mean faster page loads
- **Automation**: No manual resizing needed

## Important Notes

- Original images in your project are **not modified**
- The uploader creates optimized copies in Azure Blob Storage
- Directory structure is preserved (e.g., `AI/Zodiak/Dragon.png`)
- Images are cached with 1-year cache control headers for optimal CDN performance
