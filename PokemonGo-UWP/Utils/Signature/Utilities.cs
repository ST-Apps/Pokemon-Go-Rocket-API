using System;

namespace PokemonGo_UWP.Utils
{
    public static class Utilities
    {
        /// <summary>Provides Random instance</summary>
        public static readonly Random Rand = new Random();

        /// <summary>Generates random HEX number as string with defined length</summary>
        /// <param name="length">Length of wanted HEX</param>
        /// <returns>Hex number as string</returns>
        public static string RandomHex(int length)
        {
            const string chars = "ABCDEF0123456789";
            return RandomString(chars, length);
        }

        /// <summary>Generates random decimal number as string with defined length</summary>
        /// <param name="length">Length of wanted decimal</param>
        /// <returns>Decimal number as string</returns>
        public static string RandomNum(int length)
        {
            const string chars = "0123456789";
            return RandomString(chars, length);
        }

        /// <summary>Generates random string from defined characters with defined length</summary>
        /// <param name="chars">Chars from which the string will be created</param>
        /// <param name="length">Length of wanted string</param>
        /// <returns>Random string with defined length</returns>
        public static string RandomString(string chars, int length)
        {
            var stringChars = new char[length];

            for (var i = 0; i < stringChars.Length; i++)
            {
                var randIndex = Rand.Next(chars.Length);
                stringChars[i] = chars[randIndex];
            }

            return new string(stringChars);
        }

        /// <summary>
        ///     Generic function that clips any IComparable value with min and max.
        ///     Also if min is less than max, it corrects for that automatically
        /// </summary>
        /// <typeparam name="T">Any IComparable type</typeparam>
        /// <param name="value">Input value, that will be clipped</param>
        /// <param name="min">Minimum for clipping</param>
        /// <param name="max">Maximum for clipping</param>
        /// <returns>Clipped value by min,max</returns>
        public static T EnsureRange<T>(T value, T min, T max) where T : IComparable
        {
            var res = min.CompareTo(max);
            if (res == 0)
                return min;

            if (res > 0)
            {
                var temp = max;
                max = min;
                min = temp;
            }

            return value.CompareTo(min) < 0 ? min : (value.CompareTo(max) > 0 ? max : value);
        }

        #region Random Extension methods

        /// <summary>Calculates random value in [0;max], it includes maximum as well</summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="max">Maximum, that is also included</param>
        /// <returns>Returns random value in [0;max]</returns>
        public static int NextInclusive(this Random rnd, int max) =>
            rnd.Next(max + 1);

        /// <summary>Calculates random value in [min;max], it includes maximum as well</summary>
        /// <param name="rnd">Random instance</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum, that is also included</param>
        /// <returns>Returns random value in [min;max]</returns>
        public static int NextInclusive(this Random rnd, int min, int max) =>
            rnd.Next(min, max + 1);

        #endregion Extension methods
    }
}