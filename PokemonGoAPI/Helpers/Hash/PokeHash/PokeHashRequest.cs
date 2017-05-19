using System.Collections.Generic;

namespace POGOLib.Official.Util.Hash.PokeHash
{
    /// <summary>
    ///     Data sent to PokeHash.
    ///     Class provided by https://talk.pogodev.org/d/54-getting-started-with-our-api-hashing-service
    /// </summary>
    internal class PokeHashRequest
    {
        /// <summary>
        ///     The timestamp for the packet being sent to Niantic. This much match what you use in the SignalLog and RpcRequest
        ///     protos! (EpochTimestampMS)
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// The Latitude field from your ClientRpc request envelope. (The one you will be sending to Niantic)
        /// For safety reasons, this should also match your last LocationUpdate entry in the SignalLog
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// The Longitude field from your ClientRpc request envelope. (The one you will be sending to Niantic)
        /// For safety reasons, this should also match your last LocationUpdate entry in the SignalLog
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// The Altitude field from your ClientRpc request envelope. (The one you will be sending to Niantic)
        /// For safety reasons, this should also match your last LocationUpdate entry in the SignalLog
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        ///     The Niantic-specific auth ticket data.
        /// </summary>
        public byte[] AuthTicket { get; set; }

        /// <summary>
        ///     Also known as the "replay check" field. (Field 22 in SignalLog)
        /// </summary>
        public byte[] SessionData { get; set; }

        /// <summary>
        ///     A collection of the request data to be hashed.
        /// </summary>
        public List<byte[]> Requests { get; set; } = new List<byte[]>();
    }
}
