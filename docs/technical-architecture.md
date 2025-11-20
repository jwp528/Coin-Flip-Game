# ?? Technical Architecture

Developer guide to the **Coin Flip Game** codebase architecture and design patterns.

---

## ?? Project Overview

**Stack**: .NET 8 Blazor WebAssembly + Azure Static Web Apps + Azure Functions

**Language**: C# 12.0

**Paradigm**: Component-based UI with service layer architecture

---

## ?? Solution Structure

```
Coin-Flip-Game/
??? CoinFlipGame.App/          # Blazor WASM Frontend
?   ??? Components/
?   ?   ??? Pages/
?   ?   ?   ??? Home.razor(.cs)      # Main game logic
?   ?   ??? CoinDrawer.razor          # Coin selection UI
?   ?   ??? CoinPreviewModal.razor    # Coin details modal
?   ?   ??? AboutModal.razor          # Settings & info
?   ?   ??? UnlockInfoDialog.razor    # Unlock requirements
?   ??? Models/
?   ?   ??? Coins/                    # Coin type definitions
?   ?   ?   ??? CoinType.cs           # Base class
?   ?   ?   ??? ZodiakCoinType.cs
?   ?   ?   ??? RandomCoinType.cs
?   ?   ?   ??? PowersCoinType.cs
?   ?   ?   ??? AchievementCoinType.cs
?   ?   ??? Unlocks/                  # Unlock condition system
?   ?   ?   ??? UnlockCondition.cs
?   ?   ?   ??? UnlockConditionType.cs
?   ?   ?   ??? UnlockRarity.cs
?   ?   ??? CoinEffect.cs             # Effect types & models
?   ?   ??? CoinImage.cs              # Coin metadata
?   ?   ??? GameSettings.cs           # Configuration constants
?   ??? Services/
?   ?   ??? CoinService.cs            # Coin management
?   ?   ??? UnlockProgressService.cs  # Progress tracking
?   ?   ??? UpdateService.cs          # Version checking
?   ?   ??? ApiVersionService.cs      # API client
?   ??? wwwroot/
?       ??? js/
?       ?   ??? audio.js              # Sound system
?       ?   ??? particles.js          # Visual effects
?       ?   ??? physics.js            # Coin physics
?       ??? img/coins/                # Coin assets
?
??? CoinFlipGame.Api/           # Azure Functions Backend
?   ??? Functions/
?       ??? VersionFunction.cs        # Version API endpoint
?
??? docs/                       # Documentation
    ??? README.md              # Documentation hub
    ??? getting-started.md
    ??? gameplay-guide.md
    ??? unlock-system.md
    ??? coin-effects.md
    ??? technical-architecture.md
    ??? data-privacy.md
```

---

## ?? Core Architecture

### Service Layer

#### **CoinService**
**Responsibility**: Manages coin types and coin image metadata

**Key Methods**:
```csharp
Task<Dictionary<CoinType, List<CoinImage>>> GetAllAvailableCoinsAsync()
Task<List<string>> GetAllUnlockableCoinPathsAsync(string excludePath)
```

**Design Pattern**: Repository pattern with in-memory caching

---

#### **UnlockProgressService**
**Responsibility**: Tracks player progress and evaluates unlock conditions

**Key Methods**:
```csharp
List<CoinImage> TrackCoinLanding(string coinPath, bool isHeads, int currentStreak, List<CoinImage> allCoins)
List<CoinImage> TryRandomUnlocks(string usedCoinPath, List<CoinImage> allCoins, double unlockChanceMultiplier, string? headsCoinPath, string? tailsCoinPath)
bool IsUnlocked(CoinImage coin)
string GetProgressDescription(CoinImage coin)
```

**Design Pattern**: Service pattern with LocalStorage persistence

**State Management**:
- In-memory dictionaries for fast lookups
- Async save to browser LocalStorage
- Load on service initialization

---

### Component Architecture

#### **Home.razor**
**Role**: Main game page, orchestrates all gameplay

