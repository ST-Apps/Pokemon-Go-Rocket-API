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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.ViewModels;
using POGOProtos.Enums;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokedexDetailPage : Page
    {
        public PokedexDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PokemonId prm =
                    ((JObject)JsonConvert.DeserializeObject((string)e.Parameter)).Last
                        .ToObject<PokemonId>();

            
            ((PokedexDetailViewModel)this.DataContext).Load(prm);
        }

        private void EvolutionTapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = (sender as Grid);
            var pokemon = grid.DataContext as PokemonModel;
            (this.DataContext as PokedexDetailViewModel).GoToPokemon(pokemon);
        }
    }
}
