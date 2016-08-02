using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using AllEnum;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logic;
using PokemonGo_UWP.Entities;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Static class containing game's state and wrapped client methods to update data
    /// </summary>
    public static class GameClient
    {
        #region Client Vars

        private static readonly ISettings ClientSettings = new Settings();
        private static readonly Client Client = new Client(ClientSettings);
        private static readonly Inventory InventoryWrapper = new Inventory(Client);

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
        public static ObservableCollection<Item> Inventory { get; set; } = new ObservableCollection<Item>();

        #endregion

        #region Game Logic

        #region Login/Logout

        /// <summary>
        ///     Sets things up if we didn't come from the login page
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeClient()
        {
            await Client.SetServer(SettingsService.Instance.PtcAuthToken);
        }

        /// <summary>
        ///     Starts a PTC session for the given user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true if login worked</returns>
        public static async Task<bool> DoPtcLogin(string username, string password)
        {
            // Get PTC token
            var authToken = await Client.DoPtcLogin(username, password);
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
            var mapObjects = await GetMapObjects(Geoposition);
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
        public static async Task<GetMapObjectsResponse> GetMapObjects(Geoposition geoposition)
        {
            // Sends the updated position to the client
            await
                Client.UpdatePlayerLocation(geoposition.Coordinate.Point.Position.Latitude,
                    geoposition.Coordinate.Point.Position.Longitude);
            return await Client.GetMapObjects();
        }

        #endregion

        #region Player Data & Inventory

        /// <summary>
        ///     Gets user's profile
        /// </summary>
        /// <returns></returns>
        public static async Task<GetPlayerResponse> GetProfile()
        {
            return await Client.GetProfile();
        }

        /// <summary>
        ///     Gets player's inventoryDelta
        /// </summary>
        /// <returns></returns>
        public static async Task<GetInventoryResponse> GetInventoryDelta()
        {
            return await Client.GetInventory();
        }

        /// <summary>
        ///     Updates inventory data
        /// </summary>
        public static async Task UpdateInventory()
        {
            var inventoryTmp = new List<Item>(await InventoryWrapper.GetItems());
            Inventory.Clear();
            foreach (var item in inventoryTmp)
            {
                Inventory.Add(item);
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
            return await Client.EncounterPokemon(encounterId, spawnpointId);
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
        public static async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnpointId,  double latitude, double longitude, MiscEnums.Item captureItem)
        {
            return await Client.CatchPokemon(encounterId, spawnpointId, latitude, longitude, captureItem);
        }

        /// <summary>
        /// Throws a capture item to the Pokemon
        /// </summary>
        /// <param name="encounterId"></param>
        /// <param name="spawnpointId"></param>
        /// <param name="captureItem"></param>
        /// <returns></returns>
        public static async Task<UseItemCaptureRequest> UseCaptureItem(ulong encounterId, string spawnpointId,
            ItemId captureItem)
        {
            return await Client.UseCaptureItem(encounterId, captureItem, spawnpointId);
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
            return await Client.GetFort(pokestopId, latitude, longitude);
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
            return await Client.SearchFort(pokestopId, latitude, longitude);
        }

        #endregion

        #endregion
    }
}