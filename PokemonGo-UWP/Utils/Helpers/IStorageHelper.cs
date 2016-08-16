using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils.Helpers
{
    /// <summary>
    /// StorageHelper interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStorageHelper<T>
    {
        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task DeleteAsync(string fileName);

        /// <summary>
        /// Save object from file
        /// </summary>
        /// <param name="Obj"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task SaveAsync(T Obj, string fileName);

        /// <summary>
        /// Load object from file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        Task<T> LoadAsync(string fileName);

        /// <summary>
        /// Deletes all files in current directory
        /// </summary>
        /// <returns></returns>
        Task DeleteAllFiles();
    }
}
