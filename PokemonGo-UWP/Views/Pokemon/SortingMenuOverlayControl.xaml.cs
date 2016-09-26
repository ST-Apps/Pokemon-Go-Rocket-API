using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Template10.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PokemonGo_UWP.Views
{
    public sealed partial class SortingMenuOverlayControl : UserControl
    {
        public SortingMenuOverlayControl()
        {
            this.InitializeComponent();

            Loaded += (s, e) => 
            {
                SortingModeListView.ItemsSource = Enum.GetValues(typeof(PokemonSortingModes)).Cast<PokemonSortingModes>().ToList();
            };
        }

        /// <summary>
        /// Displays the sorting menu modally
        /// </summary>
        public void Show()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null)
                {
                    return;
                }

                _formerModalBrush = modal.ModalBackground;
                modal.ModalBackground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                modal.ModalContent = this;
                modal.IsModal = true;

                // animate
                Storyboard sb = this.Resources["ShowSortingMenuStoryboard"] as Storyboard;
                sb.Begin();
            });
        }

        #region Propertys

        private Brush _formerModalBrush = null;

        /// <summary>
        /// Event handling for selected SortingMode
        /// </summary>
        /// <param name="mode"></param>
        public delegate void SortingmodeSelectedHandler(PokemonSortingModes mode);
        public event SortingmodeSelectedHandler SortingmodeSelected;

        #endregion

        #region Internal methods

        private void SortingModeListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SortingmodeSelected?.Invoke((PokemonSortingModes)e.ClickedItem);
            Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Hide()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null)
                {
                    return;
                }

                // animate
                Storyboard sb = this.Resources["HideSortingMenuStoryboard"] as Storyboard;
                sb.Begin();
                sb.Completed += Cleanup;
            });
        }

        private void Cleanup(object sender, object e)
        {
            var modal = Window.Current.Content as ModalDialog;
            if (modal == null)
            {
                return;
            }

            modal.ModalBackground = _formerModalBrush;
            modal.ModalContent = null;
            modal.IsModal = false;
        }

        #endregion
    }
}
