using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils.Game
{
    /// <summary>
    /// Fort can have 3 visual states: opened, closed, cooldown
    /// </summary>
    public enum FortDataStatus
    {
        Opened,
        Closed,
        Cooldown
    }
}
