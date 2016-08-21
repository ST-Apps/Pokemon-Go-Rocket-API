
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PokemonGo_UWP.Controls {
    public class MessageDialog {
        /// <summary>
        /// Enum for Buttons, which will be shown.
        /// </summary>
        public enum MessageDialogType {
            OkOnly,
            YesNo
        }

        /// <summary>
        /// Enum for dialog results.
        /// </summary>
        public enum MessageDialogResult {
            Ok,
            Yes,
            No
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Type of Buttons, which will be shown</param>
        /// <param name="message">Message, which will be shown</param>
        public MessageDialog(MessageDialogType type, string message) {
            MessageDialogChild dialog = new MessageDialogChild(type, message);
            dialog.YesButtonTapped += Dialog_YesButtonTapped;
            dialog.NoButtonTapped += Dialog_NoButtonTapped;
            dialog.OkButtonTapped += Dialog_OkButtonTapped;
            Dialog.Content = dialog;
        }

        #region Vars

        /// <summary>
        /// Content Dialog, which handles the User Control MessageDialogChild.
        /// </summary>
        private ContentDialog Dialog { get; set; } = new ContentDialog();

        public MessageDialogResult Result { get; set; }

        #endregion

        #region Logic

        /// <summary>
        /// Shows the MessageDialog asynchornously.
        /// </summary>
        /// <returns>Result, which button was pressed</returns>
        public async Task<MessageDialogResult> ShowAsync() {
            CreateDialogStyle();
            await Dialog.ShowAsync();
            return Result;
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void Hide() {
            Dialog.Hide();
        }

        /// <summary>
        /// Create the Style for the Content dialog
        /// </summary>
        private void CreateDialogStyle() {
            Dialog.Padding = new Thickness(0);
            Dialog.Margin = new Thickness(0);
            Dialog.BorderThickness = new Thickness(0);
            SolidColorBrush brush = new SolidColorBrush(Colors.Transparent);
            brush.Opacity = 0.5;

            Dialog.Background = brush;
        }

        #endregion

        #region Events

        /// <summary>
        ///     Event fired, if user pressed YES
        /// </summary>
        public event EventHandler YesButtonTapped;

        /// <summary>
        ///     Event fired, if user pressed NO
        /// </summary>
        public event EventHandler NoButtonTapped;

        /// <summary>
        ///     Event fired, if user pressed OK
        /// </summary>
        public event EventHandler OkButtonTapped;

        #endregion

        #region Event Handlers

        private void Dialog_YesButtonTapped(object sender, EventArgs e) {
            YesButtonTapped?.Invoke(this, null);
            Result = MessageDialogResult.Yes;
            this.Hide();
        }

        private void Dialog_NoButtonTapped(object sender, EventArgs e) {
            NoButtonTapped?.Invoke(this, null);
            Result = MessageDialogResult.No;
            this.Hide();
        }

        private void Dialog_OkButtonTapped(object sender, EventArgs e) {
            OkButtonTapped?.Invoke(this, null);
            Result = MessageDialogResult.Ok;
            this.Hide();
        }

        #endregion
    }
}
