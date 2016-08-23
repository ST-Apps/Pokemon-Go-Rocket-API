using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            if (Windows.Devices.Sensors.Compass.GetDefault() == null){
                CompassBoxItem.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
    }
}
