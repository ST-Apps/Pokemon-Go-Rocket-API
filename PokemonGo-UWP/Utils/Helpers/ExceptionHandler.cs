using System.Threading.Tasks;
using Windows.UI.Popups;
using PokemonGo_UWP.Views;
using Template10.Common;
using Universal_Authenticator_v2.Views;
using System;
using System.Diagnostics;
using PokemonGo.RocketAPI.Exceptions;

namespace PokemonGo_UWP.Utils
{
    public static class ExceptionHandler
    {
        public static async Task HandleException(Exception e = null)
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
                var dialog = new MessageDialog(Resources.CodeResources.GetString("SomethingWentWrongText")
#if DEBUG
                    + "\n(" + e?.Message + ")");
#else
                    );
#endif
                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("YesText")) { Id = 0 });
                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("NoText")) { Id = 1 });
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                var result = await dialog.ShowAsyncQueue();
                if ((int)result.Id == 0)
                {
                    GameClient.DoLogout();
                    BootStrapper.Current.NavigationService.Navigate(typeof(MainPage));
                    Busy.SetBusy(false);
                }
            }
        }
    }
}