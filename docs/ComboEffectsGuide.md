# Combo Effects Guide

## Overview
Combo coins enhance the effect of the coin on the **opposite side**, creating powerful synergies between your heads and tails selections.

## How Combo Works

### With an Effect on the Opposite Side
Combo coins enhance **any effect type** on the opposite side:

#### Bias Effects (Weighted/Shaved)
- **Additive Combo**: Adds directly to the bias percentage
  - Example: Shaved (+15%) + DragonSamurai (Additive +3%) = **+18% bias**
  - Example: Heavy (-20%) + DragonSamurai (Additive +3%) = **-23% bias** (even heavier!)

- **Multiplicative Combo**: Multiplies the bias value
  - Example: Shaved (+15%) + Moai (Multiplicative 2x) = **+30% bias**
  - Example: Heavy (-20%) + Moai (Multiplicative 2x) = **-40% bias** (double heavy!)

#### Auto-Click Effect
- **Additive Combo**: Reduces the auto-click interval (faster clicks)
  - Formula: `reduction = comboMultiplier × 2000ms`
  - Example: Digital Ox (1000ms) + DragonSamurai (+3%) = 1000ms - 60ms = **940ms** (faster)
  - Minimum interval: 100ms

- **Multiplicative Combo**: Divides the interval (proportionally faster)
  - Formula: `newInterval = originalInterval / multiplier`
  - Example: Digital Ox (1000ms) + Moai (2x) = 1000ms / 2 = **500ms** (2x faster!)
  - Minimum interval: 100ms

### Without an Effect on the Opposite Side
When the opposite side has no effect (or only has Combo), the combo boosts your **current streak**:

- **Additive Combo**: Adds directly to streak continuation probability
  - DragonSamurai: +3% chance to continue current streak
  - Example: Base 50% ? **53%** to continue streak

- **Multiplicative Combo**: Multiplies streak continuation probability
  - Formula: `boost = 50% × (multiplier - 1.0)`
  - Moai (2x): 50% × (2.0 - 1.0) = **+50% boost** ? 100% to continue streak
  - Example: Base 50% ? up to **90%** (clamped) to continue streak

## The Two Combo Coins

### ?? DragonSamurai (Additive)
- **Unlock**: 150 flips (Rare)
- **Type**: Additive Combo (+3%)
- **Best For**: 
  - Fine-tuning existing effects
  - Consistent, predictable enhancements
  - Speeding up auto-click slightly
- **Strategy**: Pair with moderate effects you want to boost a bit

### ?? Moai (Multiplicative)
- **Unlock**: 300 flips (Rare)
- **Type**: Multiplicative Combo (2x)
- **Best For**:
  - Doubling powerful effects
  - Dramatically speeding up auto-click (2x faster!)
  - Creating extreme biases
- **Strategy**: Pair with strong effects for maximum impact

## Example Combinations

### Power Combos (with Effects)

| Heads Coin | Tails Coin | Result |
|------------|------------|--------|
| DragonCore (Shaved +15%) | DragonSamurai (+3%) | **+18% bias** for heads |
| Heavy (-20%) | Moai (2x) | **-40% bias** for heads (extreme tails favor!) |
| Digital Ox (1000ms auto) | Moai (2x) | **500ms auto-click** (2x faster!) |
| Digital Ox (1000ms auto) | DragonSamurai (+3%) | **940ms auto-click** (faster) |

### Streak Combos (no opposite effect)

| Heads Coin | Tails Coin | Streak Behavior |
|------------|------------|-----------------|
| DragonSamurai | (none) | **+3%** to continue heads streak |
| Moai | (none) | **+50%** to continue heads streak (very strong!) |
| (none) | DragonSamurai | **+3%** to continue tails streak |

### Double Combo (both sides)
If both sides have combo coins, **both try to boost the non-existent opposite effect**, resulting in double streak boosting!

- DragonSamurai + DragonSamurai = **+6%** to continue streak
- Moai + Moai = **+100%** to continue streak (nearly guaranteed continuation!)

## Strategic Tips

1. **Auto-Click Acceleration**: Pair auto-click coins with Moai for 2x speed
2. **Extreme Bias**: Pair Heavy with Moai for -40% bias (85% chance opposite side)
3. **Streak Master**: Use both sides as combo coins for maximum streak maintenance
4. **Balanced Enhancement**: Use DragonSamurai for subtle, controlled boosts
5. **Power Multiplier**: Use Moai when you want to double the effect's power

## Technical Details

### Effect Enhancement Formula

**Additive:**
```csharp
enhancedBias = originalBias + comboMultiplier
enhancedAutoClickInterval = original - (comboMultiplier × 2000)
```

**Multiplicative:**
```csharp
enhancedBias = originalBias × comboMultiplier
enhancedAutoClickInterval = original / comboMultiplier
```

**Streak Boost (Additive):**
```csharp
probability += comboMultiplier // Direct addition
```

**Streak Boost (Multiplicative):**
```csharp
probability += 0.5 × (comboMultiplier - 1.0) // Percentage of base 50%
```

### Clamping
- All probabilities clamped to [0.1, 0.9] (10%-90%)
- Auto-click intervals clamped to minimum 100ms

## Visual Indicators
- **Lightning bolt icon** (?) on combo coins
- **Yellow/orange pulsing badge** in coin drawer
- Badge shows on all combo coins regardless of unlock status

---

**Pro Tip**: Experiment with different combinations! The combo system creates endless strategic possibilities for both casual play and achievement hunting.
