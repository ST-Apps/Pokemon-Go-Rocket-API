using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Google.Protobuf;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data.Capture;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Resources = PokemonGo_UWP.Utils.Resources;
using POGOProtos.Settings.Master;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    ///     ViewModel that handles the Pokemon catching page
    /// </summary>
    public class CapturePokemonPageViewModel : ViewModelBase
    {
        #region ctor

        public CapturePokemonPageViewModel()
        {
            SelectedCaptureItem = SelectAvailablePokeBall();
        }

        #endregion

        #region Lifecycle Handlers

        /// <summary>
        /// Encounter logic
        /// </summary>
        /// <returns></returns>
        private async Task HandleEncounter()
        {
            if (CurrentPokemon is MapPokemonWrapper)
            {
                CurrentEncounter = await GameClient.EncounterPokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId);
                switch (CurrentEncounter.Status)
                {
                    case EncounterResponse.Types.Status.PokemonInventoryFull:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonInventoryFullText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case EncounterResponse.Types.Status.EncounterSuccess:
                        break;                    
                    case EncounterResponse.Types.Status.EncounterError:
                    case EncounterResponse.Types.Status.EncounterNotFound:
                    case EncounterResponse.Types.Status.EncounterClosed:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonEncounterErrorText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case EncounterResponse.Types.Status.EncounterNotInRange:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonEncounterNotInRangeText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case EncounterResponse.Types.Status.EncounterPokemonFled:
                    case EncounterResponse.Types.Status.EncounterAlreadyHappened:
                        await new MessageDialog(Resources.CodeResources.GetString("PokemonRanAwayText")).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        GameClient.CatchablePokemons.Remove((MapPokemonWrapper) CurrentPokemon);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                CurrentLureEncounter = await GameClient.EncounterLurePokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId);
                CurrentEncounter = new EncounterResponse()
                {
                    Background = EncounterResponse.Types.Background.Park,
                    WildPokemon = new WildPokemon()
                    {
                        PokemonData = CurrentLureEncounter.PokemonData
                    }
                };
                switch (CurrentLureEncounter.Result)
                {
                    case DiskEncounterResponse.Types.Result.PokemonInventoryFull:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonInventoryFullText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case DiskEncounterResponse.Types.Result.Success:
                        break;
                    case DiskEncounterResponse.Types.Result.Unknown:
                    case DiskEncounterResponse.Types.Result.NotAvailable:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonEncounterErrorText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case DiskEncounterResponse.Types.Result.NotInRange:
                        await new MessageDialog(string.Format(Resources.CodeResources.GetString("PokemonEncounterNotInRangeText"), Resources.Pokemon.GetString($"{CurrentPokemon.PokemonId}"))).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    case DiskEncounterResponse.Types.Result.EncounterAlreadyFinished:
                        await new MessageDialog(Resources.CodeResources.GetString("PokemonRanAwayText")).ShowAsyncQueue();
                        ReturnToGameScreen.Execute();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            PokemonExtraData = GameClient.GetExtraDataForPokemon(CurrentPokemon.PokemonId);
            SelectedCaptureItem = SelectAvailablePokeBall();
            Busy.SetBusy(false);
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter">MapPokemonWrapper containing the Pokemon that we're trying to capture</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentEncounter = new EncounterResponse();
                CurrentLureEncounter = new DiskEncounterResponse();
                CurrentCaptureAward = new CaptureAward();
                SelectedCaptureItem = new ItemData();
                CurrentPokemon = JsonConvert.DeserializeObject<IMapPokemon>((string) suspensionState[nameof(CurrentPokemon)]);
                CurrentEncounter.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentEncounter)]).CreateCodedInput());
                CurrentLureEncounter.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentLureEncounter)]).CreateCodedInput());
                CurrentCaptureAward.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentCaptureAward)]).CreateCodedInput());
                SelectedCaptureItem.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(SelectedCaptureItem)]).CreateCodedInput());
                RaisePropertyChanged(() => CurrentEncounter);
                RaisePropertyChanged(() => CurrentLureEncounter);
                RaisePropertyChanged(() => CurrentCaptureAward);
                RaisePropertyChanged(() => SelectedCaptureItem);
            }
            else
            {
                // Navigating from game page, so we need to actually load the encounter
                CurrentPokemon = (IMapPokemon) NavigationHelper.NavigationState[nameof(CurrentPokemon)];
                Busy.SetBusy(true, string.Format(Resources.CodeResources.GetString("LoadingEncounterText"), Resources.Pokemon.GetString(CurrentPokemon.PokemonId.ToString())));
                NavigationHelper.NavigationState.Remove(nameof(CurrentPokemon));
                Logger.Write($"Catching {CurrentPokemon.PokemonId}");
                await HandleEncounter();
            }
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
                suspensionState[nameof(CurrentPokemon)] = JsonConvert.SerializeObject(CurrentPokemon);
                suspensionState[nameof(CurrentEncounter)] = CurrentEncounter.ToByteString().ToBase64();
                suspensionState[nameof(CurrentLureEncounter)] = CurrentLureEncounter.ToByteString().ToBase64();
                suspensionState[nameof(CurrentCaptureAward)] = CurrentCaptureAward.ToByteString().ToBase64();
                suspensionState[nameof(SelectedCaptureItem)] = SelectedCaptureItem.ToByteString().ToBase64();
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
        /// Pokedex data for the current Pokemon
        /// </summary>
        private PokemonSettings _pokemonExtraData;

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        private IMapPokemon _currentPokemon;

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        private EncounterResponse _currentEncounter;

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        private DiskEncounterResponse _currentLureEncounter;

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        private ItemData _selectedCaptureItem;

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        private CaptureAward _currentCaptureAward;

        private bool _canUseBerry = true;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Reference to global inventory
        /// </summary>
        public ObservableCollection<ItemData> ItemsInventory => GameClient.CatchItemsInventory;

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public IMapPokemon CurrentPokemon
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
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        public DiskEncounterResponse CurrentLureEncounter
        {
            get { return _currentLureEncounter; }
            set { Set(ref _currentLureEncounter, value); }
        }

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        public ItemData SelectedCaptureItem
        {
            get { return _selectedCaptureItem; }
            set
            {
                // If user selected a berry we need to see if he can select it
                if (!_canUseBerry && value != null &&
                    (value.ItemId == ItemId.ItemRazzBerry || value.ItemId == ItemId.ItemBlukBerry ||
                     value.ItemId == ItemId.ItemNanabBerry || value.ItemId == ItemId.ItemWeparBerry ||
                     value.ItemId == ItemId.ItemPinapBerry))
                {
                    new MessageDialog(Resources.CodeResources.GetString("CantUseBerryText")).ShowAsyncQueue();
                    return;
                }
                Set(ref _selectedCaptureItem, value);
            }
        }

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        public CaptureAward CurrentCaptureAward
        {
            get { return _currentCaptureAward; }
            set { Set(ref _currentCaptureAward, value); }
        }

        /// <summary>
        /// Pokedex data for the current Pokemon
        /// </summary>
        public PokemonSettings PokemonExtraData
        {
            get { return _pokemonExtraData; }
            set { Set(ref _pokemonExtraData, value); }
        }

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (_returnToGameScreen = new DelegateCommand(
                                                         () =>
                                                         {
                                                             var currentPokemon =
                                                                 GameClient.PokemonsInventory
                                                                 .FirstOrDefault(item => item.Id == _capturedPokemonId);
                                                             if (currentPokemon != null)
                                                             {
                                                                 NavigationService.Navigate(typeof(PokemonDetailPage), new SelectedPokemonNavModel()
                                                                 {
                                                                     SelectedPokemonId = _capturedPokemonId.ToString(),
                                                                     ViewMode = PokemonDetailPageViewMode.ReceivedPokemon
                                                                 });
                                                             }
                                                             else
                                                             {
                                                                 NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.PokemonUpdate);
                                                             }
                                                         }, () => true));

        private DelegateCommand _escapeEncounterCommand;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand EscapeEncounterCommand => _escapeEncounterCommand ?? (_escapeEncounterCommand = new DelegateCommand(() =>
        {
            // Re-enable update timer
            GameClient.ToggleUpdateTimer();
            Dispatcher.Dispatch(() => NavigationService.GoBack());
        }, () => true));

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

        /// <summary>
        /// Event fired if berry worked
        /// </summary>
        public event EventHandler BerrySuccess;

        #endregion

        /// <summary>Selects the first available pokeball type</summary>
        /// <returns>Available pokeball type or default with count 0</returns>
        public ItemData SelectAvailablePokeBall()
        {
            var pokeball = SelectPokeballType(ItemId.ItemPokeBall);
            if(pokeball != null && pokeball.Count != 0)
                return pokeball;

            var greatball = SelectPokeballType(ItemId.ItemGreatBall);
            if (greatball != null && greatball.Count != 0)
                return greatball;

            var ultraball = SelectPokeballType(ItemId.ItemUltraBall);
            if (ultraball != null && ultraball.Count != 0)
                return ultraball;

            var masterball = SelectPokeballType(ItemId.ItemMasterBall);
            if (masterball != null && masterball.Count != 0)
                return masterball;

            var fallback = new ItemData {Count = 0, ItemId = ItemId.ItemPokeBall};
            return fallback;
        }

        /// <summary>Selects the pokeball type from Inventory</summary>
        /// <param name="itemId">The item identifier</param>
        /// <returns>Found balls or null</returns>
        private ItemData SelectPokeballType(ItemId? itemId)
        {
            if (itemId == null)
                return null;

            var ball = ItemsInventory?.FirstOrDefault(item => item.ItemId == itemId);
            if (ball == null || ball.Count <= 0)
                return null;

            return ball;
        }

        private DelegateCommand<bool> _useSelectedCaptureItem;
        public ItemId? LastItemUsed;

        private bool _pokeballButtonEnabled;
        public bool PokeballButtonEnabled
        {
            get { return _pokeballButtonEnabled; }
            set { Set(ref _pokeballButtonEnabled, value); }
        }

        /// <summary>
        ///     We throw the selected item to the Pokemon and see what happens
        /// </summary>
        public DelegateCommand<bool> UseSelectedCaptureItem => _useSelectedCaptureItem ?? (_useSelectedCaptureItem = new DelegateCommand<bool>(async hitPokemon =>
        {
            LastItemUsed = SelectedCaptureItem.ItemId;
            var catched = false;
            Logger.Write($"Launched {SelectedCaptureItem} at {CurrentPokemon.PokemonId}");
            if (SelectedCaptureItem.ItemId == ItemId.ItemPokeBall || SelectedCaptureItem.ItemId == ItemId.ItemGreatBall || SelectedCaptureItem.ItemId == ItemId.ItemMasterBall || SelectedCaptureItem.ItemId == ItemId.ItemUltraBall)
            {
                PokeballButtonEnabled = false;

                // Player's using a PokeBall so we try to catch the Pokemon
                catched = await ThrowPokeball(hitPokemon);
            }
            else
            {
                PokeballButtonEnabled = false;

                // He's using a berry
                await ThrowBerry();
            }

            if (SelectedCaptureItem != null && SelectedCaptureItem.Count > 0 && !catched)
                PokeballButtonEnabled = true;

            LastItemUsed = null;
        }, hitPokemon => true));

        private ulong _capturedPokemonId;

        /// <summary>
        ///     Launches the PokeBall for the current encounter, handling the different catch responses
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ThrowPokeball(bool hitPokemon)
        {
            // We use to simulate a 5 second wait to get animation going
            // If server takes too much to reply then we don't use the delay
            var requestTime = DateTime.Now;

            var caughtPokemonResponse = await GameClient.CatchPokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId, SelectedCaptureItem.ItemId, hitPokemon);

            await GameClient.UpdateInventory(); //TODO: Change to delta update inventory, so it doesn't take so long (and offico client does it too)
            SelectedCaptureItem = SelectPokeballType(LastItemUsed) ?? SelectAvailablePokeBall(); //To restore it after UpdateInventory, which overrides it

            var responseDelay = DateTime.Now - requestTime;
            if (responseDelay.TotalSeconds < 5 && hitPokemon)
                await Task.Delay(TimeSpan.FromSeconds(5 - (int) responseDelay.TotalSeconds));
            var nearbyPokemon = GameClient.NearbyPokemons.FirstOrDefault(pokemon => pokemon.EncounterId == CurrentPokemon.EncounterId);

            switch (caughtPokemonResponse.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    Logger.Write("CatchError!");
                    // TODO: what can we do?
                    break;

                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    Logger.Write($"We caught {CurrentPokemon.PokemonId}");
                    CurrentCaptureAward = caughtPokemonResponse.CaptureAward;
                    CatchSuccess?.Invoke(this, null);
                    _capturedPokemonId = caughtPokemonResponse.CapturedPokemonId;
                    if (CurrentPokemon is MapPokemonWrapper)
                        GameClient.CatchablePokemons.Remove((MapPokemonWrapper) CurrentPokemon);
                    else
                        GameClient.LuredPokemons.Remove((LuredPokemon) CurrentPokemon);
                    GameClient.NearbyPokemons.Remove(nearbyPokemon);
                    return true;

                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:
                    Logger.Write($"{CurrentPokemon.PokemonId} escaped");
                    CatchEscape?.Invoke(this, null);
                    _canUseBerry = true;
                    break;

                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    Logger.Write($"{CurrentPokemon.PokemonId} fled");
                    CatchFlee?.Invoke(this, null);
                    if (CurrentPokemon is MapPokemonWrapper)
                        GameClient.CatchablePokemons.Remove((MapPokemonWrapper) CurrentPokemon);
                    else
                        GameClient.LuredPokemons.Remove((LuredPokemon) CurrentPokemon);
                    GameClient.NearbyPokemons.Remove(nearbyPokemon);
                    // We just go back because there's nothing else to do
                    GameClient.ToggleUpdateTimer();
                    break;

                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    Logger.Write($"We missed {CurrentPokemon.PokemonId}");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }        

        /// <summary>
        ///     Uses the selected berry for the current encounter
        /// </summary>
        /// <returns></returns>
        private async Task ThrowBerry()
        {
            SelectedCaptureItem = SelectAvailablePokeBall(); //To set it immediatelly, because button image would be berry until responses
            Logger.Write($"Used {LastItemUsed}.");

            var berryResponse = await GameClient.UseCaptureItem(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId, LastItemUsed ?? ItemId.ItemRazzBerry);
            await GameClient.UpdateInventory(); //TODO: Change to delta update inventory, so it doesn't take so long (and offico client does it too)
            SelectedCaptureItem = SelectAvailablePokeBall(); //To restore it after UpdateInventory, which overrides it

            if (berryResponse.Success)
            {
                // TODO: visual feedback
                // TODO: do we need to handle the returned values or are they needed just to animate the 3d model?
                Logger.Write($"Success when using {LastItemUsed}.");
                BerrySuccess?.Invoke(this, null);
                _canUseBerry = false;
            }
            else
                Logger.Write($"Failure when using {LastItemUsed}.");
        }

        #endregion

        #endregion
    }
}