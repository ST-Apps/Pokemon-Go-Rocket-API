using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
                PokemonFoundAndSeen = (ObservableCollection<PokemonId>)state[nameof(PokemonFoundAndSeen)];
                SeenPokemons = (int)state[nameof(SeenPokemons)];
                CapturedPokemons = (int)state[nameof(CapturedPokemons)];
            }
            else
            {
                ObservableCollectionPlus<PokedexEntry> pokedexItems = GameClient.PokedexInventory;
                int lastPokemonIdSeen = pokedexItems == null || pokedexItems.Count == 0 ? 0 : pokedexItems.Max(x => (int)x.PokemonId);
                if (lastPokemonIdSeen > 0)
                {
                    var listAllPokemon = Enum.GetValues(typeof(PokemonId)).Cast<PokemonId>();
                    foreach (var item in listAllPokemon)
                    {
                        if ((int)item > lastPokemonIdSeen)
                        {
                            break;
                        }

                        switch (item)
                        {
                            case PokemonId.Missingno:
                                {
                                    break;
                                }
                            default:
                                {
                                    var pokedexEntry = pokedexItems.Where(x => x.PokemonId == item);
                                    PokemonFoundAndSeen.Add(item);
                                    break;
                                }
                        }
                    }

                    CapturedPokemons = pokedexItems.Count(x => x.TimesCaptured > 0);
                    SeenPokemons = pokedexItems.Count;
                }
                else
                {
                    CapturedPokemons = 0;
                    SeenPokemons = 0;
                }
            }

            if (parameter is PokemonId)
            {
                NavigationService.Navigate(typeof(PokedexDetailPage), (PokemonId)parameter);
            }

            NavigationService.FrameFacade.BackRequested += FrameFacade_BackRequested;
            return Task.CompletedTask;
        }

        private void FrameFacade_BackRequested(object sender, Template10.Common.HandledEventArgs e)
        {
            e.Handled = true;
            CloseCommand.Execute();
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
                EeveeEvolutions?.Clear();
                PokemonEvolutions?.Clear();
            }
            NavigationService.FrameFacade.BackRequested -= FrameFacade_BackRequested;
            return Task.CompletedTask;
        }
        #endregion

        #region Variables
        public ObservableCollection<PokemonId> PokemonFoundAndSeen { get; private set; } = new ObservableCollection<PokemonId>();
        private int _captured, _seen;
        public int CapturedPokemons { get { return _captured; } set { Set(ref _captured, value); } }
        public int SeenPokemons { get { return _seen; } set { Set(ref _seen, value); } }
        private PokemonSettings _pokemonDetails;
        public PokemonSettings PokemonDetails { get { return _pokemonDetails; } set { Set(ref _pokemonDetails, value); } }
        public ObservableCollection<PokemonId> PokemonEvolutions { get; } = new ObservableCollection<PokemonId>();
        public ObservableCollection<PokemonId> EeveeEvolutions { get; } = new ObservableCollection<PokemonId>();
        #endregion

        private DelegateCommand<PokemonId> _openPokedexEntry;
        public DelegateCommand<PokemonId> OpenPokedexEntry =>
            _openPokedexEntry ??
            (_openPokedexEntry = new DelegateCommand<PokemonId>(
                (id) =>
                {
                    if (GameClient.PokedexInventory.All(x => x.PokemonId != id)) return;
                    NavigationService.Navigate(typeof(PokedexDetailPage), id);
                },
                    (x) => true)
            );

        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand
            =>
            _closeCommand ??
            (_closeCommand = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            }, () => true)
            );
    }
}
