using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class InvalidResponseException : Exception
    {
        public InvalidResponseException()
        {}

        public InvalidResponseException(string message)
            : base(message)
        { }
    }
}