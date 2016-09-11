using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Newtonsoft.Json;

namespace PokemonGo_UWP.Utils.Helpers
{
    public class Styler /*: IEnumerable<string>, IEnumerator<string>*/
    {
        public string visibility { get; set; }
        public string color { get; set; }
        public string hue { get; set; }
        public string saturation { get; set; }
        public string lightness { get; set; }
        public string weight { get; set; }

        //[JsonIgnore]
        //public string this[int index]
        //{
        //    get
        //    {
        //        switch (_currentPosition)
        //        {
        //            case 0: return visibility;
        //            case 1: return color;
        //            case 2: return hue;
        //            case 3: return saturation;
        //            case 4: return lightness;
        //            case 5: return weight;
        //            default:
        //                return null;
        //        }
        //    }
        //}

        //[JsonIgnore]
        //public string this[string index]
        //{
        //    get
        //    {
        //        switch (index)
        //        {
        //            case nameof(visibility): return visibility;
        //            case nameof(color): return color;
        //            case nameof(hue): return hue;
        //            case nameof(saturation): return saturation;
        //            case nameof(lightness): return lightness;
        //            case nameof(weight): return weight;
        //            default:
        //                return null;
        //        }
        //    }
        //    set
        //    {
        //        switch (index)
        //        {
        //            case nameof(visibility):
        //                visibility = value;
        //                break;
        //            case nameof(color):
        //                color = value;
        //                break;
        //            case nameof(hue):
        //                hue = value;
        //                break;
        //            case nameof(saturation):
        //                saturation = value;
        //                break;
        //            case nameof(lightness):
        //                lightness = value;
        //                break;
        //            case nameof(weight):
        //                weight = value;
        //                break;
        //        }
        //    }
        //}

        //public IEnumerator<string> GetEnumerator()
        //{
        //    return this;
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        //[JsonIgnore]
        //private int _currentPosition;
        //[JsonIgnore]
        //private readonly int _maxPosition = 5;

        //public bool MoveNext()
        //{
        //    _currentPosition+=1;
        //    return _currentPosition < _maxPosition;
        //}

        //public void Reset()
        //{
        //    _currentPosition = 0;
        //}

        //[JsonIgnore]
        //public string Current => this[_currentPosition];

        //[JsonIgnore]
        //object IEnumerator.Current => Current;

        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }

    public class CustomMapStyle
    {
        public string featureType { get; set; }
        public string elementType { get; set; }
        public List<Styler> stylers { get; set; }
    }    

    public static class MapStyleHelpers
    {
        private static CustomMapStyle[] _lightMapStyles;

        /// <summary>
        /// TODO: no dark style yet
        /// </summary>
        private static CustomMapStyle[] _darkMapStyles;

        /// <summary>
        /// Encoded string for Google API
        /// </summary>
        public static string LightMapStyleString { get; private set; }

        /// <summary>
        /// TODO: no dark style yet
        /// </summary>
        public static string DarkMapStyleString { get; private set; }

        /// <summary>
        /// Mappings between feature type and feature code
        /// </summary>
        private static readonly Dictionary<string, string> StyleparseTypes = new Dictionary<string, string>()
        {
            {"all","0"},
            {"administrative","1"},
            {"administrative.country","17"},
            {"administrative.land_parcel","21"},
            {"administrative.locality","19"},
            {"administrative.neighborhood","20"},
            {"administrative.province","18"},
            {"landscape","5"},
            {"landscape.man_made","81"},
            {"landscape.natural","82"},
            {"poi","2"},
            {"poi.attraction","37"},
            {"poi.business","33"},
            {"poi.government","34"},
            {"poi.medical","36"},
            {"poi.park","40"},
            {"poi.place_of_worship","38"},
            {"poi.school","35"},
            {"poi.sports_complex","39"},
            {"road","3"},
            {"road.arterial","50"},
            {"road.highway","49"},
            {"road.local","51"},
            {"transit","4"},
            {"transit.line","65"},
            {"transit.station","66"},
            {"water","6"}
        };

