using POGOProtos.Data;
using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Utils
{
    public class PokedexEntryDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PokemonCaptured { get; set; }
        public DataTemplate PokemonSeen { get; set; }
        public DataTemplate PokemonUnseen { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            KeyValuePair<PokemonId, PokedexEntry> pokemon = (KeyValuePair<PokemonId, PokedexEntry>)item;
            if (pokemon.Value == null)
                return PokemonUnseen;
            else if (pokemon.Value.TimesEncountered > 0 && pokemon.Value.TimesCaptured == 0)
                return PokemonSeen;
            else
                return PokemonCaptured;
        }
    }
}
