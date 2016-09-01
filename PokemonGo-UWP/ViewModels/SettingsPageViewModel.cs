using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Template10.Mvvm;
using Windows.ApplicationModel.Resources.Core;
using Windows.Storage;

namespace PokemonGo_UWP.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;
	
		/// <summary>
		///     How the player wants the map to rotate following is heading (None, GPS, Compass)
		/// </summary>
		public int MapAutomaticOrientationMode_Index
		{
			get { return (int)System.Enum.ToObject(typeof(MapAutomaticOrientationModes), SettingsService.Instance.MapAutomaticOrientationMode); }
			set
			{
				if (value >= 0 && value <= 2)
				{
					SettingsService.Instance.MapAutomaticOrientationMode = (MapAutomaticOrientationModes)System.Enum.ToObject(typeof(MapAutomaticOrientationModes), value);
				}
			}
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
        ///     Whether the player wants vibration (when a Pokémon is nearby)
        /// </summary>
        public bool IsBatterySaverEnabled
        {
            get { return SettingsService.Instance.IsBatterySaverEnabled; }
            set { SettingsService.Instance.IsBatterySaverEnabled = value; }
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
        public Language UserLanguage
        {
            get {
                if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride == "")
                {
                    return new Language() {
                        Code = "System"
                    };
                } else {
                    return new Language() {
                        Code = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride
                    };
                }
            }
            set {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = value.Code.Replace("System", "");
            }
        }

        public List<Language> languageList
        {
            get {
                List<Language> list = new List<Language>();
                list.Add(new Language() {
                    Code = "System"
                });

                IReadOnlyList<string> languages = Windows.Globalization.ApplicationLanguages.ManifestLanguages;
                foreach(string language in languages) {
                    list.Add(new Language() {
                        Code = language
                    });
                }

                return list;
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
