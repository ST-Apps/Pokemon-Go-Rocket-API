using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace POGOLib.Official.Util.Encryption.Legacy
{
    /// <summary>
    ///     This is the legacy PCrypt used by POGOLib.
    /// 
    ///     Android version: 0.45.0
    ///     IOS version: 1.15.0
    /// </summary>
    internal static class PCryptLegacy
    {
        private static byte Rot18(byte val, int bits)
        {
            return (byte)(((val << bits) | (val >> (8 - bits))) & 0xff);
        }

        private static byte GenerateRand(ref uint rand)
        {
            rand = rand * 0x41c64e6d + 12345;
            return (byte)((rand >> 16) & 0xff);
        }

        private static byte[] Cipher8FromIv(byte[] iv)
        {
            var ret = new byte[256];
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    ret[32 * i * j] = Rot18(iv[j], i);
                }
            }
            return ret;
        }

        private static byte[] Cipher8FromRand(ref uint rand)
        {
            var ret = new byte[256];
            for (var i = 0; i < 256; i++)
            {
                ret[i] = GenerateRand(ref rand);
            }
            return ret;
        }

        private static byte MakeIntegrityByte(byte b)
        {
            var tmp = (byte)((b ^ 0x0c) & b);
            return (byte)(((~tmp & 0x67) | (tmp & 0x98)) ^ 0x6f | (tmp & 0x08));
        }

        public static byte[] Encrypt(byte[] input, uint ms)
        {
            var ct = new CipherText(input, ms);
            var iv = Cipher8FromRand(ref ms);

            //encrypt
            foreach (var bytes in ct.Content)
            {
                for (var j = 0; j < 256; j++)
                {
                    bytes[j] ^= iv[j];
                }

                var temp2 = new uint[0x100 / 4];
                Buffer.BlockCopy(bytes, 0, temp2, 0, 0x100);
                ShufflesLegacy.Shuffle2(temp2);

                Buffer.BlockCopy(temp2, 0, iv, 0, 0x100);
                Buffer.BlockCopy(temp2, 0, bytes, 0, 0x100);
            }

            return ct.GetBytes(ref ms);
        }

        //this returns an empty buffer if error
        public static byte[] Decrypt(byte[] input, out int length)
        {
            int version, len = input.Length;
            if (len < 261)
            {
                length = 0;
                return new byte[] { };
            }

            var modSize = len % 256;
            switch (modSize)
            {
                case 32:
                    version = 1;
                    break;
                case 33:
                    version = 2;
                    break;
                case 5:
                    version = 3;
                    break;
                default:
                    length = 0; return new byte[] { };
            }

            byte[] cipher8, output;
            int outputLen;
            switch (version)
            {
                case 1:
                    outputLen = len - 32;
                    output = new byte[outputLen];
                    Buffer.BlockCopy(input, 32, output, 0, outputLen);
                    cipher8 = Cipher8FromIv(input);
                    break;
                case 2:
                    outputLen = len - 33;
                    output = new byte[outputLen];
                    Buffer.BlockCopy(input, 32, output, 0, outputLen);
                    cipher8 = Cipher8FromIv(input);
                    break;
                default:
                    outputLen = len - 5;
                    output = new byte[outputLen];
                    Buffer.BlockCopy(input, 4, output, 0, outputLen);
                    var tmp = new byte[4];
                    Buffer.BlockCopy(input, 0, tmp, 0, 4);
                    Array.Reverse(tmp);
                    var ms = BitConverter.ToUInt32(tmp, 0);
                    cipher8 = Cipher8FromRand(ref ms);
                    if (input[len - 1] != MakeIntegrityByte(GenerateRand(ref ms))) { length = 0; return new byte[] { }; }
                    break;
            }

            var outputcontent = new Collection<byte[]>();

            //break into chunks of 256
            var roundedsize = (outputLen + 255) / 256; //round up
            for (var i = 0; i < roundedsize; i++)
                outputcontent.Add(new byte[256]);
            for (var i = 0; i < outputLen; i++)
                outputcontent[i / 256][i % 256] = output[i];

            foreach (var bytes in outputcontent)
            {
                var temp2 = new uint[0x100 / 4];
                var temp3 = new uint[0x100 / 4];
                Buffer.BlockCopy(bytes, 0, temp2, 0, 0x100);
                Buffer.BlockCopy(temp2, 0, temp3, 0, 0x100);

                if (version == 1)
                    ShufflesLegacy.Unshuffle(temp2);
                else
                    ShufflesLegacy.Unshuffle2(temp2);

                Buffer.BlockCopy(temp2, 0, bytes, 0, 0x100);
                for (var j = 0; j < 256; j++)
                {
                    bytes[j] ^= cipher8[j];
                }
                Buffer.BlockCopy(temp3, 0, cipher8, 0, 0x100);
            }

            var ret = new byte[outputLen];
            for (var i = 0; i < outputcontent.Count; i++)
            {
                Buffer.BlockCopy(outputcontent[i], 0, ret, i * 256, 0x100);
            }
            length = outputLen - ret.Last();
            return ret;
        }

        private class CipherText
        {
            private readonly byte[] _prefix;
            private readonly int _totalsize;

            public readonly Collection<byte[]> Content;

            private static byte[] IntToBytes(int x)
            {
                return BitConverter.GetBytes(x);
            }

            public CipherText(byte[] input, uint ms)
            {
                var inputlen = input.Length;
                _prefix = new byte[32];

                //allocate blocks of 256 bytes
                Content = new Collection<byte[]>();
                var roundedsize = inputlen + (256 - (inputlen % 256));
                for (var i = 0; i < roundedsize / 256; i++)
                    Content.Add(new byte[256]);
                _totalsize = roundedsize + 5;

                //first 32 bytes, pcrypt.c:68
                _prefix = IntToBytes((int)ms);
                Array.Reverse(_prefix);

                //split input into 256
                for (var i = 0; i < inputlen; i++) Content[i / 256][i % 256] = input[i];

                //pcrypt.c:75
                Content.Last()[Content.Last().Length - 1] = (byte)(256 - (input.Length % 256));
            }

            public byte[] GetBytes(ref uint ms)
            {
                var ret = new byte[_totalsize];
                Buffer.BlockCopy(_prefix, 0, ret, 0, _prefix.Length);
                var offset = _prefix.Length;
                foreach (var bytes in Content)
                {
                    Buffer.BlockCopy(bytes, 0, ret, offset, bytes.Length);
                    offset += bytes.Length;
                }
                ret[ret.Length - 1] = MakeIntegrityByte(GenerateRand(ref ms));
                return ret;
            }
        }
    }
}
