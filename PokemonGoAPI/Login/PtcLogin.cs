using Newtonsoft.Json;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGoAPI;
using PokemonGoAPI.Login;
using PokemonGoAPI.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;

namespace PokemonGo.RocketAPI.Login
{

    /// <summary>
    /// 
    /// </summary>
    internal class PtcLogin : ILoginType
    {

        #region Private Members

        private static HttpClient HttpClient;

        private static CookieContainer Cookies;

        /// <summary>
        /// The Password for the user currently attempting  to authenticate.
        /// </summary>
        private string Password { get; }

        /// <summary>
        /// The Username for the user currenrtly attempting to authenticate.
        /// </summary>
        private string Username { get; }

        #endregion

        #region Constructors

        static PtcLogin()
        {
            Cookies = new CookieContainer();
            HttpClient = new HttpClient(
                new HttpClientHandler
                {
                    CookieContainer = Cookies,
                    AutomaticDecompression = DecompressionMethods.GZip,
                    AllowAutoRedirect = false
                }
            );
            HttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(Constants.LoginUserAgent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public PtcLogin(string username, string password)
        {
            Username = username;
            Password = password;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<AccessToken> GetAccessToken()
        {
            AccessToken accessToken;
            PtcLoginParameters loginData = null;
            var cookies = Cookies.GetCookies("sso.pokemon.com").ToList();
            // @robertmclaws: "CASTGC" is the name of the login cookie that the service looks for, afaik.
            //                The second one is listed as a backup in case they change the cookie name.
            if (!cookies.Any(c => c.Name == "CASTGC") || cookies.Count == 0)
            {
                loginData = await GetLoginParameters().ConfigureAwait(false);
            }
            var authTicket = await GetAuthenticationTicket(loginData).ConfigureAwait(false);
            accessToken = await GetOAuthToken(authTicket).ConfigureAwait(false);

            return accessToken;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Responsible for retrieving login parameters for <see cref="GetAuthenticationTicket" />.
        /// </summary>
        /// <param name="httpClient">An initialized <see cref="HttpClient" /></param>
        /// <returns><see cref="PtcLoginParameters" /> for <see cref="GetAuthenticationTicket" />.</returns>
        private async Task<PtcLoginParameters> GetLoginParameters()
        {
            var response = await HttpClient.GetAsync(Constants.LoginUrl);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var loginData = JsonConvert.DeserializeObject<PtcLoginParameters>(responseContent);
            return loginData;
        }

        /// <summary>
        /// Authenticates against the PTC login service and acquires an Authentication Ticket.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance to use for this request.</param>
        /// <param name="loginData">The <see cref="PtcLoginParameters" /> to use from this request. Obtained by calling <see cref="GetLoginParameters(HttpClient)"/>.</param>
        /// <returns></returns>
        private async Task<string> GetAuthenticationTicket(PtcLoginParameters loginData)
        {
            HttpResponseMessage responseMessage;

            if (loginData != null)
            {
                var requestData = new Dictionary<string, string>
                {
                    {"lt", loginData.Lt},
                    {"execution", loginData.Execution},
                    {"_eventId", "submit"},
                    {"username", Username},
                    {"password", Password}
                };
                responseMessage = await HttpClient.PostAsync(Constants.LoginUrl, new FormUrlEncodedContent(requestData)).ConfigureAwait(false);
            }
            else
            {
                responseMessage = await HttpClient.GetAsync(Constants.LoginUrl);
            }

            // robertmclaws: No need to even read the string if we have results from the location query.
            if (responseMessage.StatusCode == HttpStatusCode.Found && responseMessage.Headers.Location != null)
            {
                var decoder = new WwwFormUrlDecoder(responseMessage.Headers.Location.Query);
                if (decoder.Count == 0)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    throw new LoginFailedException();
                }
                return decoder.GetFirstValueByName("ticket");
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            PtcAuthenticationTicketResponse response = null;

            // @robertmclaws: Let's try to catch situations we haven't thought of yet.
            try
            {
                response = JsonConvert.DeserializeObject<PtcAuthenticationTicketResponse>(responseContent);
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
                throw new LoginFailedException("We encountered a response from the PTC login servers thet we didn't anticipate. Please take a screenshot and open a ticket."
                    + Environment.NewLine + responseContent.Replace("/n", ""));
            }

            if (!string.IsNullOrWhiteSpace(response.ErrorCode) && response.ErrorCode.EndsWith("activation_required"))
            {
                throw new LoginFailedException($"Your two-day grace period has expired, and your PTC account must now be activated." + Environment.NewLine + $"Please visit {response.Redirect}.");
            }

            var loginFailedWords = new string[] { "incorrect", "disabled" };

            var loginFailed = loginFailedWords.Any(failedWord => response.Errors.Any(error => error.Contains(failedWord)));
            if (loginFailed)
            {
                throw new LoginFailedException(response.Errors[0]);
            }
            throw new Exception($"Pokemon Trainer Club responded with the following error(s): '{string.Join(", ", response.Errors)}'");
        }

        /// <summary>
        /// Retrieves an OAuth 2.0 token for a given Authentication ticket.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> instance to use for this request.</param>
        /// <param name="authTicket">The Authentication Ticket to use for this request. Obtained by calling <see cref="GetAuthenticationTicket(HttpClient, PtcLoginParameters)"/>.</param>
        /// <returns></returns>
        private async Task<AccessToken> GetOAuthToken(string authTicket)
        {
            var requestData = new Dictionary<string, string>
                {
                    {"client_id", "mobile-app_pokemon-go"},
                    {"redirect_uri", "https://www.nianticlabs.com/pokemongo/error"},
                    {"client_secret", "w8ScCUXJQc6kXKw8FiOhd8Fixzht18Dq3PEVkUCP5ZPxtgyWsbTvWHFLm2wNY0JR"},
                    {"grant_type", "refresh_token"},
                    {"code", authTicket}
                };

            var responseMessage = await HttpClient.PostAsync(Constants.LoginOauthUrl, new FormUrlEncodedContent(requestData)).ConfigureAwait(false);
            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                throw new Exception("Your login was OK, but we could not get an API Token.");
            }

            var decoder = new WwwFormUrlDecoder(responseContent);
            if (decoder.Count == 0)
            {
                throw new Exception("Your login was OK, but we could not get an API Token.");
            }

            return new AccessToken
            {
                Username = this.Username,
                Token = decoder.GetFirstValueByName("access_token"),
                // @robertmclaws: Subtract 1 hour from the token to solve this issue: https://github.com/pogodevorg/pgoapi/issues/86
                ExpiresUtc = DateTime.UtcNow.AddSeconds(int.Parse(decoder.GetFirstValueByName("expires")) - 3600),
                AuthType = AuthType.Ptc
            };
        }

        #endregion

    }

}