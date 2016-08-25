using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using Microsoft.HockeyApp;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Views;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Newtonsoft.Json.Linq;

namespace PokemonGo_UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
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
            // Prevent from going back to other pages
            NavigationService.ClearHistory();
            if (suspensionState.Any())
            {
                // Recovering the state
                Username = (string) suspensionState[nameof(Username)];
                Password = (string) suspensionState[nameof(Password)];
            }
            else
            {
                if (!RememberLoginData) return;
                var currentCredentials = SettingsService.Instance.UserCredentials;
                if (currentCredentials == null) return;
                currentCredentials.RetrievePassword();
                Username = currentCredentials.UserName;
                Password = currentCredentials.Password;
            }
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

        public bool RememberLoginData
        {
            get { return SettingsService.Instance.RememberLoginData; }
            set { SettingsService.Instance.RememberLoginData = value; }
        }

        #endregion

        #region Game Logic

        private DelegateCommand _doPtcLoginCommand;

        public DelegateCommand DoPtcLoginCommand => _doPtcLoginCommand ?? (
            _doPtcLoginCommand = new DelegateCommand(async () =>
            {
                Busy.SetBusy(true, Resources.CodeResources.GetString("LoggingInText"));

                //Let's hack the shit out of SetBusy, it didn't want to populate itself, so we will HELP IT!
                await Task.Delay(50);

                try
                {
                    var loginSuccess = await GameClient.DoPtcLogin(Username, Password);

                    if (!loginSuccess)
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog(Resources.CodeResources.GetString("WrongUsernameText"))
                                .ShowAsyncQueue();
                    }
                    else
                    {
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage), GameMapNavigationModes.AppStart);
                    }
                }
                catch (PtcOfflineException)
                {
                    await new MessageDialog(Resources.CodeResources.GetString("PtcDownText")).ShowAsyncQueue();
                }
                catch (LoginFailedException e)
                {
                    var errorMessage = Resources.CodeResources.GetString("LoginFailedText");

                    try
                    {
                        var result = e.GetLoginResponseContentAsString();
                        var json = JObject.Parse(result.Result);
                        var token = json.SelectToken("$.errors[0]");
                        if (token != null)
                            errorMessage = token.ToString();
                    }
                    catch
                    {
                    }

                    await new MessageDialog(errorMessage).ShowAsyncQueue();
                }
                catch (Exception e)
                {
                    HockeyClient.Current.TrackEvent(e.Message);
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
                Busy.SetBusy(true, Resources.CodeResources.GetString("LoggingInText"));

                //Let's hack the shit out of SetBusy, it didn't want to populate itself, so we will HELP IT!
                await Task.Delay(50);

                try
                {
                    if (!await GameClient.DoGoogleLogin(Username.Trim(), Password.Trim()))
                    {
                        // Login failed, show a message
                        await
                            new MessageDialog(Resources.CodeResources.GetString("WrongUsernameText"))
                                .ShowAsyncQueue();
                    }
                    else
                    {
                        // Goto game page
                        await NavigationService.NavigateAsync(typeof(GameMapPage), GameMapNavigationModes.AppStart);
                    }
                }
                catch (GoogleOfflineException)
                {
                    await
                        new MessageDialog(Resources.CodeResources.GetString("GoogleNotRespondingText")).ShowAsyncQueue();
                }
                catch (GoogleException e)
                {
                    await
                        new MessageDialog(Resources.CodeResources.GetString("GoogleErrorText") + e.Message)
                            .ShowAsyncQueue();
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