using Windows.Devices.Geolocation;
using Windows.Foundation;
using AllEnum;
using Newtonsoft.Json;
using PokemonGo.RocketAPI.GeneratedCode;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Common;
using Template10.Mvvm;

namespace PokemonGo_UWP.Entities
{
    public class MapPokemonWrapper
    {
        private readonly MapPokemon _mapPokemon;

        public MapPokemonWrapper(MapPokemon mapPokemon)
        {
            _mapPokemon = mapPokemon;
            Geoposition =
                new Geopoint(new BasicGeoposition {Latitude = _mapPokemon.Latitude, Longitude = _mapPokemon.Longitude});
        }

        /// <summary>
        /// HACK - this should fix Pokestop floating on map
        /// </summary>
        public Point Anchor => new Point(0.5, 1);

        private DelegateCommand _tryCatchPokemon;

        /// <summary>
        ///     We're just navigating to the capture page, reporting that the player wants to capture the selected Pokemon.
        /// </summary>
        public DelegateCommand TryCatchPokemon => _tryCatchPokemon ?? (
            _tryCatchPokemon = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentPokemon"] = this;
                BootStrapper.Current.NavigationService.Navigate(typeof(CapturePokemonPage), true);
            }, () => true)
            );


        #region Wrapped Properties

        public PokemonId PokemonId => _mapPokemon.PokemonId;

        public ulong EncounterId => _mapPokemon.EncounterId;

        public long ExpirationTimestampMs => _mapPokemon.ExpirationTimestampMs;

        public string SpawnpointId => _mapPokemon.SpawnpointId;

        public Geopoint Geoposition { get; set; }

        public double Latitude => _mapPokemon.Latitude;

        public double Longitude => _mapPokemon.Longitude;

        #endregion
    }
}