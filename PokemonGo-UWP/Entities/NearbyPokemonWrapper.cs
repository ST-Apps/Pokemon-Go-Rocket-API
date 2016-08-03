using System;
using POGOProtos.Enums;
using POGOProtos.Map.Pokemon;

namespace PokemonGo_UWP.Entities
{
    /// <summary>
    ///     We need to wrap <see cref="NearbyPokemon" /> to add the sprite
    /// </summary>
    public class NearbyPokemonWrapper
    {
        private readonly NearbyPokemon _nearbyPokemon;

        public NearbyPokemonWrapper(NearbyPokemon nearbyPokemon, Uri sprite)
        {
            _nearbyPokemon = nearbyPokemon;
            PokemonSprite = sprite;
        }

        public Uri PokemonSprite { get; }

        #region Wrapped Properties

        public PokemonId PokemonId => _nearbyPokemon.PokemonId;

        public float DistanceInMeters => _nearbyPokemon.DistanceInMeters;

        public ulong EncounterId => _nearbyPokemon.EncounterId;

        #endregion
    }
}