using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Newtonsoft.Json;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Views;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;
using Template10.Common;
using Template10.Mvvm;

namespace PokemonGo_UWP.Entities
{
    public class MapPokemonWrapper : IMapPokemon
    {
        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        private MapPokemon _mapPokemon;

        private DelegateCommand _tryCatchPokemon;

        public MapPokemonWrapper(MapPokemon mapPokemon)
        {
            _mapPokemon = mapPokemon;
            Geoposition =
                new Geopoint(new BasicGeoposition {Latitude = _mapPokemon.Latitude, Longitude = _mapPokemon.Longitude});
        }

        /// <summary>
        ///     HACK - this should fix Pokestop floating on map
        /// </summary>
        public Point Anchor => new Point(0.5, 1);

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

        public void Update(IMapPokemon update)
        {
            _mapPokemon = ((MapPokemonWrapper) update)._mapPokemon;

            OnPropertyChanged(nameof(PokemonId));
            OnPropertyChanged(nameof(EncounterId));
            OnPropertyChanged(nameof(SpawnpointId));
            OnPropertyChanged(nameof(Geoposition));
        }

        #region Wrapped Properties

        public PokemonId PokemonId => _mapPokemon.PokemonId;

        public ulong EncounterId => _mapPokemon.EncounterId;

        public string SpawnpointId => _mapPokemon.SpawnPointId;

        public Geopoint Geoposition { get; set; }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}