using System;

namespace PokemonGo_UWP.Utils {
    public class SilverAttribute : Attribute {
        public SilverAttribute(object value) {
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
