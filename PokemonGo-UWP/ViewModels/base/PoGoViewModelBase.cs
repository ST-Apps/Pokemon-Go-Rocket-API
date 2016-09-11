using Google.Protobuf;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using PokemonGo_UWP.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.ViewModels {
    /// <summary>
    /// This Base View Models handles basic implementations which are used in many other ViewModels.
    /// </summary>
    public abstract class PoGoViewModelBase : ViewModelBase {
        #region Lifecycle Handlers

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> suspensionState) {

            if(suspensionState.Any()) {
                // Recovering the state
                PlayerProfile = new PlayerData();
                PlayerStats = new PlayerStats();
                PlayerProfile.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(PlayerProfile)]).CreateCodedInput());
                PlayerStats.MergeFrom(ByteString.FromBase64((string)suspensionState[nameof(PlayerStats)]).CreateCodedInput());
                RaisePropertyChanged(() => PlayerProfile);
                RaisePropertyChanged(() => PlayerStats);
            } else {
                // No saved state, get them from the client
                PlayerProfile = GameClient.PlayerProfile;
                PlayerStats = GameClient.PlayerStats;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        ///     Save state before navigating
        /// </summary>
        /// <param name="suspensionState"></param>
        /// <param name="suspending"></param>
        /// <returns></returns>
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending) {
            if(suspending) {
                suspensionState[nameof(PlayerProfile)] = PlayerProfile.ToByteString().ToBase64();
                suspensionState[nameof(PlayerStats)] = PlayerStats.ToByteString().ToBase64();
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args) {
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

        #region Bindable GameVars

        /// <summary>
        ///     Player's profile, we use it just for the username
        /// </summary>
        public PlayerData PlayerProfile
        {
            get { return _playerProfile; }
            protected set { Set(ref _playerProfile, value); }
        }

        /// <summary>
        ///     Stats for the current player, including current level and experience related stuff
        /// </summary>
        public PlayerStats PlayerStats
        {
            get { return _playerStats; }
            protected set { Set(ref _playerStats, value); }
        }

        #endregion
    }
}
