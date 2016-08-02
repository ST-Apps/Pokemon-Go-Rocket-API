using Windows.UI.Xaml.Controls;
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

        private void passwordBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
				loginButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
			}
        }
    }
}