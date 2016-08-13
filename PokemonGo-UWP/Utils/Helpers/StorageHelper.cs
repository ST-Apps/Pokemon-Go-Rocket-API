using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Windows.Storage.Streams;

#if WINDOWS_PHONE
using Q42.WinRT.Phone;
#endif

namespace Q42.WinRT.Storage
{
    /// <summary>
    /// Possible storage locations
    /// </summary>
    public enum StorageType
    {

        /// <summary>Local</summary>
        Local,

#if NETFX_CORE
        /// <summary>Temporary</summary>
        Temporary,
        /// <summary>Roaming</summary>
        Roaming,
#endif
    }

    /// <summary>
    /// Type of Serializer used
    /// </summary>
    public enum StorageSerializer
    {
        /// <summary>JSON</summary>
        JSON,
        /// <summary>XML</summary>
        XML
    }

    /// <summary>
    /// Save object to local storage, serializes as json and writes object to a file
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StorageHelper<T> : IStorageHelper<T>
    {
        private ApplicationData _appData = Windows.Storage.ApplicationData.Current;

        [Obsolete]
        private StorageType _storageType;

        private string _subFolder;
        private StorageSerializer _serializerType;
        private StorageFolder _storageFolder;


        /// <summary>
        /// Deprecated constructor, will be removed in a future version
        /// </summary>
        /// <param name="StorageType"></param>
        /// <param name="subFolder"></param>
        /// <param name="serializerType"></param>
        [Obsolete]
        public StorageHelper(StorageType StorageType, string subFolder = null, StorageSerializer serializerType = StorageSerializer.JSON)
        {
            _storageType = StorageType;
            _subFolder = subFolder;
            _serializerType = serializerType;
        }

        /// <summary>
        /// Constructor that takes a storageFolder as input
        /// </summary>
        /// <param name="storageFolder">For example: Windows.Storage.ApplicationData.Current.LocalFolder</param>
        /// <param name="subFolder"></param>
        /// <param name="serializerType"></param>
        public StorageHelper(StorageFolder storageFolder, string subFolder = null, StorageSerializer serializerType = StorageSerializer.JSON)
        {
            _storageFolder = storageFolder;
            _subFolder = subFolder;
            _serializerType = serializerType;
        }

        /// <summary>
        /// Gets file extension based on serializer type
        /// Never deserialize with the wrong serializer
        /// </summary>
        /// <returns></returns>
        internal string GetFileExtension()
        {
            switch (_serializerType)
            {
                case StorageSerializer.JSON:
                    return ".json";
                case StorageSerializer.XML:
                    return ".xml";
            }

            return string.Empty;
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName"></param>
        public async Task DeleteAsync(string fileName)
        {
            fileName = fileName + GetFileExtension();
            try
            {
                StorageFolder folder = await GetFolderAsync().ConfigureAwait(false);
                var file = await folder.GetFileAsync(fileName);
                if (file != null)
                {                    
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Save object from file
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fileName"></param>
        public async Task SaveAsync(T obj, string fileName)
        {

            fileName = fileName + GetFileExtension();
            try
            {
                if (obj != null)
                {
                    //Get file
                    StorageFile file = null;
                    StorageFolder folder = await GetFolderAsync().ConfigureAwait(false);
                    file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);


                    //Serialize object with JSON or XML serializer
                    string storageString = null;
                    switch (_serializerType)
                    {
                        case StorageSerializer.JSON:
                            storageString = JsonConvert.SerializeObject(obj);
                            //Write content to file
                            await FileIO.WriteTextAsync(file, storageString);
                            break;
                        case StorageSerializer.XML:

                            IRandomAccessStream sessionRandomAccess = await file.OpenAsync(FileAccessMode.ReadWrite);
                            IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
                            XmlSerializer serializer = new XmlSerializer(typeof(T));
                            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), obj);
                            sessionRandomAccess.Dispose();
                            await sessionOutputStream.FlushAsync();
                            sessionOutputStream.Dispose();
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Load object from file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<T> LoadAsync(string fileName)
        {
            fileName = fileName + GetFileExtension();
            try
            {
                
                StorageFolder folder = await GetFolderAsync().ConfigureAwait(false);

                var file = await folder.GetFileAsync(fileName);
                if (file != null)
                {                    

                    //Deserialize to object with JSON or XML serializer
                    T result = default(T);

                    switch (_serializerType)
                    {
                        case StorageSerializer.JSON:
                            var data = await FileIO.ReadTextAsync(file);
                            result = JsonConvert.DeserializeObject<T>(data);
                            break;
                        case StorageSerializer.XML:
                            XmlSerializer serializer = new XmlSerializer(typeof(T));
                            IInputStream sessionInputStream = await file.OpenReadAsync();
                            result = (T)serializer.Deserialize(sessionInputStream.AsStreamForRead());
                            sessionInputStream.Dispose();

                            break;
                    }

                    return result;
                }
                else
                {
                    return default(T);
                }

            }
            catch (Exception ex)
            {
                //Unable to load contents of file
                throw;
            }
        }

        /// <summary>
        /// Get folder based on storagetype
        /// </summary>
        /// <returns></returns>
        public async Task<StorageFolder> GetFolderAsync()
        {

            StorageFolder folder;

            if (_storageFolder != null)
                folder = _storageFolder;
            else
            {
                //This part is obsolete and will be removed
                switch (_storageType)
                {
                    case StorageType.Local:
                        folder = _appData.LocalFolder;
                        break;
#if NETFX_CORE
                    case StorageType.Roaming:
                        folder = _appData.RoamingFolder;
                        break;
                    case StorageType.Temporary:
                        folder = _appData.TemporaryFolder;
                        break;
#endif
                    default:
                        throw new Exception(String.Format("Unknown StorageType: {0}", _storageType));
                }
            }

            if (!string.IsNullOrEmpty(_subFolder))
            {
                folder = await folder.CreateFolderAsync(_subFolder, CreationCollisionOption.OpenIfExists);
            }

            return folder;
        }

        /// <summary>
        /// Deletes all files in current folder
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAllFiles()
        {
            StorageFolder folder = await GetFolderAsync().ConfigureAwait(false);

            try
            {
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        /// <summary>
        /// Clear the complete cache
        /// </summary>
        /// <returns></returns>
        public static Task ClearAll(StorageFolder storageFolder)
        {
            return Task.Run(async () =>
            {
                StorageHelper<object> storage = new StorageHelper<object>(storageFolder);
                var folder = await storage.GetFolderAsync().ConfigureAwait(false);

                //Remove subfolders
                foreach (var sub in await folder.GetFoldersAsync())
                {
                    try
                    {
                        await sub.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }

                //Remove files
                foreach (var file in await folder.GetFilesAsync())
                {
                    try
                    {
                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            });
        }

    }
}
