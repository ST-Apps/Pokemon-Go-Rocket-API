using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using Template10.Services.NavigationService;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokestopInfo : Page
    {
        public PokestopInfo()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationCacheMode = NavigationCacheMode.Enabled;
            var OriginalSearch = "http://google.com/search?q=" + NavigationHelper.NavigationState["PokestopName"];
            LoadInfo(ref Display, OriginalSearch);
            /*
                NavigationHelper.NavigationState["PokestopName"] = this;
                NavigationHelper.NavigationState["LastSelectedID"] = Id;
                BootStrapper.Current.NavigationService.Navigate(typeof(PokemonDetailPage)); 
            */
        }

        private void LoadInfo(ref WebView web, string url)
        {
            web.Navigate(new System.Uri(url));
        }

        private void ReturnToLast(object sender, RoutedEventArgs e)
        {
            //NavigationHelper.NavigationState["CurrentPokestop"] = NavigationHelper.NavigationState["PokestopId"];
            BootStrapper.Current.NavigationService.Navigate(typeof(GameMapPage));
        }
    }
}
