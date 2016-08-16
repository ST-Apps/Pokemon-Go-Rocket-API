using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace PokemonGo_UWP.ViewModels
{
    public class ItemsInventoryPageViewModel : ViewModelBase
    {
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
                ItemsInventory = (ObservableCollection<ItemDataWrapper>) suspensionState[nameof(ItemsInventory)];
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the inventory
                // The sorting mode is directly bound to the settings
                ItemsInventory = new ObservableCollection<ItemDataWrapper>(
                    GameClient.ItemsInventory.Select(itemData => new ItemDataWrapper(itemData)));

                RaisePropertyChanged(() => ItemsInventory);
            }

            ItemsTotalCount = ItemsInventory.Sum(i => i.WrappedData.Count);

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
                suspensionState[nameof(ItemsInventory)] = ItemsInventory;
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
        ///     Reference to Items inventory
        /// </summary>
        public ObservableCollection<ItemDataWrapper> ItemsInventory { get; private set; } =
            new ObservableCollection<ItemDataWrapper>();

        private int _itemsTotalCount;
        public int ItemsTotalCount
        {
            get { return _itemsTotalCount; }
            private set { Set(ref _itemsTotalCount, value); }
        }

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

        public int MaxItemStorageFieldNumber => GameClient.PlayerProfile.MaxItemStorage;

        #endregion

        #endregion
    }
}