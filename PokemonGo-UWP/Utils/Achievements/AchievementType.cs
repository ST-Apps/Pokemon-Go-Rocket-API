using POGOProtos.Enums;

namespace PokemonGo_UWP.Utils
{
    public enum AchievementType
    {
        [BadgeType(BadgeType.BadgeTravelKm)] [Bronze(10)] [Silver(100)] [Gold(1000)] Jogger,
        [BadgeType(BadgeType.BadgePokedexEntries)] [Bronze(5)] [Silver(50)] [Gold(100)] Kanto,
        [BadgeType(BadgeType.BadgeCaptureTotal)] [Bronze(30)] [Silver(500)] [Gold(2000)] Collector,
        [BadgeType(BadgeType.BadgeEvolvedTotal)] [Bronze(3)] [Silver(20)] [Gold(200)] Scientist,
        [BadgeType(BadgeType.BadgeHatchedTotal)] [Bronze(10)] [Silver(100)] [Gold(1000)] Breeder,
        [BadgeType(BadgeType.BadgePokestopsVisited)] [Bronze(100)] [Silver(1000)] [Gold(2000)] Backpacker,
        [BadgeType(BadgeType.BadgeBigMagikarp)] [Bronze(3)] [Silver(50)] [Gold(200)] Fisherman,
        [BadgeType(BadgeType.BadgeBattleAttackWon)] [Bronze(10)] [Silver(100)] [Gold(1000)] BattleGirl,
        [BadgeType(BadgeType.BadgeBattleTrainingWon)] [Bronze(10)] [Silver(100)] [Gold(1000)] AceTrainer,
        [BadgeType(BadgeType.Normal)] [Bronze(10)] [Silver(50)] [Gold(200)] SchoolKid,
        [BadgeType(BadgeType.Flying)] [Bronze(10)] [Silver(50)] [Gold(200)] BirdKeeper,
        [BadgeType(BadgeType.Poison)] [Bronze(10)] [Silver(50)] [Gold(200)] PunkGirl,
        [BadgeType(BadgeType.Bug)] [Bronze(10)] [Silver(50)] [Gold(200)] BugCatcher,
        [BadgeType(BadgeType.Ghost)] [Bronze(10)] [Silver(50)] [Gold(200)] HexManiac,
        [BadgeType(BadgeType.Water)] [Bronze(10)] [Silver(50)] [Gold(200)] Swimmer,
        [BadgeType(BadgeType.Grass)] [Bronze(10)] [Silver(50)] [Gold(200)] Gardener,
        [BadgeType(BadgeType.Electric)] [Bronze(10)] [Silver(50)] [Gold(200)] Rocker,
        [BadgeType(BadgeType.Psychic)] [Bronze(10)] [Silver(50)] [Gold(200)] Psychic,
        [BadgeType(BadgeType.Ice)] [Bronze(10)] [Silver(50)] [Gold(200)] Skier,
        [BadgeType(BadgeType.Ground)] [Bronze(10)] [Silver(50)] [Gold(200)] RuinManiac,
        [BadgeType(BadgeType.Fire)] [Bronze(10)] [Silver(50)] [Gold(200)] Kindler,
        [BadgeType(BadgeType.Fairy)] [Bronze(10)] [Silver(50)] [Gold(200)] FairyTaleGirl,
        [BadgeType(BadgeType.Dragon)] [Bronze(10)] [Silver(50)] [Gold(200)] DragonTamer,
        [BadgeType(BadgeType.BadgeSmallRattata)] [Bronze(3)] [Silver(50)] [Gold(200)] Youngster,
        [BadgeType(BadgeType.Steel)] [Bronze(10)] [Silver(50)] [Gold(200)] DepotAgent,
        [BadgeType(BadgeType.Rock)] [Bronze(10)] [Silver(50)] [Gold(200)] Hiker,
        [BadgeType(BadgeType.Fighting)] [Bronze(10)] [Silver(50)] [Gold(200)] BlackBelt
    }
}