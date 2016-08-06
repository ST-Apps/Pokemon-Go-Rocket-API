
using Windows.ApplicationModel.Resources;

namespace PokemonGo_UWP.Utils {
    public static class Resources {
        public static ResourceLoader Pokemon = ResourceLoader.GetForCurrentView("Pokemon");
        public static ResourceLoader Items = ResourceLoader.GetForCurrentView("Items");
        public static ResourceLoader Translation = ResourceLoader.GetForCurrentView("Resources");
    }
}
