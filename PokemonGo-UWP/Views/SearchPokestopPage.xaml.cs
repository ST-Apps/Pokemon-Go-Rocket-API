using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPokestopPage : Page
    {
        public SearchPokestopPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
                ShowGatheredItemsMenuAnimation.From = GatheredItemsTranslateTransform.Y = ActualHeight;
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToSearchEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToSearchEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToSearchEvents()
        {
            ViewModel.SearchInCooldown += GameManagerViewModelOnSearchInCooldown;
            ViewModel.SearchInventoryFull += GameManagerViewModelOnSearchInventoryFull;
            ViewModel.SearchOutOfRange += GameManagerViewModelOnSearchOutOfRange;
            ViewModel.SearchSuccess += GameManagerViewModelOnSearchSuccess;
            // Add also handlers to report which items the user gained after the animation
            SpinPokestopImage.Completed += (s, e) => ShowGatheredItemsMenu.Begin();
        }

        private void UnsubscribeToSearchEvents()
        {
            ViewModel.SearchInCooldown -= GameManagerViewModelOnSearchInCooldown;
            ViewModel.SearchInventoryFull -= GameManagerViewModelOnSearchInventoryFull;
            ViewModel.SearchOutOfRange -= GameManagerViewModelOnSearchOutOfRange;
            ViewModel.SearchSuccess -= GameManagerViewModelOnSearchSuccess;
        }

        private void GameManagerViewModelOnSearchOutOfRange(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            OutOfRangeTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchInventoryFull(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            InventoryFullTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchInCooldown(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            CooldownTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchSuccess(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            SpinPokestopImage.Begin();
        }

        #endregion
    }
}