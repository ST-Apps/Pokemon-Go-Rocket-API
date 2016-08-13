using System;

namespace PokemonGo_UWP.Utils
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