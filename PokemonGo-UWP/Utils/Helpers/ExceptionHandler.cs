using System.Threading.Tasks;
using Windows.UI.Popups;
using PokemonGo_UWP.Views;
using Template10.Common;
using Universal_Authenticator_v2.Views;
using System;

namespace PokemonGo_UWP.Utils
{
    public static class ExceptionHandler
    {
        public static async Task HandleException(Exception e = null)
        {
            if (e.GetType().Namespace.Equals("PokemonGo.RocketAPI.Exceptions"))
            {
                await
                       new MessageDialog(Resources.CodeResources.GetString("LoginExpired")).ShowAsyncQueue();
                GameClient.DoLogout();
                BootStrapper.Current.NavigationService.Navigate(typeof(MainPage));
            }
            else
            {
                var dialog = new MessageDialog(Resources.CodeResources.GetString("SomethingWentWrongText"));
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