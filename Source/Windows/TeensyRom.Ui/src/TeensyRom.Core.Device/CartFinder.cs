using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Entities.Device;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Commands.FwVersionCheck;
using TeensyRom.Core.Storage;

namespace TeensyRom.Core.Device
{
    public interface ICartFinder
    {
        Task<List<TeensyRomDevice>> FindDevices();
    }

    public class CartFinder(ILoggingService log, ISerialFactory serialFactory, IStorageFactory storageFactory,ICartTagger tagger, IFwVersionChecker versionChecker, IMediator mediator) : ICartFinder
    {
        private const string UndefinedDeviceIdBase = "Unidentified";
        public async Task<List<TeensyRomDevice>> FindDevices()
        {
            string methodName = "CartFiner.Find:";
            var ports = SerialHelper.GetPorts();

            List<TeensyRomDevice> foundDevices = [];

            foreach (var port in ports)
            {
                var serial = serialFactory.Create(port);
                try
                {   
                    serial.OpenPort();                    
                }
                catch (Exception)
                {
                    log.ExternalError($"{methodName} Unable to connect to {port}");
                    continue;
                }
                var versionCheckCommand = new FwVersionCheckCommand
                {
                    Serial = serial
                };
                var versionResult = await mediator.Send(versionCheckCommand);

                if (versionResult.IsSuccess is false) continue;

                if (!versionResult.IsTeensyRom) 
                {
                    serial.Dispose();
                    continue;
                }
                var cart = new Cart
                {
                    ComPort = port,
                    Name = "Unnamed",
                    FwVersion = versionResult.Version?.ToString() ?? "",
                    IsCompatible = versionResult.IsCompatible
                };
                var sdStorage = await tagger.EnsureTag(serial, TeensyStorageType.SD);
                var usbStorage = await tagger.EnsureTag(serial, TeensyStorageType.USB);

                var deviceId = string.IsNullOrWhiteSpace(sdStorage.DeviceId)
                    ? usbStorage.DeviceId
                    : sdStorage.DeviceId;

                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    var unknownCartId = foundDevices
                        .Where(d => d.Cart.DeviceId!.Contains(UndefinedDeviceIdBase))
                        .ToList()
                        .Count();

                    deviceId = $"{UndefinedDeviceIdBase}[{unknownCartId}]";

                    cart.DeviceId = deviceId;
                    sdStorage.DeviceId = deviceId;
                    usbStorage.DeviceId = deviceId;
                    serial.SetDeviceId(deviceId);
                }
                else
                {
                    cart.DeviceId = deviceId;
                    serial.SetDeviceId(deviceId);
                }
                sdStorage.DeviceId = deviceId;
                usbStorage.DeviceId = deviceId;
                cart.SdStorage = sdStorage;
                cart.UsbStorage = usbStorage;
                var device = new TeensyRomDevice
                (
                    cart,
                    serial,
                    storageFactory.Create(sdStorage),
                    storageFactory.Create(usbStorage)
                );
                foundDevices.Add(device);
            }            
            return foundDevices;
        }
    }
}
