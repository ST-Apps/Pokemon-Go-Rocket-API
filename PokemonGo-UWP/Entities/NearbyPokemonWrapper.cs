using System.ComponentModel;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;

namespace PokemonGo_UWP.Entities
{
    /// <summary>
    ///     We need to wrap this so we can reduce the number of ui updates.
    /// </summary>
    public class NearbyPokemonWrapper : IUpdatable<NearbyPokemon>, INotifyPropertyChanged
    {
        private NearbyPokemon _nearbyPokemon;

        public NearbyPokemonWrapper(NearbyPokemon nearbyPokemon)
        {
            _nearbyPokemon = nearbyPokemon;
        }

        /// <summary>
        /// The file name for this Pokemon, located in /Assets/Pokemons
        /// </summary>
        public string ImageFileName => $"{(int)PokemonId}.png";

        /// <summary>
        /// The file name for this Pokemon, located in /Assets/Pokemons
        /// </summary>
        public string ImageFilePath => $"ms-appx:///Assets/Pokemons/{(int)PokemonId}.png";

        public void Update(NearbyPokemon update)
        {
            _nearbyPokemon = update;

            OnPropertyChanged(nameof(PokemonId));
            OnPropertyChanged(nameof(DistanceInMeters));
            OnPropertyChanged(nameof(EncounterId));
        }

        #region Wrapped Properties

        public PokemonId PokemonId => _nearbyPokemon.PokemonId;

        public float DistanceInMeters => _nearbyPokemon.DistanceInMeters;

        public ulong EncounterId => _nearbyPokemon.EncounterId;

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