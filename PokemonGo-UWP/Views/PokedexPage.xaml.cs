using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokedexPage : Page
    {
        public PokedexPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += PokedexPage_BackRequested;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName.CompareTo(nameof(ViewModel.SelectedPokedexEntry)) == 0)
                scrollPokedexEntry.ChangeView(0, 0, scrollPokedexEntry.ZoomFactor);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= PokedexPage_BackRequested;
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void PokedexPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            ViewModel.CloseCommand.Execute();
        }
    }
}
