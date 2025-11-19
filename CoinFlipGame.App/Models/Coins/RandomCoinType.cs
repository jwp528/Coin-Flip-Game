using CoinFlipGame.App.Models.Unlocks;

namespace CoinFlipGame.App.Models;

/// <summary>
/// Random/Mystery themed coins
/// </summary>
public class RandomCoinType : CoinType
{
    public override string Name { get; set; } = "Random";
    public override string BasePath { get; set; } = "/img/coins/AI/Random";
    public override string Category { get; set; } = "AI";

    public override List<string> GetCoinFiles() => new()
    {
        "River.png",
        "Winter.png",
        "City.png",
        "Chaos.png",
        "Lisa.png",
        "Zen.png",
        "Scenery.png",
        "Panda.png",
        "Brook.png",
        "Dillon.png"
    };

    public override Dictionary<string, UnlockCondition> GetUnlockConditions() => new()
    {
        {
            "River.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.02,
                Description = "2% chance to unlock on any flip",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Winter.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.06,
                Description = "6% chance to unlock on any flip",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "City.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.05,
                Description = "5% chance to unlock on any flip",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Chaos.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance to unlock on any flip",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TotalFlips,
                        RequiredCount = 200,
                        Description = "Flip 200 times"
                    }
                }
            }
        },
        {
            "Lisa.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance to unlock on any flip",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Scenery.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.01, // 1% chance
                Description = "1% chance to unlock on any flip",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Panda.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.001, // 0.1% chance
                Description = "0.1% chance to unlock on any flip",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Zen.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance to unlock after 500 flips while Panda is active",
                Rarity = UnlockRarity.Rare,
                RequiresActiveCoin = true,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TotalFlips,
                        RequiredCount = 500,
                        Description = "500 total flips"
                    },
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "Panda.png",
                        RequiredCount = 1,
                        Description = "Have the Panda coin active"

                    }
                }
            }
        },
         {
            "Brook.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.01, // 1% chance
                Description = "1% chance to unlock on any flip. The Dillon coin.",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPaths = new List<string>
                        {
                            "/img/coins/AI/Random/Zen.png",
                            "/img/coins/AI/Zodiak/Dragon_Rare.png"
                        },
                        RequiredCount = 1,
                        Description = "Must have unlocked Dragon_Rare and Zen coins."
                    }
                }
            }
        },
         {
            "Dillon.png", new UnlockCondition
            {
                Type = UnlockConditionType.None,
                Description = "The Dillon coin! Thank you for the coin Dillon it's VERY nice!",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.HeadsFlips,
                        RequiredCount = 100,
                        Description = "Flip heads 100 times."
                    },
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TailsFlips,
                        RequiredCount = 100,
                        Description = "Flip tails 100 times."
                    }
                }
            }
        }
    };
}
