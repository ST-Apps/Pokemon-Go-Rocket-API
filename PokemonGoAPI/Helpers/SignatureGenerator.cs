using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGoAPI.Helpers
{
    class SignatureGenerator
    {
        private static Random rnd = new Random();

        public static byte[] GetSignature(byte[] input)
        {
            byte[] iv = new byte[32];

            rnd.NextBytes(iv);

            uint outsize = PokemonGoAPI.Encrypter.GetOutputSize(input, iv);

            byte[] output = new byte[outsize];

            PokemonGoAPI.Encrypter.Encrypt(input, iv, output);

            return output;
        }
    }
}
