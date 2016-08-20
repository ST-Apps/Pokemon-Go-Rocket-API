using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.Views
{
    public sealed partial class PlayerProfilePage : Page
    {
        public PlayerProfilePage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private void CustomizeUICloseButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HideInventoryStoryboard.Begin();
        }

        private void ShowInventoryButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ShowInventoryStoryboard.Begin();
        }
    }
}