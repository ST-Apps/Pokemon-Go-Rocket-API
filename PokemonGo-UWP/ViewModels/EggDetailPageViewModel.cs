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
using POGOProtos.Data.Capture;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class EggDetailPageViewModel : ViewModelBase
    {

        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter">MapPokemonWrapper containing the Pokemon that we're trying to capture</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentEgg = (PokemonDataWrapper)suspensionState[nameof(CurrentEgg)];
                SelectedEggIncubator = (EggIncubator)suspensionState[nameof(SelectedEggIncubator)];
            }
            else if (parameter is bool)
            {
                // Navigating from game page, so we need to actually load the encounter                
                CurrentEgg = (PokemonDataWrapper)NavigationHelper.NavigationState[nameof(CurrentEgg)];
                //Busy.SetBusy(true, Utils.Resources.Translation.GetString("LoadingEncounter") + Utils.Resources.Pokemon.GetString(CurrentEgg.PokemonId.ToString()));
                //NavigationHelper.NavigationState.Remove(nameof(CurrentEgg));
                //Logger.Write($"Catching {CurrentEgg.PokemonId}");
                //CurrentEncounter = await GameClient.EncounterPokemon(CurrentEgg.EncounterId, CurrentEgg.SpawnpointId);
                //SelectedEggIncubator = ItemsInventory.First(item => item.ItemId == ItemId.ItemPokeBall);
                //Busy.SetBusy(false);
                //if (CurrentEncounter.Status != EncounterResponse.Types.Status.EncounterSuccess)
                //{
                //    // Encounter failed, probably the Pokemon ran away
                //    await new MessageDialog(Utils.Resources.Translation.GetString("PokemonRanAway")).ShowAsyncQueue();
                //    ReturnToPokemonInventoryScreen.Execute();
                //}
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
                suspensionState[nameof(CurrentEgg)] = CurrentEgg;
                suspensionState[nameof(SelectedEggIncubator)] = SelectedEggIncubator;
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
        ///     Pokemon that we're trying to capture
        /// </summary>
        private PokemonDataWrapper _currentEgg;

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        private EggIncubator _selectedEggIncubator;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        /// Reference to global inventory
        /// </summary>
        public ObservableCollection<EggIncubator> IncubatorsInventory => GameClient.IncubatorsInventory;

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public PokemonDataWrapper CurrentEgg
        {
            get { return _currentEgg; }
            set { Set(ref _currentEgg, value); }
        }

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        public EggIncubator SelectedEggIncubator
        {
            get { return _selectedEggIncubator; }
            set { Set(ref _selectedEggIncubator, value); }
        }

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToPokemonInventoryScreen;

        /// <summary>
        ///     Going back to inventory page
        /// </summary>
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(PokemonInventoryPage));
            }, () => true)
            );

        #endregion

        #region Incubator

        #region Incubator Events

        /// <summary>
        /// Event fired if using the incubator returned Success
        /// </summary>
        public event EventHandler IncubatorSuccess;

        #endregion
        // TODO: disable button visibility ig egg has already an incubator        
        private DelegateCommand<EggIncubator> _useIncubatorCommand;

        public DelegateCommand<EggIncubator> UseIncubatorCommand => _useIncubatorCommand ?? (
            _useIncubatorCommand = new DelegateCommand<EggIncubator>(async incubator =>
            {
                var response = await GameClient.UseEggIncubator(incubator, CurrentEgg.WrappedData);
                switch (response.Result)
                {
                    case UseItemEggIncubatorResponse.Types.Result.Success:
                        IncubatorSuccess?.Invoke(this, null);
                        await GameClient.UpdateInventory();
                        CurrentEgg = new PokemonDataWrapper(GameClient.EggsInventory.First(item => item.Id == CurrentEgg.Id));                        
                        break;                    
                    default:
                        // TODO: user can only use one unlimited incubator at the same time, so we need to hide it 
                        Logger.Write($"Error using {incubator.Id} on {CurrentEgg.Id}");
                        break;
                }
            }, incubator => true));

        #endregion

        #endregion
    }
}
