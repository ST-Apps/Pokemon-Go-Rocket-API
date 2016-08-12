using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
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

        private void StackPanel_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }

        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var button = sender as Button;
            var context = button.DataContext as PokemonDataWrapper;
            var result = await GameClient.TransferPokemon(context.Id);
            MessageDialog mes = new MessageDialog("Transfer " + result.Result + ". You got " + result.CandyAwarded + " candy", "Transfer " + context.PokemonId.ToString());
            mes.ShowAsync();
            await GameClient.UpdateInventory();   
        }
        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
        }
    }
}
