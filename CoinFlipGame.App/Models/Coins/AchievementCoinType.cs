using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Achievement themed coins
/// </summary>
public class AchievementCoinType : CoinType
{
    public override string Name { get; set; } = "Achievements";
    public override string BasePath { get; set; } = "/img/coins/AI/Achievements";
    public override string Category { get; set; } = "AI";
    
    public override List<string> GetCoinFiles() => new()
    {
        "10Flips.png",
        "25Flips.png",
        "50Flips.png",
        "100Flips.png",
        "500Flips.png",
        "1000Flips.png",
        "1500Flips.png",
        "2000Flips.png",
        "5000Flips.png",
        "10000Flips.png",
        "3HeadStreak.png",
        "4HeadStreak.png",
        "5HeadStreak.png",
        "10HeadStreak.png",
        "50HeadStreak.png",
        "Headmaster.png",
        "3TailStreak.png",
        "4TailStreak.png",
        "5TailStreak.png",
        "10TailStreak.png",
        "50TailStreak.png",
        "TailMaster.png",
        "Completionist.png"
    };
    
    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        { 
            "10Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 10, 
                Description = "Flip 10 times",
                Rarity = UnlockRarity.Common
            }
        },
        { 
            "25Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 25, 
                Description = "Flip 25 times",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "50Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 50, 
                Description = "Flip 50 times",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "100Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 100, 
                Description = "Flip 100 times",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "500Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 500,
                Description = "Flip 500 times",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "1000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 1000,
                Description = "Flip 1,000 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "1500Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 1500,
                Description = "Flip 1,500 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "2000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 2000,
                Description = "Flip 2,000 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "5000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 5000,
                Description = "Flip 5,000 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "10000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 10000,
                Description = "Flip 10,000 times",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "3HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 3,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 3 times in a row",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "4HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 4,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 4 times in a row",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "5HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 5 times in a row",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "10HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 10,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 10 times in a row",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "50HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 50,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 50 times in a row",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "Headmaster.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 100,
                StreakSide = StreakSide.Heads,
                Description = "Land on heads 100 times in a row!",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "3TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 3,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 3 times in a row",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "4TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 4,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 4 times in a row",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "5TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 5 times in a row",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "10TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 10,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 10 times in a row",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "50TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 50,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 50 times in a row",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "TailMaster.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 100,
                StreakSide = StreakSide.Tails,
                Description = "Land on tails 100 times in a row",
                Rarity = UnlockRarity.Legendary
            }
        },
        { 
            "Completionist.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnMultipleCoins, 
                RequiredCount = 1,
                RequiredCoinPaths = new List<string>
                {
                    // JP Logo coins
                    "/img/coins/logo.png",
                    
                    // Zodiak coins
                    "/img/coins/AI/Zodiak/Gemini.png",
                    "/img/coins/AI/Zodiak/Rat.png",
                    "/img/coins/AI/Zodiak/Ram.png",
                    "/img/coins/AI/Zodiak/Dog.png",
                    "/img/coins/AI/Zodiak/Tauros.png",
                    "/img/coins/AI/Zodiak/Rabbit.png",
                    "/img/coins/AI/Zodiak/Rooster.png",
                    "/img/coins/AI/Zodiak/Pig.png",
                    "/img/coins/AI/Zodiak/Dragon.png",
                    "/img/coins/AI/Zodiak/Dragon_Rare.png",
                    
                    // Achievement coins (excluding self)
                    "/img/coins/AI/Achievements/10Flips.png",
                    "/img/coins/AI/Achievements/25Flips.png",
                    "/img/coins/AI/Achievements/50Flips.png",
                    "/img/coins/AI/Achievements/100Flips.png",
                    "/img/coins/AI/Achievements/500Flips.png",
                    "/img/coins/AI/Achievements/1000Flips.png",
                    "/img/coins/AI/Achievements/1500Flips.png",
                    "/img/coins/AI/Achievements/2000Flips.png",
                    "/img/coins/AI/Achievements/5000Flips.png",
                    "/img/coins/AI/Achievements/10000Flips.png",

                    "/img/coins/AI/Achievements/3HeadStreak.png",
                    "/img/coins/AI/Achievements/4HeadStreak.png",
                    "/img/coins/AI/Achievements/5HeadStreak.png",
                    "/img/coins/AI/Achievements/10HeadStreak.png",
                    "/img/coins/AI/Achievements/50HeadStreak.png",
                    "/img/coins/AI/Achievements/Headmaster.png",

                    "/img/coins/AI/Achievements/3TailStreak.png",
                    "/img/coins/AI/Achievements/4TailStreak.png",
                    "/img/coins/AI/Achievements/5TailStreak.png",
                    "/img/coins/AI/Achievements/10TailStreak.png",
                    "/img/coins/AI/Achievements/50TailStreak.png",
                    "/img/coins/AI/Achievements/TailMaster.png"
                },
                Description = "Unlock all other coins in the game",
                Rarity = UnlockRarity.Legendary
            }
        }
    };
}
