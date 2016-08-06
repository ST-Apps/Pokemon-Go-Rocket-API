using Template10.Services.SettingsService;

namespace PokemonGo_UWP.Utils
{
    public class SettingsService
    {
        public static readonly SettingsService Instance;

        private readonly SettingsHelper _helper;

        static SettingsService()
        {
            Instance = Instance ?? new SettingsService();
        }

        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        #region Login & Authentication

        public string PtcAuthToken
        {
            get { return _helper.Read(nameof(PtcAuthToken), string.Empty); }
            set { _helper.Write(nameof(PtcAuthToken), value); }
        }

        public string GoogleAuthToken
        {
            get { return _helper.Read(nameof(GoogleAuthToken), string.Empty); }
            set { _helper.Write(nameof(GoogleAuthToken), value); }
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

        #endregion
    }
}