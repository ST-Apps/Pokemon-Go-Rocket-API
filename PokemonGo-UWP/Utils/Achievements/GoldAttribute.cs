using System;

namespace PokemonGo_UWP.Utils.Achievements
{
    public class GoldAttribute : Attribute
    {
        public GoldAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}