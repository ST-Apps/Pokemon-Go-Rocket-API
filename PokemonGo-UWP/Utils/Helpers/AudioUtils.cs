using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System;

namespace PokemonGo_UWP.Utils
{
    public static class AudioUtils
    {
        public static async Task PlaySound(string asset)
        {
            var mysong = new MediaElement();
            var folder =
                await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
            var file = await folder.GetFileAsync(asset);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            mysong.Volume = 1;
            mysong.SetSource(stream, file.ContentType);
            mysong.Play();
        }
    }
}