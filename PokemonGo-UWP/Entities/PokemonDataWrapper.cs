using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using PokemonGo_UWP.Assets.Pokemons;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace PokemonGo_UWP.Entities
{
    public class PokemonDataWrapper
    {

        private readonly PokemonData _pokemonData;

        public PokemonDataWrapper(PokemonData pokemonData)
        {
            _pokemonData = pokemonData;
         
            if (PokemonGo_UWP.Utils.PokemonInfo.GetLevel(_pokemonData) > GameClient.playerStats.Level + 1.5)
            {
                CpBarMax = GameClient.playerStats.Level + 1.5;
                CpBarValue = CpBarMax;
            }
            else
            {
                CpBarMax = CpBarMax = GameClient.playerStats.Level + 1.5;
                CpBarValue = PokemonGo_UWP.Utils.PokemonInfo.GetLevel(_pokemonData);
            }
            EvolvePokemonId = _pokemonData.PokemonId;
            EvolvePokemonId++;

        }

        private DelegateCommand _gotoEggDetailsCommand;
        private DelegateCommand _gotoPokemonDetails;
        public PokemonData WrappedData => _pokemonData;
      

        

        /// <summary>
        ///     Navigate to detail page for the selected egg
        /// </summary>
        public DelegateCommand GotoEggDetailsCommand => _gotoEggDetailsCommand ?? (
            _gotoEggDetailsCommand = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentEgg"] = this;
                BootStrapper.Current.NavigationService.Navigate(typeof(EggDetailPage), true);
            }, () => true));



        public DelegateCommand GotoPokemonDetailsCommand => _gotoPokemonDetails ?? (
           _gotoPokemonDetails = new DelegateCommand(() =>
           {
               NavigationHelper.NavigationState["CurrentPokemon"] = this;
               BootStrapper.Current.NavigationService.Navigate(typeof(PokemonView), true);
           }, () => true));

        #region Wrapped Properties

        public PokemonId PokemonId => _pokemonData.PokemonId;

        public PokemonId EvolvePokemonId { get; set; }
       
        public double CpBarMax { get; set; }
        public double CpBarValue { get; set; }

        public int Cp => _pokemonData.Cp;

        public int Stamina => _pokemonData.Stamina;

        public int StaminaMax => _pokemonData.StaminaMax;

        public PokemonMove Move1 => _pokemonData.Move1;

        public PokemonMove Move2 => _pokemonData.Move2;

        public ulong Id => _pokemonData.Id;

        public string DeployedFortId => _pokemonData.DeployedFortId;

        public string OwnerName => _pokemonData.OwnerName;

        public bool IsEgg => _pokemonData.IsEgg;

        public double EggKmWalkedTarget => _pokemonData.EggKmWalkedTarget;

        public double EggKmWalkedStart => _pokemonData.EggKmWalkedStart;

        public int Origin => _pokemonData.Origin;

        public string HeightM => _pokemonData.HeightM.ToString().Remove(4);

        char[] chars = { '.' };


        public string[] WeightKg1 => _pokemonData.WeightKg.ToString().Split(chars);

        public string WeightKg2 => WeightKg1[1];
        public string WeightKg3 => WeightKg2.Remove(2);
        public string WeightKg => WeightKg1[0] + "." + WeightKg3;

        public int IndividualAttack => _pokemonData.IndividualAttack;

        public int IndividualDefense => _pokemonData.IndividualDefense;

        public int IndividualStamina => _pokemonData.IndividualStamina;

        public float CpMultiplier => _pokemonData.CpMultiplier;

        public ItemId Pokeball => _pokemonData.Pokeball;

        public ulong CapturedCellId => _pokemonData.CapturedCellId;

        public int BattlesAttacked => _pokemonData.BattlesAttacked;

        public int BattlesDefended => _pokemonData.BattlesDefended;

        public string EggIncubatorId => _pokemonData.EggIncubatorId;
        public string PokemonNamee => _pokemonData.PokemonId.ToString();

        public ulong CreationTimeMs => _pokemonData.CreationTimeMs;

        public int NumUpgrades => _pokemonData.NumUpgrades;

        public float AdditionalCpMultiplier => _pokemonData.AdditionalCpMultiplier;

        public int Favorite => _pokemonData.Favorite;

        public string candyUrl => getCandy.getCandyUrl(type);

        public string Nickname => _pokemonData.Nickname;

        public string bgType => "ms-appx:///Assets/Backgrounds/details_type_bg_" + enumType.getType(_pokemonData.PokemonId.ToString()) + ".png";

        public string hpText => "HP " + _pokemonData.Stamina + "/" + _pokemonData.StaminaMax;

        public string type => enumType.getType(_pokemonData.PokemonId.ToString());


        public int FromFort => _pokemonData.FromFort;


        #endregion

    }
}
