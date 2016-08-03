using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Envelopes;
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
using InventoryItem = POGOProtos.Inventory.InventoryItem;
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
            private const int _maxRetries = 50;

            public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
            {
                await Task.Delay(500);
                _retryCount++;
                return _retryCount < _maxRetries ? ApiOperation.Retry : ApiOperation.Abort;
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
        public static ObservableCollection<NearbyPokemon> NearbyPokemons { get; set; } = new ObservableCollection<NearbyPokemon>();

        /// <summary>
        ///     Collection of Pokestops in the current area
        /// </summary>
        public static ObservableCollection<FortDataWrapper> NearbyPokestops { get; set; } = new ObservableCollection<FortDataWrapper>();

        /// <summary>
        ///     Stores the current inventory
        /// </summary>
        public static ObservableCollection<ItemData> ItemsInventory { get; set; } = new ObservableCollection<ItemData>();

        #endregion

        #region Game Logic

        #region Login/Logout

        /// <summary>
        ///     Sets things up if we didn't come from the login page
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeClient()
        {
            ClientSettings = new Settings
            {                
                AuthType = AuthType.Ptc
            };
            Client = new Client(ClientSettings, new APIFailure()) {AuthToken = SettingsService.Instance.PtcAuthToken};
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
            // Update current token even if it's null
            SettingsService.Instance.PtcAuthToken = authToken;
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
        /// Mutex to secure the parallel access to the data update
        /// </summary>
        private static readonly Mutex UpdateDataMutex = new Mutex();

        /// <summary>
        /// If a forced refresh caused an update we should skip the next update
        /// </summary>
        private static bool _skipNextUpdate;

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
            Busy.SetBusy(true, "Getting GPS signal...");
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
                if (!UpdateDataMutex.WaitOne(0)) return;
                if (_skipNextUpdate)
                {
                    _skipNextUpdate = false;
                }
                else
                {
                    Logger.Write("Updating map");
                    await UpdateMapObjects();
                }

                UpdateDataMutex.ReleaseMutex();                
            };
            // Update before starting timer            
            Busy.SetBusy(true, "Getting user data...");
            await UpdateMapObjects();
            await UpdateInventory();
            _mapUpdateTimer.Start();
            Busy.SetBusy(false);
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
            // Replace data with the new ones                                  
            var catchableTmp = new List<MapPokemon>(mapObjects.MapCells.SelectMany(i => i.CatchablePokemons));
            Logger.Write($"Found {catchableTmp.Count} catchable pokemons");
            if (catchableTmp.Count != CatchablePokemons.Count)
            {
                MapPokemonUpdated?.Invoke(null, null);
            }
            CatchablePokemons.Clear();
            foreach (var pokemon in catchableTmp)
            {
                CatchablePokemons.Add(new MapPokemonWrapper(pokemon));
            }
            var nearbyTmp = new List<NearbyPokemon>(mapObjects.MapCells.SelectMany(i => i.NearbyPokemons));
            Logger.Write($"Found {nearbyTmp.Count} nearby pokemons");
            NearbyPokemons.Clear();           
            foreach (var pokemon in nearbyTmp)
            {
                NearbyPokemons.Add(pokemon);
            }
            // Retrieves PokeStops but not Gyms
            var pokeStopsTmp =
                new List<FortData>(mapObjects.MapCells.SelectMany(i => i.Forts)
                    .Where(i => i.Type == FortType.Checkpoint));
            Logger.Write($"Found {pokeStopsTmp.Count} nearby PokeStops");
            NearbyPokestops.Clear();
            foreach (var pokestop in pokeStopsTmp)
            {
                NearbyPokestops.Add(new FortDataWrapper(pokestop));
            }
            Logger.Write("Finished updating map objects");
        }

        public static async Task ForcedUpdateMapData()
        {
            if (!UpdateDataMutex.WaitOne(0)) return;
            _skipNextUpdate = true;
            await UpdateMapObjects();
            UpdateDataMutex.ReleaseMutex();
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
        ///     Updates inventory data
        /// </summary>
        public static async Task UpdateInventory()
        {            
            // Get ALL the items
            var fullInventory = (await GetInventory()).InventoryDelta.InventoryItems;
            var tmpItemsInventory = fullInventory.Where(item => item.InventoryItemData.Item != null).GroupBy(item => item.InventoryItemData.Item);
            ItemsInventory.Clear();
            foreach (var item in tmpItemsInventory)
            {
                ItemsInventory.Add(item.First().InventoryItemData.Item);
            }
        }

        #endregion

        #region Pokemon Handling

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
        /// <returns></returns>
        public static async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnpointId, ItemId captureItem)
        {                        
            var random = new Random();
            return await Client.Encounter.CatchPokemon(encounterId, spawnpointId, captureItem, random.NextDouble()*1.95D, random.NextDouble());
        }

        /// <summary>
        /// Throws a capture item to the Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="captureItem"></param>
        /// <returns></returns>
        public static async Task<UseItemCaptureResponse> UseCaptureItem(ulong encounterId, string spawnpointId,ItemId captureItem)
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

        #endregion
    }
}