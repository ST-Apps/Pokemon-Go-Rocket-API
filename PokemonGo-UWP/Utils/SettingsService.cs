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

        public string UDID
        {
            get { return _helper.Read(nameof(UDID), string.Empty); }
            set { _helper.Write(nameof(UDID), value); }
        }

        public AuthType LastLoginService
        {
            get { return _helper.Read(nameof(LastLoginService), AuthType.Ptc); }
            set { _helper.Write(nameof(LastLoginService), value);}
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
                var currentToken = credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(AuthToken)));
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
                var currentCredential = credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(UserCredentials)));
                if (currentCredential != null) _passwordVault.Remove(currentCredential);
                if (value == null) return;
                _passwordVault.Add(value);
            }
		}        

        #endregion

        #region Game

        public bool IsMusicEnabled
		{
			get { return _helper.Read(nameof(IsMusicEnabled), false); }
			set { _helper.Write(nameof(IsMusicEnabled), value); }
		}

		public bool IsVibrationEnabled
		{
			get { return _helper.Read(nameof(IsVibrationEnabled), false); }
			set { _helper.Write(nameof(IsVibrationEnabled), value); }
		}

        public bool IsAutoRotateMapEnabled
        {
            get { return _helper.Read(nameof(IsAutoRotateMapEnabled), false); }
            set { _helper.Write(nameof(IsAutoRotateMapEnabled), value); }
        }

        public bool IsRememberMapZoomEnabled
        {
            get { return _helper.Read(nameof(IsRememberMapZoomEnabled), false); }
            set { _helper.Write(nameof(IsRememberMapZoomEnabled), value); }
        }
        
        public double Zoomlevel
        {
            get { return _helper.Read(nameof(Zoomlevel), (double)12); }
            set { _helper.Write(nameof(Zoomlevel), value); }
        }

        public PokemonSortingModes PokemonSortingMode
        {
            get { return this._helper.Read(nameof(PokemonSortingMode), PokemonSortingModes.Combat); }
            set { this._helper.Write(nameof(PokemonSortingMode), value); }
        }
        

        #endregion
    }
}