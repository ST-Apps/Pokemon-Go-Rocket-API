using System;

namespace POGOLib.Official.Util.Encryption.Legacy
{   
    /// <summary>
    ///     This is the legacy NiaHash used by POGOLib.
    /// 
    ///     Android version: 0.45.0
    ///     IOS version: 1.15.0
    /// </summary>
    internal static class NiaHashLegacy
    {
        /* IOS 1.15.0 */
        private static readonly ulong[] MagicTable = {
            0x2dd7caaefcf073eb, 0xa9209937349cfe9c,
            0xb84bfc934b0e60ef, 0xff709c157b26e477,
            0x3936fd8735455112, 0xca141bf22338d331,
            0xdd40e749cb64fd02, 0x5e268f564b0deb26,
            0x658239596bdea9ec, 0x31cedf33ac38c624,
            0x12f56816481b0cfd, 0x94e9de155f40f095,
            0x5089c907844c6325, 0xdf887e97d73c50e3,
            0xae8870787ce3c11d, 0xa6767d18c58d2117,
        };

        private static readonly UInt128 RoundMagic = new UInt128(0xe3f0d44988bcdfab, 0x081570afdd535ec3);
        private const ulong FinalMagic0 = 0xce7c4801d683e824;
        private const ulong FinalMagic1 = 0x6823775b1daad522;
        private const uint HashSeed = 0x46e945f8;

        private static ulong read_int64(byte[] p, int offset)
        {
            return BitConverter.ToUInt64(p, offset);
        }

        public static uint Hash32(byte[] buffer)
        {
            return Hash32Salt(buffer, HashSeed);
        }

        public static uint Hash32Salt(byte[] buffer, uint salt)
        {
            var ret = Hash64Salt(buffer, salt);
            return (uint)ret ^ (uint)(ret >> 32);
        }

        public static ulong Hash64(byte[] buffer)
        {
            return Hash64Salt(buffer, HashSeed);
        }

        public static ulong Hash64Salt(byte[] buffer, uint salt)
        {
            var newBuffer = new byte[buffer.Length + 4];
            var saltBytes = BitConverter.GetBytes(salt);
            Array.Reverse(saltBytes);
            Array.Copy(saltBytes, 0, newBuffer, 0, saltBytes.Length);
            Array.Copy(buffer, 0, newBuffer, saltBytes.Length, buffer.Length);

            return Hash(newBuffer);
        }

        public static ulong Hash64Salt64(byte[] buffer, ulong salt)
        {
            var newBuffer = new byte[buffer.Length + 8];
            var saltBytes = BitConverter.GetBytes(salt);
            Array.Reverse(saltBytes);
            Array.Copy(saltBytes, 0, newBuffer, 0, saltBytes.Length);
            Array.Copy(buffer, 0, newBuffer, saltBytes.Length, buffer.Length);

            return Hash(newBuffer);
        }

        private static ulong Hash(byte[] input)
        {
            var len = (uint)input.Length;
            var numChunks = len / 128;

            // copy tail, pad with zeroes
            var tail = new byte[128];
            var tailSize = len % 128;
            Buffer.BlockCopy(input, (int) (len - tailSize), tail, 0, (int) tailSize);

            UInt128 hash;

            hash = numChunks != 0 
                ? hash_chunk(input, 128, 0) 
                : hash_chunk(tail, tailSize, 0);

            hash += RoundMagic;

            var offset = 0;

            if (numChunks != 0)
            {
                while (--numChunks > 0)
                {
                    offset += 128;
                    hash = hash_muladd(hash, RoundMagic, hash_chunk(input, 128, offset));
                }

                if (tailSize > 0)
                {
                    hash = hash_muladd(hash, RoundMagic, hash_chunk(tail, tailSize, 0));
                }
            }

            hash += new UInt128(tailSize * 8, 0);

            if (hash > new UInt128(0x7fffffffffffffff, 0xffffffffffffffff)) hash++;

            hash = hash << 1 >> 1;

            var x = hash.Hi + (hash.Lo >> 32);
            x = ((x + (x >> 32) + 1) >> 32) + hash.Hi;
            var y = (x << 32) + hash.Lo;

            var a = x + FinalMagic0;
            if (a < x) a += 0x101;

            var b = y + FinalMagic1;
            if (b < y) b += 0x101;

            var h = new UInt128(a) * b;
            var mul = new UInt128(0x101);
            h = (mul * h.Hi) + h.Lo;
            h = (mul * h.Hi) + h.Lo;

            if (h.Hi > 0) h += mul;
            if (h.Lo > 0xFFFFFFFFFFFFFEFE) h += mul;
            return h.Lo;
        }

        static UInt128 hash_chunk(byte[] chunk, long size, int off)
        {
            var hash = new UInt128(0);
            for (var i = 0; i < 8; i++)
            {
                var offset = i * 16;
                if (offset >= size) break;
                var a = read_int64(chunk, off + offset);
                var b = read_int64(chunk, off + offset + 8);
                hash += (new UInt128(a + MagicTable[i * 2])) * (new UInt128(b + MagicTable[i * 2 + 1]));
            }
            return hash << 2 >> 2;
        }

