# Running the App Locally with Azure Functions

## Prerequisites
- .NET 8 SDK
- Azure Functions Core Tools (install via: `npm install -g azure-functions-core-tools@4 --unsafe-perm true`)

## Running Both Projects

### Option 1: Using Visual Studio
1. Right-click the solution in Solution Explorer
2. Select **Set Startup Projects...**
3. Choose **Multiple startup projects**
4. Set both `CoinFlipGame.App` and `CoinFlipGame.Api` to **Start**
5. Press F5

### Option 2: Using Separate Terminals

**Terminal 1 - Azure Function:**
```powershell
cd CoinFlipGame.Api
func start
```
The function should start on `http://localhost:7071`

**Terminal 2 - Blazor App:**
```powershell
cd CoinFlipGame.App
dotnet run
```
The app should start on `https://localhost:5001` or similar

## Configuration

### Local Development
- `appsettings.Development.json` is configured to point to `http://localhost:7071`
- The version check will call `http://localhost:7071/api/version`

### Production (Azure Static Web Apps)
- `appsettings.json` has an empty `ApiSettings:BaseUrl`
- When empty, the app uses relative paths (`/api/version`)
- Azure Static Web Apps automatically routes `/api/*` to the deployed function

## Testing the Version Check

1. Open browser DevTools (F12) ? Console tab
2. Launch the app
3. You should see one of these logs:
   - `"API version check: API unavailable"` - Function isn't running
   - `"API version check: API=1.2.0+20250118220000, Client=1.2.0+20250118220000, UpdateAvailable=false"` - Working correctly

## Troubleshooting

### "API unavailable" in console
- Verify the Azure Function is running on port 7071
- Check `http://localhost:7071/api/version` in browser - should return JSON
- Ensure no CORS errors in DevTools Console

### Function won't start
- Install Azure Functions Core Tools: `npm install -g azure-functions-core-tools@4 --unsafe-perm true`
- Or download from: https://docs.microsoft.com/azure/azure-functions/functions-run-local

### Port conflicts
- If 7071 is in use, the function will use a different port
- Update `appsettings.Development.json` to match the actual port
