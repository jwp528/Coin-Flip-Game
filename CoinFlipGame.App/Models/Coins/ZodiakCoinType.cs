using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Zodiak themed coins
/// </summary>
public class ZodiakCoinType : CoinType
{
    public override string Name { get; set; } = "Zodiak";
    public override string BasePath { get; set; } = "/img/coins/AI/Zodiak";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "Gemini.png",
        "Rat.png",
        "Ram.png",
        "Dog.png",
        "Tauros.png",
        "Rabbit.png",
        "Rooster.png",
        "Pig.png",
        "Dragon.png",
        "Dragon_Rare.png"  // Rare variant
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "Gemini.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 10, 
                Description = "Flip 10 times",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Rat.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 20, 
                Description = "Flip 20 times",
                Rarity = UnlockRarity.Common
            } 
        },
        {
            "Ram.png", new UnlockCondition
            {
                Type = UnlockConditionType.HeadsFlips,
                RequiredCount = 10,
                Description = "Flip heads 10 times",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Dog.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 100,
                Description = "Flip 100 times",
                Rarity = UnlockRarity.Common
            }
        },
        { 
            "Tauros.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TailsFlips, 
                RequiredCount = 10, 
                Description = "Flip tails 10 times",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Rooster.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.Streak, 
                RequiredCount = 5, 
                Description = "Get a 5-flip streak with any side",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        {
            "Rabbit.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.07, // 7% chance
                Description = "7% chance after flipping tails 25 times",
                Rarity = UnlockRarity.Uncommon,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TailsFlips,
                        RequiredCount = 25,
                        Description = "Flip tails 25 times"
                    }
                }
            }
        },
        { 
            "Pig.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnCoin, 
                RequiredCount = 10,
                RequiredCoinPath = "/img/coins/AI/Zodiak/Ram.png",
                Description = "Land on the Ram coin 10 times",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        { 
            "Dragon.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnMultipleCoins, 
                RequiredCount = 10,
                RequiredCoinPaths = new List<string>
                {
                    "/img/coins/AI/Zodiak/Gemini.png",
                    "/img/coins/AI/Zodiak/Rat.png",
                    "/img/coins/AI/Zodiak/Ram.png",
                    "/img/coins/AI/Zodiak/Dog.png",
                    "/img/coins/AI/Zodiak/Rabbit.png",
                    "/img/coins/AI/Zodiak/Tauros.png",
                    "/img/coins/AI/Zodiak/Rooster.png",
                    "/img/coins/AI/Zodiak/Pig.png"
                },
                Description = "Flip each other Zodiak coin at least 10 times",
                Rarity = UnlockRarity.Rare
            } 
        },
        { 
            "Dragon_Rare.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance, 
                RequiredCount = 1,
                RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon.png",
                UnlockChance = 0.0005, // 0.05% chance
                Description = "0.05% chance when Dragon coin is selected",
                Rarity = UnlockRarity.Legendary,
                RequiresActiveCoin = true, // Only rolls when Dragon is actively selected
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon.png",
                        RequiredCount = 1,
                        Description = "Dragon must be unlocked"
                    }
                }
            } 
        }
    };
}
