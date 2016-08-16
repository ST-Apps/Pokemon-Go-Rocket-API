using System;

namespace PokemonGo_UWP.Utils.Achievements
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