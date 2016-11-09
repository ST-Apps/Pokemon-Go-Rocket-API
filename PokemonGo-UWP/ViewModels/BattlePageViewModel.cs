using POGOProtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace PokemonGo_UWP.ViewModels
{
    class BattlePageViewModel : ViewModelBase
    {
        #region "Binding vars"
        
        public TeamColor Team1 { get; set; }
        public string Trainer1Name { get; set; }
        public PokemonId Pokemon1Id { get; set; }
        public string Pokemon1Name { get; set; }
        private int _pokemon1Health;
        public int Pokemon1Health {
            get { return _pokemon1Health; }
            set { Set(ref _pokemon1Health, value); }
        }
        public int Pokemon1Cp { get; set; }
        
        public TeamColor Team2 { get; set; }
        public string Trainer2Name { get; set; }
        public PokemonId Pokemon2Id { get; set; }
        public string Pokemon2Name { get; set; }
        private int _pokemon2Health;
        public int Pokemon2Health
        {
            get { return _pokemon2Health; }
            set { Set(ref _pokemon2Health, value); }
        }
        public int Pokemon2Cp { get; set; }

        private int _timerValue;
        public int TimerValue
        {
            get { return _timerValue; }
            set { Set(ref _timerValue, value); }
        }

        private double _chargeAvailableValue;
        public double ChargeAvailableValue
        {
            get { return _chargeAvailableValue; }
            set { Set(ref _chargeAvailableValue, value); }
        }
        public double MaxChargeAvailableValue { get; set; }
        #endregion

        private System.Threading.Timer clockTimer;
        private System.Threading.Timer chargeTimer;

        public BattlePageViewModel()
        {
            Team1 = TeamColor.Blue;
            Trainer1Name = "Trainer1";
            Pokemon1Id = PokemonId.Poliwrath;
            Pokemon1Name = "Pokemon1";
            Pokemon1Health = 100;
            Pokemon1Cp = 300;

            Team2 = TeamColor.Red;
            Trainer2Name = "Trainer2";
            Pokemon2Id = PokemonId.Lickitung;
            Pokemon2Name = "Pokemon2";
            Pokemon2Health = 100;
            Pokemon2Cp = 2000;

            TimerValue = 100;
            clockTimer = new System.Threading.Timer((state) =>
                {
                    TimerValue -= 1;
                    if (TimerValue < 0) TimerValue = 100;
                }, null, 1000, 1000);

            ChargeAvailableValue = 0;
            MaxChargeAvailableValue = 4;
            chargeTimer = new System.Threading.Timer((state) =>
                {
                    var newval = ChargeAvailableValue + 0.1;
                    if (newval >= MaxChargeAvailableValue) newval = MaxChargeAvailableValue;
                    ChargeAvailableValue = newval;
                }, null, 500, 500);
        }

    }
}
