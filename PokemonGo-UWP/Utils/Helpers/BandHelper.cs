using Microsoft.Band;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonGo_UWP.Utils.Helpers
{
    public class BandHelper
    {
        private IBandClient _bandClient = null;
        private bool _isConnected = false;

        public BandHelper()
        {

        }

        ~BandHelper()
        {
            _bandClient?.Dispose();
        }

        public async Task<bool> Connect()
        {
            if (_isConnected == true)
                return false;

            IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();

            if (pairedBands?.Count() > 0)
            {
                try
                {
                    IBandClient client = await BandClientManager.Instance.ConnectAsync(pairedBands.First());
                    if (client != null)
                    {
                        _bandClient = client;
                        _isConnected = true;

                        string hwVer = await client.GetHardwareVersionAsync();
                        string fwVer = await client.GetFirmwareVersionAsync();

                        Debug.WriteLine("Connected to MS Band. HW: " + hwVer + ", FW: " + fwVer);

                        // Vibrate the band twice to acknowledge a connection
                        await _bandClient?.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.NotificationTwoTone);

                        return true;
                    }
                }
                catch (BandException ex)
                {
                    Debug.WriteLine("Band Exception: " + ex.Message);
                }
            }
            else
                Debug.WriteLine("No bands paired.");

            return false;
        }

        public async Task<int> GetPairedBandsCount()
        {
            IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
            int count = 0;
            if (pairedBands?.Count() > 0)
                count = pairedBands.Count();

            return count;
        }

        public async void Vibrate(Microsoft.Band.Notifications.VibrationType _type = Microsoft.Band.Notifications.VibrationType.NotificationOneTone)
        {
            await _bandClient?.NotificationManager.VibrateAsync(_type);
        }

#region Instance Stuff
        private static Lazy<BandHelper> _instance = new Lazy<BandHelper>(() => new BandHelper());
        public static BandHelper Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion
    }
}
