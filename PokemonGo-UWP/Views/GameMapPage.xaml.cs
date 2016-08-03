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

            Loaded += (s, e) =>
            {
                // TODO: find a proper way to center all the panels based on resolution
                NearbyGridTranslateTransform.Y = ActualHeight*3/2;
            };
            //WindowWrapper.Current().Window.VisibilityChanged += (s, e) =>
            //{
            //    if (App.ViewModelLocator.GameManagerViewModel != null)
            //    {
            //        // We need to disable vibration
            //        App.ViewModelLocator.GameManagerViewModel.CanVibrate = e.Visible;
            //    }
            //};
            //SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            //{
            //    // TODO: clearing navigation history before reaching this page doesn't seem enough because back button brings us back to login page, so we need to brutally close the app
            //    BootStrapper.Current.Exit();
            //};
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (GameClient.Geoposition != null)
                UpdateMap(GameClient.Geoposition);
            SubscribeToCaptureEvents();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
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
        }        

        private void UnsubscribeToCaptureEvents()
        {
            GameClient.GeopositionUpdated -= GeopositionUpdated;
        }

        private void GeopositionUpdated(object sender, Geoposition e)
        {
            UpdateMap(e);
        }

        #endregion

    }
}