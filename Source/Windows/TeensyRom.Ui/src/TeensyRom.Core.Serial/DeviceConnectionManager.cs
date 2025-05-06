using System.IO.Ports;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Commands.GetFile;
using TeensyRom.Core.Serial.Commands.SaveFiles;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Serial
{
    public record TeensyRomDevice(Cart Cart, ISerialStateContext SerialState);
    public interface IDeviceConnectionManager 
    {
        Task<TeensyRomDevice?> Connect(string deviceId);
        List<Cart> FindAvailableCarts();
        List<Cart> GetConnectedCarts();
        TeensyRomDevice? GetConnectedDevice(string portName);
    }
    public class DeviceConnectionManager(ICartFinder finder, ICartTagger tagger, ILoggingService log, IAlertService alert) : IDeviceConnectionManager
    {
        private List<TeensyRomDevice> _connectedDevices = [];
        private List<TeensyRomDevice> _knownDevices = [];

        public TeensyRomDevice? GetConnectedDevice(string deviceId)
        {
            return _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);
        }

        public List<Cart> GetConnectedCarts() => _connectedDevices.Select(c => c.Cart).ToList();

        public List<Cart> FindAvailableCarts() 
        {
            _connectedDevices.ForEach(d => d.SerialState.Dispose());
            _knownDevices.ForEach(d => d.SerialState.Dispose());
            _knownDevices.Clear();

            var carts = finder.FindCarts();

            foreach (var cart in carts)
            {
                var port = new SimpleObservableSerialPort(log, alert);
                var cartData = tagger.EnsureCartTag(cart);
                UpdateCart(cartData, port);                
            }
            var devicesToReconnect = _knownDevices
                .Where(d => _connectedDevices
                    .Any(cd => cd.Cart.DeviceId == d.Cart.DeviceId))
                .ToList();

            _connectedDevices.Clear();
            _connectedDevices.AddRange(devicesToReconnect);
            _connectedDevices.ForEach(d => d.SerialState.OpenPort());

            return _knownDevices
                .Select(d => d.Cart)
                .ToList();
        }

        public async Task<TeensyRomDevice?> Connect(string deviceId) 
        {
            var alreadyConnected = _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (alreadyConnected is not null) return alreadyConnected;

            var knownDevice = _knownDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (knownDevice is null) 
            {
                throw new Exception("Unknown deviceId.");
            }
            knownDevice.SerialState.OpenPort();

            var connected = await knownDevice.SerialState.CurrentState.FirstAsync();

            if (connected is SerialConnectedState) 
            {
                _connectedDevices.Add(knownDevice);
                return knownDevice;
            }
            return null;
        }

        private void UpdateCart(Cart cartData, IObservableSerialPort port)
        {
            TeensyRomDevice newDevice = new
            (
                cartData,
                new SerialStateContext(port, log)
            );
            newDevice.SerialState.SetPort(cartData.ComPort);

            var existingDevice = _knownDevices.FirstOrDefault(d => d.Cart.DeviceId == cartData.DeviceId);

            if (existingDevice is not null)
            {
                _knownDevices.Remove(existingDevice);
            }
            _knownDevices.Add(newDevice);
        }
    }
}
