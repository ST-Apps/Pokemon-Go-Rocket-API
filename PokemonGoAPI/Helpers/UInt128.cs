struct UInt128
{
    public ulong hi, lo;

    #region constructors

    public UInt128(ulong high, ulong low)
    {
        hi = high; lo = low;
    }

    public UInt128(ulong low)
    {
        hi = 0; lo = low;
    }

    #endregion
    #region comparators

    public bool Equals(UInt128 other)
    {
        return (hi == other.hi && lo == other.lo);
    }

    public static bool operator >(UInt128 a, UInt128 b)
    {
        if (a.hi == b.hi) return a.lo > b.lo;
        return a.hi > b.hi;
    }

    public static bool operator <(UInt128 a, UInt128 b)
    {
        if (a.hi == b.hi) return a.lo < b.lo;
        return a.hi < b.hi;
    }

    #endregion
    #region arithmetic

    public static UInt128 operator ++(UInt128 a)
    {
        a.lo++;
        if (a.lo == 0) a.hi++;
        return a;
    }

    public static UInt128 operator +(UInt128 a, UInt128 b)
    {
        ulong C = (((a.lo & b.lo) & 1) + (a.lo >> 1) + (b.lo >> 1)) >> 63;
        return new UInt128(a.hi + b.hi + C, a.lo + b.lo);
    }

    public static UInt128 operator +(UInt128 a, ulong b)
    {
        return a + new UInt128(b);
    }

    public static UInt128 operator -(UInt128 a, UInt128 b)
    {
        ulong L = a.lo - b.lo;
        ulong C = (((L & b.lo) & 1) + (b.lo >> 1) + (L >> 1)) >> 63;
        return new UInt128(a.hi - (b.hi + C), L);
    }

    #endregion
    #region bitwise operations

    public static UInt128 operator &(UInt128 a, UInt128 b)
    {
        return new UInt128(a.hi & b.hi, a.lo & b.lo);
    }

    public static UInt128 operator <<(UInt128 a, int b)
    {
        a.hi <<= b;
        a.hi |= (a.lo >> (64 - b));
        a.lo <<= b;
        return a;
    }

    public static UInt128 operator >>(UInt128 a, int b)
    {
        a.lo >>= b;
        a.lo |= (a.hi << (64 - b));
        a.hi >>= b;
        return a;
    }

    #endregion
    #region multiplication

    private static UInt128 m64(ulong a, ulong b)
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
        UInt128 ans = m64(a.lo, b);
        ans.hi += (a.hi * b);
        return ans;
    }

    public static UInt128 operator *(UInt128 a, UInt128 b)
    {
        UInt128 ans = m64(a.lo, b.lo);
        ans.hi += (a.hi * b.lo) + (a.lo * b.hi);
        return ans;
    }

    #endregion
}