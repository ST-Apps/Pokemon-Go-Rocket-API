using System;
using System.Text;
using Google.Protobuf;
using POGOProtos.Networking.Signature;
using PokemonGo.RocketAPI.Enums;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using PokemonGo.RocketAPI.Extensions;
using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private readonly string _authToken;
        private readonly AuthType _authType;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly double _altitude;
        private readonly AuthTicket _authTicket;
        private readonly DateTime _startTime = DateTime.UtcNow;

        public RequestBuilder(string authToken, AuthType authType, double latitude, double longitude, double altitude,
            AuthTicket authTicket = null)
        {
            _authToken = authToken;
            _authType = authType;
            _latitude = latitude;
            _longitude = longitude;
            _altitude = altitude;
            _authTicket = authTicket;
        }

        public RequestEnvelope SetRequestEnvelopeUnknown6(RequestEnvelope requestEnvelope)
        {
            var rnd32 = new byte[32];
            var rnd = new Random();
            rnd.NextBytes(rnd32);

            var ticketBytes = requestEnvelope.AuthTicket.ToByteArray();

            var sig = new Signature()
            {
                LocationHash1 =
                    Utils.GenerateLocation1(ticketBytes, requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                LocationHash2 =
                    Utils.GenerateLocation2(requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                Unk22 = ByteString.CopyFrom(rnd32),
                Timestamp = (ulong) DateTime.UtcNow.ToUnixTime(),
                TimestampSinceStart = (ulong)(DateTime.UtcNow.ToUnixTime() - _startTime.ToUnixTime())
            };

            foreach (var request in requestEnvelope.Requests)
            {
                sig.RequestHash.Add(
                    Utils.GenerateRequestHash(ticketBytes, request.ToByteArray())
                );
            }

            requestEnvelope.Unknown6.Add(new Unknown6()
            {
                RequestType = 6,
                Unknown2 = new Unknown6.Types.Unknown2()
                {
                    Unknown1 = ByteString.CopyFrom(Crypt.Encrypt(sig.ToByteArray()))
                }
            });

            return requestEnvelope;
        }

        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            return SetRequestEnvelopeUnknown6(new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = 1469378659230941192, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Altitude = _altitude, //9
                AuthTicket = _authTicket, //11
                Unknown12 = 989 //12
            });
        }

        public RequestEnvelope GetInitialRequestEnvelope(params Request[] customRequests)
        {
            return new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = 1469378659230941192, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Altitude = _altitude, //9
                AuthInfo = new AuthInfo
                {
                    Provider = _authType == AuthType.Google ? "google" : "ptc",
                    Token = new AuthInfo.Types.JWT
                    {
                        Contents = _authToken,
                        Unknown2 = 14
                    }
                }, //10
                Unknown12 = 989 //12
            };
        }

        public RequestEnvelope GetRequestEnvelope(RequestType type, IMessage message)
        {
            return GetRequestEnvelope(new Request()
            {
                RequestType = type,
                RequestMessage = message.ToByteString()
            });

        }
    }
}