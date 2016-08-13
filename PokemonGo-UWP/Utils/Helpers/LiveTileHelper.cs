using NotificationsExtensions.Tiles;
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
            tile.Visual.TileMedium = GetPeopleBinding(urls, 12);
            // Recommended to use 15 photos on Wide
            tile.Visual.TileWide = GetPeopleBinding(urls, 18);
            // Recommended to use 20 photos on Large
            tile.Visual.TileLarge = GetPeopleBinding(urls, 28);

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
                    Branding = TileBranding.NameAndLogo
                }
            };
        }

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

    }

}