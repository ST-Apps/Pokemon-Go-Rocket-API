using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
	/// <summary>
	/// Contains the different modes available for the MapAutomaticOrientation
	/// </summary>
	public enum MapAutomaticOrientationModes
	{
		/// <summary>
		/// No rotation of map
		/// </summary>
		None = 0,
		/// <summary>
		/// Rotation based on GPS movement
		/// </summary>
		GPS = 1,
		/// <summary>
		/// Rotation based on Compass
		/// </summary>
		Compass = 2,
		//To be done later : Gyroscope=3
	}
}
