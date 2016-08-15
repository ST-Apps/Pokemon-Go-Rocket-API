using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings;
using POGOProtos.Settings.Master;
using Q42.WinRT.Data;
using Template10.Common;
using Template10.Utils;
using Universal_Authenticator_v2.Views;

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
        ///     Handles failures by having a fixed number of retries
        /// </summary>
        internal class ApiFailure : IApiFailureStrategy
        {
            private const int MaxRetries = 50;

            private int _retryCount;


            public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
            {
                if (_retryCount == MaxRetries)
                    return ApiOperation.Abort;

                await Task.Delay(500);
                _retryCount++;

                if (_retryCount % 5 == 0)
                {
                    await DoRelogin();
                    Debug.WriteLine("[Relogin] Stopping API via ApiHandledException.");
                    throw new ApiHandledException("Relogin completed.");
                }

                return ApiOperation.Retry;
            }

            public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
            {
                _retryCount = 0;
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
        public static ObservableCollection<PokedexEntry> PokedexInventory { get; set; } =
            new ObservableCollection<PokedexEntry>();

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

        #region Game Logic

        #region Login/Logout

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

            _client = new Client(_clientSettings, new ApiFailure(), DeviceInfos.Instance)
            {
                AuthToken = SettingsService.Instance.AuthToken
            };
            try
            {
                await _client.Login.DoLogin();
            }
            catch (Exception e)
            {
                if (e is PokemonGo.RocketAPI.Exceptions.AccessTokenExpiredException)
                {
                    await Relogin();
                }
                else throw;
            }
        }
        public static async Task<bool> Relogin()
        {
            switch (_clientSettings.AuthType)
            {
                case AuthType.Ptc:
                    {
                        return await DoPtcLogin(_clientSettings.PtcUsername, _clientSettings.PtcPassword);
                    }
                case AuthType.Google:
                    {
                        return await DoGoogleLogin(_clientSettings.GoogleUsername, _clientSettings.GooglePassword);
                    }
                default:
                    {
                        throw new InvalidOperationException();
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
            _client = new Client(_clientSettings, new ApiFailure(), DeviceInfos.Instance);
            // Get PTC token
            var authToken = await _client.Login.DoLogin();
            // Update current token even if it's null and clear the token for the other identity provide
            SettingsService.Instance.AuthToken = authToken;
            // Update other data if login worked
            if (authToken == null) return false;
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

            _client = new Client(_clientSettings, new ApiFailure(), DeviceInfos.Instance);
            // Get Google token
            var authToken = await _client.Login.DoLogin();
            // Update current token even if it's null
            SettingsService.Instance.AuthToken = authToken;
            // Update other data if login worked
            if (authToken == null) return false;
            SettingsService.Instance.LastLoginService = AuthType.Google;
            SettingsService.Instance.UserCredentials =
                new PasswordCredential(nameof(SettingsService.Instance.UserCredentials), email, password);
            // Return true if login worked, meaning that we have a token
            return true;
        }

        /// <summary>
        ///     Logs the user out by clearing data and timers
        /// </summary>
        public static void DoLogout()
        {
            // Clear stored token
            SettingsService.Instance.AuthToken = null;
            SettingsService.Instance.UserCredentials = null;
            _mapUpdateTimer?.Stop();
            _mapUpdateTimer = null;
            _geolocator = null;
            CatchablePokemons.Clear();
            NearbyPokemons.Clear();
            NearbyPokestops.Clear();
        }

        public static async Task DoRelogin()
        {
            Debug.WriteLine("[Relogin] Started.");
            DoLogout();

            var token = _client.AuthToken;

            await
                (_clientSettings.AuthType == AuthType.Google
                    ? DoGoogleLogin(_clientSettings.GoogleUsername, _clientSettings.GooglePassword)
                    : DoPtcLogin(_clientSettings.PtcUsername, _clientSettings.PtcPassword));

            if (token != _client.AuthToken)
                Debug.WriteLine("[Relogin] Token successfuly changed.");

            Debug.WriteLine("[Relogin] Reloading gps and playerdata.");
            await InitializeDataUpdate();
            await UpdateProfile();
            await UpdatePlayerStats();
            Debug.WriteLine("[Relogin] Restarting MapUpdate timer.");
            _lastUpdate = DateTime.Now;
            await ToggleUpdateTimer();
        }

        #endregion

        #region Data Updating

        private static Geolocator _geolocator;

        public static Geoposition Geoposition { get; private set; }

        private static DispatcherTimer _mapUpdateTimer;

        /// <summary>
        ///     We fire this event when the current position changes
        /// </summary>
        public static event EventHandler<Geoposition> GeopositionUpdated;

        /// <summary>
        ///     Starts the timer to update map objects and the handler to update position
        /// </summary>
        public static async Task InitializeDataUpdate()
        {
            _geolocator = new Geolocator
            {
                DesiredAccuracy = PositionAccuracy.High,
                DesiredAccuracyInMeters = 5,
                ReportInterval = 5000,
                MovementThreshold = 5
            };
            Busy.SetBusy(true, Resources.CodeResources.GetString("GettingGpsSignalText"));
            Geoposition = Geoposition ?? await _geolocator.GetGeopositionAsync();
            GeopositionUpdated?.Invoke(null, Geoposition);
            _geolocator.PositionChanged += async (s, e) =>
            {
                Geoposition = e.Position;
                // Updating player's position
                var position = Geoposition.Coordinate.Point.Position;
                await _client.Player.UpdatePlayerLocation(position.Latitude, position.Longitude, position.Altitude);
                GeopositionUpdated?.Invoke(null, Geoposition);
            };
            // Before starting we need game settings
            GameSetting =
                await
                    DataCache.GetAsync(nameof(GameSetting), async () => (await _client.Download.GetSettings()).Settings,
                        DateTime.Now.AddMonths(1));
            // Update geolocator settings based on server
            _geolocator.MovementThreshold = GameSetting.MapSettings.GetMapObjectsMinDistanceMeters;
            _mapUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(GameSetting.MapSettings.GetMapObjectsMinRefreshSeconds)
            };
            _mapUpdateTimer.Tick += async (s, e) =>
            {
                // Update before starting but only if more than 10s passed since the last one
                if ((DateTime.Now - _lastUpdate).Seconds <= GameSetting.MapSettings.GetMapObjectsMinRefreshSeconds)
                    return;
                Logger.Write("Updating map");

                try
                {
                    await UpdateMapObjects();
                }
                catch (Exception ex)
                {
                    await ExceptionHandler.HandleException(ex);
                }
            };
            // Update before starting timer
            Busy.SetBusy(true, Resources.CodeResources.GetString("GettingUserDataText"));
            await UpdateMapObjects();
            await UpdateInventory();
            await UpdateItemTemplates();
            Busy.SetBusy(false);
        }

        /// <summary>
        ///     DateTime for the last map update
        /// </summary>
        private static DateTime _lastUpdate;

        /// <summary>
        ///     Toggles the update timer based on the isEnabled value
        /// </summary>
        /// <param name="isEnabled"></param>
        public static async Task ToggleUpdateTimer(bool isEnabled = true)
        {
            if (isEnabled)
            {
                if (_mapUpdateTimer.IsEnabled) return;
                // Update before starting but only if more than 10s passed since the last one
                if ((DateTime.Now - _lastUpdate).Seconds > GameSetting.MapSettings.GetMapObjectsMinRefreshSeconds)
                    await UpdateMapObjects();
                _mapUpdateTimer.Start();
            }
            else
            {
                _mapUpdateTimer.Stop();
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
            var newCatchablePokemons = mapObjects.Item1.MapCells.SelectMany(x => x.CatchablePokemons).ToArray();
            Logger.Write($"Found {newCatchablePokemons.Length} catchable pokemons");
            CatchablePokemons.UpdateWith(newCatchablePokemons, x => new MapPokemonWrapper(x),
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

            Logger.Write("Finished updating map objects");
        }

        #endregion

        #region Map & Position

        /// <summary>
        ///     Gets updated map data based on provided position
        /// </summary>
        /// <param name="geoposition"></param>
        /// <returns></returns>
        public static async
            Task
                <
                    Tuple
                        <GetMapObjectsResponse, GetHatchedEggsResponse, GetInventoryResponse, CheckAwardedBadgesResponse,
                            DownloadSettingsResponse>> GetMapObjects(Geoposition geoposition)
        {
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

            if (checkForLevelUp && PlayerStats != null && PlayerStats.Level != tmpStats.Level)
            {
                PlayerStats = tmpStats;
                var levelUpResponse = await GetLevelUpRewards(tmpStats.Level);
                return levelUpResponse;
            }
            PlayerStats = tmpStats;

            // Update candies
            CandyInventory.AddRange(from item in InventoryDelta.InventoryItems
                                    where item.InventoryItemData?.Candy != null
                                    where item.InventoryItemData?.Candy.FamilyId != PokemonFamilyId.FamilyUnset
                                    group item by item.InventoryItemData?.Candy.FamilyId into family
                                    select new Candy
                                    {
                                        FamilyId = family.FirstOrDefault().InventoryItemData.Candy.FamilyId,
                                        Candy_ = family.FirstOrDefault().InventoryItemData.Candy.Candy_
                                    }, true);

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
        ///     TODO: store it in local settings maybe?
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
            var fullInventory = (await GetInventory()).InventoryDelta.InventoryItems;
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

            // Update Pokemons
            PokemonsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                .Where(item => item != null && item.PokemonId > 0), true);
            EggsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                .Where(item => item != null && item.IsEgg), true);

            // Update Pokedex
            PokedexInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.PokedexEntry != null)
                .Select(item => item.InventoryItemData.PokedexEntry), true);

            // Update Player stats
            PlayerStats =
                fullInventory.First(item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;

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