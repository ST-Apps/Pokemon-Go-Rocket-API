using System;
using POGOProtos.Enums;

namespace PokemonGo_UWP.Utils
{
    public class BadgeTypeAttribute : Attribute
    {
        public BadgeTypeAttribute(BadgeType value)
        {
            Value = value;
        }

        public BadgeType Value { get; set; }
    }
}