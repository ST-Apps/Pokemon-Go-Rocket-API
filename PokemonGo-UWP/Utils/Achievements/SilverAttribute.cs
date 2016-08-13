using System;

namespace PokemonGo_UWP.Utils
{
    public class SilverAttribute : Attribute
    {
        public SilverAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}