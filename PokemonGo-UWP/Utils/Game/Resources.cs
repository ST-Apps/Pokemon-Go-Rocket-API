using Windows.ApplicationModel.Resources;

namespace PokemonGo_UWP.Utils
{
    public static class Resources
    {
        public static readonly ResourceLoader Pokemon = ResourceLoader.GetForCurrentView("Pokemon");
        public static readonly ResourceLoader Items = ResourceLoader.GetForCurrentView("Items");
        public static readonly ResourceLoader CodeResources = ResourceLoader.GetForCurrentView("CodeResources");
        public static readonly ResourceLoader Achievements = ResourceLoader.GetForCurrentView("Achievements");
        public static readonly ResourceLoader PokemonMoves = ResourceLoader.GetForCurrentView("PokemonMoves");
        public static readonly ResourceLoader Pokedex = ResourceLoader.GetForCurrentView("Pokedex");
    }
}