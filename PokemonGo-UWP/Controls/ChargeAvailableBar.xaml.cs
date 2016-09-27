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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PokemonGo_UWP.Controls
{
    public sealed partial class ChargeAvailableBar : UserControl
    {
        #region "Dependency properties"
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(ChargeAvailableBar),
                                    new PropertyMetadata(0.0, OnPropertyChanged));
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(ChargeAvailableBar),
                                    new PropertyMetadata(0.0, OnPropertyChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ChargeAvailableBar)d).UpdateBar();
        }

        #endregion

        private void UpdateBar()
        {
            TextChargeAvailable.Text = Value.ToString("n1");
            TextMaxChargeAvailable.Text = MaxValue.ToString("n1");
        }

        public ChargeAvailableBar()
        {
            this.InitializeComponent();
            this.Loaded += ChargeAvailableBar_Loaded;
        }

        private void ChargeAvailableBar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBar();
        }
    }
}
