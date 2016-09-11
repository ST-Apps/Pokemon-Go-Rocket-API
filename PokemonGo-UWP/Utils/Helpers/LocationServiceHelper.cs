using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Devices.Geolocation;

namespace PokemonGo_UWP.Utils.Helpers
{
	public class LocationServiceHelper : BindableBase
	{
		#region Singleton
		private readonly static Lazy<LocationServiceHelper> _instance = new Lazy<LocationServiceHelper>(() => new LocationServiceHelper());
		public static LocationServiceHelper Instance { get { return _instance.Value; } }

		#region internal vars
		private static Geolocator _geolocator;
		#endregion
		private LocationServiceHelper()
		{
			_geolocator = new Geolocator
			{
				DesiredAccuracy = PositionAccuracy.High,
				DesiredAccuracyInMeters = 5,
				ReportInterval = 1000,
				MovementThreshold = 5
			};
			_geolocator.PositionChanged += async (Geolocator sender, PositionChangedEventArgs args) => { Geoposition = await sender.GetGeopositionAsync(); };
		}
		#endregion

		private Geoposition _Geoposition = null;
		public Geoposition Geoposition
		{
			get { return _Geoposition; }
			private set { Set(ref _Geoposition, value); }
		}

		public async Task InitializeAsync()
		{
			Geoposition = Geoposition ?? await _geolocator.GetGeopositionAsync();
		}

		public void UpdateMovementThreshold(float newMovementThreshold)
		{
			_geolocator.MovementThreshold = newMovementThreshold;
		}
	}
}
