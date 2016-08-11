using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
    public static class enumType
    {
        public static string getType(string name)
        {
            string type;
            switch (name)
            {
                case "Bulbasaur":
                    type = "Default";
                    break;
                case "Porygon":
                    type = "137";
                    break;
                case "Nidoran":
                    type = "137";
                    break;
                 
                case "Ivysaur":
                    type = "Default";
                    break;
                case "Venusaur":
                    type = "Default";
                    break;
                case "Charmander":
                    type = "Fire";
                    break;
                case "Charmeleon":
                    type = "Fire";
                    break;
                case "Charizard":
                    type = "Fire";
                    break;
                case "Squirtle":
                    type = "Water";
                    break;
                case "Wartortle":
                    type = "Water";
                    break;
                case "Blastoise":
                    type = "Water";
                    break;
                case "Caterpie":
                    type = "Bug";
                    break;
                case "Metapod":
                    type = "Bug";
                    break;
                case "Butterfree":
                    type = "Bug";
                    break;
                case "Weedle":
                    type = "Bug";
                    break;
                case "Kakuna":
                    type = "Bug";
                    break;
                case "Beedrill":
                    type = "Bug";
                    break;
                case "Pidgey":
                    type = "Flying";
                    break;
                case "Pidgeotto":
                    type = "Flying";
                    break;
                case "Pidgeot":
                    type = "Flying";
                    break;
                case "Rattata":
                    type = "Normal";
                    break;
                case "Raticate":
                    type = "Normal";
                    break;
                case "Spearow":
                    type = "Flying";
                    break;
                case "Fearow":
                    type = "Flying";
                    break;
                case "Ekans":
                    type = "Poison";
                    break;
                case "Arbok":
                    type = "Poison";
                    break;
                case "Pikachu":
                    type = "Electric";
                    break;
                case "Raichu":
                    type = "Electric";
                    break;
                case "Sandshrew":
                    type = "Ground";
                    break;
                case "Sandslash":
                    type = "Ground";
                    break;
                case "NidoranFemale":
                    type = "Poison";
                    break;
                case "Nodorina":
                    type = "Poison";
                    break;
                case "Nidoqueen":
                    type = "Poison";
                    break;
                case "NidoranMale":
                    type = "Poison";
                    break;
                case "Nidorino":
                    type = "Poison";
                    break;
                case "Nidoking":
                    type = "Poison";
                    break;
                case "Clefairy":
                    type = "Fairy";
                    break;
                case "Clefable":
                    type = "Fairy";
                    break;
                case "Vulpix":
                    type = "Fire";
                    break;
                case "Ninetales":
                    type = "Fire";
                    break;
                case "Jigglypuff":
                    type = "Fairy";
                    break;
                case "Wigglytuff":
                    type = "Fairy";
                    break;
                case "Zubat":
                    type = "Poison";
                    break;
                case "Golbat":
                    type = "Poison";
                    break;
                case "Oddish":
                    type = "Default";
                    break;
                case "Gloom":
                    type = "Default";
                    break;
                case "Vileplume":
                    type = "Default";
                    break;
                case "Paras":
                    type = "Bug";
                    break;
                case "Parasect":
                    type = "Bug";
                    break;
                case "Venonat":
                    type = "Bug";
                    break;
                case "Venomoth":
                    type = "Bug";
                    break;
                case "Diglett":
                    type = "Ground";
                    break;

                case "Dugtrio":
                    type = "Ground";
                    break;
                case "Meowth":
                    type = "Normal";
                    break;
                case "Persian":
                    type = "Normal";
                    break;
                case "Psyduck":
                    type = "Water";
                    break;
                case "Golduck":
                    type = "Water";
                    break;
                case "Mankey":
                    type = "Fighting";
                    break;
                case "Primeape":
                    type = "Fighting";
                    break;
                case "Growlithe":
                    type = "Fire";
                    break;
                case "Arcanine":
                    type = "Fire";
                    break;
                case "Poliwag":
                    type = "Water";
                    break;
                case "Poliwhirl":
                    type = "Water";
                    break;
                case "Poliwrath":
                    type = "Water";
                    break;
                case "Abra":
                    type = "Psychic";
                    break;
                case "Kadabra":
                    type = "Psychic";
                    break;
                case "Alakazam":
                    type = "Psychic";
                    break;
                case "Machop":
                    type = "Fighting";
                    break;
                case "Machoke":
                    type = "Fighting";
                    break;
                case "Machamp":
                    type = "Fighting";
                    break;
                case "Bellsprout":
                    type = "Default";
                    break;
                case "Weepinbell":
                    type = "Default";
                    break;
                case "Victreebel":
                    type = "Default";
                    break;
                case "Tentacool":
                    type = "Water";
                    break;
                case "Tentacruel":
                    type = "Water";
                    break;
                case "Geodude":
                    type = "Rock";
                    break;
                case "Graveler":
                    type = "Rock";
                    break;
                case "Golem":
                    type = "Rock";
                    break;
                case "Ponyta":
                    type = "Fire";
                    break;
                case "Rapidash":
                    type = "Fire";
                    break;
                case "Slowpoke":
                    type = "Water";
                    break;
                case "Slowbro":
                    type = "Water";
                    break;

                case "Magnemite":
                    type = "Electric";
                    break;
                case "Magneton":
                    type = "Electric";
                    break;
                case "Farfetch'd":
                    type = "Flying";
                    break;
                case "Doduo":
                    type = "Normal";
                    break;
                case "Dodrio":
                    type = "Normal";
                    break;
                case "Seel":
                    type = "Water";
                    break;
                case "Dewgong":
                    type = "Water";
                    break;
                case "Grimer":
                    type = "Poison";
                    break;
                case "Muk":
                    type = "Poison";
                    break;
                case "Shellder":
                    type = "Water";
                    break;
                case "Cloyster":
                    type = "Water";
                    break;
                case "Gastly":
                    type = "Ghost";
                    break;
                case "Haunter":
                    type = "Ghost";
                    break;
                case "Gengar":
                    type = "Ghost";
                    break;
                case "Onix":
                    type = "Rock";
                    break;
                case "Drowzee":
                    type = "Psychic";
                    break;
                case "Hypno":
                    type = "Psychic";
                    break;
                case "Krabby":
                    type = "Water";
                    break;
                case "Kingler":
                    type = "Water";
                    break;
                case "Voltorb":
                    type = "Electric";
                    break;
                case "Electrode":
                    type = "Electric";
                    break;
                case "Exeggcute":
                    type = "Default";
                    break;
                case "Exeggutor":
                    type = "Default";
                    break;
                case "Cubone":
                    type = "Ground";
                    break;
                case "Marowak":
                    type = "Ground";
                    break;
                case "Hitmonlee":
                    type = "Fighting";
                    break;
                case "Hitmonchan":
                    type = "Fighting";
                    break;
                case "Lickitung":
                    type = "Normal";
                    break;
                case "Koffing":
                    type = "Poison";
                    break;
                case "Weezing":
                    type = "Poison";
                    break;
                case "Rhyhorn":
                    type = "Rock";
                    break;
                case "Rhydon":
                    type = "Rock";
                    break;
                case "Chansey":
                    type = "Normal";
                    break;
                case "Tangela":
                    type = "Default";
                    break;

                case "Kangaskhan":
                    type = "Normal";
                    break;
                case "Horsea":
                    type = "Water";
                    break;
                case "Seadra":
                    type = "Water";
                    break;
                case "Goldeen":
                    type = "Water";
                    break;
                case "Seaking":
                    type = "Water";
                    break;
                case "Staryu":
                    type = "Water";
                    break;
                case "Starmie":
                    type = "Water";
                    break;
                case "Mr. Mime":
                    type = "Psychic";
                    break;
                case "Scyther":
                    type = "Bug";
                    break;
                case "Jynx":
                    type = "Psychic";
                    break;
                case "Electabuzz":
                    type = "Electric";
                    break;
                case "Magmar":
                    type = "Fire";
                    break;
                case "Pinsir":
                    type = "Bug";
                    break;
                case "Tauros":
                    type = "Normal";
                    break;
                case "Magikarp":
                    type = "Water";
                    break;
                case "Gyarados":
                    type = "Water";
                    break;
                case "Lapras":
                    type = "Water";
                    break;
                case "Ditto":
                    type = "Normal";
                    break;
                case "Eevee":
                    type = "Normal";
                    break;
                case "Vaporeon":
                    type = "Water";
                    break;
                case "Jolteon":
                    type = "Electric";
                    break;
                case "Flareon":
                    type = "Fire";
                    break;
                case "Omanyte":
                    type = "Rock";
                    break;
                case "Omastar":
                    type = "Rock";
                    break;
                case "Kabuto":
                    type = "Rock";
                    break;
                case "Kabutops":
                    type = "Rock";
                    break;
                case "Aerodactyl":
                    type = "Flying";
                    break;
                case "Snorlax":
                    type = "Normal";
                    break;
                case "Articuno":
                    type = "Ice";
                    break;
                case "Zapdos":
                    type = "Electric";
                    break;
                case "Moltres":
                    type = "Fire";
                    break;
                case "Dratini":
                    type = "Dragon";
                    break;
                case "Dragonair":
                    type = "Dragon";
                    break;
                case "Dragonite":
                    type = "Dragon";
                    break;
                case "Mewto":
                    type = "Psychic";
                    break;
                case "Mew":
                    type = "Psychic";
                    break;
                default:
                    type = "1";
                    break;
            }
            return type;
        }

    }
}
