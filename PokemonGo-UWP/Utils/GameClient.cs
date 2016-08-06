using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using Template10.Utils;
using Universal_Authenticator_v2.Views;
using CatchPokemonResponse = POGOProtos.Networking.Responses.CatchPokemonResponse;
using CheckAwardedBadgesResponse = POGOProtos.Networking.Responses.CheckAwardedBadgesResponse;
using DownloadSettingsResponse = POGOProtos.Networking.Responses.DownloadSettingsResponse;
using EncounterResponse = POGOProtos.Networking.Responses.EncounterResponse;
using FortDetailsResponse = POGOProtos.Networking.Responses.FortDetailsResponse;
using FortSearchResponse = POGOProtos.Networking.Responses.FortSearchResponse;
using GetHatchedEggsResponse = POGOProtos.Networking.Responses.GetHatchedEggsResponse;
using GetInventoryResponse = POGOProtos.Networking.Responses.GetInventoryResponse;
using GetMapObjectsResponse = POGOProtos.Networking.Responses.GetMapObjectsResponse;
using GetPlayerResponse = POGOProtos.Networking.Responses.GetPlayerResponse;
using MapPokemon = POGOProtos.Map.Pokemon.MapPokemon;
using NearbyPokemon = POGOProtos.Map.Pokemon.NearbyPokemon;
using UseItemCaptureResponse = POGOProtos.Networking.Responses.UseItemCaptureResponse;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Static class containing game's state and wrapped client methods to update data
    /// </summary>
    public static class GameClient
    {
        #region Client Vars

        private static ISettings ClientSettings;
        private static Client Client;

        /// <summary>
        /// Handles failures by having a fixed number of retries
        /// </summary>
        internal class APIFailure : IApiFailureStrategy
        {

            private int _retryCount;
            private const int MaxRetries = 50;


            public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
            {
                if (_retryCount == MaxRetries)
                    return ApiOperation.Abort;

                await Task.Delay(500);
                _retryCount++;

                if (_retryCount % 5 == 0)
                {
                    // Let's try to refresh the session by getting a new token
                    await (ClientSettings.AuthType == AuthType.Google ? DoGoogleLogin(ClientSettings.GoogleUsername, ClientSettings.GooglePassword) : DoPtcLogin(ClientSettings.PtcUsername, ClientSettings.PtcPassword));
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
        ///     Collection of Pokemon in 1 step from current position
        /// </summary>
        public static ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } = new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        ///     Collection of Pokemon in 2 steps from current position
        /// </summary>
        public static ObservableCollection<NearbyPokemonWrapper> NearbyPokemons { get; set; } = new ObservableCollection<NearbyPokemonWrapper>();

        /// <summary>
        ///     Collection of Pokestops in the current area
        /// </summary>
        public static ObservableCollection<FortDataWrapper> NearbyPokestops { get; set; } = new ObservableCollection<FortDataWrapper>();

        /// <summary>
        ///     Stores Items in the current inventory
        /// </summary>
        public static ObservableCollection<ItemData> ItemsInventory { get; set; } = new ObservableCollection<ItemData>();

        /// <summary>
        ///     Stores Items that can be used to catch a Pokemon
        /// </summary>
        public static ObservableCollection<ItemData> CatchItemsInventory { get; set; } = new ObservableCollection<ItemData>();

        /// <summary>
        ///     Stores Incubators in the current inventory
        /// </summary>
        public static ObservableCollection<EggIncubator> IncubatorsInventory { get; set; } = new ObservableCollection<EggIncubator>();

        /// <summary>
        /// Stores Pokemons in the current inventory
        /// </summary>
        public static ObservableCollection<PokemonData> PokemonsInventory { get; set; } = new ObservableCollection<PokemonData>();

        /// <summary>
        /// Stores Eggs in the current inventory
        /// </summary>
        public static ObservableCollection<PokemonData> EggsInventory { get; set; } = new ObservableCollection<PokemonData>();

        /// <summary>
        /// Stores player's current Pokedex
        /// </summary>
        public static ObservableCollection<PokedexEntry> PokedexInventory { get; set; } = new ObservableCollection<PokedexEntry>();

        /// <summary>
        /// Stores extra useful data for the Pokedex, like Pokemon type and other stuff that is missing from PokemonData
        /// </summary>
        public static IEnumerable<PokemonSettings> PokedexExtraData { get; set; } = new List<PokemonSettings>();

        #endregion

        #region Game Logic

        #region Login/Logout

        /// <summary>
        ///     Sets things up if we didn't come from the login page
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeClient(bool isPtcAccount)
        {
            var isPtcLogin = !string.IsNullOrWhiteSpace(SettingsService.Instance.PtcAuthToken);

            ClientSettings = new Settings
            {
                AuthType = isPtcLogin ? AuthType.Ptc : AuthType.Google
            };

            Client = new Client(ClientSettings, new APIFailure()) { AuthToken = SettingsService.Instance.PtcAuthToken ?? SettingsService.Instance.GoogleAuthToken };

            await Client.Login.DoLogin();
        }

        /// <summary>
        ///     Starts a PTC session for the given user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true if login worked</returns>
        public static async Task<bool> DoPtcLogin(string username, string password)
        {
            ClientSettings = new Settings
            {
                PtcUsername = username,
                PtcPassword = password,
                AuthType = AuthType.Ptc
            };
            Client = new Client(ClientSettings, new APIFailure());
            // Get PTC token
            var authToken = await Client.Login.DoLogin();
            // Update current token even if it's null and clear the token for the other identity provide
            SettingsService.Instance.PtcAuthToken = authToken;
            SettingsService.Instance.GoogleAuthToken = null;
            // Return true if login worked, meaning that we have a token
            return authToken != null;
        }

        /// <summary>
        ///     Starts a Google session for the given user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>true if login worked</returns>
        public static async Task<bool> DoGoogleLogin(string email, string password)
        {
            ClientSettings = new Settings
            {
                GoogleUsername = email,
                GooglePassword = password,
                AuthType = AuthType.Google,
            };

            Client = new Client(ClientSettings, new APIFailure());
            // Get Google token
            var authToken = await Client.Login.DoLogin();
            // Update current token even if it's null
            SettingsService.Instance.GoogleAuthToken = authToken;
            // Return true if login worked, meaning that we have a token
            return authToken != null;
        }

        /// <summary>
        /// Logs the user out by clearing data and timers
        /// </summary>
        public static void DoLogout()
        {
            // Clear stored token
            SettingsService.Instance.PtcAuthToken = null;
            SettingsService.Instance.GoogleAuthToken = null;
            _mapUpdateTimer?.Stop();
            _mapUpdateTimer = null;
            _geolocator = null;
            CatchablePokemons.Clear();
            NearbyPokemons.Clear();
            NearbyPokestops.Clear();
        }

        #endregion

        #region Data Updating

        private static Geolocator _geolocator;

        public static Geoposition Geoposition { get; private set; }

        private static DispatcherTimer _mapUpdateTimer;

        /// <summary>
        /// We fire this event when the current position changes
        /// </summary>
        public static event EventHandler<Geoposition> GeopositionUpdated;

        /// <summary>
        /// We fire this event when we have found new Pokemons on the map
        /// </summary>
        public static event EventHandler MapPokemonUpdated;

        /// <summary>
        /// Starts the timer to update map objects and the handler to update position
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
            Busy.SetBusy(true, Resources.Translation.GetString("GettingGPSSignal"));
            Geoposition = Geoposition ?? await _geolocator.GetGeopositionAsync();
            GeopositionUpdated?.Invoke(null, Geoposition);
            _geolocator.PositionChanged += (s, e) =>
            {
                Geoposition = e.Position;
                GeopositionUpdated?.Invoke(null, Geoposition);
            };
            _mapUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _mapUpdateTimer.Tick += async (s, e) =>
            {
                Logger.Write("Updating map");
                await UpdateMapObjects();
            };            
            // Update before starting timer            
            Busy.SetBusy(true, Resources.Translation.GetString("GettingUserData"));
            await UpdateMapObjects();
            await UpdateInventory();
            await UpdatePokedex();
            Busy.SetBusy(false);
        }

        /// <summary>
        /// Toggles the update timer based on the isEnabled value
        /// </summary>
        /// <param name="isEnabled"></param>
        public static void ToggleUpdateTimer(bool isEnabled = true)
        {
            if (isEnabled)
            {      
                _mapUpdateTimer.Start();          
            }
            else
            {
                _mapUpdateTimer.Stop();
            }
        }

        /// <summary>
        /// Updates catcheable and nearby Pokemons + Pokestops.
        /// We're using a single method so that we don't need two separate calls to the server, making things faster.
        /// </summary>
        /// <returns></returns>
        private static async Task UpdateMapObjects()
        {
            // Get all map objects from server
            var mapObjects = (await GetMapObjects(Geoposition)).Item1;

            // update catchable pokemons
            var newCatchablePokemons = mapObjects.MapCells.SelectMany(x => x.CatchablePokemons).ToArray();
            Logger.Write($"Found {newCatchablePokemons.Length} catchable pokemons");
            if (newCatchablePokemons.Length != CatchablePokemons.Count)
            {
                MapPokemonUpdated?.Invoke(null, null);
            }
            CatchablePokemons.UpdateWith(newCatchablePokemons, x => new MapPokemonWrapper(x), (x, y) => x.EncounterId == y.EncounterId);

            // update nearby pokemons
            var newNearByPokemons = mapObjects.MapCells.SelectMany(x => x.NearbyPokemons).ToArray();
            Logger.Write($"Found {newNearByPokemons.Length} nearby pokemons");
            // for this collection the ordering is important, so we follow a slightly different update mechanism 
            NearbyPokemons.UpdateByIndexWith(newNearByPokemons, x => new NearbyPokemonWrapper(x));

            // update poke stops on map (gyms are ignored for now)
            var newPokeStops = mapObjects.MapCells
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
        public static async Task<Tuple<GetMapObjectsResponse, GetHatchedEggsResponse, POGOProtos.Networking.Responses.GetInventoryResponse, CheckAwardedBadgesResponse, DownloadSettingsResponse>> GetMapObjects(Geoposition geoposition)
        {
            // Sends the updated position to the client
            await
                Client.Player.UpdatePlayerLocation(geoposition.Coordinate.Point.Position.Latitude,
                    geoposition.Coordinate.Point.Position.Longitude, geoposition.Coordinate.Point.Position.Altitude);
            return await Client.Map.GetMapObjects();
        }

        #endregion

        #region Player Data & Inventory

        /// <summary>
        /// List of items that can be used when trying to catch a Pokemon
        /// </summary>
        private static readonly List<ItemId> _catchItemIds = new List<ItemId>()
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
        public static async Task<GetPlayerResponse> GetProfile()
        {
            return await Client.Player.GetPlayer();
        }

        /// <summary>
        ///     Gets player's inventoryDelta
        /// </summary>
        /// <returns></returns>
        public static async Task<GetInventoryResponse> GetInventory()
        {
            return await Client.Inventory.GetInventory();
        }

        /// <summary>
        /// Gets the rewards after leveling up
        /// </summary>
        /// <returns></returns>
        public static async Task<LevelUpRewardsResponse> GetLevelUpRewards(int newLevel)
        {
            return await Client.Player.GetLevelUpRewards(newLevel);
        }

        /// <summary>
        /// Pokedex extra data doesn't change so we can just call this method once.
        /// TODO: store it in local settings maybe?
        /// </summary>
        /// <returns></returns>
        private static async Task UpdatePokedex()
        {
            // Update Pokedex data
            PokedexExtraData = (await Client.Download.GetItemTemplates()).ItemTemplates.Where(item => item.PokemonSettings != null && item.PokemonSettings.FamilyId != PokemonFamilyId.FamilyUnset).Select(item => item.PokemonSettings);
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
            CatchItemsInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.Item != null && _catchItemIds.Contains(item.InventoryItemData.Item.ItemId))
                                                 .GroupBy(item => item.InventoryItemData.Item)
                                                 .Select(item => item.First().InventoryItemData.Item), true);
            // Update incbuators          
            // TODO: check if unused incubators have pokemonId = 0 to separate between sable and non-usable incubators
            IncubatorsInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.EggIncubators != null)
                                                      .SelectMany(item => item.InventoryItemData.EggIncubators.EggIncubator)
                                                      .Where(item => item != null && item.PokemonId != 0 && (item.UsesRemaining > 0 || item?.ItemId == ItemId.ItemIncubatorBasicUnlimited)), true);            
            // Update Pokemons
            PokemonsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                                                    .Where(item => item != null && item.PokemonId > 0),true);
            EggsInventory.AddRange(fullInventory.Select(item => item.InventoryItemData.PokemonData)
                                                .Where(item => item != null && item.IsEgg), true); 
            // Update Pokedex            
            PokedexInventory.AddRange(fullInventory.Where(item => item.InventoryItemData.PokedexEntry != null)
                                                   .Select(item => item.InventoryItemData.PokedexEntry), true);
        }

        #endregion

        #region Pokemon Handling

        #region Pokedex        

        /// <summary>
        /// Gets extra data for the current pokemon
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <returns></returns>
        public static PokemonSettings GetExtraDataForPokemon(PokemonId pokemonId)
        {
            return PokedexExtraData.First(pokemon => pokemon.PokemonId == pokemonId);
        }

        #endregion

        #region Catching

        /// <summary>
        /// Encounters the selected Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <returns></returns>
        public static async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnpointId)
        {
            return await Client.Encounter.EncounterPokemon(encounterId, spawnpointId);
        }

        /// <summary>
        /// Executes Pokemon catching
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="longitude"></param>
        /// <param name="captureItem"></param>
        /// <param name="latitude"></param>
        /// <param name="shotMissed"></param>
        /// <returns></returns>
        public static async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnpointId, ItemId captureItem, bool hitPokemon = true)
        {
            var random = new Random();
            return await Client.Encounter.CatchPokemon(encounterId, spawnpointId, captureItem, random.NextDouble() * 1.95D, random.NextDouble(), 1, hitPokemon);
        }

        /// <summary>
        /// Throws a capture item to the Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="captureItem"></param>
        /// <returns></returns>
        public static async Task<UseItemCaptureResponse> UseCaptureItem(ulong encounterId, string spawnpointId, ItemId captureItem)
        {
            return await Client.Encounter.UseCaptureItem(encounterId, captureItem, spawnpointId);
        }

        #endregion

        #endregion

        #region Pokestop Handling

        /// <summary>
        /// Gets fort data for the given Id
        /// </summary>
        /// <param name="pokestopId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static async Task<FortDetailsResponse> GetFort(string pokestopId, double latitude, double longitude)
        {
            return await Client.Fort.GetFort(pokestopId, latitude, longitude);
        }

        /// <summary>
        /// Searches the given fort
        /// </summary>
        /// <param name="pokestopId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public static async Task<FortSearchResponse> SearchFort(string pokestopId, double latitude, double longitude)
        {
            return await Client.Fort.SearchFort(pokestopId, latitude, longitude);
        }

        #endregion

        #region Eggs Handling

        /// <summary>
        /// Uses the selected incubator on the given egg
        /// </summary>
        /// <param name="incubator"></param>
        /// <param name="egg"></param>
        /// <returns></returns>
        public static async Task<UseItemEggIncubatorResponse> UseEggIncubator(EggIncubator incubator, PokemonData egg)
        {
            return await Client.Inventory.UseItemEggIncubator(incubator.Id, egg.Id);
        }

        /// <summary>
        /// Gets the incubator used by the given egg
        /// </summary>
        /// <param name="egg"></param>
        /// <returns></returns>
        public static EggIncubator GetIncubatorFromEgg(PokemonData egg)
        {
            return IncubatorsInventory.First(item => item.Id.Equals(egg.EggIncubatorId));
        }

        #endregion

        #endregion
    }
}