using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.Web.Http;
using Octokit;
using Universal_Authenticator_v2.Views;

namespace PokemonGo_UWP.Utils
{
    /// <summary>
    ///     Manager that checks if there's an updated version on GitHub
    /// </summary>
    public static class UpdateManager
    {
        private const string FileExtensionAppxbundle = ".appxbundle";
        private const string FileExtensionAppx = ".appx";

        /// <summary>
        ///     Client to access GitHub
        /// </summary>
        private static readonly GitHubClient GitHubClient = new GitHubClient(new ProductHeaderValue("PoGo-UWP"));

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

                if (!match.Success || match.Groups.Count < 4) return null;
                var repoVersion = new PackageVersion
                {
                    Major = ushort.Parse(match.Groups[1].Value),
                    Minor = ushort.Parse(match.Groups[2].Value),
                    Build = ushort.Parse(match.Groups[3].Value)
                };

                //compare major & minor & build (ignore revision)
                if ((repoVersion.Major > currentVersion.Major)
                    || (repoVersion.Major == currentVersion.Major && repoVersion.Minor > currentVersion.Minor)
                    ||
                    (repoVersion.Major == currentVersion.Major && repoVersion.Minor == currentVersion.Minor &&
                     repoVersion.Build > currentVersion.Build)
                    )
                {
                    return new UpdateInfo(repoVersion.Major + "." + repoVersion.Minor + "." + repoVersion.Build,
                        latestRelease.HtmlUrl, latestRelease.Body, latestRelease);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     Try install update. If it cant be installed directly, redirect user to page with release
        /// </summary>
        /// <param name="release">release to which update</param>
        /// <returns></returns>
        public static async Task InstallUpdate(Release release)
        {
            var browserFallback = true;


            var updateError = "";

            //if we have some assets try to install them or fallback to browser link
            if (release.Assets.Count > 0)
            {
                var archAssets = FilterAssetsByArchitecture(release);

                //find bundle
                var mainAsset = archAssets.FirstOrDefault(asset => asset.Name.EndsWith(FileExtensionAppxbundle));

                if (mainAsset != null)
                {
                    //only dependencies will stay
                    archAssets.Remove(mainAsset);

                    try
                    {
                        //this needs restricted capability "packageManagement" in appxmanifest
                        var packageManager = new PackageManager();

                        var dependencies = new List<Uri>();

                        try
                        {
                            var uri = new Uri(mainAsset.BrowserDownloadUrl);

                            Busy.SetBusy(true,
                                string.Format(Resources.CodeResources.GetString("UpdateDownloadingText"),
                                    release.TagName));


                            //Download dependencies
                            foreach (var asset in archAssets)
                            {
                                if (asset.Name.EndsWith(FileExtensionAppx))
                                {
                                    var file = await GetTemporaryUpdateFileAsync(asset.Name);
                                    await DownloadFile(file, new Uri(asset.BrowserDownloadUrl));

                                    dependencies.Add(new Uri(file.Path));
                                }
                            }


                            var destinationFile = await GetTemporaryUpdateFileAsync(mainAsset.Name);
                            await DownloadFile(destinationFile, uri);
                            Busy.SetBusy(false);
                            Busy.SetBusy(true, Resources.CodeResources.GetString("UpdateInstallingText"));

                            await packageManager.UpdatePackageAsync(new Uri(destinationFile.Path),
                                dependencies,
                                DeploymentOptions.ForceApplicationShutdown);

                            //in case of error COMException is thrown so we cant get result (?????)
                            browserFallback = false;
                        }
                        finally
                        {
                            //clean all temorary files and dont wait on it
                            var t1 = CleanTemporaryUpdateFolderAsync();
                        }
                    }
                    catch (Exception exc)
                    {
                        updateError = exc.HResult.ToString("0x") + " " + exc.Message;
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
                var dialog =
                    new MessageDialog(string.Format(Resources.CodeResources.GetString("UpdateFailedText"), updateError));

                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("YesText")) {Id = 0});
                dialog.Commands.Add(new UICommand(Resources.CodeResources.GetString("NoText")) {Id = 1});
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;

                var result = await dialog.ShowAsyncQueue();

                if ((int) result.Id != 0)
                    return;

                //we can laso open direct link to appx/appxbundle with mainAsset.BrowserDownloadUrl, but Edge waits on click small unseeable "Save" button before download
                await Launcher.LaunchUriAsync(new Uri(release.HtmlUrl));
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
            using (var client = new HttpClient())
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


        /// <summary>
        ///     Filter assets only to current architecture
        /// </summary>
        /// <param name="release"></param>
        /// <returns></returns>
        private static List<ReleaseAsset> FilterAssetsByArchitecture(Release release)
        {
            return
                release.Assets.Where(
                    asset =>
                        asset.Name.ToLower().Contains("_" + Package.Current.Id.Architecture.ToString().ToLower() + "_")
                        ||
                        asset.Name.ToLower().Contains("." + Package.Current.Id.Architecture.ToString().ToLower() + "."))
                    .ToList();
        }


        /// <summary>
        ///     Describes update details
        /// </summary>
        public class UpdateInfo
        {
            public string Description;
            public Release Release;
            public string Url;
            public string Version;

            public UpdateInfo(string version, string url, string description, Release release)
            {
                Version = version;
                Url = url;
                Release = release;
                Description = description;
            }
        }
    }
}