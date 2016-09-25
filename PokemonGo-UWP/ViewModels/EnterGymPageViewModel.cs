using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Newtonsoft.Json;
using Google.Protobuf;
using POGOProtos.Data.Gym;
using Windows.UI.Popups;
using POGOProtos.Enums;

namespace PokemonGo_UWP.ViewModels
{
    public enum AttackType
    {
        None,
        Train,
        Attack
    }

    public class EnterGymPageViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter">FortData containing the Gym that we're visiting</param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state
                CurrentGym = JsonConvert.DeserializeObject<FortDataWrapper>((string)suspensionState[nameof(CurrentGym)]);
                CurrentGymInfo = JsonConvert.DeserializeObject<GetGymDetailsResponse>((string)suspensionState[nameof(CurrentGymInfo)]);
                CurrentMembers = GetCurrentMembers(CurrentGymInfo);
                UpdateBindableData();
            }
            else
            {

                CurrentGym = (FortDataWrapper)NavigationHelper.NavigationState[nameof(CurrentGym)];
                NavigationHelper.NavigationState.Remove(nameof(CurrentGym));

                await CalculateGymData(CurrentGym);

                await CheckPlayerTeamSelection();
            }
        }

        /// <summary>
        ///     Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(CurrentGym)] = JsonConvert.SerializeObject(CurrentGym);
                suspensionState[nameof(CurrentGymInfo)] = JsonConvert.SerializeObject(CurrentGymInfo);
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

        private FortDataWrapper _currentGym;

        private GetGymDetailsResponse _currentGymInfo;

        private ObservableCollection<GymMembershipWrapper> _currentMembers;

        private AttackType _AtckType = AttackType.None;
        private bool _notLvl5Yet = true;
        private bool _canDeploy = false;
        private bool _notInRange = true;
        private bool _isEnabled = false;


        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Gym that the user is visiting
        /// </summary>
        public FortDataWrapper CurrentGym
        {
            get { return _currentGym; }
            set { Set(ref _currentGym, value); }
        }

        /// <summary>
        ///     Infos on the current Gym
        /// </summary>
        public GetGymDetailsResponse CurrentGymInfo
        {
            get { return _currentGymInfo; }
            set { Set(ref _currentGymInfo, value); }
        }

        public ObservableCollection<GymMembershipWrapper> CurrentMembers
        {
            get { return _currentMembers; }
            set { Set(ref _currentMembers, value); }
        }

        public AttackType AtckType
        {
            get { return _AtckType; }
            set { Set(ref _AtckType, value); }
        }

        public bool NotLvl5Yet
        {
            get { return _notLvl5Yet; }
            set { Set(ref _notLvl5Yet, value); }
        }

        public bool CanDeploy
        {
            get { return _canDeploy; }
            set { Set(ref _canDeploy, value); }
        }

        public bool NotInRange
        {
            get { return _notInRange; }
            set { Set(ref _notInRange, value); }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { Set(ref _isEnabled, value); }
        }

        #endregion

        #region Game Logic

        private DelegateCommand _returnToGameScreen;

        public DelegateCommand ReturnToGameScreen =>
            _returnToGameScreen ?? ( _returnToGameScreen = new DelegateCommand( () =>
            {
                NavigationService.Navigate(typeof(GameMapPage), GameMapNavigationModes.GymUpdate);
            },() => true));

        private ObservableCollection<GymMembershipWrapper> GetCurrentMembers(GetGymDetailsResponse currentGymInfo) =>
            new ObservableCollection<GymMembershipWrapper>(
                currentGymInfo.GymState.Memberships.Select(m => new GymMembershipWrapper(m)));

        private async Task CalculateGymData(FortDataWrapper currentGym)
        {
            // Navigating from game page, so we need to actually load the Gym
            Busy.SetBusy(true, "Loading Gym");
            var response = await GameClient.GetGymDetails(currentGym.Id, currentGym.Latitude, currentGym.Longitude);
            Logger.Write($"Entering {response.Name} [ID = {CurrentGym.Id}]");
            Busy.SetBusy(false);
            switch (response.Result)
            {
                case GetGymDetailsResponse.Types.Result.Unset:
                    Logger.Write("Entering Gym Unset");
                    var dialog = new MessageDialog("Gym is unset, whatever it means");
                    await dialog.ShowAsync();
                    ReturnToGameScreen.Execute();
                    break;
                case GetGymDetailsResponse.Types.Result.Success:
                    // Success, we play the animation and update inventory
                    Logger.Write("Entering Gym success");
                    CurrentGym = new FortDataWrapper(response.GymState.FortData);
                    CurrentGymInfo = response;
                    await GameClient.UpdateInventory();
                    CurrentMembers = GetCurrentMembers(CurrentGymInfo);
                    UpdateBindableData();
                    break;
                case GetGymDetailsResponse.Types.Result.ErrorNotInRange:
                    Logger.Write("Entering Gym out of range");
                    var dialog2 = new MessageDialog("Gym is completely out of range");
                    await dialog2.ShowAsync();
                    ReturnToGameScreen.Execute();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(response.Result), "GetGymDetails result enum had something undefined");
            }
        }

        private void UpdateBindableData()
        {
            var AmIPresent = CurrentMembers.Where(cm => cm.PlayerName == GameClient.PlayerProfile.Username).FirstOrDefault() != null;
            var AmINeutralTeam = GameClient.PlayerProfile.Team == TeamColor.Neutral;
            var HasSpace = CurrentGym.GymLevel > CurrentMembers.Count;
            var IsTeamOk = CurrentGym.OwnedByTeam == TeamColor.Neutral ? true : CurrentGym.OwnedByTeam == GameClient.PlayerProfile.Team;

            NotLvl5Yet = GameClient.PlayerStats?.Level < 5;
            CanDeploy = !AmINeutralTeam && IsTeamOk && HasSpace && !AmIPresent;
            NotInRange = CurrentGym.FortDataStatus == Utils.Game.FortDataStatus.Closed;
            IsEnabled = !NotInRange && !NotLvl5Yet;

            if (CurrentGym.OwnedByTeam == TeamColor.Neutral)
                AtckType = AttackType.None;
            else if (CurrentGym.OwnedByTeam == GameClient.PlayerProfile.Team)
                AtckType = AttackType.Train;
            else
                AtckType = AttackType.Attack;

            DeployPokemon.RaiseCanExecuteChanged();
            StartBattle.RaiseCanExecuteChanged();
        }


        private void CheckPlayerTeamSelection()
        {
            if(GameClient.PlayerStats.Level >= 5 && GameClient.PlayerProfile.Team == TeamColor.Neutral)
            {
                // TODO Modal dialog with teams and selection
                // TODO modal dialog with confirmation
            }
        }


        private DelegateCommand _deployPokemon;
        public DelegateCommand DeployPokemon =>
            _deployPokemon ?? (_deployPokemon = new DelegateCommand( async() =>
            {
                //TODO make pokemon selection, now gets highest CP of favorites
                var pokemon = GameClient.PokemonsInventory.OrderByDescending(p => Convert.ToBoolean(p.Favorite)).ThenByDescending(p => p.Cp).FirstOrDefault();

                if (pokemon == null)
                {
                    var dialog = new MessageDialog("No pokemon", "You don't have any pokemon to deploy!");
                    await dialog.ShowAsync();
                    ReturnToGameScreen.Execute();
                }

                var response = await GameClient.FortDeployPokemon(CurrentGym.Id, pokemon.Id);
                switch (response.Result)
                {
                    case FortDeployPokemonResponse.Types.Result.NoResultSet:
                        Logger.Write("Deploy to Gym NoResultSet");
                        var d1 = new MessageDialog("Deploy to Gym NoResultSet, whatever it means");
                        await d1.ShowAsync();
                        break;
                    case FortDeployPokemonResponse.Types.Result.Success:
                        await CalculateGymData(CurrentGym);
                        CurrentGym.Update();
                        ReturnToGameScreen.Execute();
                        break;
                    case FortDeployPokemonResponse.Types.Result.ErrorAlreadyHasPokemonOnFort:
                    case FortDeployPokemonResponse.Types.Result.ErrorOpposingTeamOwnsFort:
                    case FortDeployPokemonResponse.Types.Result.ErrorFortIsFull:
                    case FortDeployPokemonResponse.Types.Result.ErrorNotInRange:
                        await CalculateGymData(CurrentGym);
                        CurrentGym.Update();
                        break;
                    case FortDeployPokemonResponse.Types.Result.ErrorPlayerHasNoTeam:
                    case FortDeployPokemonResponse.Types.Result.ErrorPokemonNotFullHp:
                    case FortDeployPokemonResponse.Types.Result.ErrorPlayerBelowMinimumLevel:
                    case FortDeployPokemonResponse.Types.Result.ErrorPokemonIsBuddy:
                        var d2 = new MessageDialog($"Should have never happened: {response.Result}");
                        await d2.ShowAsync();
                        break;
                    default:
                        break;
                }
            }, () => IsEnabled));





        private DelegateCommand _startBattle;
        public DelegateCommand StartBattle =>
            _startBattle ?? (_startBattle = new DelegateCommand(async () =>
            {
                //TODO implement Gym battles
                var dialog = new MessageDialog("Sorry, check back later ;)", "Not yet implemented");
                await dialog.ShowAsync();
            }, () => IsEnabled));

        #endregion
    }
}