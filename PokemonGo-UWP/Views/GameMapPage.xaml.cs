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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameMapPage : Page
    {

        /// <summary>
        /// False when the map is being manipulated, so that we don't override user interaction
        /// </summary>
        private bool _canUpdateMap = true;

        public GameMapPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;            

            // Setup nearby translation
            Loaded += (s, e) =>
            {
                var randomTileSourceIndex = new Random().Next(0, ApplicationKeys.MapBoxTokens.Length);
                Logger.Write($"Using MapBox's keyset {randomTileSourceIndex}");
                var mapBoxTileSource =
                    new HttpMapTileDataSource(
                        "https://api.mapbox.com/styles/v1/" + (RequestedTheme == ElementTheme.Light ? ApplicationKeys.MapBoxStylesLight[randomTileSourceIndex] : ApplicationKeys.MapBoxStylesDark[randomTileSourceIndex]) +
                        "/tiles/256/{zoomlevel}/{x}/{y}?access_token=" + ApplicationKeys.MapBoxTokens[randomTileSourceIndex])
                    {
                        AllowCaching = true
                    };

                GameMapControl.TileSources.Clear();
                GameMapControl.TileSources.Add(new MapTileSource(mapBoxTileSource) {AllowOverstretch = true, IsFadingEnabled = false});                

                ShowNearbyModalAnimation.From =
                    HideNearbyModalAnimation.To = NearbyPokemonModal.ActualHeight;
                HideNearbyModalAnimation.Completed += (ss, ee) =>
                {
                    NearbyPokemonModal.IsModal = false;
                };
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
        }

        #endregion

        #region Handlers

        private async void UpdateMap(Geoposition position)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Set player icon's position
                MapControl.SetLocation(PlayerImage, position.Coordinate.Point);
                // Update angle and center only if map is not being manipulated 
                // TODO: set this to false on gesture
                if (!_canUpdateMap) return;
                GameMapControl.Center = position.Coordinate.Point;
                if (position.Coordinate.Heading != null && !double.IsNaN(position.Coordinate.Heading.Value))
                {
                    GameMapControl.Heading = position.Coordinate.Heading.Value;
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
    }
}