using NotificationsExtensions;
using NotificationsExtensions.Tiles;
using PokemonGo_UWP.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGo_UWP.Utils
{

    /// <summary>
    /// A set of helpers to easily create Live Tiles.
    /// </summary>
    public static class LiveTileHelper
    {

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static TileContent GetImageTile(string image)
        {
            var tile = GetTile();

            tile.Visual.TileSmall = GetImageBinding(image);
            tile.Visual.TileMedium = GetImageBinding(image);
            tile.Visual.TileWide = GetImageBinding(image);
            tile.Visual.TileLarge = GetImageBinding(image);

            return tile;
        }

        /// <summary>
        /// Gets a Live Tile containing a "peek" template that renders like the Me tile.
        /// </summary>
        /// <param name="urls">
        ///     A <see cref="List{string}"/> containing the URLs to use for the tile images. May be app-relative or internet URLs.
        /// </param>
        /// <returns>A populated <see cref="TileContent"/> object suitable for submitting to a TileUpdateManager.</returns>
        /// <remarks>https://msdn.microsoft.com/windows/uwp/controls-and-patterns/tiles-and-notifications-special-tile-templates-catalog</remarks>
        public static TileContent GetPeekTile(PokemonDataWrapper pokemon)
        {
            var tile = GetTile();

            tile.Visual.TileSmall = GetPeekBindingSmall(pokemon);
            tile.Visual.TileMedium = GetPeekBindingMedium(pokemon);
            tile.Visual.TileWide = GetPeekBindingWide(pokemon);
            tile.Visual.TileLarge = GetPeekBindingLarge(pokemon);

            return tile;
        }


        /// <summary>
        /// Gets a Live Tile containing multiple cropped-circle images that render like the People Hub tile.
        /// </summary>
        /// <param name="urls">
        ///     A <see cref="List{string}"/> containing the URLs to use for the tile images. May be app-relative or internet URLs.
        /// </param>
        /// <returns>A populated <see cref="TileContent"/> object suitable for submitting to a TileUpdateManager.</returns>
        /// <remarks>https://msdn.microsoft.com/windows/uwp/controls-and-patterns/tiles-and-notifications-special-tile-templates-catalog</remarks>
        public static TileContent GetPeopleTile(List<string> urls)
        {
            var tile = GetTile();

            // Recommended to use 9 photos on Medium
            tile.Visual.TileMedium = GetPeopleBinding(urls, 15);
            // Recommended to use 15 photos on Wide
            tile.Visual.TileWide = GetPeopleBinding(urls, 22);
            // Recommended to use 20 photos on Large
            tile.Visual.TileLarge = GetPeopleBinding(urls, 30);

            return tile;
        }

        /// <summary>
        /// Gets a Live Tile containing images that render like the Photos Hub tile.
        /// </summary>
        /// <param name="urls">
        ///     A <see cref="List{string}"/> containing the URLs to use for the tile images. May be app-relative or internet URLs.
        /// </param>
        /// <returns>A populated <see cref="TileContent"/> object suitable for submitting to a TileUpdateManager.</returns>
        /// <remarks>https://msdn.microsoft.com/windows/uwp/controls-and-patterns/tiles-and-notifications-special-tile-templates-catalog</remarks>
        public static TileContent GetPhotosTile(List<string> urls)
        {
            var tile = GetTile();
            var binding = GetPhotosBinding(urls);

            tile.Visual.TileMedium = binding;
            tile.Visual.TileWide = binding;
            tile.Visual.TileLarge = binding;

            return tile;
        }

        #endregion

        #region Private Methods

        #region Adaptive Helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static AdaptiveText GetCenteredAdaptiveText(string text, AdaptiveTextStyle style = AdaptiveTextStyle.CaptionSubtle)
        {
            return new AdaptiveText()
            {
                Text = text,
                HintWrap = true,
                HintAlign = AdaptiveTextAlign.Center,
                HintStyle = style
            };
        }

        #endregion

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetImageBinding(string imageSource)
        {
            var content = new TileBindingContentAdaptive()
            {
                Children =
                {
                    new AdaptiveImage
                    {
                        Source = imageSource,
                        HintRemoveMargin = true,
                        HintAlign = AdaptiveImageAlign.Center
                    }
                }

            };

            return new TileBinding()
            {
                Content = content
            };

        }

        #region PeekTile Helpers

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetPeekBinding(string imageSource, params ITileAdaptiveChild[] children)
        {
            var content = new TileBindingContentAdaptive()
            {
                PeekImage = new TilePeekImage()
                {
                    Source = imageSource
                }
            };

            foreach (var child in children)
            {
                content.Children.Add(child);
            }

            return new TileBinding()
            {
                Content = content
            };

        }

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetPeekBindingSmall(PokemonDataWrapper pokemon)
        {
            return GetPeekBinding(
                $"{(int)pokemon.PokemonId}.png",
                GetCenteredAdaptiveText($"CP: {pokemon.Cp}", AdaptiveTextStyle.Caption),
                GetCenteredAdaptiveText($"{(pokemon.Stamina / pokemon.StaminaMax) * 100}%", AdaptiveTextStyle.Caption)
            );
        }

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetPeekBindingMedium(PokemonDataWrapper pokemon)
        {
            return GetPeekBinding(
                $"{(int)pokemon.PokemonId}.png",
                GetCenteredAdaptiveText(Resources.Pokemon.GetString(pokemon.PokemonId.ToString()), AdaptiveTextStyle.Body),
                GetCenteredAdaptiveText($"CP: {pokemon.Cp}"),
                GetCenteredAdaptiveText($"HP: {(pokemon.Stamina / pokemon.StaminaMax) * 100}%")
            );
        }

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetPeekBindingWide(PokemonDataWrapper pokemon)
        {
            return GetPeekBinding(
                $"{(int)pokemon.PokemonId}.png",
                GetCenteredAdaptiveText(Resources.Pokemon.GetString(pokemon.PokemonId.ToString()), AdaptiveTextStyle.Body),
                GetCenteredAdaptiveText($"Combat Points: {pokemon.Cp}"),
                GetCenteredAdaptiveText($"Stamina: {(pokemon.Stamina / pokemon.StaminaMax) * 100}%")
            );
        }

        /// <summary>
        /// Generates a <see cref="TileBindingContentAdaptive"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="pokemon">
        ///     A <see cref="PokemonDataWrapper"/> containing the Pokemon to generate a tile for.
        /// </param>
        /// <returns></returns>
        /// <remarks>Original contribution from sam9116 (https://github.com/ST-Apps/PoGo-UWP/pull/626/files)</remarks>
        private static TileBinding GetPeekBindingLarge(PokemonDataWrapper pokemon)
        {
            return GetPeekBinding(
                $"{(int)pokemon.PokemonId}.png",
                GetCenteredAdaptiveText(Resources.Pokemon.GetString(pokemon.PokemonId.ToString()), AdaptiveTextStyle.Title),
                GetCenteredAdaptiveText($"Combat Points: {pokemon.Cp}", AdaptiveTextStyle.BodySubtle),
                GetCenteredAdaptiveText($"Stamina: {(pokemon.Stamina / pokemon.StaminaMax) * 100}%", AdaptiveTextStyle.BodySubtle)
            );
        }

        #endregion

        #region Built-In Tile Template Helpers

        /// <summary>
        /// Generates a <see cref="TileBindingContentPeople"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="urls">
        ///     A <see cref="List{string}"/> containing the URLs to use for the tile images. May be app-relative or internet URLs.
        /// </param>
        /// <param name="maxCount">The maximum number of images to use for this particular binding size.</param>
        /// <returns></returns>
        private static TileBinding GetPeopleBinding(List<string> urls, int maxCount = 25)
        {

            var content = new TileBindingContentPeople();

            foreach (var url in urls.Take(maxCount))
            {
                content.Images.Add(new TileImageSource(url));
            }

            return new TileBinding()
            {
                Content = content
            };

        }

        /// <summary>
        /// Generates a <see cref="TileBindingContentPhotos"/> for a given list of image URLs. 
        /// </summary>
        /// <param name="urls">
        ///     A <see cref="List{string}"/> containing the URLs to use for the tile images. May be app-relative or internet URLs.
        /// </param>
        /// <param name="maxCount">The maximum number of images to use for this particular binding size.</param>
        /// <returns></returns>
        private static TileBinding GetPhotosBinding(List<string> urls, int maxCount = 12)
        {

            var content = new TileBindingContentPhotos();

            foreach (var url in urls.Take(maxCount))
            {
                content.Images.Add(new TileImageSource(url));
            }

            return new TileBinding()
            {
                Content = content
            };

        }

        #endregion

        #region Stuff bring submitted to next version of NotificationsExtensions

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static TileContent GetTile()
        {
            // Create the notification content
            return new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    BaseUri = new Uri("Assets/Pokemons/", UriKind.Relative)
                }
            };
        }

        #endregion

        #endregion

    }

}