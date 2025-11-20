# ?? Unlock System

Complete guide to unlocking all coins in **Coin Flip Game**, including the powerful **Double-Chance Mechanic**!

---

## ?? Table of Contents

1. [Unlock Condition Types](#-unlock-condition-types)
2. [Double-Chance Mechanic](#-double-chance-mechanic-new)
3. [Super Flip Multiplier](#-super-flip-multiplier)
4. [Stacking Bonuses](#-stacking-bonuses)
5. [Rarity System](#-rarity-system)
6. [Coin Collections](#-coin-collections)
7. [Unlock Strategies](#-unlock-strategies)
8. [Prerequisites System](#-prerequisites-system)

---

## ?? Unlock Condition Types

The game features **8 different unlock condition types**:

### 1. **None** (Always Unlocked)
- **Description**: No unlock requirement
- **Examples**: JP Logo, DragonCore, Heavy
- **Strategy**: These are your starting coins

### 2. **Total Flips**
- **Description**: Unlock after X total flips (heads + tails)
- **Formula**: `totalFlips >= requiredCount`
- **Examples**:
  - Gemini: 10 flips
  - Dog: 100 flips
  - 1000Flips: 1,000 flips
- **Strategy**: Guaranteed unlock - just keep flipping!

### 3. **Heads Flips**
- **Description**: Unlock after X heads landings
- **Formula**: `headsCount >= requiredCount`
- **Examples**:
  - Ram: 10 heads
  - Dillon: 100 heads (with prerequisite)
- **Strategy**: Use Shaved effect on heads to farm faster

### 4. **Tails Flips**
- **Description**: Unlock after X tails landings
- **Formula**: `tailsCount >= requiredCount`
- **Examples**:
  - Tauros: 10 tails
  - Dillon: 100 tails (with prerequisite)
- **Strategy**: Use Weighted effect on heads to get more tails

### 5. **Streak**
- **Description**: Achieve X consecutive flips on the same side
- **Formula**: `longestStreak >= requiredCount`
- **Variants**:
  - **Any side**: Rooster (5 streak)
  - **Heads only**: 3HeadStreak, Headmaster (100!)
  - **Tails only**: 3TailStreak, TailMaster (100!)
- **Strategy**: Use coin effects to bias flips toward one side

### 6. **Land On Coin**
- **Description**: Land on a specific coin X times
- **Formula**: `coinLandCount[specificCoin] >= requiredCount`
- **Examples**:
  - Pig: Land on Ram 10 times
- **Strategy**: Set that coin as your active face and flip

### 7. **Land On Multiple Coins**
- **Description**: Land on ALL specified coins X times each
- **Formula**: All coins in list must reach required count
- **Examples**:
  - Dragon: Land on all 8 Zodiac coins 10x each
  - Completionist: Land on every coin 1x (dynamic list)
- **Strategy**: Rotate through coins systematically

### 8. **Random Chance** ??
- **Description**: X% chance to unlock on any flip
- **Formula**: `random() <= unlockChance`
- **Base Chances**:
  - River: 2.0%
  - Winter: 6.0%
  - Lisa: 0.5%
  - Panda: 0.1%
  - Dragon_Rare: 0.5% (active only)
- **Modifiers**: Super Flip (3x), Double-Chance (2x), Both (6x)!
- **Strategy**: See [Double-Chance](#-double-chance-mechanic-new) below

---

## ?? Double-Chance Mechanic (NEW!)

### What is Double-Chance?

For coins with `RequiresActiveCoin = true`, you can **double the unlock chance** by setting **both heads AND tails** to the required coin!

### How It Works

**Normal Setup**:
- **Heads**: Dragon
- **Tails**: Random or different coin
- **Unlock Chance**: 0.5% (base rate)

**Double-Chance Setup**:
- **Heads**: Dragon
- **Tails**: Dragon (SAME coin!)
- **Unlock Chance**: **1.0%** (2x multiplier!)

### Technical Details

```csharp
// Base unlock chance
double baseChance = 0.005; // 0.5%

// Check if both faces match the required coin
if (headsCoinPath == requiredCoinPath && 
    tailsCoinPath == requiredCoinPath)
{
    effectiveMultiplier *= 2.0; // Double the chance!
}

// Final chance
double effectiveChance = baseChance * effectiveMultiplier;
```

### Coins That Support Double-Chance

| Coin | Required Coin | Base Chance | With Double-Chance |
|------|---------------|-------------|---------------------|
| **Dragon_Rare** | Dragon | 0.5% | **1.0%** |
| **Zen** | Panda | 0.5% | **1.0%** |

**Note**: Only coins with `RequiresActiveCoin = true` benefit from double-chance.

### Why Use Double-Chance?

**Without Double-Chance** (1 face Dragon):
- **0.5% per flip**
- Expected unlocks: **200 flips** on average

**With Double-Chance** (both faces Dragon):
- **1.0% per flip**
- Expected unlocks: **100 flips** on average
- **Saves 100 flips!**

---

## ?? Super Flip Multiplier

### What is Super Flip?

Hold the coin for **750ms** until it glows, then release for a **Super Flip** with **3x unlock chance**!

### Super Flip Bonus

**All random-chance unlocks** get tripled:

| Coin | Base Chance | Super Flip Chance |
|------|-------------|-------------------|
| River | 2.0% | **6.0%** |
| Winter | 6.0% | **18.0%** |
| City | 5.0% | **15.0%** |
| Lisa | 0.5% | **1.5%** |
| Panda | 0.1% | **0.3%** |
| Dragon_Rare | 0.5% | **1.5%** |
| Zen | 0.5% | **1.5%** |

### How It Works

```csharp
// Super flip multiplier
double superFlipMultiplier = 3.0;

// Apply to base chance
double effectiveChance = baseChance * superFlipMultiplier;

// Example: Lisa
// Base: 0.5% ? Super Flip: 0.5% × 3 = 1.5%
```

### Configuration

Developers can adjust in `GameSettings.cs`:

```csharp
public const double SUPER_FLIP_UNLOCK_MULTIPLIER = 3.0;
```

---

## ?? Stacking Bonuses

### The Ultimate Combo: Super Flip + Double-Chance

**Both bonuses stack multiplicatively** for massive unlock rates!

### Calculation

```csharp
effectiveMultiplier = 1.0;

// Step 1: Apply Super Flip
if (isSuperFlip)
    effectiveMultiplier *= 3.0; // Now 3.0

// Step 2: Apply Double-Chance
if (bothFacesMatch)
    effectiveMultiplier *= 2.0; // Now 6.0!

// Final chance
effectiveChance = baseChance * effectiveMultiplier;
```

### Real-World Examples

#### **Dragon_Rare Unlock**

| Method | Calculation | Final Chance | Expected Flips |
|--------|-------------|--------------|----------------|
| **Normal** | 0.5% × 1 | **0.5%** | ~200 flips |
| **Super Flip Only** | 0.5% × 3 | **1.5%** | ~67 flips |
| **Double-Chance Only** | 0.5% × 2 | **1.0%** | ~100 flips |
| **BOTH COMBINED** | 0.5% × 3 × 2 | **3.0%** | **~33 flips** |

**Savings**: **167 flips saved** with optimal strategy!

#### **Panda Unlock**

| Method | Calculation | Final Chance | Expected Flips |
|--------|-------------|--------------|----------------|
| **Normal** | 0.1% × 1 | **0.1%** | ~1,000 flips |
| **Super Flip Only** | 0.1% × 3 | **0.3%** | ~333 flips |
| **Double-Chance** | N/A | N/A | Not applicable* |

*Panda doesn't have `RequiresActiveCoin`, so double-chance doesn't apply

#### **Zen Unlock**

| Method | Calculation | Final Chance | Expected Flips |
|--------|-------------|--------------|----------------|
| **Normal** | 0.5% × 1 | **0.5%** | ~200 flips |
| **Both Panda + Super** | 0.5% × 3 × 2 | **3.0%** | **~33 flips** |

### Strategy Priority

**For Maximum Efficiency**:
1. **Always use Super Flip** for random-chance coins
2. **Set both faces** to required coin (if applicable)
3. **Combine both** for 6x multiplier

---

## ?? Rarity System

### Rarity Tiers

| Rarity | Color | Unlock Difficulty | Examples |
|--------|-------|-------------------|----------|
| **Common** | White/Blue | Easy milestones | 10Flips, Gemini, Ram |
| **Uncommon** | Light Blue | Moderate challenges | 25Flips, Rooster, Rabbit |
| **Rare** | Purple | Tough requirements | 100Flips, Dragon, Panda |
| **Legendary** | Orange/Red | Ultimate achievements | Dragon_Rare, Completionist, Headmaster |

### Rarity Impact

**Visual**:
- **Notification style** (color, animation intensity)
- **Glow effects** on unlock
- **Particle count** in celebration

**Gameplay**:
- **Random selection weight** (affects Random mode)
- Common: 1.0x frequency
- Uncommon: 0.5x frequency
- Rare: 0.25x frequency
- Legendary: 0.1x frequency

---

## ?? Coin Collections

### Default Collection
- **JP Logo** (default)

### AI/Zodiac Collection
- Gemini, Rat, Ram, Dog, Tauros, Rabbit, Rooster, Pig
- **Dragon** (requires landing on all others 10x)
- **Dragon_Rare** (requires Dragon active, 0.5% chance)

### AI/Random Collection
- River, Winter, City, Chaos, Lisa, Scenery, Panda
- **Zen** (requires Panda active, 500 flips, 0.5% chance)
- **Brook** (requires Zen + Dragon_Rare unlocked)
- **Dillon** (requires 100 heads + 100 tails)

### AI/Powers Collection
- **Digital Ox** (Auto-Click effect)
- **DragonCore** (Shaved effect)
- **Heavy** (Weighted effect)

### Achievement Collection
- **Flip Milestones**: 10, 25, 50, 100, 500, 1K, 1.5K, 2K, 5K, 10K
- **Head Streaks**: 3, 4, 5, 10, 50, 100 (Headmaster)
- **Tail Streaks**: 3, 4, 5, 10, 50, 100 (TailMaster)
- **Completionist**: Unlock all other coins

---

## ?? Unlock Strategies

### Strategy 1: **Milestone Speedrun**
**Goal**: Unlock all flip-count coins quickly

**Method**:
1. Use **Digital Ox** (Auto-Click) after unlocking at 100 flips
2. Leave tab open for passive flipping
3. Progress: 10 ? 25 ? 50 ? 100 ? 500 ? 1K ? ...

**Time**: ~3 hours for 10K with Auto-Click

---

### Strategy 2: **Dragon_Rare Farming**
**Goal**: Unlock Dragon_Rare efficiently

**Prerequisites**:
1. Land on all 8 Zodiac coins 10x each ? Unlock Dragon

**Method**:
1. Set **Heads = Dragon**
2. Set **Tails = Dragon**
3. **Super Flip every time** (hold coin)
4. **Combined**: 0.5% × 2 × 3 = **3.0% per flip**

**Expected**: **~33 flips** on average

---

### Strategy 3: **Streak Master**
**Goal**: Unlock Headmaster (100 heads streak)

**Method**:
1. Set **Heads = DragonCore** (Shaved, +15% bias UP)
2. Set **Tails = Heavy** (Weighted, -20% bias UP)
3. **Combined**: 50% + 15% + 20% = **85% heads** probability
4. Keep flipping until 100-streak

**Expected**: Much more achievable than 50/50

---

### Strategy 4: **Completionist Path**
**Goal**: Unlock all coins

**Order**:
1. **Milestones** (10, 25, 50, 100 flips)
2. **Early Zodiac** (Gemini, Ram, Tauros, Rat)
3. **Random-Chance** (River, Winter, City with Super Flips)
4. **Streaks** (use biased effects)
5. **Dragon** (land on all Zodiac 10x)
6. **Dragon_Rare** (both faces + Super Flip)
7. **Panda** (0.1%, lots of Super Flips)
8. **Zen** (both faces Panda + Super Flip after 500)
9. **Late Milestones** (1K, 2K, 5K, 10K with Auto-Click)
10. **Completionist** (last unlock)

---

### Strategy 5: **Passive Farming**
**Goal**: Maximize progress with minimal effort

**Method**:
1. Unlock **Digital Ox** (100 flips)
2. Set one face to **Digital Ox**
3. **Install as PWA** (keeps tab active even when closed)
4. Let it run overnight

**Result**: 1 flip/second = 3,600 flips/hour = 86,400 flips/day

---

## ?? Prerequisites System

### What are Prerequisites?

Some coins require you to **meet other conditions first** before the main unlock condition is checked.

### Prerequisite Types

#### 1. **Simple Prerequisites**
Example: **Chaos** (0.5% chance)
- **Main**: 0.5% random chance
- **Prerequisite**: 200 total flips first

**Logic**: Can't start rolling for Chaos until 200 flips

#### 2. **Multiple Prerequisites**
Example: **Dillon**
- **Prerequisite 1**: 100 heads
- **Prerequisite 2**: 100 tails
- **Main**: Unlocks immediately when both met

#### 3. **Prerequisite Chains**
Example: **Brook** (1% chance)
- **Prerequisite 1**: Zen unlocked
- **Prerequisite 2**: Dragon_Rare unlocked
- **Main**: 1% random chance after both unlocked

**Logic**: Need Zen AND Dragon_Rare before Brook rolls

#### 4. **Active Coin Prerequisites**
Example: **Zen** (0.5% chance)
- **Prerequisite 1**: 500 total flips
- **Prerequisite 2**: Panda unlocked (landed once)
- **Main**: 0.5% chance when **Panda is active**

**Logic**: Must have Panda, 500 flips, AND Panda selected

### Checking Prerequisites

**In Coin Drawer**:
- **Locked coins** show prerequisites in description
- Example: "Must have unlocked Dragon_Rare and Zen coins"

**Progress Tracking**:
- Prerequisites checked **before** main condition
- If any prerequisite fails, unlock can't trigger
- All prerequisites must be **100% complete**

---

## ?? Unlock Calculator

### Manual Calculation

To calculate expected flips for a random-chance unlock:

```
Expected Flips = 1 / (baseChance × multiplier)

Example: Dragon_Rare with both bonuses
= 1 / (0.005 × 2 × 3)
= 1 / 0.03
= ~33.3 flips on average
```

### Luck vs Guarantee

**Important**: Random chance means:
- **Average** unlock time is calculated
- **Actual** time varies (you might get lucky or unlucky)
- **No guarantees** - 1% chance ? unlock in 100 flips

**Example**: 1% chance
- 63% chance to unlock within 100 flips
- 95% chance to unlock within 300 flips
- 99.9% chance to unlock within 700 flips

---

## ?? Troubleshooting

### "Coin Won't Unlock!"

**Check**:
1. **Prerequisites met?** (check description)
2. **Correct coin active?** (for RequiresActiveCoin)
3. **Both faces set?** (for double-chance)
4. **Using Super Flips?** (for better odds)
5. **Random chance?** (might just be unlucky)

### "Double-Chance Not Working!"

**Verify**:
1. **Both faces** set to the **exact required coin**
2. Coin has `RequiresActiveCoin = true` (check documentation)
3. Not applicable to coins without active requirement

### "Progress Not Tracking!"

**Solutions**:
1. Check browser **Local Storage** enabled
2. Not in **Incognito/Private** mode
3. Don't clear browser data
4. Refresh page if stats seem stuck

---

## ?? Next Steps

- **[Coin Effects ?](coin-effects.md)** - Master power-ups and combos
- **[? Back to Gameplay Guide](gameplay-guide.md)**

---

[? Back to Documentation Hub](README.md)

---

**Last Updated**: January 2025
