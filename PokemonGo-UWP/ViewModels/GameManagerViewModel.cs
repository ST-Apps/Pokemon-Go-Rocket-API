using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation.Metadata;
using Windows.Phone.Devices.Notification;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using AllEnum;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Console;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo.RocketAPI.Logging;
using PokemonGo.RocketAPI.Logic;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;
using System.Threading;
using Windows.ApplicationModel;

namespace PokemonGo_UWP.ViewModels
{
    /// <summary>
    ///     Main class for the game.
    ///     This handles connection to client and UI updating via binding.
    /// </summary>
    public class GameManagerViewModel : ViewModelBase
    {
        #region ctor

        public GameManagerViewModel()
        {
            // Client init            
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
            _clientSettings = new Settings();
            _client = new Client(_clientSettings);
            _inventory = new Inventory(_client);

            if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                _vibrationDevice = VibrationDevice.GetDefault();
            }
        }

        #endregion

        #region Lifecycle Handlers

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {            
            if (suspensionState.Any() || (parameter != null && mode == NavigationMode.New))
            {
                // Restoring from suspend or having that parameter means that we need to initialize the game again
                await InitGame(true);
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                // We save this value just to report that suspension happened with an open session
                suspensionState[nameof(SettingsService.Instance.PtcAuthToken)] = SettingsService.Instance.PtcAuthToken;
                // Don't update data if app is in background
                _updateDataTimer?.Cancel();
                _updateDataTimer = null;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Client Vars

        private readonly Client _client;
        private readonly ISettings _clientSettings;
        private readonly Inventory _inventory;

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     We use it to notify that we found at least one catchable Pokemon in our area
        /// </summary>
        private readonly VibrationDevice _vibrationDevice;

        /// <summary>
        ///     True if the phone can vibrate (e.g. the app is not in background)
        /// </summary>
        public bool CanVibrate;

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        private Profile _playerProfile;

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
        ///     Pokemon that we're trying to capture
        /// </summary>
        private MapPokemonWrapper _currentPokemon;

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        private EncounterResponse _currentEncounter;

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        private Item _selectedCaptureItem;

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        private CaptureScore _currentCaptureScore;

        /// <summary>
        ///     True if we're in catching mode or in fort searching, so that we can avoid updating the map with the timer
        /// </summary>
        private bool _stopUpdatingMap;

        /// <summary>
        ///     Pokestop that the user is visiting
        /// </summary>
        private FortData _currentPokestop;

        /// <summary>
        ///     Infos on the current Pokestop
        /// </summary>
        private FortDetailsResponse _currentPokestopInfo;

        /// <summary>
        ///     Results of the current Pokestop search
        /// </summary>
        private FortSearchResponse _currentSearchResponse;

        /// <summary>
        /// Timer used to update game data
        /// </summary>
        private ThreadPoolTimer _updateDataTimer;

        /// <summary>
        /// Mutex to secure the parallel access to the data update
        /// </summary>
        private Mutex _updateDataMutex = new Mutex();

        /// <summary>
        /// If a forced refresh caused an update we should skip the next update
        /// </summary>
        private bool _skipNextUpdate = false;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        /// App's current version
        /// </summary>
        public string CurrentVersion
        {
            get
            {
                var currentVersion = Package.Current.Id.Version;
                return $"v{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}";
            }
        }

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public Profile PlayerProfile
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

        public InventoryDelta InventoryDelta
        {
            get { return _inventoryDelta; }
            set { Set(ref _inventoryDelta, value); }
        }

        /// <summary>
        ///     Collection of Pokemon in 1 step from current position
        /// </summary>
        public ObservableCollection<MapPokemonWrapper> CatchablePokemons { get; set; } =
            new ObservableCollection<MapPokemonWrapper>();

        /// <summary>
        ///     Collection of Pokemon in 2 steps from current position
        /// </summary>
        public ObservableCollection<NearbyPokemon> NearbyPokemons { get; set; } =
            new ObservableCollection<NearbyPokemon>();

        /// <summary>
        ///     Collection of Pokestops in the current area
        /// </summary>
        public ObservableCollection<FortData> NearbyPokestops { get; set; } = new ObservableCollection<FortData>();

        /// <summary>
        ///     Stores the current inventory
        /// </summary>
        public ObservableCollection<Item> Inventory { get; set; } = new ObservableCollection<Item>();

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public MapPokemonWrapper CurrentPokemon
        {
            get { return _currentPokemon; }
            set { Set(ref _currentPokemon, value); }
        }

        /// <summary>
        ///     Encounter for the Pokemon that we're trying to capture
        /// </summary>
        public EncounterResponse CurrentEncounter
        {
            get { return _currentEncounter; }
            set { Set(ref _currentEncounter, value); }
        }

        /// <summary>
        ///     Current item for capture page
        /// </summary>
        public Item SelectedCaptureItem
        {
            get { return _selectedCaptureItem; }
            set { Set(ref _selectedCaptureItem, value); }
        }

        /// <summary>
        ///     Score for the current capture, updated only if we captured the Pokemon
        /// </summary>
        public CaptureScore CurrentCaptureScore
        {
            get { return _currentCaptureScore; }
            set { Set(ref _currentCaptureScore, value); }
        }

        /// <summary>
        ///     Pokestop that the user is visiting
        /// </summary>
        public FortData CurrentPokestop
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

        public double CompassHeading
        {
            get { return _compassHeading; }
            set { Set(ref _compassHeading, value); }
        }

        #endregion

        #region Game Logic

        #region PTC Login

        private string _ptcUsername;

        public string PtcUsername
        {
            get { return _ptcUsername; }
            set
            {
                Set(ref _ptcUsername, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _ptcPassword;

        public string PtcPassword
        {
            get { return _ptcPassword; }
            set
            {
                Set(ref _ptcPassword, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Sets things up before being able to play
        /// </summary>
        public async Task InitGame(bool hadAuthTokenStored = false)
        {
            try
            {
                if (hadAuthTokenStored)
                    await _client.SetServer(SettingsService.Instance.PtcAuthToken);
                Busy.SetBusy(true, "Getting GPS position");
                await InitGps();
                Busy.SetBusy(true, "Getting player data");
                UpdatePlayerData();
                Busy.SetBusy(true, "Updating map");
                UpdateMapData(false);
                Busy.SetBusy(true, "Getting player items");
                UpdateInventory();
                // Prevent from going back to login page
                NavigationService.ClearHistory();
                //Start a timer to update map data every 10 seconds
                if (_updateDataTimer == null)
                {
                    _updateDataTimer = ThreadPoolTimer.CreatePeriodicTimer(t =>
                    {
                        if (_stopUpdatingMap) return;
                        if (!_updateDataMutex.WaitOne(0)) return;
                        if (_skipNextUpdate)
                        {
                            _skipNextUpdate = false;
                        }
                        else
                        {
                            Logger.Write("Updating map");
                            UpdateMapData(false);
                        }
                            
                        _updateDataMutex.ReleaseMutex();
                    }, TimeSpan.FromSeconds(15));
                }
            }
            catch (Exception)
            {
                HandleException();
            }
            finally
            {
                Busy.SetBusy(false);
            }
        }


        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Logging in...");
                try
                {
                    var authToken = await _client.DoPtcLogin(PtcUsername, PtcPassword);
                    // Update current token even if it's null
                    SettingsService.Instance.PtcAuthToken = authToken;
                    if (string.IsNullOrEmpty(authToken))
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog("Wrong username/password or offline server, please try again.").ShowAsyncQueue();
                    }
                    else
                    {
                        // Login worked, init game
                        await InitGame();
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage));
                    }
                }
                catch (Exception)
                {
                    await new MessageDialog("PTC login is probably down, please retry later.").ShowAsyncQueue();
                }
                finally
                {
                    Busy.SetBusy(false);
                }
            }, () => !string.IsNullOrEmpty(PtcUsername) && !string.IsNullOrEmpty(PtcPassword))
            );

        private DelegateCommand _doPtcLogoutCommand;

        public DelegateCommand DoPtcLogoutCommand => _doPtcLogoutCommand ?? (
            _doPtcLogoutCommand = new DelegateCommand(() =>
            {
                // Clear stored token
                SettingsService.Instance.PtcAuthToken = null;
                // Stop the timer
                _updateDataTimer?.Cancel();
                _updateDataTimer = null;
                _updateDataMutex = null;
                // Navigate to login page
                NavigationService.Navigate(typeof(MainPage));
            }, () => true)
            );

        #endregion

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Since we handled everything in the LaunchBall method, we use this command to get back to main game page
        /// </summary>
        public DelegateCommand ReturnToGameScreen => _returnToGameScreen ?? (
            _returnToGameScreen = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
                // Clear history to avoid issues when using back button    
                NavigationService.ClearHistory();
                // Start updating map again
                _stopUpdatingMap = false;
            }, () => true)
            );

