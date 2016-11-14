using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using PokemonGo_UWP.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PokemonGo_UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChallengePage : Page
    {
        public string ChallengeUri;
        public ChallengePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ChallengeUri = ((JObject)JsonConvert.DeserializeObject((string)e.Parameter)).Last
                        .ToObject<string>();

            if (ChallengeUri.Length > 0 && ChallengeUri[0] == '"')
                ChallengeUri = ChallengeUri.Substring(1);

            if (ChallengeUri.Length > 0 && ChallengeUri[ChallengeUri.Length-1] == '"')
                ChallengeUri = ChallengeUri.Substring(0, ChallengeUri.Length - 1);

            ChallengePanel.Navigate(new Uri(ChallengeUri));
            Busy.SetBusy(false);
        }

        private void ChallengePanel_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string url = e.Uri.ToString();
            if (url.StartsWith("unity:"))
            {
                string token = url.Substring("unity:".Length);
                Verify(token);
            }
            else
            {
                //Error, no token!
            }
        }
        private async void Verify(string token)
        {
            await GameClient.VerifyChallenge(token);
            //BootStrapper.Current.NavigationService.Navigate(typeof(GameMapPage));
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        private void ChallengePanel_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {

            string url = args.Uri.ToString();
            if (url.StartsWith("unity:"))
            {
                string token = url.Substring("unity:".Length);
                Verify(token);
                args.Handled = true;
            }
        }
    }
}
