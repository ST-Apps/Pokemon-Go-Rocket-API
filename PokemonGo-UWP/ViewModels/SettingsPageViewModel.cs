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
        
        /// <summary>
        ///     Windows handles the PrimaryLanguageOverride, so we don't need to save the value by ourself.
        /// </summary>
        public string UserLanguage
        {
            get {
                    if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride == "")
                    {
                         return "System";
                } else {
                        return Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
                    }
                }
            set { Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = value.Replace("System", ""); }
        }

        public System.Collections.Generic.List<string> languageList
        {
            get {
                System.Collections.Generic.List<string> tmp = new System.Collections.Generic.List<string>();
                tmp.Clear();
                tmp.Add("System");
                tmp.Add("cs");
                tmp.Add("de");
                tmp.Add("el");
                tmp.Add("en-US");
                tmp.Add("es");
                tmp.Add("fi");
                tmp.Add("fr");
                tmp.Add("hu");
                tmp.Add("id");
                tmp.Add("it");
                tmp.Add("ja");
                tmp.Add("nl");
                tmp.Add("pl");
                tmp.Add("pt-BR");
                tmp.Add("pt-PT");
                tmp.Add("ru");
                tmp.Add("sk");
                tmp.Add("tr");
                tmp.Add("zh-CN");
                tmp.Add("zh-HK");
                tmp.Add("zh-TW");
                return tmp;
            }
            set { }
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
