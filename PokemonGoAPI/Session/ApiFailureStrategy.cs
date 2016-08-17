using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;

namespace PokemonGoAPI.Session
{
    public class ApiFailureStrategy : IApiFailureStrategy
    {

        private Client _client;

        private const int MaxRetries = 50;

        private int _retryCount;

        public event EventHandler OnAccessTokenUpdated;
        
        public ApiFailureStrategy(Client client)
        {
            _client = client;
        }

        #region Reauth

        private Mutex ReauthenticateMutex { get; } = new Mutex();

        /// <summary>
        ///     Ensures the <see cref="Session" /> gets reauthenticated, no matter how long it takes.
        /// </summary>
        private async Task Reauthenticate()
        {
            ReauthenticateMutex.WaitOne();
            if (_client.AccessToken.IsExpired)
            {
                _client.AccessToken = null;
                var tries = 0;
                while (_client.AccessToken == null)
                {
                    try
                    {                        
                        await _client.Login.DoLogin();
                    }
                    catch (Exception exception)
                    {
                        Logger.Write($"Reauthenticate exception was catched: {exception}");
                    }
                    finally
                    {
                        if (_client.AccessToken == null)
                        {
                            var sleepSeconds = Math.Min(60, ++tries * 5);
                            Logger.Write($"Reauthentication failed, trying again in {sleepSeconds} seconds.");
                            await Task.Delay(sleepSeconds * 1000);
                        }
                    }
                }                
                OnAccessTokenUpdated?.Invoke(this, null);
            }
            ReauthenticateMutex.ReleaseMutex();
        }

        #endregion

        #region Implementation of IApiFailureStrategy

        public async Task<ApiOperation> HandleApiFailure(string[] url, RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == MaxRetries)
                // We failed, let's abort
                return ApiOperation.Abort;

            switch (response.StatusCode)
            {
                case 1:
                    // Success!?
                    break;
                case 52:
                    // Slow servers?
                    Logger.Write("Server may be slow, let's wait a little bit");
                    await Task.Delay(2000);
                    break;
                case 53:
                    // New RPC url
                    if (!Regex.IsMatch(response.ApiUrl, "pgorelease\\.nianticlabs\\.com\\/plfe\\/\\d+"))
                    {
                        throw new Exception(
                            $"Received an incorrect API url: '{response.ApiUrl}', status code was: '{response.StatusCode}'.");
                    }
                    _client.ApiUrl = $"https://{response.ApiUrl}/rpc";
                    url[0] = _client.ApiUrl;
                    Logger.Write($"Received an updated API url = {_client.ApiUrl}");
                    break;
                case 102:
                    // Invalid auth
                    Logger.Write("Received StatusCode 102, reauthenticating.");
                    _client.AccessToken.Expire();
                    await Reauthenticate();
                    request.AuthTicket = _client.AuthTicket;
                    break;
                default:
                    Logger.Write($"Unknown status code: {response.StatusCode}");
                    break;
            }
            _retryCount++;
            return ApiOperation.Retry;
        }

        /// <summary>
        /// Request worked, reset retry count and check if we have an updated token
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
            // Check if we got an updated ticket
            if (response.AuthTicket == null) return;
            // Update the new token               
            _client.AccessToken.AuthTicket = response.AuthTicket;
            OnAccessTokenUpdated?.Invoke(this, null);
            Logger.Write("Received a new AuthTicket from Pokémon!");
        }

        #endregion
    }
}
