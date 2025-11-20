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
                Description = "2% chance to unlock per flip",
                FlavorText = "The river flows eternal, sometimes revealing treasures to those patient enough to watch.",
                Rarity = UnlockRarity.Common
            }
        },
        {
            "Winter.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.06,
                Description = "6% chance to unlock per flip",
                FlavorText = "Winter's first snow arrives without warning, blanketing the world in quiet possibility.",
                Rarity = UnlockRarity.Uncommon
            }
        },
        {
            "City.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.05,
                Description = "5% chance to unlock per flip",
                FlavorText = "In the urban sprawl, millions of stories unfold. This coin captures but one fleeting moment.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Chaos.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance to unlock per flip (requires 200 total flips first)",
                FlavorText = "From order emerges chaos. From chaos, beauty. The coin spins between both eternally.",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TotalFlips,
                        RequiredCount = 200,
                        Description = "Must have completed 200 total flips"
                    }
                }
            }
        },
        {
            "Lisa.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance to unlock per flip",
                FlavorText = "A mysterious smile captured in metal. What secrets does she keep behind those enigmatic eyes?",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Scenery.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.01, // 1% chance
                Description = "1% chance to unlock per flip",
                FlavorText = "A vista frozen in time, where mountains meet sky in perfect harmony.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Panda.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.001, // 0.1% chance
                Description = "0.1% chance to unlock per flip (ultra rare!)",
                FlavorText = "Gentle giant of the bamboo forests. To witness one is to witness rarity itself.",
                Rarity = UnlockRarity.Rare
            }
        },
        {
            "Zen.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.005, // 0.5% chance
                Description = "0.5% chance per flip when Panda is active (requires 500 total flips and Panda unlocked)",
                FlavorText = "Inner peace found through the rarest of companions. Tranquility and rarity unite.",
                Rarity = UnlockRarity.Rare,
                RequiresActiveCoin = true,
                RequiredCoinPath = "/img/coins/AI/Random/Panda.png",
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TotalFlips,
                        RequiredCount = 500,
                        Description = "Must have completed 500 total flips"
                    },
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "/img/coins/AI/Random/Panda.png",
                        RequiredCount = 1,
                        Description = "Must have Panda coin unlocked"

                    }
                }
            }
        },
         {
            "Brook.png", new UnlockCondition
            {
                Type = UnlockConditionType.RandomChance,
                UnlockChance = 0.01, // 1% chance
                Description = "1% chance per flip (requires Zen and Dragon_Rare unlocked)",
                FlavorText = "Where calm waters meet celestial fire, only the most dedicated seekers may find this treasure.",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "/img/coins/AI/Random/Zen.png",
                        RequiredCount = 1,
                        Description = "Must have Zen coin unlocked"
                    },
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.LandOnCoin,
                        RequiredCoinPath = "/img/coins/AI/Zodiak/Dragon_Rare.png",
                        RequiredCount = 1,
                        Description = "Must have Dragon_Rare coin unlocked"
                    }
                }
            }
        },
        {
            "Dillon.png", new UnlockCondition
            {
                Type = UnlockConditionType.None,
                Description = "Complete 100 heads flips and 100 tails flips (balanced dedication)",
                FlavorText = "A gift from Dillon, keeper of balance. Where heads and tails meet in perfect harmony, this treasure awaits.",
                Rarity = UnlockRarity.Rare,
                Prerequisites = new List<UnlockCondition>
                {
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.HeadsFlips,
                        RequiredCount = 100,
                        Description = "Must have landed heads 100 times"
                    },
                    new UnlockCondition
                    {
                        Type = UnlockConditionType.TailsFlips,
                        RequiredCount = 100,
                        Description = "Must have landed tails 100 times"
                    }
                }
            }
        }
    };
}
