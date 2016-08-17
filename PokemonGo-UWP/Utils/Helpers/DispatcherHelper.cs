using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils.Helpers
{
    class DispatcherHelper
    {
        /// <summary>
        /// Runs asynchronous action in dispatcher thread and waits for finishing it
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task RunInDispatcherAndAwait(Func<Task> action)
        {
            var completionSource = new TaskCompletionSource<bool>();

            var dispAsync = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
                    (Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await action();
                        completionSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        completionSource.SetException(e);
                    }
                });
            var result = await completionSource.Task;
        }

        /// <summary>
        /// Runs action in dispatcher thread and waits for finishing it
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task RunInDispatcherAndAwait(Action action)
        {
            var completionSource = new TaskCompletionSource<bool>();

            var dispAsync = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync
                    (Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        try
                        {
                            action();
                            completionSource.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            completionSource.SetException(e);
                        }
                    });
            var result = await completionSource.Task;
        }
    }
}
