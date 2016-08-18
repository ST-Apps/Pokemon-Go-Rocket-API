using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using POGOProtos.Enums;

namespace PokemonGo_UWP.Entities
{
    public interface IMapPokemon : IUpdatable<IMapPokemon>, INotifyPropertyChanged
    {
        Geopoint Geoposition { get; set; }

        PokemonId PokemonId { get; }

        ulong EncounterId { get; }

        string SpawnpointId { get; }
    }
}
