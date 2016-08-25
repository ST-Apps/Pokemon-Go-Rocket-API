using System;
using System.Collections;
using System.Collections.Generic;

namespace Superbest_random
{
    /// <summary>
    ///     Some extension methods for <see cref="Random" /> for creating a few more kinds of random stuff.
    /// </summary>
    public static class RandomExtensions
    {
        /// <summary>
        ///     Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently
        ///     they can be cached or something for better performance, but who cares.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="mu">Mean of the distribution</param>
        /// <param name="sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0*Math.Log(u1))*
                                Math.Sin(2.0*Math.PI*u2);

            var randNormal = mu + sigma*randStdNormal;

            return randNormal;
        }

        public static double NextDouble(this Random r, double minimum, double maximum)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }

        /// <summary>
        ///     Generates values from a triangular distribution.
        /// </summary>
        /// <remarks>
        ///     See http://en.wikipedia.org/wiki/Triangular_distribution for a description of the triangular probability
        ///     distribution and the algorithm for generating one.
        /// </remarks>
        /// <param name="r"></param>
        /// <param name="a">Minimum</param>
        /// <param name="b">Maximum</param>
        /// <param name="c">Mode (most frequent value)</param>
        /// <returns></returns>
        public static double NextTriangular(this Random r, double a, double b, double c)
        {
            var u = r.NextDouble();

            return u < (c - a)/(b - a)
                ? a + Math.Sqrt(u*(b - a)*(c - a))
                : b - Math.Sqrt((1 - u)*(b - a)*(b - c));
        }

        /// <summary>
        ///     Equally likely to return true or false. Uses <see cref="Random.Next()" />.
        /// </summary>
        /// <returns></returns>
        public static bool NextBoolean(this Random r)
        {
            return r.Next(2) > 0;
        }

        /// <summary>
        ///     Shuffles a list in O(n) time by using the Fisher-Yates/Knuth algorithm.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="list"></param>
        public static void Shuffle(this Random r, IList list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                var j = r.Next(0, i + 1);

                var temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }

        /// <summary>
        ///     Returns n unique random numbers in the range [1, n], inclusive.
        ///     This is equivalent to getting the first n numbers of some random permutation of the sequential numbers from 1 to
        ///     max.
        ///     Runs in O(k^2) time.
        /// </summary>
        /// <param name="rand"></param>
        /// <param name="n">Maximum number possible.</param>
        /// <param name="k">How many numbers to return.</param>
        /// <returns></returns>
        public static int[] Permutation(this Random rand, int n, int k)
        {
            var result = new List<int>();
            var sorted = new SortedSet<int>();

            for (var i = 0; i < k; i++)
            {
                var r = rand.Next(1, n + 1 - i);

                foreach (var q in sorted)
                    if (r >= q) r++;

                result.Add(r);
                sorted.Add(r);
            }

            return result.ToArray();
        }
    }
}