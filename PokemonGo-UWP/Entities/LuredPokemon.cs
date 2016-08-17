using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using POGOProtos.Map.Fort;

namespace PokemonGo_UWP.Entities
{
    
    public class LuredPokemon
    {
        /// <summary>
        /// Infos on the current lured Pokemon
        /// </summary>
        public FortLureInfo LureInfo { get; private set; }

        public Geopoint Geoposition { get; private set; }

        /// <summary>
        ///     HACK - this should fix Pokestop floating on map
        /// </summary>
        public Point Anchor => new Point(0.5, 1);
        
        public LuredPokemon(FortLureInfo lureInfo, double lat, double lng)
        {
            LureInfo = lureInfo;
            Geoposition = new Geopoint(new BasicGeoposition {Latitude = lat, Longitude = lng});
        }
    }
}
