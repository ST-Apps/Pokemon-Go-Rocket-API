using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
            if (state.Any())
            {
                PokemonFoundAndSeen = (ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>)state[nameof(PokemonFoundAndSeen)];
                SeenPokemons = (int)state[nameof(SeenPokemons)];
                CapturedPokemons = (int)state[nameof(CapturedPokemons)];
            }
            else
            {
                var list = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
                var pokedexItems = GameClient.PokedexInventory;
                foreach (var item in list)
                {
                    switch (item)
                    {
                        case PokemonId.Missingno:
                            break;
                        default:
                            var pokedexEntry = pokedexItems.Where(x => x.PokemonId == item);
                            if(pokedexEntry.Count()==1)
                                PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, pokedexEntry.ElementAt(0)));
                            else
                                PokemonFoundAndSeen.Add(new KeyValuePair<PokemonId, PokedexEntry>(item, null));
                            break;
                    }
                }
                CapturedPokemons = pokedexItems.Where(x => x.TimesCaptured > 0).Count();
                SeenPokemons = pokedexItems.Count;
            }
            return Task.CompletedTask;
        }
        public override Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            if (suspending)
            {
                pageState[nameof(PokemonFoundAndSeen)] = PokemonFoundAndSeen;
                pageState[nameof(SeenPokemons)] = SeenPokemons;
                pageState[nameof(CapturedPokemons)] = CapturedPokemons;
            }
            else
            {
                pageState?.Clear();
                PokemonFoundAndSeen?.Clear();
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonFoundAndSeen { get; private set; } = 
            new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
            =>
            _closeCommand ??
            (_closeCommand = new DelegateCommand(() =>
                {
                    if (IsPokemonDetailsOpen)
                        IsPokemonDetailsOpen = false;
                    else
                        NavigationService.Navigate(typeof(GameMapPage));
                }, () => true)
            );
        private bool _pokemonDetailsOpen;
        public bool IsPokemonDetailsOpen { get { return _pokemonDetailsOpen; } set { Set(ref _pokemonDetailsOpen, value); } }
        #endregion
        private KeyValuePair<PokemonId, PokedexEntry> _selectedPokedex;
        public KeyValuePair<PokemonId, PokedexEntry> SelectedPokedexEntry
        {
            get { return _selectedPokedex; }
            set
            {
                if (IsPokemonDetailsOpen && value.Key == _selectedPokedex.Key)
                    return;
                Set(ref _selectedPokedex, value);
                PokemonDetails = GameClient.GetExtraDataForPokemon(value.Key);
                PopulateEvolutions();
            }
        }
        private PokemonSettings _pokemonDetails;
        public PokemonSettings PokemonDetails { get { return _pokemonDetails; } set { Set(ref _pokemonDetails, value); } }
        private DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> _openPokedexEntry;
        public DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>> OpenPokedexEntry =>
            _openPokedexEntry ??
            (_openPokedexEntry = new DelegateCommand<KeyValuePair<PokemonId, PokedexEntry>>(
                (x) => 
                    {
                        SelectedPokedexEntry = x;
                        RaisePropertyChanged(nameof(SelectedPokedexEntry));
                        IsPokemonDetailsOpen = true;
                    }, 
                    (x) => { return true; }
                )
            );
        public ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>> PokemonEvolutions { get; } =
            new ObservableCollection<KeyValuePair<PokemonId, PokedexEntry>>();
        private void PopulateEvolutions()
        {
            PokemonEvolutions.Clear();
            PokemonId InitPokemon = SelectedPokedexEntry.Key;
            PokemonSettings CurrPokemon = PokemonDetails;
            PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(InitPokemon, GetPokedexEntry(InitPokemon)));
            while (CurrPokemon.ParentPokemonId != PokemonId.Missingno)
            {
                PokemonEvolutions.Insert(0, new KeyValuePair<PokemonId, PokedexEntry>(CurrPokemon.ParentPokemonId, GetPokedexEntry(CurrPokemon.ParentPokemonId)));
                CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.ParentPokemonId);
            }
            CurrPokemon = PokemonDetails;
            while(CurrPokemon.EvolutionIds.Count > 0)
            {
                foreach (var ev in CurrPokemon.EvolutionIds) //for Eevee
                    PokemonEvolutions.Add(new KeyValuePair<PokemonId, PokedexEntry>(ev, GetPokedexEntry(ev)));
                CurrPokemon = GameClient.GetExtraDataForPokemon(CurrPokemon.EvolutionIds.ElementAt(0));
            }
        }
        private PokedexEntry GetPokedexEntry(PokemonId pokemon)
        {
            return PokemonFoundAndSeen.Where(x => x.Key == pokemon).ElementAt(0).Value;
        }
    }
}
