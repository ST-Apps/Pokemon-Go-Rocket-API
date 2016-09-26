using POGOProtos.Settings.Master;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Mvvm;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace PokemonGo_UWP.Views
{
    public sealed partial class PokemonDetailControl : UserControl
    {
        public PokemonDetailControl()
        {
            this.InitializeComponent();

            DataContextChanged += DataContextChangedEvent;

            PokemonTypeCol.MinWidth = PokemonTypeCol.ActualWidth;
            PokemonTypeCol.Width = new GridLength(1, GridUnitType.Star);
        }

        private void DataContextChangedEvent(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ContentScroll.ChangeView(0.0, 0.0, 1.0f);
        }

        #region Dependency Propertys

        public static readonly DependencyProperty FavoritePokemonCommandProperty = 
            DependencyProperty.Register(nameof(FavoritePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RenamePokemonCommandProperty =
            DependencyProperty.Register(nameof(RenamePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PowerUpPokemonCommandProperty =
            DependencyProperty.Register(nameof(PowerUpPokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty EvolvePokemonCommandProperty =
            DependencyProperty.Register(nameof(EvolvePokemonCommand), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public static readonly DependencyProperty StardustAmountProperty =
            DependencyProperty.Register(nameof(StardustAmount), typeof(int), typeof(PokemonDetailControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty PokemonExtraDataProperty =
            DependencyProperty.Register(nameof(PokemonExtraData), typeof(DelegateCommand), typeof(PokemonDetailControl),
                new PropertyMetadata(null));

        public DelegateCommand FavoritePokemonCommand
        {
            get { return (DelegateCommand)GetValue(FavoritePokemonCommandProperty); }
            set { SetValue(FavoritePokemonCommandProperty, value); }
        }

        public DelegateCommand RenamePokemonCommand
        {
            get { return (DelegateCommand)GetValue(RenamePokemonCommandProperty); }
            set { SetValue(RenamePokemonCommandProperty, value); }
        }

        public DelegateCommand PowerUpPokemonCommand
        {
            get { return (DelegateCommand)GetValue(PowerUpPokemonCommandProperty); }
            set { SetValue(PowerUpPokemonCommandProperty, value); }
        }

        public DelegateCommand EvolvePokemonCommand
        {
            get { return (DelegateCommand)GetValue(EvolvePokemonCommandProperty); }
            set { SetValue(EvolvePokemonCommandProperty, value); }
        }

        public int StardustAmount
        {
            get { return (int)GetValue(StardustAmountProperty); }
            set { SetValue(StardustAmountProperty, value); }
        }

        public PokemonSettings PokemonExtraData
        {
            get { return (PokemonSettings)GetValue(PokemonExtraDataProperty); }
            set { SetValue(PokemonExtraDataProperty, value); }
        }

        #endregion

    }
}
