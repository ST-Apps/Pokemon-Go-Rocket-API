using System;
using System.Linq;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private readonly double _altitude;
        private readonly AuthTicket _authTicket;
        private readonly string _authToken;
        private readonly AuthType _authType;
        private readonly IDeviceInfo _deviceInfo;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly Random _random = new Random();
        private byte[] _sessionHash = null;

        public RequestBuilder(string authToken, AuthType authType, double latitude, double longitude, double altitude,
            IDeviceInfo deviceInfo,
            AuthTicket authTicket = null)
        {
            _authToken = authToken;
            _authType = authType;
            _latitude = latitude;
            _longitude = longitude;
            _altitude = altitude;
            _authTicket = authTicket;
            _deviceInfo = deviceInfo;
        }

        public RequestEnvelope SetRequestEnvelopeUnknown6(RequestEnvelope requestEnvelope)
        {
            if(_sessionHash == null)
            {
                _sessionHash = new byte[32];
                _random.NextBytes(_sessionHash);
            }

            byte[] authSeed = requestEnvelope.AuthTicket != null ?
                requestEnvelope.AuthTicket.ToByteArray() :
                requestEnvelope.AuthInfo.ToByteArray();


            var normAccel = new Vector(_deviceInfo.AccelRawX, _deviceInfo.AccelRawY, _deviceInfo.AccelRawZ);
            normAccel.NormalizeVector(9.81);
            normAccel.Round(2);

            ulong timeFromStart = (ulong)_deviceInfo.TimeSnapshot.TotalMilliseconds;

            var sig = new Signature
            {
                LocationHash1 =
                    Utils.GenerateLocation1(authSeed, requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                LocationHash2 =
                    Utils.GenerateLocation2(requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                SessionHash = ByteString.CopyFrom(_sessionHash),
                Unknown25 = -8537042734809897855L,
                Timestamp = (ulong) DateTime.UtcNow.ToUnixTime(),
                TimestampSinceStart = timeFromStart,
                SensorInfo = new Signature.Types.SensorInfo
                {
                    AccelNormalizedX = normAccel.X,
                    AccelNormalizedY = normAccel.Y,
                    AccelNormalizedZ = normAccel.Z,
                    AccelRawX = -_deviceInfo.AccelRawX,
                    AccelRawY = -_deviceInfo.AccelRawY,
                    AccelRawZ = -_deviceInfo.AccelRawZ,
                    MagnetometerX = _deviceInfo.MagnetometerX,
                    MagnetometerY = _deviceInfo.MagnetometerY,
                    MagnetometerZ = _deviceInfo.MagnetometerZ,
                    GyroscopeRawX = _deviceInfo.GyroscopeRawX,
                    GyroscopeRawY = _deviceInfo.GyroscopeRawY,
                    GyroscopeRawZ = _deviceInfo.GyroscopeRawZ,
                    AngleNormalizedX = _deviceInfo.AngleNormalizedX,
                    AngleNormalizedY = _deviceInfo.AngleNormalizedY,
                    AngleNormalizedZ = _deviceInfo.AngleNormalizedZ,
                    AccelerometerAxes = _deviceInfo.AccelerometerAxes,
                    TimestampSnapshot = timeFromStart - (ulong) _random.Next(150, 260)
                },
                DeviceInfo = new Signature.Types.DeviceInfo
                {
                    DeviceId = _deviceInfo.DeviceID,
                    FirmwareBrand = _deviceInfo.FirmwareBrand,
                    FirmwareType = _deviceInfo.FirmwareType
                }

                /*ActivityStatus = new Signature.Types.ActivityStatus()
                {
                    StartTimeMs = timeFromStart - (ulong)_random.Next(150, 350),
                    Walking = false,
                    Automotive = false,
                    Cycling = false,
                    Running = false,
                    Stationary = true,
                    Tilting = false,
                    UnknownStatus = false,
                    Status = ByteString.Empty //Have no idea what is there
                }*/
            };

            _deviceInfo.LocationFixes.ToList().ForEach(loc => sig.LocationFix.Add(new Signature.Types.LocationFix
            {
                Floor = loc.Floor,
                Longitude = loc.Longitude,
                Latitude = loc.Latitude,
                Altitude = loc.Altitude,
                LocationType = loc.LocationType,
                Provider = loc.Provider,
                ProviderStatus = loc.ProviderStatus,
                HorizontalAccuracy = loc.HorizontalAccuracy,
                VerticalAccuracy = loc.VerticalAccuracy,
                TimestampSnapshot = loc.Timestamp

            }));

            foreach (var request in requestEnvelope.Requests)
            {
                sig.RequestHash.Add(
                    Utils.GenerateRequestHash(authSeed, request.ToByteArray())
                    );
            }

            requestEnvelope.Unknown6 = new Unknown6
            {
                RequestType = 6,
                Unknown2 = new Unknown6.Types.Unknown2
                {
                     EncryptedSignature = ByteString.CopyFrom(Crypt.Encrypt(sig.ToByteArray()))
                }
            };

            return requestEnvelope;
        }

        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            return SetRequestEnvelopeUnknown6(new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = 1469378659230941192, //3
                Requests = {customRequests}, //4

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
            return SetRequestEnvelopeUnknown6(new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = 1469378659230941192, //3
                Requests = {customRequests}, //4

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
            });
        }

        public RequestEnvelope GetRequestEnvelope(RequestType type, IMessage message)
        {
            return GetRequestEnvelope(new Request
            {
                RequestType = type,
                RequestMessage = message.ToByteString()
            });
        }
    }
}