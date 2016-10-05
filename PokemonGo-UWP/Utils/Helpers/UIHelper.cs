using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;

namespace PokemonGo_UWP.Utils.Helpers
{
    public static class UIHelper
    {
        /// <summary>
        /// Create Color structure from HTML style color string
        /// </summary>
        /// <param name="colorString">Color string in format #AARRGGBB</param>
        /// <returns>Color structure</returns>
        public static Color ColorFromString(string colorString)
        {
            colorString = colorString.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(colorString.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(colorString.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(colorString.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(colorString.Substring(6, 2), 16));
            Color color = Color.FromArgb(a, r, g, b);
            return color;
        }

        /// <summary>
        /// Approximate if it should be day or night sky. Calculated from a given time & geolocation
        /// </summary>
        /// <param name="time">local time to check</param>
        /// <param name="tz">time zone of that location</param>
        /// <param name="location">geolocation</param>
        /// <returns>True for day sky, False for night sky.</returns>
        public static bool IsDaySky(DateTime time, TimeZoneInfo tz, Geoposition location)
        {
            var lat = location.Coordinate.Point.Position.Latitude;
            var lng = location.Coordinate.Point.Position.Longitude;
            var approxLocalNoon = TimeSpan.FromHours(12)
                - TimeSpan.FromHours((lng - 15 * tz.BaseUtcOffset.TotalHours) / 15)
                + TimeSpan.FromHours(tz.IsDaylightSavingTime(time) ? 1 : 0);
            var hours = 1 / 15 * (180 / Math.PI)
                * Math.Acos(
                    -Math.Tan(lat * (Math.PI / 180))
                    * Math.Tan(23.44 * (Math.PI / 180)
                        * Math.Sin((2 * Math.PI) * (time.DayOfYear + 284) / (DateTime.IsLeapYear(time.Year) ? 366 : 365))
                    )
                );
            var sunrise = approxLocalNoon - TimeSpan.FromHours(hours);
            var sunset = approxLocalNoon + TimeSpan.FromHours(hours);
            var currentTime = time.TimeOfDay;
            return (currentTime >= sunrise) && (currentTime < sunset);
        }

    }
}
