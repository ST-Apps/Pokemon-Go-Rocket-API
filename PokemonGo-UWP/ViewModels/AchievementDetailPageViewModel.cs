using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Utils;
using Template10.Mvvm;
using Template10.Services.NavigationService;

namespace PokemonGo_UWP.ViewModels {
    public class AchievementDetailPageViewModel : ViewModelBase {

        #region Lifecycle Handlers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="mode"></param>
        /// <param name="suspensionState"></param>
        /// <returns></returns>
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            KeyValuePair<AchievementType, object> achievement;
            if(suspensionState.Any()) {
                // Recovering the state                
                achievement = (KeyValuePair<AchievementType, object>)parameter;
            } else {
                // No saved state, get them from the client                
                achievement = (KeyValuePair<AchievementType, object>)parameter;
            }
            Achievement = achievement;
            RaisePropertyChanged(nameof(Achievement));
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
            if(suspending)
            {

            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args) {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        #region Bindable Game Vars

        private KeyValuePair<AchievementType, object> m_Achievement;

        public KeyValuePair<AchievementType, object> Achievement
        {
            get
            {
                return m_Achievement.Equals(default(KeyValuePair<AchievementType, object>))
                    ? new KeyValuePair<AchievementType, object>(AchievementType.Jogger, 0)
                    : m_Achievement;
            }
            private set { m_Achievement = value; }
        }

        #endregion

        #region Game Logic

        #region Shared Logic

        private DelegateCommand _returnToRProfileScreen;

        /// <summary>
        ///     Going back to map page
        /// </summary>
        public DelegateCommand ReturnToProfileScreen => _returnToRProfileScreen ?? (_returnToRProfileScreen = new DelegateCommand(() => { NavigationService.GoBack(); }, () => true));

        #endregion

        #endregion
    }
}
