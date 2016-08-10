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
    public static class DeviceInfos
    {

        #region Device

        public static string UDID
        {
            get
            {
                if (string.IsNullOrEmpty(SettingsService.Instance.UDID)) SettingsService.Instance.UDID = RandomString(40);
                return SettingsService.Instance.UDID;
            }
        }

        private static readonly Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEF0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // TODO: check for iPhone
        public static string FirmwareBrand = "10";

        // TODO: check for iPhone
        public static string FirmwareType = "13";

        #endregion

        #region LocationFix

        public static string Provider = "fused";

        public static string Latitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Latitude}";

        public static string Longitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Longitude}";

        public static string Altitude = $"{GameClient.Geoposition.Coordinate.Point.Position.Altitude}";

        // TODO: why 3? need more infos.
        public static string Floor = "3";

        // TODO: why 1? need more infos.
        public static string LocationType = "1";

        #endregion

        #region Sensors

        private static readonly Accelerometer Accelerometer = Accelerometer.GetDefault();

        private static Compass _compass = Compass.GetDefault();

        private static readonly Magnetometer Magnetometer = Magnetometer.GetDefault();

        private static readonly Gyrometer _gyrometer = Gyrometer.GetDefault();

        // TODO: if no accelerometer/compass (e.g. on desktop) what should we do?
        public static string AccelNormalizedX => $"{Accelerometer.GetCurrentReading().AccelerationX}";

        public static string AccelNormalizedY => $"{Accelerometer.GetCurrentReading().AccelerationY}";

        public static string AccelNormalizedZ => $"{Accelerometer.GetCurrentReading().AccelerationZ}";

        public static string TimestampSnapshot = ""; //(ulong)(ElapsedMilliseconds - 230L) = TimestampSinceStart - 30L

        public static string MagnetometerX => $"{Magnetometer.GetCurrentReading().MagneticFieldX}";

        public static string MagnetometerY => $"{Magnetometer.GetCurrentReading().MagneticFieldY}";

        public static string MagnetometerZ => $"{Magnetometer.GetCurrentReading().MagneticFieldZ}";

        public static string AccelRawX => $"{_gyrometer.GetCurrentReading().AngularVelocityX}";

        public static string AccelRawY => $"{_gyrometer.GetCurrentReading().AngularVelocityY}";

        public static string AccelRawZ => $"{_gyrometer.GetCurrentReading().AngularVelocityZ}";

        // TODO: missing values!
        //AngleNormalizedX = 17.950439453125,
        //AngleNormalizedY = -23.36273193359375,
        //AngleNormalizedZ = -48.8250732421875,

        //GyroscopeRawX = 7.62939453125E-05,
        //GyroscopeRawY = -0.00054931640625,
        //GyroscopeRawZ = 0.0024566650390625,

        public static string AccelerometerAxes = "3";

        #endregion

    }
}
