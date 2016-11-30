using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PokemonGo_UWP.Entities;
using PokemonGo_UWP.Utils;
using Template10.Services.NavigationService;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Common;

namespace PokemonGo_UWP.Views
{
    public sealed partial class AchievementDetailPage : Page
    {
        public AchievementDetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var AchievementID = Convert.ToString(AchevementID.Text);
            if (AchievementID == "jogger")
            {
                var currentText = AchievementValue.Text;
                decimal numVal = decimal.Parse(currentText);
                decimal correctText = Math.Round(numVal, 2);
                AchievementValue.Text = Convert.ToString(correctText);
            }
        }
    }

}
