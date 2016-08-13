using System.Collections.Generic;
using System.Threading.Tasks;
using POGOProtos.Data.Battle;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Fort : BaseRpc
    {
        public Fort(Client client) : base(client)
        {
        }

        public async Task<FortDetailsResponse> GetFort(string fortId, double fortLatitude, double fortLongitude)
        {
            var message = new FortDetailsMessage
            {
                FortId = fortId,
                Latitude = fortLatitude,
                Longitude = fortLongitude
            };

            return await PostProtoPayload<Request, FortDetailsResponse>(RequestType.FortDetails, message);
        }

        public async Task<FortSearchResponse> SearchFort(string fortId, double fortLat, double fortLng)
        {
            var message = new FortSearchMessage
            {
                FortId = fortId,
                FortLatitude = fortLat,
                FortLongitude = fortLng,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, FortSearchResponse>(RequestType.FortSearch, message);
        }

        public async Task<AddFortModifierResponse> AddFortModifier(string fortId, ItemId modifierType)
        {
            var message = new AddFortModifierMessage
            {
                FortId = fortId,
                ModifierType = modifierType,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, AddFortModifierResponse>(RequestType.AddFortModifier, message);
        }

        public async Task<AttackGymResponse> AttackGym(string fortId, string battleId, List<BattleAction> battleActions,
            BattleAction lastRetrievedAction)
        {
            var message = new AttackGymMessage
            {
                BattleId = battleId,
                GymId = fortId,
                LastRetrievedActions = lastRetrievedAction,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude,
                AttackActions = {battleActions}
            };

            message.AttackActions.AddRange(battleActions);

            return await PostProtoPayload<Request, AttackGymResponse>(RequestType.AttackGym, message);
        }

        public async Task<FortDeployPokemonResponse> FortDeployPokemon(string fortId, ulong pokemonId)
        {
            var message = new FortDeployPokemonMessage
            {
                PokemonId = pokemonId,
                FortId = fortId,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, FortDeployPokemonResponse>(RequestType.FortDeployPokemon, message);
        }

        public async Task<FortRecallPokemonResponse> FortRecallPokemon(string fortId, ulong pokemonId)
        {
            var message = new FortRecallPokemonMessage
            {
                PokemonId = pokemonId,
                FortId = fortId,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, FortRecallPokemonResponse>(RequestType.FortRecallPokemon, message);
        }

        public async Task<GetGymDetailsResponse> GetGymDetails(string gymId, double gymLat, double gymLng)
        {
            var message = new GetGymDetailsMessage
            {
                GymId = gymId,
                GymLatitude = gymLat,
                GymLongitude = gymLng,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, GetGymDetailsResponse>(RequestType.GetGymDetails, message);
        }

        public async Task<StartGymBattleResponse> StartGymBattle(string gymId, ulong defendingPokemonId,
            IEnumerable<ulong> attackingPokemonIds)
        {
            var message = new StartGymBattleMessage
            {
                GymId = gymId,
                DefendingPokemonId = defendingPokemonId,
                AttackingPokemonIds = {attackingPokemonIds},
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, StartGymBattleResponse>(RequestType.StartGymBattle, message);
        }
    }
}