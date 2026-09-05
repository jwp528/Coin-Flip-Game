# Coin Flip Game

**An addictive coin-flipping experience with 100+ unlockable coins, coin effects, streak mechanics, and super flips.**

**[Play Now @: coin.joshparsons.ca](https://coin.joshparsons.ca)**

---

## Quick Start

### First Time Playing?

1. **Click or drag the coin** to flip it
2. **Hold for 750ms** for a Super Flip (3x unlock chances!)
3. **Tap the counters** at the top to customize coin faces
4. **Unlock new coins** through achievements and random drops

**Full Guide**: [Getting Started? ](docs/getting-started.md)

---

## Documentation

### For Players
- **[Getting Started](docs/getting-started.md)** - New to the game? Start here!
- **[Gameplay Guide](docs/gameplay-guide.md)** - Complete guide to all mechanics
- **[Unlock System](docs/unlock-system.md)** - How to unlock all coins + **Double-Chance** feature
- **[Coin Effects](docs/coin-effects.md)** - Special abilities and combos
- **[Data & Privacy](docs/data-privacy.md)** - How your progress is saved

### For Developers
- **[Technical Architecture](docs/technical-architecture.md)** - Code structure & design patterns
- **[Contributing Guide](CONTRIBUTING.md)** - How to contribute
- **[Local Development](LOCAL_DEVELOPMENT.md)** - Dev environment setup

---

## Key Features

### 100+ Unlockable Coins
Multiple themed collections with unique unlock requirements

### Double-Chance System (NEW!)
Set **both heads AND tails** to the required coin for **2x unlock rate**:
- Dragon_Rare: 0.5% **1.0%** with double-chance
- **Stacks with Super Flip** for **6x total multiplier**!

### Super Flip Mechanic
Hold coin for 750ms to charge:
- **3x unlock chance** for random coins
- Enhanced animation and particle effects
- **Combines with double-chance** for maximum efficiency

### Coin Effects System
Special abilities that change gameplay:
- **Auto-Click**: Passive flipping (1 flip/second)
- **Weighted**: Bias coin to land DOWN on this side
- **Shaved**: Bias coin to land UP on this side
- **Combo**: Boost effects on opposite side

### Progressive Unlock System
- **8 unlock condition types**
- **4 rarity tiers** (Common Legendary)
- **Achievement milestones** (10 flips - 10,000 flips)
- **Streak challenges** (3-flip - 100-flip streaks)

---

## How to Play

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

## Example: Unlocking Dragon_Rare

**Goal**: Unlock Dragon_Rare (0.5% base chance, requires Dragon active)

**Optimal Strategy**:
1. Unlock Dragon first (land on all Zodiac coins 10x each)
2. Set **Heads = Dragon**
3. Set **Tails = Dragon** (enables double-chance!)
4. **Super Flip every time** (hold coin)

**Result**:
- Base: 0.5%
- Super Flip: 0.5% × 3 = 1.5%
- Double-Chance: 0.5% × 2 = 1.0%
- **Combined: 0.5% × 3 × 2 = 3.0%**

**Expected flips**: ~33 (vs 200 without strategy!)

**Learn more**: [Unlock System Guide](docs/unlock-system.md)

---

## Rarity Tiers

| Rarity | Color | Examples | Difficulty |
|--------|-------|----------|------------|
| **Common** | White/Blue | 10Flips, Gemini, Ram | Easy milestones |
| **Uncommon** | Light Blue | 25Flips, Rooster | Moderate challenges |
| **Rare** | Purple | Dragon, 100Flips, Panda | Tough requirements |
| **Legendary** | Orange/Red | Dragon_Rare, Completionist, Headmaster | Ultimate achievements |

---

## Tech Stack

**Built with:**
- **.NET 8** Blazor WebAssembly
- **C#** for game logic and unlock system
- **CSS3** animations (3D coin physics)
- **JavaScript** for particle effects, audio, and haptics
- **Blazored.LocalStorage** for guest/offline progress
- **Azure Table Storage** for signed-in accounts and cloud progress
- **Entra External ID (CIAM)** for Google/Apple sign-in (same tenant as Cribbage Trainer)

**Key Features:**
- Progressive Web App (install to home screen)
- Offline support via Service Worker
- Responsive design (desktop + mobile)
- Haptic feedback on supported devices
- Optional cloud save after Google/Apple sign-in

**Architecture Highlights:**
- `UnlockProgressService`: Tracks stats, checks unlock conditions, dual-mode cache (guest local-only / signed-in read-through + write-behind)
- `CoinService`: Manages coin types and metadata
- Component-based UI with real-time state management
- Modular coin type system for easy expansion
- Coin catalog/images: blob storage (`CoinStorageService`). Player progress is **not** SQL.

---

## Project Structure

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

## Running Locally

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

Open `https://localhost:5003` in your browser.

For cloud accounts locally, also run Azurite and the Functions API (see below).

---

## Cloud accounts and Azure Tables

Player progress no longer uses SQL Server, EF Core, or `CoinFlipGame.DB`. That project is retired for the Api runtime. Coins remain static files / blob images; only **accounts and progress** live in Tables.

### Shared identity with Cribbage Trainer

Use the **same Entra External ID / CIAM app** Josh already uses for Cribbage so one Google or Apple sign-in maps to a stable provider subject in both games. Coin Flip keeps its **own** storage account and `AccountId` rows; identity is shared via the same JWT `iss` + `sub`, not by sharing Cribbage tables.

Setting names (do not commit secrets or connection strings):

| App | Setting | Purpose |
|-----|---------|---------|
| Client | `ExternalAuth:Enabled` | Feature flag |
| Client | `ExternalAuth:ClientId` | SPA / public client id |
| Client | `ExternalAuth:AuthorizationEndpoint` | CIAM authorize URL |
| Client | `ExternalAuth:TokenEndpoint` | CIAM token URL |
| Client | `ExternalAuth:Scope` | `openid profile api://<audience>/access_as_user` |
| Client | `ApiSettings:BaseUrl` | Empty in SWA (same origin `/api`); `http://localhost:7071` locally |
| Api | `ExternalAuth:Enabled` | Feature flag |
| Api | `ExternalAuth:Authority` | CIAM issuer, e.g. `https://cribbagetrainer.ciamlogin.com/<tenant>/v2.0` |
| Api | `ExternalAuth:Audience` | API app id / audience |
| Api | `TablesStorageConnectionString` | Dedicated Coin Flip Tables account (not Cribbage storage) |
| Api | `AzureStorage:ConnectionString` | Blob images only (keep separate from Tables) |

Register Coin Flip redirect URIs on that existing SPA app, for example `https://localhost:5003/auth/callback` and `https://coin.joshparsons.ca/auth/callback`.

### Tables

Dedicated Hot LRS StorageV2 account (do **not** put Coin Flip player data in the Cribbage Trainer storage account):

| Table | PartitionKey | RowKey |
|-------|--------------|--------|
| `PlayerAccounts` | AccountId | `Profile` |
| `ExternalIdentities` | SHA256(issuer) | SHA256(subject) |
| `PlayerSessions` | token id | `Session` |
| `PlayerProgress` | AccountId | `Progress` |

Api routes:

- `POST /api/player/external/login`
- `POST /api/player/external/link`
- `GET /api/player/external/identities`
- `GET /api/player/me`
- `POST /api/player/logout`
- `GET /api/player/progress`
- `PUT /api/player/progress` (server merge: max counters/streaks, union unlocks, earliest unlock timestamps, max consecutive counts)

### Caching / merge

- **Guest:** LocalStorage only. Zero Table operations.
- **Signed in:** memory + LocalStorage read-through cache. Tables writes are write-behind: unlock events, flip milestones, tab hide, sign-out, and a 45s throttle. Individual flips do not write Tables.
- **First successful login:** load cloud progress, merge with local (max/union, never wipe harder progress), save merged to LocalStorage and Tables.

### Local (Azurite)

`local.settings.json` is gitignored (`CopyToPublishDirectory` Never). Copy the example:

```bash
copy CoinFlipGame.Api\local.settings.example.json CoinFlipGame.Api\local.settings.json
```

`TablesStorageConnectionString` defaults to `UseDevelopmentStorage=true`. Start Azurite, then the Api:

```bash
npx azurite --silent --location .azurite
dotnet run --project CoinFlipGame.Api
dotnet run --project CoinFlipGame.App
```

### Azure (Josh / Azure Bot)

Do not put player data in the Cribbage storage account. Create a separate account such as `jparsonscoinflip` or `coinflipgame*` in an appropriate resource group:

```bash
az storage account create --name jparsonscoinflip --resource-group <rg> --location canadacentral --sku Standard_LRS --kind StorageV2 --access-tier Hot

az storage table create --account-name jparsonscoinflip --name PlayerAccounts
az storage table create --account-name jparsonscoinflip --name ExternalIdentities
az storage table create --account-name jparsonscoinflip --name PlayerSessions
az storage table create --account-name jparsonscoinflip --name PlayerProgress
```

Set the Functions / SWA app setting `TablesStorageConnectionString` to that account's connection string. Keep blob `AzureStorage:ConnectionString` as-is. Also set `ExternalAuth:Enabled`, `ExternalAuth:Authority`, and `ExternalAuth:Audience` to the same public CIAM identifiers used by Cribbage.

---

## License

MIT License - See [LICENSE](LICENSE) file for details.

**Developed by [Josh Parsons](https://www.joshparsons.ca)**
