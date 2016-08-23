using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoAPI.Enums
{
    public enum StatusCode : int
    {
        Unknown = 0,
        Success = 1,
        AccessDenied = 3,
        ServerOverloaded = 52,
        Redirect = 53,
        InvalidToken = 102,
    }
}
