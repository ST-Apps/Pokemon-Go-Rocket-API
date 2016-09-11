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
                CurrentPokestop = JsonConvert.DeserializeObject<FortDataWrapper>((string)suspensionState[nameof(CurrentPokestop)]);
                CurrentPokestopInfo.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentPokestop)]).CreateCodedInput());
                CurrentSearchResponse.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(CurrentSearchResponse)]).CreateCodedInput());
                RaisePropertyChanged(() => CurrentPokestopInfo);
                RaisePropertyChanged(() => CurrentSearchResponse);
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

        #endregion

        #region Bindable Game Vars

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
        ///     Items awarded by Pokestop searching
        /// </summary>
        public ObservableCollection<ItemAward> AwardedItems { get; } = new ObservableCollection<ItemAward>();

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

        #endregion

        #endregion
    }
}