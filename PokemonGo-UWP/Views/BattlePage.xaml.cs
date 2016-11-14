using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BattlePage : Page
    {
        public BattlePage()
        {
            this.InitializeComponent();
            this.Loaded += BattlePage_Loaded;
        }

        private void BattlePage_Loaded(object sender, RoutedEventArgs e)
        {
            Pokemon1Image.Opacity = 0;
            Pokemon2Image.Opacity = 0;
            ButtonRunAway.Click += ButtonRunAway_Click;
            ButtonSwap.Click += ButtonSwap_Click;
        }

        private void ButtonSwap_Click(object sender, RoutedEventArgs e)
        {
            Pokemon1Image.Opacity = 1;
            Task.Delay(0).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DodgeRightStoryboard .Begin()));
        }

        private void ButtonRunAway_Click(object sender, RoutedEventArgs e)
        {
            Pokemon1Image.Opacity = 1;
            Task.Delay(0).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DodgeLeftStoryboard.Begin()));

            //return;

            ViewModel.TimerValue = 100;
            ViewModel.ChargeAvailableValue = 0;
            ViewModel.Pokemon1Health = 100;
            ViewModel.Pokemon2Health = 100;
            Pokemon1Image.Opacity = 0;
            Pokemon2Image.Opacity = 0;
            Task.Delay(3000).ContinueWith(_ =>
                           Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                              Pokemon1AppearStoryboard.Begin()));
            Task.Delay(3100).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon2AppearStoryboard.Begin()));
            Task.Delay(4000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    BattleBeginStoryboard.Begin()));
            Task.Delay(8000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1FastAttackStoryboard.Begin()));
            Task.Delay(8050).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon2FastAttackStoryboard.Begin()));
            Task.Delay(9000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1FastAttackStoryboard.Begin()));
            Task.Delay(9100).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Pokemon2DamageStoryboard.Begin();
                    ViewModel.Pokemon2Health -= 5;
                }));
            Task.Delay(9900).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon2FastAttackStoryboard.Begin()));
            Task.Delay(10000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DodgeLeftStoryboard.Begin()));
            Task.Delay(11000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DodgeRightStoryboard.Begin()));
            Task.Delay(11500).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DodgeRightStoryboard.Begin()));
            Task.Delay(11800).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1FocusStoryboard.Begin()));
            Task.Delay(12000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Pokemon1ChargeAttackStoryboard.Begin();
                        ViewModel.ChargeAvailableValue -= 1;
                    }));
            Task.Delay(13000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Pokemon2DamageStoryboard.Begin();
                    ViewModel.Pokemon2Health -= 10;
                }));
            Task.Delay(13000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1UnfocusStoryboard.Begin()));
            Task.Delay(14000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon2ChargeAttackStoryboard.Begin()));
            Task.Delay(15000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Pokemon1DamageStoryboard.Begin();
                    ViewModel.Pokemon1Health -= 100;
                }));
            Task.Delay(16000).ContinueWith(_ =>
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    Pokemon1DisappearStoryboard.Begin()));
        }
    }
}
