using System;

namespace POGOLib.Official.Util.Hash
{
    public class HashVersionMismatchException : Exception
    {
        public HashVersionMismatchException(string message) : base(message)
        {
        }
    }
}
