using Microsoft.HockeyApp;
using NotificationsExtensions.Tiles;
using POGOProtos.Data;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Logging;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.Phone.Devices.Notification;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PokemonGo_UWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    sealed partial class App : BootStrapper
    {

        #region Private Members

        /// <summary>
        ///     We use it to notify that we found at least one catchable Pokemon in our area.
        /// </summary>
        private VibrationDevice _vibrationDevice;

        /// <summary>
        ///     Stores the current <see cref="DisplayRequest"/> instance for the app.
        /// </summary>
        private readonly DisplayRequest _displayRequest;

        #endregion

        #region Properties

        /// <summary>
        /// The TileUpdater instance for the app.
        /// </summary>
        public static TileUpdater LiveTileUpdater { get; private set; }

        #endregion

        #region Constructor

        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Splash(e);

            // ensure unobserved task exceptions (unawaited async methods returning Task or Task<T>) are handled
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // ensure general app exceptions are handled
            Application.Current.UnhandledException += App_UnhandledException;

            // Init HockeySDK
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.Configure(ApplicationKeys.HockeyAppToken);

            // Set this in the instance constructor to prevent the creation of an unnecessary static constructor.
            _displayRequest = new DisplayRequest();

            // Initialize the Live Tile Updater.
            LiveTileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        }

        #endregion

        #region Event Handlers

        private static async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await ExceptionHandler.HandleException(new Exception(e.Message));
            // We should be logging these exceptions too so they can be tracked down.
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.TrackException(e.Exception);
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Logger.Write(e.Exception.Message);
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.TrackException(e.Exception);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PokemonsInventory_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Off) return;
            // Using a Switch here because we might handle other changed events in other ways.
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    UpdateLiveTile(e.NewItems.Cast<PokemonData>().OrderByDescending(c => c.Cp).ToList());
                    break;

            }
        }

        /// <summary>
        ///     Vibrates and/or plays a sound when new pokemons are in the area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CatchablePokemons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            if (SettingsService.Instance.IsVibrationEnabled)
                _vibrationDevice?.Vibrate(TimeSpan.FromMilliseconds(500));
            if (SettingsService.Instance.IsMusicEnabled)
                await AudioUtils.PlaySound(@"pokemon_found_ding.wav");
        }

        #endregion

        #region Application Lifecycle

        /// <summary>
        ///     Disable vibration on suspending
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="prelaunchActivated"></param>
        /// <returns></returns>
        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            GameClient.PokemonsInventory.CollectionChanged -= PokemonsInventory_CollectionChanged;
            GameClient.CatchablePokemons.CollectionChanged -= CatchablePokemons_CollectionChanged;
            // TODO: Probably not needed as stated here: https://blogs.windows.com/buildingapps/2016/05/24/how-to-prevent-screen-locks-in-your-uwp-apps/
            _displayRequest.RequestRelease();
            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Peek)
            {
                LiveTileUpdater.EnableNotificationQueue(false);
            }
            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }

        /// <summary>
        ///     This runs everytime the app is launched, even after suspension, so we use this to initialize stuff
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
#if DEBUG
            // Init logger
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
#endif

            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            // Enter into full screen mode
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            // Forces the display to stay on while we play
            //_displayRequest.RequestActive();
            WindowWrapper.Current().Window.VisibilityChanged += WindowOnVisibilityChanged;

            // Init vibration device
            if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                _vibrationDevice = VibrationDevice.GetDefault();
            }

            if (SettingsService.Instance.LiveTileMode == LiveTileModes.Peek)
            {
                LiveTileUpdater.EnableNotificationQueue(true);
            }

            // Respond to changes in inventory and Pokemon in the immediate viscinity.
                GameClient.PokemonsInventory.CollectionChanged += PokemonsInventory_CollectionChanged;
            GameClient.CatchablePokemons.CollectionChanged += CatchablePokemons_CollectionChanged;

            await Task.CompletedTask;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="startKind"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            AsyncSynchronizationContext.Register();            
            var currentAccessToken = GameClient.LoadAccessToken();
            if (currentAccessToken == null)
            {
                await NavigationService.NavigateAsync(typeof(MainPage));
            }
            else
            {
                await GameClient.InitializeClient();
                NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.AppStart);
            }

            // Check for updates (ignore resume)
            if (startKind == StartKind.Launch)
            {
                var latestUpdateInfo = await UpdateManager.IsUpdateAvailable();
                if (latestUpdateInfo != null)
                {
                    var dialog =
                        new MessageDialog(string.Format(Utils.Resources.CodeResources.GetString("UpdatedVersion"),
                            latestUpdateInfo.Version, latestUpdateInfo.Description));

                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("YesText")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(Utils.Resources.CodeResources.GetString("NoText")) { Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsyncQueue();

                    if ((int)result.Id != 0)
                        return;

                    //continue with execution because we need Busy page working (cannot work on splash screen)
                    //result is irrelevant
                    var t1 = UpdateManager.InstallUpdate(latestUpdateInfo.Release);
                }
            }
            await Task.CompletedTask;
        }

        private void WindowOnVisibilityChanged(object sender, VisibilityChangedEventArgs visibilityChangedEventArgs)
        {
            if (!visibilityChangedEventArgs.Visible)
                _displayRequest.RequestRelease();
            else
                _displayRequest.RequestActive();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pokemonList"></param>
        /// <remarks>
        /// advancedrei: The LiveTileUpdater is on teh App class, so this has to stay here for now. Might refactor later.
        /// </remarks>
        internal static void UpdateLiveTile(IList<PokemonData> pokemonList)
        {
            // Let's run this on a separate thread.
            Task.Run(() => {
                try
                {
                    TileContent tile = null;
                    var images = new List<string>();
                    var mode = SettingsService.Instance.LiveTileMode;

                    // Generate the images list for multi-image modes.
                    if (mode == LiveTileModes.People && mode == LiveTileModes.Photo)
                    {
                        images.AddRange(pokemonList.Select(c => new PokemonDataWrapper(c).ImageFileName));
                    }

                    if (mode != LiveTileModes.Peek)
                    {
                        App.LiveTileUpdater.EnableNotificationQueue(true);
                    }
                    else
                    {
                        App.LiveTileUpdater.EnableNotificationQueue(false);
                        LiveTileUpdater.Clear();
                    }

                    switch (mode)
                    {
                        case LiveTileModes.Off:
                            break;
                        case LiveTileModes.Peek:
                            foreach (PokemonData pokemonData in pokemonList)
                            {
                                if (LiveTileUpdater.GetScheduledTileNotifications().Count >= 300) return;
                                var peekTile = LiveTileHelper.GetPeekTile(new PokemonDataWrapper(pokemonData));
                                var scheduled = new ScheduledTileNotification(peekTile.GetXml(),
                                    DateTimeOffset.Now.AddSeconds((pokemonList.IndexOf(pokemonData) * 30) + 1));
                                LiveTileUpdater.AddToSchedule(scheduled);
                            }
                            break;
                        case LiveTileModes.People:
                            tile = LiveTileHelper.GetPeopleTile(images);
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                        case LiveTileModes.Photo:
                            tile = LiveTileHelper.GetPhotosTile(images);
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                        case LiveTileModes.Transparent:
                            tile = LiveTileHelper.GetImageTile("LiveTiles/Transparent/Square44x44Logo.scale-400.png");
                            LiveTileUpdater.Update(new TileNotification(tile.GetXml()));
                            break;
                    }
                    if (tile != null)
                    {
                        //Logger.Write(tile.GetXml().GetXml());
                    }

                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message);
                    if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                        HockeyClient.Current.TrackException(ex);
                }
            });
        }

        #endregion

    }

}