using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Login
{
    /// <summary>
    /// Interface for the login into the game using either Google or PTC
    /// </summary>
    interface ILoginType
    {
        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <returns></returns>
        Task<string> GetAccessToken();
    }
}