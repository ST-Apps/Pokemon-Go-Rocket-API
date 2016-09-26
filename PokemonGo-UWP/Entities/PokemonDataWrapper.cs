using Newtonsoft.Json;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;
using System.ComponentModel;

namespace PokemonGo_UWP.Entities
{
    public class PokemonDataWrapper : INotifyPropertyChanged
    {
        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        private PokemonData _wrappedData;

        public PokemonDataWrapper(PokemonData pokemonData)
        {
            _wrappedData = pokemonData;
        }


        public PokemonData WrappedData => _wrappedData;

        /// <summary>
        /// The file name for this Pokemon, located in /Assets/Pokemons
        /// </summary>
        public string ImageFileName => $"{(int)PokemonId}.png";

        /// <summary>
        /// The file name for this Pokemon, located in /Assets/Pokemons
        /// </summary>
        public string ImageFilePath => $"ms-appx:///Assets/Pokemons/{(int)PokemonId}.png";

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

        /// <summary>
        /// HACK - ProgressBar requires a two-way binding for value, so we need this workaround
        /// </summary>
        public double EggKmWalkedStart {get { return WrappedData.EggKmWalkedStart; } set {} } 

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

        // Stubb Var
        public bool IsBuddy = false;

        public bool IsDeployed { get { return !string.IsNullOrEmpty(DeployedFortId) || IsBuddy; } }

        public int Favorite
        {
            get { return WrappedData.Favorite; }
            set
            {
                if (WrappedData.Favorite == value) return;
                WrappedData.Favorite = value;
                OnPropertyChanged(nameof(Favorite));
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        public bool IsFavorite => System.Convert.ToBoolean(Favorite);

        public string Nickname
        {
            get { return WrappedData.Nickname; }
            set
            {
                if (WrappedData.Nickname == value) return;
                WrappedData.Nickname = value;
                OnPropertyChanged(nameof(Nickname));
                OnPropertyChanged(nameof(Name));
            }
        }

        public int FromFort => WrappedData.FromFort;

        public string Name { get { return WrappedData.Nickname == "" ? Resources.Pokemon.GetString(WrappedData.PokemonId.ToString()) : WrappedData.Nickname; } }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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