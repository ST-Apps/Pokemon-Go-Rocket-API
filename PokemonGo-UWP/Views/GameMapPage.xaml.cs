using System;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Utils;
using Template10.Common;
using PokemonGo_UWP.Utils.Helpers;
using System.ComponentModel;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using PokemonGo_UWP.Entities;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameMapPage : Page
    {
        private Geopoint lastAutoPosition;
        private Button ReactivateMapAutoUpdateButton;

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

                // Add reactivate map update button
                if (ReactivateMapAutoUpdateButton != null) return;
                #region Reactivate Map AutoUpdate Button
                ReactivateMapAutoUpdateButton = new Button
                {
                    Visibility = Visibility.Collapsed,
                    Style = (Style)BootStrapper.Current.Resources["ImageButtonStyle"],
                    Height = 44,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 0),
                    Content = new Image
                    {
                        Source =
                new BitmapImage
                {
                    UriSource =
                        new Uri($"ms-appx:///Assets/Icons/RecenterMapIcon{ViewModel.CurrentTheme}.png")
                },
                        Stretch = Stretch.Uniform,
                        Height = 36,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                ReactivateMapAutoUpdateButton.Tapped += ReactivateMapAutoUpdate_Tapped;

                var tsp = (StackPanel)
                    VisualTreeHelper.GetChild(
                        VisualTreeHelper.GetChild(
                            VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(GameMapControl, 0), 1), 0), 0);

                tsp.Children.Add(ReactivateMapAutoUpdateButton);
                #endregion
                #region Map Style button ;)
                if (GameMapControl.Is3DSupported)
                {
                    var MapStyleButton = new Button
                    {
                        Style = (Style)BootStrapper.Current.Resources["ImageButtonStyle"],
                        Height = 44,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 34),
                        Content = new Image
                        {
                            Source =
                                        new BitmapImage
                                        {
                                            UriSource =
                                                        new Uri($"ms-appx:///Assets/Teams/no-team.png")
                                        },
                            Stretch = Stretch.Uniform,
                            Height = 36,
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        }
                    };
                    MapStyleButton.Tapped += MapStyleButton_Tapped;

                    tsp.Children.Add(MapStyleButton);
                }
                #endregion
                DisplayInformation.GetForCurrentView().OrientationChanged += GameMapPage_OrientationChanged;
            };
        }

        private void GameMapPage_OrientationChanged(DisplayInformation sender, object args)
        {
            if (SettingsService.Instance.IsBatterySaverEnabled)
                if (sender.NativeOrientation == DisplayOrientations.Portrait)
                {
                    HideBatterySaver.Begin();

                    IsHitTestVisible = true;
                }
                else if (sender.NativeOrientation == DisplayOrientations.PortraitFlipped)
                {
                    ShowBatterySaver.Begin();

                    IsHitTestVisible = false;
                }
        }

        private void SetupMap()
        {
            if (SettingsService.Instance.IsNianticMapEnabled)
            {
                var googleTileSource =
                    new HttpMapTileDataSource(
                        "http://mts0.google.com/vt/lyrs=m@289000001&hl=en&src=app&x={x}&y={y}&z={zoomlevel}&s=Gal&apistyle=" + (RequestedTheme == ElementTheme.Light ? MapStyleHelpers.LightMapStyleString : MapStyleHelpers.DarkMapStyleString));

                GameMapControl.Style = MapStyle.None;
                GameMapControl.TileSources.Clear();
                GameMapControl.TileSources.Add(new MapTileSource(googleTileSource)
                {
                    AllowOverstretch = true,
                    IsFadingEnabled = false,
                    Layer = MapTileLayer.BackgroundReplacement
                });

                GoogleAttributionBorder.Visibility = Visibility.Visible;
            }
            else
            {
                // Fallback to Bing Maps
                // TODO: map color scheme is set but the visual style doesn't update!
                GameMapControl.ColorScheme = ViewModel.CurrentTheme == ElementTheme.Dark
                    ? MapColorScheme.Dark
                    : MapColorScheme.Light;
                GameMapControl.TileSources.Clear();
                GameMapControl.Style = MapStyle.Road;

                GoogleAttributionBorder.Visibility = Visibility.Collapsed;
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                lastAutoPosition = null;
                await UpdateMap();
            });
        }

        private void GameMapControl_TargetCameraChanged(MapControl sender, MapTargetCameraChangedEventArgs args)
        {
            if ((args.ChangeReason == MapCameraChangeReason.UserInteraction) && (lastAutoPosition != null))
                ReactivateMapAutoUpdateButton.Visibility = Visibility.Visible;
        }

        private void GameMapControl_OnZoomLevelChanged(MapControl sender, object args)
        {
            var currentZoomLevel = sender.ZoomLevel;
            sender.ZoomLevel = currentZoomLevel < 18 ? 18 : currentZoomLevel;
        }

        #region Overrides of Page

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                // Hide PokeMenu panel just in case
                HidePokeMenuStoryboard.Begin();
                // See if we need to update the map
                if ((e.Parameter != null) && (e.NavigationMode != NavigationMode.Back))
                {
                    GameMapNavigationModes mode =
                        ((JObject)JsonConvert.DeserializeObject((string)e.Parameter)).Last
                            .ToObject<GameMapNavigationModes>();
                    if ((mode == GameMapNavigationModes.AppStart) || (mode == GameMapNavigationModes.SettingsUpdate))
                        SetupMap();
                }
                // Set first position if we shomehow missed it
                await UpdateMap();
                //Changed order of calls, this allow to have events registration before trying to move map
                //appears that for some reason TryRotate and/or TryTilt fails sometimes!
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                SubscribeToCaptureEvents();

            }
            catch (Exception ex)
            {
                //because we are in "async void" unhandled exception might not be raised
                await ExceptionHandler.HandleException(ex);
            }
            try
            {
                await GameMapControl.TryRotateToAsync(SettingsService.Instance.MapHeading);
                await GameMapControl.TryTiltToAsync(SettingsService.Instance.MapPitch);
            }
            catch
            {
                //we don't care :)
            }

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
                SaveZoomLevel();
            SettingsService.Instance.MapPitch = GameMapControl.Pitch;
            SettingsService.Instance.MapHeading = GameMapControl.Heading;
        }

        private void SaveZoomLevel()
        {
            // Bug fix for Issue 586
            if ((SettingsService.Instance.Zoomlevel == 0) || (GameMapControl.ZoomLevel == 0))
                try
                {
                    GameMapControl.ZoomLevel = 18;
                }
                catch
                {
                }
            // End Bug fix for Issue 586
            SettingsService.Instance.Zoomlevel = GameMapControl.ZoomLevel;
        }

        #endregion

        #region Handlers

        private async Task UpdateMap()
        {
            if (LocationServiceHelper.Instance.Geoposition != null)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    // Set player icon's position
                    MapControl.SetLocation(PlayerImage, LocationServiceHelper.Instance.Geoposition.Coordinate.Point);

                    // Update angle and center only if map is not being manipulated
                    if (lastAutoPosition == null)
                    {
                        //Reset of position or first run
                        //Save Center
                        lastAutoPosition = GameMapControl.Center;
                        //Reset orientation to default
                        if (double.IsNaN(GameMapControl.Heading))//DarkAngel: I love non nullable double that can be "!= null and not a number"...
                            await GameMapControl.TryRotateToAsync(0);
                    }

                    //Small Trick: I'm not testing lastAutoPosition == GameMapControl.Center because MapControl is not taking exact location when setting center!!
                    string currentCoord =
                        $"{GameMapControl.Center.Position.Latitude: 000.0000} ; {GameMapControl.Center.Position.Longitude: 000.0000}";
                    string previousCoord =
                        $"{lastAutoPosition.Position.Latitude: 000.0000} ; {lastAutoPosition.Position.Longitude: 000.0000}";
                    if (currentCoord == previousCoord && ReactivateMapAutoUpdateButton != null)
                    {
                        //Previous position was set automatically, continue!
                        ReactivateMapAutoUpdateButton.Visibility = Visibility.Collapsed;
                        GameMapControl.Center = LocationServiceHelper.Instance.Geoposition.Coordinate.Point;

                        lastAutoPosition = GameMapControl.Center;

                        if ((SettingsService.Instance.MapAutomaticOrientationMode == MapAutomaticOrientationModes.GPS) &&
                                                                            (
                                                                            LocationServiceHelper.Instance.Geoposition.Coordinate.Heading.GetValueOrDefault(-1) >= 0
                                                                            && LocationServiceHelper.Instance.Geoposition.Coordinate.Heading.GetValueOrDefault(-1) <= 360
                                                                            ))
                            await GameMapControl.TryRotateToAsync(LocationServiceHelper.Instance.Geoposition.Coordinate.Heading.GetValueOrDefault(GameMapControl.Heading));
                        if (SettingsService.Instance.IsRememberMapZoomEnabled)
                            GameMapControl.ZoomLevel = SettingsService.Instance.Zoomlevel;
                    }
                }
                catch (Exception ex)
                {
                    await ExceptionHandler.HandleException(ex);
                }
            });
            }
        }

        private void SubscribeToCaptureEvents()
        {
            LocationServiceHelper.Instance.PropertyChanged += LocationHelperPropertyChanged;
            GameClient.HeadingUpdated += HeadingUpdated;
            ViewModel.LevelUpRewardsAwarded += ViewModelOnLevelUpRewardsAwarded;
            ViewModel.AppliedItemExpired += ViewModelOnAppliedItemExpired;
            ViewModel.AppliedItemStarted += ViewModelOnAppliedItemStarted;
        }

        private TimeSpan tick = new TimeSpan(DateTime.Now.Ticks);

        private async void HeadingUpdated(object sender, CompassReading e)
        {
            var newTick = new TimeSpan(DateTime.Now.Ticks);
            if (newTick.Subtract(tick).TotalMilliseconds > 10)
            {
                await
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () => { await GameMapControl.TryRotateToAsync(e.HeadingTrueNorth ?? e.HeadingMagneticNorth); });
                tick = newTick;
            }
        }

        private void UnsubscribeToCaptureEvents()
        {
            LocationServiceHelper.Instance.PropertyChanged -= LocationHelperPropertyChanged;
            GameClient.HeadingUpdated -= HeadingUpdated;
            ViewModel.LevelUpRewardsAwarded -= ViewModelOnLevelUpRewardsAwarded;
        }

        private async void LocationHelperPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocationServiceHelper.Instance.Geoposition))
            {
                await UpdateMap();
            }
        }

        private void ViewModelOnLevelUpRewardsAwarded(object sender, EventArgs eventArgs)
        {
            if (PokeMenuPanel.Opacity > 0)
                HidePokeMenuStoryboard.Begin();
            ShowLevelUpPanelStoryboard.Begin();
        }

        #endregion
        private async void MapStyleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (GameMapControl.Is3DSupported)
            {
                switch (GameMapControl.Style)
                {
                    case MapStyle.None:
                        GameMapControl.Style = MapStyle.Road;
                        break;
                    case MapStyle.Road:
                        GameMapControl.Style = MapStyle.Aerial3DWithRoads;
                        break;
                    case MapStyle.Aerial:
                    case MapStyle.AerialWithRoads:
                    case MapStyle.Terrain:
                    case MapStyle.Aerial3D:
                    case MapStyle.Aerial3DWithRoads:
                        GameMapControl.Style = MapStyle.Road;
                        break;
                    default:
                        GameMapControl.Style = MapStyle.Road;
                        break;
                }
            }
            else
            {
                await new Windows.UI.Popups.MessageDialog("Sorry 3DView is not supported!").ShowAsyncQueue();
            }
        }

        private void ViewModelOnAppliedItemExpired(object sender, AppliedItemWrapper AppliedItem)
        {
            HideAppliedItemStoryboard.Begin();
        }

        private void ViewModelOnAppliedItemStarted(object sender, AppliedItemWrapper AppliedItem)
        {
            ShowAppliedItemStoryboard.Begin();
        }
    }
}