using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Enums;
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
            if (PokemonGo_UWP.Utils.PokemonInfo.GetLevel(WrappedData) > GameClient.PlayerStats.Level + 1.5)
            {
                CpBarMax = GameClient.PlayerStats.Level + 1.5;
                CpBarValue = CpBarMax;
            }
            else
            {
                CpBarMax = CpBarMax = GameClient.PlayerStats.Level + 1.5;
                CpBarValue = PokemonGo_UWP.Utils.PokemonInfo.GetLevel(WrappedData);
            }
        }

        public PokemonData WrappedData { get; }

        /// <summary>
        ///     Navigate to detail page for the selected egg
        /// </summary>
        public DelegateCommand GotoEggDetailsCommand => _gotoEggDetailsCommand ?? (
            _gotoEggDetailsCommand = new DelegateCommand(() =>
            {
                NavigationHelper.NavigationState["CurrentEgg"] = this;
                BootStrapper.Current.NavigationService.Navigate(typeof(EggDetailPage), true);
            }, () => true));

        /// <summary>
        ///     Go to Pokemon Details
        /// </summary>
        public DelegateCommand GotoPokemonDetailsCommand => _gotoPokemonDetailsCommand ?? (
         _gotoPokemonDetailsCommand = new DelegateCommand(() =>
         {
             NavigationHelper.NavigationState["CurrentPokemon"] = this;
             BootStrapper.Current.NavigationService.Navigate(typeof(PokemonView), true);
         }, () => true));

        #region Wrapped Properties

        public PokemonId PokemonId => WrappedData.PokemonId;

        public double CpBarMax { get; set; }

        public double CpBarValue { get; set; }

        public int Cp => WrappedData.Cp;

        public int Stamina => WrappedData.Stamina;

        public int StaminaMax => WrappedData.StaminaMax;

        public PokemonMove Move1 => WrappedData.Move1;

        public PokemonMove Move2 => WrappedData.Move2;

        public ulong Id => WrappedData.Id;

        public string DeployedFortId => WrappedData.DeployedFortId;

        public string OwnerName => WrappedData.OwnerName;

        public bool IsEgg => WrappedData.IsEgg;

        public double EggKmWalkedTarget => WrappedData.EggKmWalkedTarget;

        public double EggKmWalkedStart => WrappedData.EggKmWalkedStart;

        public int Origin => WrappedData.Origin;

        public string HeightM => WrappedData.HeightM.ToString().Remove(4);

        char[] chars = { '.' };


        public string[] WeightKg1 => WrappedData.WeightKg.ToString().Split(chars);

        public string WeightKg2 => WeightKg1[1];
        public string WeightKg3 => WeightKg2.Remove(2);
        public string WeightKg => WeightKg1[0] + "." + WeightKg3;

        public int IndividualAttack => WrappedData.IndividualAttack;

        public int IndividualDefense => WrappedData.IndividualDefense;

        public int IndividualStamina => WrappedData.IndividualStamina;

        public float CpMultiplier => WrappedData.CpMultiplier;

        public ItemId Pokeball => WrappedData.Pokeball;

        public ulong CapturedCellId => WrappedData.CapturedCellId;

        public int BattlesAttacked => WrappedData.BattlesAttacked;

        public int BattlesDefended => WrappedData.BattlesDefended;

        public string EggIncubatorId => WrappedData.EggIncubatorId;
        public string PokemonNamee => WrappedData.PokemonId.ToString();

        public ulong CreationTimeMs => WrappedData.CreationTimeMs;

        public int NumUpgrades => WrappedData.NumUpgrades;

        public float AdditionalCpMultiplier => WrappedData.AdditionalCpMultiplier;

        public int Favorite => WrappedData.Favorite;

        public string candyUrl => getCandy.getCandyUrl(type);

        public string Nickname => WrappedData.Nickname;

        public string bgType => "ms-appx:///Assets/Backgrounds/details_type_bg_" + enumType.getType(WrappedData.PokemonId.ToString()) + ".png";

        public string hpText => "HP " + WrappedData.Stamina + "/" + WrappedData.StaminaMax;

        public string type => enumType.getType(WrappedData.PokemonId.ToString());


        public int FromFort => WrappedData.FromFort;


        #endregion

    }
}