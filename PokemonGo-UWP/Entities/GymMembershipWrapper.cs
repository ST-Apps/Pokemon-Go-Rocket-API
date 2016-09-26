using System;
using System.ComponentModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Google.Protobuf;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Game;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Views;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using Template10.Common;
using Template10.Mvvm;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using POGOProtos.Inventory.Item;
using POGOProtos.Data.Gym;
using POGOProtos.Data.Player;

namespace PokemonGo_UWP.Entities
{
    public class GymMembershipWrapper : INotifyPropertyChanged
    {
        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        private GymMembership _gymMembership;
        private PokemonDataWrapper _pokemonData;
        private bool _selected;

        public GymMembershipWrapper(GymMembership gymMembership)
        {
            _gymMembership = gymMembership;
            _pokemonData = new PokemonDataWrapper(_gymMembership.PokemonData);
        }

        #region Wrapped Properties

        public PokemonId PokemonId => _pokemonData.PokemonId;
        public int PokemonCp => _pokemonData.Cp;
        public string PokemonName => _pokemonData.Name;

        public string PlayerName => _gymMembership.TrainerPublicProfile.Name;
        public int PlayerLevel => _gymMembership.TrainerPublicProfile.Level;
        public PlayerAvatar PlayerAvatar => _gymMembership.TrainerPublicProfile.Avatar;


        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
            }
        }

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