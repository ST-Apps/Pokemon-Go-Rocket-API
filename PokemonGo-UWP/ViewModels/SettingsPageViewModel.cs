using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System.Linq;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;

        /// <summary>
        ///     Whether the player wants the map to rotate following is heading
        /// </summary>
        public bool IsAutoRotateMapEnabled
        {
            get { return SettingsService.Instance.IsAutoRotateMapEnabled; }
            set { SettingsService.Instance.IsAutoRotateMapEnabled = value; }
        }

        /// <summary>
        ///     Whether the player wants the map to rotate using compass
        /// </summary>
        public bool IsCompassEnabled
        {
            get { return SettingsService.Instance.IsCompassEnabled; }
            set { SettingsService.Instance.IsCompassEnabled = value; }
        }

        /// <summary>
        ///     Whether the player wants music
        /// </summary>
        public bool IsMusicEnabled
        {
            get { return SettingsService.Instance.IsMusicEnabled; }
            set { SettingsService.Instance.IsMusicEnabled = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNianticMapEnabled
        {
            get { return SettingsService.Instance.IsNianticMapEnabled; }
            set
            {
                SettingsService.Instance.IsNianticMapEnabled = value;
                _mapSettingsChangedCounter++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRememberMapZoomEnabled
        {
            get { return SettingsService.Instance.IsRememberMapZoomEnabled; }
            set { SettingsService.Instance.IsRememberMapZoomEnabled = value; }
        }

        /// <summary>
        ///     Whether the player wants vibration (when a Pokémon is nearby)
        /// </summary>
        public bool IsVibrationEnabled
        {
            get { return SettingsService.Instance.IsVibrationEnabled; }
            set { SettingsService.Instance.IsVibrationEnabled = value; }
        }

        /// <summary>
        ///     Whether the player wants a Live Tile or a regular one.
        /// </summary>
        public LiveTileModes LiveTileMode
        {
            get { return SettingsService.Instance.LiveTileMode; }
            set
            {
                SettingsService.Instance.LiveTileMode = value;
                App.UpdateLiveTile(GameClient.PokemonsInventory.OrderByDescending(c => c.Cp).ToList());
            }
        }

        #endregion

        #region Constructor

        public SettingsPageViewModel()
        {
            LiveTileSelectionCommand = new DelegateCommand<string>(param =>
            {
                var mode = (LiveTileModes)int.Parse(param);
                LiveTileMode = mode;
            });
        }

        #endregion

        #region Game Logic

        #region LiveTileSelection

        public DelegateCommand<string> LiveTileSelectionCommand { get; set; }

        #endregion

        #region Logout

        private DelegateCommand _doPtcLogoutCommand;

        public DelegateCommand DoPtcLogoutCommand => _doPtcLogoutCommand ?? (
            _doPtcLogoutCommand = new DelegateCommand(() =>
            {
                // Clear stored token
                GameClient.DoLogout();
                // Navigate to login page
                NavigationService.Navigate(typeof(MainPage));
                // Remove all pages from the history
                NavigationService.ClearHistory();
            }, () => true)
            );

        #endregion

        #region Close

        private int _mapSettingsChangedCounter;

        private DelegateCommand _closeCommand;

        public DelegateCommand CloseCommand => _closeCommand ?? (
            _closeCommand = new DelegateCommand(() =>
            {
                // Navigate back if we didn't change map settings
                if (_mapSettingsChangedCounter%2 == 0)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.SettingsUpdate);
                }
            }));

        #endregion

        #endregion
    }
}