using System;
using System.Threading;

namespace POGOLib.Official.Util.Hash.PokeHash
{
    internal class PokeHashAuthKey : IComparable<PokeHashAuthKey>
    {
        private volatile int _requests;

        public PokeHashAuthKey(string authKey)
        {
            AuthKey = authKey;
            MaxRequests = 150;
            WaitList = new Semaphore(1, 1);
        }

        /// <summary>
        ///     The auth key obtained from https://talk.pogodev.org/d/51-api-hashing-service-by-pokefarmer.
        /// </summary>
        public string AuthKey { get; }

        /// <summary>
        ///     The maximum requests per second.
        /// </summary>
        public int MaxRequests { get; set; }

        public bool MaxRequestsParsed { get; set; }

        /// <summary>
        ///     The amount of requests that have been made
        ///     in this second.
        /// </summary>
        public int Requests
        {
            get
            {
                // Check if the current second is different from the last request.
                var minutes = DateTime.UtcNow.ToMinutes() - LastRequestMinutes;
                if (minutes >= 1)
                {
                    _requests = 0;
                }

                return _requests;
            }
            set
            {
                LastRequestMinutes = DateTime.UtcNow.ToMinutes();

                _requests = value;
            }
        }

        /// <summary>
        ///     The waiting list.
        /// </summary>
        public Semaphore WaitList { get; }

        /// <summary>
        ///     Amount of threads waiting to use this key.
        /// </summary>
        public int WaitListCount { get; set; }

        /// <summary>
        ///     The last time a request was sent using this <see cref="AuthKey"/>.
        /// </summary>
        public long LastRequestMinutes { get; private set; }

        /// <summary>
        ///     Determines whether this <see cref="AuthKey"/> can be used in this current minute.
        /// </summary>
        /// <returns>Returns true if the <see cref="AuthKey"/> can be used.</returns>
        public bool IsUsable()
        {
            return Requests < MaxRequests;
        }

        /// <summary>
        ///     Time left in seconds until the key is usable.
        /// </summary>
        /// <returns></returns>
        public int GetTimeLeft()
        {
            var secondsUntilNextMinute = 60 - DateTime.UtcNow.Second;

            if (WaitListCount <= 0)
                return secondsUntilNextMinute;
            
            var minutesUntil = WaitListCount / MaxRequests;
            if (minutesUntil < 0)
                minutesUntil = 0;

            return secondsUntilNextMinute + minutesUntil * 60;
        }

        public int CompareTo(PokeHashAuthKey that)
        {
            return string.Compare(AuthKey, that.AuthKey, StringComparison.Ordinal);
        }
    }
}
