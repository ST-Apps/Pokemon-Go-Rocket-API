using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Inventory;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonInventoryPageViewModel : ViewModelBase
    {
        #region Game Management Vars

        /// <summary>
        ///     Egg selected for incubation
        /// </summary>
        private PokemonData _selectedEgg;

        #endregion

        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                PokemonInventory = JsonConvert.DeserializeObject<ObservableCollection<PokemonDataWrapper>>((string)suspensionState[nameof(PokemonInventory)]);
                EggsInventory = JsonConvert.DeserializeObject<ObservableCollection<PokemonDataWrapper>>((string)suspensionState[nameof(EggsInventory)]);
            }
            else
            {
                // Navigating from game page, so we need to actually load the inventory
                // The sorting mode is directly bound to the settings
                PokemonInventory = new ObservableCollection<PokemonDataWrapper>(GetSortedPokemonCollection(
                    GameClient.PokemonsInventory.Select(pokemonData => new PokemonDataWrapper(pokemonData)),
                    CurrentPokemonSortingMode));

                RaisePropertyChanged(() => PokemonInventory);

                var unincubatedEggs = GameClient.EggsInventory.Where(o => string.IsNullOrEmpty(o.EggIncubatorId))
                                                              .OrderBy(c => c.EggKmWalkedTarget);
                var incubatedEggs = GameClient.EggsInventory.Where(o => !string.IsNullOrEmpty(o.EggIncubatorId))
                                                              .OrderBy(c => c.EggKmWalkedTarget);
                EggsInventory.Clear();                
                // advancedrei: I have verified this is the sort order in the game.
                foreach (var incubatedEgg in incubatedEggs)
                {
                    var incubatorData = GameClient.UsedIncubatorsInventory.FirstOrDefault(incubator => incubator.Id == incubatedEgg.EggIncubatorId);
                    EggsInventory.Add(new IncubatedEggDataWrapper(incubatorData, GameClient.PlayerStats.KmWalked, incubatedEgg));
                }

                foreach (var pokemonData in unincubatedEggs)
                {
                    EggsInventory.Add(new PokemonDataWrapper(pokemonData));
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(PokemonInventory)] = JsonConvert.SerializeObject(PokemonInventory);
                suspensionState[nameof(EggsInventory)] = JsonConvert.SerializeObject(EggsInventory);
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Sorting mode for current Pokemon view
        /// </summary>
        public PokemonSortingModes CurrentPokemonSortingMode
        {
            get { return SettingsService.Instance.PokemonSortingMode; }
            set
            {
                SettingsService.Instance.PokemonSortingMode = value;
                RaisePropertyChanged(nameof(CurrentPokemonSortingMode));

                // When this changes we need to sort the collection again
                UpdateSorting();
            }
        }

        /// <summary>
        ///     Egg selected for incubation
        /// </summary>
        public PokemonData SelectedEgg
        {
            get { return _selectedEgg; }
            set { Set(ref _selectedEgg, value); }
        }

        /// <summary>
        ///     Reference to Pokemon inventory
        /// </summary>
        public ObservableCollection<PokemonDataWrapper> PokemonInventory { get; private set; } =
            new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        ///     Reference to Eggs inventory
        /// </summary>
        public ObservableCollection<PokemonDataWrapper> EggsInventory { get; private set; } =
            new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        ///     Reference to Incubators inventory
        /// </summary>
        public ObservableCollection<EggIncubator> IncubatorsInventory => GameClient.FreeIncubatorsInventory;

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen
            =>
                _returnToGameScreen ??
                (_returnToGameScreen =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(GameMapPage)); }, () => true));

        #endregion

        #region Pokemon Inventory Handling

        private void UpdateSorting()
        {
            PokemonInventory =
                new ObservableCollection<PokemonDataWrapper>(GetSortedPokemonCollection(PokemonInventory,
                    CurrentPokemonSortingMode));

            RaisePropertyChanged(() => PokemonInventory);
        }

        /// <summary>
        ///     Returns a new ObservableCollection of the pokemonInventory sorted by the sortingMode
        /// </summary>
        /// <param name="pokemonInventory">Original inventory</param>
        /// <param name="sortingMode">Sorting Mode</param>
        /// <returns>A new ObservableCollection of the pokemonInventory sorted by the sortingMode</returns>
        private static IEnumerable<PokemonDataWrapper> GetSortedPokemonCollection(
            IEnumerable<PokemonDataWrapper> pokemonInventory, PokemonSortingModes sortingMode)
        {
            switch (sortingMode)
            {
                case PokemonSortingModes.Date:
                    return pokemonInventory.OrderByDescending(pokemon => pokemon.CreationTimeMs);
                case PokemonSortingModes.Fav:
                    return pokemonInventory.OrderByDescending(pokemon => pokemon.Favorite)
                         .ThenByDescending(pokemon => pokemon.Cp); 
                case PokemonSortingModes.Number:
                    return pokemonInventory.OrderBy(pokemon => pokemon.PokemonId)
                         .ThenByDescending(pokemon => pokemon.Cp); 
                case PokemonSortingModes.Health:
                    return pokemonInventory.OrderByDescending(pokemon => pokemon.Stamina)
                        .ThenByDescending(pokemon => pokemon.Cp);
                case PokemonSortingModes.Name:
                    return pokemonInventory.OrderBy(pokemon => Resources.Pokemon.GetString(pokemon.PokemonId.ToString()))
                            .ThenByDescending(pokemon => pokemon.Cp);
                case PokemonSortingModes.Combat:
                    return pokemonInventory.OrderByDescending(pokemon => pokemon.Cp)
                        .ThenBy(pokemon => Resources.Pokemon.GetString(pokemon.PokemonId.ToString()));
                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentPokemonSortingMode), sortingMode, null);
            }
        }

        #endregion

        #endregion
    }
}
