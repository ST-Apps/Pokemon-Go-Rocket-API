using Windows.UI.Xaml.Controls;

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
            var AchievementID = Convert.ToString(AchevementID.text);
            if (AchievementID == "jogger")
            {
                int currentText = AchievementValue.Text;
                int correctText = Math.Round(Int32.Parse(currentText), 2, MidpointRounding.ToEven);
                AchievementValue.Text = Convert.ToString(correctText);
            }
        }
    }

}
