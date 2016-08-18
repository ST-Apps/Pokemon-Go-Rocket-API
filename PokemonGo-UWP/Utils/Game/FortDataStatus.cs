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
    [Flags]
    public enum FortDataStatus
    {
        Closed = 0,
        Opened = 1,
        Cooldown = 2,
        Lure = 4
    }
}
