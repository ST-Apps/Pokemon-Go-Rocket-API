using PokemonGo_UWP.Entities;
using System.Collections.ObjectModel;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Views
{
    public sealed partial class EggInventoryControl : UserControl
    {
        public EggInventoryControl()
        {
            this.InitializeComponent();
        }

        #region Propertys

        public static readonly DependencyProperty EggsInventoryProperty =
            DependencyProperty.Register(nameof(EggsInventory), typeof(ObservableCollection<PokemonDataWrapper>), typeof(EggInventoryControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EggSelectedCommandProperty =
            DependencyProperty.Register(nameof(EggSelectedCommand), typeof(DelegateCommand<PokemonDataWrapper>), typeof(EggInventoryControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CanShowIncubatorSelectionOverlayProperty =
            DependencyProperty.Register(nameof(CanShowIncubatorSelectionOverlay), typeof(bool), typeof(EggInventoryControl),
                new PropertyMetadata(null));

        public ObservableCollection<PokemonDataWrapper> EggsInventory
        {
            get { return (ObservableCollection<PokemonDataWrapper>)GetValue(EggsInventoryProperty); }
            set { SetValue(EggsInventoryProperty, value); }
        }

        public DelegateCommand<PokemonDataWrapper> EggSelectedCommand
        {
            get { return (DelegateCommand<PokemonDataWrapper>)GetValue(EggSelectedCommandProperty); }
            set { SetValue(EggSelectedCommandProperty, value); }
        }

        public bool CanShowIncubatorSelectionOverlay
        {
            get { return (bool)GetValue(CanShowIncubatorSelectionOverlayProperty); }
            set { SetValue(CanShowIncubatorSelectionOverlayProperty, value); }
        }

        #endregion

        #region Internal Methods

        private void ShowIncubatorSelection_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Enable this path of activation incubator via ItemsUseOnPokemonPage
            IncubatorSelectionOverlayControl incubatorControl = new IncubatorSelectionOverlayControl();
            incubatorControl.Show();
        }

        #endregion
    }
}
