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
    public sealed partial class EnterGymPage : Page
    {
        public EnterGymPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                // Of course binding doesn't work so we need to manually setup height for animations
                // TODO: Set up height for animations
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToEnterEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToEnterEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToEnterEvents()
        {
            ViewModel.EnterOutOfRange += GameManagerViewModelOnEnterOutOfRange;
            ViewModel.EnterSuccess += GameManagerViewModelOnEnterSuccess;

            // TODO: What after the enter succeeded
        }

        private void UnsubscribeToEnterEvents()
        {
            ViewModel.EnterOutOfRange -= GameManagerViewModelOnEnterOutOfRange;
            ViewModel.EnterSuccess -= GameManagerViewModelOnEnterSuccess;
        }

        private void GameManagerViewModelOnEnterOutOfRange(object sender, EventArgs eventArgs)
        {            
            OutOfRangeTextBlock.Visibility = ErrorMessageBorder.Visibility = Visibility.Visible;
        }

        private void GameManagerViewModelOnEnterSuccess(object sender, EventArgs eventArgs)
        {
        }

        #endregion
    }
}