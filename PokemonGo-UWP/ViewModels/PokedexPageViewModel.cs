using POGOProtos.Enums;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers
        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var list = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>().ToList();
            var pokedexItems = GameClient.PokedexInventory;
            foreach (var item in list)
            {
                switch (item)
                {
                    case PokemonId.Missingno:
                        break;
                    default:
                        bool has = pokedexItems.Where(x => x.PokemonId == item).Count() > 0;
                        PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, bool>(item, has));
                        break;
                }
            }
            CapturedPokemons = pokedexItems.Where(x => x.TimesCaptured > 0).Count();
            SeenPokemons = pokedexItems.Count;

            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            PokemonFoundAndSeen.Clear();
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<KeyValuePair<PokemonId, bool>> PokemonFoundAndSeen { get; private set; } = 
            new ObservableCollection<KeyValuePair<PokemonId, bool>>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        #endregion
    }
}
