using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using PokemonGo.RocketAPI;

namespace PokemonGo_UWP.Utils.Helpers
{
    public class ImageCache
    {

        public const string CacheFolder = "_tilesCache";

        public static async Task<StorageFile> Load(Uri uri)
        {
            return await CacheManager(uri);
        }


        private static async Task<StorageFile> CacheManager(Uri uri)
        {
            // Initialize cache folder
            // Get the app's local folder.
            StorageFolder localFolder = ApplicationData.Current.LocalCacheFolder;
            // Create a new subfolder in the current folder.
            StorageFolder cacheFolder = await localFolder.CreateFolderAsync(CacheFolder, CreationCollisionOption.OpenIfExists);
            string key = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf("tiles", StringComparison.Ordinal) + 6).Replace("/", "_") + ".png";
            StorageFile file = null;
            if (await cacheFolder.TryGetItemAsync(key) == null)
            {
                Logger.Write("File " + key + " is not cached");
                byte[] bytes = await Download(uri);
                file = await cacheFolder.CreateFileAsync(key, CreationCollisionOption.GenerateUniqueName);
                await FileIO.WriteBytesAsync(file, bytes);
            }
            else
            {
                file = await cacheFolder.GetFileAsync(key);
            }
            return file;
        }

        public static async Task<Uri> GetLocalUriAsync(Uri uri)
        {
            //Try get the data from the cache
            var localFile = await CacheManager(uri);
            string localUri = string.Format("ms-appdata:///localcache/{0}/{1}", CacheFolder, localFile.Name);
            return new Uri(localUri);

        }

        public static async Task<byte[]> Download(Uri uri)
        {            
            HttpClient httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(uri);
        }
    }

}
