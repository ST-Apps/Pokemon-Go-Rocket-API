using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.ViewModels;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings;
using POGOProtos.Settings.Master;
using Q42.WinRT.Data;
using Template10.Common;
using Template10.Utils;
using Universal_Authenticator_v2.Views;
using Windows.Devices.Sensors;
using Newtonsoft.Json;
using PokemonGo.RocketAPI.Rpc;
using PokemonGoAPI.Session;
using PokemonGo_UWP.Utils.Helpers;
using System.Collections.Specialized;
using Windows.UI.Popups;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Static class containing game's state and wrapped client methods to update data
    /// </summary>
    public static class GameClient
    {
        #region Client Vars

        private static ISettings _clientSettings;
        private static Client _client;

        /// <summary>
        /// Handles map updates using Niantic's logic.
        /// Ported from <seealso cref="https://github.com/AeonLucid/POGOLib"/>
        /// </summary>
        private class Heartbeat
        {
            /// <summary>
            ///     Determines whether we can keep heartbeating.
            /// </summary>
            private bool _keepHeartbeating = true;

            /// <summary>
            /// Timer used to update map
            /// </summary>
            private DispatcherTimer _mapUpdateTimer;

            /// <summary>
            /// True if another update operation is in progress.
            /// </summary>
            private bool _isHeartbeating;

            /// <summary>
            /// Checks if we need to update data
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="o"></param>
            private async void HeartbeatTick(object sender, object o)
            {
                // We need to skip this iteration
                if (!_keepHeartbeating || _isHeartbeating) return;
                _isHeartbeating = true;
                // Heartbeat is alive so we check if we need to update data, based on GameSettings
                var canRefresh = false;


                //Collect location data for signature
                DeviceInfos.Instance.CollectLocationData();

                // We have no settings yet so we just update without further checks
                if (GameSetting == null)
                {
                    canRefresh = true;
                }
                else
                {
                    // Check if we need to update                
                    var minSeconds = GameSetting.MapSettings.GetMapObjectsMinRefreshSeconds;
                    var maxSeconds = GameSetting.MapSettings.GetMapObjectsMaxRefreshSeconds;
                    var minDistance = GameSetting.MapSettings.GetMapObjectsMinDistanceMeters;
                    var lastGeoCoordinate = _lastGeopositionMapObjectsRequest;
                    var secondsSinceLast = DateTime.Now.Subtract(BaseRpc.LastRpcRequest).Seconds;
                    if (lastGeoCoordinate == null)
                    {
                        Logger.Write("Refreshing MapObjects, reason: 'lastGeoCoordinate == null'.");
                        canRefresh = true;
                    }
                    else if (secondsSinceLast >= minSeconds)
                    {
                        var metersMoved = GeoHelper.Distance(Geoposition.Coordinate.Point, lastGeoCoordinate.Coordinate.Point);
                        if (secondsSinceLast >= maxSeconds)
                        {
                            Logger.Write($"Refreshing MapObjects, reason: 'secondsSinceLast({secondsSinceLast}) >= maxSeconds({maxSeconds})'.");
                            canRefresh = true;
                        }
                        else if (metersMoved >= minDistance)
                        {
                            Logger.Write($"Refreshing MapObjects, reason: 'metersMoved({metersMoved}) >= minDistance({minDistance})'.");
                            canRefresh = true;
                        }
                    }
                } 
                // Update!
                if (!canRefresh)
                {
                    _isHeartbeating = false;
                    return;
                }
                try
                {
                    await UpdateMapObjects();
                }
                catch (Exception ex)
                {
                    await ExceptionHandler.HandleException(ex);
                }
                finally
                {
                    _isHeartbeating = false;
                }
            }

            /// <summary>
            /// Inits heartbeat
            /// </summary>
            internal async Task StartDispatcher()
            {
                _keepHeartbeating = true;
                if (_mapUpdateTimer != null) return;

                await DispatcherHelper.RunInDispatcherAndAwait(() =>
                {
                    _mapUpdateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    _mapUpdateTimer.Tick += HeartbeatTick;
                    _mapUpdateTimer.Start();
                });
            }

            /// <summary>
            /// Stops heartbeat
            /// </summary>
            internal void StopDispatcher()
            {
                _keepHeartbeating = false;
            }
        }
        #endregion

        #region Game Vars

        /// <summary>
        ///     App's current version
        /// </summary>
        public static string CurrentVersion
        {
            get
            {
                var currentVersion = Package.Current.Id.Version;
                return $"v{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}";
            }
        }

        /// <summary>
        ///     Settings downloaded from server
        /// </summary>
        public static GlobalSettings GameSetting { get; private set; }

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public static PlayerData PlayerProfile { get; private set; }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public static PlayerStats PlayerStats { get; private set; }

        /// <summary>
        ///     Contains infos about level up rewards
        /// </summary>
        public static InventoryDelta InventoryDelta { get; private set; }

        #region Collections

        /// <summary>
        ///     Collection of Pokemon in 1 step from current position
        /// </summary>
        public static ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } =
            new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        ///     Collection of Pokemon in 2 steps from current position
        /// </summary>
        public static ObservableCollection<NearbyPokemonWrapper> NearbyPokemons { get; set; } =
            new ObservableCollection<NearbyPokemonWrapper>();

        /// <summary>
        ///     Collection of lured Pokemon
        /// </summary>
        public static ObservableCollection<LuredPokemon> LuredPokemons { get; set; } = new ObservableCollection<LuredPokemon>();

        /// <summary>
        ///     Collection of Pokestops in the current area
        /// </summary>
        public static ObservableCollection<FortDataWrapper> NearbyPokestops { get; set; } =
            new ObservableCollection<FortDataWrapper>();

        /// <summary>
        ///     Stores Items in the current inventory
        /// </summary>
        public static ObservableCollection<ItemData> ItemsInventory { get; set; } = new ObservableCollection<ItemData>()
            ;

        /// <summary>
        ///     Stores Items that can be used to catch a Pokemon
        /// </summary>
        public static ObservableCollection<ItemData> CatchItemsInventory { get; set; } =
            new ObservableCollection<ItemData>();

        /// <summary>
        ///     Stores free Incubators in the current inventory
        /// </summary>
        public static ObservableCollection<EggIncubator> FreeIncubatorsInventory { get; set; } =
            new ObservableCollection<EggIncubator>();

        /// <summary>
        ///     Stores used Incubators in the current inventory
        /// </summary>
        public static ObservableCollection<EggIncubator> UsedIncubatorsInventory { get; set; } =
            new ObservableCollection<EggIncubator>();

        /// <summary>
        ///     Stores Pokemons in the current inventory
        /// </summary>
        public static ObservableCollectionPlus<PokemonData> PokemonsInventory { get; set; } =
            new ObservableCollectionPlus<PokemonData>();

        /// <summary>
        ///     Stores Eggs in the current inventory
        /// </summary>
        public static ObservableCollection<PokemonData> EggsInventory { get; set; } =
            new ObservableCollection<PokemonData>();

        /// <summary>
        ///     Stores player's current Pokedex
        /// </summary>
        public static ObservableCollectionPlus<PokedexEntry> PokedexInventory { get; set; } =
            new ObservableCollectionPlus<PokedexEntry>();

        /// <summary>
        ///     Stores player's current candies
        /// </summary>
        public static ObservableCollection<Candy> CandyInventory { get; set; } = new ObservableCollection<Candy>();

        #endregion

        #region Templates from server

        /// <summary>
        ///     Stores extra useful data for the Pokedex, like Pokemon type and other stuff that is missing from PokemonData
        /// </summary>
        public static IEnumerable<PokemonSettings> PokemonSettings { get; private set; } = new List<PokemonSettings>();

        /// <summary>
        ///     Stores upgrade costs (candy, stardust) per each level
        /// </summary>
        public static Dictionary<int, object[]> PokemonUpgradeCosts { get; private set; } = new Dictionary<int, object[]>();

        /// <summary>
        ///     Stores data about Pokemon moves
        /// </summary>
        public static IEnumerable<MoveSettings> MoveSettings { get; private set; } = new List<MoveSettings>();

        #endregion

        #endregion

        #region Constructor

        static GameClient()
        {
            PokedexInventory.CollectionChanged += PokedexInventory_CollectionChanged;
            // TODO: Investigate whether or not this needs to be unsubscribed when the app closes.
        }

        /// <summary>
        /// When new items are added to the Pokedex, reset the Nearby Pokemon so their state can be re-run.
        /// </summary>
        /// <remarks>
        /// This exists because the Nearby Pokemon are Map objects, and are loaded before Inventory. If you don't do this,
        /// the first Nearby items are always shown as "new to the Pokedex" until they disappear, regardless of if they are
        /// ACTUALLY new.
        /// </remarks>
        private static void PokedexInventory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // advancedrei: This is a total order-of-operations hack.
                var nearby = NearbyPokemons.ToList();
                NearbyPokemons.Clear();
                NearbyPokemons.AddRange(nearby);
            }
        }

        #endregion

        #region Game Logic

        #region Login/Logout

        /// <summary>
        /// Saves the new AccessToken to settings.        
        /// </summary>
        private static void SaveAccessToken()
        {
            SettingsService.Instance.AccessTokenString = JsonConvert.SerializeObject(_client.AccessToken);
        }

        /// <summary>
        /// Loads current AccessToken
        /// </summary>
        /// <returns></returns>
        public static AccessToken LoadAccessToken()
        {
            var tokenString = SettingsService.Instance.AccessTokenString;
            return tokenString == null ? null : JsonConvert.DeserializeObject<AccessToken>(SettingsService.Instance.AccessTokenString);
        }

        /// <summary>
        ///     Sets things up if we didn't come from the login page
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeClient()
        {

            await DataCache.Init();

            var credentials = SettingsService.Instance.UserCredentials;
            credentials.RetrievePassword();
            _clientSettings = new Settings
            {
                AuthType = SettingsService.Instance.LastLoginService,
                PtcUsername = SettingsService.Instance.LastLoginService == AuthType.Ptc ? credentials.UserName : null,
                PtcPassword = SettingsService.Instance.LastLoginService == AuthType.Ptc ? credentials.Password : null,
                GoogleUsername = SettingsService.Instance.LastLoginService == AuthType.Google ? credentials.UserName : null,
                GooglePassword = SettingsService.Instance.LastLoginService == AuthType.Google ? credentials.Password : null,
            };

            _client = new Client(_clientSettings, null, DeviceInfos.Instance) {AccessToken = LoadAccessToken()};
            var apiFailureStrategy = new ApiFailureStrategy(_client);
            _client.ApiFailure = apiFailureStrategy;
            // Register to AccessTokenChanged       
            apiFailureStrategy.OnAccessTokenUpdated += (s, e) => SaveAccessToken();
            try
            {
                await _client.Login.DoLogin();
            }
            catch (Exception e)
            {
                if (e is PokemonGo.RocketAPI.Exceptions.AccessTokenExpiredException)
                {
                    Debug.WriteLine("AccessTokenExpired Exception caught");
                    await _client.Login.DoLogin();
                }
                else
                {
                    await new MessageDialog(e.Message).ShowAsyncQueue();
                }
            }
        }

        /// <summary>
        ///     Starts a PTC session for the given user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true if login worked</returns>
        public static async Task<bool> DoPtcLogin(string username, string password)
        {
            _clientSettings = new Settings
            {
                PtcUsername = username,
                PtcPassword = password,
                AuthType = AuthType.Ptc
            };
            _client = new Client(_clientSettings, null, DeviceInfos.Instance);
            var apiFailureStrategy = new ApiFailureStrategy(_client);
            _client.ApiFailure = apiFailureStrategy;
            // Register to AccessTokenChanged       
            apiFailureStrategy.OnAccessTokenUpdated += (s, e) => SaveAccessToken();
            // Get PTC token
            await _client.Login.DoLogin();
            // Update current token even if it's null and clear the token for the other identity provide
            SaveAccessToken();
            // Update other data if login worked
            if (_client.AccessToken == null) return false;
            SettingsService.Instance.LastLoginService = AuthType.Ptc;
            SettingsService.Instance.UserCredentials =
                new PasswordCredential(nameof(SettingsService.Instance.UserCredentials), username, password);
            // Return true if login worked, meaning that we have a token
            return true;
        }

        /// <summary>
        ///     Starts a Google session for the given user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>true if login worked</returns>
        public static async Task<bool> DoGoogleLogin(string email, string password)
        {
            _clientSettings = new Settings
            {
                GoogleUsername = email,
                GooglePassword = password,
                AuthType = AuthType.Google
            };

            _client = new Client(_clientSettings, null, DeviceInfos.Instance);
            var apiFailureStrategy = new ApiFailureStrategy(_client);
            _client.ApiFailure = apiFailureStrategy;
            // Register to AccessTokenChanged       
            apiFailureStrategy.OnAccessTokenUpdated += (s, e) => SaveAccessToken();
            // Get Google token
            await _client.Login.DoLogin();
            // Update current token even if it's null and clear the token for the other identity provide
            SaveAccessToken();
            // Update other data if login worked
            if (_client.AccessToken == null) return false;
            SettingsService.Instance.LastLoginService = AuthType.Google;
            SettingsService.Instance.UserCredentials =
                new PasswordCredential(nameof(SettingsService.Instance.UserCredentials), email, password);
            // Return true if login worked, meaning that we have a token
            return true;
        }

        /// <summary>
        ///     Logs the user out by clearing data and timers
        /// </summary>
        public static async void DoLogout()
        {
            // Clear stored token
            SettingsService.Instance.AccessTokenString = null;
            if (!SettingsService.Instance.RememberLoginData)
                SettingsService.Instance.UserCredentials = null;
            _heartbeat?.StopDispatcher();
            _geolocator.PositionChanged -= GeolocatorOnPositionChanged;
            _geolocator = null;
            _lastGeopositionMapObjectsRequest = null;
            CatchablePokemons.Clear();
            NearbyPokemons.Clear();
            NearbyPokestops.Clear();
        }

        #endregion

        #region Data Updating

        private static Geolocator _geolocator;
        private static Compass _compass;

        public static Geoposition Geoposition { get; private set; }

        private static Heartbeat _heartbeat;
        private static DispatcherTimer _compassTimer;

        /// <summary>
        ///     We fire this event when the current position changes
        /// </summary>
        public static event EventHandler<Geoposition> GeopositionUpdated;

        public static event EventHandler<CompassReading> HeadingUpdated;

        /// <summary>
        ///     Starts the timer to update map objects and the handler to update position
        /// </summary>
        public static async Task InitializeDataUpdate()
        {
            if (SettingsService.Instance.IsCompassEnabled)
            {
                _compass = Compass.GetDefault();
                if (_compass != null)
                {
                    _compassTimer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(Math.Max(_compass.MinimumReportInterval, 50))
                    };
                    _compassTimer.Tick += (s, e) =>
                    {
                        if (SettingsService.Instance.IsAutoRotateMapEnabled)
                        {
                            HeadingUpdated?.Invoke(null, _compass.GetCurrentReading());
                        }
                    };
                    _compassTimer.Start();
                }
            }
            _geolocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High,
                DesiredAccuracyInMeters = 5,
                ReportInterval = 1000,
                MovementThreshold = 5
            };

            Busy.SetBusy(true, Resources.CodeResources.GetString("GettingGpsSignalText"));
            Geoposition = Geoposition ?? await _geolocator.GetGeopositionAsync();
            GeopositionUpdated?.Invoke(null, Geoposition);
            _geolocator.PositionChanged += GeolocatorOnPositionChanged;
            // Before starting we need game settings
            GameSetting =
                await
                    DataCache.GetAsync(nameof(GameSetting), async () => (await _client.Download.GetSettings()).Settings,
                        DateTime.Now.AddMonths(1));
            // Update geolocator settings based on server
            _geolocator.MovementThreshold = GameSetting.MapSettings.GetMapObjectsMinDistanceMeters;
            if (_heartbeat == null)
                _heartbeat = new Heartbeat();
            await _heartbeat.StartDispatcher();
            // Update before starting timer
            Busy.SetBusy(true, Resources.CodeResources.GetString("GettingUserDataText"));
            //await UpdateMapObjects();
            await UpdateInventory();
            await UpdateItemTemplates();
            Busy.SetBusy(false);
        }

        private static async void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Geoposition = args.Position;
            // Updating player's position
            var position = Geoposition.Coordinate.Point.Position;
            if (_client != null)
                await _client.Player.UpdatePlayerLocation(position.Latitude, position.Longitude, position.Altitude);
            GeopositionUpdated?.Invoke(null, Geoposition);
        }

        /// <summary>
        ///     DateTime for the last map update
        /// </summary>
        private static DateTime _lastUpdate;

        /// <summary>
        ///     Toggles the update timer based on the isEnabled value
        /// </summary>
        /// <param name="isEnabled"></param>
        public static async void ToggleUpdateTimer(bool isEnabled = true)
        {
            if (isEnabled)
                await _heartbeat.StartDispatcher();
            else
            {
                _heartbeat.StopDispatcher();
            }
        }

        /// <summary>
        ///     Updates catcheable and nearby Pokemons + Pokestops.
        ///     We're using a single method so that we don't need two separate calls to the server, making things faster.
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateMapObjects()
        {
            // Get all map objects from server
            var mapObjects = await GetMapObjects(Geoposition);
            _lastUpdate = DateTime.Now;

            // update catchable pokemons
            var newCatchablePokemons = mapObjects.Item1.MapCells.SelectMany(x => x.CatchablePokemons).Select(item => new MapPokemonWrapper(item)).ToArray();
            Logger.Write($"Found {newCatchablePokemons.Length} catchable pokemons");
            CatchablePokemons.UpdateWith(newCatchablePokemons, x => x,
                (x, y) => x.EncounterId == y.EncounterId);

            // update nearby pokemons
            var newNearByPokemons = mapObjects.Item1.MapCells.SelectMany(x => x.NearbyPokemons).ToArray();
            Logger.Write($"Found {newNearByPokemons.Length} nearby pokemons");
            // for this collection the ordering is important, so we follow a slightly different update mechanism
            NearbyPokemons.UpdateByIndexWith(newNearByPokemons, x => new NearbyPokemonWrapper(x));

            // update poke stops on map (gyms are ignored for now)
            var newPokeStops = mapObjects.Item1.MapCells
                .SelectMany(x => x.Forts)
                .Where(x => x.Type == FortType.Checkpoint)
                .ToArray();            
            Logger.Write($"Found {newPokeStops.Length} nearby PokeStops");
            NearbyPokestops.UpdateWith(newPokeStops, x => new FortDataWrapper(x), (x, y) => x.Id == y.Id);

            // Update LuredPokemon
            var newLuredPokemon = newPokeStops.Where(item => item.ActiveFortModifier != null && item.ActiveFortModifier.Contains(ItemId.ItemTroyDisk)).Select(item => new LuredPokemon(item.LureInfo, item.Latitude, item.Longitude)).ToArray();
            Logger.Write($"Found {newLuredPokemon.Length} lured Pokemon");
            LuredPokemons.UpdateByIndexWith(newLuredPokemon, x => x);
            Logger.Write("Finished updating map objects");
        }

        #endregion

        #region Map & Position

        private static Geoposition _lastGeopositionMapObjectsRequest;

        /// <summary>
        ///     Gets updated map data based on provided position
        /// </summary>
        /// <param name="geoposition"></param>
        /// <returns></returns>
        private static async
            Task
                <
                    Tuple
                        <GetMapObjectsResponse, GetHatchedEggsResponse, GetInventoryResponse, CheckAwardedBadgesResponse,
                            DownloadSettingsResponse>> GetMapObjects(Geoposition geoposition)
        {
            _lastGeopositionMapObjectsRequest = geoposition;      
            return await _client.Map.GetMapObjects();
        }

        #endregion

        #region Player Data & Inventory

        /// <summary>
        ///     List of items that can be used when trying to catch a Pokemon
        /// </summary>
        private static readonly List<ItemId> CatchItemIds = new List<ItemId>
        {
            ItemId.ItemPokeBall,
            ItemId.ItemGreatBall,
            ItemId.ItemBlukBerry,
            ItemId.ItemMasterBall,
            ItemId.ItemNanabBerry,
            ItemId.ItemPinapBerry,
            ItemId.ItemRazzBerry,
            ItemId.ItemUltraBall,
            ItemId.ItemWeparBerry
        };

        /// <summary>
        ///     Gets user's profile
        /// </summary>
        /// <returns></returns>
        public static async Task UpdateProfile()
        {
            PlayerProfile = (await _client.Player.GetPlayer()).PlayerData;
        }

        /// <summary>
        ///     Gets player's inventoryDelta
        /// </summary>
        /// <returns></returns>
        public static async Task<LevelUpRewardsResponse> UpdatePlayerStats(bool checkForLevelUp = false)
        {
            InventoryDelta = (await _client.Inventory.GetInventory()).InventoryDelta;

            var tmpStats =
                InventoryDelta.InventoryItems.First(item => item.InventoryItemData.PlayerStats != null)
                    .InventoryItemData.PlayerStats;

            if (checkForLevelUp && ((PlayerStats == null) || (PlayerStats != null && PlayerStats.Level != tmpStats.Level)))
            {
                PlayerStats = tmpStats;
                var levelUpResponse = await GetLevelUpRewards(tmpStats.Level);
                return levelUpResponse;
            }
            PlayerStats = tmpStats;            
            return null;
        }

        /// <summary>
        ///     Gets player's inventoryDelta
        /// </summary>
        /// <returns></returns>
        public static async Task<GetInventoryResponse> GetInventory()
        {
            return await _client.Inventory.GetInventory();
        }

        /// <summary>
        ///     Gets the rewards after leveling up
        /// </summary>
        /// <returns></returns>
        public static async Task<LevelUpRewardsResponse> GetLevelUpRewards(int newLevel)
        {
            return await _client.Player.GetLevelUpRewards(newLevel);
        }

        /// <summary>
        ///     Pokedex extra data doesn't change so we can just call this method once.        
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateItemTemplates()
        {
            // Get all the templates
            var itemTemplates = await DataCache.GetAsync("itemTemplates", async () => (await _client.Download.GetItemTemplates()).ItemTemplates, DateTime.Now.AddMonths(1));

            // Update Pokedex data
            PokemonSettings = await DataCache.GetAsync(nameof(PokemonSettings), async () =>
            {
                await Task.CompletedTask;
                return itemTemplates.Where(
                    item => item.PokemonSettings != null && item.PokemonSettings.FamilyId != PokemonFamilyId.FamilyUnset)
                    .Select(item => item.PokemonSettings);
            }, DateTime.Now.AddMonths(1));

            PokemonUpgradeCosts = await DataCache.GetAsync(nameof(PokemonUpgradeCosts), async () =>
            {
                await Task.CompletedTask;
                // Update Pokemon upgrade templates
                var tmpPokemonUpgradeCosts = itemTemplates.First(item => item.PokemonUpgrades != null).PokemonUpgrades;
                var tmpResult = new Dictionary<int, object[]>();
                for (var i = 0; i < tmpPokemonUpgradeCosts.CandyCost.Count; i++)
                {
                    tmpResult.Add(i,
                        new object[] { tmpPokemonUpgradeCosts.CandyCost[i], tmpPokemonUpgradeCosts.StardustCost[i] });
                }
                return tmpResult;
            }, DateTime.Now.AddMonths(1));


            // Update Moves data
            MoveSettings = await DataCache.GetAsync(nameof(MoveSettings), async () =>
            {
                await Task.CompletedTask;
                return itemTemplates.Where(item => item.MoveSettings != null)
                                    .Select(item => item.MoveSettings);
            }, DateTime.Now.AddMonths(1));
        }

        /// <summary>
        ///     Updates inventory data
        /// </summary>
        public static async Task UpdateInventory()
        {
            // Get ALL the items
            var response = await GetInventory();
            var fullInventory = response.InventoryDelta?.InventoryItems;
            // Update items
            ItemsInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.Item != null)
                .GroupBy(item => item.InventoryItemData.Item)
                .Select(item => item.First().InventoryItemData.Item), true);
            CatchItemsInventory.AddRange(
                fullInventory.Where(
                    item =>
                        item.InventoryItemData.Item != null && CatchItemIds.Contains(item.InventoryItemData.Item.ItemId))
                    .GroupBy(item => item.InventoryItemData.Item)
                    .Select(item => item.First().InventoryItemData.Item), true);

            // Update incbuators
            FreeIncubatorsInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.EggIncubators != null)
                .SelectMany(item => item.InventoryItemData.EggIncubators.EggIncubator)
                .Where(item => item != null && item.PokemonId == 0), true);
            UsedIncubatorsInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.EggIncubators != null)
                .SelectMany(item => item.InventoryItemData.EggIncubators.EggIncubator)
                .Where(item => item != null && item.PokemonId != 0), true);

            // Update Pokedex
            PokedexInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.PokedexEntry != null)
                .Select(item => item.InventoryItemData.PokedexEntry), true);

            // Update Pokemons
            PokemonsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                .Where(item => item != null && item.PokemonId > 0), true);
            EggsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                .Where(item => item != null && item.IsEgg), true);            

            // Update candies
            CandyInventory.AddRange(from item in fullInventory
                                    where item.InventoryItemData?.Candy != null
                                    where item.InventoryItemData?.Candy.FamilyId != PokemonFamilyId.FamilyUnset
                                    group item by item.InventoryItemData?.Candy.FamilyId into family
                                    select new Candy
                                    {
                                        FamilyId = family.FirstOrDefault().InventoryItemData.Candy.FamilyId,
                                        Candy_ = family.FirstOrDefault().InventoryItemData.Candy.Candy_
                                    }, true);

        }

        #endregion

        #region Pokemon Handling

        #region Pokedex

        /// <summary>
        ///     Gets extra data for the current pokemon
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <returns></returns>
        public static PokemonSettings GetExtraDataForPokemon(PokemonId pokemonId)
        {
            return PokemonSettings.First(pokemon => pokemon.PokemonId == pokemonId);
        }

        #endregion

        #region Catching

        /// <summary>
        ///     Encounters the selected Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <returns></returns>
        public static async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnpointId)
        {
            return await _client.Encounter.EncounterPokemon(encounterId, spawnpointId);
        }

        /// <summary>
        ///     Encounters the selected lured Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <returns></returns>
        public static async Task<DiskEncounterResponse> EncounterLurePokemon(ulong encounterId, string spawnpointId)
        {
            return await _client.Encounter.EncounterLurePokemon(encounterId, spawnpointId);
        }

        /// <summary>
        ///     Executes Pokemon catching
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="captureItem"></param>
        /// <param name="hitPokemon"></param>
        /// <returns></returns>
        public static async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnpointId,
            ItemId captureItem, bool hitPokemon = true)
        {
            var random = new Random();
            return
                await
                    _client.Encounter.CatchPokemon(encounterId, spawnpointId, captureItem, random.NextDouble() * 1.95D,
                        random.NextDouble(), 1, hitPokemon);
        }

        /// <summary>
        ///     Throws a capture item to the Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="captureItem"></param>
        /// <returns></returns>
        public static async Task<UseItemCaptureResponse> UseCaptureItem(ulong encounterId, string spawnpointId,
            ItemId captureItem)
        {
            return await _client.Encounter.UseCaptureItem(encounterId, captureItem, spawnpointId);
        }

        #endregion

        #region Power Up & Evolving & Transfer

        /// <summary>
        ///
        /// </summary>
        /// <param name="pokemon"></param>
        /// <returns></returns>
        public static async Task<UpgradePokemonResponse> PowerUpPokemon(PokemonData pokemon)
        {
            return await _client.Inventory.UpgradePokemon(pokemon.Id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pokemon"></param>
        /// <returns></returns>
        public static async Task<EvolvePokemonResponse> EvolvePokemon(PokemonData pokemon)
        {
            return await _client.Inventory.EvolvePokemon(pokemon.Id);
        }

        /// <summary>
        /// Transfers the Pokemon
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <returns></returns>
        public static async Task<ReleasePokemonResponse> TransferPokemon(ulong pokemonId)
        {
            return await _client.Inventory.TransferPokemon(pokemonId);
        }

        #endregion

        #endregion

        #region Pokestop Handling

        /// <summary>
        ///     Gets fort data for the given Id
        /// </summary>
        /// <param name="pokestopId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static async Task<FortDetailsResponse> GetFort(string pokestopId, double latitude, double longitude)
        {
            return await _client.Fort.GetFort(pokestopId, latitude, longitude);
        }

        /// <summary>
        ///     Searches the given fort
        /// </summary>
        /// <param name="pokestopId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static async Task<FortSearchResponse> SearchFort(string pokestopId, double latitude, double longitude)
        {
            return await _client.Fort.SearchFort(pokestopId, latitude, longitude);
        }

        #endregion

        #region Eggs Handling

        /// <summary>
        ///     Uses the selected incubator on the given egg
        /// </summary>
        /// <param name="incubator"></param>
        /// <param name="egg"></param>
        /// <returns></returns>
        public static async Task<UseItemEggIncubatorResponse> UseEggIncubator(EggIncubator incubator, PokemonData egg)
        {
            return await _client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id);
        }

        /// <summary>
        ///     Gets the incubator used by the given egg
        /// </summary>
        /// <param name="egg"></param>
        /// <returns></returns>
        public static EggIncubator GetIncubatorFromEgg(PokemonData egg)
        {
            return UsedIncubatorsInventory.First(item => item.Id.Equals(egg.EggIncubatorId));
        }

        #endregion

        #endregion
    }
}