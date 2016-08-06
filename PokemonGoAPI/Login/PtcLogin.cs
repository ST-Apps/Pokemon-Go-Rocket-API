using Newtonsoft.Json;
using PokemonGo.RocketAPI.Exceptions;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation;

namespace PokemonGo.RocketAPI.Login
{
    class PtcLogin : ILoginType
    {
        readonly string password;
        readonly string username;

        public PtcLogin(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
        public async Task<string> GetAccessToken()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var tempHttpClient = new System.Net.Http.HttpClient(handler))
            {
                //Get session cookie
                var sessionData = await GetSessionCookie(tempHttpClient).ConfigureAwait(false);

                //Login
                var ticketId = await GetLoginTicket(username, password, tempHttpClient, sessionData).ConfigureAwait(false);

                //Get tokenvar
                return await GetToken(tempHttpClient, ticketId).ConfigureAwait(false);
            }
        }

        private static string ExtracktTicketFromResponse(HttpResponseMessage loginResp)
        {
            var location = loginResp.Headers.Location;
            if (location == null)
                throw new LoginFailedException(loginResp);

            var decoder = new WwwFormUrlDecoder(loginResp.Headers.Location.Query);
            var ticketId = decoder.GetFirstValueByName("ticket");

            if (ticketId == null)
                throw new PtcOfflineException();

            return ticketId;
        }

        private static IDictionary<string, string> GenerateLoginRequest(SessionData sessionData, string user, string pass)
            => new Dictionary<string, string>
            {
                { "lt", sessionData.Lt },
                { "execution", sessionData.Execution },
                { "_eventId", "submit" },
                { "username", user },
                { "password", pass }
            };

        private static IDictionary<string, string> GenerateTokenVarRequest(string ticketId)
            => new Dictionary<string, string>
            {
                {"client_id", "mobile-app_pokemon-go"},
                {"redirect_uri", "https://www.nianticlabs.com/pokemongo/error"},
                {"client_secret", "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"},
                {"grant_type", "refresh_token"},
                {"code", ticketId}
            };

        private static async Task<string> GetLoginTicket(string username, string password, System.Net.Http.HttpClient tempHttpClient, SessionData sessionData)
        {
            HttpResponseMessage loginResp;
            var loginRequest = GenerateLoginRequest(sessionData, username, password);
            using (var formUrlEncodedContent = new FormUrlEncodedContent(loginRequest))
            {
                loginResp = await tempHttpClient.PostAsync(Resources.PtcLoginUrl, formUrlEncodedContent).ConfigureAwait(false);
            }

            var ticketId = ExtracktTicketFromResponse(loginResp);
            return ticketId;
        }

        private static async Task<SessionData> GetSessionCookie(System.Net.Http.HttpClient tempHttpClient)
        {
            var sessionResp = await tempHttpClient.GetAsync(Resources.PtcLoginUrl).ConfigureAwait(false);
            var data = await sessionResp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var sessionData = JsonConvert.DeserializeObject<SessionData>(data);
            return sessionData;
        }

        private static async Task<string> GetToken(System.Net.Http.HttpClient tempHttpClient, string ticketId)
        {
            HttpResponseMessage tokenResp;
            var tokenRequest = GenerateTokenVarRequest(ticketId);
            using (var formUrlEncodedContent = new FormUrlEncodedContent(tokenRequest))
            {
                tokenResp = await tempHttpClient.PostAsync(Resources.PtcLoginOauth, formUrlEncodedContent).ConfigureAwait(false);
            }

            var tokenData = await tokenResp.Content.ReadAsStringAsync().ConfigureAwait(false);
            var decoder = new WwwFormUrlDecoder(tokenData);
            return decoder.GetFirstValueByName("access_token");
        }

        private class SessionData
        {
            public string Lt { get; set; }
            public string Execution { get; set; }
        }
    }
}