using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
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
        
        //private static readonly MediaPlayer GameplaySound = new MediaPlayer();
        //private static readonly MediaPlayer EncounterSound = new MediaPlayer();
        //private static readonly MediaPlayer PokemonFoundSound = new MediaPlayer();
        
        #endregion       

        /// <summary>
        /// Initializes audio assets by loading them from disk
        /// </summary>
        /// <returns></returns>
        static AudioUtils()
        {                        
            //// Get files and create elements   
            //GameplaySound.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Audio/{GAMEPLAY}"));
            //EncounterSound.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Audio/{ENCOUNTER_POKEMON}"));
            //PokemonFoundSound.Source = MediaSource.CreateFromUri(new Uri($"ms-appx:///Assets/Audio/{POKEMON_FOUND_DING}"));
            //// Set mode and volume
            //GameplaySound.AudioCategory = EncounterSound.AudioCategory = PokemonFoundSound.AudioCategory = MediaPlayerAudioCategory.GameMedia;
            //GameplaySound.IsLoopingEnabled = true;
            //ToggleSounds();
            
        }

        /// <summary>
        /// Sets volume based on settings
        /// </summary>
        public static void ToggleSounds()
        {                      
            //GameplaySound.IsMuted =
            //    EncounterSound.IsMuted = PokemonFoundSound.IsMuted = !SettingsService.Instance.IsMusicEnabled;
                
        }

        /// <summary>
        /// Plays the selected asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static void PlaySound(string asset)
        {            
            //switch (asset)
            //{
            //    case GAMEPLAY:
            //        if (GameplaySound.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
            //            GameplaySound.Play();
            //        StopSound(ENCOUNTER_POKEMON);                  
            //        break;
            //    case ENCOUNTER_POKEMON:
            //        GameplaySound.Pause();
            //        EncounterSound.Play();
            //        break;
            //    case POKEMON_FOUND_DING:
            //        PokemonFoundSound.Play();
            //        break;
            //}            
        }

        /// <summary>
        /// Stops the selected asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static void StopSound(string asset)
        {            
            //switch (asset)
            //{
            //    case GAMEPLAY:
            //        GameplaySound.Pause();
            //        GameplaySound.PlaybackSession.Position = TimeSpan.Zero;
            //        break;
            //    case ENCOUNTER_POKEMON:
            //        EncounterSound.Pause();
            //        EncounterSound.PlaybackSession.Position = TimeSpan.Zero;
            //        break;
            //    case POKEMON_FOUND_DING:
            //        PokemonFoundSound.Pause();
            //        PokemonFoundSound.PlaybackSession.Position = TimeSpan.Zero;
            //        break;
            //}            
        }

        /// <summary>
        /// Stops all playing sounds
        /// </summary>
        public static void StopSounds()
        {            
            //GameplaySound.Pause();
            //GameplaySound.PlaybackSession.Position = TimeSpan.Zero;
            //EncounterSound.Pause();
            //EncounterSound.PlaybackSession.Position = TimeSpan.Zero;
            //PokemonFoundSound.Pause();
            //PokemonFoundSound.PlaybackSession.Position = TimeSpan.Zero;            
        }
    }
}