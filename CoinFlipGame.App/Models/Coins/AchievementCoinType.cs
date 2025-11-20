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
                Description = "Complete 10 total flips",
                FlavorText = "Every journey begins with a single flip. Ten marks your first steps into the unknown.",
                Rarity = UnlockRarity.Common
            }
        },
        { 
            "25Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 25, 
                Description = "Complete 25 total flips",
                FlavorText = "The coin becomes familiar in your hand. Twenty-five flips, and you're just getting started.",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "50Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 50, 
                Description = "Complete 50 total flips",
                FlavorText = "Half a hundred spins through the air. Persistence is beginning to pay dividends.",
                Rarity = UnlockRarity.Uncommon
            }
        },
        { 
            "100Flips.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.TotalFlips, 
                RequiredCount = 100, 
                Description = "Complete 100 total flips",
                FlavorText = "A century of chances taken. The coin remembers each and every one.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "500Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 500,
                Description = "Complete 500 total flips",
                FlavorText = "Five hundred revolutions. What was once random now feels like destiny.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "1000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 1000,
                Description = "Complete 1,000 total flips",
                FlavorText = "A thousand decisions made. Each flip a heartbeat in the rhythm of chance.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "1500Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 1500,
                Description = "Complete 1,500 total flips",
                FlavorText = "The masses call it obsession. You call it dedication.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "2000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 2000,
                Description = "Complete 2,000 total flips",
                FlavorText = "Two thousand times the coin has danced. You've become one with its motion.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "5000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 5000,
                Description = "Complete 5,000 total flips",
                FlavorText = "Five thousand moments where fate hung in the balance. You were there for all of them.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "10000Flips.png", new UnlockCondition
            {
                Type = UnlockConditionType.TotalFlips,
                RequiredCount = 10000,
                Description = "Complete 10,000 total flips",
                FlavorText = "Ten thousand flips. Legends will be told of your devotion to the ancient art of the toss.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "3HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 3,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 3-heads streak",
                FlavorText = "Three heads in a row. Coincidence? Or is luck finally on your side?",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "4HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 4,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 4-heads streak",
                FlavorText = "Four consecutive heads. The crowd begins to take notice of your skill.",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "5HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 5-heads streak",
                FlavorText = "Five heads straight. At this point, you're either blessed by fate or defying it.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "10HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 10,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 10-heads streak",
                FlavorText = "Ten heads without faltering. The mathematicians are starting to get suspicious.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "50HeadStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 50,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 50-heads streak",
                FlavorText = "Fifty consecutive heads. Probability itself kneels before you.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "Headmaster.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 100,
                StreakSide = StreakSide.Heads,
                Description = "Achieve a 100-heads streak",
                FlavorText = "One hundred heads. You've transcended mere luck and entered the realm of legend.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "3TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 3,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 3-tails streak",
                FlavorText = "Three tails consecutive. The shadow side reveals itself.",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "4TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 4,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 4-tails streak",
                FlavorText = "Four tails straight. The underside of fortune favors you.",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "5TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 5,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 5-tails streak",
                FlavorText = "Five tails unbroken. You've found harmony in the coin's darker half.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "10TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 10,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 10-tails streak",
                FlavorText = "Ten tails in succession. Chaos smiles upon the audacious.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "50TailStreak.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 50,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 50-tails streak",
                FlavorText = "Fifty tails without wavering. The coin has chosen its champion.",
                Rarity = UnlockRarity.Legendary
            }
        },
        {
            "TailMaster.png", new UnlockCondition
            {
                Type = UnlockConditionType.Streak,
                RequiredCount = 100,
                StreakSide = StreakSide.Tails,
                Description = "Achieve a 100-tails streak",
                FlavorText = "One hundred tails. Master of shadows, keeper of the eternal flip's darker truth.",
                Rarity = UnlockRarity.Legendary
            }
        },
        { 
            "Completionist.png", new UnlockCondition 
            { 
                Type = UnlockConditionType.LandOnMultipleCoins, 
                RequiredCount = 1,
                UseDynamicCoinList = true, // Automatically includes all coins in the game
                Description = "Land on every other coin in the game at least once",
                FlavorText = "To have seen every face, touched every surface, spun every story. The collection is complete.",
                Rarity = UnlockRarity.Legendary
            }
        }
    };
    
    /// <summary>
    /// Get effects for each coin in this type
    /// </summary>
    public Dictionary<string, CoinEffect> GetCoinEffects() => new()
    {
        {
            "Headmaster.png", new CoinEffect
            {
                Type = CoinEffectType.AlwaysHeads,
                Description = "Always lands on heads - 100% bias towards heads"
            }
        },
        {
            "TailMaster.png", new CoinEffect
            {
                Type = CoinEffectType.AlwaysTails,
                Description = "Always lands on tails - 100% bias towards tails"
            }
        }
    };
}
