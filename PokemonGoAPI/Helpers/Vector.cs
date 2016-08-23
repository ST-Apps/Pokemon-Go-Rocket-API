using System;

namespace PokemonGo.RocketAPI.Helpers
{
    public class Vector
    {
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public void NormalizeVector(double normalValue)
        {
            var length = Math.Sqrt(X*X + Y*Y + Z*Z)/normalValue;
            X = X/length;
            Y = Y/length;
            Z = Z/length;
        }

        public void Round(int places)
        {
            X = Math.Round(X, places);
            Y = Math.Round(Y, places);
            Z = Math.Round(Z, places);
        }
    }
}