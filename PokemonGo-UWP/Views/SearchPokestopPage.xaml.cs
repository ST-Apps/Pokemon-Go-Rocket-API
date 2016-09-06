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
        public bool IsLearnMoreEnabled
        {
            get { return SettingsService.Instance.IsLearnMoreEnabled; }
            set { SettingsService.Instance.IsLearnMoreEnabled = value; }
        }
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
            if (!IsLearnMoreEnabled)
            {
                HideLearnMoreButton();
            }
            else
            {
                ShowLearnMoreButton();
            }
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

        private void FindInfo(object sender, RoutedEventArgs e)
        {
            var pokestopName = ((Button)sender).Tag;
            NavigationHelper.NavigationState["PokestopName"] = pokestopName;
            //NavigationHelper.NavigationState["PokestopId"] = NavigationHelper.NavigationState["CurrentPokestop"];
            //var CurrentPokestop = NavigationHelper.NavigationState["CurrentPokestop"];
            //NavigationHelper.NavigationState["CurrentPokestop"] = CurrentPokestop;
            BootStrapper.Current.NavigationService.Navigate(typeof(PokestopInfo));
        }

        private void HideLearnMoreButton()
        {
            LearnMore.Visibility = Visibility.Collapsed;
        }

        private void ShowLearnMoreButton()
        {
            LearnMore.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
