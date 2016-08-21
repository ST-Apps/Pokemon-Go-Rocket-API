using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGoAPI;
using PokemonGoAPI.Session;

namespace PokemonGo.RocketAPI.Login
{
    internal class PtcLogin : ILoginType
    {
        private readonly string _password;
        private readonly string _username;

        public PtcLogin(string username, string password)
        {
            _username = username;
            _password = password;
        }

        internal struct LoginData
        {
            [JsonProperty("lt", Required = Required.Always)]
            public string Lt { get; set; }

            [JsonProperty("execution", Required = Required.Always)]
            public string Execution { get; set; }
        }

        /// <summary>
        ///     Responsible for retrieving login parameters for <see cref="PostLogin" />.
        /// </summary>
        /// <param name="httpClient">An initialized <see cref="HttpClient" /></param>
        /// <returns><see cref="LoginData" /> for <see cref="PostLogin" />.</returns>
        private static LoginData GetLoginData(System.Net.Http.HttpClient httpClient)
        {
            var loginDataResponse = httpClient.GetAsync(Constants.LoginUrl).Result;
            var loginData =
                JsonConvert.DeserializeObject<LoginData>(loginDataResponse.Content.ReadAsStringAsync().Result);
            return loginData;
        }

        /// <summary>
        ///     Responsible for submitting the login request.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="username">The user's PTC username.</param>
        /// <param name="password">The user's PTC password.</param>
        /// <param name="loginData"><see cref="LoginData" /> taken from PTC website using <see cref="GetLoginData" />.</param>
        /// <returns></returns>
        private static string PostLogin(System.Net.Http.HttpClient httpClient, string username, string password, LoginData loginData)
        {
            var loginResponse =
                httpClient.PostAsync(Constants.LoginUrl, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"lt", loginData.Lt},
                    {"execution", loginData.Execution},
                    {"_eventId", "submit"},
                    {"username", username},
                    {"password", password}
                })).Result;

            var loginResponseDataRaw = loginResponse.Content.ReadAsStringAsync().Result;
            if (!loginResponseDataRaw.Contains("{"))
            {
                var locationQuery = loginResponse.Headers.Location.Query;
                var ticketStartPosition = locationQuery.IndexOf("=", StringComparison.Ordinal) + 1;
                return locationQuery.Substring(ticketStartPosition, locationQuery.Length - ticketStartPosition);
            }

            var loginResponseData = JObject.Parse(loginResponseDataRaw);
            var loginResponseErrors = (JArray)loginResponseData["errors"];

            var errorMessages = string.Join(",", loginResponseErrors);
            if (errorMessages.Contains("Your username or password is incorrect.") ||
                errorMessages.Contains("As a security measure, your account has been disabled"))
                throw new LoginFailedException(loginResponse);

            throw new Exception($"Pokemon Trainer Club gave error(s): '{errorMessages}'");
        }

        /// <summary>
        ///     Responsible for finishing the oauth login request.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        private static AccessToken PostLoginOauth(System.Net.Http.HttpClient httpClient, string ticket)
        {
            var loginResponse =
                httpClient.PostAsync(Constants.LoginOauthUrl, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"client_id", "mobile-app_pokemon-go"},
                    {"redirect_uri", "https://www.nianticlabs.com/pokemongo/error"},
                    {"client_secret", "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"},
                    {"grant_type", "refresh_token"},
                    {"code", ticket}
                })).Result;

            var loginResponseDataRaw = loginResponse.Content.ReadAsStringAsync().Result;

            var oAuthData = Regex.Match(loginResponseDataRaw,
                "access_token=(?<accessToken>.*?)&expires=(?<expires>\\d+)");
            if (!oAuthData.Success)
            {
                throw new Exception($"Couldn't verify the OAuth login response data '{loginResponseDataRaw}'.");
            }
            return new AccessToken
            {
                Token = oAuthData.Groups["accessToken"].Value,
                Expiry = DateTime.UtcNow.AddSeconds(int.Parse(oAuthData.Groups["expires"].Value)),
                AuthType = AuthType.Ptc
            };
        }

        public async Task<AccessToken> GetAccessToken()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                AllowAutoRedirect = false
            };

            using (var tempHttpClient = new System.Net.Http.HttpClient(handler))
            {
                tempHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.LoginUserAgent);
                var loginData = GetLoginData(tempHttpClient);
                var ticket = PostLogin(tempHttpClient, _username, _password, loginData);
                var accessToken = PostLoginOauth(tempHttpClient, ticket);
                accessToken.Username = _username;
                await Task.CompletedTask;
                return accessToken;
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

        private static IDictionary<string, string> GenerateLoginRequest(SessionData sessionData, string user,
            string pass)
            => new Dictionary<string, string>
            {
                {"lt", sessionData.Lt},
                {"execution", sessionData.Execution},
                {"_eventId", "submit"},
                {"username", user},
                {"password", pass}
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

        private static async Task<string> GetLoginTicket(string username, string password,
            System.Net.Http.HttpClient tempHttpClient, SessionData sessionData)
        {
            HttpResponseMessage loginResp;
            var loginRequest = GenerateLoginRequest(sessionData, username, password);
            using (var formUrlEncodedContent = new FormUrlEncodedContent(loginRequest))
            {
                loginResp =
                    await tempHttpClient.PostAsync(Resources.PtcLoginUrl, formUrlEncodedContent).ConfigureAwait(false);
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
                tokenResp =
                    await tempHttpClient.PostAsync(Resources.PtcLoginOauth, formUrlEncodedContent).ConfigureAwait(false);
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