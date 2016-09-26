using PokemonGo_UWP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils.Extensions
{
    /// <summary>
    /// IEnumerable<PokemonDataWrapper> extension methods
    /// </summary>
    public static class IEnumerablePokemonExtensions
    {
        /// <summary>
        /// Sorts the IEnumerable with the sortingmode passed
        /// </summary>
        /// <param name="container"></param>
        /// <param name="sortingMode"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortBySortingmode(this IEnumerable<PokemonDataWrapper> container, PokemonSortingModes sortingMode)
        {
            switch (sortingMode)
            {
                case PokemonSortingModes.Date:
                    return container.SortByDate();
                case PokemonSortingModes.Fav:
                    return container.SortByFav();
                case PokemonSortingModes.Number:
                    return container.SortByNumber();
                case PokemonSortingModes.Health:
                    return container.SortByHealth();
                case PokemonSortingModes.Name:
                    return container.SortByName();
                case PokemonSortingModes.Combat:
                    return container.SortByCp();
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortingMode), sortingMode, null);
            }
        }

        /// <summary>
        /// Sorts the IENumerable by catchdate of the Pokémon
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByDate(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderByDescending(pokemon => pokemon.CreationTimeMs);
        }

        /// <summary>
        /// Sorts the IENumerable by favorite state of the Pokémon, then by CP
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByFav(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderByDescending(pokemon => pokemon.Favorite).ThenByDescending(pokemon => pokemon.Cp);
        }

        /// <summary>
        /// Sorts the IENumerable by number of the Pokémon, then by CP
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByNumber(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderBy(pokemon => pokemon.PokemonId).ThenByDescending(pokemon => pokemon.Cp);
        }

        /// <summary>
        /// Sorts the IENumerable by health of the Pokémon, then by CP
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByHealth(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderByDescending(pokemon => pokemon.Stamina).ThenByDescending(pokemon => pokemon.Cp);
        }

        /// <summary>
        /// Sorts the IENumerable by name of the Pokémon, then by CP
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByName(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderBy(pokemon => pokemon.Name).ThenByDescending(pokemon => pokemon.Cp);
        }

        /// <summary>
        /// Sorts the IENumerable by CP of the Pokémon, then by Name, then by ID
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<PokemonDataWrapper> SortByCp(this IEnumerable<PokemonDataWrapper> container)
        {
            return container.OrderByDescending(pokemon => pokemon.Cp).ThenBy(pokemon => pokemon.Name).ThenBy(pokemon => Resources.Pokemon.GetString(pokemon.PokemonId.ToString()));
        }
    }
}
