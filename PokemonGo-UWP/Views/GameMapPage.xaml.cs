using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using PokemonGo_UWP.Utils;
using Template10.Common;
using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GameMapPage : Page
	{
		private readonly object lockObject = new object();
		private Geopoint lastAutoPosition = null;

		public GameMapPage()
		{
			InitializeComponent();
			NavigationCacheMode = NavigationCacheMode.Enabled;

			// Setup nearby translation + map
			Loaded += (s, e) =>
			{
				if (ApplicationKeys.MapBoxTokens.Length > 0)
				{
					var randomTileSourceIndex = new Random().Next(0, ApplicationKeys.MapBoxTokens.Length);
					Logger.Write($"Using MapBox's keyset {randomTileSourceIndex}");
					var mapBoxTileSource =
										new HttpMapTileDataSource(
												"https://api.mapbox.com/styles/v1/" +
												(RequestedTheme == ElementTheme.Light
														? ApplicationKeys.MapBoxStylesLight[randomTileSourceIndex]
														: ApplicationKeys.MapBoxStylesDark[randomTileSourceIndex]) +
												"/tiles/256/{zoomlevel}/{x}/{y}?access_token=" +
												ApplicationKeys.MapBoxTokens[randomTileSourceIndex])
										{
											AllowCaching = true
										};

					GameMapControl.Style = MapStyle.None;
					GameMapControl.TileSources.Clear();
					GameMapControl.TileSources.Add(new MapTileSource(mapBoxTileSource)
					{
						AllowOverstretch = true,
						IsFadingEnabled = false,
						Layer = MapTileLayer.BackgroundReplacement
					});
				}
				ShowNearbyModalAnimation.From =
									HideNearbyModalAnimation.To = NearbyPokemonModal.ActualHeight;
				HideNearbyModalAnimation.Completed += (ss, ee) =>
							{
								NearbyPokemonModal.IsModal = false;
							};

				ReactivateMapAutoUpdate.Visibility = Visibility.Collapsed;

			};
		}

		#region Overrides of Page

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			// Set first position if we shomehow missed it
			if (GameClient.Geoposition != null)
				UpdateMap(GameClient.Geoposition);
			SubscribeToCaptureEvents();
			SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
		}

		private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
		{
			if (!(PokeMenuPanel.Opacity > 0)) return;
			backRequestedEventArgs.Handled = true;
			HidePokeMenuStoryboard.Begin();
		}

		protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			base.OnNavigatingFrom(e);
			UnsubscribeToCaptureEvents();
			SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
            if (SettingsService.Instance.IsRememberMapZoomEnabled)
            {
                SaveZoomLevel();
            }
		}

        private void SaveZoomLevel()
        {
            // Bug fix for Issue 586
            if (SettingsService.Instance.Zoomlevel == 0 || GameMapControl.ZoomLevel == 0)
            {
                try
                {
                    GameMapControl.ZoomLevel = 18;
                }
                catch
                {

                }
            }
            // End Bug fix for Issue 586
            SettingsService.Instance.Zoomlevel = GameMapControl.ZoomLevel;
        }

        #endregion

        #region Handlers

        private async void UpdateMap(Geoposition position)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				lock (lockObject)
				{
					// Set player icon's position
					MapControl.SetLocation(PlayerImage, position.Coordinate.Point);

					// Update angle and center only if map is not being manipulated 
					if (lastAutoPosition == null)
						lastAutoPosition = GameMapControl.Center;

					//Small Trick: I'm not testing lastAutoPosition == GameMapControl.Center because MapControl is not taking exact location when setting center!!
					string currentCoord = $"{GameMapControl.Center.Position.Latitude: 000.0000} ; {GameMapControl.Center.Position.Longitude: 000.0000}";
					string previousCoord = $"{lastAutoPosition.Position.Latitude: 000.0000} ; {lastAutoPosition.Position.Longitude: 000.0000}";
					if (currentCoord == previousCoord)
					{
						//Previous position was set automatically, continue!
						ReactivateMapAutoUpdate.Visibility = Visibility.Collapsed;
						GameMapControl.Center = position.Coordinate.Point;
						lastAutoPosition = GameMapControl.Center;
					    if (!SettingsService.Instance.IsAutoRotateMapEnabled || position.Coordinate.Heading == null ||
					        double.IsNaN(position.Coordinate.Heading.Value)) return;
					    GameMapControl.Heading = position.Coordinate.Heading.Value;
					    if (!SettingsService.Instance.IsRememberMapZoomEnabled) return;
					    try
					    {
					        GameMapControl.ZoomLevel = SettingsService.Instance.Zoomlevel;
					    }
					    catch
					    {

					    }
					}
					else
					{
						//Position was changed by user, activate button to go back to automatic mode
						ReactivateMapAutoUpdate.Visibility = Visibility.Visible;
					}
				}
			});
		}

		private void SubscribeToCaptureEvents()
		{
			GameClient.GeopositionUpdated += GeopositionUpdated;
			ViewModel.LevelUpRewardsAwarded += ViewModelOnLevelUpRewardsAwarded;
		}

		private void UnsubscribeToCaptureEvents()
		{
			GameClient.GeopositionUpdated -= GeopositionUpdated;
			ViewModel.LevelUpRewardsAwarded -= ViewModelOnLevelUpRewardsAwarded;
		}

		private void GeopositionUpdated(object sender, Geoposition e)
		{
			UpdateMap(e);
		}

		private void ViewModelOnLevelUpRewardsAwarded(object sender, EventArgs eventArgs)
		{
			if (PokeMenuPanel.Opacity > 0)
				HidePokeMenuStoryboard.Begin();
			ShowLevelUpPanelStoryboard.Begin();
		}

		#endregion

		private void ToggleNearbyPokemonModal(object sender, TappedRoutedEventArgs e)
		{
			if (NearbyPokemonModal.IsModal)
			{
				HideNearbyModalStoryboard.Begin();
			}
			else
			{
				NearbyPokemonModal.IsModal = true;
				ShowNearbyModalStoryboard.Begin();
			}
		}

		private async void ReactivateMapAutoUpdate_Tapped(object sender, TappedRoutedEventArgs e)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				lock (lockObject)
				{
					lastAutoPosition = null;
					UpdateMap(GameClient.Geoposition);
				}
			});
		}
	}
}