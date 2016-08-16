using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Utils;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using PokemonGo_UWP.Views;
using Template10.Common;

namespace PokemonGo_UWP.ViewModels
{
    public class PlayerProfileViewModel : ViewModelBase
    {
        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState)
        {
            if (suspensionState.Any())
            {
                // Recovering the state                                
                PlayerProfile = (PlayerData) suspensionState[nameof(PlayerProfile)];
                PlayerStats = (PlayerStats) suspensionState[nameof(PlayerStats)];
            }
            else
            {
                // No saved state, get them from the client                                
                PlayerProfile = GameClient.PlayerProfile;
                PlayerStats = GameClient.PlayerStats;
            }
            ReadPlayerStatsValues();
            await Task.CompletedTask;
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
                suspensionState[nameof(PlayerProfile)] = PlayerProfile;
                suspensionState[nameof(PlayerStats)] = PlayerStats;
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
        ///     Player's profile, we use it just for the username
        /// </summary>
        private PlayerData _playerProfile;

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        private PlayerStats _playerStats;

        #endregion

        #region Bindable Game Vars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats
        {
            get { return _playerStats; }
            set { Set(ref _playerStats, value); }
        }

        public ObservableCollection<KeyValuePair<AchievementType, object>> Achievements { get; } =
            new ObservableCollection<KeyValuePair<AchievementType, object>>();

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToGameScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToGameScreen
            =>
                _returnToGameScreen ??
                (_returnToGameScreen = new DelegateCommand(() => { NavigationService.GoBack(); }, () => true));

        #endregion

        #region Read Player Stats Values

        private void ReadPlayerStatsValues()
        {
            Achievements.Clear();
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Jogger, PlayerStats.KmWalked));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Kanto,
                PlayerStats.UniquePokedexEntries));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Collector,
                PlayerStats.PokemonsCaptured));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Scientist, PlayerStats.Evolutions));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Breeder, PlayerStats.EggsHatched));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Backpacker,
                PlayerStats.PokeStopVisits));
            if (PlayerStats.PokemonCaughtByType.Count() > 0)
            {
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.SchoolKid,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Normal]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Swimmer,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Water]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.BlackBelt,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Fighting]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.BirdKeeper,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Flying]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.PunkGirl,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Poison]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.RuinManiac,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Ground]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Hiker,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Rock]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.BugCatcher,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Bug]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.HexManiac,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Ghost]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.DepotAgent,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Steel]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Kindler,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Fire]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Gardener,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Grass]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Rocker,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Electric]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Psychic,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Psychic]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Skier,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Ice]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.DragonTamer,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Dragon]));
                Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.FairyTaleGirl,
                    PlayerStats.PokemonCaughtByType[(int)PokemonType.Fairy]));
            }
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Youngster,
                PlayerStats.SmallRattataCaught));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.Fisherman,
                PlayerStats.BigMagikarpCaught));
            Achievements.Add(new KeyValuePair<AchievementType, object>(AchievementType.AceTrainer,
                PlayerStats.BattleTrainingWon));
        }

        #endregion

        #region Navigate to detail page

        private DelegateCommand<object> m_GoToAchievementDetailPage;

        public DelegateCommand<object> GoToAchievementDetailPage => m_GoToAchievementDetailPage ?? (m_GoToAchievementDetailPage = new DelegateCommand<object>(x =>
        {
            BootStrapper.Current.NavigationService.Navigate(typeof(AchievementDetailPage), x);
        }));

        #endregion

        #endregion
    }
}