using Newtonsoft.Json;
using PokemonGo_UWP.Utils;
using PokemonGo_UWP.Utils.Helpers;
using PokemonGo_UWP.Views;
using POGOProtos.Inventory.Item;
using Template10.Common;
using Template10.Mvvm;
using System.ComponentModel;
using POGOProtos.Inventory;
using System;
using PokemonGo.RocketAPI.Extensions;

namespace PokemonGo_UWP.Entities
{
    public class AppliedItemWrapper : INotifyPropertyChanged
    {
        private AppliedItem _wrappedData;

        public AppliedItemWrapper(AppliedItem appliedItem)
        {
            _wrappedData = appliedItem;
        }

        [JsonProperty, JsonConverter(typeof(ProtobufJsonNetConverter))]
        public AppliedItem WrappedData { get { return _wrappedData; } }

        public void Update(AppliedItem update)
        {
            _wrappedData = update;

            OnPropertyChanged(nameof(ItemId));
            OnPropertyChanged(nameof(ItemType));
            OnPropertyChanged(nameof(AppliedMs));
            OnPropertyChanged(nameof(ExpireMs));
            OnPropertyChanged(nameof(MaxSeconds));
            OnPropertyChanged(nameof(RemainingMs));
            OnPropertyChanged(nameof(RemainingSeconds));
            OnPropertyChanged(nameof(RemainingTime));
        }

        #region Wrapped Properties

        public ItemId ItemId => _wrappedData.ItemId;

        public ItemType ItemType => _wrappedData.ItemType;

        public long AppliedMs => _wrappedData.AppliedMs;

        public long ExpireMs => _wrappedData.ExpireMs;

        public long RemainingMs
        {
            get
            {
                long nowMs = DateTime.UtcNow.ToUnixTime();
                long remainingMs = ExpireMs - nowMs;
                if (remainingMs < 0) remainingMs = 0;
                return remainingMs;
            }
        }

        public int RemainingSeconds
        {
            get
            {
                return (int)(RemainingMs / 1000);
            }
        }

        public string RemainingTime
        {
            get
            {
                DateTime remainingTime = new DateTime().AddMilliseconds(RemainingMs);
                return remainingTime.ToString("mm:ss");
            }
        }

        public bool IsExpired
        {
            get
            {
                if (RemainingMs <= 0) return true;
                return false;
            }
        }
        #endregion

        public int MaxSeconds
        {
            get
            {
                long maxMs = ExpireMs - AppliedMs;
                int maxSeconds = (int)maxMs / 1000;
                return maxSeconds;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
