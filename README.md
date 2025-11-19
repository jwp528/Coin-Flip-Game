# ?? Coin Flip Game

**An addictive coin-flipping experience with unlockable coins, streak mechanics, and super flips.**

Play now: [coinflipgame.app](https://coinflipgame.app)

---

## ?? How to Play

### The Basics
1. **Flip**: Tap, click, or drag the coin to flip it
2. **Pick Your Side**: Tap the heads/tails counters to customize which coin appears on each side
3. **Unlock Coins**: Complete challenges to unlock new coin designs
4. **Build Streaks**: Land the same side repeatedly for bonus achievements

### Coin Selection
- Choose any unlocked coin for heads and tails
- Or select **Random** to get surprise coins based on rarity
- Your custom selections are saved automatically

---

## ? Super Flip Mechanic

Hold down on the coin to charge a **Super Flip**:
- Takes 1.5 seconds to fully charge (configurable in `GameSettings.cs`)
- Coin shakes and glows when ready
- Triple unlock chance for random-chance coins
- More dramatic animation and effects

Super Flips are your best bet for unlocking rare coins faster.

**Configuration**: Modify `CoinFlipGame.App/Models/GameSettings.cs` to adjust:
- `SUPER_FLIP_CHARGE_TIME`: Charge time in milliseconds (default: 1500ms)
- `SUPER_FLIP_UNLOCK_MULTIPLIER`: Unlock chance multiplier (default: 3.0x)
- Animation durations for normal and super flips

---

## ?? Unlock System

### Achievement Coins
Complete milestones to unlock:
- **Total Flips**: 10, 25, 50, 100, 500, 1K, 1.5K, 2K, 5K, 10K
- **Streaks**: Land 3, 4, 5, 10, 50, or 100 in a row (heads or tails)
- **Completionist**: Unlock every other coin in the game

### Zodiac Coins
Random chance unlocks with prerequisites:
- Most require a certain number of heads or tails first
- Small percentage chance on each flip (higher with Super Flip)
- Some coins only unlock when specific coins are active

### Rarity Tiers
- **Common** (white): Easy to unlock, frequent drops
- **Uncommon** (blue): Moderate challenges
- **Rare** (purple): Tough streaks or high flip counts
- **Legendary** (orange/red): Ultimate achievements

---

## ?? Tech Stack

**Built with:**
- **.NET 8** Blazor WebAssembly
- **C#** for game logic and unlock system
- **CSS3** animations (3D coin physics)
- **JavaScript** for particle effects, audio, and haptics
- **Blazored.LocalStorage** for progress persistence

**Key Features:**
- Progressive Web App (install to home screen)
- Offline support via Service Worker
- Responsive design (desktop + mobile)
- Haptic feedback on supported devices

**Architecture Highlights:**
- `UnlockProgressService`: Tracks stats, checks unlock conditions
- `CoinService`: Manages coin types and metadata
- Component-based UI with real-time state management
- Modular coin type system for easy expansion

---

## ?? Project Structure

```
CoinFlipGame.App/
??? Components/
?   ??? Pages/
?   ?   ??? Home.razor(.cs)       # Main game logic
?   ??? CoinPreviewModal.razor     # 3D coin viewer
??? Models/
?   ??? Coins/                     # Coin type definitions
?   ??? Unlocks/                   # Unlock condition system
??? Services/
?   ??? CoinService.cs             # Coin management
?   ??? UnlockProgressService.cs   # Progress tracking
??? wwwroot/
    ??? js/                        # Audio, particles, physics
    ??? img/coins/                 # Coin image assets
```

---

## ?? Running Locally

```bash
# Clone the repo
git clone https://github.com/jwp528/Coin-Flip-Game.git

# Navigate to project
cd Coin-Flip-Game/CoinFlipGame.App

# Restore dependencies
dotnet restore

# Run the app
dotnet run
```

Open `https://localhost:5001` in your browser.

---

## ?? License

MIT License - See [LICENSE](LICENSE) file for details.

**Developed by [Josh Parsons](https://www.joshparsons.ca)**
