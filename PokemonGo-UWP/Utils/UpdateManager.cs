using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Octokit;
using System.Text.RegularExpressions;
using Windows.Storage;
using Windows.Web.Http;
using Windows.System;
using Universal_Authenticator_v2.Views;
using System.IO;
using Windows.Management.Deployment;
using Windows.Storage.Streams;
using System.Runtime.InteropServices;
using Windows.UI.Popups;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    /// Manager that checks if there's an updated version on GitHub
    /// </summary>
    public static class UpdateManager
    {
        /// <summary>
        /// Client to access GitHub
        /// </summary>
        private static readonly GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("PoGo-UWP"));
        private const string FILE_EXTENSION_APPXBUNDLE = ".appxbundle";
        private const string FILE_EXTENSION_APPX = ".appx";


        /// <summary>
        /// Describes update details
        /// </summary>
        public class UpdateInfo
        {
            public UpdateInfo(string version, string url, string description, Release release)
            {
                this.version = version;
                this.url = url;
                this.release = release;
                this.description = description;
            }
            public string version;
            public string url;
            public string description;
            public Release release;
        }

        /// <summary>
        /// Checks if we have an updated version and returns update info
        /// </summary>
        /// <returns>Update info</returns>
        public static async Task<UpdateInfo> IsUpdateAvailable()
        {
            try
            {
                //clean update folder on backgraound, dont bother with result - we are ready for collisions
                Task t1 = CleanTemporaryUpdateFolderAsync();

                var releases = await GitHubClient.Repository.Release.GetAll("ST-Apps", "PoGo-UWP");
                Release latestRelease = null;

                foreach (var release in releases)
                {
                    //Look only for releases for our architecture
                    var archAssets = FilterAssetsByArchitecture(release);

                    // We skip prereleases, only stable ones
                    if (!release.Prerelease && archAssets.Count > 0)
                    {
                        latestRelease = release;
                        break;
                    }
                }

                if (latestRelease == null)
                    return null;

                // Check if version is newer
                var currentVersion = Package.Current.Id.Version;
                var regex = new Regex(@"\D*(\d*)\.(\d*)\.(\d*).*");
                var match = regex.Match(latestRelease.TagName);

                if(match.Success && match.Groups.Count >= 4)
                {
                    PackageVersion repoVersion = new PackageVersion();
                    repoVersion.Major = ushort.Parse(match.Groups[1].Value);
                    repoVersion.Minor = ushort.Parse(match.Groups[2].Value);
                    repoVersion.Build = ushort.Parse(match.Groups[3].Value);

                    //compare major & minor & build (ignore revision)
                    if ((repoVersion.Major > currentVersion.Major)
                       || (repoVersion.Major == currentVersion.Major && repoVersion.Minor > currentVersion.Minor)
                       || (repoVersion.Major == currentVersion.Major && repoVersion.Minor == currentVersion.Minor && repoVersion.Build > currentVersion.Build)
                       )
                    {
                        return new UpdateInfo(repoVersion.Major+"."+repoVersion.Minor+"."+repoVersion.Build, latestRelease.HtmlUrl, latestRelease.Body, latestRelease);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Try install update. If it cant be installed directly, redirect user to page with release
        /// </summary>
        /// <param name="release">release to which update</param>
        /// <returns></returns>
        public static async Task InstallUpdate(Release release)
        {
            bool browserFallback = true;


            ReleaseAsset mainAsset = null;
            string updateError = "";

            //if we have some assets try to install them or fallback to browser link
            if (release.Assets.Count > 0)
            {
                var archAssets = FilterAssetsByArchitecture(release);

                //find bundle
                mainAsset = archAssets.Where(asset => asset.Name.EndsWith(FILE_EXTENSION_APPXBUNDLE)).FirstOrDefault();

                if (mainAsset != null)
                {
                    //only dependencies will stay
                    archAssets.Remove(mainAsset);

                    try
                    {
                        //this needs restricted capability "packageManagement" in appxmanifest
                        Windows.Management.Deployment.PackageManager packageManager = new Windows.Management.Deployment.PackageManager();

                        StorageFile destinationFile = null;
                        List<Uri> dependencies = new List<Uri>();

                        try
                        {
                            Uri uri = new Uri(mainAsset.BrowserDownloadUrl);

                            Busy.SetBusy(true, string.Format(Resources.Translation.GetString("UpdateDownloading"), release.TagName));


                            //Download dependencies
                            foreach(var asset in archAssets)
                            {
                                if (asset.Name.EndsWith(FILE_EXTENSION_APPX))
                                {
                                    StorageFile file = await GetTemporaryUpdateFileAsync(asset.Name);
                                    await DownloadFile(file, new Uri(asset.BrowserDownloadUrl));

                                    dependencies.Add(new Uri(file.Path));
                                }
                            }


                            destinationFile = await GetTemporaryUpdateFileAsync(mainAsset.Name);
                            await DownloadFile(destinationFile, uri);
                            Busy.SetBusy(false);
                            Busy.SetBusy(true, Resources.Translation.GetString("UpdateInstalling"));

                            var result = await packageManager.UpdatePackageAsync(new Uri(destinationFile.Path),
                                        dependencies,
                                        DeploymentOptions.ForceApplicationShutdown);

                            //in case of error COMException is thrown so we cant get result (?????)
                            browserFallback = false;
                        }
                        finally
                        {
                            //clean all temorary files and dont wait on it
                            Task t1 = CleanTemporaryUpdateFolderAsync();
                        }
                        
                    }
                    catch (Exception exc)
                    {
                        updateError = exc.HResult.ToString("X") + " " + exc.Message;
                        //lets do fallback to browser
                    }
                    finally
                    {
                        Busy.SetBusy(false);
                    }
                }
            }

            if (browserFallback)
            {
                //update failed, show dialog to user
                var dialog = new MessageDialog(string.Format(Utils.Resources.Translation.GetString("UpdateFailed"), updateError));

                dialog.Commands.Add(new UICommand(Utils.Resources.Translation.GetString("Yes")) { Id = 0 });
                dialog.Commands.Add(new UICommand(Utils.Resources.Translation.GetString("No")) { Id = 1 });
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsyncQueue();

                if ((int)result.Id != 0)
                    return;

                //we can laso open direct link to appx/appxbundle with mainAsset.BrowserDownloadUrl, but Edge waits on click small unseeable "Save" button before download
                await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl));
            }
        }

        /// <summary>
        /// Gets (or creates) temporary folder for updates
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetTemporaryUpdateFolderAsync()
        {
            StorageFolder temp = ApplicationData.Current.TemporaryFolder;
            StorageFolder folder = null;
            try
            {
                folder = await temp.GetFolderAsync("Updates");
            }
            catch(FileNotFoundException)
            {
                folder = await temp.CreateFolderAsync("Updates");

            }

            return folder;
        }

        /// <summary>
        /// Get temporary update file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<StorageFile> GetTemporaryUpdateFileAsync(string fileName)
        {
            return await (await GetTemporaryUpdateFolderAsync()).CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
        }


        /// <summary>
        /// Deletes files in temporary update folder
        /// </summary>
        /// <returns></returns>
        private static async Task CleanTemporaryUpdateFolderAsync()
        {
            var folder = await GetTemporaryUpdateFolderAsync();
            foreach(var item in await folder.GetFilesAsync())
            {
                try
                {
                    await item.DeleteAsync();
                }
                catch(Exception)
                {
                    //dont bother
                }
            }
        }

        /// <summary>
        /// Download file from URL
        /// </summary>
        /// <param name="destinationFile"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static async Task DownloadFile(StorageFile destinationFile, Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                {
                    using (IOutputStream outstream = (await destinationFile.OpenStreamForWriteAsync()).AsOutputStream())
                    {
                        await response.Content.WriteToStreamAsync(outstream);
                    }
                }
            }
        }


        /// <summary>
        /// Filter assets only to current architecture
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        private static List<ReleaseAsset> FilterAssetsByArchitecture(Release release)
        {
            return release.Assets.Where(asset =>
            {
                return asset.Name.ToLower().Contains("_" + Package.Current.Id.Architecture.ToString().ToLower() + "_")
                || asset.Name.ToLower().Contains("." + Package.Current.Id.Architecture.ToString().ToLower() + ".");
            }).ToList();
        }
    }
}
