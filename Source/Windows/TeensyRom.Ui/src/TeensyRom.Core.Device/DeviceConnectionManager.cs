using CsvHelper.Configuration.Attributes;
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
        private List<TeensyRomDevice> _devices = [];

        public void ClosePort(string deviceId) => GetConnectedDevice(deviceId)?.SerialState.Dispose();
        public List<TeensyRomDevice> GetConnectedDevices() => _devices.Where(d => d.IsConnected).ToList();
        public TeensyRomDevice? GetDevice(string deviceId) => _devices.FirstOrDefault(d => d.DeviceId == deviceId);
        public TeensyRomDevice? GetConnectedDevice(string deviceId) => GetConnectedDevices().FirstOrDefault(d => d.DeviceId == deviceId);

        private List<string> GetAvailablePorts()
        {
            var ports = SerialHelper.GetPorts();
            var availablePorts = ports.Except(_devices.Select(d => d.Cart.ComPort)).ToList();
            return availablePorts;
        }

        public async Task<bool> ConnectToNextPort(string deviceId) 
        {            
            var availablePorts = GetAvailablePorts();

            var device = GetDevice(deviceId);

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

            return false;
        }

        public async Task<List<TeensyRomDevice>> FindDevices()
        {
            var previouslyConnected = GetConnectedDevices();
            _devices.ForEach(d => d.SerialState.Dispose());
            _devices.Clear();

            var devices = await finder.FindDevices();

            _devices.AddRange(devices);

            var devicesToDisconnect = _devices
                .Where(d => !previouslyConnected.Any(d => d.DeviceId == d.DeviceId))
                .ToList();

            devicesToDisconnect.ForEach(d => d.SerialState.ClosePort());

            return _devices;
        }

        public TeensyRomDevice? Connect(string deviceId)
        {
            var connectedDevice = GetConnectedDevice(deviceId);

            if (connectedDevice is not null) return connectedDevice;

            var device = GetDevice(deviceId);

            if (device is null) return null;

            device.SerialState.OpenPort();
            return device;
        }
    }
}
