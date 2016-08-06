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
                PokemonInventory = (ObservableCollection<PokemonData>) suspensionState[nameof(PokemonInventory)];                                
                EggsInventory = (ObservableCollection<PokemonData>)suspensionState[nameof(EggsInventory)];
                CurrentPokemonSortingMode = (PokemonSortingModes) suspensionState[nameof(CurrentPokemonSortingMode)];
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the inventory and set default sorting mode             
                PokemonInventory.AddRange(GameClient.PokemonsInventory);
                EggsInventory.AddRange(GameClient.EggsInventory);
                CurrentPokemonSortingMode = PokemonSortingModes.Combat;
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
                suspensionState[nameof(CurrentPokemonSortingMode)] = CurrentPokemonSortingMode;
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

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Sorting mode for current Pokemon view
        /// </summary>
        public PokemonSortingModes CurrentPokemonSortingMode
        {
            get { return _currentPokemonSortingMode; }
            set
            {
                Set(ref _currentPokemonSortingMode, value);
                // When this changes we need to sort the collection again     
                UpdateSorting();           
            }
        }

        /// <summary>
        /// Reference to Pokemon inventory
        /// </summary>
        public ObservableCollection<PokemonData> PokemonInventory { get; private set; } = new ObservableCollection<PokemonData>();

        /// <summary>
        /// Reference to Eggs inventory
        /// </summary>
        public ObservableCollection<PokemonData> EggsInventory { get; private set; } = new ObservableCollection<PokemonData>();

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
            switch (CurrentPokemonSortingMode)
            {
                case PokemonSortingModes.Date:
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.CreationTimeMs));
                    break;
                case PokemonSortingModes.Fav:
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Favorite));
                    break;
                case PokemonSortingModes.Number:
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderBy(pokemon => pokemon.PokemonId));
                    break;
                case PokemonSortingModes.Health:
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Stamina));
                    break;
                case PokemonSortingModes.Name:
                    PokemonInventory =
                        new ObservableCollection<PokemonData>(
                            PokemonInventory.OrderBy(pokemon => pokemon.PokemonId.ToString()));
                    break;
                case PokemonSortingModes.Combat:
                    PokemonInventory = new ObservableCollection<PokemonData>(PokemonInventory.OrderByDescending(pokemon => pokemon.Cp));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentPokemonSortingMode), CurrentPokemonSortingMode, null);
            }            
            RaisePropertyChanged(() => PokemonInventory);
        }

        #endregion

        #endregion
    }
}
