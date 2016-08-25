using System.Collections.Generic;
using System.Threading.Tasks;
using POGOProtos.Enums;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Download : BaseRpc
    {
        public string DownloadSettingsHash { get; set; }
        public Download(Client client) : base(client)
        {
            DownloadSettingsHash = "";
        }

        public async Task<DownloadSettingsResponse> GetSettings()
        {
            var message = new DownloadSettingsMessage
            {
                Hash = DownloadSettingsHash
            };

            var response = await PostProtoPayload<Request, DownloadSettingsResponse>(RequestType.DownloadSettings, message);
            DownloadSettingsHash = response?.Hash ?? "";


            return response;
        }

        public async Task<DownloadItemTemplatesResponse> GetItemTemplates()
        {
            return
                await
                    PostProtoPayload<Request, DownloadItemTemplatesResponse>(RequestType.DownloadItemTemplates,
                        new DownloadItemTemplatesMessage());
        }

        public async Task<DownloadRemoteConfigVersionResponse> GetRemoteConfigVersion(uint appVersion,
            string deviceManufacturer, string deviceModel, string locale, Platform platform)
        {
            return
                await
                    PostProtoPayload<Request, DownloadRemoteConfigVersionResponse>(
                        RequestType.DownloadRemoteConfigVersion, new DownloadRemoteConfigVersionMessage
                        {
                            AppVersion = appVersion,
                            DeviceManufacturer = deviceManufacturer,
                            DeviceModel = deviceModel,
                            Locale = locale,
                            Platform = platform
                        });
        }

        public async Task<GetAssetDigestResponse> GetAssetDigest(uint appVersion, string deviceManufacturer,
            string deviceModel, string locale, Platform platform)
        {
            return
                await
                    PostProtoPayload<Request, GetAssetDigestResponse>(RequestType.GetAssetDigest,
                        new GetAssetDigestMessage
                        {
                            AppVersion = appVersion,
                            DeviceManufacturer = deviceManufacturer,
                            DeviceModel = deviceModel,
                            Locale = locale,
                            Platform = platform
                        });
        }

        public async Task<GetDownloadUrlsResponse> GetDownloadUrls(IEnumerable<string> assetIds)
        {
            return
                await
                    PostProtoPayload<Request, GetDownloadUrlsResponse>(RequestType.GetDownloadUrls,
                        new GetDownloadUrlsMessage
                        {
                            AssetId = {assetIds}
                        });
        }
    }
}