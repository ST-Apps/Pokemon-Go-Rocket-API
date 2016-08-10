using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Template10.Utils;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonInventoryPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// 
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
                PokemonInventory = (ObservableCollection<PokemonDataWrapper>) suspensionState[nameof(PokemonInventory)];                                
                EggsInventory = (ObservableCollection<PokemonDataWrapper>)suspensionState[nameof(EggsInventory)];
                
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the inventory
                // The sorting mode is directly bound to the settings
                PokemonInventory = new ObservableCollection<PokemonDataWrapper>(GetSortedPokemonCollection(
                        GameClient.PokemonsInventory.Select(pokemonData => new PokemonDataWrapper(pokemonData)), CurrentPokemonSortingMode));

                RaisePropertyChanged(() => PokemonInventory);

                foreach (var pokemonData in GameClient.EggsInventory)
                {
                    EggsInventory.Add(new PokemonDataWrapper(pokemonData));
                }                    
                
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(PokemonInventory)] = PokemonInventory;
                suspensionState[nameof(EggsInventory)] = EggsInventory;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     Sorting mode for current Pokemon view
        /// </summary>
        private PokemonSortingModes _currentPokemonSortingMode;

        /// <summary>
        /// Egg selected for incubation
        /// </summary>
        private PokemonData _selectedEgg;

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
                this.RaisePropertyChanged(nameof(CurrentPokemonSortingMode));

                // When this changes we need to sort the collection again     
                UpdateSorting();           
            }
        }

        /// <summary>
        /// Egg selected for incubation
        /// </summary>
        public PokemonData SelectedEgg
        {
            get { return _selectedEgg; }
            set { Set(ref _selectedEgg, value); }
        }

        /// <summary>
        /// Reference to Pokemon inventory
        /// </summary>
        public ObservableCollection<PokemonDataWrapper> PokemonInventory { get; private set; } = new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        /// Reference to Eggs inventory
        /// </summary>
        public ObservableCollection<PokemonDataWrapper> EggsInventory { get; private set; } = new ObservableCollection<PokemonDataWrapper>();

        /// <summary>
        /// Reference to Incubators inventory
        /// </summary>
        public ObservableCollection<EggIncubator> IncubatorsInventory => GameClient.FreeIncubatorsInventory;

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (_returnToGameScreen = new DelegateCommand(() => { NavigationService.Navigate(typeof(GameMapPage)); }, () => true));

        #endregion

        #region Pokemon Inventory Handling        

        private void UpdateSorting()
        {
            PokemonInventory = new ObservableCollection<PokemonDataWrapper>(GetSortedPokemonCollection(PokemonInventory, CurrentPokemonSortingMode));
                  
            RaisePropertyChanged(() => PokemonInventory);
        }

        /// <summary>
        /// Returns a new ObservableCollection of the pokemonInventory sorted by the sortingMode
        /// </summary>
        /// <param name="pokemonInventory">Original inventory</param>
        /// <param name="sortingMode">Sorting Mode</param>
        /// <returns>A new ObservableCollection of the pokemonInventory sorted by the sortingMode</returns>
        private static IEnumerable<PokemonDataWrapper> GetSortedPokemonCollection(
            IEnumerable<PokemonDataWrapper> pokemonInventory, PokemonSortingModes sortingMode)
        {
            switch (sortingMode)
            {
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyDate")):
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.CreationTimeMs));
                    break;
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyFav")):
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Favorite));
                    break;
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyNumber")):
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderBy(pokemon => pokemon.PokemonId));
                    break;
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyHealth")):
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Stamina));
                    break;
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyName")):
                    PokemonInventory =
                        new ObservableCollection<PokemonData>(
                            PokemonInventory.OrderBy(pokemon => pokemon.PokemonId.ToString()));
                    break;
                case PokemonSortingModes.(Utils.Resources.Translation.GetString("SortbyCombat")):
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Cp));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentPokemonSortingMode), CurrentPokemonSortingMode, null);
            }
        }

        #endregion

        #endregion
    }
}
