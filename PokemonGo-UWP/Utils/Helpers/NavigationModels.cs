using PokemonGo_UWP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
    public class SelectedPokemonNavModel
    {
        public string SelectedPokemonId { get; set; }
        public PokemonSortingModes SortingMode { get; set; }
        public Views.PokemonDetailPageViewMode ViewMode { get; set; }
    }
}
