using Microsoft.Band;
using Microsoft.Band.Tiles;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace PokemonGo_UWP.Utils.Helpers
{
    public class BandHelper
    {
        private IBandClient _bandClient = null;
        private bool _isConnected = false;

        private readonly Guid _appGuid = Guid.Parse("D5F82B61-5667-483B-B447-7990DAC6CB31");

        public BandHelper()
        {

        }

        ~BandHelper()
        {
            Disconnect();
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

                        ShowDialog( "Connected", "Pokemon Go! has been connected");

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

        public void Disconnect()
        {
            _bandClient?.Dispose();

            _bandClient = null;
            _isConnected = false;
        }

        public async Task<bool> SetupTile()
        {
            if (_bandClient == null)
                return false;

            int tileCapacity = await _bandClient.TileManager.GetRemainingTileCapacityAsync();
            if (tileCapacity == 0)
            {
                Debug.WriteLine("No space on Band for tile");
                return false;
            }

            // Create the tile with the app GUID
            BandTile bandTile = new BandTile(_appGuid)
            {
                Name = "PoGo",
            };

            if ( bandTile != null )
            {
                StorageFile largeiconfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/MSBand/LargeBandLogo.png"));
                StorageFile smalliconfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/MSBand/SmallBandLogo.png"));

                using (var stream = await largeiconfile.OpenAsync(FileAccessMode.Read))
                {
                    var wb = new WriteableBitmap(24, 24);
                    wb.SetSource(stream);
                    bandTile.TileIcon = wb.ToBandIcon();
                }

                using (var stream = await smalliconfile.OpenAsync(FileAccessMode.Read))
                {
                    var wb = new WriteableBitmap(46, 46);
                    wb.SetSource(stream);
                    bandTile.SmallIcon = wb.ToBandIcon();
                }

                try
                {
                    if (await _bandClient.TileManager.AddTileAsync(bandTile))
                    {
                        // Success!
                        SendMessage("Connected", "The Pokemon Go! tile has been added successfully.");
                        return true;
                    }
                }
                catch ( BandException ex )
                {
                    Debug.WriteLine("Band Exception: " + ex.Message);
                }
            }

            return false;
        }

        public async Task<bool> RemoveTile()
        {
            if (_bandClient == null)
                return false;

            try
            {
                await _bandClient.TileManager.RemoveTileAsync(_appGuid);

                return true;
            }
            catch ( BandException ex)
            {
                Debug.WriteLine("Band Exception: " + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Shows a dialog message on the band. This is a popup message that, once dismissed, is gone forever
        /// </summary>
        /// <param name="_header">The header to show on the dialog</param>
        /// <param name="_body">The text within the dialog</param>
        public async void ShowDialog( string _header, string _body)
        {
            if (_bandClient == null)
                return;

            await _bandClient.NotificationManager.ShowDialogAsync(_appGuid, _header, _body);
        }

        /// <summary>
        /// Sends a message to the band which is displayed and stored in the apps tile
        /// </summary>
        /// <param name="_header">The header for the message</param>
        /// <param name="_body">The text within the message</param>
        public async void SendMessage(string _header, string _body)
        {
            if (_bandClient == null)
                return;

            await _bandClient.NotificationManager.SendMessageAsync(_appGuid, _header, _body, DateTimeOffset.Now, Microsoft.Band.Notifications.MessageFlags.ShowDialog);
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
