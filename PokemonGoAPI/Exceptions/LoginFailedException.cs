using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Exceptions
{
    public class LoginFailedException : Exception
    {
        public LoginFailedException(HttpResponseMessage loginResponse)
        {
            LoginResponse = loginResponse;
        }

        public HttpResponseMessage LoginResponse { get; }

        public async Task<string> GetLoginResponseContentAsString()
        {
            return await LoginResponse.Content.ReadAsStringAsync();
        }
    }
}