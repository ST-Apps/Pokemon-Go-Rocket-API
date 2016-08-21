using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PokemonGo_UWP.Utils
{
    public static class AudioUtils
    {

        #region Audio Files

        public const string GAMEPLAY = "Gameplay.mp3";
        public const string ENCOUNTER_POKEMON = "EncounterPokemon.mp3";
        public const string POKEMON_FOUND_DING = "pokemon_found_ding.wav";

        #endregion

        #region Media Elements
        private static readonly MediaElement GameplaySound = new MediaElement();
        private static readonly MediaElement EncounterSound = new MediaElement();
        private static readonly MediaElement PokemonFoundSound = new MediaElement();
        #endregion       

        /// <summary>
        /// Initializes audio assets by loading them from disk
        /// </summary>
        /// <returns></returns>
        public static async Task Init()
        {
            // Get folder
            var folder =
                    await (await Package.Current.InstalledLocation.GetFolderAsync("Assets")).GetFolderAsync("Audio");
            ToggleSounds();
            // Get files and create elements   
            var currentFile = await folder.GetFileAsync(GAMEPLAY);
            var currentStream = await currentFile.OpenAsync(FileAccessMode.Read);
            GameplaySound.SetSource(currentStream, currentFile.ContentType);

            currentFile = await folder.GetFileAsync(ENCOUNTER_POKEMON);
            currentStream = await currentFile.OpenAsync(FileAccessMode.Read);
            EncounterSound.SetSource(currentStream, currentFile.ContentType);

            currentFile = await folder.GetFileAsync(POKEMON_FOUND_DING);
            currentStream = await currentFile.OpenAsync(FileAccessMode.Read);
            PokemonFoundSound.SetSource(currentStream, currentFile.ContentType);
            // Set mode and volume
            GameplaySound.IsLooping = true;
        }

        /// <summary>
        /// Sets volume based on settings
        /// </summary>
        public static void ToggleSounds()
        {
            // TODO: not working yet for some weird reasons
            GameplaySound.IsMuted =
                EncounterSound.IsMuted = PokemonFoundSound.IsMuted = !SettingsService.Instance.IsMusicEnabled;
        }

        /// <summary>
        /// Plays the selected asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static void PlaySound(string asset)
        {
            switch (asset)
            {
                case GAMEPLAY:
                    if (GameplaySound.CurrentState != MediaElementState.Playing)
                        GameplaySound.Play();
                    EncounterSound.Stop();
                    break;
                case ENCOUNTER_POKEMON:
                    GameplaySound.Pause();
                    EncounterSound.Play();
                    break;
                case POKEMON_FOUND_DING:
                    PokemonFoundSound.Play();
                    break;
            }
        }

        /// <summary>
        /// Stops the selected asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static void StopSound(string asset)
        {
            switch (asset)
            {
                case GAMEPLAY:
                    GameplaySound.Stop();
                    break;
                case ENCOUNTER_POKEMON:
                    EncounterSound.Stop();
                    break;
                case POKEMON_FOUND_DING:
                    PokemonFoundSound.Stop();
                    break;
            }
        }

        /// <summary>
        /// Stops all playing sounds
        /// </summary>
        public static void StopSounds()
        {
            GameplaySound.Stop();
            EncounterSound.Stop();
            PokemonFoundSound.Stop();
        }
    }
}