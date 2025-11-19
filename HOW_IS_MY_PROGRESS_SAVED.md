# How Is My Progress Saved?

This document explains what data the Coin Flip Game stores and how your progress is saved locally in your browser.

## Data Storage Overview

The Coin Flip Game is a client-side web application that runs entirely in your browser. All progress data is stored locally on your device using your browser's **Local Storage** mechanism. No data is transmitted to external servers or third parties.

## What Data Is Stored?

The following information is saved to preserve your gameplay progress:

### 1. **Game Statistics**
- **Total flips**: The cumulative number of coin flips you've performed
- **Heads count**: Number of times the coin landed on heads
- **Tails count**: Number of times the coin landed on tails
- **Longest streak**: Your highest consecutive streak (same result)
- **Longest heads streak**: Your highest consecutive heads streak
- **Longest tails streak**: Your highest consecutive tails streak

### 2. **Coin Unlock Progress**
- **Unlocked coins**: List of coins you've unlocked through gameplay
- **Coin landing counts**: Number of times each coin has been landed on
- **Notification history**: Tracks which unlock notifications you've already seen

### 3. **User Preferences**
- **Sound settings**: Whether sound effects are enabled or disabled
- **Coin selections**: Your chosen heads and tails coin preferences
- **Random mode**: Whether random coin selection is enabled for heads or tails
- **First-time status**: Whether you've seen the initial tutorial hint

### 4. **Referral Status**
- **Referral bonus applied**: Tracks if you've received a referral bonus (if applicable)

## How Is Data Stored?

All data is stored in your browser's **Local Storage** under the following keys:

| Storage Key | Purpose |
|------------|---------|
| `coinUnlockProgress` | Contains all game statistics and unlock progress |
| `coinSelectionPreferences` | Stores your coin customization preferences |
| `soundEnabled` | Tracks your sound preference setting |
| `hasSeenGame` | Records if you've completed the first-time tutorial |
| `referrerBonusApplied` | Tracks referral bonus status |

## Data Security & Privacy

### Local Storage Only
- **No server communication**: Your data never leaves your device
- **No tracking**: We do not track, collect, or analyze user behavior
- **No personal information**: No email addresses, names, or identifying information is stored

### Browser-Specific Storage
Your progress is tied to the specific browser and device you're using. This means:
- Progress is **not synchronized** across different browsers or devices
- Clearing your browser's local storage will **permanently delete** your progress
- Incognito/private browsing mode will **not save** any progress

## Managing Your Data

### Viewing Stored Data
You can inspect your stored data using your browser's Developer Tools:
1. Press `F12` (or `Cmd+Option+I` on Mac)
2. Navigate to the **Application** or **Storage** tab
3. Expand **Local Storage** and select the game's domain
4. View all stored key-value pairs

### Resetting Your Progress
To delete all saved progress:
1. Open the **About** modal in the game (click the info icon)
2. Navigate to the **Danger Zone** section
3. Click **Reset Progress** and confirm the action

**Warning**: This action is permanent and cannot be undone.

### Manual Data Deletion
You can also manually clear data through your browser:
- **Chrome/Edge**: Settings ? Privacy ? Clear browsing data ? Cookies and site data
- **Firefox**: Settings ? Privacy & Security ? Cookies and Site Data ? Clear Data
- **Safari**: Preferences ? Privacy ? Manage Website Data ? Remove All

## Update Notes

When the app receives updates:
- Your existing progress is **preserved** and automatically migrated
- New features may add additional data storage
- You can check for updates using the **Check for Updates** button in the About modal

## Questions or Concerns?

If you have questions about data storage or privacy, please:
- Review the [project repository](https://github.com/jwp528/Coin-Flip-Game)
- Open an issue on GitHub for technical questions
- Contact the developer through the links provided in the app

---

**Last Updated**: November 2024  
**Version**: 1.0
