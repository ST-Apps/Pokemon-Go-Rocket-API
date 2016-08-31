using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Template10.Common;
using Template10.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Activation;
using PokemonGo_UWP.Utils;

namespace PokemonGo_UWP.Views
{
    public sealed partial class LoadingScreen : UserControl
    {
        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.Register(nameof(BusyText), typeof(string), typeof(LoadingScreen),
                new PropertyMetadata("LOADING..."));

        public static readonly DependencyProperty InfoMessageProperty =
            DependencyProperty.Register(nameof(InfoMessage), typeof(string), typeof(LoadingScreen),
                new PropertyMetadata(""));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(LoadingScreen), new PropertyMetadata(false));

        public LoadingScreen()
        {
            InitializeComponent();
        }

        public LoadingScreen(SplashScreen splashScreen) {
            InitializeComponent();
            BusyText = Utils.Resources.CodeResources.GetString("LadingMessage");
            ComputeInfoMessage(this);
            IsBusy = true;
        }

        public string BusyText
        {
            get { return (string) GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        public bool IsBusy
        {
            get { return (bool) GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public string InfoMessage
        {
            get { return (string)GetValue(InfoMessageProperty); }
            set { SetValue(InfoMessageProperty, value); }
        }

        // hide and show busy dialog
        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null) return;
                var view = modal.ModalContent as LoadingScreen;
                if (view == null)
                    modal.ModalContent = view = new LoadingScreen();
                modal.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                modal.ModalBackground = new SolidColorBrush(Colors.Transparent);
                if (busy && !view.IsBusy) {
                    ComputeInfoMessage(view);
                }
                modal.IsModal = view.IsBusy = busy;
                view.BusyText = text;
            });
        }

        private static Random random;
        private static void ComputeInfoMessage(LoadingScreen view) {
            random = new Random();
            // Sets the number of different images and messages.
            // The second parameter is the number for max count + 1 of images and messages.
            int number = random.Next(1, 3);
            view.BackgroundImageBrush.ImageSource =
                new BitmapImage(new Uri($"ms-appx:///Assets/Warnings/{number}.png"));
            view.InfoMessage = Utils.Resources.CodeResources.GetString($"WarningMessage{number}");
        }
    }
}