using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Rpc;
using PokemonGoAPI.Session;
using POGOProtos.Networking.Envelopes;
using PokemonGoAPI.Interfaces;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGoAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using System.Diagnostics;
using System;
using Google.Protobuf.Collections;
using POGOLib.Official.Util.Hash;

namespace PokemonGo.RocketAPI
{
    public partial class Client : HttpClient
    {
        private static readonly HttpClientHandler Handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = false
        };

        public IDeviceInfo DeviceInfo;
        public Download Download;
        public Encounter Encounter;
        public Fort Fort;
        public Inventory Inventory;
        public Rpc.Login Login;
        public Map Map;
        public Misc Misc;
        public Player Player;

        public IApiFailureStrategy ApiFailure { get; set; }
        public ISettings Settings { get; }
        public string AuthToken => AccessToken?.Token;

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAccuracy { get; internal set; }

        public AuthType AuthType => Settings.AuthType;
        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket => AccessToken?.AuthTicket;
        public AccessToken AccessToken { get; set; }

        public static IHasher Hasher = new PokeHashHasher(string.Empty);



        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public Client() : base(new RetryHandler(Handler))
        {
            DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            DefaultRequestHeaders.ExpectContinue = false;
            DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="apiFailureStrategy"></param>
        /// <param name="deviceInfo"></param>
        /// <param name="accessToken"></param>
        public Client(string PokehashAuthKey, ISettings settings, IApiFailureStrategy apiFailureStrategy, IDeviceInfo deviceInfo, AccessToken accessToken = null) : this()
        {
            Hasher = new PokeHashHasher(PokehashAuthKey);

            Settings = settings;
            ApiFailure = apiFailureStrategy;
            AccessToken = accessToken;

            Login = new Rpc.Login(this);
            Player = new Player(this);
            Download = new Download(this);
            Inventory = new Inventory(this);
            Map = new Map(this);
            Fort = new Fort(this);
            Encounter = new Encounter(this);
            Misc = new Misc(this);
            DeviceInfo = deviceInfo;

            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAccuracy);
        }

        #endregion

        #region HttpClient stuff

        public async Task<IMessage[]> PostProtoPayload<TRequest>(string url, RequestEnvelope requestEnvelope, IApiFailureStrategy strategy, 
            params Type[] responseTypes)
            where TRequest : IMessage<TRequest>
        {
            var result = new IMessage[responseTypes.Length];
            for (var i = 0; i < responseTypes.Length; i++)
            {
                result[i] = Activator.CreateInstance(responseTypes[i]) as IMessage;
                if (result[i] == null)
                {
                    throw new ArgumentException($"ResponseType {i} is not an IMessage");
                }
            }
            var urlArray = new[] { url };
            ResponseEnvelope response;

            while ((response = await PostProto<TRequest>(urlArray[0], requestEnvelope)).Returns.Count !=
                   responseTypes.Length)
            {
                var operation = await strategy.HandleApiFailure(urlArray, requestEnvelope, response);
                if (operation == ApiOperation.Abort)
                {
                    throw new InvalidResponseException(
                        $"Expected {responseTypes.Length} responses, but got {response.Returns.Count} responses");
                }
            }

            strategy.HandleApiSuccess(requestEnvelope, response);

            for (var i = 0; i < responseTypes.Length; i++)
            {
                var payload = response.Returns[i];
                result[i].MergeFrom(payload);
            }
            return result;
        }

        public async Task<TResponsePayload> PostProtoPayload<TRequest, TResponsePayload>(string url, RequestEnvelope requestEnvelope, IApiFailureStrategy strategy)
            where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            Debug.WriteLine($"Requesting {typeof(TResponsePayload).Name}");
            var urlArray = new[] { url };

            var response = await PostProto<TRequest>(url, requestEnvelope);

            while (response.Returns.Count == 0)
            {
                var operation = await strategy.HandleApiFailure(urlArray, requestEnvelope, response);
                if (operation == ApiOperation.Abort)
                {
                    break;
                }

                response = await PostProto<TRequest>(urlArray[0], requestEnvelope);
            }

            // TODO: statuscode = 3 probably means ban!
            if (response.Returns.Count == 0)
                throw new InvalidResponseException();

            strategy.HandleApiSuccess(requestEnvelope, response);

            //Decode payload
            //todo: multi-payload support
            var payload = response.Returns[0];
            var parsedPayload = new TResponsePayload();
            parsedPayload.MergeFrom(payload);

            return parsedPayload;
        }

        public async Task<ResponseEnvelope> PostProto<TRequest>(string url, RequestEnvelope requestEnvelope) 
            where TRequest : IMessage<TRequest>
        {
            // robertmclaws: Let's be pro-active about token failures, instead of reactive.
            if (AccessToken == null || AccessToken.IsExpired)
            {
                await Login.DoLogin();
            }

            //Encode payload and put in envelop, then send
            var data = requestEnvelope.ToByteString();
            var result = await PostAsync(url, new ByteArrayContent(data.ToByteArray())).ConfigureAwait(false);

            //Decode message
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new ResponseEnvelope();
            decodedResponse.MergeFrom(codedStream);

            return decodedResponse;
        }

        #endregion

        #region API 2.0 bridge

        // For smoother migration to 2.0


        // Conversion from obsolete Tuple
        internal bool ProcessMessages<T1, T2, T3, T4, T5, T6>(Tuple<T1, T2, T3, T4, T5, T6> tuple)
            where T1 : class, IMessage, new()
            where T2 : class, IMessage, new()
            where T3 : class, IMessage, new()
            where T4 : class, IMessage, new()
            where T5 : class, IMessage, new()
            where T6 : class, IMessage, new()
        {
            IMessage[] msgs = new IMessage[] { tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6 };
            return ProcessMessages(msgs);
        }

        // Conversion from obsolete Tuple
        internal bool ProcessMessages<T1, T2>(Tuple<T1, T2> tuple)
            where T1 : class, IMessage, new()
            where T2 : class, IMessage, new()
        {
            IMessage[] msgs = new IMessage[] { tuple.Item1, tuple.Item2};
            return ProcessMessages(msgs);
        }

        internal bool ProcessMessages(RepeatedField<ByteString> responses, params Type[] responseTypes)
        {
            IMessage[] msgs = new IMessage[responses.Count];

            for (var i = 0; i < responseTypes.Length; i++)
            {
                msgs[i] = Activator.CreateInstance(responseTypes[i]) as IMessage;

                var payload = responses[i];
                msgs[i].MergeFrom(payload);
            }

            return ProcessMessages(msgs);
        }

    #endregion

}

}