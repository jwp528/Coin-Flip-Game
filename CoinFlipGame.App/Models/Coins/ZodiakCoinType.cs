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
                Description = "Complete 10 total flips",
                FlavorText = "Twin-faced and ever-changing, the Gemini represents duality in all things. Heads or tails? Why not both?",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Rat.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 20, 
                Description = "Complete 20 total flips",
                FlavorText = "Clever and resourceful, the Rat thrives in chaos. Every flip is another opportunity seized.",
                Rarity = UnlockRarity.Common
            } 
        },
        {
            "Ram.png", new UnlockCondition
            {
                Type = UnlockConditionType.HeadsFlips,
                RequiredCount = 10,
                Description = "Land heads 10 times",
                FlavorText = "Bold and headstrong, the Ram charges forward without hesitation. Victory favors the brave.",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Dog.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 100,
                Description = "Complete 100 total flips",
                FlavorText = "Loyal companion through countless flips, the Dog rewards persistence and dedication.",
                Rarity = UnlockRarity.Common
            }
        },
        { 
            "Tauros.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TailsFlips, 
                RequiredCount = 10, 
                Description = "Land tails 10 times",
                FlavorText = "Patient and steadfast, the bull knows that fortune comes to those who wait for their moment.",
                Rarity = UnlockRarity.Common
            } 
        },
        { 
            "Rooster.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.Streak, 
                RequiredCount = 5, 
                Description = "Achieve a 5-flip streak on any side",
                FlavorText = "The Rooster crows at dawn, heralding consistency. Five in a row marks your awakening.",
                Rarity = UnlockRarity.Uncommon
            } 
        },
        {
            "Rabbit.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.07, // 7% chance
                Description = "7% chance to unlock per flip (requires 25 tails flips first)",
                FlavorText = "Swift and elusive, the Rabbit appears only to those who have walked the shadowed path.",
                Rarity = UnlockRarity.Uncommon,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TailsFlips,
                        RequiredCount = 25,
                        Description = "Must have landed tails 25 times"
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
                FlavorText = "Prosperous and content, the Pig values comfort. Master the Ram's courage to earn this reward.",
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
                Description = "Land on each of the 8 other Zodiak coins at least 10 times",
                FlavorText = "Majestic and powerful, the Dragon reigns supreme. Only those who have mastered all signs may claim its power.",
                Rarity = UnlockRarity.Rare
            } 
        },
        { 
            "Dragon_Rare.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.RandomChance, 
                RequiredCount = 1,
                RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon.png",
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance per flip when using the Dragon coin (requires Dragon unlocked)",
                FlavorText = "Legends speak of a celestial dragon, born when the common dragon transcends its mortal form. Will you witness its ascension?",
                Rarity = UnlockRarity.Legendary,
                RequiresActiveCoin = true, // Only rolls when Dragon is actively selected
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon.png",
                        RequiredCount = 1,
                        Description = "Must have unlocked the Dragon coin"
                    }
                }
            } 
        }
    };
}
