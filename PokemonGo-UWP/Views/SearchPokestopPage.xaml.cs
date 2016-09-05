using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using Template10.Services.NavigationService;
using Template10.Common;

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
            OutOfRangeTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchInventoryFull(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            SpinPokestopImage.Begin();
            InventoryFullTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchInCooldown(object sender, EventArgs eventArgs)
        {            
            CooldownTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnSearchSuccess(object sender, EventArgs eventArgs)
        {
            SearchPokestopButton.IsEnabled = false;
            SpinPokestopImage.Begin();
        }

        private async void startSearch(object sender, RoutedEventArgs e)
        {
            //Gets data for link
            var pokestopName = ((Button)sender).Tag;

            // The URI to launch
            var uriSearch = new Uri(@"http://www.google.com/search?q=" + pokestopName);

            // Launch the URI
            var success = await Windows.System.Launcher.LaunchUriAsync(uriSearch);

            if (!success)
            {
                var dialog = new MessageDialog("Error!");
                await dialog.ShowAsync();
            }
        }

        private void FindInfo(object sender, RoutedEventArgs e)
        {
            var pokestopName = ((Button)sender).Tag;
            NavigationHelper.NavigationState["PokestopName"] = pokestopName;
            //NavigationHelper.NavigationState["PokestopId"] = NavigationHelper.NavigationState["CurrentPokestop"];
            BootStrapper.Current.NavigationService.Navigate(typeof(PokestopInfo));
        }

        private void HideButton()
        {
            LearnMore.Visibility = Visibility.Collapsed;
        }

        private void ShowButton()
        {
            LearnMore.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
