using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.Views
{
    public sealed partial class EggDetailControl : UserControl
    {
        public EggDetailControl()
        {
            this.InitializeComponent();
        }

        private void ShowIncubatorSelection_Click(object sender, RoutedEventArgs e)
        {
            IncubatorSelectionOverlayControl incubatorControl = new IncubatorSelectionOverlayControl();
            incubatorControl.IncubatorSelected += IncubatorControl_IncubatorSelected;
            incubatorControl.Show();
        }

        private void IncubatorControl_IncubatorSelected(POGOProtos.Inventory.EggIncubator incubator)
        {
            IncubateEggCommand.Execute(incubator);
        }

        #region Dependency Propertys

        public static readonly DependencyProperty IncubateEggCommandProperty =
            DependencyProperty.Register(nameof(IncubateEggCommand), typeof(DelegateCommand), typeof(EggDetailControl),
                new PropertyMetadata(null));


        public DelegateCommand IncubateEggCommand
        {
            get { return (DelegateCommand)GetValue(IncubateEggCommandProperty); }
            set { SetValue(IncubateEggCommandProperty, value); }
        }

        #endregion
    }
}
