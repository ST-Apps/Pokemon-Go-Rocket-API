using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using POGOProtos.Inventory;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EggDetailPage : Page
    {
        public EggDetailPage()
        {
            this.InitializeComponent();
            // Setup incubators translation
            Loaded += (s, e) =>
            {
                ShowIncubatorsModalAnimation.From =
                    HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
                HideIncubatorsModalStoryboard.Completed += (ss, ee) =>
                {
                    IncubatorsModal.IsModal = false;
                };
            };
        }

        #region Overrides of Page

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SubscribeToCaptureEvents();
        }


        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            UnsubscribeToCaptureEvents();
        }

        #endregion

        #region Handlers

        private void SubscribeToCaptureEvents()
        {
            ViewModel.IncubatorSuccess += ViewModelOnIncubatorSuccess;
        }

        private void UnsubscribeToCaptureEvents()
        {
            ViewModel.IncubatorSuccess -= ViewModelOnIncubatorSuccess;
        }

        private void ViewModelOnIncubatorSuccess(object sender, EventArgs eventArgs)
        {
            // Replace image
            // TODO: doing this with code-behind maybe?
            //EggImage.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Items/{(int) ViewModel.SelectedEggIncubator.ItemId}"));            
        }        

        #endregion

        private void ToggleIncubatorModel(object sender, TappedRoutedEventArgs e)
        {
            if (IncubatorsModal.IsModal)
            {
                HideIncubatorsModalStoryboard.Begin();
            }
            else
            {
                IncubatorsModal.IsModal = true;
                ShowIncubatorsModalStoryboard.Begin();
            }
        }

        // TODO: replace with mvvm command, doing like this because I'm in a rush
        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.UseIncubatorCommand.Execute((EggIncubator)e.ClickedItem);
            HideIncubatorsModalStoryboard.Begin();
        }
    }
}
