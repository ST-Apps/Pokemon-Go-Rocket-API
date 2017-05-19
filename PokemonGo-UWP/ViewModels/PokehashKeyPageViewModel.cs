using Newtonsoft.Json;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using PokemonGoAPI.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels
{
    public class PokehashKeyPageViewModel : ViewModelBase
    {
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
            if (suspensionState.Any())
            {
                // Recovering the state
                PokehashKey = (string)suspensionState[nameof(PokehashKey)];
            }
            else
            {
                PokehashKey = SettingsService.Instance.PokehashAuthKey;
            }
            await Task.CompletedTask;
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
                suspensionState[nameof(PokehashKey)] = PokehashKey;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        private string _pokehashkey;

        #endregion

        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;

        public string PokehashKey
        {
            get { return _pokehashkey; }
            set
            {
                Set(ref _pokehashkey, value);
                DoStoreCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Game Logic

        private DelegateCommand _doStoreCommand;

        public DelegateCommand DoStoreCommand => _doStoreCommand ?? (
            _doStoreCommand = new DelegateCommand(async () =>
            {
                // Store Pokehash auth key
                SettingsService.Instance.PokehashAuthKey = PokehashKey;

                // Check if token is available and still valid
                var tokenString = SettingsService.Instance.AccessTokenString;
                if (tokenString != null && tokenString.Length > 0)
                {
                    var accessToken = JsonConvert.DeserializeObject<AccessToken>(tokenString);

                    if (accessToken != null)
                    {
                        GameClient.SetCredentialsFromSettings();
                        await GameClient.InitializeClient();
                        await NavigationService.NavigateAsync(typeof(GameMapPage), GameMapNavigationModes.AppStart);
                    }
                    else
                    {
                        await NavigationService.NavigateAsync(typeof(MainPage));
                    }
                }
                else
                {
                    await NavigationService.NavigateAsync(typeof(MainPage));
                }

            }, () => !string.IsNullOrEmpty(PokehashKey))
            );

        #endregion
    }
}