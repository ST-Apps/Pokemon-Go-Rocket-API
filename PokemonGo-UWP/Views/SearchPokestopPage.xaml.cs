using PokemonGo_UWP.Utils;
using System;
using Template10.Common;
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
        public bool IsLearnMoreEnabled
        {
            get { return SettingsService.Instance.IsMusicEnabled; }
            set { SettingsService.Instance.IsMusicEnabled = value; }
        }
        public SearchPokestopPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
                ShowGatheredItemsMenuAnimation.From = GatheredItemsTranslateTransform.Y = ActualHeight;
            };
            if (IsLearnMoreEnabled)
            {
                PokestopLearnMore.Visibility = Visibility.Visible;
            } else
            {
                PokestopLearnMore.Visibility = Visibility.Collapsed;
            }
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

            ViewModel.ShowModifierDetails += GameManagerViewModelOnShowModifierDetails;
            ViewModel.HideModifierDetails += GameManagerViewModelOnHideModifierDetails;

            ViewModel.AddModifierSuccess += GameManagerViewModelOnAddModifierTooFarAwayAddModifierSuccess;
            ViewModel.AddModifierTooFarAway += GameManagerViewModelOnAddModifierTooFarAway;
            ViewModel.AddModifierAlreadyHasModifier += GameManagerViewModelOnAddModifierFortAlreadyHasModifier;
            ViewModel.AddModifierNoItemInInventory += GameManagerViewModelOnAddModifierNoItemInInventory;
        }

        private void UnsubscribeToSearchEvents()
        {
            ViewModel.SearchInCooldown -= GameManagerViewModelOnSearchInCooldown;
            ViewModel.SearchInventoryFull -= GameManagerViewModelOnSearchInventoryFull;
            ViewModel.SearchOutOfRange -= GameManagerViewModelOnSearchOutOfRange;
            ViewModel.SearchSuccess -= GameManagerViewModelOnSearchSuccess;

            ViewModel.ShowModifierDetails -= GameManagerViewModelOnShowModifierDetails;
            ViewModel.HideModifierDetails -= GameManagerViewModelOnHideModifierDetails;

            ViewModel.AddModifierSuccess -= GameManagerViewModelOnAddModifierTooFarAwayAddModifierSuccess;
            ViewModel.AddModifierTooFarAway -= GameManagerViewModelOnAddModifierTooFarAway;
            ViewModel.AddModifierAlreadyHasModifier -= GameManagerViewModelOnAddModifierFortAlreadyHasModifier;
            ViewModel.AddModifierNoItemInInventory -= GameManagerViewModelOnAddModifierNoItemInInventory;
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

        private void GameManagerViewModelOnShowModifierDetails(object sender, EventArgs eventArgs)
        {
            ShowFortModifierDetails.Begin();
            FortModifierPanel.Visibility = Visibility.Visible;
            ErrorMessageBorder.Visibility = Visibility.Collapsed;
			if (ViewModel.IsPokestopLured)									// Cannot choose item when there is a module installed
			{
				DeployModifierButton.IsEnabled = false;
				DeployModifierButton.Opacity = .5;
			}
			if (!ViewModel.IsModifierAvailable)								// When there is no item in the players' inventory, do not show it at all
			{
				DeployModifierButton.Visibility = Visibility.Collapsed;
			}
        }

        private void GameManagerViewModelOnHideModifierDetails(object sender, EventArgs eventArgs)
        {
            HideFortModifierDetails.Begin();
            FortModifierPanel.Visibility = Visibility.Collapsed;
        }

        private void GameManagerViewModelOnAddModifierTooFarAwayAddModifierSuccess(object sender, EventArgs e)
        {
            AddModifierSuccessTextBlock.Visibility = DetailsSuccessMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnAddModifierTooFarAway(object sender, EventArgs eventArgs)
        {
            TooFarAwayTextBlock.Visibility = DetailsErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnAddModifierFortAlreadyHasModifier(object sender, EventArgs eventArgs)
        {
            FortAlreadyHasModifierTextBlock.Visibility = DetailsErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnAddModifierNoItemInInventory(object sender, EventArgs eventArgs)
        {
            NoItemInInventoryTextBlock.Visibility = DetailsErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void PokestopLearnMore_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.NavigationState["PokestopName"] = pokestopName.Text;
            BootStrapper.Current.NavigationService.Navigate(typeof(PokestopInfoPage));
        }
        #endregion
    }
}
