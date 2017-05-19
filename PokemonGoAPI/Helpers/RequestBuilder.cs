using System;
using System.Linq;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;
using System.Collections.Generic;
using POGOLib.Official.Util.Hash;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;
using Google.Protobuf.Collections;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private readonly double _accuracy;
        private readonly AuthTicket _authTicket;
        private readonly string _authToken;
        private readonly AuthType _authType;
        private readonly IDeviceInfo _deviceInfo;
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly Random _random = new Random();
        private uint _requestId = 0;
        private static byte[] _sessionHash = null;

        public RequestBuilder(string authToken, AuthType authType, double latitude, double longitude, double accuracy,
            IDeviceInfo deviceInfo,
            AuthTicket authTicket = null)
        {
            _authToken = authToken;
            _authType = authType;
            _latitude = latitude;
            _longitude = longitude;
            _accuracy = accuracy;
            _authTicket = authTicket;
            _deviceInfo = deviceInfo;
        }

        public RequestEnvelope SetRequestEnvelopeUnknown6(RequestEnvelope requestEnvelope)
        {
            if (_sessionHash == null)
            {
                //_sessionHash = new byte[32];
                _sessionHash = new byte[16];
                _random.NextBytes(_sessionHash);
            }

            byte[] authSeed = requestEnvelope.AuthTicket != null ?
                requestEnvelope.AuthTicket.ToByteArray() :
                requestEnvelope.AuthInfo.ToByteArray();


            var normAccel = new Vector(_deviceInfo.Sensors.AccelRawX, _deviceInfo.Sensors.AccelRawY, _deviceInfo.Sensors.AccelRawZ);
            normAccel.NormalizeVector(1.0f);//1.0f on iOS, 9.81 on Android?

            // Hashing code here
            bool UseHashServer = true;
            ulong LocHash1 = 0;
            ulong LocHash2 = 0;
            ByteString SessHash = null;
            long Unk25 = 0;
            ulong Timestmp = (ulong)DateTime.UtcNow.ToUnixTime();
            List<ulong> ReqHash = new List<ulong>();
            ByteString EncSig = null;

            if (UseHashServer)
            {
                var sign = new Signature
                {
                    Timestamp = Timestmp,
                    SessionHash = ByteString.CopyFrom(_sessionHash)
                };
                var locationBytes = new List<byte>();
                locationBytes.AddRange(BitConverter.GetBytes(requestEnvelope.Latitude).Reverse());
                locationBytes.AddRange(BitConverter.GetBytes(requestEnvelope.Longitude).Reverse());
                locationBytes.AddRange(BitConverter.GetBytes(requestEnvelope.Accuracy).Reverse());

                var requestBytes = new List<byte[]>();
                foreach (var req in requestEnvelope.Requests)
                {
                    requestBytes.Add(req.ToByteArray());
                }

                var task = Task.Run(async () => await Client.Hasher.GetHashDataAsync(requestEnvelope, sign, locationBytes.ToArray(), requestBytes.ToArray(), authSeed).ConfigureAwait(false));
                var hashContent = task.WaitAndUnwrapException();

                LocHash1 = hashContent.LocationAuthHash;
                LocHash2 = hashContent.LocationHash;
                SessHash = ByteString.CopyFrom(_sessionHash);
                Unk25 = Client.Hasher.Unknown25;
                foreach (var req in hashContent.RequestHashes)
                {
                    ReqHash.Add(req);
                }

            }
            else
            {
                LocHash1 = Utils.GenerateLocation1(authSeed, requestEnvelope.Latitude, requestEnvelope.Longitude,
                            requestEnvelope.Accuracy, _deviceInfo.VersionData.HashSeed1);
                LocHash2 = Utils.GenerateLocation2(requestEnvelope.Latitude, requestEnvelope.Longitude,
                            requestEnvelope.Accuracy, _deviceInfo.VersionData.HashSeed1);
                SessHash = ByteString.CopyFrom(_sessionHash);
                Unk25 = _deviceInfo.VersionData.VersionHash;
                foreach (var request in requestEnvelope.Requests)
                {
                    ReqHash.Add(Utils.GenerateRequestHash(authSeed, request.ToByteArray(), _deviceInfo.VersionData.HashSeed1));
                }
            }

            var sig = new Signature
            {
                LocationHash1 = LocHash1,
                LocationHash2 = LocHash2,
                SessionHash = SessHash,
                Unknown25 = Unk25,
                Timestamp = Timestmp,
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

            if (_deviceInfo.GpsSattelitesInfo.Length > 0)
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
                Altitude = loc.Altitude,
                LocationType = loc.LocationType,
                Provider = loc.Provider,
                ProviderStatus = loc.ProviderStatus,
                HorizontalAccuracy = loc.HorizontalAccuracy,
                VerticalAccuracy = loc.VerticalAccuracy,
                Course = loc.Course,
                Speed = loc.Speed,
                TimestampSnapshot = loc.TimeSnapshot

            }));

            foreach (var request in requestEnvelope.Requests)
            {
                sig.RequestHash.Add(ReqHash);
            }

            // Encryption code here
            if (UseHashServer)
            {
                EncSig = ByteString.CopyFrom(Client.Hasher.GetEncryptedSignature(sig.ToByteArray(), (uint)_deviceInfo.TimeSnapshot));
            }
            else
            {
                EncSig = ByteString.CopyFrom(PCrypt.encrypt(sig.ToByteArray(), (uint)_deviceInfo.TimeSnapshot));
            }

            requestEnvelope.PlatformRequests.Add(new PlatformRequest
            {
                Type = POGOProtos.Networking.Platform.PlatformRequestType.SendEncryptedSignature,
                RequestMessage = EncSig
            });

            foreach (Request request in requestEnvelope.Requests)
            {
                RequestType requestType = request.RequestType;
                if (requestType == RequestType.GetMapObjects || requestType == RequestType.GetPlayer)
                {
                    requestEnvelope.PlatformRequests.Add(new PlatformRequest
                    {
                        Type = POGOProtos.Networking.Platform.PlatformRequestType.UnknownPtr8,
                        RequestMessage = ByteString.Empty
                    });
                    break;
                }
            }

            return requestEnvelope;
        }

        private ulong GetNextRequestId()
        {
            int rand = 0;
            if (_requestId == 0)
            {
                rand = 0x000041A7; // Initial value for Apple
                _requestId++;
            }
            else
            {
                rand = _random.Next(0, Int32.MaxValue);
            }

            _requestId++;
            return (((uint)rand | ((_requestId & 0xFFFFFFFF) >> 31)) << 32) | _requestId;
        }


        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            return SetRequestEnvelopeUnknown6(new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = GetNextRequestId(), //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Accuracy = (int)_accuracy, //9
                AuthTicket = _authTicket, //11
                MsSinceLastLocationfix = _random.Next(500, 1000) //12
            });
        }

        public RequestEnvelope GetInitialRequestEnvelope(params Request[] customRequests)
        {
            return SetRequestEnvelopeUnknown6(new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = GetNextRequestId(), //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _latitude, //7
                Longitude = _longitude, //8
                Accuracy = (int)_accuracy, //9
                AuthInfo = new AuthInfo
                {
                    Provider = _authType == AuthType.Google ? "google" : "ptc",
                    Token = new AuthInfo.Types.JWT
                    {
                        Contents = _authToken,
                        Unknown2 = 14
                    }
                }, //10
                MsSinceLastLocationfix = _random.Next(500, 1000) //12
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