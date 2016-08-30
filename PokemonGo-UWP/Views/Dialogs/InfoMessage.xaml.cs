using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Template10.Common;
using Template10.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace PokemonGo_UWP.Views
{
    public sealed partial class InfoMessage : UserControl
    {
        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.Register(nameof(BusyText), typeof(string), typeof(InfoMessage),
                new PropertyMetadata("Please wait..."));

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(nameof(IsBusy), typeof(bool), typeof(InfoMessage), new PropertyMetadata(false));

        public InfoMessage()
        {
            InitializeComponent();
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

        // hide and show busy dialog
        public static void SetBusy(bool busy, string text = null)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null) return;
                var view = modal.ModalContent as InfoMessage;
                if (view == null)
                    modal.ModalContent = view = new InfoMessage();
                modal.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                modal.ModalBackground = new SolidColorBrush(Colors.Transparent);
                modal.IsModal = view.IsBusy = busy;
                view.BusyText = text;
            });
        }
    }
}