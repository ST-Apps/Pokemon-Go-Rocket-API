using Google.Protobuf;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Login;
using PokemonGoAPI.Enums;
using System;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Rpc
{
    public delegate void GoogleDeviceCodeDelegate(string code, string uri);

    public class Login : BaseRpc
    {
        //public event GoogleDeviceCodeDelegate GoogleDeviceCodeEvent;
        private readonly ILoginType login;

        public Login(Client client) : base(client)
        {
            login = SetLoginType(client.Settings);
        }

        private static ILoginType SetLoginType(ISettings settings)
        {
            switch (settings.AuthType)
            {
                case AuthType.Google:
                    return new GoogleLogin(settings.GoogleUsername, settings.GooglePassword);
                case AuthType.Ptc:
                    return new PtcLogin(settings.PtcUsername, settings.PtcPassword);
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.AuthType), "Unknown AuthType");
            }
        }

        public async Task DoLogin()
        {
            if (Client.AccessToken == null || Client.AccessToken.IsExpired)
            {
                Client.AccessToken = await login.GetAccessToken().ConfigureAwait(false);
            }
            ///robertmclaws: Is it really necessary to put this in a separate function?
            await SetServer().ConfigureAwait(false);                        
        }

        private async Task SetServer()
        {

            var serverRequest = RequestBuilder.GetInitialRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = new GetPlayerMessage().ToByteString()
                },
                new Request
                {
                    RequestType = RequestType.CheckChallenge,
                    RequestMessage = new CheckChallengeMessage().ToByteString()
                }
            );

            var serverResponse = await PostProto<Request>(Resources.RpcUrl, serverRequest);

            if(serverRequest.StatusCode == (int) StatusCode.AccessDenied)
            {
                throw new AccountLockedException();
            }

            if (serverResponse.AuthTicket == null)
            {
                Client.AccessToken = null;
                throw new AccessTokenExpiredException();
            }

            Client.AccessToken.AuthTicket = serverResponse.AuthTicket;

            if (serverResponse.StatusCode == (int)StatusCode.Redirect)
            {
                Client.ApiUrl = serverResponse.ApiUrl;
            }
        }
    }
}