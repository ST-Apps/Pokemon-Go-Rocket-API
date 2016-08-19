using POGOProtos.Data;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonDetailsPageViewModel : ViewModelBase
    {

        public PokemonDetailsPageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var pokeData = new PokemonData
                {
                    PokemonId = PokemonId.Abra,
                    Cp = 10,
                    Stamina = 800,
                    StaminaMax = 1000,
                    WeightKg = 12,
                    BattlesAttacked = 5
                    
                };
                CurrentPokemon = new PokemonDataWrapper(pokeData);
                StardustAmount = 18000;
                StardustToPowerUp = 1800;
                CandiesToPowerUp = 100;
                CurrentCandy = new Candy
                {
                    FamilyId = PokemonFamilyId.FamilyAbra,
                    Candy_ = 10
                };
            }
        }

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
                // TODO: ulong needed
                CurrentPokemon = JsonConvert.DeserializeObject<PokemonDataWrapper>((string)suspensionState[nameof(CurrentPokemon)]);
                PlayerProfile = JsonConvert.DeserializeObject<PlayerData>((string)suspensionState[nameof(PlayerProfile)]);
            }
            else
            {
                // Navigating from inventory page so we need to load the pokemon
                CurrentPokemon = (PokemonDataWrapper)NavigationHelper.NavigationState[nameof(CurrentPokemon)];
                UpdateCurrentData();
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
                suspensionState[nameof(CurrentPokemon)] = JsonConvert.SerializeObject(CurrentPokemon);
                suspensionState[nameof(PlayerProfile)] = JsonConvert.SerializeObject(PlayerProfile);
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
        /// Current Pokemons favorite status
        /// </summary>
        private bool _isFavorite;

        /// <summary>
        /// Amount of Stardust owned by the player
        /// </summary>
        private int _stardustAmount;

        /// <summary>
        /// Candies needed to powerup the Pokemon
        /// </summary>
        private int _candiesToPowerUp;

        /// <summary>
        /// Stardust needed to evolve the Pokemon
        /// </summary>
        private int _stardustToPowerUp;

        /// <summary>
        /// Candy type for the current Pokemon
        /// </summary>
        private Candy _currentCandy;

        /// <summary>
        /// Result of Pokemon evolution
        /// </summary>
        private EvolvePokemonResponse _evolvePokemonResponse;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Key for Bing's Map Service (not included in GIT, you need to get your own token to use maps!)
        /// </summary>
        public string MapServiceToken => ApplicationKeys.MapServiceToken;

        public ElementTheme CurrentTheme
        {
            get
            {
                // Set theme
                var currentTime = int.Parse(DateTime.Now.ToString("HH"));
                return currentTime > 7 && currentTime < 19 ? ElementTheme.Light : ElementTheme.Dark;
            }
        }

        /// <summary>
        ///     Pokemon that we're trying to capture
        /// </summary>
        public PokemonDataWrapper CurrentPokemon
        {
            get { return _currentPokemon; }
            set
            {
                Set(ref _currentPokemon, value);
                RaisePropertyChanged(() => EvolvedPokemonId);
            }
        }

        /// <summary>
        /// Id for current pokemon's evolution
        /// </summary>
        public PokemonId EvolvedPokemonId => EvolvePokemonResponse?.EvolvedPokemonData.PokemonId ?? PokemonId.Missingno;

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
        /// Current Pokemons favorite status
        /// </summary>
        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { Set(ref _isFavorite, value); }
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
        /// Candies needed to powerup the Pokemon
        /// </summary>
        public int CandiesToPowerUp
        {
            get { return _candiesToPowerUp; }
            set
            {
                Set(ref _candiesToPowerUp, value);
                PowerUpPokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Stardust needed to evolve the Pokemon
        /// </summary>
        public int StardustToPowerUp
        {
            get { return _stardustToPowerUp; }
            set
            {
                Set(ref _stardustToPowerUp, value);
                PowerUpPokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Candy type for the current Pokemon
        /// </summary>
        public Candy CurrentCandy
        {
            get { return _currentCandy; }
            set
            {
                Set(ref _currentCandy, value);
                EvolvePokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Result of Pokemon evolution
        /// </summary>
        public EvolvePokemonResponse EvolvePokemonResponse
        {
            get { return _evolvePokemonResponse; }
            set
            {
                Set(ref _evolvePokemonResponse, value);
                EvolvePokemonCommand.RaiseCanExecuteChanged();
            }
        }


        #endregion

        #region Game Logic

        #region Shared Logic

        /// <summary>
        /// Updates data related to current Pokemon
        /// </summary>
        private void UpdateCurrentData()
        {
            // Retrieve data
            PlayerProfile = GameClient.PlayerProfile;
            IsFavorite = Convert.ToBoolean(CurrentPokemon.Favorite);
            StardustAmount = PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount;
            var upgradeCosts =
                GameClient.PokemonUpgradeCosts[
                    Convert.ToInt32(Math.Round(PokemonInfo.GetLevel(CurrentPokemon.WrappedData)) - 1)];
            CandiesToPowerUp = Convert.ToInt32(upgradeCosts[0]);
            StardustToPowerUp = Convert.ToInt32(upgradeCosts[1]);
            PokemonExtraData = GameClient.GetExtraDataForPokemon(CurrentPokemon.PokemonId);
            CurrentCandy = GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == PokemonExtraData.FamilyId);
            RaisePropertyChanged(() => PokemonExtraData);
            PowerUpPokemonCommand.RaiseCanExecuteChanged();
            EvolvePokemonCommand.RaiseCanExecuteChanged();
        }

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

        #region Transfer

        private DelegateCommand _transferPokemonCommand;

        public DelegateCommand TransferPokemonCommand => _transferPokemonCommand ?? (
          _transferPokemonCommand = new DelegateCommand(async () =>
          {
              // Ask for confirmation before moving the Pokemon
              // TODO: better style maybe?
              var dialog =
                  new MessageDialog(string.Format(Resources.CodeResources.GetString("TransferPokemonWarningText"),
                      Resources.Pokemon.GetString(CurrentPokemon.PokemonId.ToString())));
              dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("YesText")) { Id = 0 });
              dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("NoText")) { Id = 1 });
              dialog.DefaultCommandIndex = 0;
              dialog.CancelCommandIndex = 1;
              // User canceled transfer
              if ((int)(await dialog.ShowAsyncQueue()).Id == 1) return;
              // User confirmed transfer
              var pokemonTransferResponse = await GameClient.TransferPokemon(CurrentPokemon.Id);
              switch (pokemonTransferResponse.Result)
              {
                  case ReleasePokemonResponse.Types.Result.Unset:
                      break;
                  case ReleasePokemonResponse.Types.Result.Success:
                      await
                          new MessageDialog(
                              string.Format(Resources.CodeResources.GetString("TransferPokemonSuccessText"),
                                  Resources.Pokemon.GetString(CurrentCandy.FamilyId.ToString().Replace("Family", "")))).ShowAsyncQueue();
                      await GameClient.UpdateInventory();
                      await GameClient.UpdatePlayerStats();
                      NavigationService.GoBack();
                      break;
                  // TODO: what to do on error?
                  case ReleasePokemonResponse.Types.Result.PokemonDeployed:
                      break;
                  case ReleasePokemonResponse.Types.Result.Failed:
                      break;
                  case ReleasePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                      break;
                  default:
                      throw new ArgumentOutOfRangeException();
              }
          }, () => true));

        #endregion

        #region Favorite

        private DelegateCommand _favoritePokemonCommand;

        public DelegateCommand FavoritePokemonCommand => _favoritePokemonCommand ?? (
          _favoritePokemonCommand = new DelegateCommand(async () =>
          {
              bool isFavorite = Convert.ToBoolean(CurrentPokemon.Favorite);
              var pokemonFavoriteResponse = await GameClient.SetFavoritePokemon(CurrentPokemon.Id, !isFavorite);
              switch (pokemonFavoriteResponse.Result)
              {
                  case SetFavoritePokemonResponse.Types.Result.Unset:
                      break;
                  case SetFavoritePokemonResponse.Types.Result.Success:
                      // Inverse favorite state
                      CurrentPokemon.WrappedData.Favorite = Convert.ToInt32(!isFavorite);

                      IsFavorite = !isFavorite;
                      break;
                  // TODO: what to do on error?
                  case SetFavoritePokemonResponse.Types.Result.ErrorPokemonNotFound:
                      break;
                  case SetFavoritePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                      break;
                  default:
                      throw new ArgumentOutOfRangeException();
              }
          }, () => true));

        #endregion

        #region Rename

        private DelegateCommand _renamePokemonCommand;

        public DelegateCommand RenamePokemonCommand => _renamePokemonCommand ?? (
          _renamePokemonCommand = new DelegateCommand(async () =>
          {
              // TODO: Implement
          }, () => true));

        #endregion

        #region Power Up

        private DelegateCommand _powerUpPokemonCommand;

        public DelegateCommand PowerUpPokemonCommand => _powerUpPokemonCommand ?? (_powerUpPokemonCommand = new DelegateCommand(async () =>
        {
            // Send power up request
            var res = await GameClient.PowerUpPokemon(CurrentPokemon.WrappedData);
            switch (res.Result)
            {
                case UpgradePokemonResponse.Types.Result.Unset:
                    break;
                case UpgradePokemonResponse.Types.Result.Success:
                    // Reload updated data
                    CurrentPokemon = new PokemonDataWrapper(res.UpgradedPokemon);
                    await GameClient.UpdateInventory();
                    await GameClient.UpdateProfile();
                    UpdateCurrentData();
                    break;
                // TODO: do something if we have an error!
                case UpgradePokemonResponse.Types.Result.ErrorPokemonNotFound:
                    break;
                case UpgradePokemonResponse.Types.Result.ErrorInsufficientResources:
                    break;
                case UpgradePokemonResponse.Types.Result.ErrorUpgradeNotAvailable:
                    break;
                case UpgradePokemonResponse.Types.Result.ErrorPokemonIsDeployed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }, () => CurrentCandy != null && StardustAmount >= StardustToPowerUp && CurrentCandy.Candy_ >= CandiesToPowerUp));

        #endregion

        #region Evolve

        #region Evolve Events

        public event EventHandler PokemonEvolved;

        #endregion

        private DelegateCommand _evolvePokemonCommand;

        public DelegateCommand EvolvePokemonCommand => _evolvePokemonCommand ?? (_evolvePokemonCommand = new DelegateCommand(async () =>
        {
            EvolvePokemonResponse = await GameClient.EvolvePokemon(CurrentPokemon.WrappedData);   
            RaisePropertyChanged(() => EvolvedPokemonId);         
            switch (EvolvePokemonResponse.Result)
            {
                case EvolvePokemonResponse.Types.Result.Unset:
                    break;
                case EvolvePokemonResponse.Types.Result.Success:
                    PokemonEvolved?.Invoke(this, null);
                    await GameClient.UpdateInventory();
                    await GameClient.UpdateProfile();
                    break;
                // TODO: do something if we have an error!
                case EvolvePokemonResponse.Types.Result.FailedPokemonMissing:
                    break;
                case EvolvePokemonResponse.Types.Result.FailedInsufficientResources:
                    break;
                case EvolvePokemonResponse.Types.Result.FailedPokemonCannotEvolve:
                    break;
                case EvolvePokemonResponse.Types.Result.FailedPokemonIsDeployed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }, () => CurrentCandy != null && CurrentCandy.Candy_ >= PokemonExtraData.CandyToEvolve));

        private DelegateCommand _replaceEvolvedPokemonCommand;

        public DelegateCommand ReplaceEvolvedPokemonCommand => _replaceEvolvedPokemonCommand ?? (
            _replaceEvolvedPokemonCommand = new DelegateCommand(() =>
        {
            CurrentPokemon = new PokemonDataWrapper(EvolvePokemonResponse.EvolvedPokemonData);
            UpdateCurrentData();
        }));

        #endregion

        #endregion
    }
}
