using System;

namespace PokemonGo_UWP.Utils
{
    public class AchievementValueAttribute : Attribute
    {
        public AchievementValueAttribute(object value)
        {
            Value = value;
        }

        public object Value { get; set; }
    }
}