**Responsibilities**:
- Flip mechanics (tap, drag, super flip)
- Coin face rendering
- Effect application
- Progress tracking
- UI state management

**Key Features**:
- 3D CSS animations
- Pointer event handling
- Super Flip charging system
- Auto-click timer management

---

#### **CoinDrawer.razor**
**Role**: Coin selection interface (bottom drawer)

**Features**:
- Felt-lined display case UI
- Search/filter functionality
- Lock/unlock indicators
- Effect badges
- Long-press for details

**Animation**: Slide-up with bounce effect (0.6s)

---

### Data Models

#### **CoinType** (Abstract Base Class)
```csharp
public abstract class CoinType
{
    public abstract string Name { get; set; }
    public abstract string BasePath { get; set; }
    public abstract string Category { get; set; }
    
    public abstract List<string> GetCoinFiles();
    public abstract Dictionary<string, UnlockCondition> GetUnlockConditions();
    public virtual Dictionary<string, CoinEffect> GetCoinEffects() => new();
}
```

**Subclasses**: ZodiakCoinType, RandomCoinType, PowersCoinType, AchievementCoinType

---

#### **UnlockCondition**
```csharp
public class UnlockCondition
{
    public UnlockConditionType Type { get; set; }
    public int RequiredCount { get; set; }
    public string? RequiredCoinPath { get; set; }
    public List<string>? RequiredCoinPaths { get; set; }
    public double UnlockChance { get; set; }
    public List<UnlockCondition>? Prerequisites { get; set; }
    public bool RequiresActiveCoin { get; set; }
    public StreakSide? StreakSide { get; set; }
    public bool UseDynamicCoinList { get; set; }
    public UnlockRarity Rarity { get; set; }
    public string Description { get; set; }
}
```

---

#### **CoinEffect**
```csharp
public class CoinEffect
{
    public CoinEffectType Type { get; set; }
    public string Description { get; set; }
    public int AutoClickInterval { get; set; } = 1000;
    public double BiasStrength { get; set; } = 0.1;
    public ComboType ComboType { get; set; }
    public double ComboMultiplier { get; set; }
}
```

---

## ?? Key Algorithms

### Unlock Chance Calculation

```csharp
public List<CoinImage> TryRandomUnlocks(
    string usedCoinPath, 
    List<CoinImage> allCoins, 
    double unlockChanceMultiplier, 
    string? headsCoinPath, 
    string? tailsCoinPath)
{
    // 1. Check prerequisites
    if (coin.UnlockCondition.Prerequisites != null) {
        // All must be satisfied
    }
    
    // 2. Check if should roll (for RequiresActiveCoin)
    if (coin.UnlockCondition.RequiresActiveCoin) {
        shouldRoll = coin.UnlockCondition.RequiredCoinPath == usedCoinPath;
    }
    
    // 3. Calculate effective multiplier
    double effectiveMultiplier = unlockChanceMultiplier; // Super Flip (3x)
    
    // 4. Apply double-chance bonus
    if (headsCoinPath == requiredCoinPath && 
        tailsCoinPath == requiredCoinPath) {
        effectiveMultiplier *= 2.0; // Both faces match (2x)
    }
    
    // 5. Calculate final chance
    double effectiveChance = coin.UnlockCondition.UnlockChance * effectiveMultiplier;
    
    // 6. Roll for unlock
    if (_random.NextDouble() <= effectiveChance) {
        // Unlock!
    }
}
```

**Multiplier Stacking**: Super Flip (3x) × Double-Chance (2x) = **6x total**

---

### Effect Bias Calculation

```csharp
private bool ApplyCoinEffectBias(
    double flipValue, 
    CoinEffect? headsEffect, 
    CoinEffect? tailsEffect)
{
    double headsProbability = 0.5; // Start at 50%
    
    // Apply heads effects
    if (headsEffect?.Type == Weighted)
        headsProbability -= headsEffect.BiasStrength; // Heavy lands down
    if (headsEffect?.Type == Shaved)
        headsProbability += headsEffect.BiasStrength; // Light lands up
    
    // Apply tails effects (inverse logic)
    if (tailsEffect?.Type == Weighted)
        headsProbability += tailsEffect.BiasStrength; // Tails down = heads up
    if (tailsEffect?.Type == Shaved)
        headsProbability -= tailsEffect.BiasStrength; // Tails up = heads down
    
    // Clamp to prevent guaranteed outcomes
    headsProbability = Math.Clamp(headsProbability, 0.1, 0.9);
    
    return flipValue < headsProbability;
}
```

