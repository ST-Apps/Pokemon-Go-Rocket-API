using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.Web.Http;
using PokemonGo_UWP.Views;
using PokemonGo_UWP.Entities;
using Windows.Web.Http.Filters;
using System.Diagnostics;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Manager that checks if there's an updated version on air
    /// </summary>
    public static class UpdateManager
    {
        private const string VersionFileUrl = @"https://raw.githubusercontent.com/PoGo-Devs/PoGo/master/version.json";

        /// <summary>
        ///     Checks if we have an updated version and returns update info
        /// </summary>
        /// <returns>Update info</returns>
        public static async Task<UpdateInfo> IsUpdateAvailable()
        {
            try
            {
                //clean update folder on backgraound, dont bother with result - we are ready for collisions
                var t1 = CleanTemporaryUpdateFolderAsync();

                var httpFilter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
                httpFilter.CacheControl.ReadBehavior =
                    Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;

                //dwonload version info
                using (var client = new HttpClient(httpFilter))
                {
                    using (var response = await client.GetAsync(new Uri(VersionFileUrl), HttpCompletionOption.ResponseContentRead))
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        if (!VersionInfo.SetInstance(json))
                        {
                            return new UpdateInfo(UpdateStatus.NoInternet);
                        }
                    }
                }

                if (Debugger.IsAttached)
                {
                    return new UpdateInfo(UpdateStatus.NoUpdate);
                }
#if DEBUG
                // For debugging purposes we dont check version now
                return new UpdateInfo(UpdateStatus.NoUpdate);
#endif


                // Check if version is newer
                var currentVersion = Package.Current.Id.Version;
                var repoVersion = GetVersionFromPattern(@"(\d*)\.(\d*)\.(\d*)", VersionInfo.Instance.latest_release.version);
                var minVersion = GetVersionFromPattern(@"(\d*)\.(\d*)\.(\d*)", VersionInfo.Instance.minimum_version);

                //compare major & minor & build (ignore revision)
                if (IsVersionGreater(currentVersion, repoVersion))
                {
                    UpdateStatus updateStatus = UpdateStatus.UpdateAvailable;
                    //patch architecture
                    VersionInfo.Instance.latest_release.setup_file = VersionInfo.Instance.latest_release.setup_file.Replace("{arch}", Package.Current.Id.Architecture.ToString());

                    if (IsVersionGreater(currentVersion, minVersion))
                    {
                        updateStatus = UpdateStatus.UpdateForced;
                    }


                    return new UpdateInfo(updateStatus, repoVersion.Major + "." + repoVersion.Minor + "." + repoVersion.Build,
                    VersionInfo.Instance.latest_release.setup_file, VersionInfo.Instance.latest_release.changes);
                }
                else if(IsVersionGreater(repoVersion, minVersion))
                {
                    return new UpdateInfo(UpdateStatus.NextVersionNotReady);
                }

                return new UpdateInfo(UpdateStatus.NoUpdate);
            }
            catch (Exception)
            {
                return new UpdateInfo(UpdateStatus.NoInternet);
            }
        }

        public static bool IsVersionGreater(PackageVersion currentVersion, PackageVersion newVersion)
        {
            return ((newVersion.Major > currentVersion.Major)
                   || (newVersion.Major == currentVersion.Major && newVersion.Minor > currentVersion.Minor)
                   || (newVersion.Major == currentVersion.Major && newVersion.Minor == currentVersion.Minor &&
                      newVersion.Build > currentVersion.Build)
                   );
        }

        public static PackageVersion GetVersionFromPattern(string pattern, string version)
        {
            var regex = new Regex(pattern);
            var match = regex.Match(version);
            if (!match.Success || match.Groups.Count < 4)
                throw new Exception("Version format wrong");

            PackageVersion repoVersion = new PackageVersion
            {
                Major = ushort.Parse(match.Groups[1].Value),
                Minor = ushort.Parse(match.Groups[2].Value),
                Build = ushort.Parse(match.Groups[3].Value)
            };
            return repoVersion;
        }

        /// <summary>
        ///     Try install update. If it cant be installed directly, redirect user to page with release
        /// </summary>
        /// <param name="release">release to which update</param>
        /// <returns></returns>
        public static async Task InstallUpdate()
        {
            var browserFallback = true;


            var updateError = "";


            try
            {
                //this needs restricted capability "packageManagement" in appxmanifest
                var packageManager = new PackageManager();

                var dependencies = new List<Uri>();

                var uri = new Uri(VersionInfo.Instance.latest_release.setup_file);

                Busy.SetBusy(true,
                    string.Format(Resources.CodeResources.GetString("UpdateDownloadingText"),
                        VersionInfo.Instance.latest_release.version));


                //Download dependencies
                foreach (string assetUrl in VersionInfo.Instance.latest_release.dependencies)
                {
                    string url = assetUrl.Replace("{arch}", Package.Current.Id.Architecture.ToString());
                    var assetUri = new Uri(url);
                    var file = await GetTemporaryUpdateFileAsync(Path.GetFileName(assetUri.LocalPath));
                    await DownloadFile(file, assetUri);

                    dependencies.Add(new Uri(file.Path));
                }


                var destinationFile = await GetTemporaryUpdateFileAsync(Path.GetFileName(uri.LocalPath));
                await DownloadFile(destinationFile, uri);
                Busy.SetBusy(false);
                Busy.SetBusy(true, Resources.CodeResources.GetString("UpdateInstallingText"));

                await packageManager.UpdatePackageAsync(new Uri(destinationFile.Path),
                    dependencies,
                    DeploymentOptions.ForceApplicationShutdown);

                //in case of error COMException is thrown so we cant get result (?????)
                browserFallback = false;
            }
            catch (Exception exc)
            {
                updateError = exc.HResult.ToString("0x") + " " + exc.Message;
                //lets do fallback to browser
            }
            finally
            {
                //clean all temorary files and dont wait on it
                var t1 = CleanTemporaryUpdateFolderAsync();
                Busy.SetBusy(false);
            }

            if (browserFallback)
            {
                //update failed, show dialog to user
                var dialog =
                    new MessageDialog(string.Format(Resources.CodeResources.GetString("UpdateFailedText"), updateError));

                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("YesText")) {Id = 0});
                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("NoText")) {Id = 1});
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsyncQueue();

                if ((int) result.Id != 0)
                    return;

                //open appx to download
                await Launcher.LaunchUriAsync(new Uri(VersionInfo.Instance.latest_release.setup_file));
            }
        }

        /// <summary>
        ///     Gets (or creates) temporary folder for updates
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetTemporaryUpdateFolderAsync()
        {
            var temp = ApplicationData.Current.TemporaryFolder;
            var folder = await temp.CreateFolderAsync("Updates", CreationCollisionOption.OpenIfExists);

            return folder;
        }

        /// <summary>
        ///     Get temporary update file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<StorageFile> GetTemporaryUpdateFileAsync(string fileName)
        {
            return
                await
                    (await GetTemporaryUpdateFolderAsync()).CreateFileAsync(fileName,
                        CreationCollisionOption.GenerateUniqueName);
        }


        /// <summary>
        ///     Deletes files in temporary update folder
        /// </summary>
        /// <returns></returns>
        private static async Task CleanTemporaryUpdateFolderAsync()
        {
            var folder = await GetTemporaryUpdateFolderAsync();
            foreach (var item in await folder.GetFilesAsync())
            {
                try
                {
                    await item.DeleteAsync();
                }
                catch (Exception)
                {
                    //dont bother
                }
            }
        }

        /// <summary>
        ///     Download file from URL
        /// </summary>
        /// <param name="destinationFile"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static async Task DownloadFile(StorageFile destinationFile, Uri uri)
        {
            var filter = new HttpBaseProtocolFilter();
            filter.AllowAutoRedirect = true;

            using (var client = new HttpClient(filter))
            {
                using (var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (var outstream = (await destinationFile.OpenStreamForWriteAsync()).AsOutputStream())
                    {
                        await response.Content.WriteToStreamAsync(outstream);
                    }
                }
            }
        }


        public enum UpdateStatus
        {
            //No internet connection
            NoInternet,
            //no update available
            NoUpdate,
            //Update is available and user can choose if he start update
            UpdateAvailable,
            //Update is available and will be installed without user permission
            UpdateForced,
            //This version is old, but update is not ready yet
            NextVersionNotReady

        }

        /// <summary>
        ///     Describes update details
        /// </summary>
        public class UpdateInfo
        {
            public string Description;
            public string Url;
            public string Version;
            public UpdateStatus Status;

            public UpdateInfo(UpdateStatus status, string version, string url, string description)
            {
                Status = status;
                Version = version;
                Url = url;
                Description = description;
            }
            public UpdateInfo(UpdateStatus status)
            {
                Status = status;
            }
        }
    }
}