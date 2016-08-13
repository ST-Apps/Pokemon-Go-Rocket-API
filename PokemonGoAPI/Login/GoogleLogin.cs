using System;
using System.Threading.Tasks;
using Windows.System;
using DankMemes.GPSOAuthSharp;
using PokemonGo.RocketAPI.Exceptions;

namespace PokemonGo.RocketAPI.Login
{
    public class GoogleLogin : ILoginType
    {
        public const string GoogleLoginAndroidId = "9774d56d682e549c";

        public const string GoogleLoginService =
            "audience:server:client_id:848232511240-7so421jotr2609rmqakceuu1luuq0ptb.apps.googleusercontent.com";

        public const string GoogleLoginApp = "com.nianticlabs.pokemongo";
        public const string GoogleLoginClientSig = "321187995bc7cdc2b5fc91b11a96e2baa8602c62";
        private readonly string _email;

        private readonly string _password;

        public GoogleLogin(string email, string password)
        {
            _email = email;
            _password = password;
        }

#pragma warning disable 1998
        public async Task<string> GetAccessToken()
#pragma warning restore 1998
        {
            var client = new GPSOAuthClient(_email, _password);
            var response = await client.PerformMasterLogin(GoogleLoginAndroidId);

            if (response.ContainsKey("Error"))
            {
                if (response.ContainsKey("Url"))
                {
                    await Launcher.LaunchUriAsync(new Uri(response["Url"]));
                }
                else
                    throw new GoogleException(response["Error"]);
            }

            //Todo: captcha/2fa implementation

            if (!response.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            var oauthResponse =
                await
                    client.PerformOAuth(response["Token"], GoogleLoginService, GoogleLoginAndroidId, GoogleLoginApp,
                        GoogleLoginClientSig);

            if (!oauthResponse.ContainsKey("Auth"))
                throw new GoogleOfflineException();

            return oauthResponse["Auth"];
        }
    }
}