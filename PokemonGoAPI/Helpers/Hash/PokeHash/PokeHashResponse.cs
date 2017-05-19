using System.Collections.Generic;

namespace POGOLib.Official.Util.Hash.PokeHash
{
    /// <summary>
    ///     Received data from PokeHash.
    ///     Class provided by https://talk.pogodev.org/d/54-getting-started-with-our-api-hashing-service
    /// </summary>
    internal class PokeHashResponse
    {
        public uint LocationAuthHash { get; set; }

        public uint LocationHash { get; set; }

        // Note: These are actually "unsigned" values. They are sent as signed values simply due to JSON format specifications. 
        //       You should re-cast these to unsigned variants (or leave them as-is in their byte form)
        public List<long> RequestHashes { get; set; }
    }
}
