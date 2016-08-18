using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace PokemonGo_UWP.Utils
{
    public static class AudioUtils
    {
        private static readonly MediaElement NormalSounds = new MediaElement();
        private static readonly MediaElement CaptureSound = new MediaElement();
        private static bool _isPlaying;

        public static async Task PlaySound(string asset)
        {
            if (SettingsService.Instance.IsMusicEnabled && !_isPlaying)
            {
                if (asset == "Gameplay.mp3")
                {
                    NormalSounds.IsLooping = true;
                }
                var folder =
                    await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
                var file = await folder.GetFileAsync(asset);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                NormalSounds.Volume = 1;
                NormalSounds.SetSource(stream, file.ContentType);
                _isPlaying = true;
                NormalSounds.Play();
            }
        }

        public static async Task PlaySoundCapture(string asset)
        {
            if (SettingsService.Instance.IsMusicEnabled)
            {
                NormalSounds.Pause();

                var folder =
                    await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
                var file = await folder.GetFileAsync(asset);
                var stream = await file.OpenAsync(FileAccessMode.Read);
                CaptureSound.Volume = 1;
                CaptureSound.SetSource(stream, file.ContentType);
                CaptureSound.Play();
            }
        }

        public static void StopSounds()
        {
            CaptureSound.Stop();
            NormalSounds.Stop();            
        }
    }
}