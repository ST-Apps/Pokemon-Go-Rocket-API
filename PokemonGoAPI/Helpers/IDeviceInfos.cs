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
        long TimeSnapshot { get; }
        float HorizontalAccuracy { get; }
        float VerticalAccuracy { get; }
        float Unknown20 { get; }
    }

    public interface IGpsSattelitesInfo
    {
        int SattelitesPrn { get; }
        float Azimuth { get; }
        float Elevation { get; }
        float Snr { get; }
        bool Almanac { get; }
        bool Emphasis { get; }
        bool UsedInFix { get; }
    }
    public interface ISensorInfo
    {

        long TimeSnapshot { get; }
        ulong AccelerometerAxes { get; }
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
    }

    public interface IActivityStatus
    {
        bool Walking { get; }
        bool Automotive { get; }
        bool Cycling { get; }
        bool Running { get; }
        bool Stationary  { get; }
        bool Tilting  { get; }
}

    public interface IDeviceInfo
    {
        string DeviceID { get; }
		string AndroidBoardName { get; }
        string AndroidBootloader { get; }
        string DeviceBrand { get; }
        string DeviceModel { get; }
        string DeviceModelIdentifier { get; }
        string DeviceModelBoot { get; }
        string HardwareManufacturer { get; }
        string HardwareModel { get; }
        string FirmwareBrand { get; }
        string FirmwareTags { get; }
        string FirmwareType { get; }
        string FirmwareFingerprint { get; }
        long TimeSnapshot { get; }
        ILocationFix[] LocationFixes { get; }
        IGpsSattelitesInfo[] GpsSattelitesInfo { get; }
        ISensorInfo Sensors { get; }
        IActivityStatus ActivityStatus { get; }
    }
}