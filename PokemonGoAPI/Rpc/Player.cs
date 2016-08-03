using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Player : BaseRpc
    {
        public Player(Client client) : base(client)
        {
            _client = client;
        }

        public async Task<PlayerUpdateResponse> UpdatePlayerLocation(double latitude, double longitude, double altitude)
        {
            SetCoordinates(latitude, longitude, altitude);
            var message = new PlayerUpdateMessage
            {
                Latitude = _client.CurrentLatitude,
                Longitude = _client.CurrentLongitude
            };

            var updatePlayerLocationRequestEnvelope = RequestBuilder.GetRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.PlayerUpdate,
                    RequestMessage = message.ToByteString()
                });

            return await PostProtoPayload<Request, PlayerUpdateResponse>(updatePlayerLocationRequestEnvelope);
        }

        internal void SetCoordinates(double lat, double lng, double altitude)
        {
            _client.CurrentLatitude = lat;
            _client.CurrentLongitude = lng;
            _client.CurrentAltitude = altitude;
        }

        public async Task<GetPlayerResponse> GetPlayer()
        {
            return await PostProtoPayload<Request, GetPlayerResponse>(RequestType.GetPlayer, new GetPlayerMessage());
        }

        public async Task<GetPlayerProfileResponse> GetPlayerProfile(string playerName)
        {
            return await PostProtoPayload<Request, GetPlayerProfileResponse>(RequestType.GetPlayerProfile, new GetPlayerProfileMessage()
            {
                PlayerName = playerName
            });
        }

        public async Task<CheckAwardedBadgesResponse> GetNewlyAwardedBadges()
        {
            return await PostProtoPayload<Request, CheckAwardedBadgesResponse>(RequestType.CheckAwardedBadges, new CheckAwardedBadgesMessage());
        }

        public async Task<CollectDailyBonusResponse> CollectDailyBonus()
        {
            return await PostProtoPayload<Request, CollectDailyBonusResponse>(RequestType.CollectDailyBonus, new CollectDailyBonusMessage());
        }

        public async Task<CollectDailyDefenderBonusResponse> CollectDailyDefenderBonus()
        {
            return await PostProtoPayload<Request, CollectDailyDefenderBonusResponse>(RequestType.CollectDailyDefenderBonus, new CollectDailyDefenderBonusMessage());
        }

        public async Task<EquipBadgeResponse> EquipBadge(BadgeType type)
        {
            return await PostProtoPayload<Request, EquipBadgeResponse>(RequestType.EquipBadge, new EquipBadgeMessage() { BadgeType = type });
        }

        public async Task<LevelUpRewardsResponse> GetLevelUpRewards(int level)
        {
            return await PostProtoPayload<Request, LevelUpRewardsResponse>(RequestType.LevelUpRewards, new LevelUpRewardsMessage()
            {
                Level = level
            });
        }

        public async Task<SetAvatarResponse> SetAvatar(PlayerAvatar playerAvatar)
        {
            return await PostProtoPayload<Request, SetAvatarResponse>(RequestType.SetAvatar, new SetAvatarMessage()
            {
                PlayerAvatar = playerAvatar
            });
        }

        public async Task<SetContactSettingsResponse> SetContactSetting(ContactSettings contactSettings)
        {
            return await PostProtoPayload<Request, SetContactSettingsResponse>(RequestType.SetContactSettings, new SetContactSettingsMessage()
            {
                ContactSettings = contactSettings
            });
        }

        public async Task<SetPlayerTeamResponse> SetPlayerTeam(TeamColor teamColor)
        {
            return await PostProtoPayload<Request, SetPlayerTeamResponse>(RequestType.SetPlayerTeam, new SetPlayerTeamMessage()
            {
                Team = teamColor
            });
        }
    }
}
