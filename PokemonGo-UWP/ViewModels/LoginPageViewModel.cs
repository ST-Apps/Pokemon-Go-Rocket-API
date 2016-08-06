using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Universal_Authenticator_v2.Views;
using System.Collections.ObjectModel;

namespace PokemonGo_UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
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
            // Prevent from going back to other pages
            NavigationService.ClearHistory();
            if (suspensionState.Any())
            {
                // Recovering the state                
                Username = (string) suspensionState[nameof(Username)];
                Password = (string) suspensionState[nameof(Password)];
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
                suspensionState[nameof(Username)] = Username;
                suspensionState[nameof(Password)] = Password;
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

        private string _username;

        private string _password;

        #endregion

        #region Bindable Game Vars

        public string CurrentVersion => GameClient.CurrentVersion;

        public string Username
        {
            get { return _username; }
            set
            {
                Set(ref _username, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
                DoGoogleLoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                Set(ref _password, value);
                DoPtcLoginCommand.RaiseCanExecuteChanged();
                DoGoogleLoginCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Game Logic        

        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, Resources.Translation.GetString("LoggingIn"));
                try
                {
                    var loginSuccess = await GameClient.DoPtcLogin(Username, Password);

                    if (!loginSuccess)
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog(Resources.Translation.GetString("WrongUsername"))
                                .ShowAsyncQueue();
                    }
                    else
                    {
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage), true);
                    }
                }
                catch (PtcOfflineException)
                {
                    await new MessageDialog(Resources.Translation.GetString("PTCDown")).ShowAsyncQueue();
                }
                catch (LoginFailedException)
                {
                    await
                            new MessageDialog(Resources.Translation.GetString("LoginFailed"))
                                .ShowAsyncQueue();
                }
                finally
                {
                    Busy.SetBusy(false);
                }
            }, () => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !Username.Contains("@"))
            );

        private DelegateCommand _doGoogleLoginCommand;

        public DelegateCommand DoGoogleLoginCommand => _doGoogleLoginCommand ?? (
            _doGoogleLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, Resources.Translation.GetString("LoggingIn"));
                try
                {
                    if (!await GameClient.DoGoogleLogin(Username.Trim(), Password.Trim()))
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog(Resources.Translation.GetString("WrongUsername"))
                                .ShowAsyncQueue();
                    }
                    else
                    {
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage), true);
                    }
                }
                catch (GoogleOfflineException)
                {
                    await new MessageDialog(Resources.Translation.GetString("GoogleNotResponding")).ShowAsyncQueue();
                }
                catch (GoogleException e)
                {
                    await new MessageDialog(Resources.Translation.GetString("GoogleError") + e.Message).ShowAsyncQueue();
                }
                finally
                {
                    Busy.SetBusy(false);
                }
            }, () => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && Username.Contains("@"))
            );

        #endregion

    }
}
