using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils
{
   public static class getCandy
    {

        public static string getCandyUrl(string pokemonType)
        {
            string candyUrl = "";
            switch (pokemonType)
            {
                case "Fire":
                candyUrl = "https://pokemon.agne.no/candyMaker.php?base=E54C3C&secondary=f8f8f8&zoom=1";
                    break;
                case "Electric":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=FFD742&secondary=f8f8f8&zoom=1";
                    break;
                case "Ground":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=DABB51&secondary=f8f8f8&zoom=1";
                    break;
                case "Fighting":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=B36A57&secondary=f8f8f8&zoom=1";
                    break;
                case "Normal":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=AFB09E&secondary=f8f8f8&zoom=1";
                    break;
                case "Poison":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=9A599B&secondary=f8f8f8&zoom=1";
                    break;
                case "Psyhic":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=DE5FA4&secondary=f8f8f8&zoom=1";
                    break;
                case "Default":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=83C44C&secondary=f8f8f8&zoom=1";
                    break;
                case "Ice":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=87DBFD&secondary=f8f8f8&zoom=1";
                    break;
                case "Rock":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=BBAD62&secondary=f8f8f8&zoom=1";
                    break;
                case "Dragon":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=776CE2&secondary=f8f8f8&zoom=1";
                    break;
                case "Water":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=47A1FF&secondary=f8f8f8&zoom=1";
                    break;
                case "Bug":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=B1BF21&secondary=f8f8f8&zoom=1";
                    break;
                case "Dark":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=81604D&secondary=f8f8f8&zoom=1";
                    break;
                case "Ghost":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=6D6EC1&secondary=f8f8f8&zoom=1";
                    break;
                case "Steel":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=B0AFC1&secondary=f8f8f8&zoom=1";
                    break;
                case "Flying":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=6B95F7&secondary=f8f8f8&zoom=1";
                    break;
                case "Fairy":
                    candyUrl = "https://pokemon.agne.no/candyMaker.php?base=E59DE7&secondary=f8f8f8&zoom=1";
                    break;
                 
            }
            return candyUrl;
        }
    }
}
