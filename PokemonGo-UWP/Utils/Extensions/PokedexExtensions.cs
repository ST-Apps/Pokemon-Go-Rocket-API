using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Data;
using POGOProtos.Enums;

namespace PokemonGo_UWP.Utils.Extensions
{
    public static class PokedexExtensions
    {
        public static PokedexEntry GetPokedexEntry(this PokemonId pokemon, ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen)
        {
            var found = PokemonFoundAndSeen.Where(x => x.Key == pokemon);
            if (found != null && found.Count() > 0)
                return found.ElementAt(0).Value;
            return null;
        }

        public static PokedexEntry GetPokedexEntry(this ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen, PokemonId pokemon)
        {
            var found = PokemonFoundAndSeen.Where(x => x.Key == pokemon);
            if (found != null && found.Count() > 0)
                return found.ElementAt(0).Value;
            return null;
        }
    }
}
