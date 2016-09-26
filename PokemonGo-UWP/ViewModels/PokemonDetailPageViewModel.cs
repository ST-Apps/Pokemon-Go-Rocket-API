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
using Google.Protobuf;
using PokemonGo_UWP.Controls;
using PokemonGo_UWP.Views;
using PokemonGo_UWP.Utils.Extensions;

namespace PokemonGo_UWP.ViewModels
{
    public class PokemonDetailPageViewModel : ViewModelBase
    {

        public PokemonDetailPageViewModel() { }

        #region Lifecycle Handlers

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                var sortingMode = (PokemonSortingModes)suspensionState[nameof(SortingMode)];
                var viewMode = (PokemonDetailPageViewMode)suspensionState[nameof(ViewMode)];
                var currentPokemonId = (ulong)suspensionState[nameof(SelectedPokemon)];
                Load(currentPokemonId, sortingMode, viewMode);
            } else
            {
                // Navigating from inventory page so we need to load the pokemoninventory and the current pokemon
                var navParam =  (SelectedPokemonNavModel)parameter;
                Load(Convert.ToUInt64(navParam.SelectedPokemonId), navParam.SortingMode, navParam.ViewMode);
            }

            // prevent going back to another PokemonDetailPage or CapturePokemonPage
            while (NavigationService.Frame.BackStack.Last().SourcePageType == typeof(PokemonDetailPage) || NavigationService.Frame.BackStack.Last().SourcePageType == typeof(CapturePokemonPage))
            {
                NavigationService.Frame.BackStack.Remove(NavigationService.Frame.BackStack.LastOrDefault());
            }

