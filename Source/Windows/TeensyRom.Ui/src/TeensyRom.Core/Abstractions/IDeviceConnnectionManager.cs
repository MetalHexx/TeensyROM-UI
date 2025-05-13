using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Core.Abstractions
{
    public interface IDeviceConnectionManager
    {
        TeensyRomDevice? Connect(string deviceId);
        Task<List<Cart>> FindAvailableCarts();
        List<TeensyRomDevice> GetAllConnectedDevices();
        List<Cart> GetConnectedCarts();
        TeensyRomDevice? GetConnectedDevice(string portName);
        Task<bool> ConnectToNextPort(string deviceId);
        List<string> GetAvailablePorts();
        void ClosePort(string deviceId);
    }
}
