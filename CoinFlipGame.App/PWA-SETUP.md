# PWA Setup and Testing Guide

## ? What's Been Fixed

### 1. **Missing Icons** ?
- Created `/wwwroot/icons/` directory
- Added placeholder icons (all sizes from 72x72 to 512x512)
- ?? **TODO**: Replace with properly sized/optimized icons before production

### 2. **Service Worker** ?
- Development: `/service-worker.js` (basic caching)
- Production: `/service-worker.published.js` (advanced offline support)
- Cache version updated to match app version (1.4.1)

### 3. **Manifest** ?
- Updated version to 1.4.1
- Improved start_url with PWA source tracking
- All required fields properly configured

### 4. **index.html** ?
- Service worker registration ?
- Manifest link ?
- PWA meta tags ?
- Apple touch icons ?
- Install prompt handler ?

## ?? Testing PWA Installation

### On Android (Chrome/Edge)
1. Open https://your-site.com in Chrome
2. Look for "Install app" banner at bottom
3. Or tap ? menu ? "Install app" or "Add to Home screen"
4. Accept the installation prompt
5. App icon should appear on home screen

**Install Criteria:**
- ? Served over HTTPS
- ? Has valid manifest.json
- ? Has service worker
- ? Has icons (192x192 and 512x512)
- ?? User must interact with page (visit at least twice)

### On iOS (Safari)
1. Open https://your-site.com in Safari
2. Tap Share button (square with arrow)
3. Scroll down and tap "Add to Home Screen"
4. Edit name if desired
5. Tap "Add"

**Note**: iOS doesn't show automatic install prompts. Users must manually add to home screen.

### On Desktop (Chrome/Edge)
1. Open https://your-site.com
2. Look for install icon (?) in address bar
3. Or ? menu ? "Install Coin Flip Game"
4. Confirm installation
5. App opens in standalone window

## ?? Testing Offline Functionality

### Method 1: Chrome DevTools
1. Open Chrome DevTools (F12)
2. Go to "Application" tab
3. Under "Service Workers", check "Offline"
4. Refresh the page
5. App should still load and work

### Method 2: Network Tab
1. Open Chrome DevTools (F12)
2. Go to "Network" tab
3. Select "Offline" from throttling dropdown
4. Refresh the page
5. Verify app loads from cache

### Method 3: Airplane Mode
1. Install the PWA
2. Turn on airplane mode
3. Launch the PWA
4. Verify app works offline

## ?? Debugging PWA Issues

### Chrome DevTools ? Application Tab

**Manifest Section:**
- Check manifest loads correctly
- Verify all icons are accessible
- Look for warnings/errors

**Service Workers Section:**
- Verify service worker is registered
- Check status: "activated and is running"
- Use "Update" to force update
- Use "Unregister" to reset

**Cache Storage Section:**
- Expand `coin-flip-game-v1.4.1`
- Verify all resources are cached
- Check file sizes

**Console Errors:**
- Look for service worker errors
- Check for failed resource loads
- Verify no manifest errors

## ?? Production Deployment Checklist

### Before Deploying:
1. ? HTTPS is enabled (required for PWA)
2. ?? Replace placeholder icons with properly sized versions
3. ? Test on multiple devices (Android, iOS, Desktop)
4. ? Verify offline mode works
5. ? Test install prompt appears
6. ? Check all meta tags are correct
7. ? Update cache version in service workers
8. ?? Create proper favicon.ico (multi-size)

### After Deploying:
1. Clear browser cache completely
2. Visit site and verify install prompt
3. Install and test all features
4. Go offline and verify functionality
5. Test update mechanism (deploy new version)

## ?? Why Install Prompt May Not Show

### Common Issues:
1. **Not HTTPS** - PWAs require secure connection
2. **Missing Icons** - Need 192x192 and 512x512 minimum
3. **Invalid Manifest** - Check JSON syntax
4. **No Service Worker** - Must be registered and active
5. **User Engagement** - User must visit site at least twice
6. **Already Installed** - Chrome won't show prompt again
7. **Incognito Mode** - PWA features disabled
8. **Browser Support** - Some browsers don't support PWA install

### How to Reset (for testing):
```javascript
// In Chrome DevTools Console:
// 1. Unregister service worker
navigator.serviceWorker.getRegistrations().then(regs => regs.forEach(reg => reg.unregister()));

// 2. Clear site data
// DevTools ? Application ? Storage ? Clear site data

// 3. Close and reopen browser
// 4. Visit site again (may need 2 visits)
```

## ??? Useful Tools

### Testing:
- **Lighthouse** (Chrome DevTools ? Lighthouse tab)
  - Run PWA audit
  - Check installability
  - Verify offline support
  
- **PWA Builder** - https://www.pwabuilder.com/
  - Test your PWA
  - Generate missing assets
  
- **Maskable.app** - https://maskable.app/
  - Test Android adaptive icons

### Generating Icons:
- **RealFaviconGenerator** - https://realfavicongenerator.net/
- **PWA Builder Image Generator** - https://www.pwabuilder.com/imageGenerator
- **pwa-asset-generator** (npm) - `npm install -g pwa-asset-generator`

## ?? Current PWA Score Checklist

- ? Registers a service worker
- ? Responds with 200 when offline
- ? Has a manifest file
- ? Has a name in manifest
- ? Has a short_name in manifest
- ? Has a start_url in manifest
- ? Has a display mode (standalone)
- ? Has icons (192x192 and 512x512)
- ? Has a theme_color
- ? Has a background_color
- ?? Icons need proper optimization
- ?? Maskable icons recommended for Android

## ?? Update Strategy

When deploying a new version:
1. Update `AppVersion.cs` version number
2. Update `manifest.json` version
3. Update `service-worker.published.js` CACHE_NAME
4. Service worker will detect new version
5. Prompts user to reload for update
6. Old cache is cleared automatically

## ?? Known Limitations

### iOS Safari:
- No automatic install prompt
- Must manually "Add to Home Screen"
- Some PWA features limited
- Push notifications not supported

### Desktop:
- Install icon may not appear on first visit
- User must interact with site first
- Some users may not notice install option

### General:
- Requires HTTPS (localhost exempted for dev)
- Users must visit site at least twice
- Some older browsers don't support PWA