            await Task.CompletedTask;
        }

        public void Load(ulong selectedPokemonId, PokemonSortingModes sortingMode, PokemonDetailPageViewMode viewMode)
        {
            PokemonInventory.Clear();
            SortingMode = sortingMode;
            ViewMode = viewMode;
            if(viewMode == PokemonDetailPageViewMode.Normal)
            {
                // Navigating from inventory page so we need to load the pokemoninventory and the current pokemon
                PokemonInventory.AddRange(GameClient.PokemonsInventory.Select(pokemonData => new PokemonDataWrapper(pokemonData)).SortBySortingmode(sortingMode));
                SelectedPokemon = PokemonInventory.FirstOrDefault(pokemon => pokemon.Id == selectedPokemonId);
            } else
            {
                // Navigating from Capture, Egg hatch or evolve, only show this pokemon
                PokemonInventory.Add(GameClient.PokemonsInventory.Where(pokemon => pokemon.Id == selectedPokemonId).Select(pokemonData => new PokemonDataWrapper(pokemonData)).FirstOrDefault());
                SelectedPokemon = PokemonInventory.First();
            }

            StardustAmount = GameClient.PlayerProfile.Currencies.FirstOrDefault(item => item.Name.Equals("STARDUST")).Amount;
            PlayerTeamIsSet = GameClient.PlayerProfile.Team != TeamColor.Neutral;
        }

        /// <summary>
        /// Handling when navigating away to prevent illegal back
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            if (args.NavigationMode == NavigationMode.Back)
            {
                if (EvolveAnimationIsRunning)
                {
                    args.Cancel = true;
                }

                if (NavigationService.Frame.BackStack.Last().SourcePageType == typeof(PokemonInventoryPage))
                {
                    NavigationHelper.NavigationState.Add("LastViewedPokemonDetailID", SelectedPokemon.Id);
                }
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
                suspensionState[nameof(SortingMode)] = SortingMode;
                suspensionState[nameof(ViewMode)] = ViewMode;
                suspensionState[nameof(SelectedPokemon)] = SelectedPokemon.Id;
            }
            await Task.CompletedTask;
        }

        #endregion

        #region Bindable Vars

        public List<PokemonDataWrapper> PokemonInventory { get; private set; } = new List<PokemonDataWrapper>();

        public PokemonSortingModes SortingMode { get; private set; }

        public bool EvolveAnimationIsRunning { get; set; }

        /// <summary>
        /// Flag for an ongoing server request. Used to disable the controls
        /// </summary>
        private bool _serverRequestRunning;
        public bool ServerRequestRunning
        {
            get { return _serverRequestRunning; }
            set
            {
                Set(ref _serverRequestRunning, value);
                FavoritePokemonCommand.RaiseCanExecuteChanged();
                PowerUpPokemonCommand.RaiseCanExecuteChanged();
                EvolvePokemonCommand.RaiseCanExecuteChanged();
                TransferPokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Viewmode for current Pokemon
        /// </summary>
        private PokemonDetailPageViewMode _viewMode;
        public PokemonDetailPageViewMode ViewMode
        {
            get { return _viewMode; }
            set { Set(ref _viewMode, value); }
        }

        /// <summary>
        /// Current displayed Pokemon
        /// </summary>
        private PokemonDataWrapper _selectedPokemon;
        public PokemonDataWrapper SelectedPokemon
        {
            get { return _selectedPokemon; }
            set
            {
                Set(ref _selectedPokemon, value);
                PowerUpPokemonCommand.RaiseCanExecuteChanged();
                EvolvePokemonCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The ID of the resulting pokemon type of an ongoing evolution
        /// </summary>
        public PokemonId EvolvedPokemonId
        {
            get { return EvolvedPokemon == null ? PokemonId.Missingno : EvolvedPokemon.PokemonId; }
        }

        /// <summary>
        /// The resulting pokemon of an ongoing evolution
        /// </summary>
        private PokemonDataWrapper _evolvedPokemon;
        public PokemonDataWrapper EvolvedPokemon
        {
            get { return _evolvedPokemon; }
            set {
                Set(ref _evolvedPokemon, value);
                RaisePropertyChanged(() => EvolvedPokemonId);
            }
        }

        /// <summary>
        /// Currently available amount of stardust
        /// </summary>
        private int _stardustAmount;
        public int StardustAmount
        {
            get { return _stardustAmount; }
            set { Set(ref _stardustAmount, value); }
        }

        /// <summary>
        /// Indicator if the player has already chosen a team. This is needed for the appraise function.
        /// </summary>
        private bool _playerTeamIsSet;
        public bool PlayerTeamIsSet
        {
            get { return _playerTeamIsSet; }
            set { Set(ref _playerTeamIsSet, value); }
        }

        #endregion

        #region Game Logic

        #region Shared Logic


        private DelegateCommand _closePokemonDetailPage;

        /// <summary>
        /// Close the Page and go back to the previous page
        /// </summary>
        public DelegateCommand ClosePokemonDetailPage => _closePokemonDetailPage ?? (
            _closePokemonDetailPage = new DelegateCommand(() =>
            {
                if (ServerRequestRunning) return;
                NavigationService.GoBack();
            }, () => true)
            );

        #endregion

        #region Appraise

        private DelegateCommand _appraisePokemonCommand;

        public DelegateCommand AppraisePokemonCommand => _appraisePokemonCommand ?? (
          _appraisePokemonCommand = new DelegateCommand(() =>
          {
              // TODO: Implement appraise
              var dialog = new MessageDialog("Sorry, check back later 😉", "Not yet implemented");
              dialog.ShowAsync();
          }, () => true));

        #endregion

        #region Transfer

        private DelegateCommand _transferPokemonCommand;

        public DelegateCommand TransferPokemonCommand => _transferPokemonCommand ?? (
          _transferPokemonCommand = new DelegateCommand(() =>
          {
              // Catch if the Pokémon is a Favorite, transfering in this case is not permitted
              // TODO: This isn't a MessageDialog in the original apps, implement error style (Shell needed)
              if (Convert.ToBoolean(SelectedPokemon.Favorite))
              {
                  var cannotTransferDialog = new PoGoMessageDialog(Resources.CodeResources.GetString("CannotTransferFavorite"), "")
                  {
                      CoverBackground = true,
                      AnimationType = PoGoMessageDialogAnimation.Bottom
                  };
                  cannotTransferDialog.Show();
                  return;
              }
              // Ask for confirmation before moving the Pokemon
              var name = Resources.Pokemon.GetString(SelectedPokemon.PokemonId.ToString());
              var dialog =
                  new PoGoMessageDialog(
                      string.Format(Resources.CodeResources.GetString("TransferPokemonWarningTitle"), name),
                      Resources.CodeResources.GetString("TransferPokemonWarningText"))
                  {
                      AcceptText = Resources.CodeResources.GetString("YesText"),
                      CancelText = Resources.CodeResources.GetString("NoText"),
                      CoverBackground = true,
                      AnimationType = PoGoMessageDialogAnimation.Bottom
                  };

              dialog.AcceptInvoked += async (sender, e) =>
              {
                  // User confirmed transfer
                  try
                  {
                      ServerRequestRunning = true;
                      var pokemonTransferResponse = await GameClient.TransferPokemon(SelectedPokemon.Id);

                      switch (pokemonTransferResponse.Result)
                      {
                          case ReleasePokemonResponse.Types.Result.Unset:
                              break;
                          case ReleasePokemonResponse.Types.Result.Success:
                              // TODO: Implement message informing about success of transfer (Shell needed)
                              await GameClient.UpdateInventory();
                              await GameClient.UpdatePlayerStats();
                              NavigationService.GoBack();
                              break;

                          case ReleasePokemonResponse.Types.Result.PokemonDeployed:
                              break;
                          case ReleasePokemonResponse.Types.Result.Failed:
                              break;
                          case ReleasePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                              break;
                          default:
                              throw new ArgumentOutOfRangeException();
                      }
                  }
                  finally
                  {
                      ServerRequestRunning = false;
                  }

              };

              dialog.Show();

          }, () => !ServerRequestRunning));

        #endregion

        #region Favorite

        private DelegateCommand _favoritePokemonCommand;

        public DelegateCommand FavoritePokemonCommand => _favoritePokemonCommand ?? (
          _favoritePokemonCommand = new DelegateCommand(async () =>
          {
              try
              {
                  ServerRequestRunning = true;
                  var isFavorite = Convert.ToBoolean(SelectedPokemon.Favorite);
                  var pokemonFavoriteResponse = await GameClient.SetFavoritePokemon(SelectedPokemon.Id, !isFavorite);
                  switch (pokemonFavoriteResponse.Result)
                  {
                      case SetFavoritePokemonResponse.Types.Result.Unset:
                          break;
                      case SetFavoritePokemonResponse.Types.Result.Success:
                          // Inverse favorite state
                          SelectedPokemon.Favorite = Convert.ToInt32(!isFavorite);
                          break;
                      case SetFavoritePokemonResponse.Types.Result.ErrorPokemonNotFound:
                          break;
                      case SetFavoritePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                          break;
                      default:
                          throw new ArgumentOutOfRangeException();
                  }
              }
              finally
              {
                  ServerRequestRunning = false;
              }
          }, () => !ServerRequestRunning));

        #endregion

        #region Rename

        private DelegateCommand _renamePokemonCommand;

        public DelegateCommand RenamePokemonCommand => _renamePokemonCommand ?? (
          _renamePokemonCommand = new DelegateCommand(() =>
          {
              var textbox = new TextboxMessageDialog(SelectedPokemon.Name, 12);
              var dialog = new PoGoMessageDialog("", Resources.CodeResources.GetString("SetNickName"))
              {
                  DialogContent = textbox,
                  AcceptText = Resources.CodeResources.GetString("OkText"),
                  CancelText = Resources.CodeResources.GetString("CancelText"),
                  CoverBackground = true,
                  BackgroundTapInvokesCancel = true,
                  AnimationType = PoGoMessageDialogAnimation.Bottom
              };

              dialog.AppearCompleted += (sender, e) =>
              {
                  textbox.SelectAllOnTextBoxFocus = true;
                  textbox.FocusTextbox(FocusState.Programmatic);
              };
              dialog.AcceptInvoked += async (sender, e) =>
              {

                  try
                  {
                      ServerRequestRunning = true;
                      // Send rename request
                      var res = await GameClient.SetPokemonNickName((ulong)SelectedPokemon.Id, textbox.Text);
                      switch (res.Result)
                      {
                          case NicknamePokemonResponse.Types.Result.Unset:
                              break;
                          case NicknamePokemonResponse.Types.Result.Success:
                              SelectedPokemon.Nickname = textbox.Text;
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorPokemonNotFound:
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorInvalidNickname:
                              break;
                          case NicknamePokemonResponse.Types.Result.ErrorPokemonIsEgg:
                              break;
                          default:
                              throw new ArgumentOutOfRangeException();
                      }
                  }
                  finally
                  {
                      ServerRequestRunning = false;
                  }
              };

              dialog.Show();

          }, () => true));

        #endregion

        #region Power Up

        private DelegateCommand _powerUpPokemonCommand;

        public DelegateCommand PowerUpPokemonCommand => _powerUpPokemonCommand ?? (_powerUpPokemonCommand = new DelegateCommand(() =>
        {
            // Ask for confirmation before powering up the Pokemon
            var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("PowerUpPokemonWarningText"),Resources.Pokemon.GetString(SelectedPokemon.PokemonId.ToString())));
            dialog.AcceptText = Resources.CodeResources.GetString("YesText");
            dialog.CancelText = Resources.CodeResources.GetString("NoText");
            dialog.CoverBackground = true;
            dialog.AnimationType = PoGoMessageDialogAnimation.Bottom;
            dialog.AcceptInvoked += async (sender, e) =>
            {
                try
                {
                    ServerRequestRunning = true;
                    // Send power up request
                    var res = await GameClient.PowerUpPokemon(SelectedPokemon.WrappedData);
                    switch (res.Result)
                    {
                        case UpgradePokemonResponse.Types.Result.Unset:
                            break;
                        case UpgradePokemonResponse.Types.Result.Success:
                            // Reload updated data
                            PokemonInventory.Remove(SelectedPokemon);
                            var uppedPokemon = new PokemonDataWrapper(res.UpgradedPokemon);
                            PokemonInventory.Add(uppedPokemon);
                            PokemonInventory.SortBySortingmode(SortingMode);
                            SelectedPokemon = uppedPokemon;
                            RaisePropertyChanged(nameof(SelectedPokemon));
                            await GameClient.UpdateInventory();
                            await GameClient.UpdateProfile();
                            break;
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
                }
                finally
                {
                    ServerRequestRunning = false;
                }
            };

            dialog.Show();
        }, CanPowerUp));

        private bool CanPowerUp()
        {
            if (SelectedPokemon == null || ServerRequestRunning) return false;
            var pokemonLevel = PokemonInfo.GetLevel(SelectedPokemon.WrappedData);
            var currentCandy = GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == GameClient.GetExtraDataForPokemon(SelectedPokemon.PokemonId).FamilyId);
            var candiesToPowerUp = Convert.ToInt32(GameClient.PokemonUpgradeSettings.CandyCost[Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(SelectedPokemon.WrappedData)) - 1)]);
            var stardustToPowerUp = Convert.ToInt32(GameClient.PokemonUpgradeSettings.StardustCost[Convert.ToInt32(Math.Floor(PokemonInfo.GetLevel(SelectedPokemon.WrappedData)) - 1)]);

            return currentCandy != null && StardustAmount >= stardustToPowerUp && currentCandy.Candy_ >= candiesToPowerUp && pokemonLevel < GameClient.PlayerStats.Level + 1.5;
        }

        #endregion

        #region Evolve

        #region Evolve Events

        public event EventHandler PokemonEvolved;

        #endregion

        private DelegateCommand _evolvePokemonCommand;

        public DelegateCommand EvolvePokemonCommand => _evolvePokemonCommand ?? (_evolvePokemonCommand = new DelegateCommand(() =>
        {
            // Ask for confirmation before evolving the Pokemon
            var dialog = new PoGoMessageDialog("", string.Format(Resources.CodeResources.GetString("EvolvePokemonWarningText"),
                Resources.Pokemon.GetString(SelectedPokemon.PokemonId.ToString())))
            {
                AcceptText = Resources.CodeResources.GetString("YesText"),
                CancelText = Resources.CodeResources.GetString("NoText"),
                CoverBackground = true,
                AnimationType = PoGoMessageDialogAnimation.Bottom
            };
            dialog.AcceptInvoked += async (sender, e) =>
            {
                try
                {
                    ServerRequestRunning = true;
                    // Send evolve request
                    EvolvePokemonResponse res = await GameClient.EvolvePokemon(SelectedPokemon.WrappedData);
                    RaisePropertyChanged(() => EvolvedPokemonId);
                    switch (res.Result)
                    {
                        case EvolvePokemonResponse.Types.Result.Unset:
                            break;
                        case EvolvePokemonResponse.Types.Result.Success:
                            EvolvedPokemon = new PokemonDataWrapper(res.EvolvedPokemonData);
                            PokemonEvolved?.Invoke(this, null);
                            await GameClient.UpdateInventory();
                            await GameClient.UpdateProfile();
                            break;

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
                    };
                }
                finally
                {
                    ServerRequestRunning = false;
                }
            };

            dialog.Show();
            }, CanEvolve));

        private bool CanEvolve()
        {
            if (SelectedPokemon == null || ServerRequestRunning) return false;
            var currentCandy = GameClient.CandyInventory.FirstOrDefault(item => item.FamilyId == GameClient.GetExtraDataForPokemon(SelectedPokemon.PokemonId).FamilyId);
            return currentCandy != null && currentCandy.Candy_ >= GameClient.GetExtraDataForPokemon(SelectedPokemon.PokemonId).CandyToEvolve;
        }

        private DelegateCommand _navigateToEvolvedPokemonCommand;

        public DelegateCommand NavigateToEvolvedPokemonCommand => _navigateToEvolvedPokemonCommand ?? (_navigateToEvolvedPokemonCommand = new DelegateCommand(() =>
        {
            NavigationService.Navigate(typeof(PokemonDetailPage), new SelectedPokemonNavModel()
            {
                SelectedPokemonId = EvolvedPokemon.Id.ToString(),
                ViewMode = PokemonDetailPageViewMode.ReceivedPokemon
            });
        }));

        #endregion

        #endregion
    }
}
