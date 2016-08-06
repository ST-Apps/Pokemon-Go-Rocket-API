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

namespace PokemonGo_UWP.Entities
{
    public class PokemonDataWrapper
    {

        private readonly PokemonData _pokemonData;

        public PokemonDataWrapper(PokemonData pokemonData)
        {
            _pokemonData = pokemonData;            
        }

        private DelegateCommand _gotoEggDetailsCommand;

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

        #region Wrapped Properties

        public ulong Id => _pokemonData.Id;

        public PokemonId PokemonId => _pokemonData.PokemonId;

        public int Cp => _pokemonData.Cp;

        public int Stamina => _pokemonData.Stamina;

        public int StaminaMax => _pokemonData.StaminaMax;        

        public PokemonMove Move1 => _pokemonData.Move1;

        public PokemonMove Move2 => _pokemonData.Move2;

        public string DeployedFortId => _pokemonData.DeployedFortId;

        public string OwnerName => _pokemonData.OwnerName;

        public bool IsEgg => _pokemonData.IsEgg;

        public double EggKmWalkedTarget => _pokemonData.EggKmWalkedTarget;

        public double EggKmWalkedStart => _pokemonData.EggKmWalkedStart;

        public int Origin => _pokemonData.Origin;

        public float HeightM => _pokemonData.HeightM;

        public float WeightKg => _pokemonData.WeightKg;

        public int IndividualAttack => _pokemonData.IndividualAttack;

        public int IndividualDefense => _pokemonData.IndividualDefense;

        public int IndividualStamina => _pokemonData.IndividualStamina;

        public float CpMultiplier => _pokemonData.CpMultiplier;

        public ItemId Pokeball => _pokemonData.Pokeball;

        public ulong CapturedCellId => _pokemonData.CapturedCellId;

        public int BattlesAttacked => _pokemonData.BattlesAttacked;

        public int BattlesDefended => _pokemonData.BattlesDefended;

        public string EggIncubatorId => _pokemonData.EggIncubatorId;

        public ulong CreationTimeMs => _pokemonData.CreationTimeMs;

        public int NumUpgrades => _pokemonData.NumUpgrades;

        public float AdditionalCpMultiplier => _pokemonData.AdditionalCpMultiplier;

        public int Favorite => _pokemonData.Favorite;

        public string Nickname => _pokemonData.Nickname;

        public int FromFort => _pokemonData.FromFort;


        #endregion

    }
}
