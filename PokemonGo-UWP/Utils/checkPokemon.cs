using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Assets.Pokemons
{
    public static class checkPokemon
    {
        public static string[] getId(string id)
        {
            string img = "";
            string BaseCandy = "";
            string SecondCandy = "";

            string[] Name = { img, BaseCandy, SecondCandy };

            switch (id)
            {
                case "Bulbasaur":
                    {
                        img = "1";
                        BaseCandy = "3AC5A6";
                        SecondCandy = "9EFE90";
                        break;
                    }
                case "Ivysaur":
                    {
                        img = "2";
                        BaseCandy = "3AC5A6";
                        SecondCandy = "9EFE90";
                        break;
                    }
                case "Venusaur":
                    {
                        img = "3";
                        BaseCandy = "3AC5A6";
                        SecondCandy = "9EFE90";
                        break;
                    }
                case "Charmander":
                    {
                        img = "4";
                        BaseCandy = "F29432";
                        SecondCandy = "FCE69D";
                        break;
                    }
                case "Charmeleon":
                    {
                        img = "5";
                        BaseCandy = "F29432";
                        SecondCandy = "FCE69D";
                        break;
                    }
                case "Charizard":
                    {
                        img = "6";
                        BaseCandy = "F29432";
                        SecondCandy = "FCE69D";
                        break;
                    }
                case "Squirtle":
                    {
                        img = "7";
                        BaseCandy = "8AC1D5";
                        SecondCandy = "CEEFE8";
                        break;
                    }
                case "Wartortle":
                    {
                        img = "8";
                        BaseCandy = "8AC1D5";
                        SecondCandy = "CEEFE8";
                        break;
                    }
                case "Blastoise":
                    {
                        img = "9";
                        BaseCandy = "8AC1D5";
                        SecondCandy = "CEEFE8";
                        break;
                    }
                case "Caterpie":
                    {
                        img = "10";
                        BaseCandy = "ABCE8C";
                        SecondCandy = "F1E8B1";
                        break;
                    }
                case "Metapod":
                    {
                        img = "11";
                        BaseCandy = "ABCE8C";
                        SecondCandy = "F1E8B1";
                        break;
                    }
                case "Butterfree":
                    {
                        img = "12";
                        BaseCandy = "ABCE8C";
                        SecondCandy = "F1E8B1";
                        break;
                    }
                case "Weedle":
                    {
                        img = "13";
                        BaseCandy = "F1E8B1";
                        SecondCandy = "D17C9D";
                        break;
                    }
                case "Kakuna":
                    {
                        img = "14";
                        BaseCandy = "F1E8B1";
                        SecondCandy = "D17C9D";
                        break;
                    }
                case "Beedrill":
                    {
                        img = "15";
                        BaseCandy = "F1E8B1";
                        SecondCandy = "D17C9D";
                        break;
                    }
                case "Pidgey":
                    {
                        img = "16";
                        BaseCandy = "F2E1B5";
                        SecondCandy = "C39A6C";
                        break;
                    }
                case "Pidgeotto":
                    {
                        img = "17";
                        BaseCandy = "F2E1B5";
                        SecondCandy = "C39A6C";
                        break;
                    }
                case "Pidgeot":
                    {
                        img = "18";
                        BaseCandy = "F2E1B5";
                        SecondCandy = "C39A6C";
                        break;
                    }
                case "Rattata":
                    {
                        img = "19";
                        BaseCandy = "A885BB";
                        SecondCandy = "DED4CA";
                        break;
                    }
                case "Raticate":
                    {
                        img = "20";
                        BaseCandy = "A885BB";
                        SecondCandy = "DED4CA";
                        break;
                    }
                case "Spearow":
                    {
                        img = "21";
                        BaseCandy = "F0B99A";
                        SecondCandy = "E56866";
                        break;
                    }
                case "Fearow":
                    {
                        img = "22";
                        BaseCandy = "F0B99A";
                        SecondCandy = "E56866";
                        break;
                    }
                case "Ekans":
                    {
                        img = "23";
                        BaseCandy = "D3ACBF";
                        SecondCandy = "F3DE9F";
                        break;
                    }
                case "Arbok":
                    {
                        img = "24";
                        BaseCandy = "D3ACBF";
                        SecondCandy = "F3DE9F";
                        break;
                    }
                case "Pikachu":
                    {
                        img = "25";
                        BaseCandy = "FFD36E";
                        SecondCandy = "E3AA5B";
                        break;
                    }
                case "Raichu":
                    {
                        img = "26";
                        BaseCandy = "FFD36E";
                        SecondCandy = "E3AA5B";
                        break;
                    }
                case "Sandshrew":
                    {
                        img = "27";
                        BaseCandy = "ECDBA5";
                        SecondCandy = "CFB874";
                        break;
                    }
                case "Sandslash":
                    {
                        img = "28";
                        BaseCandy = "ECDBA5";
                        SecondCandy = "CFB874";
                        break;
                    }
                case "Nidoran♀":
                    {
                        img = "29";
                        BaseCandy = "D4DFF5";
                        SecondCandy = "999EC4";
                        break;
                    }
                case "Nidorina":
                    {
                        img = "30";
                        BaseCandy = "D4DFF5";
                        SecondCandy = "999EC4";
                        break;
                    }
                case "Nidoqueen":
                    {
                        img = "31";
                        BaseCandy = "D4DFF5";
                        SecondCandy = "999EC4";
                        break;
                    }
                case "Nidoran♂":
                    {
                        img = "32";
                        BaseCandy = "D9ABC8";
                        SecondCandy = "C0769B";
                        break;
                    }
                case "Nidorino":
                    {
                        img = "33";
                        BaseCandy = "D9ABC8";
                        SecondCandy = "C0769B";
                        break;
                    }
                case "Nidoking":
                    {
                        img = "34";
                        BaseCandy = "D9ABC8";
                        SecondCandy = "C0769B";
                        break;
                    }
                case "Clefairy":
                    {
                        img = "35";
                        BaseCandy = "FBD7DB";
                        SecondCandy = "EEC1C6";
                        break;
                    }
                case "Clefable":
                    {
                        img = "36";
                        BaseCandy = "FBD7DB";
                        SecondCandy = "EEC1C6";
                        break;
                    }
                case "Vulpix":
                    {
                        img = "37";
                        BaseCandy = "E06B61";
                        SecondCandy = "FFBA9B";
                        break;
                    }
                case "Ninetales":
                    {
                        img = "38";
                        BaseCandy = "E06B61";
                        SecondCandy = "FFBA9B";
                        break;
                    }
                case "Jigglypuff":
                    {
                        img = "39";
                        BaseCandy = "F7D8E7";
                        SecondCandy = "E6BAD1";
                        break;
                    }
                case "Wigglytuff":
                    {
                        img = "40";
                        BaseCandy = "F7D8E7";
                        SecondCandy = "E6BAD1";
                        break;
                    }
                case "Zubat":
                    {
                        img = "41";
                        BaseCandy = "4583CC";
                        SecondCandy = "D587D2";
                        break;
                    }
                case "Golbat":
                    {
                        img = "42";
                        BaseCandy = "4583CC";
                        SecondCandy = "D587D2";
                        break;
                    }
                case "Oddish":
                    {
                        img = "43";
                        BaseCandy = "799BB4";
                        SecondCandy = "74B980";
                        break;
                    }
                case "Gloom":
                    {
                        img = "44";
                        BaseCandy = "799BB4";
                        SecondCandy = "74B980";
                        break;
                    }
                case "Vileplume":
                    {
                        img = "45";
                        BaseCandy = "799BB4";
                        SecondCandy = "74B980";
                        break;
                    }
                case "Paras":
                    {
                        img = "46";
                        BaseCandy = "EA6043";
                        SecondCandy = "FFA25D";
                        break;
                    }
                case "Parasect":
                    {
                        img = "47";
                        BaseCandy = "EA6043";
                        SecondCandy = "FFA25D";
                        break;
                    }
                case "Venonat":
                    {
                        img = "48";
                        BaseCandy = "9987D9";
                        SecondCandy = "BB5584";
                        break;
                    }
                case "Venomoth":
                    {
                        img = "49";
                        BaseCandy = "9987D9";
                        SecondCandy = "BB5584";
                        break;
                    }
                case "Diglett":
                    {
                        img = "50";
                        BaseCandy = "B68479";
                        SecondCandy = "F3C7D4";
                        break;
                    }
                case "Dugtrio":
                    {
                        img = "51";
                        BaseCandy = "B68479";
                        SecondCandy = "F3C7D4";
                        break;
                    }
                case "Meowth":
                    {
                        img = "52";
                        BaseCandy = "E8E3C6";
                        SecondCandy = "F7E39A";
                        break;
                    }
                case "Persian":
                    {
                        img = "53";
                        BaseCandy = "E8E3C6";
                        SecondCandy = "F7E39A";
                        break;
                    }
                case "Psyduck":
                    {
                        img = "54";
                        BaseCandy = "FFCE7E";
                        SecondCandy = "EFE9D1";
                        break;
                    }
                case "Golduck":
                    {
                        img = "55";
                        BaseCandy = "FFCE7E";
                        SecondCandy = "EFE9D1";
                        break;
                    }
                case "Mankey":
                    {
                        img = "56";
                        BaseCandy = "E6D8CF";
                        SecondCandy = "C79A83";
                        break;
                    }
                case "Primeape":
                    {
                        img = "57";
                        BaseCandy = "E6D8CF";
                        SecondCandy = "C79A83";
                        break;
                    }
                case "Growlithe":
                    {
                        img = "58";
                        BaseCandy = "FA9D40";
                        SecondCandy = "46442D";
                        break;
                    }
                case "Arcanine":
                    {
                        img = "59";
                        BaseCandy = "FA9D40";
                        SecondCandy = "46442D";
                        break;
                    }
                case "Poliwag":
                    {
                        img = "60";
                        BaseCandy = "82879B";
                        SecondCandy = "BCC2D8";
                        break;
                    }
                case "Poliwhirl":
                    {
                        img = "61";
                        BaseCandy = "82879B";
                        SecondCandy = "BCC2D8";
                        break;
                    }
                case "Poliwrath":
                    {
                        img = "62";
                        BaseCandy = "82879B";
                        SecondCandy = "BCC2D8";
                        break;
                    }
                case "Abra":
                    {
                        img = "63";
                        BaseCandy = "E2DC50";
                        SecondCandy = "9C7882";
                        break;
                    }
                case "Kadabra":
                    {
                        img = "64";
                        BaseCandy = "E2DC50";
                        SecondCandy = "9C7882";
                        break;
                    }
                case "Alakazam":
                    {
                        img = "65";
                        BaseCandy = "E2DC50";
                        SecondCandy = "9C7882";
                        break;
                    }
                case "Machop":
                    {
                        img = "66";
                        BaseCandy = "ADC4D4";
                        SecondCandy = "D4D5CD";
                        break;
                    }
                case "Machoke":
                    {
                        img = "67";
                        BaseCandy = "ADC4D4";
                        SecondCandy = "D4D5CD";
                        break;
                    }
                case "Machamp":
                    {
                        img = "68";
                        BaseCandy = "ADC4D4";
                        SecondCandy = "D4D5CD";
                        break;
                    }
                case "Bellsprout":
                    {
                        img = "69";
                        BaseCandy = "E7E169";
                        SecondCandy = "B0D892";
                        break;
                    }
                case "Weepinbell":
                    {
                        img = "70";
                        BaseCandy = "E7E169";
                        SecondCandy = "B0D892";
                        break;
                    }
                case "Victreebel":
                    {
                        img = "71";
                        BaseCandy = "E7E169";
                        SecondCandy = "B0D892";
                        break;
                    }
                case "Tentacool":
                    {
                        img = "72";
                        BaseCandy = "7CB3EB";
                        SecondCandy = "C0537D";
                        break;
                    }
                case "Tentacruel":
                    {
                        img = "73";
                        BaseCandy = "7CB3EB";
                        SecondCandy = "C0537D";
                        break;
                    }
                case "Geodude":
                    {
                        img = "74";
                        BaseCandy = "AEA886";
                        SecondCandy = "72670D";
                        break;
                    }
                case "Graveler":
                    {
                        img = "75";
                        BaseCandy = "AEA886";
                        SecondCandy = "72670D";
                        break;
                    }
                case "Golem":
                    {
                        img = "76";
                        BaseCandy = "AEA886";
                        SecondCandy = "72670D";
                        break;
                    }
                case "Ponyta":
                    {
                        img = "77";
                        BaseCandy = "F4BF93";
                        SecondCandy = "F9D6AC";
                        break;
                    }
                case "Rapidash":
                    {
                        img = "78";
                        BaseCandy = "F4BF93";
                        SecondCandy = "F9D6AC";
                        break;
                    }
                case "Slowpoke":
                    {
                        img = "79";
                        BaseCandy = "E4A4BF";
                        SecondCandy = "F8DDD4";
                        break;
                    }
                case "Slowbro":
                    {
                        img = "80";
                        BaseCandy = "E4A4BF";
                        SecondCandy = "F8DDD4";
                        break;
                    }
                case "Magnemite":
                    {
                        img = "81";
                        BaseCandy = "DFDFEB";
                        SecondCandy = "A2BECA";
                        break;
                    }
                case "Magneton":
                    {
                        img = "82";
                        BaseCandy = "DFDFEB";
                        SecondCandy = "A2BECA";
                        break;
                    }
                case "Farfetch'd":
                    {
                        img = "83";
                        BaseCandy = "B3A29B";
                        SecondCandy = "96FD90";
                        break;
                    }
                case "Doduo":
                    {
                        img = "84";
                        BaseCandy = "D4995F";
                        SecondCandy = "A67860";
                        break;
                    }
                case "Dodrio":
                    {
                        img = "85";
                        BaseCandy = "D4995F";
                        SecondCandy = "A67860";
                        break;
                    }
                case "Seel":
                    {
                        img = "86";
                        BaseCandy = "D6F2FD";
                        SecondCandy = "C9E4F9";
                        break;
                    }
                case "Dewgong":
                    {
                        img = "87";
                        BaseCandy = "D6F2FD";
                        SecondCandy = "C9E4F9";
                        break;
                    }
                case "Grimer":
                    {
                        img = "88";
                        BaseCandy = "C3A6CB";
                        SecondCandy = "5F5370";
                        break;
                    }
                case "Muk":
                    {
                        img = "89";
                        BaseCandy = "C3A6CB";
                        SecondCandy = "5F5370";
                        break;
                    }
                case "Shellder":
                    {
                        img = "90";
                        BaseCandy = "B2A2BD";
                        SecondCandy = "D4B2C3";
                        break;
                    }
                case "Cloyster":
                    {
                        img = "91";
                        BaseCandy = "B2A2BD";
                        SecondCandy = "D4B2C3";
                        break;
                    }
                case "Gastly":
                    {
                        img = "92";
                        BaseCandy = "292728";
                        SecondCandy = "9B7FB7";
                        break;
                    }
                case "Haunter":
                    {
                        img = "93";
                        BaseCandy = "292728";
                        SecondCandy = "9B7FB7";
                        break;
                    }
                case "Gengar":
                    {
                        img = "94";
                        BaseCandy = "292728";
                        SecondCandy = "9B7FB7";
                        break;
                    }
                case "Onix":
                    {
                        img = "95";
                        BaseCandy = "C1C1C1";
                        SecondCandy = "777777";
                        break;
                    }
                case "Drowzee":
                    {
                        img = "96";
                        BaseCandy = "F2CC61";
                        SecondCandy = "B37A4D";
                        break;
                    }
                case "Hypno":
                    {
                        img = "97";
                        BaseCandy = "F2CC61";
                        SecondCandy = "B37A4D";
                        break;
                    }
                case "Krabby":
                    {
                        img = "98";
                        BaseCandy = "F29566";
                        SecondCandy = "EED9CD";
                        break;
                    }
                case "Kingler":
                    {
                        img = "99";
                        BaseCandy = "F29566";
                        SecondCandy = "EED9CD";
                        break;
                    }
                case "Voltorb":
                    {
                        img = "100";
                        BaseCandy = "BF485C";
                        SecondCandy = "F3EEF4";
                        break;
                    }
                case "Electrode":
                    {
                        img = "101";
                        BaseCandy = "BF485C";
                        SecondCandy = "F3EEF4";
                        break;
                    }
                case "Exeggcute":
                    {
                        img = "102";
                        BaseCandy = "F6DEE9";
                        SecondCandy = "FAC9C0";
                        break;
                    }
                case "Exeggutor":
                    {
                        img = "103";
                        BaseCandy = "F6DEE9";
                        SecondCandy = "FAC9C0";
                        break;
                    }
                case "Cubone":
                    {
                        img = "104";
                        BaseCandy = "D7DAE1";
                        SecondCandy = "C4B57C";
                        break;
                    }
                case "Marowak":
                    {
                        img = "105";
                        BaseCandy = "D7DAE1";
                        SecondCandy = "C4B57C";
                        break;
                    }
                case "Hitmonlee":
                    {
                        img = "106";
                        BaseCandy = "C0A28A";
                        SecondCandy = "C0A28A";
                        break;
                    }
                case "Hitmonchan":
                    {
                        img = "107";
                        BaseCandy = "C6B1C0";
                        SecondCandy = "E66444";
                        break;
                    }
                case "Lickitung":
                    {
                        img = "108";
                        BaseCandy = "E6B2BF";
                        SecondCandy = "F6E5DB";
                        break;
                    }
                case "Koffing":
                    {
                        img = "109";
                        BaseCandy = "8A8EB3";
                        SecondCandy = "D5DBB9";
                        break;
                    }
                case "Weezing":
                    {
                        img = "110";
                        BaseCandy = "8A8EB3";
                        SecondCandy = "D5DBB9";
                        break;
                    }
                case "Rhyhorn":
                    {
                        img = "111";
                        BaseCandy = "C1C3C4";
                        SecondCandy = "959CA2";
                        break;
                    }
                case "Rhydon":
                    {
                        img = "112";
                        BaseCandy = "C1C3C4";
                        SecondCandy = "959CA2";
                        break;
                    }
                case "Chansey":
                    {
                        img = "113";
                        BaseCandy = "E5B2B6";
                        SecondCandy = "C68D87";
                        break;
                    }
                case "Tangela":
                    {
                        img = "114";
                        BaseCandy = "646A9A";
                        SecondCandy = "D07296";
                        break;
                    }
                case "Kangaskhan":
                    {
                        img = "115";
                        BaseCandy = "A28D8C";
                        SecondCandy = "E4D6BC";
                        break;
                    }
                case "Horsea":
                    {
                        img = "116";
                        BaseCandy = "B2D8ED";
                        SecondCandy = "D0EFF2";
                        break;
                    }
                case "Seadra":
                    {
                        img = "117";
                        BaseCandy = "B2D8ED";
                        SecondCandy = "D0EFF2";
                        break;
                    }
                case "Goldeen":
                    {
                        img = "118";
                        BaseCandy = "ECEAEB";
                        SecondCandy = "DE8D7A";
                        break;
                    }
                case "Seaking":
                    {
                        img = "119";
                        BaseCandy = "ECEAEB";
                        SecondCandy = "DE8D7A";
                        break;
                    }
                case "Staryu":
                    {
                        img = "120";
                        BaseCandy = "B8986B";
                        SecondCandy = "F5E688";
                        break;
                    }
                case "Starmie":
                    {
                        img = "121";
                        BaseCandy = "B8986B";
                        SecondCandy = "F5E688";
                        break;
                    }
                case "Mr. Mime":
                    {
                        img = "122";
                        BaseCandy = "E579AB";
                        SecondCandy = "FFB3D9";
                        break;
                    }
                case "Scyther":
                    {
                        img = "123";
                        BaseCandy = "8DC983";
                        SecondCandy = "FAFFD3";
                        break;
                    }
                case "Jynx":
                    {
                        img = "124";
                        BaseCandy = "C74653";
                        SecondCandy = "643187";
                        break;
                    }
                case "Electabuzz":
                    {
                        img = "125";
                        BaseCandy = "FEDF73";
                        SecondCandy = "150B14";
                        break;
                    }
                case "Magmar":
                    {
                        img = "126";
                        BaseCandy = "FBD67D";
                        SecondCandy = "E56E4E";
                        break;
                    }
                case "Pinsir":
                    {
                        img = "127";
                        BaseCandy = "C0B5AF";
                        SecondCandy = "D2D0D1";
                        break;
                    }
                case "Tauros":
                    {
                        img = "128";
                        BaseCandy = "DAA65A";
                        SecondCandy = "868072";
                        break;
                    }
                case "Magikarp":
                    {
                        img = "129";
                        BaseCandy = "868072";
                        SecondCandy = "F1ECC4";
                        break;
                    }
                case "Gyarados":
                    {
                        img = "130";
                        BaseCandy = "868072";
                        SecondCandy = "F1ECC4";
                        break;
                    }
                case "Lapras":
                    {
                        img = "131";
                        BaseCandy = "6FA9D7";
                        SecondCandy = "F8EFDE";
                        break;
                    }
                case "Ditto":
                    {
                        img = "132";
                        BaseCandy = "D57399";
                        SecondCandy = "FBF2CB";
                        break;
                    }
                case "Eevee":
                    {
                        img = "133";
                        BaseCandy = "C99364";
                        SecondCandy = "7F4F0F";
                        break;
                    }
                case "Vaporeon":
                    {
                        img = "134";
                        BaseCandy = "C99364";
                        SecondCandy = "7F4F0F";
                        break;
                    }
                case "Jolteon":
                    {
                        img = "135";
                        BaseCandy = "C99364";
                        SecondCandy = "7F4F0F";
                        break;
                    }
                case "Flareon":
                    {
                        img = "136";
                        BaseCandy = "C99364";
                        SecondCandy = "7F4F0F";
                        break;
                    }
                case "Porygon":
                    {
                        img = "137";
                        BaseCandy = "EB7B7A";
                        SecondCandy = "8BB9B9";
                        break;
                    }
                case "Omanyte":
                    {
                        img = "138";
                        BaseCandy = "E1E0D0";
                        SecondCandy = "73CEE2";
                        break;
                    }
                case "Omastar":
                    {
                        img = "139";
                        BaseCandy = "E1E0D0";
                        SecondCandy = "73CEE2";
                        break;
                    }
                case "Kabuto":
                    {
                        img = "140";
                        BaseCandy = "C78726";
                        SecondCandy = "625239";
                        break;
                    }
                case "Kabutops":
                    {
                        img = "141";
                        BaseCandy = "C78726";
                        SecondCandy = "625239";
                        break;
                    }
                case "Aerodactyl":
                    {
                        img = "142";
                        BaseCandy = "D8BDD7";
                        SecondCandy = "B196C5";
                        break;
                    }
                case "Snorlax":
                    {
                        img = "143";
                        BaseCandy = "366B8D";
                        SecondCandy = "DEE3E9";
                        break;
                    }
                case "Articuno":
                    {
                        img = "144";
                        BaseCandy = "CE975F";
                        SecondCandy = "E7F2E3";
                        break;
                    }
                case "Zapdos":
                    {
                        img = "145";
                        BaseCandy = "CE975F";
                        SecondCandy = "E7F2E3";
                        break;
                    }
                case "Moltres":
                    {
                        img = "146";
                        BaseCandy = "CE975F";
                        SecondCandy = "E7F2E3";
                        break;
                    }
                case "Dratini":
                    {
                        img = "147";
                        BaseCandy = "91B3D9";
                        SecondCandy = "EDE8EC";
                        break;
                    }
                case "Dragonair":
                    {
                        img = "148";
                        BaseCandy = "91B3D9";
                        SecondCandy = "EDE8EC";
                        break;
                    }
                case "Dragonite":
                    {
                        img = "149";
                        BaseCandy = "91B3D9";
                        SecondCandy = "EDE8EC";
                        break;
                    }
                case "Mewtwo":
                    {
                        img = "150";
                        BaseCandy = "CE975F";
                        SecondCandy = "E7F2E3";
                        break;
                    }
                case "Mew":
                    {
                        img = "151";
                        BaseCandy = "CE975F";
                        SecondCandy = "E7F2E3";
                        break;
                    }


            }
            return Name;
        }
    }
}
