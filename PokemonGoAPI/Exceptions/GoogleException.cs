using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class GoogleException : Exception
    {
        public GoogleException(string message) : base(message)
        {
            
        }
    }
}
