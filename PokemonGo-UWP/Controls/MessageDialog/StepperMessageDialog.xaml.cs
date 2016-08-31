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


namespace PokemonGo_UWP.Controls
{
    public sealed partial class StepperMessageDialog : UserControl
    {
        public StepperMessageDialog()
        {
            this.InitializeComponent();
        }

        public StepperMessageDialog(int minValue, int maxValue, int value) :this()
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
        }

        #region Propertys

        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(PoGoMessageDialog),
                new PropertyMetadata(1));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private int _minValue;
        public int MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }

        private int _maxValue;
        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        #endregion

        #region Events

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            if(Value > MinValue)
            {
                Value--;
            }
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            if(Value < MaxValue)
            {
                Value++;
            }
        }

        #endregion
    }
}