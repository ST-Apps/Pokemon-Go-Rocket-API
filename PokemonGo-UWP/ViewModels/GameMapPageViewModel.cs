using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation.Metadata;
using Windows.Phone.Devices.Notification;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Inventory;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class GameMapPageViewModel : ViewModelBase
    {

        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            // Prevent from going back to other pages
            NavigationService.ClearHistory();
            if (parameter is bool && mode != NavigationMode.Back)
            {
                // First time navigating here, we need to initialize data updating but only if we have GPS access
                await Dispatcher.DispatchAsync(async () =>
                {
                    var accessStatus = await Geolocator.RequestAccessAsync();
                    switch (accessStatus)
                    {
                        case GeolocationAccessStatus.Allowed:
                            await GameClient.InitializeDataUpdate();
                            break;
                        default:
                            Logger.Write("Error during GPS activation");
                            await new MessageDialog(Utils.Resources.Translation.GetString("NoGPSPermissions")).ShowAsyncQueue();
                            BootStrapper.Current.Exit();
                            break;
                    }
                });
            }
            // Restarts map timer
            GameClient.ToggleUpdateTimer();
            if (suspensionState.Any())
            {
                // Recovering the state                
                PlayerProfile = (PlayerData)suspensionState[nameof(PlayerProfile)];
                PlayerStats = (PlayerStats)suspensionState[nameof(PlayerStats)];
            }
            else
            {
                // No saved state, get them from the client                
                PlayerProfile = (await GameClient.GetProfile()).PlayerData;
                InventoryDelta = (await GameClient.GetInventory()).InventoryDelta;                
                var tmpStats = InventoryDelta.InventoryItems.First(item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
                if (PlayerStats != null && PlayerStats.Level != tmpStats.Level)
                {
                    LevelUpResponse = await GameClient.GetLevelUpRewards(tmpStats.Level);                                        
                    switch (LevelUpResponse.Result)
                    {
                        case LevelUpRewardsResponse.Types.Result.Success:
                            LevelUpRewardsAwarded?.Invoke(this, null);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                PlayerStats = tmpStats;
                RaisePropertyChanged(nameof(ExperienceValue));
            }
            // Setup vibration and sound
            if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice") && _vibrationDevice == null)
            {
                _vibrationDevice = VibrationDevice.GetDefault();
            }
            GameClient.MapPokemonUpdated += GameClientOnMapPokemonUpdated;
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
                suspensionState[nameof(PlayerProfile)] = PlayerProfile;
                suspensionState[nameof(PlayerStats)] = PlayerStats;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            // Stops map timer
            GameClient.ToggleUpdateTimer(false);
            GameClient.MapPokemonUpdated -= GameClientOnMapPokemonUpdated;
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     We use it to notify that we found at least one catchable Pokemon in our area
        /// </summary>
        private VibrationDevice _vibrationDevice;

        /// <summary>
        ///     True if the phone can vibrate (e.g. the app is not in background)
        /// </summary>
        public bool CanVibrate;

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        private PlayerData _playerProfile;

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        private PlayerStats _playerStats;

        /// <summary>
        ///     Player's inventory
        ///     TODO: do we really need it?
        /// </summary>
        private InventoryDelta _inventoryDelta;

        /// <summary>
        /// Response to the level up event
        /// </summary>
        private LevelUpRewardsResponse _levelUpRewards;

        #endregion

        #region Bindable Game Vars   

        public ElementTheme CurrentTheme
        {
            get
            {
                // Set theme
                var currentTime = int.Parse(DateTime.Now.ToString("HH"));
                return currentTime > 7 && currentTime < 19 ? ElementTheme.Light : ElementTheme.Dark;
            }
        }

        public string CurrentVersion => GameClient.CurrentVersion;

        /// <summary>
        ///     Key for Bing's Map Service (not included in GIT, you need to get your own token to use maps!)
        /// </summary>
        public string MapServiceToken => ApplicationKeys.MapServiceToken;

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats
        {
            get { return _playerStats; }
            set { Set(ref _playerStats, value); }
        }

        public int ExperienceValue => _playerStats == null ? 0 : (int) (((double) _playerStats.Experience - _playerStats.PrevLevelXp)/(_playerStats.NextLevelXp - _playerStats.PrevLevelXp)*100);

        public InventoryDelta InventoryDelta
        {
            get { return _inventoryDelta; }
            set { Set(ref _inventoryDelta, value); }
        }

        /// <summary>
        ///     Collection of Pokemon in 1 step from current position
        /// </summary>
        public static ObservableCollection<MapPokemonWrapper> CatchablePokemons => GameClient.CatchablePokemons;

        /// <summary>
        ///     Collection of Pokemon in 2 steps from current position
        /// </summary>
        public static ObservableCollection<NearbyPokemonWrapper> NearbyPokemons => GameClient.NearbyPokemons;

        /// <summary>
        ///     Collection of Pokestops in the current area
        /// </summary>
        public static ObservableCollection<FortDataWrapper> NearbyPokestops => GameClient.NearbyPokestops;

        #endregion

        #region Game Logic

        #region Player

        #region Level Up Events

        /// <summary>
        /// Event fired when level up rewards are awarded to user
        /// </summary>
        public event EventHandler LevelUpRewardsAwarded;

        #endregion

        /// <summary>
        /// Response to the level up event
        /// </summary>
        public LevelUpRewardsResponse LevelUpResponse {
            get { return _levelUpRewards; }
            set{ Set(ref _levelUpRewards, value); }
        }

        #endregion

        #region Settings

        private DelegateCommand _openSettingsCommand;

        public DelegateCommand SettingsCommand => _openSettingsCommand ?? (_openSettingsCommand = new DelegateCommand(() =>
        {
            // Navigate back
            NavigationService.Navigate(typeof(SettingsPage));
        }, () => true));

        #endregion

        #region Notify

        /// <summary>
        /// Vibrates and/or plays a sound when new pokemons are in the area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private async void GameClientOnMapPokemonUpdated(object sender, EventArgs eventArgs)
        {
            if (SettingsService.Instance.IsVibrationEnabled)
                _vibrationDevice?.Vibrate(TimeSpan.FromMilliseconds(500));
            if (SettingsService.Instance.IsMusicEnabled)
                await AudioUtils.PlaySound(@"pokemon_found_ding.wav");
        }

        #endregion

        #region Inventory

        private DelegateCommand _gotoPokemonInventoryPage;

        public DelegateCommand GotoPokemonInventoryPageCommand => _gotoPokemonInventoryPage ?? (_gotoPokemonInventoryPage = new DelegateCommand(() => { NavigationService.Navigate(typeof(PokemonInventoryPage), true); }));

        #endregion

        #endregion
    }
}
