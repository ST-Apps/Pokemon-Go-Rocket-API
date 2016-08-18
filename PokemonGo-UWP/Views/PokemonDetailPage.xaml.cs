using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Utils;
using POGOProtos.Networking.Responses;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PokemonDetailPage : Page
    {
        public PokemonDetailPage()
        {
            this.InitializeComponent();
            // Setup evolution stats translation
            Loaded += (s, e) =>
            {
                ShowEvolveStatsModalAnimation.From = EvolveStatsTranslateTransform.Y = ActualHeight;

                PokemonTypeCol.MinWidth = PokemonTypeCol.ActualWidth;
                PokemonTypeCol.Width = new GridLength(1, GridUnitType.Star);
                LevelProgressBar.Diameter = (int) this.ActualWidth*3/2 -16;
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }

        #region Overrides of Page

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            ViewModel.PokemonEvolved += ViewModelOnPokemonEvolved;
            ShowEvolveMenuStoryboard.Completed += async (s, e) =>
            {
                await Task.Delay(1000);
                EvolvePokemonStoryboard.Begin();
            };
            EvolvePokemonStoryboard.Completed += async (s, e) =>
            {
                await Task.Delay(1000);
                ShowEvolveStatsModalStoryboard.Begin();
            };
        }        

        private void UnsubscribeToCaptureEvents()
        {
            ViewModel.PokemonEvolved -= ViewModelOnPokemonEvolved;
        }

        private void ViewModelOnPokemonEvolved(object sender, EventArgs evolvePokemonResponse)
        {
            ShowEvolveMenuStoryboard.Begin();
        }

        #endregion
    }
}
