using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;
using Newtonsoft.Json;

namespace PokemonGo_UWP.ViewModels
{
    public class EnterGymPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter">FortData containing the Gym that we're visiting</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentGym = JsonConvert.DeserializeObject<FortDataWrapper>((string)suspensionState[nameof(CurrentGym)]);
                CurrentGymInfo = JsonConvert.DeserializeObject<GetGymDetailsResponse>((string)suspensionState[nameof(CurrentGymInfo)]);
                CurrentEnterResponse = JsonConvert.DeserializeObject<GetGymDetailsResponse>((string)suspensionState[nameof(CurrentEnterResponse)]);
            }
            else
            {
                // Navigating from game page, so we need to actually load the Gym                  
                Busy.SetBusy(true, "Loading Gym");
                CurrentGym = (FortDataWrapper)NavigationHelper.NavigationState[nameof(CurrentGym)];
                NavigationHelper.NavigationState.Remove(nameof(CurrentGym));
                Logger.Write($"Entering {CurrentGym.Id}");
                CurrentGymInfo =
                    await GameClient.GetGymDetails(CurrentGym.Id, CurrentGym.Latitude, CurrentGym.Longitude);
                Busy.SetBusy(false);
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
                suspensionState[nameof(CurrentGym)] = JsonConvert.SerializeObject(CurrentGym);
                suspensionState[nameof(CurrentGymInfo)] = JsonConvert.SerializeObject(CurrentGymInfo);
                suspensionState[nameof(CurrentEnterResponse)] = JsonConvert.SerializeObject(CurrentEnterResponse);
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
        ///     Gym that the user is visiting
        /// </summary>
        private FortDataWrapper _currentGym;

        /// <summary>
        ///     Infos on the current Gym
        /// </summary>
        private GetGymDetailsResponse _currentGymInfo;

        /// <summary>
        ///     Results of the current Gym enter
        /// </summary>
        private GetGymDetailsResponse _currentEnterResponse;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Gym that the user is visiting
        /// </summary>
        public FortDataWrapper CurrentGym
        {
            get { return _currentGym; }
            set { Set(ref _currentGym, value); }
        }

        /// <summary>
        ///     Infos on the current Gym
        /// </summary>
        public GetGymDetailsResponse CurrentGymInfo
        {
            get { return _currentGymInfo; }
            set { Set(ref _currentGymInfo, value); }
        }

        /// <summary>
        ///     Results of the current Gym enter
        /// </summary>
        public GetGymDetailsResponse CurrentEnterResponse
        {
            get { return _currentEnterResponse; }
            set { Set(ref _currentEnterResponse, value); }
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
                    () => { NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.GymUpdate); },
                    () => true)
            );

        private DelegateCommand _abandonGym;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand AbandonGym => _abandonGym ?? (
            _abandonGym = new DelegateCommand(() =>
            {
                // Re-enable update timer
                GameClient.ToggleUpdateTimer();
                NavigationService.GoBack();
            }, () => true)
            );

        #endregion

        #region Gym Handling

        #region Gym Events

        /// TODO: Events fired 
        /// <summary>
        ///     Event fired if the user was able to enter the Gym
        /// </summary>
        public event EventHandler EnterSuccess;

        /// <summary>
        ///     Event fired if the user tried to enter a Gym which is out of range
        /// </summary>
        public event EventHandler EnterOutOfRange;

        /// <summary>
        /// <summary>
        ///     Event fired if the Player's inventory is full and he can't get items from the Pokestop
        /// </summary>
        public event EventHandler EnterInventoryFull;

        #endregion

        private DelegateCommand _enterCurrentGym;

        /// <summary>
        ///     Enters the current Gym, don't know what to do then
        /// </summary>
        public DelegateCommand EnterCurrentGym => _enterCurrentGym ?? (
            _enterCurrentGym = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Entering Gym");
                Logger.Write($"Entering {CurrentGymInfo.Name} [ID = {CurrentGym.Id}]");
                CurrentEnterResponse =
                    await GameClient.GetGymDetails(CurrentGym.Id, CurrentGym.Latitude, CurrentGym.Longitude);
                Busy.SetBusy(false);
                switch (CurrentEnterResponse.Result)
                {
                    case GetGymDetailsResponse.Types.Result.Unset:
                        break;
                    case GetGymDetailsResponse.Types.Result.Success:
                        // Success, we play the animation and update inventory
                        Logger.Write("Entering Gym success");

                        // TODO: What to do when we are in the Gym?
                        EnterSuccess?.Invoke(this, null);
                        await GameClient.UpdateInventory();
                        break;
                    case GetGymDetailsResponse.Types.Result.ErrorNotInRange:
                        // Gym can't be used because it's out of range, there's nothing that we can do
                        Logger.Write("Entering Gym out of range");
                        EnterOutOfRange?.Invoke(this, null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, () => true));

        #endregion

        #endregion
    }
}