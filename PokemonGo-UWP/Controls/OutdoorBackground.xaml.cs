using PokemonGo_UWP.Utils.Helpers;
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

            if (!UIHelper.IsDaySky(DateTime.Now, TimeZoneInfo.Local, Utils.Helpers.LocationServiceHelper.Instance?.Geoposition))
            {
                BackgroundBrush.GradientStops[0].Color = UIHelper.ColorFromString("#FF01080E");
                BackgroundBrush.GradientStops[1].Color = UIHelper.ColorFromString("#FF01080E");
                BackgroundBrush.GradientStops[2].Color = UIHelper.ColorFromString("#FF02154D");
                BackgroundBrush.GradientStops[3].Color = UIHelper.ColorFromString("#FF182247");
                BackgroundBrush.GradientStops[4].Color = UIHelper.ColorFromString("#FF1684B9");
                BackgroundBrush.GradientStops[5].Color = UIHelper.ColorFromString("#FF6A86B5");
                DayPic.Visibility = Visibility.Collapsed;
                NightPic.Visibility =  Visibility.Visible;
            }
        }
        
    }
}
