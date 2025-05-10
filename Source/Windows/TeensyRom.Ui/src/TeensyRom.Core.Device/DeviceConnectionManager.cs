using System.Reactive.Linq;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Device
{
    public class DeviceConnectionManager(ICartFinder finder, ICartTagger tagger, ILoggingService log, IAlertService alert, ISerialFactory serialFactory, IStorageFactory storageFactory) : IDeviceConnectionManager
    {
        private List<TeensyRomDevice> _connectedDevices = [];
        private List<TeensyRomDevice> _availableDevices = [];
        private const string UndefinedDeviceIdBase = "Unidentified";

        public TeensyRomDevice? GetConnectedDevice(string deviceId)
        {
            return _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);
        }

        public List<Cart> GetConnectedCarts() => _connectedDevices.Select(c => c.Cart).ToList();

        public async Task<List<Cart>> FindAvailableCarts()
        {
            _connectedDevices.ForEach(d => d.SerialState.Dispose());
            _availableDevices.ForEach(d => d.SerialState.Dispose());
            _availableDevices.Clear();

            var carts = finder.FindCarts();

            foreach (var cart in carts)
            {
                Cart cartResult = await tagger.EnsureCartTag(cart);
                UpdateCart(cartResult!);
            }
            var devicesToReconnect = _availableDevices
                .Where(d => _connectedDevices.Any(cd => cd.Cart.DeviceId == d.Cart.DeviceId))
                .ToList();

            _connectedDevices.Clear();
            _connectedDevices.AddRange(devicesToReconnect);
            _connectedDevices.ForEach(d => d.SerialState.OpenPort());

            return _availableDevices.Select(d => d.Cart).ToList();
        }

        public async Task<TeensyRomDevice?> Connect(string deviceId)
        {
            var alreadyConnected = _connectedDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (alreadyConnected is not null) return alreadyConnected;

            var knownDevice = _availableDevices.FirstOrDefault(d => d.Cart.DeviceId == deviceId);

            if (knownDevice is null)
            {
                return null;
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

        private void UpdateCart(Cart cartData)
        {
            if (cartData.DeviceId is null)
            {
                var unknownCartId = _availableDevices
                    .Where(d => d.Cart.DeviceId!.Contains(UndefinedDeviceIdBase))
                    .ToList()
                    .Count();

                cartData.DeviceId = $"{UndefinedDeviceIdBase}[{unknownCartId}]";
                cartData.SdStorage.DeviceId = cartData.DeviceId;
                cartData.UsbStorage.DeviceId = cartData.DeviceId;
            }
            TeensyRomDevice newDevice = new
            (
                cartData,
                serialFactory.Create(cartData.ComPort),
                storageFactory.Create(cartData.SdStorage),
                storageFactory.Create(cartData.UsbStorage)
            );
            newDevice.SerialState.SetPort(cartData.ComPort);

            var existingDevice = _availableDevices.FirstOrDefault(d => d.Cart.DeviceId == cartData.DeviceId);

            if (existingDevice is not null)
            {
                _availableDevices.Remove(existingDevice);
            }
            _availableDevices.Add(newDevice);
        }
    }
}
