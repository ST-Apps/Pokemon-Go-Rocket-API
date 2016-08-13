using System.Collections.Generic;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Serialization fails when passing parameters, so we need to do this in a different way
    /// </summary>
    public static class NavigationHelper
    {
        /// <summary>
        ///     Dictionary used to store navigation items
        /// </summary>
        public static Dictionary<object, object> NavigationState { get; } = new Dictionary<object, object>();
    }
}