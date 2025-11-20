# ?? Coin Flip Game

**An addictive coin-flipping experience with 100+ unlockable coins, coin effects, streak mechanics, and super flips.**

? **[Play Now: coinflipgame.app](https://coinflipgame.app)** ?

---

## ?? Quick Start

### First Time Playing?

1. **Click or drag the coin** to flip it
2. **Hold for 750ms** for a Super Flip (3x unlock chances!)
3. **Tap the counters** at the top to customize coin faces
4. **Unlock new coins** through achievements and random drops

**Full Guide**: [Getting Started ?](docs/getting-started.md)

---

## ?? Documentation

### For Players
- **[? Getting Started](docs/getting-started.md)** - New to the game? Start here!
- **[?? Gameplay Guide](docs/gameplay-guide.md)** - Complete guide to all mechanics
- **[?? Unlock System](docs/unlock-system.md)** - How to unlock all coins + **Double-Chance** feature
- **[?? Coin Effects](docs/coin-effects.md)** - Special abilities and combos
- **[?? Data & Privacy](docs/data-privacy.md)** - How your progress is saved

### For Developers
- **[?? Technical Architecture](docs/technical-architecture.md)** - Code structure & design patterns
- **[?? Contributing Guide](CONTRIBUTING.md)** - How to contribute
- **[?? Local Development](LOCAL_DEVELOPMENT.md)** - Dev environment setup

---

## ?? Key Features

### ?? 100+ Unlockable Coins
Multiple themed collections with unique unlock requirements

### ?? Double-Chance System (NEW!)
Set **both heads AND tails** to the required coin for **2x unlock rate**:
- Dragon_Rare: 0.5% ? **1.0%** with double-chance
- **Stacks with Super Flip** for **6x total multiplier**!

### ?? Super Flip Mechanic
Hold coin for 750ms to charge:
- **3x unlock chance** for random coins
- Enhanced animation and particle effects
- **Combines with double-chance** for maximum efficiency

### ?? Coin Effects System
Special abilities that change gameplay:
- **Auto-Click**: Passive flipping (1 flip/second)
- **Weighted**: Bias coin to land DOWN on this side
- **Shaved**: Bias coin to land UP on this side
- **Combo**: Boost effects on opposite side

### ?? Progressive Unlock System
- **8 unlock condition types**
- **4 rarity tiers** (Common ? Legendary)
- **Achievement milestones** (10 flips ? 10,000 flips)
- **Streak challenges** (3-flip ? 100-flip streaks)

---

## ?? How to Play

### The Basics
1. **Flip**: Tap, click, or drag the coin
2. **Customize**: Tap heads/tails counters to select coin faces
3. **Unlock**: Complete challenges and random drops
4. **Strategize**: Use effects and double-chance for efficiency

### Pro Tips
- **Always Super Flip** for random-chance coins (3x multiplier)
- **Set both faces** to required coin for active unlocks (2x multiplier)
- **Combine both** for maximum efficiency (6x multiplier!)
- Use **coin effects** strategically to bias flips

**Full Gameplay Guide**: [docs/gameplay-guide.md](docs/gameplay-guide.md)

---

## ?? Example: Unlocking Dragon_Rare

**Goal**: Unlock Dragon_Rare (0.5% base chance, requires Dragon active)

**Optimal Strategy**:
1. Unlock Dragon first (land on all Zodiac coins 10x each)
2. Set **Heads = Dragon** ?
3. Set **Tails = Dragon** ? (enables double-chance!)
4. **Super Flip every time** (hold coin) ?

**Result**:
- Base: 0.5%
- Super Flip: 0.5% × 3 = 1.5%
- Double-Chance: 0.5% × 2 = 1.0%
- **Combined: 0.5% × 3 × 2 = 3.0%** ?

**Expected flips**: ~33 (vs 200 without strategy!)

**Learn more**: [Unlock System Guide ?](docs/unlock-system.md)

---

## ?? Rarity Tiers

| Rarity | Color | Examples | Difficulty |
|--------|-------|----------|------------|
| **Common** | White/Blue | 10Flips, Gemini, Ram | Easy milestones |
| **Uncommon** | Light Blue | 25Flips, Rooster | Moderate challenges |
| **Rare** | Purple | Dragon, 100Flips, Panda | Tough requirements |
| **Legendary** | Orange/Red | Dragon_Rare, Completionist, Headmaster | Ultimate achievements |

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
