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
    public sealed partial class OutdoorBackground : UserControl
    {
        public OutdoorBackground()
        {
            this.InitializeComponent();

            var currentTime = int.Parse(DateTime.Now.ToString("HH"));
            var dayTime = currentTime > 7 && currentTime < 19;
            if (!dayTime)
            {
                BackgroundBrush.GradientStops[0].Color = ColorFromString("#FF01080E");
                BackgroundBrush.GradientStops[1].Color = ColorFromString("#FF01080E");
                BackgroundBrush.GradientStops[2].Color = ColorFromString("#FF02154D");
                BackgroundBrush.GradientStops[3].Color = ColorFromString("#FF182247");
                BackgroundBrush.GradientStops[4].Color = ColorFromString("#FF1684B9");
                BackgroundBrush.GradientStops[5].Color = ColorFromString("#FF6A86B5");
                DayPic.Visibility = Visibility.Collapsed;
                NightPic.Visibility =  Visibility.Visible;
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
    }
}
