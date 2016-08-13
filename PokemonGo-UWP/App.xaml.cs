using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.Phone.Devices.Notification;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Microsoft.HockeyApp;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Logging;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Common;

namespace PokemonGo_UWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    sealed partial class App : BootStrapper
    {
        /// <summary>
        ///     We use it to notify that we found at least one catchable Pokemon in our area
        /// </summary>
        private VibrationDevice _vibrationDevice;

        private readonly DisplayRequest _displayRequest = new DisplayRequest();

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
        }

        private static async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await ExceptionHandler.HandleException();
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            Logger.Write(e.Exception.Message);
            HockeyClient.Current.TrackException(e.Exception);
        }

        #region Toast & Notifications

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

        #region Overrides of BootStrapper

        /// <summary>
        ///     Disable vibration on suspending
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="prelaunchActivated"></param>
        /// <returns></returns>
        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunchActivated)
        {
            GameClient.CatchablePokemons.CollectionChanged -= CatchablePokemons_CollectionChanged;
            return base.OnSuspendingAsync(s, e, prelaunchActivated);
        }

        #endregion

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
            _displayRequest.RequestActive();

            // Init vibration device
            if (ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice"))
            {
                _vibrationDevice = VibrationDevice.GetDefault();
            }

            // Set to vibrate, based on user's preference, if a new Pokemon is found
            GameClient.CatchablePokemons.CollectionChanged += CatchablePokemons_CollectionChanged;

            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            AsyncSynchronizationContext.Register();
            // TODO: this is really ugly!
            if (!string.IsNullOrEmpty(SettingsService.Instance.AuthToken))
            {
                try
                {
                    await GameClient.InitializeClient();
                    // We have a stored token, let's go to game page
                    NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.AppStart);
                }
                catch (Exception)
                {
                    await ExceptionHandler.HandleException();
                }
            }
            else
            {
                await NavigationService.NavigateAsync(typeof(MainPage));
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
    }
}