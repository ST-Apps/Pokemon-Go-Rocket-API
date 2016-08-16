using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;

namespace PokemonGo_UWP.Entities
{
    public class PokemonDataWrapper
    {
        private DelegateCommand _gotoEggDetailsCommand;
        private DelegateCommand _gotoPokemonDetailsCommand;

        public PokemonDataWrapper(PokemonData pokemonData)
        {
            WrappedData = pokemonData;
        }

        public PokemonData WrappedData { get; }

        /// <summary>
        /// The file name for this Pokemen, located in /Assets/Pokemons
        /// </summary>
        public string ImageFileName => $"{(int)PokemonId}.png";

        /// <summary>
        ///     Navigate to detail page for the selected egg
        /// </summary>
        public DelegateCommand GotoEggDetailsCommand => _gotoEggDetailsCommand ?? (
            _gotoEggDetailsCommand = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentEgg"] = this;
                BootStrapper.Current.NavigationService.Navigate(typeof(EggDetailPage));
            }, () => true));

        /// <summary>
        ///     Navigate to detail page for the selected Pokemon
        /// </summary>
        public DelegateCommand GotoPokemonDetailsCommand => _gotoPokemonDetailsCommand ?? (
            _gotoPokemonDetailsCommand = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentPokemon"] = this;
                BootStrapper.Current.NavigationService.Navigate(typeof(PokemonDetailPage));
            }, () => true));

        #region Wrapped Properties

        public ulong Id => WrappedData.Id;

        public PokemonId PokemonId => WrappedData.PokemonId;

        public int Cp => WrappedData.Cp;

        public int Stamina => WrappedData.Stamina;

        public int StaminaMax => WrappedData.StaminaMax;

        public PokemonMove Move1 => WrappedData.Move1;

        public PokemonMove Move2 => WrappedData.Move2;

        public string DeployedFortId => WrappedData.DeployedFortId;

        public string OwnerName => WrappedData.OwnerName;

        public bool IsEgg => WrappedData.IsEgg;

        public double EggKmWalkedTarget => WrappedData.EggKmWalkedTarget;

        public double EggKmWalkedStart => WrappedData.EggKmWalkedStart;

        public int Origin => WrappedData.Origin;

        public float HeightM => WrappedData.HeightM;

        public float WeightKg => WrappedData.WeightKg;

        public int IndividualAttack => WrappedData.IndividualAttack;

        public int IndividualDefense => WrappedData.IndividualDefense;

        public int IndividualStamina => WrappedData.IndividualStamina;

        public float CpMultiplier => WrappedData.CpMultiplier;

        public ItemId Pokeball => WrappedData.Pokeball;

        public ulong CapturedCellId => WrappedData.CapturedCellId;

        public int BattlesAttacked => WrappedData.BattlesAttacked;

        public int BattlesDefended => WrappedData.BattlesDefended;

        public string EggIncubatorId => WrappedData.EggIncubatorId;

        public ulong CreationTimeMs => WrappedData.CreationTimeMs;

        public int NumUpgrades => WrappedData.NumUpgrades;

        public float AdditionalCpMultiplier => WrappedData.AdditionalCpMultiplier;

        public int Favorite => WrappedData.Favorite;

        public string Nickname => WrappedData.Nickname;

        public int FromFort => WrappedData.FromFort;

        #endregion
    }

    public class IncubatedEggDataWrapper : PokemonDataWrapper
    {
        private readonly EggIncubator _incubatorData;
        private readonly double playerWalkedKm;
        public IncubatedEggDataWrapper(EggIncubator incubatorData, double playerWalkedKm, PokemonData incubatedEgg) : base(incubatedEgg)
        {
            _incubatorData = incubatorData;
            this.playerWalkedKm = playerWalkedKm;
        }

        public new double EggKmWalkedStart => playerWalkedKm - _incubatorData.StartKmWalked;

        //public new double EggKmWalkedTarget => _incubatorData.TargetKmWalked;
    }
}