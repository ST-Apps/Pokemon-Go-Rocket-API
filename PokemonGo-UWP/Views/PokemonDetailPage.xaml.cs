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
            };
        }

        // TODO: move this to App.xaml.cs
        private int _mapBoxIndex = -1;

        private void SetupMap()
        {
            if (ApplicationKeys.MapBoxTokens.Length > 0 && SettingsService.Instance.IsNianticMapEnabled)
            {
                if (_mapBoxIndex == -1)
                    _mapBoxIndex = new Random().Next(0, ApplicationKeys.MapBoxTokens.Length);
                Logger.Write($"Using MapBox's keyset {_mapBoxIndex}");
                var mapBoxTileSource =
                    new HttpMapTileDataSource(
                        "https://api.mapbox.com/styles/v1/" +
                        (RequestedTheme == ElementTheme.Light
                            ? ApplicationKeys.MapBoxStylesLight[_mapBoxIndex]
                            : ApplicationKeys.MapBoxStylesDark[_mapBoxIndex]) +
                        "/tiles/256/{zoomlevel}/{x}/{y}?access_token=" +
                        ApplicationKeys.MapBoxTokens[_mapBoxIndex])
                    {
                        AllowCaching = true
                    };

                DetailMapControl.Style = MapStyle.None;
                DetailMapControl.TileSources.Clear();
                DetailMapControl.TileSources.Add(new MapTileSource(mapBoxTileSource)
                {
                    AllowOverstretch = true,
                    IsFadingEnabled = false,
                    Layer = MapTileLayer.BackgroundReplacement
                });
            }
            else
            {
                // Fallback to Bing Maps   
                // TODO: map color scheme is set but the visual style doesn't update!             
                DetailMapControl.ColorScheme = ViewModel.CurrentTheme == ElementTheme.Dark
                    ? MapColorScheme.Dark
                    : MapColorScheme.Light;
                DetailMapControl.TileSources.Clear();
                DetailMapControl.Style = MapStyle.Terrain;
            }
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
            SetupMap();
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
