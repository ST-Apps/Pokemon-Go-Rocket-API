using DankMemes.GPSOAuthSharp;
using PokemonGo.RocketAPI.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Login
{
    public class GoogleLogin : ILoginType
    {
        private readonly string password;
        private readonly string email;

        public GoogleLogin(string email, string password)
        {
            this.email = email;
            this.password = password;
        }

#pragma warning disable 1998
        public async Task<string> GetAccessToken()
#pragma warning restore 1998
        {
            var client = new GPSOAuthClient(email, password);
            var response = await client.PerformMasterLogin();

            if (response.ContainsKey("Error"))
                throw new GoogleException(response["Error"]);

            //Todo: captcha/2fa implementation

            if (!response.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            var oauthResponse = await client.PerformOAuth(response["Token"],
                "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com",
                "com.nianticlabs.pokemongo",
                "321187995bc7cdc2b5fc91b11a96e2baa8602c62");

            if (!oauthResponse.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            return oauthResponse["Auth"];
        }
    }
}