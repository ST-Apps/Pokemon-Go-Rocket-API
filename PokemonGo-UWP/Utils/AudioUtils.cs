using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace PokemonGo_UWP.Utils
{
    public static class AudioUtils
    {
        public static async Task PlaySound(string asset)
        {
            var mysong = new Windows.UI.Xaml.Controls.MediaElement();
            var folder = await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
            var file = await folder.GetFileAsync(asset);
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            mysong.Volume = 1;
            mysong.SetSource(stream, file.ContentType);
            mysong.Play();
        }
    }
}
