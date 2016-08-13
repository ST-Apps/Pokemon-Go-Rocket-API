using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Settings.Master;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonDetailPageViewModel : ViewModelBase
    {

        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter">MapPokemonWrapper containing the Pokemon that we're trying to capture</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {

            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentPokemon = (PokemonDataWrapper)suspensionState[nameof(CurrentPokemon)];
                PlayerProfile = (PlayerData)suspensionState[nameof(PlayerProfile)];
            }
            else
            {
                // Navigating from game page, so we need to actually load the encounter                
                CurrentPokemon = (PokemonDataWrapper)NavigationHelper.NavigationState[nameof(CurrentPokemon)];
                var baseData = PokemonInfo.GetBaseStats(CurrentPokemon.PokemonId);
                Busy.SetBusy(true, CurrentPokemon.PokemonId.ToString() + " is getting ready!");
                PlayerProfile = GameClient.PlayerProfile;
                try
                {
                    starDust = PlayerProfile.Currencies.First(item => item.Name.Equals("STARDUST")).Amount;
                }
                catch (Exception e1)
                {
                    MessageDialog mes = new MessageDialog(e1.Message);
                    await mes.ShowAsync();
                }

                var myPokemonSettings = await GetPokemonSettings();
                var pokemonSettings = myPokemonSettings.ToList();
                var settings = pokemonSettings.SingleOrDefault(x => x.PokemonId == CurrentPokemon.PokemonId);
                CandyToEvolve = settings.CandyToEvolve;

                var myPokemonFamilies = await GameClient.GetPokemonFamilies();
                var pokemonFamilies = myPokemonFamilies.ToArray();

                var familyCandy = pokemonFamilies.SingleOrDefault(x => settings.FamilyId == x.FamilyId);
                CandyFam = familyCandy.Candy_;
                candyfamname = familyCandy.FamilyId.ToString();
                CandyName = candyfamname.ToUpper();
                string[] familySplit = { "FAMILY" };
                string[] familySplitted = CandyName.Split(familySplit, StringSplitOptions.None);
                CandyName = familySplitted[1];
                if (CandyToEvolve == 0)
                {
                    evolveBut = Visibility.Collapsed;
                }
                else
                {
                    evolveBut = Visibility.Visible;
                }

                //Get cost of the power up(candy, stardust)
                try
                {
                    int lvl = Convert.ToInt32(Math.Round(Utils.PokemonInfo.GetLevel(CurrentPokemon.WrappedData)));
                    lvl = lvl - 1;
                    var upgradeCost = GameClient.PokemonUpgradeCosts[lvl];
                    var candyCost = upgradeCost[0];
                    var stardustCost = upgradeCost[1];
                    CandyCost = (int)candyCost;
                    StarCost = (int)stardustCost;
                }
                catch (Exception e1)
                {

                    MessageDialog mes = new MessageDialog(e1.Message);
                    await mes.ShowAsync();
                }
                Debug.WriteLine(PokemonGo_UWP.Utils.PokemonInfo.GetLevel(CurrentPokemon.WrappedData));
                Debug.WriteLine(GameClient.PlayerStats.Level + 1.5);
                Busy.SetBusy(false);
                //NavigationHelper.NavigationState.Remove(nameof(CurrentEgg));
                //Logger.Write($"Catching {CurrentEgg.PokemonId}");
                //CurrentEncounter = await GameClient.EncounterPokemon(CurrentEgg.EncounterId, CurrentEgg.SpawnpointId);
                //SelectedEggIncubator = ItemsInventory.First(item => item.ItemId == ItemId.ItemPokeBall);
                //Busy.SetBusy(false);
                //if (CurrentEncounter.Status != EncounterResponse.Types.Status.EncounterSuccess)
                //{
                //    // Encounter failed, probably the Pokemon ran away
                //    await new MessageDialog(Utils.Resources.Translation.GetString("PokemonRanAway")).ShowAsyncQueue();
                //    ReturnToPokemonInventoryScreen.Execute();
                //}
            }
            await Task.CompletedTask;

        }
        public class Example
        {
            [JsonProperty("name")]
            public string name { get; set; }
            [JsonProperty("amount")]
            public int amount { get; set; }
        }


        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
            var templates = await GameClient.GetItemTemplates();
            return
                templates.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p.FamilyId != PokemonFamilyId.FamilyUnset);
        }
        /// <summary>
        /// Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(CurrentPokemon)] = CurrentPokemon;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Game Management Vars

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        private PokemonDataWrapper _currentEgg;

        /// <summary>
        ///     Current item for capture page
        /// </summary>

        #endregion

        #region Bindable Game Vars
        private PlayerData _playerProfile;
        private int _starDust;
        private int _candyToEvolve;
        private string candyfamname;
        private int candyFam;
        private int _candyCost;
        private int _starCost;

        public string _url;


        public PokemonId EvolvePokemonId { get; set; }

        public string Url
        {
            get { return _url; }
            set { Set(ref _url, value); }
        }
        /// <summary>
        /// Reference to global inventory
        /// </summary>

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public PokemonDataWrapper CurrentPokemon
        {
            get { return _currentEgg; }
            set { Set(ref _currentEgg, value); }
        }
        public int starDust
        {
            get { return _starDust; }
            set { Set(ref _starDust, value); }
        }
        public string CandyName
        {
            get { return candyfamname; }
            set { Set(ref candyfamname, value); }
        }
        public int CandyFam
        {
            get { return candyFam; }
            set { Set(ref candyFam, value); }
        }
        public int CandyToEvolve
        {
            get { return _candyToEvolve; }
            set { Set(ref _candyToEvolve, value); }
        }
        public int CandyCost
        {
            get { return _candyCost; }
            set { Set(ref _candyCost, value); }
        }
        public int StarCost
        {
            get { return _starCost; }
            set { Set(ref _starCost, value); }
        }
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }
        public Visibility evolveBut
        {
            get { return _evolve; }
            set { Set(ref _evolve, value); }
        }
        private Visibility _evolve;

        /// <summary>
        ///     Current item for capture page
        /// </summary>


        #endregion

        #region commands

        public DelegateCommand _TappEvent;
        public DelegateCommand TappEvent => _TappEvent ?? (_TappEvent = new DelegateCommand(async () =>
        {

            var resp = await GameClient.evolvePokemon(CurrentPokemon.WrappedData);

            if (resp.Status == true)
            {

            }
            else
            {
                MessageDialog mes = new MessageDialog("Failed", CurrentPokemon.PokemonId.ToString());
                await mes.ShowAsync();
            }
        }, () => true));


        public DelegateCommand _TappEvent1;
        public DelegateCommand TappEvent1 => _TappEvent1 ?? (_TappEvent1 = new DelegateCommand(async () =>
        {
            try
            {


                MessageDialog mes = new MessageDialog("trying to powerup a: " + CurrentPokemon.PokemonId + " Available candy: ", CurrentPokemon.WrappedData.Id.ToString());
                await mes.ShowAsync();
                var ok = await GameClient.Powerup(CurrentPokemon.WrappedData.Id);

                MessageDialog mes1 = new MessageDialog("awards: none " + " base data: " + ok.UpgradedPokemon.Cp);
                await mes1.ShowAsync();
            }
            catch (Exception e1)
            {
                MessageDialog mes1 = new MessageDialog(e1.Message);
                await mes1.ShowAsync();
            }

        }, () => true));


        #endregion

        #region Game Logic
        DelegateCommand _TransferEvent;
        public DelegateCommand TransferEvent => _TransferEvent ?? (
          _TransferEvent = new DelegateCommand(async () =>
          {
              var pk = CurrentPokemon;
              var mes = await GameClient.TransferPokemon(pk.Id);
              MessageDialog mes1 = new MessageDialog(mes.Result.ToString());
              await mes1.ShowAsync();
              await GameClient.UpdateInventory();
              NavigationHelper.NavigationState["CurrentPokemon"] = CurrentPokemon;
              BootStrapper.Current.NavigationService.Navigate(typeof(PokemonDetailPage), true);
          }, () => true));

        #region Shared Logic

        private DelegateCommand _returnToPokemonInventoryScreen;

        /// <summary>
        ///     Going back to inventory page
        /// </summary>
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                NavigationService.Navigate(typeof(PokemonInventoryPage));
            }, () => true)
            );

        #endregion

        #endregion

    }
}
