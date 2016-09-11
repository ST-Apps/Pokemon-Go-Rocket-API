using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using POGOProtos.Enums;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
    public class PokedexDetailViewModel : ViewModelBase
    {
        public int CapturedPokemons { get; set; } = 0;
        public int SeenPokemons { get; set; } = 0;
        public ObservableCollection<PokemonModel> PokedexItems { get; private set; } = new ObservableCollection<PokemonModel>();
        private PokemonModel selectedItem;
        public PokemonModel SelectedItem
        {
            get { return selectedItem; }
            set { Set(ref selectedItem, value); }
        }
        
        public void Load(PokemonId loadedId = PokemonId.Missingno)
        {
            var pokedexItems = GameClient.PokedexInventory;
            var lastPokemonIdSeen = pokedexItems == null || pokedexItems.Count == 0 ? 0 : pokedexItems.Max(x => (int)x.PokemonId);
            if (lastPokemonIdSeen > 0)
            {
                IEnumerable<PokemonId> listAllPokemon = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();

                foreach (PokemonId id in listAllPokemon.Where(t => t != PokemonId.Missingno && pokedexItems.Any(y => y.PokemonId == t)))
                {
                    var pokemonModel = new PokemonModel(id);

                    PokedexItems.Add(pokemonModel);
                }

                CapturedPokemons = pokedexItems.Count(x => x.TimesCaptured > 0);
                if (pokedexItems != null)
                {
                    SeenPokemons = pokedexItems.Count;
                }
            }
            else
            {
                CapturedPokemons = 0;
                SeenPokemons = 0;
            }

            if (loadedId != PokemonId.Missingno)
            {
                GoToPokemon(loadedId);
            }

            Debug.WriteLine("Loaded!");
        }

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
          =>
          _closeCommand ??
          (_closeCommand = new DelegateCommand(() =>
          {
              NavigationService.GoBack();
          }, () => true)
          );

        public void GoToPokemon(PokemonModel pokemon)
        {
            GoToPokemon(pokemon.Id);
        }

        public void GoToPokemon(PokemonId pokemon)
        {
            if (pokemon != PokemonId.Missingno)
            {
                SelectedItem = PokedexItems.First(t => t.Id == pokemon);
            }
        }
    }
}
