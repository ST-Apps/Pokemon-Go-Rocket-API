using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void passwordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != VirtualKey.Enter) return;
            // If username contains @ we login with google
            if (LoginUsernameTextBox.Text.Contains("@"))
                GoogleLoginButton.Focus(FocusState.Programmatic);
            else
                PtcLoginButton.Focus(FocusState.Programmatic);
        }
    }
}