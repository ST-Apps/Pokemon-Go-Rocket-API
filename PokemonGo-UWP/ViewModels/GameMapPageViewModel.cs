using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Networking.Responses;
using Template10.Common;
using Template10.Mvvm;
using Resources = PokemonGo_UWP.Utils.Resources;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using Google.Protobuf;

namespace PokemonGo_UWP.ViewModels
{
    public class GameMapPageViewModel : ViewModelBase
    {

        public GameMapPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var poke1 = new NearbyPokemon()
                {
                    PokemonId = PokemonId.Abra,
                    DistanceInMeters = 10,
                };
                var poke2 = new NearbyPokemon()
                {
                    PokemonId = PokemonId.Arbok,
                    DistanceInMeters = 11,
                };
                var poke3 = new NearbyPokemon()
                {
                    PokemonId = PokemonId.Blastoise,
                    DistanceInMeters = 12,
                };
                GameClient.NearbyPokemons.Add(new NearbyPokemonWrapper(poke1));
                GameClient.NearbyPokemons.Add(new NearbyPokemonWrapper(poke2));
                GameClient.NearbyPokemons.Add(new NearbyPokemonWrapper(poke3));
                GameClient.PokedexInventory.Add(new PokedexEntry { PokemonId = poke1.PokemonId, TimesCaptured = 1 });
                GameClient.PokedexInventory.Add(new PokedexEntry { PokemonId = poke2.PokemonId, TimesCaptured = 1 });
            }
        }


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
            // Prevent from going back to other pages
            NavigationService.ClearHistory();
            if (parameter == null || mode == NavigationMode.Back) return;
            var gameMapNavigationMode = (GameMapNavigationModes)parameter;

            AudioUtils.PlaySound(AudioUtils.GAMEPLAY);

            // We just resumed from suspension so we restart update service and we get data from suspension state
            if (suspensionState.Any())
            {
                // Recovering the state
                PlayerProfile.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(PlayerProfile)]).CreateCodedInput());
                PlayerStats.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(PlayerStats)]).CreateCodedInput());
                // Restarting update service
                await StartGpsDataService();
                return;
            }

            // Let's do the proper action
            switch (gameMapNavigationMode)
            {
                case GameMapNavigationModes.AppStart:
                    // App just started, so we get GPS access and eventually initialize the client
                    await StartGpsDataService();
                    await UpdatePlayerData(true);
                    GameClient.ToggleUpdateTimer();
                    break;
                case GameMapNavigationModes.SettingsUpdate:
                    // We navigated back from Settings page after changing the Map provider, but this is managed in the page itself
                    break;
                case GameMapNavigationModes.PokestopUpdate:
                    // We came here after the catching page so we need to restart map update timer and update player data. We also check for level up.
                    GameClient.ToggleUpdateTimer();
                    await UpdatePlayerData(true);
                    break;
                case GameMapNavigationModes.PokemonUpdate:
                    // As above
                    GameClient.ToggleUpdateTimer();
                    await UpdatePlayerData(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                suspensionState[nameof(PlayerProfile)] = PlayerProfile.ToByteString().ToBase64();
                suspensionState[nameof(PlayerStats)] = PlayerStats.ToByteString().ToBase64();
            }
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        private PlayerData _playerProfile;

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        private PlayerStats _playerStats;

        /// <summary>
        ///     Response to the level up event
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
            private set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats
        {
            get { return _playerStats; }
            private set { Set(ref _playerStats, value); }
        }

        /// <summary>
        ///     Response to the level up event
        /// </summary>
        public LevelUpRewardsResponse LevelUpResponse
        {
            get { return _levelUpRewards; }
            private set { Set(ref _levelUpRewards, value); }
        }

        /// <summary>
        ///     Collection of Pokemon in 1 step from current position
        /// </summary>
        public static ObservableCollection<MapPokemonWrapper> CatchablePokemons => GameClient.CatchablePokemons;

        /// <summary>
        ///     Collection of lured Pokemon
        /// </summary>
        public static ObservableCollection<LuredPokemon> LuredPokemon => GameClient.LuredPokemons;

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
        ///     Event fired when level up rewards are awarded to user
        /// </summary>
        public event EventHandler LevelUpRewardsAwarded;

        #endregion

        /// <summary>
        ///     Waits for GPS auth and, if auth is given, starts updating data
        /// </summary>
        /// <returns></returns>
        public async Task StartGpsDataService()
        {
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
                        await
                            new MessageDialog(Resources.CodeResources.GetString("NoGpsPermissionsText")).ShowAsyncQueue();
                        BootStrapper.Current.Exit();
                        break;
                }
            });
        }


        /// <summary>
        ///     Updates player profile & stats
        /// </summary>
        /// <param name="checkForLevelUp"></param>
        /// <returns></returns>
        public async Task UpdatePlayerData(bool checkForLevelUp = false)
        {
            await GameClient.UpdateProfile();
            LevelUpResponse = await GameClient.UpdatePlayerStats(checkForLevelUp);
            PlayerProfile = GameClient.PlayerProfile;
            PlayerStats = GameClient.PlayerStats;
            if (checkForLevelUp && LevelUpResponse != null)
            {
                switch (LevelUpResponse.Result)
                {
                    case LevelUpRewardsResponse.Types.Result.Success:
                        LevelUpRewardsAwarded?.Invoke(this, null);
                        break;
                }
            }
        }

        #endregion

        #region Settings

        private DelegateCommand _openSettingsCommand;

        public DelegateCommand SettingsCommand
            =>
                _openSettingsCommand ??
                (_openSettingsCommand = new DelegateCommand(() => { NavigationService.Navigate(typeof(SettingsPage)); }))
            ;

        #endregion

        #region Inventory

        private DelegateCommand _gotoPokemonInventoryPage;

        public DelegateCommand GotoPokemonInventoryPageCommand
            =>
                _gotoPokemonInventoryPage ??
                (_gotoPokemonInventoryPage =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(PokemonInventoryPage), true); }));

        #endregion

        #region Items

        private DelegateCommand _gotoItemsInventoryPage;

        public DelegateCommand GotoItemsInventoryPageCommand
            =>
                _gotoItemsInventoryPage ??
                (_gotoItemsInventoryPage =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(ItemsInventoryPage), true); }));

        #endregion

        #region Pokedex

        private DelegateCommand _gotoPlayerProfilePage;

        public DelegateCommand GotoPlayerProfilePageCommand
            =>
                _gotoPlayerProfilePage ??
                (_gotoPlayerProfilePage =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(PlayerProfilePage), true); }));

        private DelegateCommand _gotoPokedexPage;
        public DelegateCommand GotoPokedexPageCommand
            =>
                _gotoPokedexPage ??
                (_gotoPokedexPage =
                    new DelegateCommand(() => { NavigationService.Navigate(typeof(PokedexPage)); }));

        #endregion

        #endregion
    }
}