using System;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class AccountNotVerifiedException : Exception
    {
        public AccountNotVerifiedException(string message) : base(message)
        {
            
        }
    }
}