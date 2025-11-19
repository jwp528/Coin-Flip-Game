# Coin Effects System Documentation

## Overview

The Coin Effects System adds special abilities to coins in the game. When a coin with an effect is selected as either heads or tails, that effect becomes active during gameplay. This system enhances the game with unique mechanics that change how coins flip and behave.

## Table of Contents

1. [Effect Types](#effect-types)
2. [Architecture](#architecture)
3. [Implementation Details](#implementation-details)
4. [How to Add New Effects](#how-to-add-new-effects)
5. [Powers Coins](#powers-coins)
6. [User Experience](#user-experience)

---

## Effect Types

### 1. Auto-Click (?)
**Description**: Automatically flips the coin at regular intervals  
**Implementation**: Uses a timer to trigger flips without user interaction  
**Default Interval**: 1000ms (1 flip per second)  

**Behavior**:
- Activates when a coin with AutoClick effect is selected
- Pauses when user manually interacts with the coin
- Resumes automatically after user interaction ends
- Does not trigger during active flips

**Example Coin**: **Digital Ox** (`Digital_Ox.png`)
- **Unlock Condition**: Flip 100 times
- **Effect**: Auto-flips once per second
- **Strategy**: Great for passive coin accumulation

### 2. Weighted (??)
**Description**: The coin is heavy on this side, making it more likely to land this side down (opposite side up)  
**Implementation**: Biases flip probability away from showing this face  
**Default Bias**: -20% (20% less likely to land this side up)

**Behavior**:
- Modifies the random flip calculation
- Makes the coin "heavier" on the side where it's selected
- Results in this side landing DOWN more often (opposite side shows)
- Example: If selected as heads, tails will land face-up more often (heads lands down)

**Example Coin**: **Heavy** (`Heavy.png`)
- **Unlock Condition**: Unlocked by default
- **Effect**: 20% bias to land this side DOWN
- **Strategy**: Select on the side you DON'T want to see face-up

### 3. Shaved (??)
**Description**: This side is shaved/lighter, making it more likely to land this side up  
**Implementation**: Biases flip probability toward showing this face  
**Default Bias**: +15% (15% more likely to land this side up)

**Behavior**:
- Modifies the random flip calculation  
- Makes the coin "lighter" on the side where it's selected
- Results in this side landing UP more often
- Example: If selected as heads, heads will appear more frequently

**Example Coin**: **DragonCore** (`DragonCore.png`)
- **Unlock Condition**: Unlocked by default
- **Effect**: 15% bias to land this side UP
- **Strategy**: Select on the side you WANT to see face-up

---

## Architecture

### Core Components

```
Models/
??? Coins/
?   ??? CoinEffect.cs           # Effect type enum and model
?   ??? CoinImage.cs            # Extended with Effect property
?   ??? PowersCoinType.cs       # Powers category coins with effects
?   ??? CoinType.cs             # Base class (unchanged)
?
Services/
??? CoinService.cs              # Modified to load and apply effects
?
Components/
??? Pages/
?   ??? Home.razor.cs           # Core game logic with effect handling
??? CoinDrawer.razor            # UI for selecting coins (with effect badges)
```

### Data Flow

```
1. CoinType Definition
   ??> PowersCoinType.GetCoinEffects()
       ??> Returns Dictionary<string, CoinEffect>

2. CoinService Loading
   ??> GetAvailableCoinsAsync()
       ??> Applies effects to CoinImage objects

3. User Selection
   ??> Home.SelectCoin()
       ??> Updates selectedHeadsImage/selectedTailsImage
       ??> Calls UpdateAutoClickState()

4. Flip Execution
   ??> Home.FlipCoin()
       ??> GetActiveCoinEffect() for both sides
       ??> ApplyCoinEffectBias() modifies probability
       ??> Random flip with bias applied

5. Auto-Click (if active)
   ??> Timer callback
       ??> HandleClick() triggers flip automatically
```

---

## Implementation Details

### CoinEffect Model

```csharp
public class CoinEffect
{
    public CoinEffectType Type { get; set; }
    public string Description { get; set; }
    public int AutoClickInterval { get; set; } = 1000;
    public double BiasStrength { get; set; } = 0.1;
}

public enum CoinEffectType
{
    None,
    AutoClick,
    Weighted,
    Shaved
}
```

### Bias Calculation

The bias system modifies the base 50/50 flip probability:

```csharp
double headsProbability = 0.5;

// Weighted on heads: heavy side lands down, opposite shows
if (headsEffect?.Type == Weighted)
    headsProbability -= headsEffect.BiasStrength;  // Decrease heads probability

// Shaved on heads: light side lands up more often
if (headsEffect?.Type == Shaved)
    headsProbability += headsEffect.BiasStrength;  // INCREASE heads probability

// Apply inverse for tails effects
if (tailsEffect?.Type == Weighted)
    headsProbability += tailsEffect.BiasStrength;  // Tails down = heads up
if (tailsEffect?.Type == Shaved)
    headsProbability -= tailsEffect.BiasStrength;  // Tails up = heads down

// Clamp between 0.1 and 0.9 to avoid guaranteed outcomes
headsProbability = Math.Clamp(headsProbability, 0.1, 0.9);

bool isHeads = randomValue < headsProbability;
```

**Example Scenarios**:
- **No effects**: 50% heads, 50% tails
- **Heavy on heads (-20%)**: 30% heads, 70% tails (heads lands down)
- **Shaved (DragonCore) on heads (+15%)**: 65% heads, 35% tails (heads lands up)
- **Heavy on heads + Shaved on tails**: 50% - 20% - 15% = 15% heads, 85% tails (both land down/up respectively)

### Auto-Click Implementation

```csharp
private void StartAutoClick(int intervalMs)
{
    autoClickTimer = new Timer(async _ =>
    {
        if (!isFlipping && !isDragging && isAutoClickActive)
        {
            await InvokeAsync(async () => await HandleClick());
        }
    }, null, intervalMs, intervalMs);
}

private void UpdateAutoClickState()
{
    var headsEffect = GetActiveCoinEffect(selectedHeadsImage);
    var tailsEffect = GetActiveCoinEffect(selectedTailsImage);
    
    bool shouldAutoClick = (headsEffect?.Type == AutoClick) || 
                           (tailsEffect?.Type == AutoClick);
    
    if (shouldAutoClick && !isAutoClickActive)
        StartAutoClick(interval);
    else if (!shouldAutoClick && isAutoClickActive)
        StopAutoClick();
}
```

**Auto-Click Behavior**:
- Activates immediately when coin is selected
- Pauses on `OnPointerDown` (user interaction)
- Resumes on `OnPointerUp` (after drag/click)
- Respects `isFlipping` and `isDragging` flags

---

## How to Add New Effects

### Step 1: Define the Effect Type

Add to `CoinEffect.cs`:

```csharp
public enum CoinEffectType
{
    None,
    AutoClick,
    Weighted,
    Shaved,
    YourNewEffect  // Add here
}
```

### Step 2: Create or Update CoinType

Example: Adding to an existing type

```csharp
public class PowersCoinType : CoinType
{
    public Dictionary<string, CoinEffect> GetCoinEffects() => new()
    {
        {
            "YourCoin.png", new CoinEffect
            {
                Type = CoinEffectType.YourNewEffect,
                Description = "Description of what it does",
                // Add custom properties as needed
            }
        }
    };
}
```

### Step 3: Implement Effect Logic

Add handling in `Home.razor.cs`:

#### For Passive Effects (like bias):
Modify `ApplyCoinEffectBias()`:

```csharp
case CoinEffectType.YourNewEffect:
    // Modify headsProbability
    headsProbability += someValue;
    break;
```

#### For Active Effects (like auto-click):
1. Add state fields
2. Create Start/Stop methods
3. Hook into `UpdateAutoClickState()` or create new update method
4. Call from appropriate lifecycle methods

### Step 4: Add UI Indicator

Update `GetEffectIcon()` in `CoinDrawer.razor`:

```csharp
private string GetEffectIcon(CoinEffectType effectType)
{
    return effectType switch
    {
        CoinEffectType.YourNewEffect => "??",  // Your emoji
        // ... existing cases
    };
}
```

### Step 5: Add Documentation

Update this file with:
- Effect description
- Implementation details
- Example coins
- Strategy tips

---

## Powers Coins

### Digital Ox
- **File**: `Digital_Ox.png`
- **Effect**: ? Auto-Click (1 flip/second)
- **Unlock**: 100 total flips
- **Rarity**: Rare
- **Strategy**: Best for idle/passive gameplay
- **Notes**: 
  - User interaction resets the timer
  - Great for unlocking random chance coins

### DragonCore
- **File**: `DragonCore.png`
- **Effect**: ?? Shaved (+15% bias to land UP)
- **Unlock**: Default (no condition)
- **Rarity**: Common
- **Strategy**: Select on side you WANT to win
- **Notes**:
  - Moderate positive bias for controlled outcomes
  - Good for favoring a specific side

### Heavy
- **File**: `Heavy.png`
- **Effect**: ?? Weighted (-20% bias, lands DOWN)
- **Unlock**: Default (no condition)
- **Rarity**: Common
- **Strategy**: Select on side you DON'T want to win
- **Notes**:
  - Strong negative bias
  - Significant impact - opposite side wins more

---

## User Experience

### Visual Indicators

1. **Effect Badges**: 
   - Displayed on coins in the drawer
   - Shows emoji representing effect type
   - Positioned at top-left corner
   - Animated pulse effect

2. **Tooltips**:
   - Hover over effect badge shows description
   - Explains what the effect does

3. **Selected State**:
   - Green glow around selected coins
   - Effect badges visible on active coins
   - Clear visual feedback

### Interaction Flow

```
Player opens Coin Drawer
    ?
Sees coins with effect badges
    ?
Selects coin with effect (e.g., Digital Ox)
    ?
Effect becomes active immediately
    ?
Auto-Click: Coin starts flipping automatically
Weighted/Shaved: Next flip is biased
    ?
Player can switch coins anytime
    ?
Effect updates immediately on selection change
```

### Effect Combinations

Players can mix and match effects by selecting different coins for heads and tails:

**Example Combinations**:
- **Digital Ox (heads) + Heavy (tails)**:
  - Auto-flips with tails more likely (tails lands down = heads shows)
  - Passive gameplay with heads favored

- **DragonCore (heads) + DragonCore (tails)**:
  - Heads biased UP (+15%), Tails biased UP (-15% for heads)
  - Effects CANCEL OUT perfectly
  - Returns to 50/50 random

- **Heavy (heads) + DragonCore (tails)**:
  - Heads lands down (-20%), Tails lands up (-15% for heads)
  - Total: -35% for heads
  - Strong bias towards tails (15% heads, 85% tails)

- **Shaved (heads) + Shaved (tails)**:
  - Both sides want to land UP
  - Effects cancel: +15% and -15% = back to 50/50

---

## Technical Notes

### Performance Considerations

1. **Auto-Click Timer**:
   - Disposed properly on coin change
   - Checks `isFlipping` to avoid conflicts
   - Uses `InvokeAsync` for thread-safe state updates

2. **Effect Lookup**:
   - Effects cached in `CoinImage` objects
   - No repeated dictionary lookups during flips
   - O(1) access time via `GetActiveCoinEffect()`

3. **Bias Calculation**:
   - Simple arithmetic operations
   - Clamped to avoid edge cases
   - Maintains 50/50 baseline

### Testing Checklist

- [ ] Effect badges display correctly
- [ ] Auto-Click starts/stops appropriately
- [ ] Auto-Click pauses on user interaction
- [ ] Auto-Click resumes after interaction
- [ ] Weighted bias produces expected results
- [ ] Shaved bias produces expected results
- [ ] Effects combine correctly (both sides)
- [ ] Effects update on coin selection
- [ ] Build compiles without errors
- [ ] No console errors during gameplay

---

## Future Enhancements

Potential new effects to add:

1. **Streak Multiplier**: Increases chance of continuing current streak
2. **Reverse Flip**: Flips to opposite of what was shown
3. **Lucky Strike**: Small chance for guaranteed outcome
4. **Double Flip**: Counts as 2 flips per actual flip
5. **Slow Motion**: Longer, more dramatic flip animation
6. **Magnet**: Attracts rare coin unlocks
7. **Chameleon**: Randomly shows different coin images
8. **Echo**: Repeats last 3 flip results

---

## Troubleshooting

### Effect Not Activating
**Check**:
1. Coin is unlocked
2. Coin is selected (green glow)
3. Effect badge is visible
4. Console for JavaScript errors

### Auto-Click Not Working
**Check**:
1. Timer is not null in debugger
2. `isAutoClickActive` flag is true
3. Not currently flipping
4. Not currently dragging
5. Browser console for errors

### Bias Not Affecting Flips
**Check**:
1. `ApplyCoinEffectBias` is being called
2. Effect type is correct (Weighted/Shaved)
3. BiasStrength value is reasonable (0.1-0.3)
4. Random seed is not fixed

---

## Credits

**Implementation**: Josh Parsons  
**Game**: Coin Flip Game v1.3  
**Date**: January 2025

---

## License

This effects system is part of the Coin Flip Game project.  
See main project license for details.
