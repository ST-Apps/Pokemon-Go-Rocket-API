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
        private static byte[] _sessionHash = null;

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


            var normAccel = new Vector(_deviceInfo.Sensors.AccelRawX, _deviceInfo.Sensors.AccelRawY, _deviceInfo.Sensors.AccelRawZ);
            normAccel.NormalizeVector(1.0f);//1.0f on iOS, 9.81 on Android?

            var sig = new Signature
            {
                LocationHash1 =
                    Utils.GenerateLocation1(authSeed, requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                LocationHash2 =
                    Utils.GenerateLocation2(requestEnvelope.Latitude, requestEnvelope.Longitude,
                        requestEnvelope.Altitude),
                SessionHash = ByteString.CopyFrom(_sessionHash),
                Unknown25 = 7363665268261373700L,
                Timestamp = (ulong)DateTime.UtcNow.ToUnixTime(),
                TimestampSinceStart = (ulong)_deviceInfo.TimeSnapshot,
                SensorInfo = new Signature.Types.SensorInfo
                {
                    AccelNormalizedX = normAccel.X,
                    AccelNormalizedY = normAccel.Y,
                    AccelNormalizedZ = normAccel.Z,
                    AccelRawX = -_deviceInfo.Sensors.AccelRawX,
                    AccelRawY = -_deviceInfo.Sensors.AccelRawY,
                    AccelRawZ = -_deviceInfo.Sensors.AccelRawZ,
                    MagnetometerX = _deviceInfo.Sensors.MagnetometerX,
                    MagnetometerY = _deviceInfo.Sensors.MagnetometerY,
                    MagnetometerZ = _deviceInfo.Sensors.MagnetometerZ,
                    GyroscopeRawX = _deviceInfo.Sensors.GyroscopeRawX,
                    GyroscopeRawY = _deviceInfo.Sensors.GyroscopeRawY,
                    GyroscopeRawZ = _deviceInfo.Sensors.GyroscopeRawZ,
                    AngleNormalizedX = _deviceInfo.Sensors.AngleNormalizedX,
                    AngleNormalizedY = _deviceInfo.Sensors.AngleNormalizedY,
                    AngleNormalizedZ = _deviceInfo.Sensors.AngleNormalizedZ,
                    AccelerometerAxes = _deviceInfo.Sensors.AccelerometerAxes,
                    TimestampSnapshot = (ulong)(_deviceInfo.Sensors.TimeSnapshot - _random.Next(150, 260))
                },
                DeviceInfo = new Signature.Types.DeviceInfo
                {
                    DeviceId = _deviceInfo.DeviceID,
                    AndroidBoardName = _deviceInfo.AndroidBoardName,
                    AndroidBootloader = _deviceInfo.AndroidBootloader,
                    DeviceBrand = _deviceInfo.DeviceBrand,
                    DeviceModel = _deviceInfo.DeviceModel,
                    DeviceModelBoot = _deviceInfo.DeviceModelBoot,
                    DeviceModelIdentifier = _deviceInfo.DeviceModelIdentifier,
                    FirmwareFingerprint = _deviceInfo.FirmwareFingerprint,
                    FirmwareTags = _deviceInfo.FirmwareTags,
                    HardwareManufacturer = _deviceInfo.HardwareManufacturer,
                    HardwareModel = _deviceInfo.HardwareModel,
                    FirmwareBrand = _deviceInfo.FirmwareBrand,
                    FirmwareType = _deviceInfo.FirmwareType
                },

                ActivityStatus = _deviceInfo.ActivityStatus != null ? new Signature.Types.ActivityStatus()
                {
                    Walking = _deviceInfo.ActivityStatus.Walking,
                    Automotive = _deviceInfo.ActivityStatus.Automotive,
                    Cycling = _deviceInfo.ActivityStatus.Cycling,
                    Running = _deviceInfo.ActivityStatus.Running,
                    Stationary = _deviceInfo.ActivityStatus.Stationary,
                    Tilting = _deviceInfo.ActivityStatus.Tilting,
                }
                : null
            };


            if(_deviceInfo.GpsSattelitesInfo.Length > 0)
            {
                sig.GpsInfo = new Signature.Types.AndroidGpsInfo();
                //sig.GpsInfo.TimeToFix //currently not filled

                _deviceInfo.GpsSattelitesInfo.ToList().ForEach(sat =>
                {
                    sig.GpsInfo.Azimuth.Add(sat.Azimuth);
                    sig.GpsInfo.Elevation.Add(sat.Elevation);
                    sig.GpsInfo.HasAlmanac.Add(sat.Almanac);
                    sig.GpsInfo.HasEphemeris.Add(sat.Emphasis);
                    sig.GpsInfo.SatellitesPrn.Add(sat.SattelitesPrn);
                    sig.GpsInfo.Snr.Add(sat.Snr);
                    sig.GpsInfo.UsedInFix.Add(sat.UsedInFix);
                });
            }

            _deviceInfo.LocationFixes.ToList().ForEach(loc => sig.LocationFix.Add(new Signature.Types.LocationFix
            {
                Floor = loc.Floor,
                Longitude = loc.Longitude,
                Latitude = loc.Latitude,
                //Altitude = loc.Altitude, //currently probably not filled
                LocationType = loc.LocationType,
                Provider = loc.Provider,
                ProviderStatus = loc.ProviderStatus,
                HorizontalAccuracy = loc.HorizontalAccuracy,
                VerticalAccuracy = loc.VerticalAccuracy,
                Unknown20 = loc.Unknown20,
                TimestampSnapshot = loc.TimeSnapshot

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