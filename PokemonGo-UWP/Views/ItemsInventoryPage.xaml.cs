using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemsInventoryPage : Page
    {
        public ItemsInventoryPage()
        {
            InitializeComponent();
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (!(DiscardPanel.Opacity > 0)) return;
            backRequestedEventArgs.Handled = true;
            HideDiscardStoryboard.Begin();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
        }

        #endregion
    }
}