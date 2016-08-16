using System.Linq;
using Windows.Security.Credentials;
using PokemonGo.RocketAPI.Enums;
using Template10.Services.SettingsService;

namespace PokemonGo_UWP.Utils
{
    public class SettingsService
    {
        public static readonly SettingsService Instance;

        private readonly SettingsHelper _helper;

        private readonly PasswordVault _passwordVault = new PasswordVault();

        static SettingsService()
        {
            Instance = Instance ?? new SettingsService();
        }

        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        #region Login & Authentication

        public bool RememberLoginData
        {
            get { return _helper.Read(nameof(RememberLoginData), false); }
            set { _helper.Write(nameof(RememberLoginData), value); }
        }

        public string Udid
        {
            get { return _helper.Read(nameof(Udid), string.Empty); }
            set { _helper.Write(nameof(Udid), value); }
        }

        public AuthType LastLoginService
        {
            get { return _helper.Read(nameof(LastLoginService), AuthType.Ptc); }
            set { _helper.Write(nameof(LastLoginService), value); }
        }

        public string AuthToken
        {
            get
            {
                var credentials = _passwordVault.RetrieveAll();
                var token = credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(AuthToken)));
                if (token == null) return string.Empty;
                token.RetrievePassword();
                return token.Password;
            }
            set
            {
                var credentials = _passwordVault.RetrieveAll();
                var currentToken =
                    credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(AuthToken)));
                if (currentToken != null) _passwordVault.Remove(currentToken);
                if (value == null) return;
                _passwordVault.Add(new PasswordCredential
                {
                    UserName = nameof(AuthToken),
                    Password = value,
                    Resource = nameof(AuthToken)
                });
            }
        }

        public PasswordCredential UserCredentials
        {
            get
            {
                var credentials = _passwordVault.RetrieveAll();
                return credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(UserCredentials)));
            }
            set
            {
                var credentials = _passwordVault.RetrieveAll();
                var currentCredential =
                    credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(UserCredentials)));
                if (currentCredential != null) _passwordVault.Remove(currentCredential);
                if (value == null) return;
                _passwordVault.Add(value);
            }
        }

        #endregion

        #region Game

        public bool IsAutoRotateMapEnabled
        {
            get { return _helper.Read(nameof(IsAutoRotateMapEnabled), false); }
            set { _helper.Write(nameof(IsAutoRotateMapEnabled), value); }
        }

        public bool IsMusicEnabled
        {
            get { return _helper.Read(nameof(IsMusicEnabled), false); }
            set { _helper.Write(nameof(IsMusicEnabled), value); }
        }

        public bool IsNianticMapEnabled
        {
            get { return _helper.Read(nameof(IsNianticMapEnabled), false); }
            set { _helper.Write(nameof(IsNianticMapEnabled), value); }
        }

        public bool IsRememberMapZoomEnabled
        {
            get { return _helper.Read(nameof(IsRememberMapZoomEnabled), false); }
            set { _helper.Write(nameof(IsRememberMapZoomEnabled), value); }
        }

        public bool IsVibrationEnabled
        {
            get { return _helper.Read(nameof(IsVibrationEnabled), false); }
            set { _helper.Write(nameof(IsVibrationEnabled), value); }
        }

        public LiveTileModes LiveTileMode
        {
            get { return _helper.Read(nameof(LiveTileMode), LiveTileModes.Off); }
            set { _helper.Write(nameof(LiveTileMode), value); }
        }

        public PokemonSortingModes PokemonSortingMode
        {
            get { return _helper.Read(nameof(PokemonSortingMode), PokemonSortingModes.Combat); }
            set { _helper.Write(nameof(PokemonSortingMode), value); }
        }
        
        public double Zoomlevel
        {
            get { return _helper.Read(nameof(Zoomlevel), (double)12); }
            set { _helper.Write(nameof(Zoomlevel), value); }
        }

        #endregion
    }
}