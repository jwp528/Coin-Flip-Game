# ?? Coin Effects System

Master the special abilities and power-ups that transform your coin flipping experience!

---

## ?? Table of Contents

1. [Effect Types Overview](#-effect-types-overview)
2. [Auto-Click Effect](#-auto-click-effect)
3. [Weighted Effect](#-weighted-effect)
4. [Shaved Effect](#-shaved-effect)
5. [Combo Effect](#-combo-effect)
6. [Effect Combinations](#-effect-combinations)
7. [Strategic Pairings](#-strategic-pairings)
8. [Effect Calculator](#-effect-calculator)

---

## ?? Effect Types Overview

### Quick Reference

| Effect | Icon | Type | Impact |
|--------|------|------|--------|
| **Auto-Click** | ?? | Active | Passive flipping |
| **Weighted** | ?? | Passive | Bias away from side |
| **Shaved** | ?? | Passive | Bias towards side |
| **Combo** | ?? | Passive | Boosts other effects |

### Effect Indicators

In the Coin Drawer:
- **Effect badges** appear on coins with abilities
- **Top-left corner** of coin image
- **Hover** for description (desktop)
- **Long-press** for details (mobile)

---

## ?? Auto-Click Effect

### What It Does

**Automatically flips the coin** at regular intervals without user input.

### How It Works

```csharp
// Default interval
public int AutoClickInterval { get; set; } = 1000; // 1 second

// Timer triggers flip every interval
autoClickTimer = new Timer(async _ => {
    if (!isFlipping && !isDragging && isAutoClickActive) {
        await HandleClick(); // Triggers flip
    }
}, null, intervalMs, intervalMs);
```

### Behavior

**Activation**:
- Activates **immediately** when coin is selected
- Works on **either heads OR tails** side

**Pausing**:
- **Pauses** when you manually interact
- **Resumes** after interaction ends
- Never flips during an active flip

**Requirements**:
- **Tab must be active** (browser limitation)
- For always-on behavior: **Install as PWA**

### Stats

- **Interval**: 1 flip per second (1000ms)
- **Hourly Rate**: 3,600 flips/hour
- **Daily Rate**: 86,400 flips/day (if left running)

### Coins with Auto-Click

| Coin | Unlock Requirement | Collection |
|------|-------------------|------------|
| **Digital Ox** | 100 total flips | Powers |

### Strategic Use

**Best for**:
- ?? **Passive farming** (leave running)
- ?? **Milestone unlocks** (1K, 5K, 10K flips)
- ?? **Random-chance farming** (more rolls = more chances)

**Combine with**:
- **Super Flip charge** doesn't work with auto-click
- **Other effects** work normally (Weighted, Shaved)
- **Random mode** for variety

### Example Strategy

```
Goal: Reach 10,000 flips
Method:
1. Unlock Digital Ox (100 flips)
2. Set Heads OR Tails to Digital Ox
3. Install as PWA (optional)
4. Let it run overnight
Result: ~28 hours of passive flipping
```

---

## ?? Weighted Effect

### What It Does

Makes the coin **heavy on this side**, causing it to **land DOWN more often** (opposite side shows).

### How It Works

```csharp
// Default bias strength
public double BiasStrength { get; set; } = 0.20; // 20%

// Weighted coin reduces probability of landing UP
if (headsEffect?.Type == Weighted)
    headsProbability -= headsEffect.BiasStrength; // -20%

// Example: 50% - 20% = 30% heads, 70% tails
```

### Behavior

- **Physics Concept**: Heavy side lands face-down
- **Result**: **Opposite side** shows more often
- **Magnitude**: **-20% default** bias

### Calculation

| Setup | Heads | Tails | Notes |
|-------|-------|-------|-------|
| **No effects** | 50% | 50% | Baseline |
| **Weighted on Heads** | 30% | 70% | Heads lands down |
| **Weighted on Tails** | 70% | 30% | Tails lands down |

### Coins with Weighted

| Coin | Bias Strength | Unlock Requirement |
|------|---------------|-------------------|
| **Heavy** | -20% | Default (unlocked) |

### Strategic Use

**Use when**:
- ?? You want to **avoid** a specific side
- ?? Building **opposite-side streaks**
- ?? Farming **opposite-side count**

**Example**:
- Set **Heads = Heavy** (Weighted)
- Result: **Tails appears 70%** of the time
- Perfect for **tails-based unlocks**

---

## ?? Shaved Effect

### What It Does

Makes the coin **lighter on this side**, causing it to **land UP more often**.

### How It Works

```csharp
// Default bias strength
public double BiasStrength { get; set; } = 0.15; // 15%

// Shaved coin increases probability of landing UP
if (headsEffect?.Type == Shaved)
    headsProbability += headsEffect.BiasStrength; // +15%

// Example: 50% + 15% = 65% heads, 35% tails
```

### Behavior

- **Physics Concept**: Light side lands face-up
- **Result**: **This side** shows more often
- **Magnitude**: **+15% default** bias

### Calculation

| Setup | Heads | Tails | Notes |
|-------|-------|-------|-------|
| **No effects** | 50% | 50% | Baseline |
| **Shaved on Heads** | 65% | 35% | Heads lands up |
| **Shaved on Tails** | 35% | 65% | Tails lands up |

### Coins with Shaved

| Coin | Bias Strength | Unlock Requirement |
|------|---------------|-------------------|
| **DragonCore** | +15% | Default (unlocked) |

### Strategic Use

**Use when**:
- ?? You want to **favor** a specific side
- ?? Building **same-side streaks**
- ?? Farming **same-side count**

**Example**:
- Set **Heads = DragonCore** (Shaved)
- Result: **Heads appears 65%** of the time
- Perfect for **heads-based unlocks** or **head streaks**

---

## ?? Combo Effect

### What It Does

**Boosts other effects** on the opposite side when paired together.

### How It Works

```csharp
// Combo types
public enum ComboType {
    Additive,       // Adds flat bonus
    Multiplicative  // Multiplies effect
}

// Example: Additive combo
if (oppositeEffect?.Type == CoinEffectType.Combo) {
    if (comboType == Additive)
        enhanced.BiasStrength += comboMultiplier; // +0.05
}

// Example: Multiplicative combo
if (oppositeEffect?.Type == CoinEffectType.Combo) {
    if (comboType == Multiplicative)
        enhanced.BiasStrength *= comboMultiplier; // ×1.5
}
```

### Behavior

- **Requires pairing**: Only works with **effect on opposite side**
- **Doesn't boost Combo**: Can't stack combos on combos
- **Boosts**:
  - ?? Weighted (increases bias)
  - ?? Shaved (increases bias)
  - ?? Auto-Click (decreases interval)

### Calculation Examples

#### Additive Combo

**Setup**: Heads = Shaved (+15%), Tails = Combo (Additive, +0.05)

```
Base: 50%
Shaved alone: 50% + 15% = 65%
With Combo: 50% + (15% + 5%) = 70%
```

**Result**: **70% heads, 30% tails**

#### Multiplicative Combo

**Setup**: Heads = Weighted (-20%), Tails = Combo (Multiplicative, ×1.5)

```
Base: 50%
Weighted alone: 50% - 20% = 30%
With Combo: 50% - (20% × 1.5) = 20%
```

**Result**: **20% heads, 80% tails**

### Strategic Use

**Best for**:
- ?? **Maximizing bias** (heads or tails)
- ?? **Extreme streaks** (80%+ probability)
- ?? **Speed farming** (faster auto-click)

**Note**: Currently **no coins have Combo** effect (planned for future)

---

## ?? Effect Combinations

### Two-Sided Effects

When you set effects on **both heads and tails**:

### Cancel Effects

**Setup**: Heads = Shaved (+15%), Tails = Shaved (-15% to heads)

**Result**:
```
50% + 15% - 15% = 50% heads, 50% tails
```

**Effects cancel out** ? Back to 50/50

### Stack Effects

**Setup**: Heads = Shaved (+15%), Tails = Weighted (-20% to tails up)

**Result**:
```
50% + 15% + 20% = 85% heads, 15% tails
```

**Effects stack** ? Strong bias to heads

### Neutral Combinations

**Setup**: Heads = Auto-Click, Tails = DragonCore (Shaved +15%)

**Result**:
- Auto-flips every second
- Tails has 65% probability
- Good for **passive tails farming**

---

## ?? Strategic Pairings

### Pairing 1: **Maximum Heads Bias**

**Goal**: Get heads as often as possible

**Setup**:
- **Heads** = DragonCore (Shaved, +15%)
- **Tails** = Heavy (Weighted, -20% tails)

**Calculation**:
```
50% (base) + 15% (shaved) + 20% (weighted against tails) = 85%
```

**Result**: **85% heads, 15% tails**

**Use for**:
- Headmaster unlock (100-head streak)
- Heads-based milestones
- Heads flip count farming

---

### Pairing 2: **Maximum Tails Bias**

**Goal**: Get tails as often as possible

**Setup**:
- **Heads** = Heavy (Weighted, -20% heads)
- **Tails** = DragonCore (Shaved, +15%)

**Calculation**:
```
50% (base) - 20% (weighted heads) - 15% (shaved tails) = 15%
```

**Result**: **15% heads, 85% tails**

**Use for**:
- TailMaster unlock (100-tail streak)
- Tails-based milestones
- Tails flip count farming

---

### Pairing 3: **Passive Farming**

**Goal**: Auto-flip with bias

**Setup**:
- **Heads** = Digital Ox (Auto-Click)
- **Tails** = DragonCore (Shaved, +15%)

**Result**:
- Auto-flips once per second
- Tails appears 65% of the time

**Use for**:
- Passive tails milestone farming
- Overnight progress
- Hands-free gameplay

---

### Pairing 4: **Balanced Auto-Click**

**Goal**: Auto-flip without bias

**Setup**:
- **Heads** = Digital Ox (Auto-Click)
- **Tails** = Any non-effect coin (or Heavy to cancel)

**Result**:
- Auto-flips once per second
- 50/50 heads-tails split

**Use for**:
- Total flip milestones
- Random-chance unlocks
- General progress farming

---

## ?? Effect Calculator

### Manual Calculation

```
Final Heads Probability = 
    50% 
    + (Heads Shaved Bonus) 
    - (Heads Weighted Penalty)
    + (Tails Weighted Bonus)
    - (Tails Shaved Penalty)
```

### Example Calculations

#### Example 1: Shaved vs Weighted

**Setup**: Heads = Shaved (+15%), Tails = Weighted (-20%)

```
= 50% + 15% + 20%
= 85% heads, 15% tails
```

#### Example 2: Weighted vs Shaved

**Setup**: Heads = Weighted (-20%), Tails = Shaved (+15%)

```
= 50% - 20% - 15%
= 15% heads, 85% tails
```

#### Example 3: Both Shaved

**Setup**: Heads = Shaved (+15%), Tails = Shaved (-15%)

```
= 50% + 15% - 15%
= 50% heads, 50% tails (cancel out)
```

---

## ?? Tips & Tricks

### General Tips

1. **Mix effects** for strategic advantage
2. **Auto-Click** is best for passive farming
3. **Shaved** for building streaks on favored side
4. **Weighted** for building streaks on opposite side

### Streak Building

**For 100-head streak (Headmaster)**:
- Heads = Shaved (+15%)
- Tails = Weighted (-20%)
- Result: 85% heads per flip
- Expected: ~12 flips average for 100-streak

**For 100-tail streak (TailMaster)**:
- Heads = Weighted (-20%)
- Tails = Shaved (+15%)
- Result: 85% tails per flip
- Expected: ~12 flips average for 100-streak

### Farming Efficiency

**For fastest total flips**:
- Use **Digital Ox** (Auto-Click)
- Leave running overnight
- 86,400 flips per day

**For biased farming**:
- Pair **Auto-Click** with **Shaved/Weighted**
- Passive + biased = efficient milestone grinding

---

## ?? Next Steps

- **[? Back to Gameplay Guide](gameplay-guide.md)**
- **[Unlock System ?](unlock-system.md)**

---

[? Back to Documentation Hub](README.md)

---

**Last Updated**: January 2025
