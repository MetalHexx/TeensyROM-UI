using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Serial
{
    public record TeensyRomDevice(KnownCart cart, IObservableSerialPort port);
    public interface IDeviceConnectionManager 
    {
        List<Cart> FindCarts();
        TeensyRomDevice? GetDevice(string portName);
    }
    public class DeviceConnectionManager(ICartFinder cartFinder) : IDeviceConnectionManager
    {
        private List<TeensyRomDevice> _devices = [];

        public TeensyRomDevice? GetDevice(string deviceHash)
        {
            return _devices.FirstOrDefault(d => d.cart.DeviceHash == deviceHash);
        }

        public List<Cart> FindCarts() => cartFinder.FindCarts();
    }
}
