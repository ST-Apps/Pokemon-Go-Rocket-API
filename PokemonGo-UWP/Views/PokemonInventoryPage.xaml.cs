using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokemonInventoryPage : Page
    {
        public PokemonInventoryPage()
        {
            InitializeComponent();
            // TODO: fix header
            // Setup incubators translation
            Loaded += (s, e) =>
            {
                ShowIncubatorsModalAnimation.From =
                    HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
                HideIncubatorsModalStoryboard.Completed += (ss, ee) => { IncubatorsModal.IsModal = false; };
            };
        }


        private void ToggleIncubatorModel(object sender, TappedRoutedEventArgs e)
        {
            if (IncubatorsModal.IsModal)
            {
                HideIncubatorsModalStoryboard.Begin();
            }
            else
            {
                IncubatorsModal.IsModal = true;
                ShowIncubatorsModalStoryboard.Begin();
            }
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs backRequestedEventArgs)
        {
            if (!(SortMenuPanel.Opacity > 0)) return;
            backRequestedEventArgs.Handled = true;
            HideSortMenuStoryboard.Begin();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
        }

        #endregion
    }
}