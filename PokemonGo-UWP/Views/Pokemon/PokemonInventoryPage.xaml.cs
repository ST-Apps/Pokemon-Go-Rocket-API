using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;

namespace PokemonGo_UWP.Views
{
    public sealed partial class PokemonInventoryPage : Page
	{
		public PokemonInventoryPage()
		{
			InitializeComponent();

			NavigationCacheMode = NavigationCacheMode.Enabled;
		}

        #region  Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.ScrollPokemonToVisibleRequired += ScrollPokemonToVisible;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ViewModel.ScrollPokemonToVisibleRequired -= ScrollPokemonToVisible;
        }

        #endregion

        private void ScrollPokemonToVisible(PokemonDataWrapper p)
        {
            PokemonInventory.PokemonInventoryGridView.ScrollIntoView(p);
        }

    }
}