using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Google.Protobuf;

namespace PokemonGo_UWP.ViewModels
{
    public class SearchPokeStopPageViewModel : ViewModelBase
    {
        #region ctor

        public SearchPokeStopPageViewModel()
        {
            SelectedModifierItem = SelectAvailableModifier();
        }

        #endregion

        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter">FortData containing the Pokestop that we're visiting</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state     
                CurrentPokestopInfo = new FortDetailsResponse();           
                CurrentSearchResponse = new FortSearchResponse();
                SelectedModifierItem = new ItemData();
                CurrentAddModifierResponse = new AddFortModifierResponse();
                CurrentPokestop = JsonConvert.DeserializeObject<FortDataWrapper>((string)suspensionState[nameof(CurrentPokestop)]);
                CurrentPokestopInfo.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentPokestop)]).CreateCodedInput());
                CurrentSearchResponse.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentSearchResponse)]).CreateCodedInput());
                SelectedModifierItem.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(SelectedModifierItem)]).CreateCodedInput());
                CurrentAddModifierResponse.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentAddModifierResponse)]).CreateCodedInput());
                RaisePropertyChanged(() => CurrentPokestopInfo);
                RaisePropertyChanged(() => CurrentSearchResponse);
                RaisePropertyChanged(() => SelectedModifierItem);
                RaisePropertyChanged(() => CurrentAddModifierResponse);
            }
            else
            {
                // Navigating from game page, so we need to actually load the Pokestop
                Busy.SetBusy(true, "Loading Pokestop");
                CurrentPokestop = (FortDataWrapper)NavigationHelper.NavigationState[nameof(CurrentPokestop)];
                NavigationHelper.NavigationState.Remove(nameof(CurrentPokestop));
                Logger.Write($"Searching {CurrentPokestop.Id}");
                CurrentPokestopInfo =
                    await GameClient.GetFort(CurrentPokestop.Id, CurrentPokestop.Latitude, CurrentPokestop.Longitude);
                Busy.SetBusy(false);
                // If timeout is expired we can go to to pokestop page
                if (CurrentPokestop.CooldownCompleteTimestampMs >= DateTime.UtcNow.ToUnixTime())
                {
                    // Timeout is not expired yet, player can't get items from the fort
                    SearchInCooldown?.Invoke(null, null);
                }
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
                suspensionState[nameof(CurrentPokestop)] = JsonConvert.SerializeObject(CurrentPokestop);
                suspensionState[nameof(CurrentPokestopInfo)] = CurrentPokestopInfo.ToByteString().ToBase64();
                suspensionState[nameof(CurrentSearchResponse)] = CurrentSearchResponse.ToByteString().ToBase64();
                suspensionState[nameof(SelectedModifierItem)] = SelectedModifierItem.ToByteString().ToBase64();
                suspensionState[nameof(CurrentAddModifierResponse)] = CurrentAddModifierResponse.ToByteString().ToBase64();
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
        ///     Pokestop that the user is visiting
        /// </summary>
        private FortDataWrapper _currentPokestop;

        /// <summary>
        ///     Infos on the current Pokestop
        /// </summary>
        private FortDetailsResponse _currentPokestopInfo;

        /// <summary>
        ///     Results of the current Pokestop search
        /// </summary>
        private FortSearchResponse _currentSearchResponse = new FortSearchResponse();

        /// <summary>
        ///     Current item for modifying a Pokestop
        /// </summary>
        private ItemData _selectedModifierItem;

        /// <summary>
        ///     Results of the current Pokestop modifier addition
        /// </summary>
        private AddFortModifierResponse _currentAddModifierResponse = new AddFortModifierResponse();

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Reference to global inventory
        /// </summary>
        public ObservableCollection<ItemData> ItemsInventory => GameClient.ItemsInventory;

        /// <summary>
        ///     Pokestop that the user is visiting
        /// </summary>
        public FortDataWrapper CurrentPokestop
        {
            get { return _currentPokestop; }
            set { Set(ref _currentPokestop, value); }
        }

        /// <summary>
        ///     Infos on the current Pokestop
        /// </summary>
        public FortDetailsResponse CurrentPokestopInfo
        {
            get { return _currentPokestopInfo; }
            set { Set(ref _currentPokestopInfo, value); }
        }

        /// <summary>
        ///     Results of the current Pokestop search
        /// </summary>
        public FortSearchResponse CurrentSearchResponse
        {
            get { return _currentSearchResponse; }
            set { Set(ref _currentSearchResponse, value); }
        }

        /// <summary>
        ///     Results of the current Pokestop modifier addition
        /// </summary>
        public AddFortModifierResponse CurrentAddModifierResponse
        {
            get { return _currentAddModifierResponse; }
            set { Set(ref _currentAddModifierResponse, value); }
        }

        /// <summary>
        ///     Items awarded by Pokestop searching
        /// </summary>
        public ObservableCollection<ItemAward> AwardedItems { get; } = new ObservableCollection<ItemAward>();

        /// <summary>
        ///     Current item for modifying pokestop
        /// </summary>
        public ItemData SelectedModifierItem
        {
            get { return _selectedModifierItem; }
            set { Set(ref _selectedModifierItem, value); }
        }

        public string LureTitleText
        {
            get
            {
                if (CurrentPokestopInfo.Modifiers.Count == 0)
                {
					return Utils.Resources.CodeResources.GetString("LureTitleTextEmpty");
                }
                else
                {
                    return Utils.Resources.Items.GetString("ItemTroyDisk");
				}
            }
        }

        public string LureDetailsText
        {
            get
            {
                if (CurrentPokestopInfo.Modifiers.Count == 0)
                {
                    return Utils.Resources.CodeResources.GetString("LureDetailsTextEmpty");
				}
                else
                {
                    return Utils.Resources.Items.GetString("D_ItemTroyDisk");
                }
			}
        }

        public bool IsPokestopLured
        {
            get { return CurrentPokestopInfo.Modifiers.Count > 0; }
        }
        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (
            _returnToGameScreen =
                new DelegateCommand(
                    () => { NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.PokestopUpdate); },
                    () => true)
            );

        private DelegateCommand _abandonPokestop;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand AbandonPokestop => _abandonPokestop ?? (
            _abandonPokestop = new DelegateCommand(() =>
            {
                // Re-enable update timer
                GameClient.ToggleUpdateTimer();
                Dispatcher.Dispatch(() => NavigationService.GoBack());
            }, () => true)
        );

        #endregion

        #region Pokestop Handling

        #region Search Events

        /// <summary>
        ///     Event fired if the user was able to get items from the Pokestop
        /// </summary>
        public event EventHandler SearchSuccess;

        /// <summary>
        ///     Event fired if the user tried to search a Pokestop which is out of range
        /// </summary>
        public event EventHandler SearchOutOfRange;

        /// <summary>
        ///     Event fired if the Pokestop is currently on cooldown and can't be searched
        /// </summary>
        public event EventHandler SearchInCooldown;

        /// <summary>
        ///     Event fired if the Player's inventory is full and he can't get items from the Pokestop
        /// </summary>
        public event EventHandler SearchInventoryFull;

        #endregion

        private DelegateCommand _searchCurrentPokestop;

        #region Helper Functions
        //Method for summing the item count
        private static IEnumerable<ItemAward> AggregateItems(IEnumerable<ItemAward> items) {
            //Check if the collection is not null
            if (items != null)
            {
                //If there is no items, just break
                if (!items.Any())
                {
                    yield break;
                }
                else
                {
                    //Order the list so the repeated items are near each other
                    var orderedItems = items.OrderBy(i => i.ItemId);
                    //Set the current as the first item
                    var currentItem = orderedItems.First().Clone();
                    //Iterate through the rest
                    foreach (var item in orderedItems.Skip(1))
                    {
                        //If the next item is still the same, sum the counts
                        if (item.ItemId == currentItem.ItemId)
                        {
                            currentItem.ItemCount += item.ItemCount;
                        }
                        //Otherwise return the item and set the new current
                        else
                        {
                            yield return currentItem;
                            currentItem = item.Clone();
                        }
                    }
                    //Return the last grouped item
                    yield return currentItem;
                }
            }
        }

        #endregion


        /// <summary>
        ///     Searches the current PokeStop, trying to get items from it
        /// </summary>
        public DelegateCommand SearchCurrentPokestop => _searchCurrentPokestop ?? (
            _searchCurrentPokestop = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Searching PokeStop");
                Logger.Write($"Searching {CurrentPokestopInfo.Name} [ID = {CurrentPokestop.Id}]");
                CurrentSearchResponse =
                    await GameClient.SearchFort(CurrentPokestop.Id, CurrentPokestop.Latitude, CurrentPokestop.Longitude);
                Busy.SetBusy(false);
                switch (CurrentSearchResponse.Result)
                {
                    case FortSearchResponse.Types.Result.NoResultSet:
                        break;
                    case FortSearchResponse.Types.Result.Success:
                        // Success, we play the animation and update inventory
                        Logger.Write("Searching Pokestop success");
                        AwardedItems.Clear();                        
                        foreach (var tmpAwardedItem in AggregateItems(CurrentSearchResponse.ItemsAwarded))
                        {                            
                            AwardedItems.Add(tmpAwardedItem);
                        }
                        CurrentPokestop.UpdateCooldown(CurrentSearchResponse.CooldownCompleteTimestampMs);
                        SearchSuccess?.Invoke(this, null);
                        await GameClient.UpdateInventory();
                        break;
                    case FortSearchResponse.Types.Result.OutOfRange:
                        // PokeStop can't be used because it's out of range, there's nothing that we can do
                        Logger.Write("Searching Pokestop out of range");
                        SearchOutOfRange?.Invoke(this, null);
                        break;
                    case FortSearchResponse.Types.Result.InCooldownPeriod:
                        // PokeStop can't be used because it's on cooldown, there's nothing that we can do
                        Logger.Write("Searching Pokestop in cooldown");
                        SearchInCooldown?.Invoke(this, null);
                        break;
                    case FortSearchResponse.Types.Result.InventoryFull:
                        // Items can't be gathered because player's inventory is full, there's nothing that we can do
                        AwardedItems.Clear();      
                        foreach (var tmpAwardedItem in AggregateItems(CurrentSearchResponse.ItemsAwarded))
                        {                           
                            AwardedItems.Add(tmpAwardedItem);
                        }
                        CurrentPokestop.UpdateCooldown(CurrentSearchResponse.CooldownCompleteTimestampMs);
                        SearchInventoryFull?.Invoke(this, null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, () => true));

        #region Add modifier Events

        /// <summary>
        ///     Event fired if the user wants to see the Pokestop modifier details
        /// </summary>
        public event EventHandler ShowModifierDetails;

        /// <summary>
        ///     Event fired if the user wants to close the Pokestop modifier details
        /// </summary>
        public event EventHandler HideModifierDetails;

        /// <summary>
        ///     Event fired if the user was able to add a modifier to the Pokestop
        /// </summary>
        public event EventHandler AddModifierSuccess;

        /// <summary>
        ///     Event fired if the user tried to add a modifier to a Pokestop which is too far away
        /// </summary>
        public event EventHandler AddModifierTooFarAway;

        /// <summary>
        ///     Event fired if the Pokestop already has a modifier
        /// </summary>
        public event EventHandler AddModifierAlreadyHasModifier;

        /// <summary>
        ///     Event fired if the Player's inventory does not have the correct item to add to the Pokestop
        /// </summary>
        public event EventHandler AddModifierNoItemInInventory;

        #endregion

        private DelegateCommand _showFortModifierDetails;

        public DelegateCommand ShowFortModifierDetails => _showFortModifierDetails ?? (
            _showFortModifierDetails = new DelegateCommand(async() =>
            {
                Busy.SetBusy(true, "Getting Pokestop module details");
                Logger.Write($"showing modifier details for {CurrentPokestopInfo.Name} [ID = {CurrentPokestop.Id}]");
                CurrentPokestopInfo =
                    await GameClient.GetFort(CurrentPokestop.Id, CurrentPokestop.Latitude, CurrentPokestop.Longitude);
                ShowModifierDetails?.Invoke(this, null);
                Busy.SetBusy(false);
            }, () => true));

        private DelegateCommand _hideFortModifierDetails;

        public DelegateCommand HideFortModifierDetails => _hideFortModifierDetails ?? (
            _hideFortModifierDetails = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, String.Empty);
                Logger.Write($"hiding modifier details for {CurrentPokestopInfo.Name} [ID = {CurrentPokestop.Id}]");
				CurrentPokestopInfo =
					await GameClient.GetFort(CurrentPokestop.Id, CurrentPokestop.Latitude, CurrentPokestop.Longitude);
				HideModifierDetails?.Invoke(this, null);
                Busy.SetBusy(false);
            }, () => true));

        private DelegateCommand _addFortModifier;

        /// <summary>
        ///     Adds a modifier to the current PokeStop, like a lure module
        /// </summary>
        public DelegateCommand AddFortModifier => _addFortModifier ?? (
            _addFortModifier = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Adding module to PokeStop");
                Logger.Write($"Adding modifier {CurrentPokestopInfo.Name} [ID = {CurrentPokestop.Id}]");
                CurrentAddModifierResponse =
                    await GameClient.AddFortModifier(CurrentPokestop.Id, SelectedModifierItem.ItemId);
                Busy.SetBusy(false);
                switch (CurrentAddModifierResponse.Result)
                {
                    case AddFortModifierResponse.Types.Result.NoResultSet:
                        break;
                    case AddFortModifierResponse.Types.Result.Success:
                        // Success, we play the animation
                        Logger.Write("Adding Pokestop modifier success");
                        AddModifierSuccess?.Invoke(this, null);
                        await GameClient.UpdateInventory();
						RaisePropertyChanged(() => IsPokestopLured);
                        break;
                    case AddFortModifierResponse.Types.Result.TooFarAway:
                        // PokeStop can't be modified because it's too far away, there's nothing that we can do
                        Logger.Write("Adding Pokestop modifier too far away");
                        AddModifierTooFarAway?.Invoke(this, null);
                        break;
                    case AddFortModifierResponse.Types.Result.FortAlreadyHasModifier:
                        // PokeStop can't be modified because it already has a modifier, there's nothing that we can do
                        Logger.Write("Adding Pokestop already has modifier");
                        AddModifierAlreadyHasModifier?.Invoke(this, null);
                        break;
                    case AddFortModifierResponse.Types.Result.NoItemInInventory:
                        // PokeStop can't be modified because there is no suitable Items in the player's inventory, there's nothing that we can do
                        Logger.Write("Adding Pokestop modifier, but there is no item in the player's inventory");
                        AddModifierNoItemInInventory?.Invoke(this, null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, () => true));

        /// <summary>Selects the first available modifier type</summary>
        /// <returns>Available modifier type or default with count 0</returns>
        public ItemData SelectAvailableModifier()
        {
            var modifier = SelectModifierType(ItemId.ItemTroyDisk);
            if (modifier != null && modifier.Count != 0)
                return modifier;

            var fallback = new ItemData { Count = 0, ItemId = ItemId.ItemTroyDisk };
            return fallback;
        }

		/// <summary>
		/// Checks whether there is a modifier available
		/// </summary>
		/// <returns></returns>
		public bool IsModifierAvailable
		{
			get
			{
				var modifier = SelectModifierType(ItemId.ItemTroyDisk);
				if (modifier != null && modifier.Count != 0)
					return true;

				return false;
			}
		}

        /// <summary>Selects the modifier type from Inventory</summary>
        /// <param name="itemId">The item identifier</param>
        /// <returns>Found modifier or null</returns>
        private ItemData SelectModifierType(ItemId? itemId)
        {
            if (itemId == null)
                return null;

            var modifier = ItemsInventory?.FirstOrDefault(item => item.ItemId == itemId);
            if (modifier == null || modifier.Count <= 0)
                return null;

            return modifier;
        }

        #endregion

        #endregion
    }
}