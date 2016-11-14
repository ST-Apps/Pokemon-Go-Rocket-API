using System.ComponentModel;
using Newtonsoft.Json;
using PokemonGo_UWP.Utils.Helpers;
using POGOProtos.Map;
using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace PokemonGo_UWP.Entities
{
    /// <summary>
    ///     We need to wrap this so we can reduce the number of ui updates.
    /// </summary>
    public class SpawnPointWrapper : IUpdatable<SpawnPoint>, INotifyPropertyChanged
    {
        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        private SpawnPoint _spawnPoint;

        public Point Anchor => new Point(0.5, 1);

        public SpawnPointWrapper(SpawnPoint spawnPoint)
        {
            _spawnPoint = spawnPoint;
            Geoposition =
	            new Geopoint(new BasicGeoposition { Latitude = _spawnPoint.Latitude, Longitude = _spawnPoint.Longitude });
        }

        /// <summary>
        /// The file name for spawnpoint, located in /Assets/Icons
        /// </summary>
        public string ImageFileName => "spawnpoint.png";

        /// <summary>
        /// The file name for spawnpoint, located in /Assets/Icons
        /// </summary>
        public string ImageFilePath => $"ms-appx:///Assets/Icons/spawnpoint.png";

        public void Update(SpawnPoint update)
        {
            _spawnPoint = update;
        }

        #region Wrapped Properties

        //public float DistanceInMeters => _spawnPoint.DistanceInMeters;
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
