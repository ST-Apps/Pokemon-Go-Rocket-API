using POGOProtos.Enums;
using PokemonGo_UWP.Utils.Helpers;
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

            if (!UIHelper.IsDaySky(DateTime.Now, TimeZoneInfo.Local, Utils.Helpers.LocationServiceHelper.Instance?.Geoposition))
            {
                ((LinearGradientBrush)Floor.Fill).GradientStops[0].Color = UIHelper.ColorFromString("#FF306680");
                ((LinearGradientBrush)Floor.Fill).GradientStops[1].Color = UIHelper.ColorFromString("#FF133339");
            }
        }

        private void UpdateColor()
        {
            switch(Team)
            {
                case TeamColor.Red:
                    OuterEdgeLight.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FFFF0505"));
                    OuterEdgeDark.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FFA20202"));
                    break;
                case TeamColor.Yellow:
                    OuterEdgeLight.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FFFFEC00"));
                    OuterEdgeDark.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FFD3AB00"));
                    break;
                case TeamColor.Blue:
                    OuterEdgeLight.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FF3090FF"));
                    OuterEdgeDark.Fill = new SolidColorBrush(UIHelper.ColorFromString("#FF3090FF"));
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
