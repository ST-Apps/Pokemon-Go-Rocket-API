using PokemonGo.RocketAPI.Enums;

namespace PokemonGo.RocketAPI.Console
{
    public class Settings : ISettings
    {
        private AuthType _authType;
        private double _defaultLatitude;
        private double _defaultLongitude;
        private double _defaultAltitude;
        private string _googleRefreshToken;
        private string _ptcPassword;
        private string _ptcUsername;
        private string _googleUsername;
        private string _googlePassword;

        #region Implementation of ISettings

        public AuthType AuthType
        {
            get { return _authType; }
            set { _authType = value; }
        }

        public double DefaultLatitude
        {
            get { return _defaultLatitude; }
            set { _defaultLatitude = value; }
        }

        public double DefaultLongitude
        {
            get { return _defaultLongitude; }
            set { _defaultLongitude = value; }
        }

        public double DefaultAltitude
        {
            get { return _defaultAltitude; }
            set { _defaultAltitude = value; }
        }

        public string GoogleRefreshToken
        {
            get { return _googleRefreshToken; }
            set { _googleRefreshToken = value; }
        }

        public string PtcPassword
        {
            get { return _ptcPassword; }
            set { _ptcPassword = value; }
        }

        public string PtcUsername
        {
            get { return _ptcUsername; }
            set { _ptcUsername = value; }
        }

        public string GoogleUsername
        {
            get { return _googleUsername; }
            set { _googleUsername = value; }
        }

        public string GooglePassword
        {
            get { return _googlePassword; }
            set { _googlePassword = value; }
        }

        #endregion
    }
}