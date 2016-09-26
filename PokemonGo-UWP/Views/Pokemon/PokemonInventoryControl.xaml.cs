using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using System.Collections.ObjectModel;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Views
{
    public sealed partial class PokemonInventoryControl : UserControl
    {
        public PokemonInventoryControl()
        {
            this.InitializeComponent();
        }

        #region Propertys

        public static readonly DependencyProperty PokemonInventoryProperty =
            DependencyProperty.Register(nameof(PokemonInventory), typeof(ObservableCollection<PokemonDataWrapper>), typeof(PokemonInventoryControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PokemonSelectedCommandProperty =
            DependencyProperty.Register(nameof(PokemonSelectedCommand), typeof(DelegateCommand<PokemonDataWrapper>), typeof(PokemonInventoryControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SortingModeProperty =
            DependencyProperty.Register(nameof(SortingMode), typeof(PokemonSortingModes), typeof(PokemonInventoryControl),
                new PropertyMetadata(PokemonSortingModes.Combat));

        public ObservableCollection<PokemonDataWrapper> PokemonInventory
        {
            get { return (ObservableCollection<PokemonDataWrapper>)GetValue(PokemonInventoryProperty); }
            set { SetValue(PokemonInventoryProperty, value); }
        }

        public DelegateCommand<PokemonDataWrapper> PokemonSelectedCommand
        {
            get { return (DelegateCommand<PokemonDataWrapper>)GetValue(PokemonSelectedCommandProperty); }
            set { SetValue(PokemonSelectedCommandProperty, value); }
        }

        public PokemonSortingModes SortingMode
        {
            get { return (PokemonSortingModes)GetValue(SortingModeProperty); }
            set { SetValue(SortingModeProperty, value); }
        }

        #endregion

        #region Internal Methods

        private void ShowSortingPanel_Click(object sender, RoutedEventArgs e)
        {
            SortingMenuOverlayControl sortingMenu = new SortingMenuOverlayControl();
            sortingMenu.SortingmodeSelected += ((mode) => { SortingMode = mode; });
            sortingMenu.Show();
        }

        #endregion
    }
}
