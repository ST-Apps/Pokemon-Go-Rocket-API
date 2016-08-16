using System;

namespace PokemonGo_UWP.Utils.Achievements
{
    public class BronzeAttribute : Attribute
    {
        public BronzeAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}