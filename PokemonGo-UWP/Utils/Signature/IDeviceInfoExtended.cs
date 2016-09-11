using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
    public interface IDeviceInfoExtended : IDeviceInfo
    {
        void CollectLocationData();
        string Platform { get; }
    }
}
