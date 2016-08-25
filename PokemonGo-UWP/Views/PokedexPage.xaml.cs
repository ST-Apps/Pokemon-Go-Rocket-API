using PokemonGo_UWP.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    public sealed partial class PokedexPage : Page
    {
        public PokedexPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.CompareTo(nameof(ViewModel.SelectedPokedexEntry)) == 0)
                scrollPokedexEntry.ChangeView(0, 0, scrollPokedexEntry.ZoomFactor);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
        private WidthConverter widthCalc = new WidthConverter();
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var newWidth = (int)widthCalc.Convert(1, null, "0,28", null);
            pokeindex.Width = newWidth;
        }
    }
}