        /// <summary>
        /// Mappings between feature element and its code
        /// </summary>
        private static readonly Dictionary<string, string> StyleparseElements = new Dictionary<string, string>()
        {
            {"all","a"},
            {"geometry","g"},
            {"geometry.fill","g.f"},
            {"geometry.stroke","g.s"},
            {"labels","l"},
            {"labels.icon","l.i"},
            {"labels.text","l.t"},
            {"labels.text.fill","l.t.f"},
            {"labels.text.stroke","l.t.s"}
        };

        /// <summary>
        /// Mappings between feature styler and its code
        /// </summary>
        private static readonly Dictionary<string, string> StyleparseStylers = new Dictionary<string, string>()
        {
            {"color","p.c"},
            {"gamma","p.g"},
            {"hue","p.h"},
            {"invert_lightness","p.il"},
            {"lightness","p.l"},
            {"saturation","p.s"},
            {"visibility","p.v"},
            {"weight","p.w"}
        };

        /// <summary>
        /// Converts the style to the encoded string format used by Google
        /// </summary>
        /// <param name="styles"></param>
        /// <returns></returns>
        private static string StyleToGoogleStringFormat(CustomMapStyle[] styles)
        {
            var ret = "";
            foreach (CustomMapStyle style in styles)
            {
                if (style.featureType != null)
                {
                    ret += "s.t:" + StyleparseTypes[style.featureType] + "|";
                }
                if (style.elementType != null)
                {
                    ret += "s.e:" + StyleparseElements[style.elementType] + "|";
                }
                if (style.stylers != null)
                {
                    foreach (var cstyler in style.stylers)
                    {
                        if (!string.IsNullOrEmpty(cstyler.visibility))
                        {
                            ret += StyleparseStylers[nameof(cstyler.visibility)] + ":" + cstyler.visibility + "|";
                        }
                        if (!string.IsNullOrEmpty(cstyler.color))
                        {
                            if (cstyler.color.Length == 7)
                                cstyler.color = "#ff" + cstyler.color.Substring(1);
                            ret += StyleparseStylers[nameof(cstyler.color)] + ":" + cstyler.color + "|";
                        }
                        if (!string.IsNullOrEmpty(cstyler.hue))
                        {
                            ret += StyleparseStylers[nameof(cstyler.hue)] + ":" + cstyler.hue + "|";
                        }
                        if (!string.IsNullOrEmpty(cstyler.saturation))
                        {
                            ret += StyleparseStylers[nameof(cstyler.saturation)] + ":" + cstyler.saturation + "|";
                        }
                        if (!string.IsNullOrEmpty(cstyler.lightness))
                        {
                            ret += StyleparseStylers[nameof(cstyler.lightness)] + ":" + cstyler.lightness + "|";
                        }
                        if (!string.IsNullOrEmpty(cstyler.weight))
                        {
                            ret += StyleparseStylers[nameof(cstyler.weight)] + ":" + cstyler.weight + "|";
                        }
                    }
                }
                ret = ret.Substring(0, ret.Length - 1);
                ret += ",";
            }
            return Uri.EscapeDataString(ret.Substring(0, ret.Length - 1));
        }

        /// <summary>
        /// Loads json style and deserializes it
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<CustomMapStyle[]> ReadStyleFromFile(string fileName)
        {
                var appUri = new Uri(fileName);
                var anjFile = await StorageFile.GetFileFromApplicationUriAsync(appUri);
                var jsonText = await FileIO.ReadTextAsync(anjFile);
                return JsonConvert.DeserializeObject<CustomMapStyle[]>(jsonText);
        }

        /// <summary>
        /// Initialize styles from json files
        /// </summary>
        public static async Task Initialize()
        {
            _lightMapStyles = _darkMapStyles = await ReadStyleFromFile("ms-appx:///Styles/NianticStyleLight.json");

            LightMapStyleString = StyleToGoogleStringFormat(_lightMapStyles);
            DarkMapStyleString = StyleToGoogleStringFormat(_darkMapStyles);            
        }
    }
}
