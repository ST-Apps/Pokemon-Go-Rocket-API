using System;
using System.Linq;
using System.Threading.Tasks;
using POGOLib.Official.Util.Encryption.Legacy;
using POGOProtos.Networking.Envelopes;

namespace POGOLib.Official.Util.Hash
{
    /// <summary>
    ///     This is the default <see cref="IHasher"/> used by POGOLib.
    /// 
    ///     Android version: 0.45.0
    ///     IOS version: 1.15.0
    /// </summary>
    internal class LegacyHasher : IHasher
    {
        public Version PokemonVersion { get; } = new Version("0.45.0");

        public long Unknown25 { get; } = -1553869577012279119;

        public async Task<HashData> GetHashDataAsync(RequestEnvelope requestEnvelope, Signature signature, byte[] locationBytes, byte[][] requestsBytes, byte[] serializedTicket)
        {
            return new HashData
            {
                LocationAuthHash = NiaHashLegacy.Hash32Salt(locationBytes, NiaHashLegacy.Hash32(serializedTicket)),
                LocationHash = NiaHashLegacy.Hash32(locationBytes),
                RequestHashes = requestsBytes.Select(x => NiaHashLegacy.Hash64Salt64(x, NiaHashLegacy.Hash64(serializedTicket))).ToArray()
            };
        }

        public byte[] GetEncryptedSignature(byte[] signatureBytes, uint timestampSinceStartMs)
        {
            return PCryptLegacy.Encrypt(signatureBytes, timestampSinceStartMs);
        }
    }
}
