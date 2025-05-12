using System.Reactive.Linq;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Device
{
    public class DeviceConnectionManager(ICartFinder finder, ICartTagger tagger, ILoggingService log, IAlertService alert, ISerialFactory serialFactory, IStorageFactory storageFactory, IFwVersionChecker versionChecker) : IDeviceConnectionManager
    {
        private List<TeensyRomDevice> _availableDevices = [];
        private List<TeensyRomDevice> _connectedDevices = [];

        public List<string> GetAvailablePorts()
        {
            var ports = SerialHelper.GetPorts();
            var availablePorts = ports.Except(_connectedDevices.Select(d => d.Cart.ComPort)).ToList();
            return availablePorts;
        }

        public async Task<bool> ConnectToNextPort(string deviceId) 
        {            
            var availablePorts = GetAvailablePorts();

            var device = _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (device is null)
            {
                throw new TeensyException($"Device with ID {deviceId} not found in connected devices.");
            }   
            
            foreach (var port in availablePorts)
            {
                var state = await device.SerialState.CurrentState.FirstAsync();

                device.SerialState.TransitionTo(typeof(SerialConnectedState));

                if (device.SerialState.IsOpen) 
                {
                    try
                    {
                        device.SerialState.ClosePort();
                    }
                    catch (Exception ex) 
                    { 
                    }
                }                
                device.SerialState.SetPort(port);
                device.SerialState.OpenPort();
                device.SerialState.Lock();
                device.SerialState.TransitionTo(typeof(SerialBusyState));

                var (isTeensyRom, isMinimal, isVersionCompatible, version) = versionChecker.GetAllVersionInfo(device.SerialState);

                if (!isTeensyRom) 
                {
                    continue;
                }
                device.Cart.ComPort = port;
                return true;
            }
            log.InternalError($"Could not reconnect to {deviceId}.  Check your devices and try reconnnecting.");
            device.SerialState.ClosePort();
            _connectedDevices.Remove(device);
            return false;
        }

        public TeensyRomDevice? GetConnectedDevice(string deviceId)
        {
            return _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);
        }

        public List<Cart> GetConnectedCarts() => _connectedDevices.Select(c => c.Cart).ToList();

        public List<TeensyRomDevice> GetAllConnectedDevices() 
        {  
            return _connectedDevices.ToList();
        }

        public async Task<List<Cart>> FindAvailableCarts()
        {
            _availableDevices.ForEach(d => d.SerialState.Dispose());
            _connectedDevices.ForEach(d => d.SerialState.Dispose());
            _availableDevices.Clear();

            var devices = await finder.FindDevices();
            _availableDevices.AddRange(devices);

            var connectedDevices = _availableDevices
                .Where(d => _connectedDevices
                    .Any(c => c.Cart.DeviceId == d.Cart.DeviceId)).ToList();

            var disconnectedDevices = _availableDevices
                .Where(d => !connectedDevices
                    .Any(c => c.Cart.DeviceId == d.Cart.DeviceId)).ToList();

            disconnectedDevices.ForEach(d => d.SerialState.ClosePort());

            _connectedDevices.Clear();
            _connectedDevices.AddRange(connectedDevices);

            return _availableDevices.Select(d => d.Cart).ToList();
        }

        public TeensyRomDevice? Connect(string deviceId)
        {
            var alreadyConnected = _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (alreadyConnected is not null) return alreadyConnected;

            var knownDevice = _availableDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (knownDevice is null)
            {
                return null;
            }
            _connectedDevices.Add(knownDevice);
            knownDevice.SerialState.OpenPort();
            return knownDevice;
        }
    }
}
