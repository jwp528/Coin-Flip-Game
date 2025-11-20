# ?? Coin Flip Game - Documentation

Welcome to the complete documentation for the **Coin Flip Game**! This guide will help you understand all aspects of the game, from basic gameplay to advanced mechanics and technical architecture.

---

## ?? Quick Links

### For Players
- **[Getting Started](getting-started.md)** - New to the game? Start here!
- **[Gameplay Guide](gameplay-guide.md)** - Complete guide to all game mechanics
- **[Unlock System](unlock-system.md)** - How to unlock all coins (including double-chance mechanics)
- **[Coin Effects](coin-effects.md)** - Special abilities and power-ups
- **[Data & Privacy](data-privacy.md)** - How your progress is saved

### For Developers
- **[Technical Architecture](technical-architecture.md)** - System design and code structure
- **[Contributing](../CONTRIBUTING.md)** - How to contribute to the project
- **[Local Development](../LOCAL_DEVELOPMENT.md)** - Setting up your dev environment

---

## ?? What is Coin Flip Game?

Coin Flip Game is an **addictive, feature-rich coin flipping experience** with:

- ?? **100+ Unlockable Coins** across multiple themed collections
- ?? **Coin Effects System** - Special abilities like Auto-Click, Weighted, and Shaved coins
- ?? **Super Flip Mechanic** - Charge up for 3x unlock chances
- ?? **Double-Chance System** - Set both faces to the required coin for 2x unlock rates
- ?? **Progressive Unlocks** - From common milestone coins to legendary rare drops
- ?? **Combo System** - Mix and match coin effects for powerful combinations
- ?? **Offline Support** - Full PWA with Service Worker caching
- ?? **Cross-Platform** - Desktop, mobile, and installable as an app

---

## ?? Documentation Structure

### **Getting Started** [(View)](getting-started.md)
Perfect for new players. Covers:
- How to play your first flip
- Understanding the UI
- Customizing your coin faces
- Basic unlock mechanics

### **Gameplay Guide** [(View)](gameplay-guide.md)
Complete gameplay mechanics including:
- Flip mechanics (tap, drag, super flip)
- Streak system
- Achievement tracking
- Coin customization

### **Unlock System** [(View)](unlock-system.md)
Everything about unlocking coins:
- Unlock condition types
- Rarity tiers
- **Double-Chance Mechanic** (set both faces for 2x rate!)
- Super Flip unlock bonuses (3x multiplier)
- Stacking bonuses (up to 6x with both!)

### **Coin Effects** [(View)](coin-effects.md)
Master the power-up system:
- Auto-Click effect (passive flipping)
- Weighted coins (bias against a side)
- Shaved coins (bias towards a side)
- Combo effects (boost other effects)
- Effect combinations and strategies

### **Data & Privacy** [(View)](data-privacy.md)
Understand how your data is handled:
- What data is stored locally
- Browser storage explained
- How to reset your progress
- Privacy guarantees

### **Technical Architecture** [(View)](technical-architecture.md)
For developers and contributors:
- Project structure
- Service architecture
- Component design patterns
- Adding new features

---

## ?? Feature Highlights

### ?? Super Flip Mechanic
Hold the coin for **750ms** to charge a Super Flip:
- **3x unlock chance** for random-chance coins
- Dramatic animation and effects
- Stacks with double-chance bonus

### ?? Double-Chance System
**NEW!** Set both heads and tails to the same required coin:
- **2x unlock multiplier** for coins with `RequiresActiveCoin = true`
- **Stacks with Super Flip** for up to **6x total multiplier**
- Example: Dragon_Rare goes from 0.5% to 3.0% with both bonuses!

### ?? Coin Effects System
Special abilities that change gameplay:
- **Auto-Click**: Coin flips automatically every second
- **Weighted**: Makes one side heavier (lands down more)
- **Shaved**: Makes one side lighter (lands up more)
- **Combo**: Boosts other effects when paired

Mix and match for powerful combinations!

---

## ?? Game Statistics

- **Total Coins**: 100+ unique designs
- **Coin Categories**: 10+ themed collections
- **Unlock Conditions**: 8 different types
- **Rarity Tiers**: 4 levels (Common ? Legendary)
- **Achievement Milestones**: 20+ challenges
- **Effect Types**: 4 unique abilities

---

## ?? Tech Stack

| Technology | Purpose |
|------------|---------|
| **.NET 8** | Backend framework |
| **Blazor WebAssembly** | Frontend framework |
| **C# 12** | Primary language |
| **CSS3** | 3D animations & styling |
| **JavaScript** | Audio, particles, physics |
| **Blazored.LocalStorage** | Progress persistence |
| **Azure Static Web Apps** | Hosting |
| **Azure Functions** | Version API |

---

## ?? Quick Navigation

| Topic | Link |
|-------|------|
| Play the Game | [coinflipgame.app](https://coinflipgame.app) |
| GitHub Repo | [jwp528/Coin-Flip-Game](https://github.com/jwp528/Coin-Flip-Game) |
| Report Issues | [GitHub Issues](https://github.com/jwp528/Coin-Flip-Game/issues) |
| Developer Website | [joshparsons.ca](https://www.joshparsons.ca) |

---

## ?? Support & Contribution

### Found a Bug?
- Check existing [GitHub Issues](https://github.com/jwp528/Coin-Flip-Game/issues)
- Open a new issue with detailed reproduction steps

### Want to Contribute?
- Read the [Contributing Guide](../CONTRIBUTING.md)
- Check out [Local Development Setup](../LOCAL_DEVELOPMENT.md)
- Review the [Technical Architecture](technical-architecture.md)

### Have Feedback?
- Open a GitHub Discussion
- Contact via the developer website

---

## ?? License

MIT License - See [LICENSE](../LICENSE) file for details.

**Developed by [Josh Parsons](https://www.joshparsons.ca)**

---

**Documentation Version**: 2.0  
**Last Updated**: January 2025  
**Game Version**: 1.3+
