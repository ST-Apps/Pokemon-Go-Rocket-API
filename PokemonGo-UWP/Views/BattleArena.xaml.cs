using POGOProtos.Enums;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PokemonGo_UWP.Views
{
    public sealed partial class BattleArena : UserControl
    {
        public static readonly DependencyProperty TeamProperty =
            DependencyProperty.Register(nameof(Team), typeof(TeamColor), typeof(BattleArena),
                                    new PropertyMetadata(0, OnPropertyChanged));

        public TeamColor Team
        {
            get { return (TeamColor)GetValue(TeamProperty); }
            set { SetValue(TeamProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BattleArena)d).UpdateColor();
        }

        public BattleArena()
        {
            this.InitializeComponent();

            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            var dayTime = currentTime > 7 && currentTime < 19;
            if (!dayTime)
            {
                ((LinearGradientBrush)Floor.Fill).GradientStops[0].Color = ColorFromString("#FF306680");
                ((LinearGradientBrush)Floor.Fill).GradientStops[1].Color = ColorFromString("#FF133339");
            }
        }
        private Windows.UI.Color ColorFromString(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            Windows.UI.Color color = Windows.UI.Color.FromArgb(a, r, g, b);
            return color;
        }

        private void UpdateColor()
        {
            switch(Team)
            {
                case TeamColor.Red:
                    OuterEdgeLight.Fill = new SolidColorBrush(ColorFromString("#FFFF0505"));
                    OuterEdgeDark.Fill = new SolidColorBrush(ColorFromString("#FFA20202"));
                    break;
                case TeamColor.Yellow:
                    OuterEdgeLight.Fill = new SolidColorBrush(ColorFromString("#FFFFEC00"));
                    OuterEdgeDark.Fill = new SolidColorBrush(ColorFromString("#FFD3AB00"));
                    break;
                case TeamColor.Blue:
                    OuterEdgeLight.Fill = new SolidColorBrush(ColorFromString("#FF3090FF"));
                    OuterEdgeDark.Fill = new SolidColorBrush(ColorFromString("#FF3090FF"));
                    break;
                default:
                    OuterEdgeLight.Fill = new SolidColorBrush(Colors.Silver);
                    OuterEdgeDark.Fill = new SolidColorBrush(Colors.Gray);
                    break;
            }
            InnerEdgeLight.Fill = OuterEdgeLight.Fill;
            InnerEdgeDark.Fill = OuterEdgeDark.Fill;
        }

    }
}
