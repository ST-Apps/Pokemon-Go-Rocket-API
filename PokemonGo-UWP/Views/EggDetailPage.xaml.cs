using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using POGOProtos.Inventory;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EggDetailPage : Page
    {
        public EggDetailPage()
        {
            InitializeComponent();
            // Setup incubators translation
            Loaded += (s, e) =>
            {
                ShowIncubatorsModalAnimation.From =
                    HideIncubatorsModalAnimation.To = IncubatorsModal.ActualHeight;
                HideIncubatorsModalStoryboard.Completed += (ss, ee) => { IncubatorsModal.IsModal = false; };
            };
        }

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
            ViewModel.UseIncubatorCommand.Execute((EggIncubator) e.ClickedItem);
            HideIncubatorsModalStoryboard.Begin();
        }

    }
}