        #endregion

        #region Data Update

        /// <summary>
        ///     True if we're already handling the exception, to prevent multiple dialogs and avoid throwing a
        ///     System.UnauthorizedAccessException
        /// </summary>
        private bool _isHandlingException;

        /// <summary>
        ///     This exception means that something went wrong with the server, so we close the app and remove the saved token
        /// </summary>
        private async void HandleException()
        {
            if (_isHandlingException) return;
            _isHandlingException = true;
            await
                Dispatcher.DispatchAsync(
                    async () =>
                    {
                        var dialog =
                            new MessageDialog(
                                "Something went wrong and app may be unstable now. Do you want to logout and try again?");
                        dialog.Commands.Add(new UICommand("Yes") {Id = 0});
                        dialog.Commands.Add(new UICommand("No") {Id = 1});
                        dialog.DefaultCommandIndex = 0;
                        dialog.CancelCommandIndex = 1;
                        var result = await dialog.ShowAsyncQueue();
                        if ((int) result.Id == 0)
                        {
                            //SettingsService.Instance.PtcAuthToken = null;
                            //BootStrapper.Current.Exit();
                            DoPtcLogoutCommand.Execute();
                        }
                        _isHandlingException = false;
                    });
        }

        private void ForcedUpdateMapData(bool updateOnlyPokemonData = true)
        {
            if (!_updateDataMutex.WaitOne(0)) return;
            _skipNextUpdate = true;
            UpdateMapData(updateOnlyPokemonData);
            _updateDataMutex.ReleaseMutex();
        }

