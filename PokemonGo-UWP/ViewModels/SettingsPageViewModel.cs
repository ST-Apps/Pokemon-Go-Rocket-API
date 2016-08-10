using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
	public class SettingsPageViewModel : ViewModelBase
	{
		#region Bindable Game Vars

		public string CurrentVersion => GameClient.CurrentVersion;

		/// <summary>
		/// Whether the player wants music
		/// </summary>
		public bool IsMusicEnabled => SettingsService.Instance.IsMusicEnabled;

		/// <summary>
		/// Whether the player wants vibration (when a Pokémon is nearby)
		/// </summary>
		public bool IsVibrationEnabled => SettingsService.Instance.IsVibrationEnabled;

		/// <summary>
		/// Whether the player wants the map to rotate following is heading
		/// </summary>
		public bool IsAutoRotateMapEnabled => SettingsService.Instance.IsAutoRotateMapEnabled;

		public bool IsMapZoomEnabled => SettingsService.Instance.IsMapZoomEnabled;

		#endregion

		#region Game Logic

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

		private DelegateCommand _closeCommand;

		public DelegateCommand CloseCommand => _closeCommand ?? (
			_closeCommand = new DelegateCommand(() =>
			{
				// Navigate back
				NavigationService.Navigate(typeof(GameMapPage));
			}, () => true)
			);


		#endregion

		#endregion

	}
}
