using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Views
{
    public sealed partial class PlayerProfilePage : Page
    {
        public PlayerProfilePage()
        {
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //ViewModel.NavigateToDetailPage(sender, e);
        }
    }
}