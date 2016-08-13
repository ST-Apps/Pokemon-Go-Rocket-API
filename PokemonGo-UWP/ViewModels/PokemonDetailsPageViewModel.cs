using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Settings.Master;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonDetailsPageViewModel : ViewModelBase
    {

        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter">MapPokemonWrapper containing the Pokemon that we're trying to capture</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentPokemon = (PokemonDataWrapper)suspensionState[nameof(CurrentPokemon)];
                PlayerProfile = (PlayerData)suspensionState[nameof(PlayerProfile)];
            }
            else
            {
                // Navigating from inventory page so we need to load the pokemon               
                CurrentPokemon = (PokemonDataWrapper)NavigationHelper.NavigationState[nameof(CurrentPokemon)];
                // Retrieve data
                PlayerProfile = GameClient.PlayerProfile;                
                StardustAmount = PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount;
                PokemonExtraData = GameClient.PokedexExtraData.FirstOrDefault(item => item.PokemonId == CurrentPokemon.PokemonId);
                CurrentCandy = GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == PokemonExtraData.FamilyId);
            }
            await Task.CompletedTask;
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
                suspensionState[nameof(PlayerProfile)] = PlayerProfile;
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
        private PokemonDataWrapper _currentPokemon;

        /// <summary>
        /// Data for the current user
        /// </summary>
        private PlayerData _playerProfile;

        /// <summary>
        /// Pokedex data for the current Pokemon
        /// </summary>
        private PokemonSettings _pokemonExtraData;

        /// <summary>
        /// Amount of Stardust owned by the player
        /// </summary>
        private int _stardustAmount;

        /// <summary>
        /// Candies needed to evolve the Pokemon
        /// </summary>
        private int _candiesToEvolve;

        /// <summary>
        /// Candy type for the current Pokemon
        /// </summary>
        private Candy _currentCandy;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public PokemonDataWrapper CurrentPokemon
        {
            get { return _currentPokemon; }
            set { Set(ref _currentPokemon, value); }
        }

        /// <summary>
        /// Data for the current user
        /// </summary>
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        /// Pokedex data for the current Pokemon
        /// </summary>
        public PokemonSettings PokemonExtraData
        {
            get { return _pokemonExtraData; }
            set { Set(ref _pokemonExtraData, value); }
        }

        /// <summary>
        /// Amount of Stardust owned by the player
        /// </summary>
        public int StardustAmount
        {
            get { return _stardustAmount; }
            set { Set(ref _stardustAmount, value); }
        }

        /// <summary>
        /// Candies needed to evolve the Pokemon
        /// </summary>
        public int CandiesToEvolve
        {
            get { return _candiesToEvolve; }
            set { Set(ref _candiesToEvolve, value); }
        }

        /// <summary>
        /// Candy type for the current Pokemon
        /// </summary>
        public Candy CurrentCandy
        {
            get { return _currentCandy; }
            set { Set(ref _currentCandy, value); }
        }

        #endregion

        #region Game Logic

        private DelegateCommand _transferPokemonCommand;

        public DelegateCommand TransferEvent => _transferPokemonCommand ?? (
          _transferPokemonCommand = new DelegateCommand(() =>
          {
              //var pk = CurrentPokemon;
              //var mes = await GameClient.TransferPokemon(pk.Id);
              //MessageDialog mes1 = new MessageDialog(mes.Result.ToString());
              //await mes1.ShowAsync();
              //await GameClient.UpdateInventory();
              //NavigationHelper.NavigationState["CurrentPokemon"] = CurrentPokemon;
              //BootStrapper.Current.NavigationService.Navigate(typeof(PokemonView), true);
          }, () => true));

        #region Shared Logic

        private DelegateCommand _returnToPokemonInventoryScreen;

        /// <summary>
        ///     Going back to inventory page
        /// </summary>
        public DelegateCommand ReturnToPokemonInventoryScreen => _returnToPokemonInventoryScreen ?? (
            _returnToPokemonInventoryScreen = new DelegateCommand(() =>
            {
                NavigationService.GoBack();
            }, () => true)
            );

        #endregion

        #endregion

    }
}
