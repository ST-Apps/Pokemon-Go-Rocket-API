using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace PokemonGo.RocketAPI.Helpers
{
    internal class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 25;

        private readonly object _locker = new object();

        private bool _isNetworkAvailable;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
            UpdateConnectionStatus();
        }

        private void UpdateConnectionStatus()
        {
            lock (_locker)
            {
                var connectionProfile = NetworkInformation.GetInternetConnectionProfile();                
                _isNetworkAvailable = connectionProfile != null &&
                                      connectionProfile.GetNetworkConnectivityLevel() ==
                                      NetworkConnectivityLevel.InternetAccess;
                Monitor.PulseAll(_locker);
            }
        }

        private void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            UpdateConnectionStatus();                        
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {                      

            for (var i = 0; i <= MaxRetries; i++)
            {
                try
                {

                    lock (_locker)
                    {
                        while (!_isNetworkAvailable)
                        {
                            Logger.Write($"{request.RequestUri} is waiting for Network to be available again.");
                            Monitor.Wait(_locker);
                        }
                    }

                    var response = await base.SendAsync(request, cancellationToken);
                    if (response.StatusCode == HttpStatusCode.BadGateway ||
                        response.StatusCode == HttpStatusCode.InternalServerError)
                        throw new Exception(); //todo: proper implementation

                    return response;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[#{i} of {MaxRetries}] retry request {request.RequestUri} - Error: {ex}");
                    if (i < MaxRetries)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    throw;
                }
            }
            return null;
        }
    }
}