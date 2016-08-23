using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class ApiNonRecoverableException : Exception
    {
        public ApiNonRecoverableException(string reason) : base(reason)
        {
        }
    }
}