using System;


namespace PokemonGo.RocketAPI.Exceptions
{
    public class AccountLockedException : Exception
    {
        public AccountLockedException() : base("Your account has been locked/banned.")
        {
        }
    }
}
