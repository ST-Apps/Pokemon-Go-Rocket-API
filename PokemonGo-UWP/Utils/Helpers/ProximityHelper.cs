using System;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;

namespace PokemonGo_UWP.Utils.Helpers
{
    public class ProximityHelper
    {
        private ProximitySensor _sensor;
        private ProximitySensorDisplayOnOffController _displayController;
        private DeviceWatcher _watcher;

        public ProximityHelper()
        {
            _watcher = DeviceInformation.CreateWatcher(ProximitySensor.GetDeviceSelector());
            _watcher.Added += OnProximitySensorAdded;
            _watcher.Start();
        }

        ~ProximityHelper()
        {
            _watcher.Stop();

            if (_displayController != null)
            {
                _displayController.Dispose();
                _displayController = null;
            }
        }

        private void OnProximitySensorAdded(DeviceWatcher sender, DeviceInformation args)
        {
            if (_sensor == null)
            {
                ProximitySensor foundSensor = ProximitySensor.FromId(args.Id);
                if (foundSensor != null)
                {
                    _sensor = foundSensor;
                }
                else
                {
                    // No proximity sensor found
                    Debug.WriteLine("No proximity sensor found");
                }
            }
        }

        public void EnableDisplayAutoOff(bool _enabled)
        {
            if (_enabled)
            {
                if ( ( _sensor != null ) && (_displayController == null ) )
                {
                    _displayController = _sensor.CreateDisplayOnOffController();
                }
            }
            else
            {
                if (_displayController != null )
                {
                    _displayController.Dispose();
                    _displayController = null;
                }
            }
        }
    }
}
