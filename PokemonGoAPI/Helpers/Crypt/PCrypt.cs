using System;
using System.Collections.ObjectModel;
using System.Linq;

class PCrypt
{
    static byte rot18(byte val, int bits)
    {
        return (byte)(((val << bits) | (val >> (8 - bits))) & 0xff);
    }

    static byte gen_rand(ref uint rand)
    {
        rand = rand * 0x41c64e6d + 12345;
        return (byte)((rand >> 16) & 0xff);
    }

    static byte[] cipher8_from_iv(byte[] iv)
    {
        byte[] ret = new byte[256];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 32; j++)
            {
                ret[32 * i * j] = rot18(iv[j], i);
            }
        }
        return ret;
    }

    static byte[] cipher8_from_rand(ref uint rand)
    {
        byte[] ret = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            ret[i] = gen_rand(ref rand);
        }
        return ret;
    }

    static byte make_integrity_byte(byte b)
    {
        byte tmp = (byte)((b ^ 0x0c) & b);
        return (byte)(((~tmp & 0x67) | (tmp & 0x98)) ^ 0x6f | (tmp & 0x08));
    }

    public static byte[] encrypt(byte[] input, uint ms)
    {
        CipherText ct = new CipherText(input, ms);
        byte[] iv = cipher8_from_rand(ref ms);

        //encrypt
        for (int i = 0; i < ct.content.Count; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                ct.content[i][j] ^= iv[j];
            }

            uint[] temp2 = new uint[0x100 / 4];
            Buffer.BlockCopy(ct.content[i], 0, temp2, 0, 0x100);
            Shuffles.Shuffle2(temp2);

            Buffer.BlockCopy(temp2, 0, iv, 0, 0x100);
            Buffer.BlockCopy(temp2, 0, ct.content[i], 0, 0x100);
        }

        return ct.getBytes(ref ms);
    }

    //this returns an empty buffer if error
    public static byte[] decrypt(byte[] input, out int length)
    {
        int version, len = input.Length;
        if (len < 261) { length = 0; return new byte[] { }; }
        else
        {
            int mod_size = len % 256;
            if (mod_size == 32) version = 1;
            else if (mod_size == 33) version = 2;
            else if (mod_size == 5) version = 3;
            else { length = 0; return new byte[] { }; }
        }

        byte[] cipher8, output;
        int output_len;
        if (version == 1)
        {
            output_len = len - 32;
            output = new byte[output_len];
            Buffer.BlockCopy(input, 32, output, 0, output_len);
            cipher8 = cipher8_from_iv(input);
        }
        else if (version == 2)
        {
            output_len = len - 33;
            output = new byte[output_len];
            Buffer.BlockCopy(input, 32, output, 0, output_len);
            cipher8 = cipher8_from_iv(input);
        }
        else
        {
            output_len = len - 5;
            output = new byte[output_len];
            Buffer.BlockCopy(input, 4, output, 0, output_len);
            byte[] tmp = new byte[4];
            Buffer.BlockCopy(input, 0, tmp, 0, 4);
            Array.Reverse(tmp);
            uint ms = BitConverter.ToUInt32(tmp, 0);
            cipher8 = cipher8_from_rand(ref ms);
            if (input[len - 1] != make_integrity_byte(gen_rand(ref ms))) { length = 0; return new byte[] { }; }
        }

        Collection<byte[]> outputcontent = new Collection<byte[]>();

        //break into chunks of 256
        int roundedsize = (output_len + 255) / 256; //round up
        for (int i = 0; i < roundedsize; i++) outputcontent.Add(new byte[256]);
        for (int i = 0; i < output_len; i++) outputcontent[i / 256][i % 256] = output[i];

        for (int i = 0; i < outputcontent.Count; i++)
        {
            uint[] temp2 = new uint[0x100 / 4];
            uint[] temp3 = new uint[0x100 / 4];
            Buffer.BlockCopy(outputcontent[i], 0, temp2, 0, 0x100);
            Buffer.BlockCopy(temp2, 0, temp3, 0, 0x100);
            if (version == 1) Shuffles.Unshuffle(temp2);
            else Shuffles.Unshuffle2(temp2);
            Buffer.BlockCopy(temp2, 0, outputcontent[i], 0, 0x100);
            for (int j = 0; j < 256; j++)
            {
                outputcontent[i][j] ^= cipher8[j];
            }
            Buffer.BlockCopy(temp3, 0, cipher8, 0, 0x100);
        }

        byte[] ret = new byte[output_len];
        for (int i = 0; i < outputcontent.Count; i++)
        {
            Buffer.BlockCopy(outputcontent[i], 0, ret, i * 256, 0x100);
        }
        length = output_len - ret.Last();
        return ret;
    }

    public class CipherText
    {
        byte[] prefix;
        public Collection<byte[]> content;

        int totalsize;
        int inputlen;

        byte[] intToBytes(int x) { return BitConverter.GetBytes(x); }

        public CipherText(byte[] input, uint ms)
        {
            inputlen = input.Length;
            prefix = new byte[32];

            //allocate blocks of 256 bytes
            content = new Collection<byte[]>();
            int roundedsize = inputlen + (256 - (inputlen % 256));
            for (int i = 0; i < roundedsize / 256; i++) content.Add(new byte[256]);
            totalsize = roundedsize + 5;

            //first 32 bytes, pcrypt.c:68
            prefix = intToBytes((int)ms);
            Array.Reverse(prefix);

            //split input into 256
            for (int i = 0; i < inputlen; i++) content[i / 256][i % 256] = input[i];

            //pcrypt.c:75
            content.Last()[content.Last().Length - 1] = (byte)(256 - (input.Length % 256));
        }

        public byte[] getBytes(ref uint ms)
        {
            byte[] ret = new byte[totalsize];
            Buffer.BlockCopy(prefix, 0, ret, 0, prefix.Length);
            int offset = prefix.Length;
            for (int i = 0; i < content.Count; i++)
            {
                Buffer.BlockCopy(content[i], 0, ret, offset, content[i].Length);
                offset += content[i].Length;
            }
            ret[ret.Length - 1] = make_integrity_byte(gen_rand(ref ms));
            return ret;
        }
    }
}