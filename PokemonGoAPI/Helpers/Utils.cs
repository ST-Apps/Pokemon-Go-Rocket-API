using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Utils
    {

        public static ulong FloatAsUlong(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static ulong GenerateLocation1(byte[] authTicket, double lat, double lng, double alt, uint hashSeed)
        {
            var first_hash = NiaHash.compute_hash32(authTicket, hashSeed);

            var locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat).Reverse());
            locationBytes.AddRange(BitConverter.GetBytes(lng).Reverse());
            locationBytes.AddRange(BitConverter.GetBytes(alt).Reverse());

            return NiaHash.compute_hash32(locationBytes.ToArray(), first_hash);
        }

        public static ulong GenerateLocation2(double lat, double lng, double alt, uint hashSeed)
        {
            var locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat).Reverse());
            locationBytes.AddRange(BitConverter.GetBytes(lng).Reverse());
            locationBytes.AddRange(BitConverter.GetBytes(alt).Reverse());

            return NiaHash.compute_hash32(locationBytes.ToArray(), hashSeed);
        }

        public static ulong GenerateRequestHash(byte[] authTicket, byte[] request, uint hashSeed)
        {
            var first_hash = NiaHash.compute_hash64(authTicket, hashSeed);
            return NiaHash.compute_hash64(request.ToArray(), first_hash);
        }
    }
}