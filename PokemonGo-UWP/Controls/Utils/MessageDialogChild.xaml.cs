using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static PokemonGo_UWP.Controls.MessageDialog;

namespace PokemonGo_UWP.Controls {
    sealed partial class MessageDialogChild : UserControl {

        #region Constructors

        public MessageDialogChild(MessageDialogType type, String message) {
            this.InitializeComponent();

            // Workaround, otherwise message is not display
            MessageTextBlock.Text = message;

            this.Type = type;
            this.Message = message;
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
        }

        #endregion

        #region Shared Logic

        private void ControlButtons() {
            switch(Type) {
                case MessageDialogType.OkOnly:
                    YesButton.Visibility = Visibility.Collapsed;
                    NoButton.Visibility = Visibility.Collapsed;
                    OkButton.Visibility = Visibility.Visible;
                    break;
                case MessageDialogType.YesNo:
                    YesButton.Visibility = Visibility.Visible;
                    NoButton.Visibility = Visibility.Visible;
                    OkButton.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new ArgumentException("The MessageDialogType '" + Type.ToString() + "' is currently not supported");
            }
        }

        #endregion

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("InnerSegmentColor", typeof(string), typeof(CircularProgressBar), null);

        #region Vars

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public MessageDialogType Type { get; private set; }

        #endregion

        #region Events

        /// <summary>
        ///     Event fired user pressed YES
        /// </summary>
        public event EventHandler YesButtonTapped;

        /// <summary>
        ///     Event fired user pressed NO
        /// </summary>
        public event EventHandler NoButtonTapped;

        /// <summary>
        ///     Event fired user pressed OK
        /// </summary>
        public event EventHandler OkButtonTapped;

        #endregion

        #region Event Handlers

        private void YesButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            YesButtonTapped?.Invoke(this, null);
        }

        private void NoButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            NoButtonTapped?.Invoke(this, null);
        }

        private void OkButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            OkButtonTapped?.Invoke(this, null);
        }


        #endregion

    }
}
