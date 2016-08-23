using PokemonGo.RocketAPI.Helpers;
using Superbest_random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    /// Device infos used to sign requests
    /// </summary>
    public class DeviceInfosAndroid : IDeviceInfoExtended
    {

        public DeviceInfosAndroid()
        {
        }


        #region Device

        public string DeviceID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.AndroidDeviceID))
                {
                    SettingsService.Instance.AndroidDeviceID = Utilities.RandomHex(16);
                }
                return SettingsService.Instance.AndroidDeviceID;
            }
        }
        public string AndroidBoardName => "shamu";
        public string AndroidBootloader => "moto-apq8084-71.15";
        public string DeviceBrand => "google";
        public string DeviceModel => "shamu";
        public string DeviceModelIdentifier => "MRA58R";
        public string DeviceModelBoot => "shamu";
        public string HardwareManufacturer => "motorola";
        public string HardwareModel => "Nexus 6";
        public string FirmwareTags => "release-keys";
        public string FirmwareFingerprint => "google/shamu/shamu:6.0/MRA58R/2308909:user/release-keys";

        public string FirmwareBrand => "shamu";

        public string FirmwareType => "user";

        public long TimeSnapshot => DeviceInfos.RelativeTimeFromStart;

        #endregion

        #region LocationFixes
        private List<ILocationFix> _locationFixes = new List<ILocationFix>();
        private object _gpsLocationFixesLock = new object();
        public ILocationFix[] LocationFixes
        {
            get
            {
                lock (_gpsLocationFixesLock)
                {
                    //atomically exchange lists (new is empty)
                    List<ILocationFix> data = _locationFixes;
                    _locationFixes = new List<ILocationFix>();
                    return data.ToArray();
                }
            }
        }

        public void CollectLocationData()
        {
            ILocationFix loc = LocationFixAndroid.CollectData();

            if (loc != null)
            {
                lock (_gpsLocationFixesLock)
                {
                    _locationFixes.Add(loc);
                }
            }
        }


        #endregion




        public string Platform => "ANDROID";

        public int Version => 3300;

        private List<IGpsSattelitesInfo> _gpsSattelitesInfo = new List<IGpsSattelitesInfo>();
        private object _gpsSattelitesInfoLock = new object();
        public IGpsSattelitesInfo[] GpsSattelitesInfo
        {
            get
            {
                lock (_gpsSattelitesInfoLock)
                {
                    _gpsSattelitesInfo.Clear();


                    if(GameClient.Geoposition?.Coordinate?.SatelliteData != null)
                    {
                        //cant find API for this in UWP
                        //so mwhere to get that data? emulate? is there some service or algorithm for get GPS sattelites info?
                    }

                    return _gpsSattelitesInfo.ToArray();
                }
            }
        }

        private SensorInfoAndroid _sensors = new SensorInfoAndroid();
        public ISensorInfo Sensors => _sensors;

        //not filled on android
        public IActivityStatus ActivityStatus => null;

        private class SensorInfoAndroid : ISensorInfo
        {
            private readonly Random _random = new Random();

            private readonly Accelerometer _accelerometer = Accelerometer.GetDefault();

            private readonly Magnetometer _magnetometer = Magnetometer.GetDefault();

            private readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

            private readonly Inclinometer _inclinometer = Inclinometer.GetDefault();

            public long TimeSnapshot => DeviceInfos.RelativeTimeFromStart;

            //public ulong TimestampSnapshot = 0; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

            public double MagnetometerX => _magnetometer?.GetCurrentReading()?.MagneticFieldX ?? _random.NextGaussian(0.0, 0.1);

            public double MagnetometerY => _magnetometer?.GetCurrentReading()?.MagneticFieldY ?? _random.NextGaussian(0.0, 0.1);

            public double MagnetometerZ => _magnetometer?.GetCurrentReading()?.MagneticFieldZ ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawX => _gyrometer?.GetCurrentReading()?.AngularVelocityX ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawY => _gyrometer?.GetCurrentReading()?.AngularVelocityY ?? _random.NextGaussian(0.0, 0.1);

            public double GyroscopeRawZ => _gyrometer?.GetCurrentReading()?.AngularVelocityZ ?? _random.NextGaussian(0.0, 0.1);

            public double AngleNormalizedX => _inclinometer?.GetCurrentReading()?.PitchDegrees ?? _random.NextGaussian(0.0, 5.0);
            public double AngleNormalizedY => _inclinometer?.GetCurrentReading()?.YawDegrees ?? _random.NextGaussian(0.0, 5.0);
            public double AngleNormalizedZ => _inclinometer?.GetCurrentReading()?.RollDegrees ?? _random.NextGaussian(0.0, 5.0);

            public double AccelRawX => _accelerometer?.GetCurrentReading()?.AccelerationX ?? _random.NextGaussian(0.0, 0.3);

            public double AccelRawY => _accelerometer?.GetCurrentReading()?.AccelerationY ?? _random.NextGaussian(0.0, 0.3);

            public double AccelRawZ => _accelerometer?.GetCurrentReading()?.AccelerationZ ?? _random.NextGaussian(0.0, 0.3);


            public ulong AccelerometerAxes => 3;

        }

        #region LocationFix

        private class LocationFixAndroid : ILocationFix
        {
            private static readonly Random _random = new Random();

            private LocationFixAndroid()
            {
            }

            public static ILocationFix CollectData()
            {
                if (GameClient.Geoposition.Coordinate == null)
                    return null; //Nothing to collect

                LocationFixAndroid loc = new LocationFixAndroid();
                //Collect provider
                switch (GameClient.Geoposition.Coordinate.PositionSource)
                {
                    case Windows.Devices.Geolocation.PositionSource.WiFi:
                    case Windows.Devices.Geolocation.PositionSource.Cellular:
                        loc.Provider = "network"; break;
                    case Windows.Devices.Geolocation.PositionSource.Satellite:
                        loc.Provider = "gps"; break;
                    default:
                        loc.Provider = "fused"; break;
                }

                //1 = no fix, 2 = acquiring/inaccurate, 3 = fix acquired
                loc.ProviderStatus = 3;

                //Collect coordinates

                loc.Latitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Latitude;
                loc.Longitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Longitude;
                loc.Altitude = (float)GameClient.Geoposition.Coordinate.Point.Position.Altitude;

                // TODO: why 3? need more infos.
                loc.Floor = 3;

                // TODO: why 1? need more infos.
                loc.LocationType = 1;

                loc.TimeSnapshot = DeviceInfos.RelativeTimeFromStart;

                loc.HorizontalAccuracy = (float?)GameClient.Geoposition.Coordinate?.Accuracy ?? (float)Math.Floor((float)_random.NextGaussian(1.0, 1.0)); //better would be exp distribution

                return loc;
            }

            public string Provider { get; private set; }

            public ulong ProviderStatus { get; private set; }

            public float Latitude { get; private set; }

            public float Longitude { get; private set; }

            public float Altitude { get; private set; }

            public uint Floor { get; private set; }

            public ulong LocationType { get; private set; }

            public long TimeSnapshot { get; private set; }
            public float HorizontalAccuracy { get; private set; }
            public float VerticalAccuracy { get; private set; }
            public float Unknown20 { get; private set; }

        }

        #endregion

    }

}
