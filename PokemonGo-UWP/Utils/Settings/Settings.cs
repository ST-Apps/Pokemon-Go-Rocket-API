using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        #region Implementation of ISettings

        public AuthType AuthType { get; set; }

        public double DefaultLatitude { get; set; }

        public double DefaultLongitude { get; set; }

        public double DefaultAltitude { get; set; }

        public string GoogleRefreshToken { get; set; }

        public string PtcPassword { get; set; }

        public string PtcUsername { get; set; }

        public string GoogleUsername { get; set; }

        public string GooglePassword { get; set; }

        #endregion
    }
}