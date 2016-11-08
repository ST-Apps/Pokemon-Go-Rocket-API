using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace PokemonGo_UWP.Controls
{
    public sealed partial class HealthIndicatorBar : UserControl
    {
        #region "Dependency properties"
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(HealthIndicatorBar),
                                    new PropertyMetadata(0, OnPropertyChanged));
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(HealthIndicatorBar),
                                    new PropertyMetadata(100, OnPropertyChanged));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HealthIndicatorBar)d).UpdateBar();
        }
        #endregion

        private static SolidColorBrush LimeBrush = new SolidColorBrush(Colors.Lime);
        private static SolidColorBrush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private static SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);

        private void UpdateBar()
        {
            double valueScale = (MaxValue == 0) ? 0 : (double)this.Value / MaxValue;
            if (valueScale < 0) valueScale = 0;
            else if (valueScale > 1) valueScale = 1;
            ((ScaleTransform)HealthBar.RenderTransform).ScaleX = valueScale;

            if (valueScale==1)
            {
                HealthBar.CornerRadius = new CornerRadius(5);
                DampedBar.CornerRadius = new CornerRadius(5);
            }
            else
            {
                HealthBar.CornerRadius = new CornerRadius(5, 0, 0, 5);
                DampedBar.CornerRadius = new CornerRadius(5, 0, 0, 5);
                FlashStoryboard.Begin();
            }
            if (valueScale > 0.6) HealthBar.Background = LimeBrush;
            else if (valueScale > 0.3) HealthBar.Background = YellowBrush;
            else HealthBar.Background = RedBrush;
            ((DoubleAnimation)DampStoryboard.Children[0]).To = valueScale;
            DampStoryboard.Begin();
        }

        public HealthIndicatorBar()
        {
            this.InitializeComponent();
            this.Loaded += HealthIndicatorBar_Loaded;
        }

        private void HealthIndicatorBar_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBar();
        }
    }
}
