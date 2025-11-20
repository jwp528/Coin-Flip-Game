# PWA Icons Setup

## Current Status
?? **TEMPORARY**: Currently using copies of logo.png for all icon sizes.

## Required Icons for PWA
The following icon sizes are required for proper PWA functionality:

### Favicon Sizes
- favicon.ico (16x16, 32x32, 48x48 multi-size)
- favicon-16x16.png
- favicon-32x32.png
- favicon.png (196x196 or 512x512)

### PWA App Icons
- icon-72x72.png
- icon-96x96.png
- icon-120x120.png (Apple)
- icon-128x128.png
- icon-144x144.png (Windows)
- icon-152x152.png (Apple)
- icon-180x180.png (Apple)
- icon-192x192.png (Android, maskable)
- icon-384x384.png
- icon-512x512.png (Android, maskable)

## How to Generate Proper Icons

### Option 1: Online Tools (Recommended)
1. Go to https://realfavicongenerator.net/ or https://www.pwabuilder.com/imageGenerator
2. Upload `/img/coins/logo.png` (minimum 512x512 recommended)
3. Download the generated icon package
4. Extract all files to `/wwwroot/icons/`

### Option 2: Using ImageMagick (Command Line)
```bash
# Install ImageMagick first
# Windows: choco install imagemagick
# Mac: brew install imagemagick

# Then generate all sizes
magick convert logo.png -resize 72x72 icon-72x72.png
magick convert logo.png -resize 96x96 icon-96x96.png
magick convert logo.png -resize 120x120 icon-120x120.png
magick convert logo.png -resize 128x128 icon-128x128.png
magick convert logo.png -resize 144x144 icon-144x144.png
magick convert logo.png -resize 152x152 icon-152x152.png
magick convert logo.png -resize 180x180 icon-180x180.png
magick convert logo.png -resize 192x192 icon-192x192.png
magick convert logo.png -resize 384x384 icon-384x384.png
magick convert logo.png -resize 512x512 icon-512x512.png
```

### Option 3: Using Node.js (pwa-asset-generator)
```bash
npm install -g pwa-asset-generator
pwa-asset-generator logo.png icons --icon-only --favicon
```

## Maskable Icons
For Android adaptive icons, consider creating maskable versions with proper safe zones:
- Keep important content in the center 80% of the image
- Background should extend to edges
- Use https://maskable.app/ to test

## Current Workaround
For now, we've copied logo.png to all required sizes. This will work but:
- ?? Icons may look pixelated at different sizes
- ?? Not optimized for file size
- ?? No maskable safe zones

**TODO**: Generate proper sized icons before production deployment.
