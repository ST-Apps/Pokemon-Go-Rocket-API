using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Data;
using Microsoft.HockeyApp;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.ViewModels;
using PokemonGo_UWP.Views;
using Template10.Common;
using System;
using Windows.System;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Logging;
using Universal_Authenticator_v2.Views;
using Splash = PokemonGo_UWP.Views.Splash;

namespace PokemonGo_UWP
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki
    [Bindable]
    sealed partial class App : BootStrapper
    {


        /// <summary>
        /// Used to prevent lockscreen while playing
        /// </summary>
        public static DisplayRequest DisplayRequest;

        public App()
        {
            InitializeComponent();
            SplashFactory = e => new Splash(e);

#if DEBUG
            // Init logger
            Logger.SetLogger(new ConsoleLogger(LogLevel.Info));
#endif            
            
            // Init HockeySDK
            if (!string.IsNullOrEmpty(ApplicationKeys.HockeyAppToken))
                HockeyClient.Current.Configure(ApplicationKeys.HockeyAppToken);

            // Forces the display to stay on while we play
            DisplayRequest = new DisplayRequest();
            DisplayRequest.RequestActive();                            
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }

            // Enter into full screen mode
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: this is really ugly!
            var hasPreviousSession = !string.IsNullOrEmpty(SettingsService.Instance.PtcAuthToken) ||
                                     !string.IsNullOrEmpty(SettingsService.Instance.GoogleAuthToken);
            if (hasPreviousSession)
            {
                try
                {
                    await GameClient.InitializeClient(!string.IsNullOrEmpty(SettingsService.Instance.PtcAuthToken));
                    // We have a stored token, let's go to game page 
                    NavigationService.Navigate(typeof(GameMapPage), true);
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
                    var dialog = new MessageDialog( string.Format(Utils.Resources.Translation.GetString("UpdatedVersion"), latestUpdateInfo.version, latestUpdateInfo.description));

                    dialog.Commands.Add(new UICommand(Utils.Resources.Translation.GetString("Yes")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(Utils.Resources.Translation.GetString("No")) { Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    var result = await dialog.ShowAsyncQueue();

                    if ((int)result.Id != 0)
                        return;

                    //continue with execution because we need Busy page working (cannot work on splash screen)
                    //result is irrelevant
                    Task t1 = UpdateManager.InstallUpdate(latestUpdateInfo.release);
                }
            }
            await Task.CompletedTask;
        }
    }
}