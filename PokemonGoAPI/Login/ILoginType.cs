using System.Threading.Tasks;
using PokemonGoAPI.Session;

namespace PokemonGo.RocketAPI.Login
{
    /// <summary>
    ///     Interface for the login into the game using either Google or PTC
    /// </summary>
    internal interface ILoginType
    {
        /// <summary>
        ///     Gets the access token.
        /// </summary>
        /// <returns></returns>
        Task<AccessToken> GetAccessToken();
    }
}