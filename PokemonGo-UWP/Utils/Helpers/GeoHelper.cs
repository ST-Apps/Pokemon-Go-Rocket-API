using System;
using Windows.Devices.Geolocation;

namespace PokemonGo_UWP.Utils.Helpers
{
    class GeoHelper
    {
        public static double Distance(Geopoint point1, Geopoint point2)
        {
            double theta = (point1.Position.Longitude - point2.Position.Longitude) * Math.PI / 180.0;
            double lat1 = point1.Position.Latitude * Math.PI / 180.0;
            double long1 = point1.Position.Longitude * Math.PI / 180.0;
            double lat2 = point2.Position.Latitude * Math.PI / 180.0;
            double long2 = point2.Position.Longitude * Math.PI / 180.0;
            double dist = Math.Sin(lat1) * Math.Sin(lat2) +
                          Math.Cos(lat1) * Math.Cos(lat2) *
                          Math.Cos(theta);
            dist = Math.Acos(dist);
            dist = dist / Math.PI * 180.0;
            dist = dist * 60 * 1.1515 * 1.609344 * 1000;
            return dist;
        }
    }
}
