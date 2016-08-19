﻿using System;
using Windows.Devices.Geolocation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Utils;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameMapPage : Page
    {
        private readonly object lockObject = new object();
        private int _mapBoxIndex = -1;
        private Geopoint lastAutoPosition;

        public GameMapPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;

            // Setup nearby translation + map
            Loaded += (s, e) =>
            {
                ShowNearbyModalAnimation.From =
                    HideNearbyModalAnimation.To = NearbyPokemonModal.ActualHeight;
                HideNearbyModalAnimation.Completed += (ss, ee) => { NearbyPokemonModal.IsModal = false; };

                ReactivateMapAutoUpdate.Visibility = Visibility.Collapsed;
            };
        }

        private void SetupMap()
        {
            if (ApplicationKeys.MapBoxTokens.Length > 0 && SettingsService.Instance.IsNianticMapEnabled)
            {
                if (_mapBoxIndex == -1)
                    _mapBoxIndex = new Random().Next(0, ApplicationKeys.MapBoxTokens.Length);
                Logger.Write($"Using MapBox's keyset {_mapBoxIndex}");
                var mapBoxTileSource =
                    new HttpMapTileDataSource(
                        "https://api.mapbox.com/styles/v1/" +
                        (RequestedTheme == ElementTheme.Light
                            ? ApplicationKeys.MapBoxStylesLight[_mapBoxIndex]
                            : ApplicationKeys.MapBoxStylesDark[_mapBoxIndex]) +
                        "/tiles/256/{zoomlevel}/{x}/{y}?access_token=" +
                        ApplicationKeys.MapBoxTokens[_mapBoxIndex])
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
            else
            {
                // Fallback to Bing Maps   
                // TODO: map color scheme is set but the visual style doesn't update!             
                GameMapControl.ColorScheme = ViewModel.CurrentTheme == ElementTheme.Dark
                    ? MapColorScheme.Dark
                    : MapColorScheme.Light;
                GameMapControl.TileSources.Clear();
                GameMapControl.Style = MapStyle.Terrain;
            }
        }

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

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Hide PokeMenu panel just in case
            HidePokeMenuStoryboard.Begin();
            // See if we need to update the map
            if (e.Parameter != null && e.NavigationMode != NavigationMode.Back)
            {
                var mode =
                    ((JObject)JsonConvert.DeserializeObject((string)e.Parameter)).Last
                        .ToObject<GameMapNavigationModes>();
                if (mode == GameMapNavigationModes.AppStart || mode == GameMapNavigationModes.SettingsUpdate)
                    SetupMap();
            }

            GameClient.InitCompass();

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
                    string currentCoord =
                        $"{GameMapControl.Center.Position.Latitude: 000.0000} ; {GameMapControl.Center.Position.Longitude: 000.0000}";
                    string previousCoord =
                        $"{lastAutoPosition.Position.Latitude: 000.0000} ; {lastAutoPosition.Position.Longitude: 000.0000}";
                    if (currentCoord == previousCoord)
                    {
                        //Previous position was set automatically, continue!
                        ReactivateMapAutoUpdate.Visibility = Visibility.Collapsed;
                        GameMapControl.Center = position.Coordinate.Point;
                        lastAutoPosition = GameMapControl.Center;

                        if (SettingsService.Instance.IsAutoRotateMapEnabled == true && position.Coordinate.Heading != null)
                        {
                            GameMapControl.Heading = position.Coordinate.Heading.Value;
                        }

                        if (SettingsService.Instance.IsRememberMapZoomEnabled == true)
                        {
                            GameMapControl.ZoomLevel = SettingsService.Instance.Zoomlevel;
                        }

                    }
                }
            });
        }

        private void SubscribeToCaptureEvents()
        {
            GameClient.GeopositionUpdated += GeopositionUpdated;
            GameClient.HeadingUpdated += HeadingUpdated;
            ViewModel.LevelUpRewardsAwarded += ViewModelOnLevelUpRewardsAwarded;
        }

        private void HeadingUpdated(object sender, Windows.Devices.Sensors.CompassReading e)
        {
            GameMapControl.Heading = e.HeadingTrueNorth ?? e.HeadingMagneticNorth;
        }

        private void UnsubscribeToCaptureEvents()
        {
            GameClient.GeopositionUpdated -= GeopositionUpdated;
            GameClient.HeadingUpdated -= HeadingUpdated;
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

        private void GameMapControl_TargetCameraChanged(MapControl sender, MapTargetCameraChangedEventArgs args)
        {
            if (args.ChangeReason == MapCameraChangeReason.UserInteraction && lastAutoPosition != null)
            {
                ReactivateMapAutoUpdate.Visibility = Visibility.Visible;
            }
        }
    }
}
