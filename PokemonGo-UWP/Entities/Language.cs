using System.Globalization;

namespace PokemonGo_UWP.Entities {
    public class Language {
        public string Code { get; set; }

        public override string ToString() {
            if("System".Equals(Code)){
                return "System";
            }
            return new CultureInfo(Code).NativeName;
        }

        public override bool Equals(object obj) {
            return Code.Equals(((Language)obj).Code);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
