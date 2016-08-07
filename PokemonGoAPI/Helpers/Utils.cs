using System;
using System.Collections.Generic;
using System.Text;
using xxHashSharp;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Utils
    {
        public static ulong FloatAsUlong(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public static uint GenerateLocation1(string authTicket, double lat, double lng, double alt)
        {
            uint firstHash = xxHash.CalculateHash(Encoding.ASCII.GetBytes(authTicket), -1, 0x1B845238);

            var locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat));
            locationBytes.AddRange(BitConverter.GetBytes(lng));
            locationBytes.AddRange(BitConverter.GetBytes(alt));

            return xxHash.CalculateHash(locationBytes.ToArray(), locationBytes.Count, firstHash);
        }

        public static uint GenerateLocation2(double lat, double lng, double alt)
        {
            var locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat));
            locationBytes.AddRange(BitConverter.GetBytes(lng));
            locationBytes.AddRange(BitConverter.GetBytes(alt));

            return xxHash.CalculateHash(locationBytes.ToArray(), locationBytes.Count, 0x1B845238);
        }

        public static uint GenerateRequestHash(string authTicket, string request)
        {
            uint firstHash = xxHash.CalculateHash(Encoding.ASCII.GetBytes(authTicket), -1, 0x1B845238);
            return xxHash.CalculateHash(Encoding.ASCII.GetBytes(request), -1, firstHash);
        }
    }
}