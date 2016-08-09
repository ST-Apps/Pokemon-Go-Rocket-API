using System;

namespace PokemonGo_UWP.Utils {
    public class GoldAttribute : Attribute {
        public GoldAttribute(object value) {
            this.value = value;
        }
        private object value;
        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
