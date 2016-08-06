using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data.Capture;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    /// ViewModel that handles the Pokemon catching page
    /// </summary>
    public class CapturePokemonPageViewModel : ViewModelBase
    {

        #region ctor

        public CapturePokemonPageViewModel()
        {
            // Set default item
            SelectedCaptureItem = ItemsInventory.First(item => item.ItemId == ItemId.ItemPokeBall);
        }

        #endregion

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
                CurrentPokemon = (MapPokemonWrapper) suspensionState[nameof(CurrentPokemon)];                
                CurrentEncounter = (EncounterResponse) suspensionState[nameof(CurrentEncounter)];
                CurrentCaptureAward = (CaptureAward) suspensionState[nameof(CurrentCaptureAward)];
                SelectedCaptureItem =  (ItemData) suspensionState[nameof(SelectedCaptureItem)];
            } else if (parameter is bool)
            {                
                // Navigating from game page, so we need to actually load the encounter                
                CurrentPokemon = (MapPokemonWrapper) NavigationHelper.NavigationState[nameof(CurrentPokemon)];
                Busy.SetBusy(true, Utils.Resources.Translation.GetString("LoadingEncounter") + Utils.Resources.Pokemon.GetString(CurrentPokemon.PokemonId.ToString()));
                NavigationHelper.NavigationState.Remove(nameof(CurrentPokemon));
                Logger.Write($"Catching {CurrentPokemon.PokemonId}");                
                CurrentEncounter = await GameClient.EncounterPokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId);
                SelectedCaptureItem = ItemsInventory.First(item => item.ItemId == ItemId.ItemPokeBall);
                Busy.SetBusy(false);
                if (CurrentEncounter.Status != EncounterResponse.Types.Status.EncounterSuccess)
                {
                    // Encounter failed, probably the Pokemon ran away
                    await new MessageDialog(Utils.Resources.Translation.GetString("PokemonRanAway")).ShowAsyncQueue();
                    ReturnToGameScreen.Execute();
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
                suspensionState[nameof(CurrentPokemon)] = CurrentPokemon;
                suspensionState[nameof(CurrentEncounter)] = CurrentEncounter;
                suspensionState[nameof(CurrentCaptureAward)] = CurrentCaptureAward;
                suspensionState[nameof(SelectedCaptureItem)] = SelectedCaptureItem;
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
        private MapPokemonWrapper _currentPokemon;

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        private EncounterResponse _currentEncounter;

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        private ItemData _selectedCaptureItem;

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        private CaptureAward _currentCaptureAward;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        /// Reference to global inventory
        /// </summary>
        public ObservableCollection<ItemData> ItemsInventory => GameClient.ItemsInventory;

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public MapPokemonWrapper CurrentPokemon
        {
            get { return _currentPokemon; }
            set { Set(ref _currentPokemon, value); }
        }

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        public EncounterResponse CurrentEncounter
        {
            get { return _currentEncounter; }
            set { Set(ref _currentEncounter, value); }
        }

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        public ItemData SelectedCaptureItem
        {
            get { return _selectedCaptureItem; }
            set { Set(ref _selectedCaptureItem, value); }
        }

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        public CaptureAward CurrentCaptureAward
        {
            get { return _currentCaptureAward; }
            set { Set(ref _currentCaptureAward, value); }
        }

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (
            _returnToGameScreen = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(GameMapPage));
            }, () => true)
            );

        #endregion

        #region Pokemon Catching

        #region Catching Events

        /// <summary>
        ///     Event fired if the user was able to catch the Pokemon
        /// </summary>
        public event EventHandler CatchSuccess;

        /// <summary>
        ///     Event fired if the Pokemon flees
        /// </summary>
        public event EventHandler CatchFlee;

        /// <summary>
        ///     Event fired if the Pokemon escapes
        /// </summary>
        public event EventHandler CatchEscape;

        #endregion

        private DelegateCommand<bool> _useSelectedCaptureItem;

        /// <summary>
        ///     We throw the selected item to the Pokemon and see what happens
        /// </summary>
        public DelegateCommand<bool> UseSelectedCaptureItem => _useSelectedCaptureItem ?? (
            _useSelectedCaptureItem = new DelegateCommand<bool>(async (hitPokemon) =>
            {
                Logger.Write($"Launched {SelectedCaptureItem} at {CurrentPokemon.PokemonId}");                
                if (SelectedCaptureItem.ItemId == ItemId.ItemPokeBall || SelectedCaptureItem.ItemId == ItemId.ItemGreatBall || SelectedCaptureItem.ItemId == ItemId.ItemMasterBall || SelectedCaptureItem.ItemId == ItemId.ItemUltraBall)
                {
                    // Player's using a PokeBall so we try to catch the Pokemon
                    await ThrowPokeball(hitPokemon);
                }
                else
                {
                    // TODO: check if player can only use a ball or a berry during an encounter, and maybe avoid displaying useless items in encounter's inventory
                    // He's using a berry
                    await ThrowBerry();
                }
                // Update selected item to get the new item count
                SelectedCaptureItem = ItemsInventory.First(item => item.ItemId == SelectedCaptureItem.ItemId);
                Busy.SetBusy(false);
            }, (hitPokemon) => true));

        /// <summary>
        ///     Launches the PokeBall for the current encounter, handling the different catch responses
        /// </summary>
        /// <returns></returns>
        private async Task ThrowPokeball(bool hitPokemon)
        {            
            var caughtPokemonResponse =
                await
                    GameClient.CatchPokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId, SelectedCaptureItem.ItemId, hitPokemon);
            switch (caughtPokemonResponse.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    Logger.Write("CatchError!");
                    // TODO: what can we do?
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    CurrentCaptureAward = caughtPokemonResponse.CaptureAward;
                    Logger.Write($"We caught {CurrentPokemon.PokemonId}");
                    CatchSuccess?.Invoke(this, null);
                    await GameClient.UpdateInventory();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:                    
                    Logger.Write($"{CurrentPokemon.PokemonId} escaped");
                    CatchEscape?.Invoke(this, null);                                        
                    await GameClient.UpdateInventory();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    Logger.Write($"{CurrentPokemon.PokemonId} fleed");
                    CatchFlee?.Invoke(this, null);
                    await new MessageDialog(Utils.Resources.Pokemon.GetString(CurrentPokemon.PokemonId.ToString()) + Utils.Resources.Translation.GetString("Fleed")).ShowAsyncQueue();
                    await GameClient.UpdateInventory();
                    ReturnToGameScreen.Execute();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    Logger.Write($"We missed {CurrentPokemon.PokemonId}");
                    await GameClient.UpdateInventory();
                    CatchFlee?.Invoke(this, null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Uses the selected berry for the current encounter
        ///     TODO: what happens when the berry is used? Do we need some kind of animation or visual feedback?
        /// </summary>
        /// <returns></returns>
        public async Task ThrowBerry()
        {
            if (SelectedCaptureItem == null)
                return;
            //var berryResult =
            //    await
            //        GameClient.UseCaptureItem(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId, (ItemId)SelectedCaptureItem.Item_);
            //Logger.Write($"Used {SelectedCaptureItem}. Remaining: {SelectedCaptureItem.Count}");
        }

        #endregion

        #endregion

    }
}
