using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Core.Abstractions
{
    public interface IDeviceConnectionManager
    {
        IObservable<DeviceStateChange?> DeviceStateChanges { get; }

        TeensyRomDevice? Connect(string deviceId);
        Task<List<TeensyRomDevice>> FindDevices(bool autoConnect, CancellationToken ct);
        List<TeensyRomDevice> GetConnectedDevices();
        TeensyRomDevice? GetConnectedDevice(string portName);
        Task<bool> ConnectToNextPort(string deviceId);
        void ClosePort(string deviceId);
        void StopHealthCheck();
    }
}
