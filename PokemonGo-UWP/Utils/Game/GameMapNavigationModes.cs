using PokemonGo_UWP.ViewModels;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     We use this enum to tell to <see cref="GameMapPageViewModel" /> what's the action that generated the navigation, so
    ///     that we can handle it without needing ugly workarounds.
    /// </summary>
    public enum GameMapNavigationModes
    {
        // Navigating from App.cs when the app is launched
        AppStart,
        // Navigating from Settings page and requiring an update        
        SettingsUpdate,
        // Navigating from Pokemon catching page and requiring an update
        PokemonUpdate,
        // Navigating from Pokestop searching page and requiring an update
        PokestopUpdate
    }
}