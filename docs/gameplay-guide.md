# ?? Gameplay Guide

Master all the mechanics and strategies of **Coin Flip Game**!

---

## ?? Table of Contents

1. [Flip Mechanics](#-flip-mechanics)
2. [Streak System](#-streak-system)
3. [Super Flip Mechanic](#-super-flip-mechanic)
4. [Coin Customization](#-coin-customization)
5. [Random Coin Mode](#-random-coin-mode)
6. [Achievement System](#-achievement-system)
7. [Statistics Tracking](#-statistics-tracking)
8. [Advanced Strategies](#-advanced-strategies)

---

## ?? Flip Mechanics

### Basic Flipping

The coin can be flipped in **three ways**:

#### 1. Tap/Click
- **Action**: Single click/tap on the coin
- **Animation**: Standard flip (600ms duration)
- **Best for**: Quick, casual flipping

#### 2. Drag
- **Action**: Press, drag, and release
- **Animation**: Flip speed scales with drag velocity
- **Physics**: Responds to drag direction and speed
- **Best for**: Interactive, physical feel

#### 3. Hold (Super Flip)
- **Action**: Press and hold for 750ms
- **Indicator**: Coin shakes and glows golden
- **Animation**: Extended flip (750ms) with enhanced effects
- **Bonus**: **3x unlock chance** for random coins
- **Best for**: Maximizing unlock opportunities

### Flip Physics

The coin flip uses realistic 3D CSS animations:
- **Rotation**: Multiple axis rotation (X and Y)
- **Height**: Coin rises and falls realistically
- **Spin Speed**: 1800° rotation for heads, 1980° for tails
- **Landing**: Subtle bounce effect on landing

### Flip States

| State | Duration | Description |
|-------|----------|-------------|
| **Idle** | - | Coin at rest, waiting for interaction |
| **Charging** | 0-750ms | Super Flip charge indicator |
| **Flipping** | 600-750ms | Mid-air animation |
| **Landing** | 500ms | Flash effect and particle burst |
| **Cooldown** | - | Immediate (can flip again right away) |

---

## ?? Streak System

### What is a Streak?

A **streak** occurs when you land the **same side** consecutively:
- Land heads 3 times in a row ? **3-Flip Heads Streak**
- Land tails 5 times in a row ? **5-Flip Tails Streak**

### Streak Indicator

When your streak reaches **3 or more**:
- ?? **Streak counter** appears at the top center
- Shows current streak number
- **Pulses** with fire animation
- Tracks **both heads and tails** streaks independently

### Streak Breaking

Your streak **resets to 1** when:
- You land the **opposite side**
- Example: 5 heads in a row, then 1 tails ? Streak resets

### Longest Streak Tracking

The game tracks your **longest streak ever**:
- Displayed in **coin unlock details**
- Used for **streak-based unlock conditions**
- Separate tracking for **heads streaks** and **tails streaks**

### Streak-Based Unlocks

Many coins unlock based on streak achievements:

| Coin | Requirement | Rarity |
|------|-------------|--------|
| **3 Head Streak** | 3 heads in a row | Common |
| **5 Head Streak** | 5 heads in a row | Rare |
| **10 Head Streak** | 10 heads in a row | Legendary |
| **Rooster** | 5-flip streak (any side) | Uncommon |
| **Headmaster** | 100 heads in a row | Legendary |
| **TailMaster** | 100 tails in a row | Legendary |

**Tip**: Use **coin effects** to bias flips and build streaks!

---

## ?? Super Flip Mechanic

### How to Perform

1. **Press and hold** the coin
2. Wait for **750ms** (configurable)
3. **Coin shakes** and glows golden
4. **Release** to perform Super Flip

### Super Flip Benefits

#### 1. **3x Unlock Chance**
All **random-chance unlocks** get triple the probability:
- River (2%) ? **6% chance** with Super Flip
- Lisa (0.5%) ? **1.5% chance** with Super Flip
- Panda (0.1%) ? **0.3% chance** with Super Flip

#### 2. **Enhanced Animation**
- Longer flip duration (750ms vs 600ms)
- Higher coin throw (530px peak)
- Golden glow effect
- **30 particles** vs 15 particles
- Special haptic feedback (mobile)

#### 3. **Stacks with Double-Chance**
Super Flip multiplier **stacks multiplicatively** with double-chance:
- Super Flip: **3x multiplier**
- Both faces match: **2x multiplier**
- **Combined**: **6x total multiplier**!

### Super Flip Timing

| Phase | Duration | Visual |
|-------|----------|--------|
| **Press** | 0ms | Start holding |
| **Charging** | 0-750ms | Coin vibrates slightly |
| **Ready** | 750ms | Golden glow + shake |
| **Release** | - | Super Flip triggers |

**Tip**: The coin **shakes** when fully charged - wait for the shake!

### Configuration

Developers can adjust Super Flip settings in `GameSettings.cs`:

```csharp
SUPER_FLIP_CHARGE_TIME = 750;          // Time to charge (ms)
SUPER_FLIP_UNLOCK_MULTIPLIER = 3.0;    // Unlock chance multiplier
SUPER_FLIP_ANIMATION_DURATION = 750;   // Animation length (ms)
```

---

## ?? Coin Customization

### Opening the Coin Drawer

**Method 1**: Click/tap the **Heads counter** (top left)
- Customize which coin appears on **heads face**

**Method 2**: Click/tap the **Tails counter** (top right)
- Customize which coin appears on **tails face**

### Drawer Interface

When opened, the Coin Drawer displays:

#### Header
- **"Coin Collection"** title
- **Selected side** indicator (Heads or Tails)
- **Unlock counter** (e.g., "45/100")

#### Search Bar
- **Search** by coin name or description
- **Real-time filtering** as you type
- **Clear button** (X) to reset search

#### Coin Grid
- **Felt-lined display case** aesthetic
- **Random option** at the top
- **Coins organized** by category
- **Locked coins** show as silhouettes with **?**
- **Unlocked coins** show in full color
- **Land count badges** on unlocked coins

#### Coin Details
- **Effect badges** (Auto-Click, Weighted, Shaved, Combo)
- **Rarity indicators** via colors
- **Selection checkmark** on active coin

### Selection Process

1. **Browse** through coins
2. **Tap/click** an **unlocked coin**
3. **Checkmark appears** on selected coin
4. Face **immediately updates** on main screen
5. **Drawer stays open** for more selections
6. **Tap outside** or **drag down** to close

### Long-Press for Details

**Desktop**: Hover over any coin
**Mobile**: Long-press (500ms) on any coin

**Locked Coin Info**:
- Unlock condition description
- Progress percentage
- Rarity tier

**Unlocked Coin Info**:
- Times landed count
- Effect description (if any)
- Unlock timestamp

---

## ?? Random Coin Mode

### What is Random Mode?

Instead of selecting a **specific coin**, choose **"Random"** to get:
- A **surprise coin** on every flip
- **Weighted by rarity** (common coins more frequent)
- Variety and unpredictability

### How to Enable

1. Open the **Coin Drawer**
2. Select the **"Random"** option at the top
3. The counter icon changes to **Random logo**
4. Every flip shows a **different coin** (based on rarity)

### Rarity Weighting

Random selection uses weighted probability:

| Rarity | Weight | Frequency |
|--------|--------|-----------|
| **Common** | 1.0 | ~50% of flips |
| **Uncommon** | 0.5 | ~25% of flips |
| **Rare** | 0.25 | ~15% of flips |
| **Legendary** | 0.1 | ~10% of flips |

**Example**: If you have 10 common coins and 2 legendary coins unlocked:
- Common coins appear **10x more often** than legendary

### Strategic Use

**When to use Random**:
- You want **variety** in your flips
- You have many coins unlocked
- You enjoy the **surprise factor**

**When NOT to use Random**:
- Trying to unlock coins with **RequiresActiveCoin**
- Using **coin effects** strategically
- Building **themed flip sessions**

---

## ?? Achievement System

### Achievement Types

#### 1. **Milestone Achievements**
Unlock coins by reaching flip counts:
- **10 Flips** ? 10Flips.png
- **100 Flips** ? 100Flips.png
- **1,000 Flips** ? 1000Flips.png
- **10,000 Flips** ? 10000Flips.png

#### 2. **Streak Achievements**
Land the same side repeatedly:
- **3 in a row** ? 3HeadStreak.png / 3TailStreak.png
- **10 in a row** ? 10HeadStreak.png / 10TailStreak.png
- **100 in a row** ? Headmaster.png / TailMaster.png

#### 3. **Collection Achievements**
Unlock specific sets of coins:
- **All Zodiac Coins** ? Dragon.png
- **All Coins** ? Completionist.png

#### 4. **Random Achievements**
Passive unlocks during gameplay:
- Various themed coins with % chance

### Achievement Notifications

When you unlock an achievement:
1. **Notification appears** at bottom of screen
2. Shows **coin image** and **name**
3. **Rarity-based styling** (color/animation)
4. **Click notification** to view coin details
5. **Auto-dismisses** after a few seconds

### Achievement Rarity

| Rarity | Color | Example Coins |
|--------|-------|---------------|
| **Common** | White/Blue | 10Flips, Gemini, Ram |
| **Uncommon** | Light Blue | 25Flips, 4HeadStreak |
| **Rare** | Purple | Dragon, 100Flips, Panda |
| **Legendary** | Orange/Red | Dragon_Rare, Completionist, Headmaster |

**Legendary unlocks** feature:
- **Glowing border**
- **Pulsing animation**
- **Enhanced particle effects**
- **Special sound** (if enabled)

---

## ?? Statistics Tracking

### Real-Time Stats

Displayed at the top of the screen:

#### **Heads Count** (Top Left)
- Total times landed heads
- Updates immediately after each flip
- Stored permanently (even after refresh)

#### **Tails Count** (Top Right)
- Total times landed tails
- Updates immediately after each flip
- Stored permanently

#### **Current Streak** (Top Center, when active)
- Appears at 3+ consecutive same-side landings
- Shows live count
- Resets when opposite side lands

### Hidden/Tracked Stats

Stored but not always displayed:

- **Total Flips** (Heads + Tails)
- **Longest Streak Ever** (any side)
- **Longest Heads Streak**
- **Longest Tails Streak**
- **Coin Land Counts** (per coin)
- **Unlock Timestamps** (when each coin unlocked)

### Viewing Detailed Stats

**In Coin Drawer**:
- **Land count badges** on each coin
- Shows how many times that coin landed

**In About Modal**:
- Current app version
- Total coins unlocked (e.g., "45/100")

**In Browser DevTools** (for tech-savvy users):
- Open **Application ? Local Storage**
- View `coinUnlockProgress` key
- Raw JSON data

---

## ?? Advanced Strategies

### Strategy 1: **Active Unlock Farming**

**Goal**: Unlock Dragon_Rare (0.5% chance with Dragon active)

**Method**:
1. Unlock **Dragon** first (land on all Zodiac coins 10x each)
2. Set **both Heads AND Tails** to Dragon
3. Use **Super Flip** every time (hold coin)
4. **Combined multiplier**: 0.5% × 2 × 3 = **3.0% per flip**!

**Expected**: ~33 flips on average to unlock

---

### Strategy 2: **Streak Building with Effects**

**Goal**: Unlock Headmaster (100 heads streak)

**Method**:
1. Set **Heads** to **DragonCore** (Shaved, +15% bias)
2. Set **Tails** to **Heavy** (Weighted, -20% bias towards tails up)
3. **Combined**: 65% heads, 35% tails
4. Keep flipping until 100-streak achieved

**Expected**: Much easier than pure 50/50 luck

---

### Strategy 3: **Passive Idle Farming**

**Goal**: Accumulate flips without active play

**Method**:
1. Unlock **Digital Ox** (Auto-Click effect, unlocks at 100 flips)
2. Set **either side** to Digital Ox
3. **Leave tab open** (or install as PWA)
4. Coin auto-flips **once per second** indefinitely

**Expected**: 3,600 flips per hour

---

### Strategy 4: **Completionist Speedrun**

**Goal**: Unlock all coins as fast as possible

**Method**:
1. **Focus on milestones first** (10, 25, 50, 100 flips, etc.)
2. **Use Super Flips** for random-chance coins
3. **Set both faces** to required coins for active unlocks
4. **Build streaks** with biased coin effects
5. **Auto-click** with Digital Ox for passive progress

**Expected**: Varies, but strategic play cuts time in half

---

### Strategy 5: **Rarity Hunting**

**Goal**: Unlock all Legendary coins

**Method**:
1. **Panda** (0.1%): Super Flip + Random mode
2. **Dragon_Rare** (0.5% active): Both faces Dragon + Super Flip
3. **Zen** (0.5% active): Both faces Panda + 500 flips + Super Flip
4. **Headmaster/TailMaster**: Biased effects for streaks
5. **Completionist**: Complete all other unlocks first

**Expected**: Longest grind, but most rewarding

---

## ?? Pro Tips

### General Tips
- **Super Flip EVERYTHING** for random coins (3x chance)
- **Set both faces** for active unlocks (2x chance)
- **Combine both** for maximum efficiency (6x total!)
- **Use Random mode** to discover variety early

### Effect Tips
- **Auto-Click** is best for passive play
- **Weighted** to avoid a side (heavy lands down)
- **Shaved** to favor a side (light lands up)
- **Combo** boosts other effects (pair strategically)

### Unlock Tips
- **Start with milestones** (guaranteed unlocks)
- **Farm streaks** with biased effects
- **Focus on prerequisites** before active unlocks
- **Track progress** in the Coin Drawer

### Efficiency Tips
- **Install as PWA** for offline play + auto-click
- **Keep tab active** for auto-click to work
- **Don't clear browser data** or lose progress
- **Super Flip consistently** for best results

---

## ?? Common Mistakes

### ? **Not Using Super Flips**
- **Problem**: Rare coins take forever to unlock
- **Solution**: Hold coin every flip for 3x chance

### ? **Ignoring Double-Chance**
- **Problem**: Active unlocks take twice as long
- **Solution**: Set both faces to required coin

### ? **Random Mode for Active Unlocks**
- **Problem**: Can't unlock Dragon_Rare with Random
- **Solution**: Set specific coin for active unlocks

### ? **Not Tracking Prerequisites**
- **Problem**: Waste flips on coins you can't unlock yet
- **Solution**: Check unlock conditions in Coin Drawer

### ? **Clearing Browser Data**
- **Problem**: Loses all progress
- **Solution**: Use "Reset Progress" button instead if needed

---

## ?? Next Steps

- **[Unlock System ?](unlock-system.md)** - Deep dive into unlock mechanics
- **[Coin Effects ?](coin-effects.md)** - Master the power-up system
- **[? Back to Getting Started](getting-started.md)**

---

[? Back to Documentation Hub](README.md)

---

**Last Updated**: January 2025
