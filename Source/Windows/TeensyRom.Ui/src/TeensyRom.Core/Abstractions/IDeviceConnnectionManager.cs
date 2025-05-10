using TeensyRom.Core.Entities.Device;

namespace TeensyRom.Core.Abstractions
{
    public interface IDeviceConnectionManager
    {
        Task<TeensyRomDevice?> Connect(string deviceId);
        Task<List<Cart>> FindAvailableCarts();
        List<Cart> GetConnectedCarts();
        TeensyRomDevice? GetConnectedDevice(string portName);
    }
}
