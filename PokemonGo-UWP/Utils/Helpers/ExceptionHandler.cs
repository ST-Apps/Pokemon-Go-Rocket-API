using System.Threading.Tasks;
using Windows.UI.Popups;
using PokemonGo_UWP.Views;
using Template10.Common;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Microsoft.HockeyApp;
using PokemonGo.RocketAPI.Exceptions;

namespace PokemonGo_UWP.Utils
{
    public static class ExceptionHandler
    {
        public static async Task HandleException(Exception e = null)
        {
            try
            {
                if (e != null && (e.GetType() == typeof(ApiNonRecoverableException)))
                {
                    Debug.WriteLine($"[Relogin] {nameof(ApiNonRecoverableException)} from API handled.");
                    Debug.WriteLine("[Relogin] Successfuly ended.");
                }
                else if (e != null && e.GetType() == (typeof(AccessTokenExpiredException)))
                {
                    await
                        new MessageDialog(Resources.CodeResources.GetString("LoginExpired")).ShowAsyncQueue();
                    GameClient.DoLogout();
                    BootStrapper.Current.NavigationService.Navigate(typeof(MainPage));
                }
                else if (e != null && e.GetType() == (typeof(AccountLockedException)))
                {
                    await
                        new MessageDialog("Account locked/banned").ShowAsyncQueue();
                    GameClient.DoLogout();
                    BootStrapper.Current.NavigationService.Navigate(typeof(MainPage));
                }
                else
                {
					bool showDebug = false;
					try
					{
						//get inside try/catch in case exception comes from settings instance (storage access issue, ...)
						showDebug = SettingsService.Instance.ShowDebugInfoInErrorMessage;
					}
					catch { }

					string message = Resources.CodeResources.GetString("SomethingWentWrongText");
					if (showDebug)
					{
						message += $"\nException";
						message += $"\n Message:[{e?.Message}]";
						message += $"\n InnerMessage:[{e?.InnerException?.Message}]";
						message += $"\n StackTrace:[{e?.StackTrace}]";
					}

					var dialog = new MessageDialog(message);
					dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("YesText")) {Id = 0});
                    dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("NoText")) {Id = 1});
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;
                    var result = await dialog.ShowAsyncQueue();
                    if ((int) result.Id == 0)
                    {
                        GameClient.DoLogout();
                        BootStrapper.Current.NavigationService.Navigate(typeof(MainPage));
                        Busy.SetBusy(false);
                    }
                }
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
                Application.Current.Exit();
            }
        }
    }
}