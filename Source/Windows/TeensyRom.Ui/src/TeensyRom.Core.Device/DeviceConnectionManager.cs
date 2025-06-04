using CsvHelper.Configuration.Attributes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
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
    public class DeviceConnectionManager : IDeviceConnectionManager
    {
        private ConcurrentDictionary<string, TeensyRomDevice> _connectedDevices = [];
        private ConcurrentDictionary<string, TeensyRomDevice> _disconnectedDevices = [];

        private readonly ICartFinder _finder;
        private readonly ILoggingService _log;
        private readonly IFwVersionChecker _versionChecker;
        private bool _healthCheckEnabled = false;

        public DeviceConnectionManager(ICartFinder finder, ILoggingService log, IFwVersionChecker versionChecker)
        {
            _finder = finder;
            _log = log;
            _versionChecker = versionChecker;
        }

        public List<TeensyRomDevice> GetConnectedDevices() => _connectedDevices.Select(d => d.Value).ToList();
        public TeensyRomDevice? GetConnectedDevice(string deviceId) => GetConnectedDevices().FirstOrDefault(d => d.DeviceId == deviceId);
        public TeensyRomDevice? GetDisconnectedDevice(string deviceId) => _disconnectedDevices.TryGetValue(deviceId, out var device) ? device : null;

        public void ClosePort(string deviceId) 
        {
            if (_connectedDevices.TryRemove(deviceId, out var device))
            {
                device.SerialState.ClosePort();
                _disconnectedDevices.TryAdd(deviceId, device);
            }
        }

        private List<string> GetAvailablePorts()
        {
            var ports = SerialHelper.GetPorts();
            var availablePorts = ports.Except(_connectedDevices.Select(d => d.Value.Cart.ComPort)).ToList();
            return availablePorts;
        }

        public async Task<bool> ConnectToNextPort(string deviceId) 
        {            
            var availablePorts = GetAvailablePorts();

            var device = GetConnectedDevice(deviceId);

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

                var (isTeensyRom, isMinimal, isVersionCompatible, version) = _versionChecker.GetAllVersionInfo(device.SerialState);

                if (!isTeensyRom) 
                {
                    continue;
                }
                device.Cart.ComPort = port;
                return true;
            }
            _log.InternalError($"Could not reconnect to {deviceId}.  Check your devices and try reconnnecting.");
            device.SerialState.ClosePort();

            return false;
        }

        public async Task<List<TeensyRomDevice>> FindDevices(bool autoConnectNew, CancellationToken ct)
        {
            var randomNumber = Random.Shared.Next(1000, 9999);

            _healthCheckEnabled = false;

            var devicesToReconnect = _connectedDevices.Select(d => d.Key).ToList();

            _connectedDevices.ToList().ForEach(d => d.Value.SerialState.Dispose());

            List<TeensyRomDevice> devices = [];

            try
            {
                devices = await _finder.FindDevices(ct);
                _log.Internal($"{randomNumber} Devices found");
            }
            catch (OperationCanceledException)
            {
                _log.InternalError("The operation was cancelled while finding devices.");
                return [];
            }
            catch (Exception ex)
            {
                _log.ExternalError($"An error occurred while finding devices: {ex.Message}");
                return [];
            }                        

            _connectedDevices.Clear();

            if (!autoConnectNew) 
            {
                var devicesToKeep = devices.Where(d => devicesToReconnect.Contains(d.DeviceId)).ToList();
                var devicesToDisconnect = devices.Where(d => !devicesToReconnect.Contains(d.DeviceId)).ToList();

                foreach (var device in devicesToDisconnect)
                {
                    device.SerialState.ClosePort();
                    _disconnectedDevices.TryAdd(device.DeviceId, device);
                }
                foreach (var device in devicesToKeep)
                {
                    var _ = _connectedDevices.TryAdd(device.DeviceId, device);
                }
                return devices;
            }

            foreach (var device in devices)
            {
                if (!_connectedDevices.TryAdd(device.DeviceId, device))
                {
                    _log.InternalWarning($"Device with ID {device.DeviceId} already exists in the device list. Skipping duplicate.");
                    continue;
                }
            }

            StartHealthCheck();

            return devices;
        }

        public TeensyRomDevice? Connect(string deviceId)
        {
            var connectedDevice = GetConnectedDevice(deviceId);

            if (connectedDevice is not null) return connectedDevice;

            var device = GetDisconnectedDevice(deviceId);

            if (device is null) return null;

            device.SerialState.OpenPort();
            _disconnectedDevices.TryRemove(deviceId, out _);
            _connectedDevices.TryAdd(deviceId, device);

            return device;
        }

        private void StartHealthCheck()
        {
            _healthCheckEnabled = true;

            Task.Run(async () =>
            {
                List<TeensyRomDevice> _devicesToKill = [];

                while (_healthCheckEnabled)
                {
                    try
                    {
                        foreach (var device in _connectedDevices.Select(d => d.Value))
                        {
                            var currentState = await device.SerialState.CurrentState.FirstAsync();

                            if (currentState is SerialBusyState) continue;

                            if (!_healthCheckEnabled) return;

                            var result = await CheckDeviceHealth(device);

                            if (result is null)
                            {
                                _devicesToKill.Add(device);
                            }
                        }
                        _devicesToKill.ForEach(d =>
                        {
                            _log.InternalWarning($"Device {d.Cart.Name} - {d.DeviceId} @ {d.Cart.ComPort} is no longer connected.  Removing from device list.");
                            _connectedDevices.TryRemove(d.DeviceId, out TeensyRomDevice? device);
                            device?.SerialState.Dispose();                            
                        });
                        _devicesToKill.Clear();

                    }
                    catch (Exception ex)
                    {
                        _log.ExternalError($"Error in health check: {ex.Message}");
                    }
                    finally
                    {
                        await Task.Delay(SerialPortConstants.Health_Check_Milliseconds);
                    }
                }
            });
        }
        public void StopHealthCheck() => _healthCheckEnabled = false;

        private async Task<TeensyRomDevice?> CheckDeviceHealth(TeensyRomDevice device)
        {
            try
            {
                device.SerialState.EnsureConnection();
            }
            catch (UnauthorizedAccessException)
            {
                _log.InternalError($"DeviceConnectionManager.CheckDeviceHealth: Unauthorized access to {device.Cart.Name} - {device.DeviceId} @ {device.Cart.ComPort}.");
                _log.InternalError($"Please check if the port is already in use.");
                return null;
            }
            catch (Exception)
            {
                _log.ExternalError($"There is a problem with the connection to {device.Cart.Name} - {device.DeviceId}");
                _log.ExternalError($"Attempting reconnection in {SerialPortConstants.Health_Check_Milliseconds}.");

                var currentState = await device.SerialState.CurrentState.FirstAsync();

                if (currentState is not SerialConnectionLostState)
                {
                    device.SerialState.TransitionTo(typeof(SerialConnectionLostState));
                }
            }
            return device;
        }

        private IDisposable? _healthCheckSubscription;
    }
}
