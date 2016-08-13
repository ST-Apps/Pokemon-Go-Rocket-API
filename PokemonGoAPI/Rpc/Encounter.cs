using System.Threading.Tasks;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Encounter : BaseRpc
    {
        public Encounter(Client client) : base(client)
        {
        }

        public async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnPointGuid)
        {
            var message = new EncounterMessage
            {
                EncounterId = encounterId,
                SpawnPointId = spawnPointGuid,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, EncounterResponse>(RequestType.Encounter, message);
        }

        public async Task<UseItemCaptureResponse> UseCaptureItem(ulong encounterId, ItemId itemId, string spawnPointId)
        {
            var message = new UseItemCaptureMessage
            {
                EncounterId = encounterId,
                ItemId = itemId,
                SpawnPointId = spawnPointId
            };

            return await PostProtoPayload<Request, UseItemCaptureResponse>(RequestType.UseItemCapture, message);
        }

        public async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnPointGuid,
            ItemId pokeballItemId, double normalizedRecticleSize = 1.950, double spinModifier = 1,
            double normalizedHitPos = 1, bool hitPokemon = true)
        {
            var message = new CatchPokemonMessage
            {
                EncounterId = encounterId,
                Pokeball = pokeballItemId,
                SpawnPointId = spawnPointGuid,
                HitPokemon = hitPokemon,
                NormalizedReticleSize = normalizedRecticleSize,
                SpinModifier = spinModifier,
                NormalizedHitPosition = normalizedHitPos
            };

            return await PostProtoPayload<Request, CatchPokemonResponse>(RequestType.CatchPokemon, message);
        }

        public async Task<IncenseEncounterResponse> EncounterIncensePokemon(ulong encounterId, string encounterLocation)
        {
            var message = new IncenseEncounterMessage
            {
                EncounterId = encounterId,
                EncounterLocation = encounterLocation
            };

            return await PostProtoPayload<Request, IncenseEncounterResponse>(RequestType.IncenseEncounter, message);
        }

        public async Task<DiskEncounterResponse> EncounterLurePokemon(ulong encounterId, string fortId)
        {
            var message = new DiskEncounterMessage
            {
                EncounterId = encounterId,
                FortId = fortId,
                PlayerLatitude = Client.CurrentLatitude,
                PlayerLongitude = Client.CurrentLongitude
            };

            return await PostProtoPayload<Request, DiskEncounterResponse>(RequestType.DiskEncounter, message);
        }

        public async Task<EncounterTutorialCompleteResponse> EncounterTutorialComplete(PokemonId pokemonId)
        {
            var message = new EncounterTutorialCompleteMessage
            {
                PokemonId = pokemonId
            };

            return
                await
                    PostProtoPayload<Request, EncounterTutorialCompleteResponse>(RequestType.EncounterTutorialComplete,
                        message);
        }
    }
}