using CoinFlipGame.App.Models;
using CoinFlipGame.App.Models.Unlocks;
using CoinFlipGame.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CoinFlipGame.App.Components.Pages;

public partial class Build
{
    [Inject]
    private CoinService CoinService { get; set; } = default!;

    [Inject]
    private UnlockProgressService UnlockProgress { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private Dictionary<CoinType, List<CoinImage>>? availableCoins;
    private List<CoinImage>? allCoinImages; // All images from directory
    private CoinImage? selectedCoin;
    private UnlockCondition mainCondition = new() { Type = UnlockConditionType.None, Rarity = UnlockRarity.Common };
    private List<UnlockCondition> prerequisites = new();
    private CoinEffect coinEffect = new() { Type = CoinEffectType.None };
    private List<string> validationErrors = new();
    private bool isValid = false;
    private string generatedCode = string.Empty;
    private bool showCoinDrawer = false;
    private int? selectingForPrereqIndex = null;
    private bool selectingForMain = false;
    private string streakSideSelection = "Any";
    private bool isClearingCache = false;
    private bool showOnlyUnmapped = false;
    private bool hasLoadedOnce = false;
    private bool hasInteractedWithDrawer = false;
    private bool showValidationRules = false;

    private double unlockChancePercent
    {
        get => mainCondition.UnlockChance * 100;
        set => mainCondition.UnlockChance = value / 100.0;
    }

    private double biasStrengthPercent
    {
        get => coinEffect.BiasStrength * 100;
        set => coinEffect.BiasStrength = value / 100.0;
    }

    protected override async Task OnInitializedAsync()
    {
        await UnlockProgress.InitializeAsync();
        availableCoins = await CoinService.GetAllAvailableCoinsAsync();

        // Load ALL coin images from directory
        allCoinImages = new List<CoinImage>();
        if (availableCoins != null)
        {
            foreach (var coinTypeGroup in availableCoins.Values)
            {
                allCoinImages.AddRange(coinTypeGroup);
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Trigger peek animation after a short delay
            await Task.Delay(500);
            hasLoadedOnce = true;
            StateHasChanged();
        }
    }

    private List<CoinImage> GetFilteredCoins()
    {
        if (allCoinImages == null)
            return new List<CoinImage>();

        var filtered = allCoinImages.AsEnumerable();

        // Exclude Random.png, logo.png, and JPLogo type coins from the list
        filtered = filtered.Where(c => 
            !c.Path.Contains("Random.png") && 
            !c.Path.Contains("logo.png") &&
            c.Type is not JpLogoCoinType);

        if (showOnlyUnmapped)
        {
            // Show only coins without unlock conditions and effects (unmapped coins)
            // These are coins defined in GetCoinFiles() but not in GetUnlockConditions() or GetCoinEffects()
            filtered = filtered.Where(c => c.UnlockCondition == null && c.Effect == null);
        }
        else
        {
            // In "All Coins" view, exclude unmapped coins (only show configured coins)
            filtered = filtered.Where(c => c.UnlockCondition != null || c.Effect != null);
        }

        return filtered.ToList();
    }

    private int GetAllCoinsCount()
    {
        if (allCoinImages == null)
            return 0;

        // Count only configured coins (exclude Random.png, logo.png, JPLogo type, and unmapped coins)
        return allCoinImages.Count(c => 
            !c.Path.Contains("Random.png") && 
            !c.Path.Contains("logo.png") &&
            c.Type is not JpLogoCoinType &&
            (c.UnlockCondition != null || c.Effect != null)); // Only count configured coins
    }

    private int GetUnmappedCoinsCount()
    {
        if (allCoinImages == null)
            return 0;

        // Count coins without conditions/effects
        // These are coins defined in CoinType.GetCoinFiles() but not in GetUnlockConditions() or GetCoinEffects()
        // Exclude Random.png, logo.png, and JPLogo type coins
        return allCoinImages.Count(c =>
            !c.Path.Contains("Random.png") &&
            !c.Path.Contains("logo.png") &&
            c.Type is not JpLogoCoinType &&
            c.UnlockCondition == null &&
            c.Effect == null);
    }

    private void ShowAllCoins()
    {
        showOnlyUnmapped = false;
        StateHasChanged();
    }

    private void ShowUnmappedCoins()
    {
        showOnlyUnmapped = true;
        StateHasChanged();
    }

    private void ToggleDrawer()
    {
        hasInteractedWithDrawer = true; // Track user interaction

        if (selectingForMain || selectingForPrereqIndex.HasValue)
        {
            CloseCoinDrawer();
        }
        else
        {
            showCoinDrawer = !showCoinDrawer;
        }
    }

    private void OpenRequiredCoinSelector(int? prereqIndex)
    {
        hasInteractedWithDrawer = true; // Track user interaction
        selectingForMain = prereqIndex == null;
        selectingForPrereqIndex = prereqIndex;
        showCoinDrawer = true;
    }

    private void CloseCoinDrawer()
    {
        showCoinDrawer = false;
        selectingForMain = false;
        selectingForPrereqIndex = null;
    }

    private void HandleCoinSelected(CoinImage coin)
    {
        hasInteractedWithDrawer = true; // Track user interaction

        if (selectingForPrereqIndex.HasValue)
        {
            prerequisites[selectingForPrereqIndex.Value].RequiredCoinPath = coin.Path;
            CloseCoinDrawer();
        }
        else if (selectingForMain)
        {
            if (mainCondition.Type == UnlockConditionType.LandOnMultipleCoins && !mainCondition.UseDynamicCoinList)
            {
                if (mainCondition.RequiredCoinPaths == null)
                    mainCondition.RequiredCoinPaths = new List<string>();

                if (!mainCondition.RequiredCoinPaths.Contains(coin.Path))
                    mainCondition.RequiredCoinPaths.Add(coin.Path);
            }
            else
            {
                mainCondition.RequiredCoinPath = coin.Path;
            }
            CloseCoinDrawer();
        }
        else
        {
            selectedCoin = coin;
            LoadExistingConfiguration();
            showCoinDrawer = false;
        }
    }

    private void LoadExistingConfiguration()
    {
        if (selectedCoin?.UnlockCondition != null)
        {
            // Load main condition
            mainCondition = new UnlockCondition
            {
                Type = selectedCoin.UnlockCondition.Type,
                RequiredCount = selectedCoin.UnlockCondition.RequiredCount,
                RequiredCoinPath = selectedCoin.UnlockCondition.RequiredCoinPath,
                Description = selectedCoin.UnlockCondition.Description ?? string.Empty,
                UnlockChance = selectedCoin.UnlockCondition.UnlockChance,
                RequiresActiveCoin = selectedCoin.UnlockCondition.RequiresActiveCoin,
                RequiredCoinPaths = selectedCoin.UnlockCondition.RequiredCoinPaths?.ToList(),
                Rarity = selectedCoin.UnlockCondition.Rarity,
                UseDynamicCoinList = selectedCoin.UnlockCondition.UseDynamicCoinList,
                StreakSide = selectedCoin.UnlockCondition.StreakSide,
                // Load characteristic-based unlock properties
                CharacteristicFilter = selectedCoin.UnlockCondition.CharacteristicFilter,
                FilterUnlockConditionType = selectedCoin.UnlockCondition.FilterUnlockConditionType,
                FilterEffectType = selectedCoin.UnlockCondition.FilterEffectType,
                FilterPrerequisiteCount = selectedCoin.UnlockCondition.FilterPrerequisiteCount,
                SideRequirement = selectedCoin.UnlockCondition.SideRequirement,
                ConsecutiveCount = selectedCoin.UnlockCondition.ConsecutiveCount
            };

            // Set streak side selection
            if (selectedCoin.UnlockCondition.StreakSide.HasValue)
            {
                streakSideSelection = selectedCoin.UnlockCondition.StreakSide.Value.ToString();
            }
            else
            {
                streakSideSelection = "Any";
            }

            // Load prerequisites
            prerequisites.Clear();
            if (selectedCoin.UnlockCondition.Prerequisites != null)
            {
                foreach (var prereq in selectedCoin.UnlockCondition.Prerequisites)
                {
                    prerequisites.Add(new UnlockCondition
                    {
                        Type = prereq.Type,
                        RequiredCount = prereq.RequiredCount,
                        RequiredCoinPath = prereq.RequiredCoinPath,
                        Description = prereq.Description ?? string.Empty
                    });
                }
            }
        }
        else
        {
            // Reset to defaults
            mainCondition = new UnlockCondition { Type = UnlockConditionType.None, Rarity = UnlockRarity.Common };
            prerequisites.Clear();
            streakSideSelection = "Any";
        }

        // Load coin effect
        if (selectedCoin?.Effect != null)
        {
            coinEffect = new CoinEffect
            {
                Type = selectedCoin.Effect.Type,
                Description = selectedCoin.Effect.Description ?? string.Empty,
                AutoClickInterval = selectedCoin.Effect.AutoClickInterval,
                BiasStrength = selectedCoin.Effect.BiasStrength,
                ComboType = selectedCoin.Effect.ComboType,
                ComboMultiplier = selectedCoin.Effect.ComboMultiplier,
                LuckModifier = selectedCoin.Effect.LuckModifier,
                LuckModifierType = selectedCoin.Effect.LuckModifierType
            };
        }
        else
        {
            coinEffect = new CoinEffect { Type = CoinEffectType.None };
        }

        // Clear validation
        validationErrors.Clear();
        isValid = false;
        generatedCode = string.Empty;
    }

    private void AddPrerequisite()
    {
        prerequisites.Add(new UnlockCondition
        {
            Type = UnlockConditionType.TotalFlips,
            RequiredCount = 10
        });
    }

    private void RemovePrerequisite(int index)
    {
        prerequisites.RemoveAt(index);
    }

    private void AddRequiredCoin()
    {
        if (mainCondition.RequiredCoinPaths == null)
            mainCondition.RequiredCoinPaths = new List<string>();

        selectingForMain = true;
        showCoinDrawer = true;
    }

    private void RemoveRequiredCoin(int index)
    {
        mainCondition.RequiredCoinPaths?.RemoveAt(index);
    }

    private string GetCoinNameFromPath(string path)
    {
        return System.IO.Path.GetFileNameWithoutExtension(path);
    }

    private void ValidateAndGenerate()
    {
        validationErrors.Clear();
        isValid = false;

        if (mainCondition.Type == UnlockConditionType.None && prerequisites.Any())
        {
            validationErrors.Add("Main condition type cannot be 'None' if prerequisites exist.");
        }

        if (string.IsNullOrWhiteSpace(mainCondition.Description) && mainCondition.Type != UnlockConditionType.None)
        {
            validationErrors.Add("Description is required for the main condition.");
        }

        switch (mainCondition.Type)
        {
            case UnlockConditionType.TotalFlips:
            case UnlockConditionType.HeadsFlips:
            case UnlockConditionType.TailsFlips:
            case UnlockConditionType.Streak:
                if (mainCondition.RequiredCount <= 0)
                    validationErrors.Add($"Required count must be greater than 0 for {mainCondition.Type}.");
                break;

            case UnlockConditionType.LandOnCoin:
                if (string.IsNullOrEmpty(mainCondition.RequiredCoinPath))
                    validationErrors.Add("Required coin path must be set for LandOnCoin type.");
                if (mainCondition.RequiredCount <= 0)
                    validationErrors.Add("Required count must be greater than 0.");
                break;

            case UnlockConditionType.RandomChance:
                if (mainCondition.UnlockChance <= 0 || mainCondition.UnlockChance > 1)
                    validationErrors.Add("Unlock chance must be between 0 and 1 (0% to 100%).");
                if (mainCondition.RequiresActiveCoin && string.IsNullOrEmpty(mainCondition.RequiredCoinPath))
                    validationErrors.Add("Required coin path must be set when RequiresActiveCoin is true.");
                break;

            case UnlockConditionType.LandOnMultipleCoins:
                if (mainCondition.RequiredCount <= 0)
                    validationErrors.Add("Required count must be greater than 0.");
                if (!mainCondition.UseDynamicCoinList && (mainCondition.RequiredCoinPaths == null || !mainCondition.RequiredCoinPaths.Any()))
                    validationErrors.Add("At least one required coin must be specified, or enable UseDynamicCoinList.");
                break;

            case UnlockConditionType.LandOnCoinsWithCharacteristics:
                if (mainCondition.ConsecutiveCount <= 0)
                    validationErrors.Add("Consecutive count must be greater than 0.");
                
                switch (mainCondition.CharacteristicFilter)
                {
                    case CoinCharacteristicFilter.SpecificCoins:
                        if (mainCondition.RequiredCoinPaths == null || !mainCondition.RequiredCoinPaths.Any())
                            validationErrors.Add("At least one coin must be specified for SpecificCoins filter.");
                        break;
                    
                    case CoinCharacteristicFilter.UnlockConditionType:
                        if (!mainCondition.FilterUnlockConditionType.HasValue)
                            validationErrors.Add("Unlock condition type must be selected for UnlockConditionType filter.");
                        break;
                    
                    case CoinCharacteristicFilter.EffectType:
                        if (!mainCondition.FilterEffectType.HasValue)
                            validationErrors.Add("Effect type must be selected for EffectType filter.");
                        break;
                    
                    case CoinCharacteristicFilter.PrerequisiteCountEquals:
                    case CoinCharacteristicFilter.PrerequisiteCountGreaterThan:
                    case CoinCharacteristicFilter.PrerequisiteCountLessThan:
                        if (mainCondition.FilterPrerequisiteCount < 0)
                            validationErrors.Add("Prerequisite count cannot be negative.");
                        break;
                }
                
                // Validate side requirements
                if (mainCondition.SideRequirement == SideRequirement.HeadsAndTails)
                {
                    if (mainCondition.RequiredCoinPaths == null || mainCondition.RequiredCoinPaths.Count != 2)
                        validationErrors.Add("HeadsAndTails side requirement requires exactly 2 coins in RequiredCoinPaths.");
                    if (!mainCondition.RequiresActiveCoin)
                        validationErrors.Add("HeadsAndTails side requirement requires RequiresActiveCoin to be true.");
                }
                
                if (mainCondition.SideRequirement == SideRequirement.AnyFromList)
                {
                    if (mainCondition.RequiredCoinPaths == null || mainCondition.RequiredCoinPaths.Count < 2)
                        validationErrors.Add("AnyFromList side requirement requires at least 2 coins in RequiredCoinPaths.");
                    if (!mainCondition.RequiresActiveCoin)
                        validationErrors.Add("AnyFromList side requirement requires RequiresActiveCoin to be true.");
                }
                
                break;
        }

        for (int i = 0; i < prerequisites.Count; i++)
        {
            var prereq = prerequisites[i];
            if (string.IsNullOrWhiteSpace(prereq.Description))
                validationErrors.Add($"Prerequisite {i + 1}: Description is required.");

            if (prereq.Type == UnlockConditionType.LandOnCoin && string.IsNullOrEmpty(prereq.RequiredCoinPath))
                validationErrors.Add($"Prerequisite {i + 1}: Required coin path must be set for LandOnCoin type.");

            // Check if prerequisite requires the coin to unlock itself
            if (selectedCoin != null && 
                prereq.Type == UnlockConditionType.LandOnCoin && 
                !string.IsNullOrEmpty(prereq.RequiredCoinPath) && 
                prereq.RequiredCoinPath == selectedCoin.Path)
            {
                validationErrors.Add($"Prerequisite {i + 1}: Coin cannot require itself to unlock.");
            }

            if ((prereq.Type == UnlockConditionType.TotalFlips ||
                 prereq.Type == UnlockConditionType.HeadsFlips ||
                 prereq.Type == UnlockConditionType.TailsFlips ||
                 prereq.Type == UnlockConditionType.Streak ||
                 prereq.Type == UnlockConditionType.LandOnCoin) && prereq.RequiredCount <= 0)
            {
                validationErrors.Add($"Prerequisite {i + 1}: Required count must be greater than 0.");
            }
        }

        if (!validationErrors.Any())
        {
            isValid = true;
            GenerateCode();
        }
    }

    private void GenerateCode()
    {
        if (selectedCoin == null) return;

        var coinFileName = System.IO.Path.GetFileName(selectedCoin.Path);
        var indent = "            ";
        var code = new System.Text.StringBuilder();

        code.AppendLine($@"{indent}{{");
        code.AppendLine($@"{indent}    ""{coinFileName}"", new UnlockCondition");
        code.AppendLine($@"{indent}    {{");
        code.AppendLine($@"{indent}        Type = UnlockConditionType.{mainCondition.Type},");

        switch (mainCondition.Type)
        {
            case UnlockConditionType.TotalFlips:
            case UnlockConditionType.HeadsFlips:
            case UnlockConditionType.TailsFlips:
            case UnlockConditionType.Streak:
            case UnlockConditionType.LandOnCoin:
                code.AppendLine($@"{indent}        RequiredCount = {mainCondition.RequiredCount},");
                break;
        }

        if (mainCondition.Type == UnlockConditionType.LandOnCoin && !string.IsNullOrEmpty(mainCondition.RequiredCoinPath))
        {
            code.AppendLine($@"{indent}        RequiredCoinPath = ""{mainCondition.RequiredCoinPath}"",");
        }

        if (mainCondition.Type == UnlockConditionType.RandomChance)
        {
            code.AppendLine($@"{indent}        UnlockChance = {mainCondition.UnlockChance.ToString("F5")}, // {unlockChancePercent:F3}% chance");
            if (mainCondition.RequiresActiveCoin)
            {
                code.AppendLine($@"{indent}        RequiresActiveCoin = true,");
                if (!string.IsNullOrEmpty(mainCondition.RequiredCoinPath))
                    code.AppendLine($@"{indent}        RequiredCoinPath = ""{mainCondition.RequiredCoinPath}"",");
            }
        }

        if (mainCondition.Type == UnlockConditionType.LandOnMultipleCoins)
        {
            code.AppendLine($@"{indent}        RequiredCount = {mainCondition.RequiredCount},");
            if (mainCondition.UseDynamicCoinList)
            {
                code.AppendLine($@"{indent}        UseDynamicCoinList = true,");
            }
            else if (mainCondition.RequiredCoinPaths != null && mainCondition.RequiredCoinPaths.Any())
            {
                code.AppendLine($@"{indent}        RequiredCoinPaths = new List<string>");
                code.AppendLine($@"{indent}        {{");
                foreach (var path in mainCondition.RequiredCoinPaths)
                {
                    code.AppendLine($@"{indent}            ""{path}"",");
                }
                code.AppendLine($@"{indent}        }},");
            }
        }

        if (mainCondition.Type == UnlockConditionType.LandOnCoinsWithCharacteristics)
        {
            code.AppendLine($@"{indent}        ConsecutiveCount = {mainCondition.ConsecutiveCount},");
            code.AppendLine($@"{indent}        CharacteristicFilter = CoinCharacteristicFilter.{mainCondition.CharacteristicFilter},");
            
            switch (mainCondition.CharacteristicFilter)
            {
                case CoinCharacteristicFilter.SpecificCoins:
                    if (mainCondition.RequiredCoinPaths != null && mainCondition.RequiredCoinPaths.Any())
                    {
                        code.AppendLine($@"{indent}        RequiredCoinPaths = new List<string>");
                        code.AppendLine($@"{indent}        {{");
                        foreach (var path in mainCondition.RequiredCoinPaths)
                        {
                            code.AppendLine($@"{indent}            ""{path}"",");
                        }
                        code.AppendLine($@"{indent}        }},");
                    }
                    break;
                
                case CoinCharacteristicFilter.UnlockConditionType:
                    if (mainCondition.FilterUnlockConditionType.HasValue)
                    {
                        code.AppendLine($@"{indent}        FilterUnlockConditionType = UnlockConditionType.{mainCondition.FilterUnlockConditionType.Value},");
                    }
                    break;
                
                case CoinCharacteristicFilter.EffectType:
                    if (mainCondition.FilterEffectType.HasValue)
                    {
                        code.AppendLine($@"{indent}        FilterEffectType = CoinEffectType.{mainCondition.FilterEffectType.Value},");
                    }
                    break;
                
                case CoinCharacteristicFilter.PrerequisiteCountEquals:
                case CoinCharacteristicFilter.PrerequisiteCountGreaterThan:
                case CoinCharacteristicFilter.PrerequisiteCountLessThan:
                    code.AppendLine($@"{indent}        FilterPrerequisiteCount = {mainCondition.FilterPrerequisiteCount},");
                    break;
            }
            
            code.AppendLine($@"{indent}        SideRequirement = SideRequirement.{mainCondition.SideRequirement},");
            
            // Add RequiresActiveCoin and RequiredCoinPath if specified
            if (mainCondition.RequiresActiveCoin)
            {
                code.AppendLine($@"{indent}        RequiresActiveCoin = true,");
                if (!string.IsNullOrEmpty(mainCondition.RequiredCoinPath))
                {
                    code.AppendLine($@"{indent}        RequiredCoinPath = ""{mainCondition.RequiredCoinPath}"",");
                }
            }
        }

        if (mainCondition.Type == UnlockConditionType.Streak && streakSideSelection != "Any")
        {
            code.AppendLine($@"{indent}        StreakSide = Models.Unlocks.StreakSide.{streakSideSelection},");
        }

        code.AppendLine($@"{indent}        Description = ""{mainCondition.Description}"",");
        code.AppendLine($@"{indent}        Rarity = UnlockRarity.{mainCondition.Rarity}");

        if (prerequisites.Any())
        {
            code.AppendLine($@"{indent}        ,");
            code.AppendLine($@"{indent}        Prerequisites = new List<UnlockCondition>");
            code.AppendLine($@"{indent}        {{");

            foreach (var prereq in prerequisites)
            {
                code.AppendLine($@"{indent}            new UnlockCondition");
                code.AppendLine($@"{indent}            {{");
                code.AppendLine($@"{indent}                Type = UnlockConditionType.{prereq.Type},");

                if (prereq.RequiredCount > 0)
                    code.AppendLine($@"{indent}                RequiredCount = {prereq.RequiredCount},");

                if (!string.IsNullOrEmpty(prereq.RequiredCoinPath))
                {
                    code.AppendLine($@"{indent}                RequiredCoinPath = ""{prereq.RequiredCoinPath}"",");
                }

                code.AppendLine($@"{indent}                Description = ""{prereq.Description}"",");
                code.AppendLine($@"{indent}                Rarity = UnlockRarity.{prereq.Rarity}");
                code.AppendLine($@"{indent}            }},");
            }

            code.AppendLine($@"{indent}        }},");
        }

        if (coinEffect.Type != CoinEffectType.None)
        {
            code.AppendLine($@"{indent}        ,");
            code.AppendLine($@"{indent}        Effect = new CoinEffect");
            code.AppendLine($@"{indent}        {{");
            code.AppendLine($@"{indent}            Type = CoinEffectType.{coinEffect.Type},");

            if (coinEffect.AutoClickInterval > 0)
                code.AppendLine($@"{indent}            AutoClickInterval = {coinEffect.AutoClickInterval},");

            if (coinEffect.BiasStrength != 0)
                code.AppendLine($@"{indent}            BiasStrength = {coinEffect.BiasStrength.ToString("F5")}, // {biasStrengthPercent:F2}%");

            if (coinEffect.ComboMultiplier != 0)
                code.AppendLine($@"{indent}            ComboMultiplier = {coinEffect.ComboMultiplier},");

            if (coinEffect.LuckModifier != 1)
                code.AppendLine($@"{indent}            LuckModifier = {coinEffect.LuckModifier},");

            code.AppendLine($@"{indent}            Description = ""{coinEffect.Description}""");
            code.AppendLine($@"{indent}        }},");
        }

        // Remove last comma and close the object
        if (code.Length > 0)
        {
            code.Length -= 3; // Remove last comma and newline
            code.AppendLine($@"{indent}    }}");
        }

        // Log the generated code for debugging
        Console.WriteLine("Generated Code: ");
        Console.WriteLine(code.ToString());

        generatedCode = code.ToString();
    }

    private void CopyToClipboard()
    {
        JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", generatedCode);
    }

    private async Task ClearCacheAndReload()
    {
        isClearingCache = true;
        StateHasChanged();

        try
        {
            await JSRuntime.InvokeVoidAsync("window.clearCachesAndReload");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing cache: {ex.Message}");
            // Fallback to hard reload
            try
            {
                await JSRuntime.InvokeVoidAsync("location.reload", true);
            }
            catch
            {
                // If even location.reload fails, do nothing
            }
        }
        finally
        {
            isClearingCache = false;
        }
    }

    private void HandleDrawerBackgroundClick()
    {
        // Close the drawer if the background is clicked and it's currently open
        if (showCoinDrawer)
        {
            showCoinDrawer = false;
        }
    }

    private async Task HandleCoinHover(CoinImage coin)
    {
        // Play hover sound when mouse enters a coin
        try
        {
            await JSRuntime.InvokeVoidAsync("playDrawerHoverSound");
        }
        catch (Exception ex)
        {
            // Silently fail if audio system isn't ready
            Console.WriteLine($"Failed to play hover sound: {ex.Message}");
        }
    }

    private string GetDrawerClass()
    {
        // Build the class string for the drawer
        var classes = new List<string>();

        // Always add the appropriate state class
        if (showCoinDrawer)
        {
            classes.Add("drawer-open");
        }
        else
        {
            classes.Add("drawer-closed");
        }

        // Add peek class only if conditions are met
        if (hasLoadedOnce && !hasInteractedWithDrawer && !showCoinDrawer)
        {
            classes.Add("drawer-peek");
        }

        return string.Join(" ", classes);
    }

    private void ToggleValidationRules()
    {
        showValidationRules = !showValidationRules;
    }

    private async Task ShowValidationGuide()
    {
        // TODO: Implement modal or tooltip with validation rules
        await JSRuntime.InvokeVoidAsync("alert", "Validation Rules:\n\n" +
            "Main Condition:\n" +
            "- Description is required (unless condition type is \"None\")\n" +
            "- Cannot be \"None\" if prerequisites exist\n" +
            "- Coin cannot require itself to unlock\n\n" +
            "Count-Based Conditions:\n" +
            "- TotalFlips, HeadsFlips, TailsFlips, Streak: Required count must be > 0\n" +
            "- LandOnCoin: Required count must be > 0 and coin must be selected\n\n" +
            "Special Conditions:\n" +
            "- RandomChance: Unlock chance must be between 0% and 100%\n" +
            "- RandomChance (RequiresActiveCoin): Active coin must be selected\n" +
            "- LandOnMultipleCoins: At least one coin required OR enable dynamic list\n\n" +
            "Prerequisites:\n" +
            "- Description is required for each prerequisite\n" +
            "- Prerequisites cannot create circular dependencies\n" +
            "- LandOnCoin prerequisites must have a coin selected\n" +
            "- Count-based prerequisites must have count > 0");
    }

    private string GetCharacteristicFilterDisplayName(CoinCharacteristicFilter filter)
    {
        return filter switch
        {
            CoinCharacteristicFilter.SpecificCoins => "Specific Coins",
            CoinCharacteristicFilter.UnlockConditionType => "By Unlock Condition Type",
            CoinCharacteristicFilter.EffectType => "By Effect Type",
            CoinCharacteristicFilter.HasAnyEffect => "Has Any Effect",
            CoinCharacteristicFilter.HasAnyUnlockCondition => "Has Any Unlock Condition",
            CoinCharacteristicFilter.PrerequisiteCountEquals => "Prerequisite Count Equals",
            CoinCharacteristicFilter.PrerequisiteCountGreaterThan => "Prerequisite Count Greater Than",
            CoinCharacteristicFilter.PrerequisiteCountLessThan => "Prerequisite Count Less Than",
            _ => filter.ToString()
        };
    }

    private string GetSideRequirementDisplayName(SideRequirement side)
    {
        return side switch
        {
            SideRequirement.Either => "Either Side (Heads OR Tails)",
            SideRequirement.Both => "Both Sides (Same Coin on Heads AND Tails)",
            SideRequirement.HeadsOnly => "Heads Side Only",
            SideRequirement.TailsOnly => "Tails Side Only",
            SideRequirement.HeadsAndTails => "Heads AND Tails (Two Different Coins)",
            SideRequirement.AnyFromList => "Any Coins From List (Both Sides)",
            _ => side.ToString()
        };
    }

    private string GetSideRequirementHint(SideRequirement side)
    {
        return side switch
        {
            SideRequirement.Either => "Matching coin can be on heads OR tails (default)",
            SideRequirement.Both => "Same matching coin must be on BOTH heads AND tails simultaneously",
            SideRequirement.HeadsOnly => "Matching coin must be set as the heads face",
            SideRequirement.TailsOnly => "Matching coin must be set as the tails face",
            SideRequirement.HeadsAndTails => "Two specific coins must be active, one on each side (order doesn't matter). Requires exactly 2 coins in RequiredCoinPaths.",
            SideRequirement.AnyFromList => "Both heads AND tails coins must be from the RequiredCoinPaths list (can be same or different)",
            _ => ""
        };
    }
}