        /// <summary>
        ///     Retrieves data for the current position
        /// </summary>
        private async void UpdateMapData(bool updateOnlyPokemonData = true)
        {
            try
            {
                // Report it to client and find things nearby
                await
                    _client.UpdatePlayerLocation(CurrentGeoposition.Coordinate.Point.Position.Latitude,
                        CurrentGeoposition.Coordinate.Point.Position.Longitude);
                var mapObjects = await _client.GetMapObjects();
                // Replace data with the new ones                                  
                var catchableTmp = new List<MapPokemon>(mapObjects.MapCells.SelectMany(i => i.CatchablePokemons));
                Logger.Write($"Found {catchableTmp.Count} catchable pokemons");
                if (_vibrationDevice != null && CanVibrate && catchableTmp.Count != CatchablePokemons.Count)
                    _vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(500));
                await Dispatcher.DispatchAsync(() =>
                {
                    CatchablePokemons.Clear();
                    foreach (var pokemon in catchableTmp)
                    {
                        CatchablePokemons.Add(new MapPokemonWrapper(pokemon));
                    }
                });
                var nearbyTmp = new List<NearbyPokemon>(mapObjects.MapCells.SelectMany(i => i.NearbyPokemons));
                Logger.Write($"Found {nearbyTmp.Count} nearby pokemons");
                await Dispatcher.DispatchAsync(() =>
                {
                    NearbyPokemons.Clear();
                    foreach (var pokemon in nearbyTmp)
                    {
                        NearbyPokemons.Add(pokemon);
                    }
                });
                // We only need to update Pokemons
                if (updateOnlyPokemonData) return;
                // Retrieves PokeStops but not Gyms
                var pokeStopsTmp =
                    new List<FortData>(mapObjects.MapCells.SelectMany(i => i.Forts)
                        .Where(i => i.Type == FortType.Checkpoint));
                Logger.Write($"Found {pokeStopsTmp.Count} nearby PokeStops");
                await Dispatcher.DispatchAsync(() =>
                {
                    NearbyPokestops.Clear();
                    foreach (var pokestop in pokeStopsTmp)
                    {                        
                        NearbyPokestops.Add(pokestop);
                    }
                });
            }
            catch (Exception)
            {
                HandleException();
            }
        }

        /// <summary>
        ///     Updates player data like profile and stats
        /// </summary>
        private async void UpdatePlayerData()
        {
            try
            {
                PlayerProfile = (await _client.GetProfile()).Profile;
                InventoryDelta = (await _client.GetInventory()).InventoryDelta;
                var tmpStats = InventoryDelta.InventoryItems.First(
                    item => item.InventoryItemData.PlayerStats != null).InventoryItemData.PlayerStats;
                if (PlayerStats != null && PlayerStats.Level != tmpStats.Level)
                {
                    // TODO: report level increase
                }
                PlayerStats = tmpStats;
            }
            catch (Exception)
            {
                HandleException();
            }
        }

        /// <summary>
        ///     Retrieves inventory for the player
        /// </summary>
        private async void UpdateInventory()
        {
            try
            {
                var inventoryTmp = new List<Item>(await _inventory.GetItems());
                await Dispatcher.DispatchAsync(() =>
                {
                    Inventory.Clear();
                    foreach (var item in inventoryTmp)
                    {
                        Inventory.Add(item);
                    }
                });
            }
            catch (Exception)
            {
                HandleException();
            }
        }

        #endregion

        #region Pokemon Catching

        #region Catching Events

        /// <summary>
        ///     Event fired if the user was able to catch the Pokemon
        /// </summary>
        public event EventHandler CatchSuccess;

        /// <summary>
        ///     Event fired if the user missed the Pokemon
        /// </summary>
        public event EventHandler CatchMissed;

        /// <summary>
        ///     Event fired if the Pokemon escapes
        /// </summary>
        public event EventHandler CatchEscape;

        #endregion

        private DelegateCommand<MapPokemonWrapper> _tryCatchPokemon;

        /// <summary>
        ///     We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        ///     The only logic here is to check if the encounter was successful before navigating, everything else is handled by
        ///     the actual capture method.
        /// </summary>
        public DelegateCommand<MapPokemonWrapper> TryCatchPokemon => _tryCatchPokemon ?? (
            _tryCatchPokemon = new DelegateCommand<MapPokemonWrapper>(async pokemon =>
            {
                Logger.Write($"Catching {pokemon.PokemonId}");
                Busy.SetBusy(true, $"Loading encounter with {pokemon.PokemonId}");
                // Get the pokemon and navigate to capture page where we can handle capturing
                CurrentPokemon = pokemon;
                CurrentEncounter = await _client.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnpointId);
                // Reset selected item to default one and score to null
                SelectedCaptureItem = Inventory.First(item => item.Item_ == ItemType.Pokeball);
                CurrentCaptureScore = null;
                Busy.SetBusy(false);
                if (CurrentEncounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    // report that we started catching a Pokemon
                    _stopUpdatingMap = true;
                    NavigationService.Navigate(typeof(CapturePokemonPage));
                }
                else
                {
                    // Encounter failed, probably the Pokemon ran away
                    await new MessageDialog("Pokemon ran away, sorry :(").ShowAsyncQueue();
                    ForcedUpdateMapData();
                }
            }, pokemon => true)
            );

        private DelegateCommand _useSelectedCaptureItem;

        /// <summary>
        ///     We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        ///     The only logic here is to check if the encounter was successful before navigating, everything else is handled by
        ///     the actual capture method.
        /// </summary>
        public DelegateCommand UseSelectedCaptureItem => _useSelectedCaptureItem ?? (
            _useSelectedCaptureItem = new DelegateCommand(async () =>
            {
                Logger.Write($"Launched {SelectedCaptureItem} at {CurrentPokemon.PokemonId}");
                // TODO: we need to see what happens if the user is throwing a different kind of ball
                if (SelectedCaptureItem.Item_ == ItemType.Pokeball)
                {
                    // Player's using a PokeBall so we try to catch the Pokemon
                    Busy.SetBusy(true, "Throwing Pokeball");
                    await ThrowPokeball();
                }
                else
                {
                    // TODO: check if player can only use a ball or a berry during an encounter, and maybe avoid displaying useless items in encounter's inventory
                    // He's using a berry
                    Busy.SetBusy(true, "Throwing Berry");
                    await ThrowBerry();
                }
                Busy.SetBusy(false);
            }, () => true));

        /// <summary>
        ///     Launches the PokeBall for the current encounter, handling the different catch responses
        /// </summary>
        /// <returns></returns>
        private async Task ThrowPokeball()
        {
            var caughtPokemonResponse =
                await
                    _client.CatchPokemon(CurrentPokemon.EncounterId, CurrentPokemon.SpawnpointId,
                        CurrentPokemon.Latitude, CurrentPokemon.Longitude, (MiscEnums.Item) SelectedCaptureItem.Item_);
            switch (caughtPokemonResponse.Status)
            {
                case CatchPokemonResponse.Types.CatchStatus.CatchError:
                    Logger.Write("CatchError!");
                    // TODO: what can we do?
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchSuccess:
                    CurrentCaptureScore = caughtPokemonResponse.Scores;
                    Logger.Write($"We caught {CurrentPokemon.PokemonId}");
                    CatchSuccess?.Invoke(this, null);
                    ForcedUpdateMapData();
                    UpdateInventory();
                    UpdatePlayerData();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchEscape:
                    CurrentCaptureScore = caughtPokemonResponse.Scores;
                    Logger.Write($"{CurrentPokemon.PokemonId} escaped");
                    CatchEscape?.Invoke(this, null);
                    await new MessageDialog($"{CurrentPokemon.PokemonId} escaped").ShowAsyncQueue();
                    ForcedUpdateMapData();
                    UpdateInventory();
                    UpdatePlayerData();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchFlee:
                    Logger.Write($"{CurrentPokemon.PokemonId} fleed");
                    CatchEscape?.Invoke(this, null);
                    await new MessageDialog($"{CurrentPokemon.PokemonId} fleed").ShowAsyncQueue();
                    ForcedUpdateMapData();
                    UpdateInventory();
                    ReturnToGameScreen.Execute();
                    break;
                case CatchPokemonResponse.Types.CatchStatus.CatchMissed:
                    Logger.Write($"We missed {CurrentPokemon.PokemonId}");
                    CatchMissed?.Invoke(this, null);
                    UpdateInventory();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Uses the selected berry for the current encounter
        ///     TODO: what happens when the berry is used? Do we need some kind of animation or visual feedback?
        /// </summary>
        /// <returns></returns>
        public async Task ThrowBerry()
        {
            if (SelectedCaptureItem == null)
                return;
            var berryResult =
                await
                    _client.UseCaptureItem(CurrentPokemon.EncounterId, (ItemId) SelectedCaptureItem.Item_,
                        CurrentPokemon.SpawnpointId);
            Logger.Write($"Used {SelectedCaptureItem}. Remaining: {SelectedCaptureItem.Count}");
        }

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

        private DelegateCommand<FortData> _trySearchPokestop;

        /// <summary>
        ///     We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        ///     The only logic here is to check if the encounter was successful before navigating, everything else is handled by
        ///     the actual capture method.
        /// </summary>
        public DelegateCommand<FortData> TrySearchPokestop => _trySearchPokestop ?? (
            _trySearchPokestop = new DelegateCommand<FortData>(async pokestop =>
            {
                Logger.Write($"Searching {pokestop.Id}");
                Busy.SetBusy(true, "Loading Pokestop");
                // Get the pokestop and navigate to search page where we can handle searching
                CurrentPokestop = pokestop;
                CurrentPokestopInfo = await _client.GetFort(pokestop.Id, pokestop.Latitude, pokestop.Longitude);
                Busy.SetBusy(false);
                // If timeout is expired we can go to to pokestop page          
                if (CurrentPokestop.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                {
                    // report that we entered a pokestop
                    _stopUpdatingMap = true;
                    CurrentSearchResponse = null;
                    NavigationService.Navigate(typeof(SearchPokestopPage));
                }
                else
                {
                    // Timeout is not expired yet, player can't get items from the fort
                    await new MessageDialog("This PokeStop is still on cooldown, please retry later.").ShowAsyncQueue();
                }
            }, pokemon => true)
            );

        private DelegateCommand _searchCurrentPokestop;

        /// <summary>
        ///     Searches the current PokeStop, trying to get items from it
        /// </summary>
        public DelegateCommand SearchCurrentPokestop => _searchCurrentPokestop ?? (
            _searchCurrentPokestop = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, "Searching PokeStop");
                Logger.Write($"Searching {CurrentPokestopInfo.Name} [ID = {CurrentPokestop.Id}]");
                CurrentSearchResponse =
                    await _client.SearchFort(CurrentPokestop.Id, CurrentPokestop.Latitude, CurrentPokestop.Longitude);
                Busy.SetBusy(false);
                switch (CurrentSearchResponse.Result)
                {
                    case FortSearchResponse.Types.Result.NoResultSet:
                        break;
                    case FortSearchResponse.Types.Result.Success:
                        // Success, we play the animation and update inventory
                        SearchSuccess?.Invoke(this, null);
                        ForcedUpdateMapData(false);
                        UpdateInventory();
                        break;
                    case FortSearchResponse.Types.Result.OutOfRange:
                        // PokeStop can't be used because it's out of range, there's nothing that we can do
                        SearchOutOfRange?.Invoke(this, null);
                        break;
                    case FortSearchResponse.Types.Result.InCooldownPeriod:
                        // PokeStop can't be used because it's on cooldown, there's nothing that we can do
                        SearchInCooldown?.Invoke(this, null);
                        break;
                    case FortSearchResponse.Types.Result.InventoryFull:
                        // Items can't be gathered because player's inventory is full, there's nothing that we can do
                        SearchInventoryFull?.Invoke(this, null);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }, () => true));

        #endregion

        #endregion

        #region GPS & Maps

        private Compass _compass;

        private Geolocator _geolocator;

        private Geoposition _currentGeoposition;

        /// <summary>
        ///     Key for Bing's Map Service (not included in GIT, you need to get your own token to use maps!)
        /// </summary>
        public string MapServiceToken => ApplicationKeys.MapServiceToken;

        /// <summary>
        ///     Current GPS position
        /// </summary>
        public Geoposition CurrentGeoposition
        {
            get { return _currentGeoposition; }
            set { Set(ref _currentGeoposition, value); }
        }

        private double _compassHeading;

        private async Task InitGps()
        {
            // Set your current location.
            var accessStatus = await Geolocator.RequestAccessAsync();
            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // Get the current location.
                    Logger.Write("GPS activated");
                    _geolocator = new Geolocator
                    {
                        DesiredAccuracy = PositionAccuracy.High,
                        DesiredAccuracyInMeters = 5,
                        ReportInterval = 5000,
                        MovementThreshold = 5
                    };
                    CurrentGeoposition = await _geolocator.GetGeopositionAsync();
                    _compass = Compass.GetDefault();
                    if (_compass != null)
                    {
                        Logger.Write("Compass activated");
                        _compass.ReportInterval = 100;
                        _compass.ReadingChanged += CompassOnReadingChanged;
                    }
                    break;
                default:
                    Logger.Write("Error during GPS activation");
                    await new MessageDialog("GPS error, sorry :(").ShowAsyncQueue();
                    BootStrapper.Current.Exit();
                    break;
            }
            _geolocator.PositionChanged += GeolocatorOnPositionChanged;
        }

        private void CompassOnReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
        {
            CompassHeading = args.Reading.HeadingMagneticNorth;
        }

        private async void GeolocatorOnPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            // Get new position
            await Dispatcher.DispatchAsync(() => { CurrentGeoposition = args.Position; });
        }

        #endregion
    }
}
