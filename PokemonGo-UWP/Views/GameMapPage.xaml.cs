using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Template10.Common;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameMapPage : Page
    {
        public GameMapPage()
        {
            InitializeComponent();
            WindowWrapper.Current().Window.VisibilityChanged += (s, e) =>
            {
                if (App.ViewModelLocator.GameManagerViewModel != null)
                {
                    // We need to disable vibration
                    App.ViewModelLocator.GameManagerViewModel.CanVibrate = e.Visible;
                }
            };
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                // TODO: clearing navigation history before reaching this page doesn't seem enough because back button brings us back to login page, so we need to brutally close the app
                BootStrapper.Current.Exit();
            };
        }

        #region Menu Animation

        private bool _isMenuOpen;
        private bool _isNearbyOpen;

        private void PokeMenuMainButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_isMenuOpen)
                ShowPokeMenuStoryboard.Begin();
            else
                HidePokeMenuStoryboard.Begin();
            _isMenuOpen = !_isMenuOpen;
        }

        private void NearbyPokemonGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_isNearbyOpen)
                ShowNearbyGridStoryboard.Begin();
            else
                HideNearbyGridStoryboard.Begin();
            _isNearbyOpen = !_isNearbyOpen;
        }

        #endregion

        private void MapControl_ZoomLevelChanged(Windows.UI.Xaml.Controls.Maps.MapControl sender, object args)
        {
            //prevent zooming too far
            if (sender.ZoomLevel < 18)
            {
                sender.ZoomLevel = 18;
            }
        }
    }
}