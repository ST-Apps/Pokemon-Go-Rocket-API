using POGOProtos.Enums;
using System;

namespace PokemonGo_UWP.Utils {
    public class BadgeTypeAttribute : Attribute {
        public BadgeTypeAttribute(BadgeType value) {
            this.value = value;
        }
        private BadgeType value;
        public BadgeType Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
