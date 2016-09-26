using POGOProtos.Inventory;
using PokemonGo_UWP.Utils;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Common;
using Template10.Controls;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace PokemonGo_UWP.Views
{
    public sealed partial class IncubatorSelectionOverlayControl : UserControl
    {
        public IncubatorSelectionOverlayControl()
        {
            this.InitializeComponent();

            GameClient.IncubatorsInventory.CollectionChanged += LoadSortedIncubatorsInventory;

            LoadSortedIncubatorsInventory(this, null);
        }

        /// <summary>
        /// Displays the selection menu modally
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
                Storyboard sb = this.Resources["ShowIncubatorSelectionStoryboard"] as Storyboard;
                sb.Begin();
            });
        }

        #region Propertys

        private Brush _formerModalBrush = null;

        private ObservableCollection<EggIncubator> _incubatorsInventory = new ObservableCollection<EggIncubator>();
        public ObservableCollection<EggIncubator> IncubatorsInventory { get { return _incubatorsInventory; } }

        /// <summary>
        /// Event handling for selected Incubator
        /// </summary>
        /// <param name="mode"></param>
        public delegate void IncubatorSelectedHandler(EggIncubator incubator);
        public event IncubatorSelectedHandler IncubatorSelected;

        #endregion

        #region Internal methods

        private void LoadSortedIncubatorsInventory(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IncubatorsInventory.Clear();
            var orderedIncubators = GameClient.IncubatorsInventory
                .OrderBy(x => x.PokemonId != 0)
                .ThenBy(x => x.ItemId == POGOProtos.Inventory.Item.ItemId.ItemIncubatorBasicUnlimited)
                .ThenByDescending(x => x.UsesRemaining);
            foreach (var incubator in orderedIncubators)
            {
                IncubatorsInventory.Add(incubator);
            }
        }

        private void IncubatorGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedInc = (EggIncubator)e.ClickedItem;
            // catch if already used incubator clicked
            if (clickedInc.PokemonId != 0) return;
            IncubatorSelected?.Invoke(clickedInc);
            Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ShowStoreButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement store linking
            var dialog = new MessageDialog("Sorry, check back later 😉", "Not yet implemented");
            dialog.ShowAsync();
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
                Storyboard sb = this.Resources["HideIncubatorSelectionStoryboard"] as Storyboard;
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
