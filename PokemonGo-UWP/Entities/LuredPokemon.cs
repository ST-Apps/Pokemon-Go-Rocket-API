using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using Template10.Common;
using Template10.Mvvm;

namespace PokemonGo_UWP.Entities
{
    
    public class LuredPokemon : IMapPokemon
    {
        /// <summary>
        /// Infos on the current lured Pokemon
        /// </summary>
        private FortLureInfo _lureInfo;


        /// <summary>
        ///     HACK - this should fix Pokestop floating on map
        /// </summary>
        public Point Anchor => new Point(0.5, 1);
        
        public LuredPokemon(FortLureInfo lureInfo, double lat, double lng)
        {
            _lureInfo = lureInfo;
            Geoposition = new Geopoint(GetLocation(lat, lng, 1));
        }

        public void Update(IMapPokemon update)
        {
            var lure = (LuredPokemon) update;

            _lureInfo = lure._lureInfo;
            Geoposition = lure.Geoposition;            

            OnPropertyChanged(nameof(PokemonId));
            OnPropertyChanged(nameof(EncounterId));
            OnPropertyChanged(nameof(SpawnpointId));
            OnPropertyChanged(nameof(Geoposition));
        }

        #region Wrapped Properties

        public PokemonId PokemonId => _lureInfo.ActivePokemonId;

        public ulong EncounterId => _lureInfo.EncounterId;

        public string SpawnpointId => _lureInfo.FortId;

        public Geopoint Geoposition { get; set; }

        #endregion

        private DelegateCommand _tryCatchPokemon;

        /// <summary>
        ///     We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        /// </summary>
        public DelegateCommand TryCatchPokemon => _tryCatchPokemon ?? (
            _tryCatchPokemon = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentPokemon"] = this;
                // Disable map update
                GameClient.ToggleUpdateTimer(false);
                BootStrapper.Current.NavigationService.Navigate(typeof(CapturePokemonPage));
            }, () => true)
        );

        private BasicGeoposition GetLocation(double x0, double y0, int radius)
        {
            var random = new Random();

            // Convert radius from meters to degrees
            double radiusInDegrees = radius / 111000f;

            var u = random.NextDouble();
            var v = random.NextDouble();
            var w = radiusInDegrees * Math.Sqrt(u);
            var t = 2 * Math.PI * v;
            var x = w * Math.Cos(t);
            var y = w * Math.Sin(t);

            // Adjust the x-coordinate for the shrinking of the east-west distances
            var new_x = x / Math.Cos(y0);

            var foundLatitude = new_x + x0;
            var foundLongitude = y + y0;
            return new BasicGeoposition {Latitude = foundLatitude, Longitude = foundLongitude };
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
