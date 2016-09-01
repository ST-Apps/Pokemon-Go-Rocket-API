using System.Linq;
using Windows.Security.Credentials;
using PokemonGo.RocketAPI.Enums;
using Template10.Services.SettingsService;
using System.ComponentModel;
using System;
using Template10.Mvvm;
using System.Runtime.CompilerServices;

namespace PokemonGo_UWP.Utils
{
	public class SettingsService : BindableBase
	{
		#region Singleton
		private readonly static Lazy<SettingsService> _instance = new Lazy<SettingsService>(() => new SettingsService());
		public static SettingsService Instance { get { return _instance.Value; } }
		private SettingsService()
		{
		}
		#endregion

		#region Storage helper
		private readonly SettingsHelper _helper = new SettingsHelper();
		public void Set<T>(T value, [CallerMemberName] string propertyName = null)
		{
			_helper.Write(propertyName, value);
			RaisePropertyChanged(propertyName);
		}
		public T Get<T>(T defaultValue, [CallerMemberName] string propertyName = null)
		{
			return _helper.Read<T>(propertyName, defaultValue);
		}
		#endregion

		private readonly PasswordVault _passwordVault = new PasswordVault();


		#region Login & Authentication

		public bool RememberLoginData
		{
			get { return Get(false); }
			set { Set(value); }
		}

		public string Udid
		{
			get { return Get(string.Empty); }
			set { Set(value); }
		}
        public string AndroidDeviceID
        {
            get { return Get(string.Empty); }
            set { Set(value); }
        }

        public AuthType LastLoginService
		{
			get { return Get(AuthType.Ptc); }
			set { Set(value); }
		}

		public string AccessTokenString
		{
			get
			{
				var credentials = _passwordVault.RetrieveAll();
				var token = credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(AccessTokenString)));
				if (token == null) return string.Empty;
				token.RetrievePassword();
				return token.Password;
			}
			set
			{
				var credentials = _passwordVault.RetrieveAll();
				var currentToken =
						credentials.FirstOrDefault(credential => credential.Resource.Equals(nameof(AccessTokenString)));
				if (currentToken != null) _passwordVault.Remove(currentToken);
				if (value == null) return;
				_passwordVault.Add(new PasswordCredential
				{
					UserName = nameof(AccessTokenString),
					Password = value,
					Resource = nameof(AccessTokenString)
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
		public MapAutomaticOrientationModes MapAutomaticOrientationMode
		{
			get { return Get(MapAutomaticOrientationModes.None); }
			set { Set(value); }
		}
		public bool IsMusicEnabled
		{
			get { return Get(false); }
		    set
		    {
		        Set(value);
                AudioUtils.ToggleSounds();
		    }
		}
		public bool IsNianticMapEnabled
		{
			get { return Get(false); }
			set { Set(value); }
		}
		public bool IsRememberMapZoomEnabled
		{
			get { return Get(false); }
			set { Set(value); }
		}
		public double MapPitch
		{
			get { return Get(0D); }
			set { Set(value); }
		}
		public double MapHeading
		{
			get { return Get(0D); }
			set { Set(value); }
		}
		public bool IsVibrationEnabled
		{
			get { return Get(false); }
			set { Set(value); }
		}
        public bool IsBatterySaverEnabled
        {
            get { return Get(false); }
            set { Set(value); }
        }
        public LiveTileModes LiveTileMode
		{
			get { return Get(LiveTileModes.Off); }
			set { Set(value); }
		}
		public PokemonSortingModes PokemonSortingMode
		{
			get { return Get(PokemonSortingModes.Combat); }
			set { Set(value); }
		}
		public double Zoomlevel
		{
			get { return Get(12D); }
			set { Set(value); }
		}

		#endregion
	}
}