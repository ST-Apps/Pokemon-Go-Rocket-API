using System;

namespace PokemonGo.RocketAPI.Helpers
{
    public interface ILocationFix
    {
        string Provider { get; }
        float Latitude { get; }
        float Longitude { get; }
        float Altitude { get; }
        uint Floor { get; }
        ulong LocationType { get; }
        ulong ProviderStatus { get; }
        ulong Timestamp { get; }
        float HorizontalAccuracy { get; }
        float VerticalAccuracy { get; }

    }

    public interface IDeviceInfo
    {
        TimeSpan TimeSnapshot { get; }
        ulong AccelerometerAxes { get;}
        double AccelRawX { get; }
        double AccelRawY { get; }
        double AccelRawZ { get; }
        double MagnetometerX { get; }
        double MagnetometerY { get; }
        double MagnetometerZ { get; }
        double AngleNormalizedX { get; }
        double AngleNormalizedY { get; }
        double AngleNormalizedZ { get; }
        double GyroscopeRawX { get; }
        double GyroscopeRawY { get; }
        double GyroscopeRawZ { get; }
        string DeviceID { get; }
        string FirmwareBrand { get; }
        string FirmwareType { get; }
        ILocationFix[] LocationFixes { get; }
    }
}