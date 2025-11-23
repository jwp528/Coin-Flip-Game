# Quick Start Guide

## First Time Setup

1. **Navigate to the uploader project:**
   ```bash
   cd CoinFlipGame.ImageUploader
   ```

2. **Verify your settings in `appsettings.json`:**
   - Azure connection string ? (already configured)
   - Container name: `coins` ?
   - Source directory: `../CoinFlipGame.App/wwwroot/img/coins` ?
   - Target size: 400x400 ?

3. **Run the uploader:**
   ```bash
   dotnet run
   ```

4. **Choose option 1** to upload and compress images

5. **Confirm** when prompted - sit back and watch the magic happen!

## What Happens

Before:
```
??? Dragon.png - 1024x1024 - 256 KB
```

After:
```
??? Dragon.png - 400x400 - 45 KB (82% smaller!)
```

## Next Steps

After uploading, you can:
- Use option 2 to see all uploaded files
- Integrate blob URLs into your API
- Update your Blazor app to load from Azure Storage instead of wwwroot

## Tips

- **Cancel anytime** with Ctrl+C
- **Tweak image size** in appsettings.json if 400x400 isn't right
- **Adjust quality** (1-100) for JPEG compression vs file size trade-offs
- The tool is **safe** - it never modifies your original files

## Estimated Time

- ~50 images: 30-60 seconds
- Shows real-time progress for each image