---

## ?? State Management

### Component State
- **Home.razor.cs**: Local component state fields
- Lifecycle: `OnAfterRenderAsync` for initialization
- **StateHasChanged()**: Trigger re-renders

### Persistent State
- **Blazored.LocalStorage**: Browser LocalStorage wrapper
- **Async operations**: All storage ops are async
- **Keys**:
  - `coinUnlockProgress`: Game stats & unlocks
  - `coinSelectionPreferences`: Coin selections
  - `soundEnabled`: Audio preference
  - `hasSeenGame`: Tutorial status

---

## ?? Performance Optimizations

### Caching
- **CoinService**: Caches coin lists (avoid repeated file scans)
- **UnlockProgressService**: In-memory state for fast lookups
- **Asset preloading**: Service Worker caches images

### Rendering
- **CSS animations**: Hardware-accelerated 3D transforms
- **will-change**: Hint browser for optimizations
- **State batching**: Minimize re-renders

### Super Flip Charging
- **Reduced polling**: 200ms intervals (was 50ms) = 75% fewer state updates
- **Cancellation tokens**: Properly disposed on interaction

---

## ?? Adding New Features

### Adding a New Coin Collection

1. **Create coin type class**:
```csharp
public class NatureCoinType : CoinType
{
    public override string Name { get; set; } = "Nature";
    public override string BasePath { get; set; } = "/img/coins/AI/Nature";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new() {
        "Tree.png", "Mountain.png", "Ocean.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new() {
        { "Tree.png", new UnlockCondition { /* ... */ } }
    };
}
```

2. **Register in CoinService**:
```csharp
_coinTypes.Add(new NatureCoinType());
```

3. **Add assets**: Place PNG files in `/wwwroot/img/coins/AI/Nature/`

---

### Adding a New Effect Type

1. **Define in CoinEffect.cs**:
```csharp
public enum CoinEffectType {
    // ...existing
    YourNewEffect
}
```

2. **Implement logic in Home.razor.cs**:
```csharp
private bool ApplyCoinEffectBias(...) {
    // Add case for new effect
    if (effect?.Type == YourNewEffect) {
        // Logic here
    }
}
```

3. **Add UI indicator in CoinDrawer.razor**:
```csharp
private string GetEffectIcon(CoinEffectType effectType) {
    return effectType switch {
        // ...existing
        CoinEffectType.YourNewEffect => "??",
    };
}
```

---

## ?? Testing

### Manual Testing Checklist
- [ ] All unlock conditions trigger correctly
- [ ] Double-chance multiplier applies
- [ ] Super Flip multiplier applies
- [ ] Effects apply bias correctly
- [ ] Auto-click starts/stops properly
- [ ] Progress persists across refresh
- [ ] Animations are smooth
- [ ] No console errors

### Build Verification
```bash
dotnet build
dotnet run
```

---

## ?? Deployment

### Azure Static Web Apps
- **Frontend**: Automatic deploy from GitHub
- **CI/CD**: GitHub Actions workflow
- **Config**: `staticwebapp.config.json`

### Azure Functions API
- **Backend**: Version check endpoint
- **Runtime**: .NET 8 Isolated
- **Trigger**: HTTP (anonymous)

---

## ?? Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

**See**: [CONTRIBUTING.md](../CONTRIBUTING.md)

---

## ?? Resources

- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor)
- [Azure Static Web Apps](https://learn.microsoft.com/azure/static-web-apps)
- [CSS 3D Transforms](https://developer.mozilla.org/en-US/docs/Web/CSS/transform)

---

[? Back to Documentation Hub](README.md)

---

**Last Updated**: January 2025
