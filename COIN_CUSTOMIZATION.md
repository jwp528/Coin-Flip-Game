# Coin Customization System

## Overview
The coin customization system allows users to select different coin images for Heads and Tails from various themed collections.

## Architecture

### CoinType System
- **CoinType** (base class): Defines coin categories with name, base path, and category
- **JpLogoCoinType**: Default JP logo coin
- **ZodiakCoinType**: Zodiac-themed coins (Gemini, Ram, Taurus)
- **CartoonCoinType**: Placeholder for future cartoon-themed coins

### CoinService
A service that manages coin types and available coin images:
- Registers and retrieves coin types
- Caches available coin images
- Uses JS interop to list available coins
- Provides filtered access by category

### CoinSelector Component
A modal UI component that displays available coins grouped by type:
- Category grouping (Default, AI, etc.)
- Grid layout for coin options
- Hover effects with checkmark overlay
- Responsive design

## Usage

### Customizing Coin Sides
The UI makes it clear and easy to customize each side of the coin independently:

1. **Heads Side Customization**:
   - Look for the card labeled "HEADS SIDE" with a blue badge
   - Click anywhere on the card to open the coin selector
   - The card shows the current selection name below the count
   - On hover, see "Customize" button with animated edit icon

2. **Tails Side Customization**:
   - Look for the card labeled "TAILS SIDE" with a purple badge
   - Click anywhere on the card to open the coin selector
   - The card shows the current selection name below the count
   - On hover, see "Customize" button with animated edit icon

3. **Visual Indicators**:
   - **Side Badges**: Each card displays "HEADS SIDE" or "TAILS SIDE" badges
   - **Coin Preview**: 64px circular preview with 3D golden border
   - **Selection Name**: Shows current coin name (e.g., "JP Logo", "Gemini", "Ram")
   - **Hover Effects**: Cards lift up, borders glow, and customize button appears
   - **First-Time Tip**: A green bouncing tooltip guides new users

4. **Selection Process**:
   - Click on Heads or Tails card
   - Modal appears showing all available coins grouped by category
   - Click on a coin to select it for that specific side
   - The coin face immediately updates on both the card and the main coin

### Adding New Coin Types
1. Create a folder under `wwwroot/img/coins/` (e.g., `/AI/Sports`)
2. Add coin images to the folder (PNG, JPG, JPEG supported)
3. Create a new CoinType class:
```csharp
public class SportsCoinType : CoinType
{
    public override string Name { get; set; } = "Sports";
    public override string BasePath { get; set; } = "/img/coins/AI/Sports";
    public override string Category { get; set; } = "AI";
}
```
4. Register in CoinService constructor:
```csharp
_coinTypes.Add(new SportsCoinType());
```
5. Update `wwwroot/js/coinhelpers.js` registry:
```javascript
const coinRegistry = {
    // ...existing entries
    "/img/coins/AI/Sports": ["Basketball.png", "Soccer.png", "Tennis.png"]
};
```

## File Structure
```
CoinFlipGame.App/
??? Models/
?   ??? CoinType.cs                  # Coin type definitions
??? Services/
?   ??? CoinService.cs               # Coin management service
??? Components/
?   ??? CoinSelector.razor           # Coin selector modal
?   ??? CoinSelector.razor.css       # Modal styles
?   ??? Pages/
?       ??? Home.razor               # Main page with coin display
?       ??? Home.razor.cs            # Page logic
?       ??? Home.razor.css           # Page styles
??? wwwroot/
    ??? img/coins/                   # Coin images
    ?   ??? logo.png                 # Default coin
    ?   ??? AI/
    ?       ??? Zodiak/
    ?       ?   ??? Gemini.png
    ?       ?   ??? Ram.png
    ?       ?   ??? Tauros.png
    ?       ??? Cartoon/             # Placeholder
    ??? js/
        ??? coinhelpers.js           # File listing helper

```

## Technical Details

### State Management
- Selected coin images stored in `selectedHeadsImage` and `selectedTailsImage`
- Modal visibility controlled by `showCoinSelector` boolean
- `selectingFor` tracks which side is being customized ("heads" or "tails")

### Performance
- Coin images are cached in CoinService to avoid repeated directory scans
- CoinType registry is initialized once on service creation
- Modal uses CSS transitions for smooth animations

### Browser Compatibility
Since browsers can't list directory contents, available coins are maintained in a static registry in `coinhelpers.js`. This registry must be updated when new coin folders are added.

## Future Enhancements
- Allow users to upload custom coin images
- Add coin preview animation in the selector
- Implement coin image downloading/saving
- Add more built-in coin collections (Cartoon, Nature, Tech, etc.)
- Persist user selections in local storage
