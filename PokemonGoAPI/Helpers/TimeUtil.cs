using System;

namespace POGOLib.Official.Util
{
    public static class TimeUtil
    {
        private static DateTime _posixTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Returns the current unix timestamp in milliseconds (UTC).
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimestampInMilliseconds()
        {
            return DateTime.UtcNow.ToMilliseconds();
        }

        /// <summary>
        ///     Returns the current unix timestamp in seconds (UTC).
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimestampInSeconds()
        {
            return DateTime.UtcNow.ToSeconds();
        }

        public static DateTime GetDateTimeFromMilliseconds(long timestampMilliseconds)
        {
            return _posixTime.AddMilliseconds(timestampMilliseconds);
        }

        public static DateTime GetDateTimeFromSeconds(int timestampSeconds)
        {
            return _posixTime.AddSeconds(timestampSeconds);
        }

        public static long ToMilliseconds(this DateTime dateTime)
        {
            return (long)(dateTime - _posixTime).TotalMilliseconds;
        }

        public static long ToSeconds(this DateTime dateTime)
        {
            return (long)(dateTime - _posixTime).TotalSeconds;
        }

        public static long ToMinutes(this DateTime dateTime)
        {
            return (long)(dateTime - _posixTime).TotalMinutes;
        }
    }
}