        static UInt128 hash_muladd(UInt128 hash, UInt128 mul, UInt128 add)
        {
            ulong a0 = add.Lo & 0xffffffff,
                a1 = add.Lo >> 32,
                a23 = add.Hi;

            ulong m0 = mul.Lo & 0xffffffff,
                m1 = mul.Lo >> 32,
                m2 = mul.Hi & 0xffffffff,
                m3 = mul.Hi >> 32;

            ulong h0 = hash.Lo & 0xffffffff,
                h1 = hash.Lo >> 32,
                h2 = hash.Hi & 0xffffffff,
                h3 = hash.Hi >> 32;

            ulong c0 = (h0 * m0),
                c1 = (h0 * m1) + (h1 * m0),
                c2 = (h0 * m2) + (h1 * m1) + (h2 * m0),
                c3 = (h0 * m3) + (h1 * m2) + (h2 * m1) + (h3 * m0),
                c4 = (h1 * m3) + (h2 * m2) + (h3 * m1),
                c5 = (h2 * m3) + (h3 * m2),
                c6 = (h3 * m3);

            ulong r2 = c2 + (c6 << 1) + a23,
                r3 = c3 + (r2 >> 32),
                r0 = c0 + (c4 << 1) + a0 + (r3 >> 31),
                r1 = c1 + (c5 << 1) + a1 + (r0 >> 32);

            var res0 = ((r3 << 33 >> 1) | (r2 & 0xffffffff)) + (r1 >> 32);
            return new UInt128(res0, (r1 << 32) | (r0 & 0xffffffff));
        }

        internal struct UInt128
        {
            public ulong Hi, Lo;

            #region constructors

            public UInt128(ulong high, ulong low)
            {
                Hi = high; Lo = low;
            }

            public UInt128(ulong low)
            {
                Hi = 0; Lo = low;
            }

            #endregion
            #region comparators

            public bool Equals(UInt128 other)
            {
                return (Hi == other.Hi && Lo == other.Lo);
            }

            public static bool operator >(UInt128 a, UInt128 b)
            {
                if (a.Hi == b.Hi) return a.Lo > b.Lo;
                return a.Hi > b.Hi;
            }

            public static bool operator <(UInt128 a, UInt128 b)
            {
                if (a.Hi == b.Hi) return a.Lo < b.Lo;
                return a.Hi < b.Hi;
            }

            #endregion
            #region arithmetic

            public static UInt128 operator ++(UInt128 a)
            {
                a.Lo++;
                if (a.Lo == 0) a.Hi++;
                return a;
            }

            public static UInt128 operator +(UInt128 a, UInt128 b)
            {
                var c = (((a.Lo & b.Lo) & 1) + (a.Lo >> 1) + (b.Lo >> 1)) >> 63;
                return new UInt128(a.Hi + b.Hi + c, a.Lo + b.Lo);
            }

            public static UInt128 operator +(UInt128 a, ulong b)
            {
                return a + new UInt128(b);
            }

            public static UInt128 operator -(UInt128 a, UInt128 b)
            {
                var l = a.Lo - b.Lo;
                var c = (((l & b.Lo) & 1) + (b.Lo >> 1) + (l >> 1)) >> 63;
                return new UInt128(a.Hi - (b.Hi + c), l);
            }

            #endregion
            #region bitwise operations

            public static UInt128 operator &(UInt128 a, UInt128 b)
            {
                return new UInt128(a.Hi & b.Hi, a.Lo & b.Lo);
            }

            public static UInt128 operator <<(UInt128 a, int b)
            {
                a.Hi <<= b;
                a.Hi |= (a.Lo >> (64 - b));
                a.Lo <<= b;
                return a;
            }

            public static UInt128 operator >>(UInt128 a, int b)
            {
                a.Lo >>= b;
                a.Lo |= (a.Hi << (64 - b));
                a.Hi >>= b;
                return a;
            }

            #endregion
            #region multiplication

            private static UInt128 M64(ulong a, ulong b)
            {
                ulong a1 = (a & 0xffffffff), b1 = (b & 0xffffffff),
                    t = (a1 * b1), w3 = (t & 0xffffffff), k = (t >> 32), w1;

                a >>= 32;
                t = (a * b1) + k;
                k = (t & 0xffffffff);
                w1 = (t >> 32);

                b >>= 32;
                t = (a1 * b) + k;
                k = (t >> 32);

                return new UInt128((a * b) + w1 + k, (t << 32) + w3);
            }

            public static UInt128 operator *(UInt128 a, int b) { return a * (ulong)b; }

            public static UInt128 operator *(UInt128 a, ulong b)
            {
                var ans = M64(a.Lo, b);
                ans.Hi += (a.Hi * b);
                return ans;
            }

            public static UInt128 operator *(UInt128 a, UInt128 b)
            {
                var ans = M64(a.Lo, b.Lo);
                ans.Hi += (a.Hi * b.Lo) + (a.Lo * b.Hi);
                return ans;
            }

            #endregion
        }
    }
}